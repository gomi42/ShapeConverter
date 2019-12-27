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
using ShapeConverter;

namespace EpsSharp.Eps.Core
{
    /// <summary>
    /// The unknown color space
    /// </summary>
    internal class UnknownColorSpace : IColorSpace
    {
        private ArrayOperand colorSpaceDetails;
        private int numberOfColors;

        /// <summary>
        /// Constructor
        /// </summary>
        public UnknownColorSpace(int numberOfColors)
        {
            this.numberOfColors = numberOfColors;
        }

        /// <summary>
        /// The color precision
        /// </summary>
        public GraphicColorPrecision ColorPrecision => GraphicColorPrecision.Placeholder;

        /// <summary>
        /// Init
        /// </summary>
        public void Init(EpsInterpreter interpreter, ArrayOperand colorSpaceDetails)
        {
            this.colorSpaceDetails = colorSpaceDetails;
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
            byte color = 0x90;
            return Color.FromArgb((byte)255, color, color, color);
        }

        /// <summary>
        /// Get the number of values (components) that build up a color
        /// </summary>
        public int GetNumberOfValuesPerColor()
        {
            return numberOfColors;
        }

        /// <summary>
        /// Get a brush descriptor
        /// </summary>
        public IBrushDescriptor GetBrushDescriptor(List<Operand> operands, Matrix matrix)
        {
            return new UnknownBrushDescriptor(GetColor(null), operands);
        }
    }

    /// <summary>
    /// The RGB brush descriptor
    /// </summary>
    internal class UnknownBrushDescriptor : IBrushDescriptor
    {
        private Color color;
        private List<Operand> operands;

        /// <summary>
        /// The color precision
        /// </summary>
        public GraphicColorPrecision ColorPrecision => GraphicColorPrecision.Precise;

        /// <summary>
        /// Constructor
        /// </summary>
        public UnknownBrushDescriptor(Color color, List<Operand> operands)
        {
            this.color = color;
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
        /// Use formula according spec
        /// </summary>
        public List<Operand> GetGrayColorOperands()
        {
            List<Operand> operands = new List<Operand>();

            var realOp = new RealOperand(0.6);
            operands.Add(realOp);

            return operands;
        }

        /// <summary>
        /// Return the color as RGB components operands that define this brush
        /// </summary>
        public List<Operand> GetRGBColorOperands()
        {
            List<Operand> operands = new List<Operand>();

            var realOp = new RealOperand(0.6);
            operands.Add(realOp);
            realOp = new RealOperand(0.6);
            operands.Add(realOp);
            realOp = new RealOperand(0.6);
            operands.Add(realOp);

            return operands;
        }

        /// <summary>
        /// Return the color as CMYK components operands that define this brush
        /// Use formula according spec
        /// </summary>
        public List<Operand> GetCMYKColorOperands()
        {
            List<Operand> operands = new List<Operand>();

            var realOp = new RealOperand(0.4);
            operands.Add(realOp);
            realOp = new RealOperand(0.3);
            operands.Add(realOp);
            realOp = new RealOperand(0.3);
            operands.Add(realOp);
            realOp = new RealOperand(0.1);
            operands.Add(realOp);

            return operands;
        }

        /// <summary>
        /// Get a graphic brush
        /// </summary>
        public GraphicBrush GetBrush(EpsRect boundingBox)
        {
            return new GraphicSolidColorBrush { Color = color };
        }
    }
}
