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
using System.Windows;

namespace ShapeConverter.BusinessLogic.ShapeConverter
{
    /// <summary>
    /// Ellipse to Geometry Converter
    /// </summary>
    internal static class EllipseToGeometryConverter
    {
        // Approximating a 1/4 circle with a Bezier curve                _
        private const double arcAsBezier = 0.5522847498307933984; // =( \/2 - 1)*4/3
        private const int pointCount = 13;

        /// <summary>
        /// Create an ellipse path with absolute points from the definition with
        /// relative values (radius)
        /// </summary>
        public static GraphicPathGeometry EllipseToGeometry(Point center, double radiusX, double radiusY)
        {
            Point[] points = new Point[pointCount];

            radiusX = Math.Abs(radiusX);
            radiusY = Math.Abs(radiusY);

            // Set the X coordinates
            double mid = radiusX * arcAsBezier;

            points[0].X = points[1].X = points[11].X = points[12].X = center.X + radiusX;
            points[2].X = points[10].X = center.X + mid;
            points[3].X = points[9].X = center.X;
            points[4].X = points[8].X = center.X - mid;
            points[5].X = points[6].X = points[7].X = center.X - radiusX;

            // Set the Y coordinates
            mid = radiusY * arcAsBezier;

            points[2].Y = points[3].Y = points[4].Y = center.Y + radiusY;
            points[1].Y = points[5].Y = center.Y + mid;
            points[0].Y = points[6].Y = points[12].Y = center.Y;
            points[7].Y = points[11].Y = center.Y - mid;
            points[8].Y = points[9].Y = points[10].Y = center.Y - radiusY;

            var geometry = new GraphicPathGeometry();
            var move = new GraphicMoveSegment { StartPoint = points[0] };
            move.IsClosed = true;
            geometry.Segments.Add(move);

            // i == 0, 3, 6, 9
            for (int i = 0; i < 12; i += 3)
            {
                var bezier = new GraphicCubicBezierSegment();
                geometry.Segments.Add(bezier);

                bezier.ControlPoint1 = points[i + 1];
                bezier.ControlPoint2 = points[i + 2];
                bezier.EndPoint = points[i + 3];
            }

            return geometry;
        }
    }
}
