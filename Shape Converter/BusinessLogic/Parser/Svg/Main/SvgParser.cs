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
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Xml.Linq;
using EpsSharp.Eps.Commands.Path;
using ShapeConverter.BusinessLogic.Generators;
using ShapeConverter.BusinessLogic.Helper;
using ShapeConverter.BusinessLogic.Parser.Svg.Helper;
using ShapeConverter.BusinessLogic.Parser.Svg.Main;
using ShapeConverter.Parser;

namespace ShapeConverter.BusinessLogic.Parser.Svg
{
    /// <summary>
    /// The SVG parser
    /// </summary>
    internal class SvgParser : IFileParser
    {
        private ShapeParser shapeParser;
        private TextParser textParser;
        private Clipping clipping;
        private CssStyleCascade cssStyleCascade;
        private Dictionary<string, XElement> globalDefinitions;
        private DoubleParser doubleParser;

        /// <summary>
        /// Parse the given file
        /// </summary>
        GraphicVisual IFileParser.Parse(string filename)
        {
            GraphicVisual visual = null;

            try
            {
                var root = XElement.Load(new Uri(filename).ToString());
                visual = ParseRoot(root);
            }
#pragma warning disable CS0168 // Variable is declared but never used
            catch (Exception e)
#pragma warning restore CS0168 // Variable is declared but never used
            {
            }

            return visual;
        }

        /// <summary>
        /// Parse an SVG given as XElement root
        /// </summary>
        public GraphicVisual ParseRoot(XElement root)
        {
            var nameSpaceAttributes = root.Attributes().Where(a => a.IsNamespaceDeclaration);
            var defaultNamespaceAttribute = root.Attributes().Where(a => a.IsNamespaceDeclaration && a.Name.Namespace == XNamespace.None).FirstOrDefault();
            XNamespace defaultNamespace = defaultNamespaceAttribute.Value;

            Matrix currentTransformationMatrix = Matrix.Identity;
            cssStyleCascade = new CssStyleCascade(root);

            var svgViewBox = new SvgViewBox
            {
                ViewBox = new Rect(0, 0, 100, 100),
                Align = "none",
                Slice = "meet"
            };
            cssStyleCascade.PushViewBox(svgViewBox);

            doubleParser = new DoubleParser(cssStyleCascade);
            ReadGlobalDefinitions(root);

            var brushParser = new BrushParser(defaultNamespace, cssStyleCascade, globalDefinitions, doubleParser);
            var geometryParser = new GeometryParser(doubleParser);

            var geometryTextParser = new GeometryTextParser(cssStyleCascade, doubleParser);
            clipping = new Clipping(cssStyleCascade, globalDefinitions, geometryParser, geometryTextParser);
            shapeParser = new ShapeParser(cssStyleCascade, brushParser, geometryParser, clipping);
            textParser = new TextParser(cssStyleCascade, doubleParser, brushParser, clipping);

            GraphicVisual visual = ParseSVG(root, currentTransformationMatrix, true);
            visual = OptimizeVisual.Optimize(visual);

            return visual;
        }

        /// <summary>
        /// Parse an SVG document fragment
        /// </summary>
        private GraphicGroup ParseSVG(XElement element, Matrix matrix, bool isTopLevelSvg)
        {
            cssStyleCascade.PushStyles(element);

            var (newViewBoxPushOnStack, viewBoxMatrix) = GetViewBoxMatrix(element, isTopLevelSvg);
            matrix = viewBoxMatrix * matrix;

            var elementTransformationMatrix = cssStyleCascade.GetTransformMatrixFromTop();
            elementTransformationMatrix = elementTransformationMatrix * matrix;

            var group = ParseChildren(element, elementTransformationMatrix);

            group.Opacity = cssStyleCascade.GetNumberPercentFromTop("opacity", 1);
            clipping.SetClipPath(group, elementTransformationMatrix);

            if (newViewBoxPushOnStack)
            {
                cssStyleCascade.PopViewBox();
            }

            cssStyleCascade.Pop();
            return group;
        }

        /// <summary>
        /// Parse an g container
        /// </summary>
        private GraphicGroup ParseGContainer(XElement element, Matrix matrix)
        {
            cssStyleCascade.PushStyles(element);

            var elementTransformationMatrix = cssStyleCascade.GetTransformMatrixFromTop();
            elementTransformationMatrix = elementTransformationMatrix * matrix;

            var group = ParseChildren(element, elementTransformationMatrix);

            group.Opacity = cssStyleCascade.GetNumberPercentFromTop("opacity", 1);
            clipping.SetClipPath(group, elementTransformationMatrix);

            cssStyleCascade.Pop();
            return group;
        }

