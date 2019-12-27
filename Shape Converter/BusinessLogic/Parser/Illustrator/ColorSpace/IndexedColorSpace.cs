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
using ShapeConverter.Parser.Pdf;

namespace ShapeConverter.BusinessLogic.Parser.Pdf.ColorSpace.Indexed
{
    /// <summary>
    /// The indexed color space
    /// </summary>
    internal class IndexedColorSpace : IColorSpace
    {
        private IColorSpace baseColorSpace;
        private int hival;
        private List<double> lookup;

        /// <summary>
        /// The color precision
        /// </summary>
        public GraphicColorPrecision ColorPrecision => baseColorSpace.ColorPrecision;

        /// <summary>
        /// Init
        /// </summary>
        public void Init(PdfArray colorSpaceArray)
        {
            var baseArray = colorSpaceArray.Elements.GetArray(1);

            if (baseArray != null)
            {
                baseColorSpace = ColorSpaceManager.CreateColorSpace(baseArray);
            }
            else
            {
                string baseColorSpaceName = colorSpaceArray.Elements.GetName(1);
                baseColorSpace = ColorSpaceManager.CreateColorSpace(baseColorSpaceName);
            }

            hival = colorSpaceArray.Elements.GetInteger(2);

            if (colorSpaceArray.Elements[3] is PdfString hexString)
            {
                lookup = new List<double>();

                for (int i = 0; i < hexString.Value.Length; i++)
                {
                    lookup.Add(hexString.Value[i] / 255.0);
                }
            }

        }

        /// <summary>
        /// Get the number of values (components) that build up a color
        /// </summary>
        /// <returns></returns>
        public int GetNumberOfValuesPerColor()
        {
            return 1;
        }

        /// <summary>
        /// Gets a color based on the specified values and opacity
        /// </summary>
        public Color GetColor(List<double> values, double alpha)
        {
            var colorValues = new List<double>();
            var numberColorValues = baseColorSpace.GetNumberOfValuesPerColor();
            int index = LimitIndexToBoundaries((int)values[0]) * numberColorValues;

            for (int i = 0; i < numberColorValues; i++)
            {
                colorValues.Add(lookup[index + i]);
            }

            return baseColorSpace.GetColor(colorValues, alpha);
        }

        /// <summary>
        /// Get a brush descriptor
        /// </summary>
        public IBrushDescriptor GetBrushDescriptor(CSequence operands, Matrix matrix, double alpha)
        {
            CSequence newSequence = new CSequence();
            var numberColorValues = baseColorSpace.GetNumberOfValuesPerColor();
            int index = LimitIndexToBoundaries(PdfUtilities.GetInteger(operands[0])) * numberColorValues;

            for (int i = 0; i < numberColorValues; i++)
            {
                var real = new CReal();
                real.Value = lookup[index + i];
                newSequence.Add(real);
            }

            var brushDescriptor = baseColorSpace.GetBrushDescriptor(newSequence, matrix, alpha);
            return new IndexedBrushDescriptor(brushDescriptor);
        }

        /// <summary>
        /// Check the allowed limits for the given index
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
    /// The indexed brush creator
    /// </summary>
    internal class IndexedBrushDescriptor : IBrushDescriptor
    {
        private IBrushDescriptor brushDescriptor;

        /// <summary>
        /// The color precision
        /// </summary>
        public GraphicColorPrecision ColorPrecision => brushDescriptor.ColorPrecision;

        /// <summary>
        /// Constructor
        /// </summary>
        public IndexedBrushDescriptor(IBrushDescriptor brushDescriptor)
        {
            this.brushDescriptor = brushDescriptor;
        }

        /// <summary>
        /// Get a graphic brush
        /// </summary>
        public GraphicBrush GetBrush(PdfRect boundingBox, List<FunctionStop> softMask)
        {
            return brushDescriptor.GetBrush(boundingBox, softMask);
        }
    }
}
