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
using System.Windows.Media;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Content.Objects;
using ShapeConverter.BusinessLogic.Parser.Pdf.Function;

namespace ShapeConverter.Parser.Pdf
{
    internal class UnknownColorSpace : IColorSpace
    {
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
        /// init
        /// </summary>
        public void Init(PdfArray colorSpaceArray)
        {
        }

        /// <summary>
        /// Get the number of values (components) that build up a color
        /// </summary>
        /// <returns></returns>
        public int GetNumberOfValuesPerColor()
        {
            return numberOfColors;
        }

        /// <summary>
        /// GetColor
        /// </summary>
        public Color GetColor(List<double> values, double alpha)
        {
            byte color = 0x90;
            return Color.FromArgb((byte)(alpha * 255), color, color, color);
        }

        /// <summary>
        /// Get a brush descriptor
        /// </summary>
        public IBrushDescriptor GetBrushDescriptor(CSequence operands, Matrix matrix, double alpha)
        {
            return new UnknownBrushDescriptor(GetColor(null, alpha));
        }
    }

    /// <summary>
    /// The RGB brush creator
    /// </summary>
    internal class UnknownBrushDescriptor : IBrushDescriptor
    {
        private Color color;

        /// <summary>
        /// The color precision
        /// </summary>
        public GraphicColorPrecision ColorPrecision => GraphicColorPrecision.Placeholder;

        /// <summary>
        /// Constructor
        /// </summary>
        public UnknownBrushDescriptor(Color color)
        {
            this.color = color;
        }

        /// <summary>
        /// Get a graphic brush
        /// </summary>
        public GraphicBrush GetBrush(PdfRect boundingBox, List<FunctionStop> softMask)
        {
            return new GraphicSolidColorBrush { Color = color };
        }
    }
}