        /// <summary>
        /// Parse all graphic elements
        /// </summary>
        private GraphicGroup ParseChildren(XElement groupElement, Matrix matrix)
        {
            var group = new GraphicGroup();

            foreach (var element in groupElement.Elements())
            {
                if (!PresentationAttribute.IsElementVisible(element) || !PresentationAttribute.IsElementDisplayed(element))
                {
                    continue;
                }

                GraphicVisual graphicVisual = ParseElement(element, matrix);

                if (graphicVisual != null)
                {
                    group.Children.Add(graphicVisual);
                }
            }

            return group;
        }

        private GraphicVisual ParseElement(XElement element, Matrix matrix)
        {
            GraphicVisual graphicVisual = null;

            switch (element.Name.LocalName)
            {
                case "defs":
                case "style":
                {
                    // already read, ignore
                    break;
                }

                case "svg":
                {
                    graphicVisual = ParseSVG(element, matrix, false);
                    break;
                }

                case "g":
                {
                    graphicVisual = ParseGContainer(element, matrix);
                    break;
                }

                case "text":
                {
                    graphicVisual = textParser.Parse(element, matrix);
                    break;
                }

                case "use":
                {
                    graphicVisual = ParseUseElement(element, matrix);
                    break;
                }

                default:
                {
                    graphicVisual = shapeParser.Parse(element, matrix);
                    break;
                }
            }

            return graphicVisual;
        }

        private readonly XNamespace xlink = "http://www.w3.org/1999/xlink";

