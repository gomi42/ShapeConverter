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

            //var geo = ParseTextGeometry(element, currentTransformationMatrix);

            //var bgGraphicPath = new GraphicPath();
            //bgGraphicPath.Geometry = geo;

            //graphicGroup = new GraphicGroup();
            //graphicGroup.Children.Add(bgGraphicPath);
            //graphicGroup.Children.Add(textGraphicPath);
            //graphicVisual = graphicGroup;

            //brushParser.SetFill(element, bgGraphicPath, currentTransformationMatrix);

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

                            var addNewGeometry = AttributeExistsOnTop("fill") || AttributeExistsOnTop("stroke");
                            GraphicPathGeometry tspanGeometry;

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
        /// Test if a given attribute exists on top of the cascade
        /// </summary>
        private bool AttributeExistsOnTop(string attrName)
        {
            var attr = cssStyleCascade.GetPropertyFromTop(attrName);

            return !string.IsNullOrEmpty(attr);
        }
    }
}
