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
using System.Windows.Media;
using System.Xml.Linq;
using ShapeConverter.BusinessLogic.Parser.Svg.Helper;

namespace ShapeConverter.BusinessLogic.Parser.Svg.Main
{
    /// <summary>
    /// The text parser
    /// </summary>
    internal class TextParser : GeometryTextParser
    {
        private Clipping clipping;
        private BrushParser brushParser;

        public TextParser(CssStyleCascade cssStyleCascade,
                          DoubleParser doubleParser,
                          BrushParser brushParser,
                          Clipping clipping)
            : base(cssStyleCascade, doubleParser)
        {
            this.cssStyleCascade = cssStyleCascade;
            this.doubleParser = doubleParser;
            this.brushParser = brushParser;
            this.clipping = clipping;
        }

        /// <summary>
        /// Parse a single text
        /// </summary>
        public GraphicVisual Parse(XElement shape,
                                   Matrix currentTransformationMatrix)
        {
            cssStyleCascade.PushStyles(shape);

            var transformMatrix = cssStyleCascade.GetTransformMatrixFromTop();
            currentTransformationMatrix = transformMatrix * currentTransformationMatrix;

            var graphicVisual = ParseText(shape, currentTransformationMatrix);

            cssStyleCascade.Pop();

            return graphicVisual;
        }

        /// <summary>
        /// 
        /// </summary>
        private class GradientTSpanAdjustment
        {
            public bool AdjustFill;
            public bool AdjustStroke;
            public double StartX;
            public double EndX;
            public GraphicPath Path;
        }

        /// <summary>
        /// Parse a text
        /// </summary>
        private GraphicVisual ParseText(XElement element,
                              Matrix currentTransformationMatrix)
        {
            GraphicVisual graphicVisual;
            GraphicGroup graphicGroup = null;

            var position = new CharacterPositions();

            var xList = GetLengthPercentList(element, "x", PercentBaseSelector.ViewBoxWidth);
            var dxList = GetLengthPercentList(element, "dx", PercentBaseSelector.ViewBoxWidth);
            position.X.SetParentValues(xList, dxList);

            var yList = GetLengthPercentList(element, "y", PercentBaseSelector.ViewBoxHeight);
            var dyList = GetLengthPercentList(element, "dy", PercentBaseSelector.ViewBoxHeight);
            position.Y.SetParentValues(yList, dyList);

            var fontSize = GetFontSize();
            var typeface = GetTypeface();

            var rotation = new ParentChildPriorityList();
            rotation.ParentValues = GetRotate(element);

            var textGeometry = new GraphicPathGeometry();
            textGeometry.FillRule = GraphicFillRule.NoneZero;
            var textGraphicPath = new GraphicPath();
            textGraphicPath.Geometry = textGeometry;
            graphicVisual = textGraphicPath;

            var adjustments = new List<GradientTSpanAdjustment>();

            XNode node = element.FirstNode;
            bool beginOfLine = true;

            while (node != null)
            {
                var nextNode = node.NextNode;
                var hasSuccessor = nextNode != null;

                switch (node)
                {
                    case XElement xElement:
                    {
                        if (xElement.Name.LocalName == "tspan")
                        {
                            cssStyleCascade.PushStyles(xElement);

                            var xChildList = GetLengthPercentList(xElement, "x", PercentBaseSelector.ViewBoxWidth);
                            var dxChildList = GetLengthPercentList(xElement, "dx", PercentBaseSelector.ViewBoxWidth);
                            position.X.SetChildValues(xChildList, dxChildList);

                            var yChildList = GetLengthPercentList(xElement, "y", PercentBaseSelector.ViewBoxHeight);
                            var dyChildList = GetLengthPercentList(xElement, "dy", PercentBaseSelector.ViewBoxHeight);
                            position.Y.SetChildValues(yChildList, dyChildList);

                            var hasOwnFill = ExistsAttributeOnTop("fill");
                            var hasOwnStroke = ExistsAttributeOnTop("stroke");
                            var addNewGeometry = hasOwnFill || hasOwnStroke;
                            GraphicPathGeometry tspanGeometry;
                            double startX = position.X.Current;

                            if (addNewGeometry)
                            {
                                tspanGeometry = new GraphicPathGeometry();
                                textGeometry.FillRule = GraphicFillRule.NoneZero;

                            }
                            else
                            {
                                tspanGeometry = textGeometry;
                            }

                            var tspanFontSize = GetFontSize();
                            var tspanTypeface = GetTypeface();
                            rotation.ChildValues = GetRotate(xElement);

                            Vectorize(tspanGeometry, xElement.Value, position, beginOfLine, hasSuccessor, tspanTypeface, tspanFontSize, rotation, currentTransformationMatrix);

                            rotation.ChildValues = null;
                            position.X.SetChildValues(null, null);
                            position.Y.SetChildValues(null, null);

                            if (addNewGeometry)
                            {
                                var tspanGraphicPath = new GraphicPath();
                                tspanGraphicPath.Geometry = tspanGeometry;

                                if (graphicGroup == null)
                                {
                                    graphicGroup = new GraphicGroup();
                                    graphicGroup.Children.Add(textGraphicPath);
                                    graphicVisual = graphicGroup;
                                }

                                graphicGroup.Children.Add(tspanGraphicPath);

                                brushParser.SetFillAndStroke(xElement, tspanGraphicPath, currentTransformationMatrix);

                                var gc = new GradientTSpanAdjustment
                                {
                                    AdjustFill = !hasOwnFill,
                                    AdjustStroke = !hasOwnStroke,
                                    StartX = startX,
                                    EndX = position.X.Current,
                                    Path = tspanGraphicPath
                                };
                                adjustments.Add(gc);
                            }

                            cssStyleCascade.Pop();
                        }
                        break;
                    }

                    case XText xText:
                    {
                        Vectorize(textGeometry, xText.Value, position, beginOfLine, hasSuccessor, typeface, fontSize, rotation, currentTransformationMatrix);
                        break;
                    }
                }

                beginOfLine = false;
                node = nextNode;
            }

            brushParser.SetFillAndStroke(element, textGraphicPath, currentTransformationMatrix);

            if (adjustments.Count > 0)
            {
                var gc2 = new GradientTSpanAdjustment
                {
                    AdjustFill = true,
                    AdjustStroke = true,
                    Path = textGraphicPath
                };
                adjustments.Add(gc2);

                AdjustGradients(adjustments);
            }

            if (clipping.IsClipPathSet())
            {
                // shapes don't support clipping, create a group around it if none exists

                if (graphicGroup == null)
                {
                    graphicGroup = new GraphicGroup();
                    graphicVisual = graphicGroup;
                    graphicGroup.Children.Add(graphicVisual);
                }

                clipping.SetClipPath(graphicGroup, currentTransformationMatrix);
            }

            return graphicVisual;
        }

