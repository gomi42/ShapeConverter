//
// Author:
//   Michael Göricke
//
// Copyright (c) 2019
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
    /// ColorSpaceDeviceRGB
    /// </summary>
    internal class RGBDeviceColorSpace : IColorSpace
    {
        /// <summary>
        /// The color precision
        /// </summary>
        public GraphicColorPrecision ColorPrecision => GraphicColorPrecision.Precise;

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
            return OperandHelper.CreateColorSpaceDescription(EpsKeys.DeviceRGB);
        }

        /// <summary>
        /// GetColor
        /// </summary>
        public Color GetColor(List<double> values)
        {
            return GetColorX(values);
        }

        /// <summary>
        /// GetColor as static method
        /// </summary>
        public static Color GetColorX(List<double> values)
        {
            var red = (byte)(int)Math.Round(values[0] * 255.0);
            var green = (byte)(int)Math.Round(values[1] * 255.0);
            var blue = (byte)(int)Math.Round(values[2] * 255.0);

            return Color.FromArgb((byte)255, red, green, blue);
        }

        /// <summary>
        /// Get the number of values (components) that build up a color
        /// </summary>
        public int GetNumberOfValuesPerColor()
        {
            return 3;
        }

        /// <summary>
        /// Get a brush descriptor
        /// </summary>
        public IBrushDescriptor GetBrushDescriptor(List<Operand> operands, Matrix matrix)
        {
            return new RGBDeviceBrushDescriptor(operands);
        }
    }

    /// <summary>
    /// The RGB brush descriptor
    /// </summary>
    internal class RGBDeviceBrushDescriptor : IBrushDescriptor
    {
        private List<double> values;

        /// <summary>
        /// The color precision
        /// </summary>
        public GraphicColorPrecision ColorPrecision => GraphicColorPrecision.Precise;

        /// <summary>
        /// Constructor
        /// </summary>
        public RGBDeviceBrushDescriptor(List<Operand> operands)
        {
            values = new List<double>();

            values.Add(OperandHelper.GetRealValue(operands[0]));
            values.Add(OperandHelper.GetRealValue(operands[1]));
            values.Add(OperandHelper.GetRealValue(operands[2]));
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

            double gray = 0.3 * values[0] + 0.59 * values[1] + 0.11 * values[2];
            var realOp = new RealOperand(gray);
            operands.Add(realOp);

            return operands;
        }

        /// <summary>
        /// Return the color as RGB components operands that define this brush
        /// </summary>
        public List<Operand> GetRGBColorOperands()
        {
            return GetColorOperands();
        }

        /// <summary>
        /// Return the color as CMYK components operands that define this brush
        /// Use formula according spec
        /// </summary>
        public List<Operand> GetCMYKColorOperands()
        {
            List<Operand> operands = new List<Operand>();

            var c = 1.0 - values[0];
            var m = 1.0 - values[1];
            var y = 1.0 - values[2];
            var k = Math.Min(Math.Min(c, m), y);

            var realOp = new RealOperand(c);
            operands.Add(realOp);
            realOp = new RealOperand(m);
            operands.Add(realOp);
            realOp = new RealOperand(y);
            operands.Add(realOp);
            realOp = new RealOperand(k);
            operands.Add(realOp);

            return operands;
        }

        /// <summary>
        /// Get a graphic brush
        /// </summary>
        public GraphicBrush GetBrush(EpsRect boundingBox)
        {
            return new GraphicSolidColorBrush { Color = RGBDeviceColorSpace.GetColorX(values) };
        }
    }
}
