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
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Xml.Linq;
using ShapeConverter.BusinessLogic.Generators;
using ShapeConverter.BusinessLogic.Helper;
using ShapeConverter.BusinessLogic.Parser.Svg.Helper;

namespace ShapeConverter.BusinessLogic.Parser.Svg.Main
{
    /// <summary>
    /// The text parser
    /// </summary>
    internal class TextParser : GeometryTextParser
    {
        /// <summary>
        /// a color block descriptor
        /// </summary>
        private class ColorBlockDescriptor
        {
            public bool AdjustFillGlobal;
            public bool AdjustStrokeGlobal;
            public List<PositionBlockCharacter> Characters;
            public bool HasParentOpacity;
            public double Opacity;
            public double FillOpacity;
            public double StrokeOpacity;
            public XElement Element;
        }

        /// <summary>
        /// a GraphicPath color block
        /// </summary>
        private class GraphicPathColorBlock
        {
            public bool AdjustFillGlobal;
            public bool AdjustStrokeGlobal;
            public List<GraphicPath> Paths = new List<GraphicPath>();
        }

        private Clipping clipping;
        private BrushParser brushParser;

        /// <summary>
        /// Constructor
        /// </summary>
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
        private GraphicVisual ParseText(XElement textElement,
                              Matrix currentTransformationMatrix)
        {
            GraphicGroup graphicGroup = new GraphicGroup();


            var colorBlocks = new List<ColorBlockDescriptor>();
            var positionBlocks = new List<PositionBlock>();
            var position = new CharacterPositions();

            var textColorBlock = new ColorBlockDescriptor()
            {
                Characters = new List<PositionBlockCharacter>(),
                AdjustFillGlobal = true,
                AdjustStrokeGlobal = true,
                HasParentOpacity = false,
                Element = textElement
            };
            colorBlocks.Add(textColorBlock);

            var xList = GetLengthPercentList(textElement, "x", PercentBaseSelector.ViewBoxWidth);
            var dxList = GetLengthPercentList(textElement, "dx", PercentBaseSelector.ViewBoxWidth);
            position.X.SetParentValues(xList, dxList);

            var yList = GetLengthPercentList(textElement, "y", PercentBaseSelector.ViewBoxHeight);
            var dyList = GetLengthPercentList(textElement, "dy", PercentBaseSelector.ViewBoxHeight);
            position.Y.SetParentValues(yList, dyList);

            var fontSize = GetFontSize();
            var typeface = GetTypeface();
            var textAnchor = GetTextAnchor();

            var rotation = new ParentChildPriorityList();
            rotation.ParentValues = GetRotate(textElement);

            var textOpacity = cssStyleCascade.GetNumberPercentFromTop("opacity", 1);
            var textFillOpacity = cssStyleCascade.GetNumberPercentFromTop("fill-opacity", 1);
            var textStrokeOpacity = cssStyleCascade.GetNumberPercentFromTop("stroke-opacity", 1);

            if (false &&!textElement.HasElements)
            {
                var geometry = ParseTextGeometry(textElement, currentTransformationMatrix);

                var graphicPath = new GraphicPath();
                graphicPath.Geometry = geometry;
                graphicPath.Geometry.FillRule = GraphicFillRule.NoneZero;

                brushParser.SetFillAndStroke(textElement, graphicPath, currentTransformationMatrix);

                return graphicPath;
            }

            XNode node = textElement.FirstNode;
            bool beginOfLine = true;

            while (node != null)
            {
                var nextNode = node.NextNode;
                var hasSuccessor = nextNode != null;

                switch (node)
                {
                    case XElement tspanElement:
                    {
                        if (tspanElement.Name.LocalName == "tspan" && PresentationAttribute.IsElementVisible(tspanElement))
                        {
                            var isTspanDisplayed = PresentationAttribute.IsElementDisplayed(tspanElement);

                            cssStyleCascade.PushStyles(tspanElement);

                            var xChildList = GetLengthPercentList(tspanElement, "x", PercentBaseSelector.ViewBoxWidth);
                            var dxChildList = GetLengthPercentList(tspanElement, "dx", PercentBaseSelector.ViewBoxWidth);
                            position.X.SetChildValues(xChildList, dxChildList);

                            var yChildList = GetLengthPercentList(tspanElement, "y", PercentBaseSelector.ViewBoxHeight);
                            var dyChildList = GetLengthPercentList(tspanElement, "dy", PercentBaseSelector.ViewBoxHeight);
                            position.Y.SetChildValues(yChildList, dyChildList);

                            var hasOwnFill = ExistsAttributeOnTop("fill");
                            var hasOwnStroke = ExistsAttributeOnTop("stroke");

                            var tspanFontSize = GetFontSize();
                            var tspanTypeface = GetTypeface();
                            var tspanAnchor = GetTextAnchor();
                            rotation.ChildValues = GetRotate(tspanElement);

                            var charBlock = Vectorize(positionBlocks,
                                                      tspanElement.Value,
                                                      tspanAnchor,
                                                      position,
                                                      beginOfLine,
                                                      hasSuccessor,
                                                      tspanTypeface,
                                                      tspanFontSize,
                                                      rotation,
                                                      currentTransformationMatrix);

                            rotation.ChildValues = null;
                            position.X.SetChildValues(null, null);
                            position.Y.SetChildValues(null, null);

                            if (isTspanDisplayed)
                            {
                                ColorBlockDescriptor colorBlock;

                                if (hasOwnFill || hasOwnStroke)
                                {
                                    colorBlock = new ColorBlockDescriptor()
                                    {
                                        Characters = charBlock,
                                        AdjustFillGlobal = !hasOwnFill,
                                        AdjustStrokeGlobal = !hasOwnStroke,
                                        HasParentOpacity = true,
                                        Opacity = textOpacity,
                                        FillOpacity = textFillOpacity,
                                        StrokeOpacity = textStrokeOpacity,
                                        Element = tspanElement
                                    };
                                    colorBlocks.Add(colorBlock);
                                }
                                else
                                {
                                    textColorBlock.Characters.AddRange(charBlock);
                                }
                            }

                            cssStyleCascade.Pop();
                        }
                        break;
                    }

                    case XText textContentElement:
                    {
                        var charBlock = Vectorize(positionBlocks,
                                                  textContentElement.Value,
                                                  textAnchor,
                                                  position,
                                                  beginOfLine,
                                                  hasSuccessor,
                                                  typeface,
                                                  fontSize,
                                                  rotation,
                                                  currentTransformationMatrix);
                        textColorBlock.Characters.AddRange(charBlock);
                        break;
                    }
                }

                beginOfLine = false;
                node = nextNode;
            }

            AdjustPosition(positionBlocks);
            var graphicPathColorBlocks = CreateGraphicPathColorBlocks(colorBlocks, currentTransformationMatrix);
            AdjustGradients(graphicPathColorBlocks);

            foreach (var block in graphicPathColorBlocks)
            {
                graphicGroup.Children.AddRange(block.Paths);
            }

            if (clipping.IsClipPathSet())
            {
                clipping.SetClipPath(graphicGroup, currentTransformationMatrix);
            }

            return graphicGroup;
        }

