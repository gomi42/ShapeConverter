//
// Author:
//   Michael Göricke
//
// Copyright (c) 2020
// Full credit goes to SharpVectors:
// The idea and the logic of the xpath creation are taken from SharpVectors.
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

using System.Text;
using System.Text.RegularExpressions;
using System.Xml.XPath;

namespace ShapeConverter.BusinessLogic.Parser.Svg.CSS
{
    /// <summary>
    /// The CSS selector
    /// </summary>
    internal class CssSelector
    {
        // the ShapeConverter doesn't need to support the full style 
        // specification, we support the most important selectors
        // plus all combinators
        internal static string SelectorRule =
            @"(?<type>([A-Za-z\*][A-Za-z0-9]*))?" +
            @"((?<class>\.[A-Za-z][_A-Za-z0-9\-]*)+)?" +
            @"(?<id>\#[A-Za-z][_A-Za-z0-9\-]*)?" +
            @"(?<combinator>(\s*(\+|\>|\~)\s*)|(\s+))?";

        private int specificity;
        private static Regex selectorRegex = new Regex(SelectorRule);
        private string xpath;

        /// <summary>
        /// Constructor
        /// </summary>
        public CssSelector(string selector)
        {
            GenerateXPath(selector);
        }

        /// <summary>
        /// The specificity of this selector
        /// </summary>
        public int Specificity => specificity;

        /// <summary>
        /// Add specificity
        /// </summary>
        private void AddSpecificity(int a, int b, int c)
        {
            specificity += a * 100 + b * 10 + c;
        }

        /// <summary>
        /// Generate xpath from type
        /// </summary>
        private string TypeToXPath(Match match)
        {
            string retVal;
            Group group = match.Groups["type"];
            string type = group.Value;

            if (!group.Success || type == "*")
            {
                retVal = string.Empty;
            }
            else
            {
                retVal = "[local-name()='" + type + "']";
                AddSpecificity(0, 0, 1);
            }

            return retVal;
        }

        /// <summary>
        /// Generate xpath from class
        /// </summary>
        private string ClassToXPath(Match match)
        {
            string retVal = string.Empty;
            Group group = match.Groups["class"];

            foreach (Capture capture in group.Captures)
            {
                retVal += "[contains(concat(' ',@class,' '),' " + capture.Value.Substring(1) + " ')]";
                AddSpecificity(0, 1, 0);
            }

            return retVal;
        }

        /// <summary>
        /// Generate xpath from id
        /// </summary>
        private string IdToXPath(Match match)
        {
            string retVal = string.Empty;
            Group group = match.Groups["id"];

            if (group.Success)
            {
                retVal = "[@id='" + group.Value.Substring(1) + "']";
                AddSpecificity(1, 0, 0);
            }
            return retVal;
        }

        /// <summary>
        /// Generate xpath from combinator
        /// </summary>
        private void CombinatorToXPath(Match match, StringBuilder xpath, string partialXpath)
        {
            Group group = match.Groups["combinator"];

            if (group.Success)
            {
                string combinator = group.Value.Trim();

                if (combinator.Length == 0)
                {
                    partialXpath += "//*";
                }
                else if (combinator == ">")
                {
                    partialXpath += "/*";
                }
                else if (combinator == "+" || combinator == "~")
                {
                    xpath.Append("[preceding-sibling::*");

                    if (combinator == "+")
                    {
                        xpath.Append("[position()=1]");
                    }

                    xpath.Append(partialXpath);
                    xpath.Append("]");
                    partialXpath = string.Empty;
                }
            }

            xpath.Append(partialXpath);
        }

        /// <summary>
        /// Generator the xpath of this selector
        /// </summary>
        internal void GenerateXPath(string selector)
        {
            specificity = 0;
            StringBuilder xpathSb = new StringBuilder("*");

            var match = selectorRegex.Match(selector.Trim());

            while (match.Success)
            {
                if (match.Success && match.Value.Length > 0)
                {
                    string partialXpath = string.Empty;

                    partialXpath += TypeToXPath(match);
                    partialXpath += ClassToXPath(match);
                    partialXpath += IdToXPath(match);
                    CombinatorToXPath(match, xpathSb, partialXpath);
                }

                match = match.NextMatch();
            }

            xpath = xpathSb.ToString();
        }

        /// <summary>
        /// Test whether the given navigator matches this selector
        /// </summary>
        public bool Match(XPathNavigator navigator)
        {
            return navigator.Matches(xpath);
        }
    }
}
