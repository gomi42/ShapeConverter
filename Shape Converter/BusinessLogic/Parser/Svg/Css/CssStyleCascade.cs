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
using System.Xml.Linq;
using ShapeConverter.BusinessLogic.Parser.Svg.CSS;
using ShapeConverter.BusinessLogic.Parser.Svg.Helper;

namespace ShapeConverter.BusinessLogic.Parser.Svg
{
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
        };

        private List<CssStyleDeclaration> styleDeclarations = new List<CssStyleDeclaration>();
        private List<CssStyleSheet> cssStyleSheets;

        /// <summary>
        /// Constructor
        /// </summary>
        public CssStyleCascade(XElement element)
        {
            cssStyleSheets = new List<CssStyleSheet>();

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
                        var styleSheet = new CssStyleSheet(child);
                        cssStyleSheets.Add(styleSheet);
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

            // the order of the next 5 calls determine the cascade
            // less important (lowest specificity) first, 
            // most important (highest specificity) last
            SetPresentationAttributes(styleDeclaration, element);
            SetElementProperties(styleDeclaration, element);
            SetClassProperties(styleDeclaration, element);
            SetIdProperties(styleDeclaration, element);
            SetStyleProperties(styleDeclaration, element);
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
        /// Set the properties of the element style of the given element in the style declaration
        /// </summary>
        private void SetElementProperties(CssStyleDeclaration styleDeclaration, XElement element)
        {
            SetProperties(styleDeclaration, element.Name.LocalName);
        }

        /// <summary>
        /// Set the properties of the class style of the given element in the style declaration
        /// </summary>
        private void SetClassProperties(CssStyleDeclaration styleDeclaration, XElement element)
        {
            var classAttr = element.Attributes().FirstOrDefault(x => x.Name == "class");

            if (classAttr != null)
            {
                SetProperties(styleDeclaration, "." + classAttr.Value);
                SetProperties(styleDeclaration, element.Name.LocalName + "." + classAttr.Value);
            }
        }

        /// <summary>
        /// Set the properties of the id style of the given element in the style declaration
        /// </summary>
        private void SetIdProperties(CssStyleDeclaration styleDeclaration, XElement element)
        {
            var idAttr = element.Attributes().FirstOrDefault(x => x.Name == "id");

            if (idAttr != null)
            {
                SetProperties(styleDeclaration, "#" + idAttr.Value);
                SetProperties(styleDeclaration, element.Name.LocalName + "#" + idAttr.Value);
            }
        }

        /// <summary>
        /// Set the properties from the style sheets from the given selector 
        /// </summary>
        private void SetProperties(CssStyleDeclaration styleDeclarationToModify, string selector)
        {
            foreach (var styleSheet in cssStyleSheets)
            {
                var styleDeclarations = styleSheet.GetStylesForSelector(selector);

                foreach (var styleDeclaration in styleDeclarations)
                {
                    foreach (var name in styleDeclaration.Properties)
                    {
                        var value = styleDeclaration.GetPropertyValue(name);
                        styleDeclarationToModify.SetProperty(name, value);
                    }
                }
            }
        }

        /// <summary>
        /// Set the properties of the style attribute of the given element in the style declaration
        /// </summary>
        private void SetStyleProperties(CssStyleDeclaration styleDeclaration, XElement element)
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
                var value = styles.GetPropertyValue(name);

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
            if (styleDeclarations.Count > 0)
            {
                var styles = styleDeclarations[styleDeclarations.Count - 1];
                var value = styles.GetPropertyValue(name);

                if (value != null)
                {
                    return value;
                }
            }

            return null;
        }

        /// <summary>
        /// Get an attribute as double from the cascade
        /// </summary>
        public double GetLength(string name, double defaultValue)
        {
            double retVal = defaultValue;

            var strVal = GetProperty(name);

            if (!string.IsNullOrEmpty(strVal))
            {
                retVal = DoubleParser.ParseLength(strVal);
            }

            return retVal;
        }

        /// <summary>
        /// Get an attribute as double from the cascade
        /// </summary>
        public double GetNumber(string name, double defaultValue)
        {
            double retVal = defaultValue;

            var strVal = GetProperty(name);

            if (!string.IsNullOrEmpty(strVal))
            {
                retVal = DoubleParser.ParseNumber(strVal);
            }

            return retVal;
        }

        /// <summary>
        /// Get an attribute as double from the top of cascade only
        /// </summary>
        public double GetNumberPercentFromTop(string name, double defaultValue)
        {
            double retVal = defaultValue;

            var strVal = GetPropertyFromTop(name);

            if (!string.IsNullOrEmpty(strVal))
            {
                DoubleParser.ParseNumberPercent(strVal, out retVal);
            }

            return retVal;
        }

        /// <summary>
        /// Get an attribute as string from the cascade
        /// </summary>
        private string GetStringAttribute(XElement element, string name, string defaultValue)
        {
            string retVal = defaultValue;

            var strVal = GetProperty(name);

            if (!string.IsNullOrEmpty(retVal))
            {
                retVal = strVal;
            }

            return retVal;
        }
    }
}
