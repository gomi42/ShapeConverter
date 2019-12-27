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
using ShapeConverter.BusinessLogic.Helper;

namespace ShapeConverter.BusinessLogic.ShapeConverter
{
    /// <summary>
    /// Rect to Geometry converter
    /// </summary>
    internal static class RectToGeometryConverter
    {
        // Approximating a 1/4 circle with a Bezier curve                _
        private const double arcAsBezier = 0.5522847498307933984; // =( \/2 - 1)*4/3

        /// <summary>
        /// Create a rectangle with absolute points from the rectangle
        /// definition with relative values (width, height)
        /// </summary>
        public static GraphicPathGeometry RectToGeometry(Rect rect, double radiusX, double radiusY)
        {
            GraphicPathGeometry geometry = new GraphicPathGeometry();

            if (!DoubleUtilities.IsZero(radiusX) && !DoubleUtilities.IsZero(radiusY))
            {
                var points = GetRectPointList(rect, radiusX, radiusY);

                var move = new GraphicMoveSegment { StartPoint = points[0] };
                move.IsClosed = true;
                geometry.Segments.Add(move);

                var bezier = new GraphicCubicBezierSegment();
                geometry.Segments.Add(bezier);
                bezier.ControlPoint1 = points[1];
                bezier.ControlPoint2 = points[2];
                bezier.EndPoint = points[3];

                var lineTo = new GraphicLineSegment { To = points[4] };
                geometry.Segments.Add(lineTo);

                bezier = new GraphicCubicBezierSegment();
                geometry.Segments.Add(bezier);
                bezier.ControlPoint1 = points[5];
                bezier.ControlPoint2 = points[6];
                bezier.EndPoint = points[7];

                lineTo = new GraphicLineSegment { To = points[8] };
                geometry.Segments.Add(lineTo);

                bezier = new GraphicCubicBezierSegment();
                geometry.Segments.Add(bezier);
                bezier.ControlPoint1 = points[9];
                bezier.ControlPoint2 = points[10];
                bezier.EndPoint = points[11];

                lineTo = new GraphicLineSegment { To = points[12] };
                geometry.Segments.Add(lineTo);

                bezier = new GraphicCubicBezierSegment();
                geometry.Segments.Add(bezier);
                bezier.ControlPoint1 = points[13];
                bezier.ControlPoint2 = points[14];
                bezier.EndPoint = points[15];
            }
            else
            {
                var points = GetRectPointList(rect);

                var move = new GraphicMoveSegment { StartPoint = points[0] };
                move.IsClosed = true;
                geometry.Segments.Add(move);

                var lineTo = new GraphicLineSegment { To = points[1] };
                geometry.Segments.Add(lineTo);

                lineTo = new GraphicLineSegment { To = points[2] };
                geometry.Segments.Add(lineTo);

                lineTo = new GraphicLineSegment { To = points[3] };
                geometry.Segments.Add(lineTo);
            }

            return geometry;
        }

        /// <summary>
        /// Create the point list for a rectangle
        /// </summary>
        private static Point[] GetRectPointList(Rect rect, double radiusX, double radiusY)
        {
            Point[] points = new Point[17];

            radiusX = Math.Min(rect.Width * (1.0 / 2.0), Math.Abs(radiusX));
            radiusY = Math.Min(rect.Height * (1.0 / 2.0), Math.Abs(radiusY));

            double bezierX = ((1.0 - arcAsBezier) * radiusX);
            double bezierY = ((1.0 - arcAsBezier) * radiusY);

            points[1].X = points[0].X = points[15].X = points[14].X = rect.X;
            points[2].X = points[13].X = rect.X + bezierX;
            points[3].X = points[12].X = rect.X + radiusX;
            points[4].X = points[11].X = rect.Right - radiusX;
            points[5].X = points[10].X = rect.Right - bezierX;
            points[6].X = points[7].X = points[8].X = points[9].X = rect.Right;

            points[2].Y = points[3].Y = points[4].Y = points[5].Y = rect.Y;
            points[1].Y = points[6].Y = rect.Y + bezierY;
            points[0].Y = points[7].Y = rect.Y + radiusY;
            points[15].Y = points[8].Y = rect.Bottom - radiusY;
            points[14].Y = points[9].Y = rect.Bottom - bezierY;
            points[13].Y = points[12].Y = points[11].Y = points[10].Y = rect.Bottom;

            points[16] = points[0];

            return points;
        }

        /// <summary>
        /// Create the point list for a rectangle
        /// </summary>
        private static Point[] GetRectPointList(Rect rect)
        {
            Point[] points = new Point[5];

            points[0].X = points[3].X = points[4].X = rect.X;
            points[1].X = points[2].X = rect.Right;

            points[0].Y = points[1].Y = points[4].Y = rect.Y;
            points[2].Y = points[3].Y = rect.Bottom;

            return points;
        }
    }
}
