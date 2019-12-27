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
using PdfSharp.Pdf;
using PdfSharp.Pdf.Content.Objects;
using ShapeConverter.BusinessLogic.Parser.Pdf.Function;

namespace ShapeConverter.Parser.Pdf
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
            return 3;
        }

        /// <summary>
        /// GetColor
        /// </summary>
        public Color GetColor(List<double> values, double alpha)
        {
            var red = (byte)(int)Math.Round(values[0] * 255.0);
            var green = (byte)(int)Math.Round(values[1] * 255.0);
            var blue = (byte)(int)Math.Round(values[2] * 255.0);

            return Color.FromArgb((byte)(alpha * 255), red, green, blue);
        }

        /// <summary>
        /// Get a brush descriptor
        /// </summary>
        public IBrushDescriptor GetBrushDescriptor(CSequence operands, Matrix matrix, double alpha)
        {
            var values = PdfUtilities.CreateDoubleArray(operands);
            return new RGBDeviceBrushDescriptor(GetColor(values, alpha));
        }
    }

    /// <summary>
    /// The RGB brush creator
    /// </summary>
    internal class RGBDeviceBrushDescriptor : IBrushDescriptor
    {
        private Color color;

        /// <summary>
        /// The color precision
        /// </summary>
        public GraphicColorPrecision ColorPrecision => GraphicColorPrecision.Precise;

        /// <summary>
        /// Constructor
        /// </summary>
        public RGBDeviceBrushDescriptor(Color color)
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