        /// <summary>
        /// Handle the use element
        /// poor man's implementation of the use element, it doesn't cover all the style rules
        /// (which one takes precidence) but it is a first version.
        /// </summary>
        private GraphicVisual ParseUseElement(XElement element, Matrix currentMatrix)
        {
            void CopyAttr(XElement sourceElement, XElement destElement, string attrName)
            {
                var sourceAttr = sourceElement.Attribute(attrName);

                if (sourceAttr == null)
                {
                    return;
                }

                destElement.SetAttributeValue(attrName, sourceAttr.Value);
            }

            var hrefAttr = element.Attribute("href");

            if (hrefAttr == null)
            {
                hrefAttr = element.Attribute(xlink + "href");

                if (hrefAttr == null)
                {
                    return null;
                }
            }

            var id = hrefAttr.Value;

            if (!id.StartsWith("#", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            id = id.Substring(1, id.Length - 1);

            if (!globalDefinitions.ContainsKey(id))
            {
                return null;
            }

            var referencedElem = globalDefinitions[id];

            var x = doubleParser.GetLengthPercent(element, "x", 0.0, PercentBaseSelector.ViewBoxWidth);
            var y = doubleParser.GetLengthPercent(element, "y", 0.0, PercentBaseSelector.ViewBoxHeight);

            Matrix matrix = Matrix.Identity;
            matrix.Translate(x, y);

            var clone = referencedElem;

            if (referencedElem.Name.LocalName == "svg")
            {
                clone = new XElement(referencedElem);
                CopyAttr(referencedElem, clone, "width");
                CopyAttr(referencedElem, clone, "height");
            }

            cssStyleCascade.PushStyles(element);
            var visual = ParseElement(clone, matrix * currentMatrix);
            cssStyleCascade.Pop();

            return visual;
        }

        /// <summary>
        /// Get the viewbox transformation matrix
        /// </summary>
        private (bool, Matrix) GetViewBoxMatrix(XElement element, bool isTopLevel)
        {
            var viewPort = GetViewPort(element, isTopLevel);
            var viewBox = GetViewBox(element);

            if (viewPort.IsEmpty && viewBox.IsEmpty)
            {
                return (false, Matrix.Identity);
            }

            Matrix matrix;
            bool newViewBoxPushOnStack = false;
            string align;
            string slice;

            if (viewPort.IsEmpty)
            {
                var svgViewBox = cssStyleCascade.GetCurrentViewBox();
                viewPort = new Rect(0, 0, svgViewBox.ViewBox.Width, svgViewBox.ViewBox.Height);
            }

            if (viewBox.IsEmpty)
            {
                var svgViewBox = cssStyleCascade.GetCurrentViewBox();
                viewBox = svgViewBox.ViewBox;
                align = svgViewBox.Align;
                slice = svgViewBox.Slice;
            }
            else
            {
                (align, slice) = GetAlignSlice(element);

                var svgViewBox = new SvgViewBox
                {
                    ViewBox = viewBox,
                    Align = align,
                    Slice = slice
                };
                cssStyleCascade.PushViewBox(svgViewBox);

                newViewBoxPushOnStack = true;
            }

            var scaleX = viewPort.Width / viewBox.Width;
            var scaleY = viewPort.Height / viewBox.Height;

            matrix = Matrix.Identity;

            if (align == "none")
            {
                matrix.Translate(-viewBox.X, -viewBox.Y);
                matrix.Scale(scaleX, scaleY);
                matrix.Translate(viewPort.X, viewPort.Y);
            }
            else
            {
                switch (slice)
                {
                    case "meet":
                        if (scaleX < scaleY)
                        {
                            scaleY = scaleX;
                        }
                        else
                        {
                            scaleX = scaleY;
                        }
                        break;

                    case "slice":
                        if (scaleX > scaleY)
                        {
                            scaleY = scaleX;
                        }
                        else
                        {
                            scaleX = scaleY;
                        }
                        break;

                    default:
                        throw new ArgumentException("invalid argument");
                }

                var xOperation = align.Substring(0, 4);
                var yOperation = align.Substring(4, 4);

                double translateX;
                double translateY;

                switch (xOperation)
                {
                    case "xmid":
                        translateX = viewBox.Width / 2;
                        break;

                    case "xmax":
                        translateX = viewBox.Width;
                        break;

                    default:
                        throw new ArgumentException("invalid argument");
                }

                switch (yOperation)
                {
                    case "ymid":
                        translateY = viewBox.Height / 2;
                        break;

                    case "ymax":
                        translateY = viewBox.Height;
                        break;

                    default:
                        throw new ArgumentException("invalid argument");
                }

                matrix.Translate(-viewBox.X - translateX, -viewBox.Y - translateY);
                matrix.Scale(scaleX, scaleY);
                matrix.Translate(viewPort.X + viewPort.Width / 2, viewPort.Y + viewPort.Height / 2);
            }

            return (newViewBoxPushOnStack, matrix);
        }

        /// <summary>
        /// Get the viewport definition
        /// </summary>
        private Rect GetViewPort(XElement element, bool isTopLevel)
        {
            double x;
            double y;

            if (isTopLevel)
            {
                x = 0.0;
                y = 0.0;
            }
            else
            {
                x = doubleParser.GetLengthPercent(element, "x", 0.0, PercentBaseSelector.ViewBoxWidth);
                y = doubleParser.GetLengthPercent(element, "y", 0.0, PercentBaseSelector.ViewBoxHeight);
            }

            var widthLPA = doubleParser.GetLengthPercentAuto(element, "width", PercentBaseSelector.ViewBoxWidth);
            var heightLPA = doubleParser.GetLengthPercentAuto(element, "height", PercentBaseSelector.ViewBoxHeight);

            if (DoubleUtilities.IsZero(x)
                && DoubleUtilities.IsZero(y)
                && widthLPA.IsAuto
                && heightLPA.IsAuto)
            {
                return Rect.Empty;
            }

            double width;
            double height;

            var svgViewBox = cssStyleCascade.GetCurrentViewBox().ViewBox;

            if (widthLPA.IsAuto)
            {
                width = svgViewBox.Width;
            }
            else
            {
                width = widthLPA.Value;
            }

            if (heightLPA.IsAuto)
            {
                height = svgViewBox.Height;
            }
            else
            {
                height = heightLPA.Value;
            }

            return new Rect(new Point(x, y), new Size(width, height));
        }

        /// <summary>
        /// Get the viewbox definition
        /// </summary>
        private Rect GetViewBox(XElement element)
        {
            var viewBoxAttr = element.Attribute("viewBox");

            if (viewBoxAttr == null)
            {
                return Rect.Empty;
            }

            var viewBox = doubleParser.GetNumberList(viewBoxAttr.Value);

            return new Rect(new Point(viewBox[0], viewBox[1]), new Size(viewBox[2], viewBox[3]));
        }

        /// <summary>
        /// Get the align and slice parameter
        /// </summary>
        private (string, string) GetAlignSlice(XElement element)
        {
            string align;
            string slice;
            var preserveAspectRatioAttr = element.Attribute("preserveAspectRatio");

            if (preserveAspectRatioAttr != null)
            {
                var list = preserveAspectRatioAttr.Value.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                align = list[0].ToLower();

                if (list.Length > 1)
                {
                    slice = list[1];
                }
                else
                {
                    slice = "meet";
                }
            }
            else
            {
                align = "xmidymid";
                slice = "meet";
            }

            return (align, slice);
        }

        /// <summary>
        /// First pass: read all global data of the svg
        /// </summary>
        private void ReadGlobalDefinitions(XElement element)
        {
            void ReadChildGlobalDefinitions(XElement childElement)
            {
                var idAttr = childElement.Attribute("id");

                if (idAttr != null)
                {
                    globalDefinitions[idAttr.Value] = childElement;
                }

                foreach (var child in childElement.Elements())
                {
                    ReadChildGlobalDefinitions(child);
                }
            }

            globalDefinitions = new Dictionary<string, XElement>();
            ReadChildGlobalDefinitions(element);
        }
    }
}
