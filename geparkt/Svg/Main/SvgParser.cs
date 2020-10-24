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
        CssStyleCascade cssStyleCascade;
        Dictionary<string, XElement> globalDefinitions;

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

            ReadGlobalDefinitions(root);
            GraphicVisual visual = ParseSVG(defaultNamespace, root, currentTransformationMatrix);
            visual = OptimizeVisual.Optimize(visual);

            return visual;
        }

        /// <summary>
        /// Parse an SVG document fragment
        /// </summary>
        private GraphicGroup ParseSVG(XNamespace ns, XElement element, Matrix matrix)
        {
            cssStyleCascade.PushStyles(element);

            var vbMatrix = GetViewBoxMatrix(element);
            matrix = vbMatrix * matrix;

            var elementTransformationMatrix = cssStyleCascade.GetTransformMatrixFromTop();
            elementTransformationMatrix = elementTransformationMatrix * matrix;

            var group = ParseGroupChildren(ns, element, elementTransformationMatrix);

            group.Opacity = cssStyleCascade.GetNumberPercentFromTop("opacity", 1);
            Clipping.SetClipPath(group, elementTransformationMatrix, cssStyleCascade, globalDefinitions);

            cssStyleCascade.Pop();
            return group;
        }

        /// <summary>
        /// Parse an g container
        /// </summary>
        private GraphicGroup ParseGContainer(XNamespace ns, XElement element, Matrix matrix)
        {
            cssStyleCascade.PushStyles(element);

            var elementTransformationMatrix = cssStyleCascade.GetTransformMatrixFromTop();
            elementTransformationMatrix = elementTransformationMatrix * matrix;

            var group = ParseGroupChildren(ns, element, elementTransformationMatrix);

            group.Opacity = cssStyleCascade.GetNumberPercentFromTop("opacity", 1);
            Clipping.SetClipPath(group, elementTransformationMatrix, cssStyleCascade, globalDefinitions);

            cssStyleCascade.Pop();
            return group;
        }

        /// <summary>
        /// Parse all graphic elements
        /// </summary>
        private GraphicGroup ParseGroupChildren(XNamespace ns, XElement groupElement, Matrix matrix)
        {
            var group = new GraphicGroup();
            var shapeParser = new ShapeParser();

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
                        var childGroup = ParseSVG(ns, element, matrix);
                        group.Childreen.Add(childGroup);
                        break;
                    }

                    case "g":
                    {
                        var childGroup = ParseGContainer(ns, element, matrix);
                        group.Childreen.Add(childGroup);
                        break;
                    }

                    default:
                    {
                        var shape = shapeParser.Parse(element, ns, matrix, cssStyleCascade, globalDefinitions);

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
        /// Get the viewbox transformation matrix
        /// </summary>
        private Matrix GetViewBoxMatrix(XElement element)
        {
            var viewPort2 = GetViewPort(element);
            var viewBox2 = GetViewBox(element);
            var (align, slice) = GetAlignSlice(element);

            if (!viewPort2.HasValue || !viewBox2.HasValue)
            {
                return Matrix.Identity;
            }

            var viewPort = viewPort2.Value;
            var viewBox = viewBox2.Value;

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

            Matrix matrix = Matrix.Identity;

            if (align == "none")
            {
                matrix = Matrix.Identity;
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

            return matrix;
        }

        /// <summary>
        /// Get the viewport definition
        /// </summary>
        private Rect? GetViewPort(XElement element)
        {
            var x = DoubleAttributeParser.GetLength(element, "x", 0.0);
            var y = DoubleAttributeParser.GetLength(element, "y", 0.0);

            var width = DoubleAttributeParser.GetLengthPercentAuto(element, "width");
            var height = DoubleAttributeParser.GetLengthPercentAuto(element, "height");

            if (width.IsAuto || height.IsAuto)
            {
                return null;
            }

            return new Rect(new Point(x, y), new Size(width.Value, height.Value));
        }

        /// <summary>
        /// Get the viewbox definition
        /// </summary>
        private Rect? GetViewBox(XElement element)
        {
            var viewBoxAttr = element.Attribute("viewBox");

            if (viewBoxAttr == null)
            {
                return null;
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
