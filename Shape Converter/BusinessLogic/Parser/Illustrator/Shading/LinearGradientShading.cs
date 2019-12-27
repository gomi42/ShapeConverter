#define GENERATE_RELATIVE_COORDINATES
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
using ShapeConverter.BusinessLogic.Parser.Pdf.Function;
using ShapeConverter.Parser.Pdf;

namespace ShapeConverter.BusinessLogic.Parser.Pdf.Shading
{
    /// <summary>
    /// The linear gradient shading descriptor
    /// PDF reference 8.7.4.5.3, page 185
    /// </summary>
    internal class LinearGradientShading : IShading
    {
        private IColorSpace colorSpace;
        private IFunction function;
        private Point coords0;
        private Point coords1;

        /// <summary>
        /// The color precision
        /// </summary>
        public GraphicColorPrecision ColorPrecision => colorSpace.ColorPrecision;

        /// <summary>
        /// Init
        /// </summary>
        public void Init(PdfDictionary shadingDict)
        {
            var colorSpaceArray = shadingDict.Elements.GetArray(PdfKeys.ColorSpace);
            colorSpace = ColorSpaceManager.CreateColorSpace(colorSpaceArray);

            var functionDict = shadingDict.Elements.GetDictionary(PdfKeys.Function);
            function = FunctionManager.ReadFunction(functionDict);

            var coordsArray = shadingDict.Elements.GetArray(PdfKeys.Coords);
            coords0 = new Point(coordsArray.Elements.GetReal(0), coordsArray.Elements.GetReal(1));
            coords1 = new Point(coordsArray.Elements.GetReal(2), coordsArray.Elements.GetReal(3));

            // I think we don't need to handle this for Illustrator
            var extendArray = shadingDict.Elements.GetArray(PdfKeys.Extend);
        }

        /// <summary>
        /// Get a brush
        /// </summary>
        public GraphicBrush GetBrush(Matrix matrix, PdfRect boundingBox, double alpha, List<FunctionStop> softMask)
        {
            var stops = function.GetBoundaryValues();
            var linearGradientBrush = new GraphicLinearGradientBrush();

            // The whole logic of the shape converter is able to handle both relative and
            // absolute coordinates of gradients. WPF also allows both mapping modes. But
            // there is one single case where absolute coordinates don't work: in a <Path/>
            // object when the stretch mode is other than None. Such a path isn't really
            // helpfull. That's why all parsers generate relative coordinates.

#if GENERATE_RELATIVE_COORDINATES
            var p0 = coords0 * matrix;
            linearGradientBrush.StartPoint = GetRelativePosition(boundingBox, p0);

            var p1 = coords1 * matrix;
            linearGradientBrush.EndPoint = GetRelativePosition(boundingBox, p1);
#else
            linearGradientBrush.MappingMode = BrushMappingMode.Absolute;

            var p0 = coords0 * matrix;
            linearGradientBrush.StartPoint = p0;

            var p1 = coords1 * matrix;
            linearGradientBrush.EndPoint = p1;
#endif

            linearGradientBrush.GradientStops = new List<GraphicGradientStop>();

            for (int i = 0; i < stops.Count; i++)
            {
                var stop = stops[i];

                var graphicGradientStop = new GraphicGradientStop();
                linearGradientBrush.GradientStops.Add(graphicGradientStop);

                var stopAlpha = alpha;

                if (softMask != null)
                {
                    stopAlpha = stopAlpha * softMask[i].Value[0];
                }

                var color = colorSpace.GetColor(stop.Value, stopAlpha);
                graphicGradientStop.Color = color;
                graphicGradientStop.Position = stop.Stop;
            }

            return linearGradientBrush;
        }

        /// <summary>
        /// Get relative point
        /// </summary>
        private Point GetRelativePosition(PdfRect rect, Point absPosition)
        {
            var x = (absPosition.X - rect.Left) / (rect.Right - rect.Left);
            var y = (absPosition.Y - rect.Top) / (rect.Bottom - rect.Top);

            return new Point(x, y);
        }
    }
}
