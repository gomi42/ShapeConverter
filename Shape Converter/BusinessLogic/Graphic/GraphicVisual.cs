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

namespace ShapeConverter
{
    /// <summary>
    /// Base class for all visuals
    /// </summary>
    public abstract class GraphicVisual
    {
    }

    /// <summary>
    /// A graphic path
    /// </summary>
    public class GraphicPath : GraphicVisual
    {
        public GraphicBrush FillBrush { get; set; }

        public double StrokeThickness { get; set; } = 1;

        public GraphicBrush StrokeBrush { get; set; }

        public double StrokeMiterLimit { get; set; } = 10;

        public GraphicLineCap StrokeLineCap { get; set; } = GraphicLineCap.Flat;

        public GraphicLineJoin StrokeLineJoin { get; set; } = GraphicLineJoin.Miter;

        public List<double> StrokeDashes { get; set; }

        public double StrokeDashOffset { get; set; }

        public GraphicColorPrecision ColorPrecision { get; set; } = GraphicColorPrecision.Precise;

        public GraphicPathGeometry Geometry { get; set; } = new GraphicPathGeometry();
    }

    /// <summary>
    /// A graphic group
    /// </summary>
    public class GraphicGroup : GraphicVisual
    {
        public List<GraphicVisual> Childreen { get; set; } = new List<GraphicVisual>();

        public double Opacity { get; set; } = 1;

        // right now we support only one single geometry for clipping
        public GraphicPathGeometry Clip { get; set; }
    }
}