        /// <summary>
        /// Adjust the gradients so that all touch the bounds of the overall text
        /// </summary>
        private void AdjustGradients(List<GradientTSpanAdjustment> adjustments)
        {
            Rect textBounds = Rect.Empty;

            void Adjust(GraphicBrush graphicBrush, Rect bounds)
            {
                Point Interpolate(Point pointIn)
                {
                    var xh = pointIn.X * (textBounds.Right - textBounds.Left) + textBounds.Left;
                    var x2 = (xh - bounds.Left) / (bounds.Right - bounds.Left);

                    var yh = pointIn.Y * (textBounds.Bottom- textBounds.Top) + textBounds.Top;
                    var y2 = (yh - bounds.Top) / (bounds.Bottom - bounds.Top);

                    return new Point(x2, y2);
                }

                if (graphicBrush == null)
                {
                    return;
                }

                switch (graphicBrush)
                {
                    case GraphicLinearGradientBrush linearGradientBrush:
                    {
                        if (linearGradientBrush.MappingMode == GraphicBrushMappingMode.RelativeToBoundingBox)
                        {
                            linearGradientBrush.StartPoint = Interpolate(linearGradientBrush.StartPoint);
                            linearGradientBrush.EndPoint = Interpolate(linearGradientBrush.EndPoint);
                        }
                        break;
                    }

                    case GraphicRadialGradientBrush radialGradientBrush:
                    {
                        if (radialGradientBrush.MappingMode == GraphicBrushMappingMode.RelativeToBoundingBox)
                        {
                            radialGradientBrush.StartPoint = Interpolate(radialGradientBrush.StartPoint);
                            radialGradientBrush.EndPoint = Interpolate(radialGradientBrush.EndPoint);

                            radialGradientBrush.RadiusX = radialGradientBrush.RadiusX * (textBounds.Right - textBounds.Left) / (bounds.Right - bounds.Left);
                        }
                        break;
                    }
                }
            }

            foreach (var adjustment in adjustments)
            {
                var bounds = adjustment.Path.Geometry.Bounds;
                textBounds = Rect.Union(textBounds, bounds);
            }

            foreach (var adjustment in adjustments)
            {
                var path = adjustment.Path;

                if (adjustment.AdjustFill)
                {
                    Adjust(path.FillBrush, path.Geometry.Bounds);
                }

                if (adjustment.AdjustStroke)
                {
                    Adjust(path.StrokeBrush, path.Geometry.Bounds);
                }
            }
        }

        /// <summary>
        /// Test if a given attribute exists on top of the cascade
        /// </summary>
        private bool ExistsAttributeOnTop(string attrName)
        {
            var attr = cssStyleCascade.GetPropertyFromTop(attrName);

            return !string.IsNullOrEmpty(attr);
        }

        /// <summary>
        /// Test if a given attribute is set at the given element
        /// </summary>
        private bool ExistsAttributeOnElement(XElement element, string attrName)
        {
            XAttribute xAttr = element.Attribute(attrName);

            return xAttr != null;
        }
    }
}
