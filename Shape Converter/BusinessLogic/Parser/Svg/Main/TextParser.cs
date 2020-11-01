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

            var x = doubleParser.GetLengthPercent(element, "x", 0.0, PercentBaseSelector.ViewBoxWidth);
            var y = doubleParser.GetLengthPercent(element, "y", 0.0, PercentBaseSelector.ViewBoxHeight);

            var fontSize = GetFontSize();
            var typeface = GetTypeface();
            var (_, rotation) = GetRotate(element);
            int rotationIndex = 0;

            var textGeometry = new GraphicPathGeometry();
            textGeometry.FillRule = GraphicFillRule.NoneZero;
            var textGraphicPath = new GraphicPath();
            textGraphicPath.Geometry = textGeometry;
            graphicVisual = textGraphicPath;

            var textStartX = x;
            var adjustments = new List<GradientTSpanAdjustment>();

            XNode node = element.FirstNode;
            bool prefixBlank = false;

            while (node != null)
            {
                switch (node)
                {
                    case XElement xElement:
                    {
                        if (xElement.Name.LocalName == "tspan")
                        {
                            cssStyleCascade.PushStyles(xElement);

                            var hasOwnFill = ExistsAttributeOnTop("fill");
                            var hasOwnStroke = ExistsAttributeOnTop("stroke");
                            var addNewGeometry = hasOwnFill || hasOwnStroke;
                            GraphicPathGeometry tspanGeometry;
                            double startX = x;

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
                            bool hasRotation;
                            List<double> tspanRotation;

                            (hasRotation, tspanRotation) = GetRotate(xElement);

                            if (!hasRotation)
                            {
                                Vectorize(tspanGeometry, xElement.Value, ref x, y, prefixBlank, tspanTypeface, tspanFontSize, rotation, ref rotationIndex, currentTransformationMatrix);
                            }
                            else
                            {
                                int rotationIndex2 = 0;
                                rotationIndex += Vectorize(tspanGeometry, xElement.Value, ref x, y, prefixBlank, tspanTypeface, tspanFontSize, tspanRotation, ref rotationIndex2, currentTransformationMatrix);

                                if (rotationIndex >= rotation.Count)
                                {
                                    rotationIndex = rotation.Count - 1;
                                }
                            }

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
                                    EndX = x,
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
                        Vectorize(textGeometry, xText.Value, ref x, y, prefixBlank, typeface, fontSize, rotation, ref rotationIndex, currentTransformationMatrix);
                        break;
                    }
                }

                prefixBlank = true;
                node = node.NextNode;
            }

            brushParser.SetFillAndStroke(element, textGraphicPath, currentTransformationMatrix);
            AdjustTSpanGradients(adjustments, textStartX, x);

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
        /// Adjust the tspan gradients so that they lie as best on top of the base text gradient
        /// </summary>
        private void AdjustTSpanGradients(List<GradientTSpanAdjustment> adjustments, double globalStartX, double globalEndX)
        {
            void Adjust(GraphicBrush graphicBrush, double startX, double endX)
            {
                double Interpolate(double x)
                {
                    var t = x * (globalEndX - globalStartX) + globalStartX;
                    var y = (t - startX) / (endX - startX);
                    return y;
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
                            var newStartX = Interpolate(linearGradientBrush.StartPoint.X);
                            var newEndX = Interpolate(linearGradientBrush.EndPoint.X);

                            linearGradientBrush.StartPoint = new Point(newStartX, linearGradientBrush.StartPoint.Y);
                            linearGradientBrush.EndPoint = new Point(newEndX, linearGradientBrush.EndPoint.Y);
                        }
                        break;
                    }

                    case GraphicRadialGradientBrush radialGradientBrush:
                    {
                        if (radialGradientBrush.MappingMode == GraphicBrushMappingMode.RelativeToBoundingBox)
                        {
                            var newStartX = Interpolate(radialGradientBrush.StartPoint.X);
                            var newEndX = Interpolate(radialGradientBrush.EndPoint.X);

                            radialGradientBrush.StartPoint = new Point(newStartX, radialGradientBrush.StartPoint.Y);
                            radialGradientBrush.EndPoint = new Point(newEndX, radialGradientBrush.EndPoint.Y);

                            radialGradientBrush.RadiusX = radialGradientBrush.RadiusX * (globalEndX - globalStartX) / (endX - startX);
                        }
                        break;
                    }
                }
            }

            foreach (var adjustment in adjustments)
            {
                if (adjustment.AdjustFill)
                {
                    Adjust(adjustment.Path.FillBrush, adjustment.StartX, adjustment.EndX);
                }

                if (adjustment.AdjustStroke)
                {
                    Adjust(adjustment.Path.StrokeBrush, adjustment.StartX, adjustment.EndX);
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
