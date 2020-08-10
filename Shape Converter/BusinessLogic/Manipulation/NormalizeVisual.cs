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

using System.Windows;

namespace ShapeConverter.BusinessLogic.Generators
{
    /// <summary>
    /// Defines how to normalize
    /// </summary>
    public enum NormalizeAspect
    {
        /// <summary>
        /// Normalize the width and keep the height proportional
        /// according to the original aspec ratio
        /// </summary>
        Width,

        /// <summary>
        /// Normalize the height and keep the width proportional
        /// according to the original aspec ratio
        /// </summary>
        Height,

        /// <summary>
        /// Take the bigger value of width and height as normalization master
        /// and keep the other value proportional according to the original aspec ratio
        /// </summary>
        Both,

        /// <summary>
        /// Normalize width and height individually
        /// </summary>
        Individual
    }

    /// <summary>
    /// The normalize graphic path class
    /// </summary>
    public class NormalizeVisual
    {
        double left;
        double top;
        double scaleWidth;
        double scaleHeight;
        double scaleThickness;

        /// <summary>
        /// The aspect ration of the resulting graphic path(s)
        /// </summary>
        public double AspectRatio { get; private set; }

        /// <summary>
        /// Normalizes the given visual and creates a deep copy of the tree
        /// </summary>
        public GraphicVisual Normalize(GraphicVisual visual, NormalizeAspect normalizeType, double scaleTo)
        {
            DetermineScaleFactor(visual, normalizeType, scaleTo);
            return Normalize(visual);
        }

        /// <summary>
        /// Normalizes the given visual so that it fits into the given 
        /// scaleTo box by keeping the aspect ratio. A deep copy of the tree is created.
        /// </summary>
        public GraphicVisual Normalize(GraphicVisual visual, Size scaleTo)
        {
            DetermineScaleFactor(visual, scaleTo);
            return Normalize(visual);
        }

        /// <summary>
        /// Determines the scale factor of the specified visual
        /// </summary>
        private void DetermineScaleFactor(GraphicVisual visual, Size scaleTo)
        {
            var bounds = DetermineBounds(visual);

            left = bounds.Left;
            top = bounds.Top;
            double width = bounds.Width;
            double height = bounds.Height;

            AspectRatio = height / width;

            if (AspectRatio < scaleTo.Height / scaleTo.Width)
            {
                scaleWidth = scaleTo.Width / width;
                scaleHeight = scaleWidth;
            }
            else
            {
                scaleWidth = scaleTo.Height / height;
                scaleHeight = scaleWidth;
            }

            scaleThickness = (scaleWidth + scaleHeight) / 2;
        }

        /// <summary>
        /// Determines the scale factor of the specified visual
        /// </summary>
        private void DetermineScaleFactor(GraphicVisual visual, NormalizeAspect normalizeAspect, double scaleTo)
        {
            var bounds = DetermineBounds(visual);

            left = bounds.Left;
            top = bounds.Top;
            double width = bounds.Width;
            double height = bounds.Height;

            AspectRatio = height / width;

            switch (normalizeAspect)
            {
                case NormalizeAspect.Width:
                    scaleWidth = scaleTo / width;
                    this.scaleHeight = this.scaleWidth;
                    break;

                case NormalizeAspect.Height:
                    scaleWidth = scaleTo / height;
                    this.scaleHeight = this.scaleWidth;
                    break;

                case NormalizeAspect.Both:
                    scaleWidth = scaleTo / width;
                    scaleHeight = scaleTo / height;
                    scaleWidth = height > width ? scaleHeight : scaleWidth;
                    this.scaleHeight = this.scaleWidth;
                    break;

                case NormalizeAspect.Individual:
                    scaleWidth = scaleTo / width;
                    scaleHeight = scaleTo / height;
                    break;
            }

            scaleThickness = (scaleWidth + scaleHeight) / 2;
        }

