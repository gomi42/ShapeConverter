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
using PdfSharp.Pdf;
using PdfSharp.Pdf.Content.Objects;
using ShapeConverter.BusinessLogic.Parser.Pdf.Function;

namespace ShapeConverter.Parser.Pdf
{
    /// <summary>
    /// A rectangle definition that handles negative values
    /// as well as mirrored axis (left > right)
    /// </summary>
    internal class PdfRect
    {
        public double Top { get; set; }

        public double Left { get; set; }

        public double Bottom { get; set; }

        public double Right { get; set; }

        public static implicit operator PdfRect(Rect from)
        {
            var to = new PdfRect();

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
        /// init
        /// </summary>
        void Init(PdfArray colorSpaceArray);

        /// <summary>
        /// Get the number of values (components) that build up a color
        /// E.g. for RGB its 3, CMYK its 4, Gray its 1
        /// </summary>
        /// <returns></returns>
        int GetNumberOfValuesPerColor();

        /// <summary>
        /// GetColor
        /// </summary>
        Color GetColor(List<double> values, double alpha);

        /// <summary>
        /// Get a brush descriptor
        /// </summary>
        IBrushDescriptor GetBrushDescriptor(CSequence operands, Matrix matrix, double alpha);
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
        /// Get a graphic brush if needed with coordinates relative to 
        /// the given rectangle
        /// </summary>
        GraphicBrush GetBrush(PdfRect rect, List<FunctionStop> softMask);
    }
}
