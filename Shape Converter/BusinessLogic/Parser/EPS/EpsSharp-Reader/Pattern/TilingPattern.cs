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
using System.Windows.Media;
using EpsSharp.Eps.Core;
using EpsSharp.Eps.Function;
using ShapeConverter;

namespace EpsSharp.Eps.Pattern
{
    /// <summary>
    /// The tiling pattern
    /// </summary>
    internal class TilingPattern : IPattern
    {
        /// <summary>
        /// The color precision
        /// </summary>
        public GraphicColorPrecision ColorPrecision => GraphicColorPrecision.Placeholder;

        /// <summary>
        /// init
        /// </summary>
        public void Init(EpsInterpreter interpreter, DictionaryOperand patternDict)
        {
        }

        /// <summary>
        /// Get a brush
        /// </summary>
        public GraphicBrush GetBrush(Matrix matrix, EpsRect rect)
        {
            var linear = new GraphicLinearGradientBrush();

            linear.StartPoint = new System.Windows.Point(0, 1);
            linear.EndPoint = new System.Windows.Point(1, 0);

            linear.GradientStops = new List<GraphicGradientStop>();

            var stop = new GraphicGradientStop();
            linear.GradientStops.Add(stop);
            stop.Color = Colors.Yellow;
            stop.Position = 0;

            stop = new GraphicGradientStop();
            linear.GradientStops.Add(stop);
            stop.Color = Colors.Cyan;
            stop.Position = 1;

            return linear;
        }
    }
}
