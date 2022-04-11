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
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Xml.Linq;
using ShapeConverter.BusinessLogic.Parser.Svg.CSS;

namespace ShapeConverter.BusinessLogic.Parser.Svg
{
    /// <summary>
    /// The svg view box definition
    /// </summary>
    internal struct SvgViewBox
    {
        public Rect ViewBox;
        public string Align;
        public string Slice;
    }

    /// <summary>
    /// The Css style cascade
    /// </summary>
    internal class CssStyleCascade
    {
        readonly string[] presentationAttributes =
        {
            "clip-path",
            "clip-rule",
            "opacity",
            "transform",
            "color",
            "fill",
            "fill-opacity",
            "stroke",
            "stroke-opacity",
            "stroke-width",
            "stroke-miterlimit",
            "stroke-miterlimit",
            "stroke-linecap",
            "stroke-linejoin",
            "stroke-dasharray",
            "stroke-dashoffset",
            "display",
            "visibility",
            "font-family",
            "font-size",
            "font-style",
            "font-weight",
            "font-stretch",
            "text-anchor",
            "stop-opacity",
            "stop-color",
        };

        private List<CssStyleDeclaration> styleDeclarations;
        private CssStyleSheet cssStyleSheet;
        private Stack<SvgViewBox> svgViewBoxStack;

        /// <summary>
        /// Constructor
        /// </summary>
        public CssStyleCascade(XElement element)
        {
            styleDeclarations = new List<CssStyleDeclaration>();
            cssStyleSheet = new CssStyleSheet();
            svgViewBoxStack = new Stack<SvgViewBox>();

            ReadGlobalStyles(element);
        }

        /// <summary>
        /// Read all styles
        /// </summary>
        private void ReadGlobalStyles(XElement element)
        {
            foreach (var child in element.Elements())
            {
                switch (child.Name.LocalName)
                {
                    case "style":
                    {
                        cssStyleSheet.Parse(child);
                        break;
                    }

                    default:
                        if (child.HasElements)
                        {
                            ReadGlobalStyles(child);
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Set the styles of the specified element at the top of the cascade
        /// </summary>
        public void PushStyles(XElement element)
        {
            var styleDeclaration = new CssStyleDeclaration();
            styleDeclarations.Add(styleDeclaration);

            SetPresentationAttributes(styleDeclaration, element);
            SetPropertiesFromGlobalStyles(styleDeclaration, element);
            SetPropertiesFromLocalStyle(styleDeclaration, element);
        }

        /// <summary>
        /// Remove entry from the top of the cascade
        /// </summary>
        public void Pop()
        {
            styleDeclarations.RemoveAt(styleDeclarations.Count - 1);
        }

        /// <summary>
        /// Set the presentation attributes of the given element in the style declaration
        /// </summary>
        private void SetPresentationAttributes(CssStyleDeclaration styleDeclaration, XElement element)
        {
            foreach (var attributeName in presentationAttributes)
            {
                XAttribute attribute = element.Attribute(attributeName);

                if (attribute != null)
                {
                    styleDeclaration.SetProperty(attributeName, attribute.Value);
                }
            }
        }

        /// <summary>
        /// Set the properties from the global styles
        /// </summary>
        void SetPropertiesFromGlobalStyles(CssStyleDeclaration styleDeclarationToModify, XElement element)
        {
            var styleDeclarations = cssStyleSheet.GetStylesForElement(element);

            foreach (var styleDeclaration in styleDeclarations)
            {
                foreach (var name in styleDeclaration.Properties)
                {
                    var value = styleDeclaration.GetProperty(name);
                    styleDeclarationToModify.SetProperty(name, value);
                }
            }
        }

        /// <summary>
        /// Set the properties from the style attribute of the given element in the style declaration
        /// </summary>
        private void SetPropertiesFromLocalStyle(CssStyleDeclaration styleDeclaration, XElement element)
        {
            var styleAttr = element.Attributes().FirstOrDefault(x => x.Name == "style");

            if (styleAttr != null)
            {
                styleDeclaration.Parse(styleAttr.Value);
            }
        }

        /// <summary>
        /// Get a property from the cascade
        /// </summary>
        public string GetProperty(string name)
        {
            for (int i = styleDeclarations.Count - 1; i >= 0; i--)
            {
                var styles = styleDeclarations[i];
                var value = styles.GetProperty(name);

                if (value != null)
                {
                    return value;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Get a property from the top of the cascade only
        /// </summary>
        public string GetPropertyFromTop(string name)
        {
            return GetPropertyFromLevel(name, 0);
        }

        /// <summary>
        /// Get a property from the given level (0 = top)
        /// </summary>
        public string GetPropertyFromLevel(string name, int level)
        {
            if (styleDeclarations.Count > level)
            {
                var styles = styleDeclarations[styleDeclarations.Count - 1 - level];
                var value = styles.GetProperty(name);

                if (value != null)
                {
                    return value;
                }
            }

            return null;
        }

        /// <summary>
        /// Get a property from the top of the cascade only
        /// </summary>
        public void SetPropertyOnTop(string name, string prop)
        {
            if (styleDeclarations.Count > 0)
            {
                var styles = styleDeclarations[styleDeclarations.Count - 1];
                styles.SetProperty(name, prop);
            }
        }

        /// <summary>
        /// Push a new view box on the stack
        /// </summary>
        public void PushViewBox(SvgViewBox svgViewBox)
        {
            svgViewBoxStack.Push(svgViewBox);
        }

        /// <summary>
        /// Pop view box from the stack
        /// </summary>
        public void PopViewBox()
        {
            svgViewBoxStack.Pop();
        }

        /// <summary>
        /// Get the current view box
        /// </summary>
        public SvgViewBox GetCurrentViewBox()
        {
            return svgViewBoxStack.Peek();
        }
    }
}
