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

namespace ShapeConverter.Parser.Pdf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    internal class TextVectorizer
    {
        GraphicsState currentGraphicsState;
        FontState fontState;
        Matrix fontTransformation;

        /// <summary>
        /// Convert a given text to graphic paths
        /// </summary>
        public List<GraphicPath> Vectorize(string text, GraphicsState currentGraphicsState, FontState fontState)
        {
            this.currentGraphicsState = currentGraphicsState;
            this.fontState = fontState;

            List<GraphicPath> pdfGraphicsPaths = new List<GraphicPath>();

            var typeFace = fontState.FontDescriptor.GetTypeFace();

            FormattedText formattedText = new FormattedText(
              text,
              CultureInfo.InvariantCulture,
              FlowDirection.LeftToRight,
              typeFace,
              fontState.FontSize,
              Brushes.Black,
              96);

            var fontMirror = new Matrix(1, 0, 0, -1, 0, formattedText.Baseline);
            fontTransformation = fontMirror * fontState.TextMatrix * currentGraphicsState.Mirror;

            var myPathGeometry = formattedText.BuildGeometry(new Point(0, 0));
            ConvertToGraphicPath(pdfGraphicsPaths, myPathGeometry);

            var m = new Matrix(fontState.TextMatrix.M11, fontState.TextMatrix.M12, fontState.TextMatrix.M21, fontState.TextMatrix.M22, 0, 0);
            var p2 = new Point(formattedText.WidthIncludingTrailingWhitespace, 0) * m;

            var fsm = fontState.TextMatrix;
            fsm.Translate(p2.X, 0);
            fontState.TextMatrix = fsm;

            return pdfGraphicsPaths;
        }

        /// <summary>
        /// Convert a geometry to graphic paths
        /// </summary>
        void ConvertToGraphicPath(List<GraphicPath> pdfGraphicsPaths, Geometry geometry)
        {
            GraphicPath currentPath = null;

            switch (geometry)
            {
                case GeometryGroup group:
                {
                    foreach (var child in group.Children)
                    {
                        ConvertToGraphicPath(pdfGraphicsPaths, child);
                    }

                    break;
                }

                case CombinedGeometry combined:
                {
                    break;
                }

                case EllipseGeometry ellipse:
                {
                    break;
                }

                case LineGeometry line:
                {
                    break;
                }

                case RectangleGeometry rectangle:
                {
                    break;
                }

                case System.Windows.Media.StreamGeometry stream:
                {
                    break;
                }

                case PathGeometry path:
                {
                    foreach (var figure in path.Figures)
                    {
                        if (currentPath == null)
                        {
                            currentPath = new GraphicPath();

                            switch (fontState.RenderingMode)
                            {
                                case FontRenderingMode.Fill:
                                    currentPath.FillBrush = currentGraphicsState.FillBrush.GetBrush(null, null);
                                    break;

                                case FontRenderingMode.Stroke:
                                    currentPath.StrokeBrush = currentGraphicsState.StrokeBrush.GetBrush(null, null);
                                    currentPath.StrokeThickness = currentGraphicsState.LineWidth;
                                    break;

                                case FontRenderingMode.FillAndStroke:
                                    currentPath.FillBrush = currentGraphicsState.FillBrush.GetBrush(null, null);
                                    currentPath.StrokeBrush = currentGraphicsState.StrokeBrush.GetBrush(null, null);
                                    currentPath.StrokeThickness = currentGraphicsState.LineWidth;
                                    break;
                            }

                            pdfGraphicsPaths.Add(currentPath);
                        }

                        var xs = figure.StartPoint.X;
                        var ys = figure.StartPoint.Y;
                        var points = TransformPoint(xs, ys);

                        var move = new GraphicMoveSegment { StartPoint = points };
                        move.IsClosed = figure.IsClosed;
                        currentPath.Geometry.Segments.Add(move);

                        foreach (var segment in figure.Segments)
                        {
                            switch (segment)
                            {
                                case LineSegment line:
                                {
                                    var x = line.Point.X;
                                    var y = line.Point.Y;
                                    var point = TransformPoint(x, y);

                                    var lineTo = new GraphicLineSegment { To = point };
                                    currentPath.Geometry.Segments.Add(lineTo);
                                    break;
                                }

                                case BezierSegment bezier:
                                {
                                    var gbezier = new GraphicCubicBezierSegment();
                                    currentPath.Geometry.Segments.Add(gbezier);

                                    var x = bezier.Point1.X;
                                    var y = bezier.Point1.Y;
                                    gbezier.ControlPoint1 = TransformPoint(x, y);

                                    x = bezier.Point2.X;
                                    y = bezier.Point2.Y;
                                    gbezier.ControlPoint2 = TransformPoint(x, y);

                                    x = bezier.Point3.X;
                                    y = bezier.Point3.Y;
                                    gbezier.EndPoint = TransformPoint(x, y);
                                    break;
                                }

                                case ArcSegment arc:
                                    break;

                                case PolyLineSegment polyLine:
                                {
                                    foreach (var point in polyLine.Points)
                                    {
                                        var x = point.X;
                                        var y = point.Y;
                                        var gpoint = TransformPoint(x, y);

                                        var lineTo = new GraphicLineSegment { To = gpoint };
                                        currentPath.Geometry.Segments.Add(lineTo);
                                    }
                                    break;
                                }

                                case PolyBezierSegment polyBezier:
                                {
                                    for (int i = 0; i < polyBezier.Points.Count; i += 3)
                                    {
                                        var gbezier = new GraphicCubicBezierSegment();
                                        currentPath.Geometry.Segments.Add(gbezier);

                                        var x = polyBezier.Points[i].X;
                                        var y = polyBezier.Points[i].Y;
                                        gbezier.ControlPoint1 = TransformPoint(x, y);

                                        x = polyBezier.Points[i + 1].X;
                                        y = polyBezier.Points[i + 1].Y;
                                        gbezier.ControlPoint2 = TransformPoint(x, y);

                                        x = polyBezier.Points[i + 2].X;
                                        y = polyBezier.Points[i + 2].Y;
                                        gbezier.EndPoint = TransformPoint(x, y);
                                    }
                                    break;
                                }

                                case PolyQuadraticBezierSegment polyQuadratic:
                                    break;

                                case QuadraticBezierSegment quadraticBezier:
                                    break;

                                default:
                                    break;
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
        private Point TransformPoint(double x, double y)
        {
            return new Point(x, y) * fontTransformation;
        }

        /// <summary>
        /// Adjust horizonal text position
        /// </summary>
        public void AdjustPosition(double adjustment, FontState fontState)
        {
            var m1 = new Matrix(fontState.TextMatrix.M11, fontState.TextMatrix.M12, fontState.TextMatrix.M21, fontState.TextMatrix.M22, 0, 0);
            var p2 = new Point(adjustment, 0) * m1;

            var m = fontState.TextMatrix;
            m.Translate(-p2.X / 1000 * fontState.FontSize, 0);
            fontState.TextMatrix = m;
        }
    }
}

