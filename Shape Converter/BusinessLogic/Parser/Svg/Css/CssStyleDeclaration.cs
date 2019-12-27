//
// Author:
//   Michael Göricke
//
// Copyright (c) 2019
// Inspired by SharpVectors
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
using System.Text.RegularExpressions;

namespace ShapeConverter.BusinessLogic.Parser.Svg.CSS
{
    internal class CssStyleDeclaration
    {
        private static Regex styleRegex = new Regex(
            @"^(?<name>[A-Za-z\-0-9]+)\s*:(?<value>[^;\}!]+)(!\s?(?<priority>important))?;?");

        private Dictionary<string, string> styles;

        /// <summary>
        /// The constructor used internally when collecting styles for a specified element
        /// </summary>
        internal CssStyleDeclaration()
        {
            styles = new Dictionary<string, string>();
        }

        /// <summary>
        /// The constructor for CssStyleDeclaration
        /// </summary>
        public CssStyleDeclaration(ref string css) : this()
        {
            css = ParseString(css);
        }

        /// <summary>
        /// Parse the given string and create name/value pairs
        /// </summary>
        public void Parse(string cssText)
        {
            ParseString(cssText);
        }

        /// <summary>
        /// Parse the given string and create name/value pairs
        /// </summary>
        private string ParseString(string cssText)
        {
            bool startedWithABracket = false;

            cssText = cssText.Trim();

            if (cssText.StartsWith("{", StringComparison.OrdinalIgnoreCase))
            {
                cssText = cssText.Substring(1).Trim();
                startedWithABracket = true;
            }

            Match match = styleRegex.Match(cssText);

            while (match.Success)
            {
                string name = match.Groups["name"].Value;
                string value = match.Groups["value"].Value;

                if (styles.ContainsKey(name))
                {
                    styles[name] = value;
                }
                else
                {
                    styles.Add(name, value);
                }

                cssText = cssText.Substring(match.Length).Trim();
                match = styleRegex.Match(cssText);
            }

            cssText = cssText.Trim();

            if (cssText.StartsWith("}", StringComparison.OrdinalIgnoreCase))
            {
                cssText = cssText.Substring(1);
            }
            else if (startedWithABracket)
            {
                throw new Exception("Style declaration ending bracket missing");
            }

            return cssText;
        }

        /// <summary>
        /// Get a property value
        /// </summary>
        public string GetPropertyValue(string propertyName)
        {
            if (!styles.ContainsKey(propertyName))
            {
                return null;
            }

            var value = styles[propertyName];

            return value;
        }

        /// <summary>
        /// Overwrite an existing property or set a new one
        /// </summary>
        public void SetProperty(string propertyName, string value)
        {
            styles[propertyName] = value;
        }

        /// <summary>
        /// The number of properties that have been explicitly set in this declaration block. The range of valid indices is 0 to length-1 inclusive.
        /// </summary>
        public int Length => styles.Count;

        /// <summary>
        /// Get a list with all properties of this style declaration
        /// </summary>
        public IReadOnlyList<string> Properties => styles.Keys.ToList().AsReadOnly();
    }
}
