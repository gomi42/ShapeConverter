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
using ShapeConverter.BusinessLogic.Generators;

namespace ShapeConverter
{
    /// <summary>
    /// Base class for all path segments
    /// </summary>
    public abstract class GraphicPathSegment
    {
    }

    /// <summary>
    /// A move object
    /// </summary>
    public class GraphicMoveSegment : GraphicPathSegment
    {
        public Point StartPoint { get; set; }

        public bool IsClosed { get; set; }
    }

    /// <summary>
    /// A line to object
    /// </summary>
    public class GraphicLineSegment : GraphicPathSegment
    {
        public Point To { get; set; }
    }

    /// <summary>
    /// A cubic bezier object
    /// </summary>
    public class GraphicCubicBezierSegment : GraphicPathSegment
    {
        public Point ControlPoint1 { get; set; }

        public Point ControlPoint2 { get; set; }

        public Point EndPoint { get; set; }
    }

    /// <summary>
    /// A quadrtic bezier object
    /// </summary>
    public class GraphicQuadraticBezierSegment : GraphicPathSegment
    {
        public Point ControlPoint { get; set; }

        public Point EndPoint { get; set; }
    }

    /// <summary>
    /// A graphic path geometry
    /// </summary>
    public class GraphicPathGeometry
    {
        private Rect? rect;

        public Rect Bounds
        {
            get
            {
                if (!rect.HasValue)
                {
                    var geometry = GeometryBinaryGenerator.GenerateGeometry(this);
                    rect = geometry.Bounds;
                }

                return rect.Value;
            }
        }

        public GraphicFillRule FillRule { get; set; }

        public List<GraphicPathSegment> Segments { get; set; } = new List<GraphicPathSegment>();

        public GraphicPathGeometry Clone()
        {
            var clone = new GraphicPathGeometry();

            clone.FillRule = FillRule;
            clone.Segments.AddRange(Segments);

            return clone;
        }
    }
}
