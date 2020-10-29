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
        public static GraphicPathGeometry Vectorize(string text, double x, double y, Typeface typeface, double fontSize)
        {
            var graphicPathGeometry = new GraphicPathGeometry();

            FormattedText formattedText = new FormattedText(
              text,
              CultureInfo.InvariantCulture,
              FlowDirection.LeftToRight,
              typeface,
              fontSize,
              Brushes.Black,
              96);

            var pathGeometry = formattedText.BuildGeometry(new Point(x, y - formattedText.Baseline));
            ConvertToGraphicGeometry(graphicPathGeometry, pathGeometry);

            return graphicPathGeometry;
        }

        /// <summary>
        /// Convert a geometry to graphic geometry
        /// </summary>
        private static void ConvertToGraphicGeometry(GraphicPathGeometry graphicPathGeometry, Geometry geometry)
        {
            switch (geometry)
            {
                case GeometryGroup group:
                {
                    foreach (var child in group.Children)
                    {
                        ConvertToGraphicGeometry(graphicPathGeometry, child);
                    }

                    break;
                }

                case PathGeometry path:
                {
                    foreach (var figure in path.Figures)
                    {
                        var move = new GraphicMoveSegment { StartPoint = figure.StartPoint };
                        move.IsClosed = figure.IsClosed;
                        graphicPathGeometry.Segments.Add(move);

                        foreach (var segment in figure.Segments)
                        {
                            switch (segment)
                            {
                                case LineSegment line:
                                {
                                    var lineTo = new GraphicLineSegment { To = line.Point };
                                    graphicPathGeometry.Segments.Add(lineTo);
                                    break;
                                }

                                case BezierSegment bezier:
                                {
                                    var gbezier = new GraphicCubicBezierSegment();
                                    graphicPathGeometry.Segments.Add(gbezier);

                                    gbezier.ControlPoint1 = bezier.Point1;
                                    gbezier.ControlPoint2 = bezier.Point2;
                                    gbezier.EndPoint = bezier.Point3;
                                    break;
                                }

                                case ArcSegment arc:
                                    break;

                                case PolyLineSegment polyLine:
                                {
                                    foreach (var point in polyLine.Points)
                                    {
                                        var lineTo = new GraphicLineSegment { To = point };
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

                                        gbezier.ControlPoint1 = polyBezier.Points[i];
                                        gbezier.ControlPoint2 = polyBezier.Points[i + 1];
                                        gbezier.EndPoint = polyBezier.Points[i + 2];
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
    }
}
