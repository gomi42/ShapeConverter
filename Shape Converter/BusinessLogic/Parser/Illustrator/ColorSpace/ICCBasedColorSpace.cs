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
using System.IO;
using System.Windows.Media;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Content.Objects;
using ShapeConverter.BusinessLogic.Parser.Pdf.Function;
using ShapeConverter.Helper;

namespace ShapeConverter.Parser.Pdf
{
    /// <summary>
    /// ColorSpaceICCBased
    /// </summary>
    internal class ICCBasedColorSpace : IColorSpace
    {
        string filename;
        int numberColorComponents;

        /// <summary>
        /// GetUri
        /// </summary>
        /// <returns>the uri of the profile's filename</returns>
        private Uri GetProfileUri()
        {
            var filename = this.filename.Replace('\\', '/');
            return new Uri(filename);
        }

        /// <summary>
        /// The color precision
        /// </summary>
        public GraphicColorPrecision ColorPrecision => GraphicColorPrecision.Precise;

        /// <summary>
        /// Init
        /// </summary>
        public void Init(PdfArray colorSpaceArray)
        {
            var iccDict = colorSpaceArray.Elements.GetDictionary(1);
            numberColorComponents = iccDict.Elements.GetInteger(PdfKeys.N);
            filename = CommonHelper.GetTempProfileFilename();

            using (var stream = File.Open(filename, FileMode.Create))
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(iccDict.Stream.UnfilteredValue);
                }
            }
        }

        /// <summary>
        /// Get the number of values (components) that build up a color
        /// </summary>
        /// <returns></returns>
        public int GetNumberOfValuesPerColor()
        {
            return numberColorComponents;
        }

        /// <summary>
        /// Gets a color based on the specified values and opacity
        /// </summary>
        public Color GetColor(List<double> values, double alpha)
        {
            float[] colorValues = new float[numberColorComponents];

            for (int i = 0; i < numberColorComponents; i++)
            {
                colorValues[i] = (float)values[i];
            }

            Color color = Color.FromValues(colorValues, GetProfileUri());

            return Color.FromArgb((byte)(alpha * 255), color.R, color.G, color.B);
        }

        /// <summary>
        /// Get a brush descriptor
        /// </summary>
        public IBrushDescriptor GetBrushDescriptor(CSequence operands, Matrix matrix, double alpha)
        {
            float[] colorValues = new float[numberColorComponents];

            for (int i = 0; i < numberColorComponents; i++)
            {
                colorValues[i] = (float)PdfUtilities.GetDouble(operands[i]);
            }

            Color color = Color.FromAValues((float)alpha, colorValues, GetProfileUri());

            return new ICCBasedBrushDescriptor(color);
        }
    }

    /// <summary>
    /// The RGB brush creator
    /// </summary>
    internal class ICCBasedBrushDescriptor : IBrushDescriptor
    {
        private Color color;

        /// <summary>
        /// The color precision
        /// </summary>
        public GraphicColorPrecision ColorPrecision => GraphicColorPrecision.Precise;

        /// <summary>
        /// Constructor
        /// </summary>
        public ICCBasedBrushDescriptor(Color color)
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