        /// <summary>
        /// Determines the scale factor of the specified geometry tree
        /// </summary>
        private Rect DetermineBounds(GraphicVisual visual)
        {
            var left = double.PositiveInfinity;
            var top = double.PositiveInfinity;
            var right = double.NegativeInfinity;
            var bottom = double.NegativeInfinity;

            void DetermineBoundsRecursive(GraphicVisual visualRecursive)
            {
                switch (visualRecursive)
                {
                    case GraphicGroup group:
                    {
                        foreach (var childVisual in group.Childreen)
                        {
                            DetermineBoundsRecursive(childVisual);
                        }

                        break;
                    }

                    case GraphicPath graphicPath:
                    {
                        var bounds = graphicPath.Geometry.Bounds;

                        if (bounds.Left < left)
                        {
                            left = bounds.Left;
                        }

                        if (bounds.Top < top)
                        {
                            top = bounds.Top;
                        }

                        if (bounds.Right > right)
                        {
                            right = bounds.Right;
                        }

                        if (bounds.Bottom > bottom)
                        {
                            bottom = bounds.Bottom;
                        }

                        break;
                    }
                }
            }

            DetermineBoundsRecursive(visual);

            if (double.IsPositiveInfinity(left))
            {
                return new Rect();
            }

            return new Rect(left, top, right - left, bottom - top);
        }

        /// <summary>
        /// Normalizes a geometry tree
        /// </summary>
        private GraphicVisual Normalize(GraphicVisual visual)
        {
            GraphicVisual graphicVisual = null;

            switch (visual)
            {
                case GraphicGroup group:
                {
                    var graphicGroup = new GraphicGroup();
                    graphicVisual = graphicGroup;
                    graphicGroup.Opacity = group.Opacity;

                    if (group.Clip != null)
                    {
                        graphicGroup.Clip = NormalizeGeometry(group.Clip);
                    }

                    foreach (var childVisual in group.Childreen)
                    {
                        var normalizedVisual = Normalize(childVisual);
                        graphicGroup.Childreen.Add(normalizedVisual);
                    }

                    break;
                }

                case GraphicPath graphicPath:
                {
                    GraphicPath normalizedPath = new GraphicPath();
                    graphicVisual = normalizedPath;

                    normalizedPath.Geometry = NormalizeGeometry(graphicPath.Geometry);
                    SetShapeAttributes(graphicPath, normalizedPath);
                    break;
                }
            }

            return graphicVisual;
        }

        /// <summary>
        /// Normalizes the specified graphic path.
        /// </summary>
        private GraphicPathGeometry NormalizeGeometry(GraphicPathGeometry graphicPathGeometry)
        {
            GraphicPathGeometry normalizedGeometry = new GraphicPathGeometry();
            normalizedGeometry.FillRule = graphicPathGeometry.FillRule;

            foreach (var pathElement in graphicPathGeometry.Segments)
            {
                switch (pathElement)
                {
                    case GraphicMoveSegment graphicMove:
                    {
                        var normalizedMove = new GraphicMoveSegment();
                        normalizedGeometry.Segments.Add(normalizedMove);

                        normalizedMove.StartPoint = NormalizePoint(graphicMove.StartPoint);
                        normalizedMove.IsClosed = graphicMove.IsClosed;
                        break;
                    }

                    case GraphicLineSegment graphicLineTo:
                    {
                        var normalizedLineTo = new GraphicLineSegment();
                        normalizedGeometry.Segments.Add(normalizedLineTo);

                        normalizedLineTo.To = NormalizePoint(graphicLineTo.To);
                        break;
                    }

                    case GraphicCubicBezierSegment graphicCubicBezier:
                    {
                        var normalizedCubicBezier = new GraphicCubicBezierSegment();
                        normalizedGeometry.Segments.Add(normalizedCubicBezier);

                        normalizedCubicBezier.ControlPoint1 = NormalizePoint(graphicCubicBezier.ControlPoint1);
                        normalizedCubicBezier.ControlPoint2 = NormalizePoint(graphicCubicBezier.ControlPoint2);
                        normalizedCubicBezier.EndPoint = NormalizePoint(graphicCubicBezier.EndPoint);
                        break;
                    }

                    case GraphicQuadraticBezierSegment graphicQuadraticBezier:
                    {
                        var normalizedQuadraticBezier = new GraphicQuadraticBezierSegment();
                        normalizedGeometry.Segments.Add(normalizedQuadraticBezier);

                        normalizedQuadraticBezier.ControlPoint = NormalizePoint(graphicQuadraticBezier.ControlPoint);
                        normalizedQuadraticBezier.EndPoint = NormalizePoint(graphicQuadraticBezier.EndPoint);
                        break;
                    }
                }
            }

            return normalizedGeometry;
        }

