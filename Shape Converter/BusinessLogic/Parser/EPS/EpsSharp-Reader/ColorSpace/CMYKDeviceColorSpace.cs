//
// Author:
//   Michael Göricke
//
// Copyright (c) 2019-2020
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
using ShapeConverter.BusinessLogic.ShapeConverter;

namespace EpsSharp.Eps.Core
{
    /// <summary>
    /// ColorSpaceDeviceCMYK
    /// </summary>
    internal class CMYKDeviceColorSpace : IColorSpace
    {
        /// <summary>
        /// The color precision
        /// </summary>
        public GraphicColorPrecision ColorPrecision => GraphicColorPrecision.Estimated;

        /// <summary>
        /// Init
        /// </summary>
        public void Init(EpsInterpreter interpreter, ArrayOperand colorSpaceArray)
        {
        }

        /// <summary>
        /// Gets the name of the color space
        /// </summary>
        public ArrayOperand GetColorSpaceName()
        {
            return OperandHelper.CreateColorSpaceDescription(EpsKeys.DeviceCMYK);
        }

        /// <summary>
        /// GetColor
        /// </summary>
        public Color GetColor(List<double> values)
        {
            return ColorSpaceConverter.ConvertCmykToRgb(values[0], values[1], values[2], values[3]);
        }

        /// <summary>
        /// Get the number of values (components) that build up a color
        /// </summary>
        /// <returns></returns>
        public int GetNumberOfValuesPerColor()
        {
            return 4;
        }

        /// <summary>
        /// Get a brush descriptor
        /// </summary>
        public IBrushDescriptor GetBrushDescriptor(List<Operand> operands, Matrix matrix)
        {
            return new CMYKDeviceBrushDescriptor(operands);
        }
    }

    /// <summary>
    /// The RGB brush creator
    /// </summary>
    internal class CMYKDeviceBrushDescriptor : IBrushDescriptor
    {
        private List<double> values;

        /// <summary>
        /// The color precision
        /// </summary>
        public GraphicColorPrecision ColorPrecision => GraphicColorPrecision.Estimated;

        /// <summary>
        /// Constructor
        /// </summary>
        public CMYKDeviceBrushDescriptor(List<Operand> operands)
        {
            values = new List<double>();

            values.Add(OperandHelper.GetRealValue(operands[0]));
            values.Add(OperandHelper.GetRealValue(operands[1]));
            values.Add(OperandHelper.GetRealValue(operands[2]));
            values.Add(OperandHelper.GetRealValue(operands[3]));
        }

        /// <summary>
        /// Return the color components as operands that define this brush
        /// </summary>
        public List<Operand> GetColorOperands()
        {
            List<Operand> operands = new List<Operand>();

            foreach (var val in values)
            {
                var realOp = new RealOperand(val);
                operands.Add(realOp);
            }

            return operands;
        }

        /// <summary>
        /// Return the color as gray components operands that define this brush
        /// Use formula according spec
        /// </summary>
        public List<Operand> GetGrayColorOperands()
        {
            List<Operand> operands = new List<Operand>();

            double gray = 1 - Math.Min(1.0, 0.3 * values[0] + 0.59 * values[1] + 0.11 * values[2] + values[3]);
            var realOp = new RealOperand(gray);
            operands.Add(realOp);

            return operands;
        }

        /// <summary>
        /// Return the color as RGB components operands that define this brush
        /// Here we don't use the formula from the spec, we use our formula that
        /// is needed to get a correct RGB value
        /// </summary>
        public List<Operand> GetRGBColorOperands()
        {
            var color = ColorSpaceConverter.ConvertCmykToRgb(values[0], values[1], values[2], values[3]);
            List<Operand> operands = new List<Operand>();

            var realOp = new RealOperand(color.R);
            operands.Add(realOp);
            realOp = new RealOperand(color.G);
            operands.Add(realOp);
            realOp = new RealOperand(color.B);
            operands.Add(realOp);

            return operands;
        }

        /// <summary>
        /// Return the color as CMYK components operands that define this brush
        /// </summary>
        public List<Operand> GetCMYKColorOperands()
        {
            return GetColorOperands();
        }

        /// <summary>
        /// Get a graphic brush
        /// </summary>
        public GraphicBrush GetBrush(EpsRect boundingBox)
        {
            var color = ColorSpaceConverter.ConvertCmykToRgb(values[0], values[1], values[2], values[3]);
            return new GraphicSolidColorBrush { Color = color };
        }
    }
}
