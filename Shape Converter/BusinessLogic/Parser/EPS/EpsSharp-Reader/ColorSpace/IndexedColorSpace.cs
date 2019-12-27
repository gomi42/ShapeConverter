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
using EpsSharp.Eps.Helper;
using ShapeConverter;

namespace EpsSharp.Eps.Core
{
    /// <summary>
    /// The Indexed color space
    /// </summary>
    internal class IndexedColorSpace : IColorSpace
    {
        private EpsInterpreter interpreter;
        private ArrayOperand colorSpaceDetails;
        private IColorSpace baseColorSpace;
        private int hival;
        private bool useProcedure;
        private string hexString;
        private ArrayOperand tintTransformationProcedure;

        /// <summary>
        /// The color precision
        /// </summary>
        public GraphicColorPrecision ColorPrecision => baseColorSpace.ColorPrecision;

        /// <summary>
        /// Init
        /// </summary>
        public void Init(EpsInterpreter interpreter, ArrayOperand colorSpaceDetails)
        {
            this.interpreter = interpreter;
            this.colorSpaceDetails = colorSpaceDetails;

            var colorSpaceOp = colorSpaceDetails.Values[1].Operand;
            baseColorSpace = ColorSpaceActivator.CreateColorSpace(interpreter, colorSpaceOp);
            hival = ((IntegerOperand)colorSpaceDetails.Values[2].Operand).Value;

            switch (colorSpaceDetails.Values[3].Operand)
            {
                case StringOperand stringOperand:
                    useProcedure = false;
                    hexString = stringOperand.Value;
                    break;

                case ArrayOperand arrayOperand:
                    useProcedure = true;
                    tintTransformationProcedure = arrayOperand;
                    break;
            }
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
            var colorValues = new List<double>();
            var index = LimitIndexToBoundaries((int)values[0]);
            var numberColorValues = baseColorSpace.GetNumberOfValuesPerColor();

            if (!useProcedure)
            {
                index *= numberColorValues;

                for (int i = 0; i < numberColorValues; i++)
                {
                    colorValues.Add(hexString[index + i] / 255.0);
                }
            }
            else
            {
                var operandStack = interpreter.OperandStack;
                
                var intOp = new IntegerOperand(index);
                operandStack.Push(intOp);

                interpreter.Execute(tintTransformationProcedure);

                for (int i = 0; i < numberColorValues; i++)
                {
                    var real = operandStack.PopRealValue();
                    colorValues.Add(real);
                }

                colorValues.Reverse();
            }

            return baseColorSpace.GetColor(colorValues);
        }

        /// <summary>
        /// Get the number of values (components) that build up a color
        /// </summary>
        public int GetNumberOfValuesPerColor()
        {
            return 1;
        }

        /// <summary>
        /// Get a brush descriptor
        /// </summary>
        public IBrushDescriptor GetBrushDescriptor(List<Operand> operands, Matrix matrix)
        {
            List<Operand> colorValues;
            var numberColorValues = baseColorSpace.GetNumberOfValuesPerColor();
            var index = LimitIndexToBoundaries(((IntegerOperand)operands[0]).Value);

            if (!useProcedure)
            {
                colorValues = new List<Operand>();
                index *= numberColorValues;

                for (int i = 0; i < numberColorValues; i++)
                {
                    colorValues.Add(new RealOperand(hexString[index + i] / 255.0));
                }
            }
            else
            {
                var operandStack = interpreter.OperandStack;

                operandStack.Push(new IntegerOperand(index));
                interpreter.Execute(tintTransformationProcedure);
                colorValues = operandStack.PopToList(numberColorValues);
            }

            var brushDescriptor = baseColorSpace.GetBrushDescriptor(colorValues, matrix);
            return new IndexedBrushDescriptor(brushDescriptor, operands);
        }

        /// <summary>
        /// Check the allowed limits for the given index
        /// The spec doesn't define the behavior if index is out-of-range,
        /// we take the PDF definition
        /// </summary>
        private int LimitIndexToBoundaries(int index)
        {
            if (index < 0)
            {
                index = 0;
            }
            else
            if (index > hival)
            {
                index = hival;
            }

            return index;
        }
    }

    /// <summary>
    /// The DeviceN brush Descriptor
    /// </summary>
    internal class IndexedBrushDescriptor : IBrushDescriptor
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
        public IndexedBrushDescriptor(IBrushDescriptor brushDescriptor, List<Operand> operands)
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
