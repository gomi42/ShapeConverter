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
using EpsSharp.Eps.Core;
using EpsSharp.Eps.Function;
using EpsSharp.Eps.Helper;
using ShapeConverter;

namespace EpsSharp.Eps.Shading
{
    /// <summary>
    /// The linear gradient shading descriptor
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
        public void Init(EpsInterpreter interpreter, DictionaryOperand shadingDict)
        {
            var colorSpaceOperand = shadingDict.Dictionary.Find(EpsKeys.ColorSpace);
            colorSpace = ColorSpaceActivator.CreateColorSpace(interpreter, colorSpaceOperand);

            var functionDict = shadingDict.Dictionary.GetDictionary(EpsKeys.Function);
            function = FunctionActivator.CreateFunction(functionDict);

            var coordsArray = shadingDict.Dictionary.GetArray(EpsKeys.Coords);
            var coords = OperandHelper.GetRealValues(coordsArray);

            coords0 = new Point(coords[0], coords[1]);
            coords1 = new Point(coords[2], coords[3]);
        }

        /// <summary>
        /// Get a brush
        /// </summary>
        public GraphicBrush GetBrush(Matrix matrix, EpsRect boundingBox)
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

                var color = colorSpace.GetColor(stop.Value);
                graphicGradientStop.Color = color;
                graphicGradientStop.Position = stop.Stop;
            }

            return linearGradientBrush;
        }

        /// <summary>
        /// Get relative point
        /// </summary>
        private Point GetRelativePosition(EpsRect rect, Point absPosition)
        {
            var x = (absPosition.X - rect.Left) / (rect.Right - rect.Left);
            var y = (absPosition.Y - rect.Top) / (rect.Bottom - rect.Top);

            return new Point(x, y);
        }
    }
}
