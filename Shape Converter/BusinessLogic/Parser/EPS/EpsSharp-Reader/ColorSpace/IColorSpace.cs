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

using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using ShapeConverter;

namespace EpsSharp.Eps.Core
{
    /// <summary>
    /// A rectangle definition that handles negative values
    /// as well as mirrored axis (left > right)
    /// </summary>
    internal class EpsRect
    {
        public double Top { get; set; }

        public double Left { get; set; }

        public double Bottom { get; set; }

        public double Right { get; set; }

        public static implicit operator EpsRect(Rect from)
        {
            var to = new EpsRect();

            to.Left = from.Left;
            to.Right = from.Right;
            to.Top = from.Top;
            to.Bottom = from.Bottom;

            return to;
        }
    }

    /// <summary>
    /// The color space descriptor
    /// </summary>
    internal interface IColorSpace
    {
        /// <summary>
        /// The color precision of the color
        /// </summary>
        GraphicColorPrecision ColorPrecision { get; }

        /// <summary>
        /// Init
        /// </summary>
        void Init(EpsInterpreter interpreter, ArrayOperand colorSpaceArray);

        /// <summary>
        /// Gets the name of the color space
        /// </summary>
        /// <returns></returns>
        ArrayOperand GetColorSpaceName();

        /// <summary>
        /// GetColor
        /// </summary>
        Color GetColor(List<double> values);

        /// <summary>
        /// Get the number of values (components) that build up a color
        /// E.g. for RGB its 3, CMYK its 4, Gray its 1
        /// </summary>
        /// <returns></returns>
        int GetNumberOfValuesPerColor();

        /// <summary>
        /// Get a brush descriptor
        /// </summary>
        IBrushDescriptor GetBrushDescriptor(List<Operand> operants, Matrix matrix);
    }

    /// <summary>
    /// The RGB brush creator
    /// </summary>
    internal interface IBrushDescriptor
    {
        /// <summary>
        /// The color precision of the brush
        /// </summary>
        GraphicColorPrecision ColorPrecision { get; }

        /// <summary>
        /// Return the color components as operands that define this brush
        /// </summary>
        List<Operand> GetColorOperands();

        /// <summary>
        /// Return the color as gray components operands that define this brush
        /// </summary>
        List<Operand> GetGrayColorOperands();

        /// <summary>
        /// Return the color as RGB components operands that define this brush
        /// </summary>
        List<Operand> GetRGBColorOperands();

        /// <summary>
        /// Return the color as CMYK components operands that define this brush
        /// </summary>
        List<Operand> GetCMYKColorOperands();

        /// <summary>
        /// Get a graphic brush if needed with coordinates relative to 
        /// the given rectangle
        /// </summary>
        GraphicBrush GetBrush(EpsRect rect);
    }
}
