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
    /// Class for creating a DrawingBrush out of a graphic visual
    /// </summary>
    public static class DrawingBrushBinaryGenerator
    {
        /// <summary>
        /// Generates a WPF brush from the specified drawing.
        /// </summary>
        public static Brush Generate(GraphicVisual visual)
        {
            var drawingBrush = new DrawingBrush();
            drawingBrush.Stretch = Stretch.Uniform;

            drawingBrush.Drawing = GenerateDrawing(visual);

            return drawingBrush;
        }

        /// <summary>
        /// Generates a WPF drawing from the specified geometry tree.
        /// </summary>
        private static Drawing GenerateDrawing(GraphicVisual visual)
        {
            Drawing drawing = null;

            switch (visual)
            {
                case GraphicGroup group:
                {
                    var drawingGroup = new DrawingGroup();
                    drawing = drawingGroup;
                    drawingGroup.Opacity = group.Opacity;

                    if (group.Clip != null)
                    {
                        drawingGroup.ClipGeometry = GeometryBinaryGenerator.GenerateGeometry(group.Clip);
                    }

                    foreach (var childVisual in group.Childreen)
                    {
                        var childDrawing = GenerateDrawing(childVisual);
                        drawingGroup.Children.Add(childDrawing);
                    }

                    break;
                }

                case GraphicPath graphicPath:
                {
                    drawing = GeneratePath(graphicPath);

                    break;
                }
            }

            return drawing;
        }

        /// <summary>
        /// Create a brush from a single graphic paths
        /// </summary>
        public static Brush Generate(GraphicPath graphicPath)
        {
            var drawingBrush = new DrawingBrush();
            drawingBrush.Stretch = Stretch.Uniform;

            drawingBrush.Drawing = GeneratePath(graphicPath);

            return drawingBrush;
        }

        /// <summary>
        /// Create a GeometryDrawing from a single graphic path
        /// </summary>
        private static Drawing GeneratePath(GraphicPath graphicPath)
        {
            var geometryDrawing = new GeometryDrawing();
            geometryDrawing.Geometry = GeometryBinaryGenerator.GenerateGeometry(graphicPath.Geometry);
            SetColors(graphicPath, geometryDrawing);

            return geometryDrawing;
        }

        /// <summary>
        /// Set the fill and stroke colors
        /// </summary>
        private static void SetColors(GraphicPath graphicPath, GeometryDrawing geometryDrawing)
        {
            if (graphicPath.FillBrush != null)
            {
                geometryDrawing.Brush = GenerateBrush(graphicPath.FillBrush);
            }

            if (graphicPath.StrokeBrush != null)
            {
                var brush = GenerateBrush(graphicPath.StrokeBrush);
                geometryDrawing.Pen = new Pen(brush, graphicPath.StrokeThickness);
                geometryDrawing.Pen.StartLineCap = Converter.ConvertToWPF(graphicPath.StrokeLineCap);
                geometryDrawing.Pen.EndLineCap = geometryDrawing.Pen.StartLineCap;
                geometryDrawing.Pen.LineJoin = Converter.ConvertToWpf(graphicPath.StrokeLineJoin);
                geometryDrawing.Pen.MiterLimit = graphicPath.StrokeMiterLimit;

                if (graphicPath.StrokeDashes != null)
                {
                    var dashStyle = new DashStyle(graphicPath.StrokeDashes, graphicPath.StrokeDashOffset);
                    geometryDrawing.Pen.DashStyle = dashStyle;
                    geometryDrawing.Pen.DashCap = geometryDrawing.Pen.StartLineCap;
                }
            }
        }

        /// <summary>
        /// Generate a WPF brush
        /// </summary>
        private static Brush GenerateBrush(GraphicBrush graphicBrush)
        {
            Brush brush;

            switch (graphicBrush)
            {
                case GraphicSolidColorBrush graphicSolidColor:
                {
                    brush = new SolidColorBrush(graphicSolidColor.Color);
                    break;
                }

                case GraphicLinearGradientBrush graphicLinearGradientBrush:
                {
                    var linearGradientBrush = new LinearGradientBrush();
                    brush = linearGradientBrush;

                    linearGradientBrush.StartPoint = graphicLinearGradientBrush.StartPoint;
                    linearGradientBrush.EndPoint = graphicLinearGradientBrush.EndPoint;
                    linearGradientBrush.MappingMode = Converter.ConvertToWpf(graphicLinearGradientBrush.MappingMode);

                    foreach (var stop in graphicLinearGradientBrush.GradientStops)
                    {
                        var gs = new GradientStop(stop.Color, stop.Position);
                        linearGradientBrush.GradientStops.Add(gs);
                    }

                    break;
                }

                case GraphicRadialGradientBrush graphicRadialGradientBrush:
                {
                    var radialGradientBrush = new RadialGradientBrush();
                    brush = radialGradientBrush;

                    radialGradientBrush.GradientOrigin = graphicRadialGradientBrush.StartPoint;
                    radialGradientBrush.Center = graphicRadialGradientBrush.EndPoint;
                    radialGradientBrush.RadiusX = graphicRadialGradientBrush.RadiusX;
                    radialGradientBrush.RadiusY = graphicRadialGradientBrush.RadiusY;
                    radialGradientBrush.MappingMode = Converter.ConvertToWpf(graphicRadialGradientBrush.MappingMode);

                    foreach (var stop in graphicRadialGradientBrush.GradientStops)
                    {
                        var gs = new GradientStop(stop.Color, stop.Position);
                        radialGradientBrush.GradientStops.Add(gs);
                    }

                    break;
                }

                default:
                {
                    brush = new SolidColorBrush(Colors.LightGray);
                    break;
                }
            }

            return brush;
        }
    }
}
