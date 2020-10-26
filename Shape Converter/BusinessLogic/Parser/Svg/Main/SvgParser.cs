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
using ShapeConverter.BusinessLogic.Generators;
using ShapeConverter.BusinessLogic.Helper;
using ShapeConverter.BusinessLogic.Parser.Svg.Helper;
using ShapeConverter.BusinessLogic.Parser.Svg.Main;
using ShapeConverter.Parser;

namespace ShapeConverter.BusinessLogic.Parser.Svg
{
    internal class ViewBoxSize
    {
        public double Width { get; set; }
        public double Height { get; set; }

        public double Diagonal
        {
            get
            {
                return Math.Sqrt(Width * Width + Height * Height);
            }
        }
    }

    /// <summary>
    /// The SVG parser
    /// </summary>
    internal class SvgParser : IFileParser
    {
        private class SvgViewBox
        {
            public Rect ViewBox;
            public string Align;
            public string Slice;
        }

        private ShapeParser shapeParser;
        private Clipping clipping;
        private Stack<SvgViewBox> svgViewBoxStack;
        private CssStyleCascade cssStyleCascade;
        private Dictionary<string, XElement> globalDefinitions;

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

            svgViewBoxStack = new Stack<SvgViewBox>();
            var svgViewBox = new SvgViewBox
            {
                ViewBox = new Rect(0, 0, 100, 100),
                Align = "none",
                Slice = "meet"
            };
            svgViewBoxStack.Push(svgViewBox);

            ReadGlobalDefinitions(root);
            shapeParser = new ShapeParser(defaultNamespace, cssStyleCascade, globalDefinitions);
            clipping = new Clipping(cssStyleCascade, globalDefinitions);

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

            var group = ParseGroupChildren(element, elementTransformationMatrix);

            group.Opacity = cssStyleCascade.GetNumberPercentFromTop("opacity", 1);
            clipping.SetClipPath(group, elementTransformationMatrix, GetCurrentViewBoxSize());

            if (newViewBoxPushOnStack)
            {
                svgViewBoxStack.Pop();
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

            var group = ParseGroupChildren(element, elementTransformationMatrix);

            group.Opacity = cssStyleCascade.GetNumberPercentFromTop("opacity", 1);
            clipping.SetClipPath(group, elementTransformationMatrix, GetCurrentViewBoxSize());

            cssStyleCascade.Pop();
            return group;
        }

        /// <summary>
        /// Parse all graphic elements
        /// </summary>
        private GraphicGroup ParseGroupChildren(XElement groupElement, Matrix matrix)
        {
            var group = new GraphicGroup();

            foreach (var element in groupElement.Elements())
            {
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
                        var childGroup = ParseSVG(element, matrix, false);
                        group.Childreen.Add(childGroup);
                        break;
                    }

                    case "g":
                    {
                        var childGroup = ParseGContainer(element, matrix);
                        group.Childreen.Add(childGroup);
                        break;
                    }

                    default:
                    {
                        var shape = shapeParser.Parse(element, matrix, GetCurrentViewBoxSize());

                        if (shape != null)
                        {
                            group.Childreen.Add(shape);
                        }
                        break;
                    }
                }
            }

            return group;
        }

        /// <summary>
        /// Return the current viewbox size
        /// </summary>
        private ViewBoxSize GetCurrentViewBoxSize()
        {
            var svgViewBox = svgViewBoxStack.Peek();
            return new ViewBoxSize
            {
                Width = svgViewBox.ViewBox.Width,
                Height = svgViewBox.ViewBox.Height
            };
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
                var svgViewBox = svgViewBoxStack.Peek();
                viewPort = new Rect(0, 0, svgViewBox.ViewBox.Width, svgViewBox.ViewBox.Height);
            }

            if (viewBox.IsEmpty)
            {
                var svgViewBox = svgViewBoxStack.Peek();
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
                svgViewBoxStack.Push(svgViewBox);

                newViewBoxPushOnStack = true;
            }

            var scaleX = viewPort.Width / viewBox.Width;
            var scaleY = viewPort.Height / viewBox.Height;

            if (align != "none")
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
            }

            matrix = Matrix.Identity;

            if (align == "none")
            {
                matrix.Translate(-viewBox.X, -viewBox.Y);
                matrix.Scale(scaleX, scaleY);
                matrix.Translate(viewPort.X, viewPort.Y);
            }
            else
            {
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
            bool isXPercent;
            bool isYPercent;
            double xLP;
            double yLP;

            if (isTopLevel)
            {
                isXPercent = false;
                isYPercent = false;
                xLP = 0.0;
                yLP = 0.0;
            }
            else
            {
                (isXPercent, xLP) = DoubleAttributeParser.GetLengthPercent(element, "x", 0.0);
                (isYPercent, yLP) = DoubleAttributeParser.GetLengthPercent(element, "y", 0.0);
            }

            var widthLPA = DoubleAttributeParser.GetLengthPercentAuto(element, "width");
            var heightLPA = DoubleAttributeParser.GetLengthPercentAuto(element, "height");

            if (DoubleUtilities.IsZero(xLP)
                && DoubleUtilities.IsZero(yLP)
                && widthLPA.IsAuto
                && heightLPA.IsAuto)
            {
                return Rect.Empty;
            }

            double x;
            double y;
            double width;
            double height;

            var svgViewBox = svgViewBoxStack.Peek().ViewBox;

            if (isXPercent)
            {
                x = svgViewBox.Width * xLP;
            }
            else
            {
                x = xLP;
            }

            if (isYPercent)
            {
                y = svgViewBox.Height * yLP;
            }
            else
            {
                y = yLP;
            }

            if (widthLPA.IsAuto)
            {
                width = svgViewBox.Width;
            }
            else
            if (widthLPA.IsPercentage)
            {
                width = svgViewBox.Width * widthLPA.Value;
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
            if (widthLPA.IsPercentage)
            {
                height = svgViewBox.Height * heightLPA.Value;
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

            var parser = new DoubleListParser();
            var viewBox = parser.ParseDoubleList(viewBoxAttr.Value);

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
            foreach (var child in element.Elements())
            {
                switch (child.Name.LocalName)
                {
                    case "defs":
                    {
                        globalDefinitions = new Dictionary<string, XElement>();

                        foreach (var defChild in child.Elements())
                        {
                            var idAttr = defChild.Attribute("id");

                            if (idAttr != null)
                            {
                                globalDefinitions[idAttr.Value] = defChild;
                            }
                        }
                        break;
                    }

                    default:
                        if (child.HasElements)
                        {
                            ReadGlobalDefinitions(child);
                        }
                        break;
                }
            }
        }
    }
}