        /// <summary>
        /// Create GraphicPaths out of the description in a ColorBlock
        /// </summary>
        private List<GraphicPathColorBlock> CreateGraphicPathColorBlocks(List<ColorBlockDescriptor> colorBlocks, Matrix matrix)
        {
            var pathColorBlocks = new List<GraphicPathColorBlock>();

            foreach (var colorBlock in colorBlocks)
            {
                var pathColorBlock = new GraphicPathColorBlock()
                {
                    AdjustFillGlobal = colorBlock.AdjustFillGlobal,
                    AdjustStrokeGlobal = colorBlock.AdjustStrokeGlobal
                };
                pathColorBlocks.Add(pathColorBlock);

                foreach (var ch in colorBlock.Characters)
                {
                    var graphicPath = new GraphicPath()
                    {
                        Geometry = ch.Character
                    };
                    pathColorBlock.Paths.Add(graphicPath);

                    if (colorBlock.HasParentOpacity)
                    {
                        brushParser.SetFillAndStroke(colorBlock.Element, graphicPath, matrix, colorBlock.Opacity, colorBlock.FillOpacity, colorBlock.StrokeOpacity);
                    }
                    else
                    {
                        brushParser.SetFillAndStroke(colorBlock.Element, graphicPath, matrix);
                    }
                }
            }

            return pathColorBlocks;
        }

        /// <summary>
        /// Adjust the gradients so that all touch the bounds of the block they belong to
        /// </summary>
        private void AdjustGradients(List<GraphicPathColorBlock> pathColorBlocks)
        {
            Rect textBounds;

            // adjust a single brush
            void Adjust(GraphicBrush graphicBrush, Rect bounds, Rect blockBounds)
            {
                Point Interpolate(Point pointIn)
                {
                    var xh = pointIn.X * (blockBounds.Right - blockBounds.Left) + blockBounds.Left;
                    var x2 = (xh - bounds.Left) / (bounds.Right - bounds.Left);

                    var yh = pointIn.Y * (blockBounds.Bottom - blockBounds.Top) + blockBounds.Top;
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

                            radialGradientBrush.RadiusX = radialGradientBrush.RadiusX * (blockBounds.Right - blockBounds.Left) / (bounds.Right - bounds.Left);
                        }
                        break;
                    }
                }
            }

            // get the bounds of the given block
            Rect GetBlockBounds(GraphicPathColorBlock block)
            {
                var blockBounds = Rect.Empty;

                foreach (var ch in block.Paths)
                {
                    var bounds = ch.Geometry.Bounds;
                    blockBounds = Rect.Union(blockBounds, bounds);
                }

                return blockBounds;
            }

            // adjust a single block
            void AdjustOneBlock(GraphicPathColorBlock block)
            {
                var blockBounds = GetBlockBounds(block);

                foreach (var ch in block.Paths)
                {
                    Rect bounds;

                    if (block.AdjustFillGlobal)
                    {
                        bounds = textBounds;
                    }
                    else
                    {
                        bounds = blockBounds;
                    }

                    Adjust(ch.FillBrush, ch.Geometry.Bounds, bounds);

                    if (block.AdjustStrokeGlobal)
                    {
                        bounds = textBounds;
                    }
                    else
                    {
                        bounds = blockBounds;
                    }

                    Adjust(ch.StrokeBrush, ch.Geometry.Bounds, bounds);
                }
            }

            // set the dimensions of the overall text block
            void SetTextBounds()
            {
                textBounds = Rect.Empty;

                foreach (var block in pathColorBlocks)
                {
                    var blockBounds = GetBlockBounds(block);
                    textBounds = Rect.Union(textBounds, blockBounds);
                }
            }

            SetTextBounds();

            foreach (var block in pathColorBlocks)
            {
                AdjustOneBlock(block);
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
    }
}
