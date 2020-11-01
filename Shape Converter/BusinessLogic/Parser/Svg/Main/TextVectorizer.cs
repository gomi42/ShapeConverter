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

using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace ShapeConverter.BusinessLogic.Parser.Svg
{
    /// <summary>
    /// TextVectorizer
    /// </summary>
    internal static class TextVectorizer
    {
        /// <summary>
        /// Convert a given text to graphic paths
        /// </summary>
        public static (double, double) Vectorize(GraphicPathGeometry graphicPathGeometry,
                                       string text, 
                                       double x, 
                                       double y, 
                                       Typeface typeface, 
                                       double fontSize, 
                                       double rotate,
                                       Matrix currentTransformationMatrix)
        {
            FormattedText formattedText = new FormattedText(
              text,
              CultureInfo.InvariantCulture,
              FlowDirection.LeftToRight,
              typeface,
              fontSize,
              Brushes.Black,
              96);

            Matrix fontTransformation = Matrix.Identity;
            fontTransformation.RotateAt(rotate, 0, formattedText.Baseline);
            fontTransformation.Translate(x, y - formattedText.Baseline);
            fontTransformation = fontTransformation * currentTransformationMatrix;

            var pathGeometry = formattedText.BuildGeometry(new Point(0, 0));
            ConvertToGraphicGeometry(graphicPathGeometry, pathGeometry, fontTransformation);
            var newX = formattedText.WidthIncludingTrailingWhitespace;

            return (x + newX, y);
        }

        /// <summary>
        /// Convert a geometry to graphic geometry
        /// </summary>
        private static void ConvertToGraphicGeometry(GraphicPathGeometry graphicPathGeometry, Geometry geometry, Matrix fontTransformation)
        {
            switch (geometry)
            {
                case GeometryGroup group:
                {
                    foreach (var child in group.Children)
                    {
                        ConvertToGraphicGeometry(graphicPathGeometry, child, fontTransformation);
                    }

                    break;
                }

                case PathGeometry path:
                {
                    foreach (var figure in path.Figures)
                    {
                        var points = TransformPoint(figure.StartPoint, fontTransformation);

                        var move = new GraphicMoveSegment { StartPoint = points };
                        move.IsClosed = figure.IsClosed;
                        graphicPathGeometry.Segments.Add(move);

                        foreach (var segment in figure.Segments)
                        {
                            switch (segment)
                            {
                                case LineSegment line:
                                {
                                    var point = TransformPoint(line.Point, fontTransformation);

                                    var lineTo = new GraphicLineSegment { To = point };
                                    graphicPathGeometry.Segments.Add(lineTo);
                                    break;
                                }

                                case BezierSegment bezier:
                                {
                                    var gbezier = new GraphicCubicBezierSegment();
                                    graphicPathGeometry.Segments.Add(gbezier);

                                    gbezier.ControlPoint1 = TransformPoint(bezier.Point1, fontTransformation);
                                    gbezier.ControlPoint2 = TransformPoint(bezier.Point2, fontTransformation);
                                    gbezier.EndPoint = TransformPoint(bezier.Point3, fontTransformation);
                                    break;
                                }

                                case PolyLineSegment polyLine:
                                {
                                    foreach (var point in polyLine.Points)
                                    {
                                        var gpoint = TransformPoint(point, fontTransformation);

                                        var lineTo = new GraphicLineSegment { To = gpoint };
                                        graphicPathGeometry.Segments.Add(lineTo);
                                    }
                                    break;
                                }

                                case PolyBezierSegment polyBezier:
                                {
                                    for (int i = 0; i < polyBezier.Points.Count; i += 3)
                                    {
                                        var gbezier = new GraphicCubicBezierSegment();
                                        graphicPathGeometry.Segments.Add(gbezier);

                                        gbezier.ControlPoint1 = TransformPoint(polyBezier.Points[i], fontTransformation);
                                        gbezier.ControlPoint2 = TransformPoint(polyBezier.Points[i + 1], fontTransformation);
                                        gbezier.EndPoint = TransformPoint(polyBezier.Points[i + 2], fontTransformation);
                                    }
                                    break;
                                }
                            }
                        }
                    }

                    break;
                }
            }
        }

        /// <summary>
        /// Transform a point to the user space
        /// </summary>
        private static Point TransformPoint(Point point, Matrix fontTransformation)
        {
            return point * fontTransformation;
        }
    }
}
