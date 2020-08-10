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

using System.Windows.Media;

namespace ShapeConverter.BusinessLogic.Generators
{
    /// <summary>
    /// Create a binary geometry
    /// </summary>
    public static class GeometryBinaryGenerator
    {
        /// <summary>
        /// Generates a WPF geometry for a single graphic path geometry
        /// </summary>
        public static Geometry GenerateGeometry(GraphicPathGeometry graphicPathGeometry)
        {
            if (graphicPathGeometry == null)
            {
                return null;
            }

            PathGeometry pathGeometry = new PathGeometry();
            pathGeometry.FillRule = ConvertFillRule(graphicPathGeometry.FillRule);

            PathFigureCollection pathFigureCollection = new PathFigureCollection();
            pathGeometry.Figures = pathFigureCollection;

            PathSegmentCollection pathSegmentCollection = null;

            foreach (var segment in graphicPathGeometry.Segments)
            {
                switch (segment)
                {
                    case GraphicMoveSegment graphicMove:
                    {
                        var pathFigure = new PathFigure();
                        pathFigureCollection.Add(pathFigure);
                        pathFigure.StartPoint = graphicMove.StartPoint;
                        pathFigure.IsClosed = graphicMove.IsClosed;

                        pathSegmentCollection = new PathSegmentCollection();
                        pathFigure.Segments = pathSegmentCollection;
                        break;
                    }

                    case GraphicLineSegment graphicLineTo:
                    {
                        LineSegment lineSegment = new LineSegment();
                        pathSegmentCollection.Add(lineSegment);
                        lineSegment.Point = graphicLineTo.To;
                        break;
                    }

                    case GraphicCubicBezierSegment graphicCubicBezier:
                    {
                        BezierSegment bezierSegment = new BezierSegment();
                        pathSegmentCollection.Add(bezierSegment);
                        bezierSegment.Point1 = graphicCubicBezier.ControlPoint1;
                        bezierSegment.Point2 = graphicCubicBezier.ControlPoint2;
                        bezierSegment.Point3 = graphicCubicBezier.EndPoint;
                        break;
                    }

                    case GraphicQuadraticBezierSegment graphicQuadraticBezier:
                    {
                        QuadraticBezierSegment quadraticBezierSegment = new QuadraticBezierSegment();
                        pathSegmentCollection.Add(quadraticBezierSegment);
                        quadraticBezierSegment.Point1 = graphicQuadraticBezier.ControlPoint;
                        quadraticBezierSegment.Point2 = graphicQuadraticBezier.EndPoint;
                        break;
                    }
                }
            }

            pathGeometry.Freeze();

            return pathGeometry;
        }

        /// <summary>
        /// Converts the fill rule.
        /// </summary>
        private static FillRule ConvertFillRule(GraphicFillRule graphicFillRule)
        {
            FillRule fillRule = FillRule.EvenOdd;

            switch (graphicFillRule)
            {
                case GraphicFillRule.EvenOdd:
                    fillRule = FillRule.EvenOdd;
                    break;

                case GraphicFillRule.NoneZero:
                    fillRule = FillRule.Nonzero;
                    break;
            }

            return fillRule;
        }
    }
}
