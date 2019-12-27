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

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using PdfSharp.Pdf;
using ShapeConverter.BusinessLogic.Helper;
using ShapeConverter.BusinessLogic.Parser.Pdf.Function;
using ShapeConverter.Parser.Pdf;

namespace ShapeConverter.BusinessLogic.Parser.Pdf.Shading
{
    internal class RadialGradientShading : IShading
    {
        private IFunction function;
        private IColorSpace colorSpace;
        private Point center0;
        private Point center1;
        private double radius0;
        private double radius1;

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
            center0 = new Point(coordsArray.Elements.GetReal(0), coordsArray.Elements.GetReal(1));
            center1 = new Point(coordsArray.Elements.GetReal(3), coordsArray.Elements.GetReal(4));
            radius0 = coordsArray.Elements.GetReal(2);
            radius1 = coordsArray.Elements.GetReal(5);

            // I think we don't need to handle this for Illustrator
            var extendArray = shadingDict.Elements.GetArray(PdfKeys.Extend);
        }

        /// <summary>
        /// Get the color
        /// </summary>
        public GraphicBrush GetBrush(Matrix matrix, PdfRect boundingBox, double alpha, List<FunctionStop> softMask)
        {
            var stops = function.GetBoundaryValues();
            var linearGradientBrush = new GraphicRadialGradientBrush();

            // see comment in LinearGradientShading.cs in GetBrush for more details

#if GENERATE_RELATIVE_COORDINATES
            // calculate the start position relative to the object rectangle
            var center0UserSpace = center0 * matrix;
            linearGradientBrush.StartPoint = GetRelativePosition(boundingBox, center0UserSpace);

            // calculate the end position relative to the object rectangle
            var center1UserSpace = center1 * matrix;
            linearGradientBrush.EndPoint = GetRelativePosition(boundingBox, center1UserSpace);

            // get the center point and a point on the outer ring
            // in user space coordinates
            var centerPointUserSpace = new Point(0,0) * matrix;
            var outerPointUserSpace = new Point(1,1) * matrix;

            // get the radii in user space
            var gradientRadiusXUserSpace = Math.Abs(outerPointUserSpace.X - centerPointUserSpace.X);
            var gradientRadiusYUserSpace = Math.Abs(outerPointUserSpace.Y - centerPointUserSpace.Y);

            // get the object's size in the user space, we need the radii relative to this size
            var objectWidth = Math.Abs(boundingBox.Right - boundingBox.Left);
            var objectHeight = Math.Abs(boundingBox.Bottom- boundingBox.Top);

            // calculate the relative radius X
            var relativeRadiusX = gradientRadiusXUserSpace / objectWidth;
            linearGradientBrush.RadiusX = radius1 * relativeRadiusX;

            // calculate the relative radius Y
            var relativeRadiusY = gradientRadiusYUserSpace / objectHeight;
            linearGradientBrush.RadiusY = radius1 * relativeRadiusY;
#else
            linearGradientBrush.MappingMode = BrushMappingMode.Absolute;

            // calculate the start position relative to the object rectangle
            var center0UserSpace = center0 * matrix;
            linearGradientBrush.StartPoint = center0UserSpace;

            // calculate the end position relative to the object rectangle
            var center1UserSpace = center1 * matrix;
            linearGradientBrush.EndPoint = center1UserSpace;

            // calculate the radius X
            linearGradientBrush.RadiusX = TransformX(radius1, matrix);

            // calculate the radius Y
            linearGradientBrush.RadiusY = TransformY(radius1, matrix);
#endif

            linearGradientBrush.GradientStops = new List<GraphicGradientStop>();

            if (stops.Count > 0 && !DoubleUtilities.IsZero(stops[0].Stop) && !DoubleUtilities.IsZero(radius0))
            {
                var graphicGradientStop = new GraphicGradientStop();
                linearGradientBrush.GradientStops.Add(graphicGradientStop);

                graphicGradientStop.Color = Colors.Transparent;
                graphicGradientStop.Position = 0;

                graphicGradientStop = new GraphicGradientStop();
                linearGradientBrush.GradientStops.Add(graphicGradientStop);

                graphicGradientStop.Color = Colors.Transparent;
                graphicGradientStop.Position = stops[0].Stop;
            }

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
        /// Convert an absolute position in the same coordinate space as the
        /// given rectangle to a relative position within the bound of the given rectangle
        /// </summary>
        private Point GetRelativePosition(PdfRect rect, Point absPosition)
        {
            var x = (absPosition.X - rect.Left) / (rect.Right - rect.Left);
            var y = (absPosition.Y - rect.Top) / (rect.Bottom - rect.Top);

            return new Point(x, y);
        }

        /// <summary>
        /// Normalize a size in x direction
        /// </summary>
        private double TransformX(double x, Matrix matrix)
        {
            return x * matrix.M11 + x * matrix.M12;
        }

        /// <summary>
        /// Normalize a size in x direction
        /// </summary>
        private double TransformY(double y, Matrix matrix)
        {
            return y * matrix.M21 + y * matrix.M22;
        }
    }
}