        /// <summary>
        /// Set common shape attributes
        /// </summary>
        private void SetShapeAttributes(GraphicPath graphicPath, GraphicPath normalizedPath)
        {
            normalizedPath.FillBrush = NormalizeBrush(graphicPath.FillBrush);

            normalizedPath.StrokeBrush = NormalizeBrush(graphicPath.StrokeBrush);
            normalizedPath.StrokeThickness = NormalizeThickness(graphicPath.StrokeThickness);
            normalizedPath.StrokeLineCap = graphicPath.StrokeLineCap;
            normalizedPath.StrokeLineJoin = graphicPath.StrokeLineJoin;
            normalizedPath.StrokeDashOffset = graphicPath.StrokeDashOffset;
            normalizedPath.StrokeDashes = graphicPath.StrokeDashes;
            normalizedPath.StrokeMiterLimit = NormalizeThickness(graphicPath.StrokeMiterLimit);
        }

        /// <summary>
        /// Normalize a brush
        /// </summary>
        private GraphicBrush NormalizeBrush(GraphicBrush graphicBrush)
        {
            GraphicBrush retBrush;

            switch (graphicBrush)
            {
                case GraphicLinearGradientBrush linearGradientBrush:
                {
                    if (linearGradientBrush.MappingMode == GraphicBrushMappingMode.Absolute)
                    {
                        var newlinearGradientBrush = new GraphicLinearGradientBrush();
                        retBrush = newlinearGradientBrush;

                        newlinearGradientBrush.StartPoint = NormalizePoint(linearGradientBrush.StartPoint);
                        newlinearGradientBrush.EndPoint = NormalizePoint(linearGradientBrush.EndPoint);
                        newlinearGradientBrush.MappingMode = linearGradientBrush.MappingMode;
                        newlinearGradientBrush.GradientStops = linearGradientBrush.GradientStops;
                    }
                    else
                    {
                        retBrush = linearGradientBrush;
                    }

                    break;
                }

                case GraphicRadialGradientBrush radialGradientBrush:
                {
                    if (radialGradientBrush.MappingMode == GraphicBrushMappingMode.Absolute)
                    {
                        var newlinearGradientBrush = new GraphicRadialGradientBrush();
                        retBrush = newlinearGradientBrush;

                        newlinearGradientBrush.StartPoint = NormalizePoint(radialGradientBrush.StartPoint);
                        newlinearGradientBrush.EndPoint = NormalizePoint(radialGradientBrush.EndPoint);
                        newlinearGradientBrush.RadiusX = NormalizeWidth(radialGradientBrush.RadiusX);
                        newlinearGradientBrush.RadiusY = NormalizeWidth(radialGradientBrush.RadiusY);
                        newlinearGradientBrush.MappingMode = radialGradientBrush.MappingMode;
                        newlinearGradientBrush.GradientStops = radialGradientBrush.GradientStops;
                    }
                    else
                    {
                        retBrush = radialGradientBrush;
                    }

                    break;
                }

                default:
                    retBrush = graphicBrush;
                    break;
            }

            return retBrush;
        }

        /// <summary>
        /// Normalize a single point
        /// </summary>
        private Point NormalizePoint(Point point)
        {
            return new Point((point.X - left) * scaleWidth, (point.Y - top) * scaleHeight);
        }

        /// <summary>
        /// Normalize an x value
        /// </summary>
        private double NormalizeWidth(double x)
        {
            return x * scaleWidth;
        }

        /// <summary>
        /// Normalize a y value
        /// </summary>
        private double NormalizeHeight(double y)
        {
            return y * scaleHeight;
        }

        /// <summary>
        /// Normalize a size
        /// </summary>
        private Size NormalizeSize(Size size)
        {
            return new Size(NormalizeWidth(size.Width), NormalizeHeight(size.Height));
        }

        /// <summary>
        /// Normalize thickness
        /// </summary>
        private double NormalizeThickness(double thickness)
        {
            return thickness * scaleThickness;
        }
    }
}
