//
// Author:
//   Michael Göricke
//
// Copyright (c) 2019
//
// This file is part of ShapeConverter.
//
// ShapeConverter is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see<http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Windows.Media;
using EpsSharp.Eps.Core;
using EpsSharp.Eps.Helper;
using ShapeConverter;

namespace EpsSharp.Eps.Core
{
    /// <summary>
    /// The DeviceN and Separation color space
    /// Separation is a real subset of DeviceN that's
    /// why we can handle both in one implementation
    /// </summary>
    internal class DeviceNColorSpace : IColorSpace
    {
        private EpsInterpreter interpreter;
        private ArrayOperand colorSpaceDetails;
        private ArrayOperand tintTransformationProcedure;
        private IColorSpace alternativeColorSpace;
        private int numberOfValuesPerColor;

        /// <summary>
        /// The color precision
        /// </summary>
        public GraphicColorPrecision ColorPrecision => alternativeColorSpace.ColorPrecision;

        /// <summary>
        /// Init
        /// </summary>
        public void Init(EpsInterpreter interpreter, ArrayOperand colorSpaceDetails)
        {
            this.interpreter = interpreter;
            this.colorSpaceDetails = colorSpaceDetails;

            switch (colorSpaceDetails.Values[1].Operand)
            {
                case StringOperand stringOperand:
                case NameOperand nameOperand:
                    numberOfValuesPerColor = 1;
                    break;

                case ArrayOperand namesArray:
                    numberOfValuesPerColor = namesArray.Values.Count;
                    break;

                default:
                    throw new Exception("illegal type");
            }

            // we only operate on the alternative color space
            var colorSpaceOp = colorSpaceDetails.Values[2].Operand;
            alternativeColorSpace = ColorSpaceActivator.CreateColorSpace(interpreter, colorSpaceOp);

            tintTransformationProcedure = (ArrayOperand)colorSpaceDetails.Values[3].Operand;
        }

        /// <summary>
        /// Gets the name of the color space
        /// </summary>
        public ArrayOperand GetColorSpaceName()
        {
            return colorSpaceDetails;
        }

        /// <summary>
        /// GetColor
        /// </summary>
        public Color GetColor(List<double> values)
        {
            var newValues = ConvertValuesToAlternativeColorSpaceValues(values);
            return alternativeColorSpace.GetColor(newValues);
        }

        /// <summary>
        /// Get the number of values (components) that build up a color
        /// </summary>
        public int GetNumberOfValuesPerColor()
        {
            return numberOfValuesPerColor;
        }

        /// <summary>
        /// Get a brush descriptor
        /// </summary>
        public IBrushDescriptor GetBrushDescriptor(List<Operand> operands, Matrix matrix)
        {
            var newOperands = ConvertOperandsToAlternativeColorSpaceOperands(operands);
            var brushDescriptor = alternativeColorSpace.GetBrushDescriptor(newOperands, matrix);
            return new DeviceNBrushDescriptor(brushDescriptor, operands);
        }

        /// <summary>
        /// Convert the given values to the destination color space values
        /// </summary>
        private List<double> ConvertValuesToAlternativeColorSpaceValues(List<double> values)
        {
            var operandStack = interpreter.OperandStack;
            
            foreach (var val in values)
            {
                var realOp = new RealOperand(val);
                operandStack.Push(realOp);
            }

            interpreter.Execute(tintTransformationProcedure);

            var newValues = new List<double>();

            for (int i = 0; i < alternativeColorSpace.GetNumberOfValuesPerColor(); i++)
            {
                var real = operandStack.PopRealValue();
                newValues.Add(real);
            }

            newValues.Reverse();

            return values;
        }

        /// <summary>
        /// Convert the given operands to the destination color space operands
        /// </summary>
        private List<Operand> ConvertOperandsToAlternativeColorSpaceOperands(List<Operand> operands)
        {
            var operandStack = interpreter.OperandStack;

            operandStack.PushFromList(operands);
            interpreter.Execute(tintTransformationProcedure);
            var newOperands = operandStack.PopToList(alternativeColorSpace.GetNumberOfValuesPerColor());

            return newOperands;
        }
    }

    /// <summary>
    /// The DeviceN brush Descriptor
    /// </summary>
    internal class DeviceNBrushDescriptor : IBrushDescriptor
    {
        private IBrushDescriptor brushDescriptor;
        private List<Operand> operands;

        /// <summary>
        /// The color precision
        /// </summary>
        public GraphicColorPrecision ColorPrecision => brushDescriptor.ColorPrecision;

        /// <summary>
        /// Constructor
        /// </summary>
        public DeviceNBrushDescriptor(IBrushDescriptor brushDescriptor, List<Operand> operands)
        {
            this.brushDescriptor = brushDescriptor;
            this.operands = operands;
        }

        /// <summary>
        /// Return the color components as operands that define this brush
        /// </summary>
        public List<Operand> GetColorOperands()
        {
            return operands;
        }

        /// <summary>
        /// Return the color as gray components operands that define this brush
        /// </summary>
        public List<Operand> GetGrayColorOperands()
        {
            return brushDescriptor.GetGrayColorOperands();
        }

        /// <summary>
        /// Return the color as RGB components as operands that define this brush
        /// </summary>
        public List<Operand> GetRGBColorOperands()
        {
            return brushDescriptor.GetRGBColorOperands();
        }

        /// <summary>
        /// Return the color as CMYK components operands that define this brush
        /// </summary>
        public List<Operand> GetCMYKColorOperands()
        {
            return brushDescriptor.GetCMYKColorOperands();
        }

        /// <summary>
        /// Get a graphic brush
        /// </summary>
        public GraphicBrush GetBrush(EpsRect boundingBox)
        {
            return brushDescriptor.GetBrush(boundingBox);
        }
    }
}
