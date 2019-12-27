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
using System.Windows.Media;
using System.Xml.Linq;
using ShapeConverter.BusinessLogic.Generators;
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
            GraphicVisual visual = ParseGroup(defaultNamespace, root, currentTransformationMatrix);
            visual = OptimizeVisual.Optimize(visual);

            return visual;
        }

        /// <summary>
        /// Parse all graphic elements
        /// </summary>
        private GraphicGroup ParseGroup(XNamespace ns, XElement groupElement, Matrix matrix)
        {
            var group = new GraphicGroup();

            cssStyleCascade.PushStyles(groupElement);

            Matrix currentTransformationMatrix = matrix;

            var transform = cssStyleCascade.GetPropertyFromTop("transform");

            if (!string.IsNullOrEmpty(transform))
            {
                var transformMatrix = TransformMatrixParser.GetTransformMatrix(transform);
                currentTransformationMatrix = transformMatrix * currentTransformationMatrix;
            }

            Clipping.SetClipPath(group, currentTransformationMatrix, cssStyleCascade, globalDefinitions);
            group.Opacity = cssStyleCascade.GetDoubleFromTop("opacity", 1);

            var shapeParser = new ShapeParser();

            foreach (var element in groupElement.Elements())
            {
                switch (element.Name.LocalName)
                {
                    case "defs":
                    case "style":
                        // already read, ignore
                        break;

                    case "g":
                    case "svg":
                    {
                        var childGroup = ParseGroup(ns, element, currentTransformationMatrix);
                        group.Childreen.Add(childGroup);
                        break;
                    }

                    default:
                    {
                        var shape = shapeParser.Parse(element, ns, currentTransformationMatrix, cssStyleCascade, globalDefinitions);

                        if (shape != null)
                        {
                            group.Childreen.Add(shape);
                        }
                        break;
                    }
                }
            }

            cssStyleCascade.Pop();

            return group;
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
