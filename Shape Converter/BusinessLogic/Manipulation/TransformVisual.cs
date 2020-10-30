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
using System.Windows.Media;
using ShapeConverter.BusinessLogic.Helper;

namespace ShapeConverter.BusinessLogic.Generators
{
    /// <summary>
    /// The normalize graphic path class
    /// </summary>
    public class TransformVisual
    {
        private Matrix transformMatrix;

        /// <summary>
        /// Transform the given graphic visual
        /// </summary>
        public GraphicVisual Transform(GraphicVisual visual, Matrix transformMatrix)
        {
            this.transformMatrix = transformMatrix;
            return Transform(visual);
        }

        /// <summary>
        /// Transform a single geometry
        /// </summary>
        public GraphicPathGeometry Transform(GraphicPathGeometry geometry, Matrix transformMatrix)
        {
            this.transformMatrix = transformMatrix;
            return TransformGeometry(geometry);
        }

        /// <summary>
        /// Transform a list of segments
        /// </summary>
        public List<GraphicPathSegment> TransformSegments(List<GraphicPathSegment> segments, Matrix transformMatrix)
        {
            this.transformMatrix = transformMatrix;
            return TransformSegments(segments);
        }

        /// <summary>
        /// The transformation engine
        /// </summary>
        /// <returns></returns>
        private GraphicVisual Transform(GraphicVisual visual)
        {
            GraphicVisual graphicGeometry = null;

            switch (visual)
            {
                case GraphicGroup group:
                {
                    var graphicGroup = new GraphicGroup();
                    graphicGeometry = graphicGroup;
                    graphicGroup.Opacity = group.Opacity;

                    if (group.Clip != null)
                    {
                        graphicGroup.Clip = TransformGeometry(group.Clip);
                    }

                    foreach (var childVisual in group.Children)
                    {
                        var normalizedVisual = Transform(childVisual);
                        graphicGroup.Children.Add(normalizedVisual);
                    }

                    break;
                }

                case GraphicPath graphicPath:
                {
                    var transformedPath = new GraphicPath();
                    graphicGeometry = transformedPath;

                    transformedPath.Geometry = TransformGeometry(graphicPath.Geometry);
                    SetShapeAttributes(graphicPath, transformedPath);
                    break;
                }
            }

            return graphicGeometry;
        }

        /// <summary>
        /// Transform a path
        /// </summary>
        private GraphicPathGeometry TransformGeometry(GraphicPathGeometry geometry)
        {
            GraphicPathGeometry transformedPath = new GraphicPathGeometry();
            transformedPath.FillRule = geometry.FillRule;

            transformedPath.Segments = TransformSegments(geometry.Segments);

            return transformedPath;
        }

        private List<GraphicPathSegment> TransformSegments(List<GraphicPathSegment> segments)
        {
            var transformedSegments = new List<GraphicPathSegment>();

            foreach (var segment in segments)
            {
                switch (segment)
                {
                    case GraphicMoveSegment graphicMove:
                    {
                        var normalizedMove = new GraphicMoveSegment();
                        transformedSegments.Add(normalizedMove);

                        normalizedMove.StartPoint = MatrixUtilities.TransformPoint(graphicMove.StartPoint, transformMatrix);
                        normalizedMove.IsClosed = graphicMove.IsClosed;
                        break;
                    }

                    case GraphicLineSegment graphicLineTo:
                    {
                        var normalizedLineTo = new GraphicLineSegment();
                        transformedSegments.Add(normalizedLineTo);

                        normalizedLineTo.To = MatrixUtilities.TransformPoint(graphicLineTo.To, transformMatrix);
                        break;
                    }

                    case GraphicCubicBezierSegment graphicCubicBezier:
                    {
                        var normalizedCubicBezier = new GraphicCubicBezierSegment();
                        transformedSegments.Add(normalizedCubicBezier);

                        normalizedCubicBezier.ControlPoint1 = MatrixUtilities.TransformPoint(graphicCubicBezier.ControlPoint1, transformMatrix);
                        normalizedCubicBezier.ControlPoint2 = MatrixUtilities.TransformPoint(graphicCubicBezier.ControlPoint2, transformMatrix);
                        normalizedCubicBezier.EndPoint = MatrixUtilities.TransformPoint(graphicCubicBezier.EndPoint, transformMatrix);
                        break;
                    }

                    case GraphicQuadraticBezierSegment graphicQuadraticBezier:
                    {
                        var normalizedQuadraticBezier = new GraphicQuadraticBezierSegment();
                        transformedSegments.Add(normalizedQuadraticBezier);

                        normalizedQuadraticBezier.ControlPoint = MatrixUtilities.TransformPoint(graphicQuadraticBezier.ControlPoint, transformMatrix);
                        normalizedQuadraticBezier.EndPoint = MatrixUtilities.TransformPoint(graphicQuadraticBezier.EndPoint, transformMatrix);
                        break;
                    }

                    default:
                        break;
                }
            }

            return transformedSegments;
        }

        /// <summary>
        /// Set common shape attributes
        /// </summary>
        private void SetShapeAttributes(GraphicPath graphicPath, GraphicPath transformedPath)
        {
            transformedPath.FillBrush = TransformBrush(graphicPath.FillBrush);

            transformedPath.StrokeBrush = TransformBrush(graphicPath.StrokeBrush);
            transformedPath.StrokeThickness = MatrixUtilities.TransformScale(graphicPath.StrokeThickness, transformMatrix);
            transformedPath.StrokeMiterLimit = MatrixUtilities.TransformScale(graphicPath.StrokeMiterLimit, transformMatrix);
            transformedPath.StrokeLineCap = graphicPath.StrokeLineCap;
            transformedPath.StrokeLineJoin = graphicPath.StrokeLineJoin;
            transformedPath.StrokeDashOffset = graphicPath.StrokeDashOffset;
            transformedPath.StrokeDashes = graphicPath.StrokeDashes;
        }

        /// <summary>
        /// Normalize a brush
        /// </summary>
        private GraphicBrush TransformBrush(GraphicBrush graphicBrush)
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

                        newlinearGradientBrush.StartPoint = MatrixUtilities.TransformPoint(linearGradientBrush.StartPoint, transformMatrix);
                        newlinearGradientBrush.EndPoint = MatrixUtilities.TransformPoint(linearGradientBrush.EndPoint, transformMatrix);
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

                        newlinearGradientBrush.StartPoint = MatrixUtilities.TransformPoint(radialGradientBrush.StartPoint, transformMatrix);
                        newlinearGradientBrush.EndPoint = MatrixUtilities.TransformPoint(radialGradientBrush.EndPoint, transformMatrix);
                        (newlinearGradientBrush.RadiusX, newlinearGradientBrush.RadiusY) = MatrixUtilities.TransformSize(radialGradientBrush.RadiusX, radialGradientBrush.RadiusY, transformMatrix);
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
    }
}
