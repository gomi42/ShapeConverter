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
    /// <summary>
    /// The DeviceN color space.
    /// </summary>
    internal class DeviceNColorSpace : IColorSpace
    {
        private int numberInputValues;
        private IColorSpace alternateColorSpace;
        private IFunction function;

        /// <summary>
        /// The color precision
        /// </summary>
        public GraphicColorPrecision ColorPrecision => alternateColorSpace.ColorPrecision;

        /// <summary>
        /// init
        /// </summary>
        public void Init(PdfArray colorSpaceArray)
        {
            var namesArray = colorSpaceArray.Elements.GetArray(1);
            numberInputValues = namesArray.Elements.Count;

            var alternateSpaceArray = colorSpaceArray.Elements.GetArray(2);
            alternateColorSpace = ColorSpaceManager.CreateColorSpace(alternateSpaceArray);
            var tintTransformDict = colorSpaceArray.Elements.GetDictionary(3);
            function = FunctionManager.ReadFunction(tintTransformDict);

            //if (colorSpaceArray.Elements.Count == 5)
            //{
            //    var attributes = colorSpaceArray.Elements.GetDictionary(4);
            //}
        }

        /// <summary>
        /// Get the number of values (components) that build up a color
        /// </summary>
        /// <returns></returns>
        public int GetNumberOfValuesPerColor()
        {
            return numberInputValues;
        }

        /// <summary>
        /// GetColor
        /// </summary>
        public Color GetColor(List<double> values, double alpha)
        {
            Color color;

            var results = function.Calculate(values);

            if (results != null)
            {
                color = alternateColorSpace.GetColor(results, alpha);
            }
            else
            {
                byte gray = 0xB0;
                color = Color.FromArgb((byte)(alpha * 255), gray, gray, gray);
            }

            return color;
        }

        /// <summary>
        /// Get a brush descriptor
        /// </summary>
        public IBrushDescriptor GetBrushDescriptor(CSequence operands, Matrix matrix, double alpha)
        {
            IBrushDescriptor altColorSpaceDescriptor = null;

            var values = PdfUtilities.CreateDoubleArray(operands);
            var altColorSpaceValues = function.Calculate(values);

            if (altColorSpaceValues != null)
            {
                var altColorOperands = new CSequence();

                foreach (var val in altColorSpaceValues)
                {
                    var real = new CReal();
                    real.Value = val;
                    altColorOperands.Add(real);
                }

                altColorSpaceDescriptor = alternateColorSpace.GetBrushDescriptor(altColorOperands, matrix, alpha);
            }

            return new DeviceNBrushDescriptor(altColorSpaceDescriptor);
        }
    }

    /// <summary>
    /// The deviceN brush creator
    /// </summary>
    internal class DeviceNBrushDescriptor : IBrushDescriptor
    {
        private IBrushDescriptor altColorSpaceDescriptor;

        /// <summary>
        /// The color precision
        /// </summary>
        public GraphicColorPrecision ColorPrecision
        {
            get
            {
                return altColorSpaceDescriptor?.ColorPrecision ?? GraphicColorPrecision.Placeholder;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public DeviceNBrushDescriptor(IBrushDescriptor altColorSpaceDescriptor)
        {
            this.altColorSpaceDescriptor = altColorSpaceDescriptor;
        }

        /// <summary>
        /// Get a graphic brush
        /// </summary>
        public GraphicBrush GetBrush(PdfRect boundingBox, List<FunctionStop> softMask)
        {
            GraphicBrush brush;

            if (altColorSpaceDescriptor != null)
            {
                brush = altColorSpaceDescriptor.GetBrush(boundingBox, softMask);
            }
            else
            {
                byte gray = 0xB0;
                var color = Color.FromArgb((byte)255, gray, gray, gray);
                brush = new GraphicSolidColorBrush { Color = color };
            }

            return brush;
        }
    }
}

