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
using System.Xml.Linq;

namespace ShapeConverter.BusinessLogic.Parser.Svg.CSS
{
    internal class CssStyleSheet
    {
        private List<CssStyleRule> cssRules = new List<CssStyleRule>();

        /// <summary>
        /// Constructor
        /// </summary>
        public void Parse(XElement styleElement)
        {
            string css = PrepareContent(styleElement);
            Parse(ref css);
        }

        /// <summary>
        /// Prepare the content of the given style element for further processing
        /// </summary>
        private string PrepareContent(XElement styleElement)
        {
            string css = styleElement.Value;

            if (css == null || css.Length == 0)
            {
                return string.Empty;
            }

            // remove comments
            Regex reComment = new Regex(@"(//.*)|(/\*(.|\n)*?\*/)");
            css = reComment.Replace(css, string.Empty);

            return css;
        }

        /// <summary>
        /// Parse the rules
        /// </summary>
        private void Parse(ref string css)
        {
            bool withBrackets = false;
            css = css.Trim();

            if (css.StartsWith("{", StringComparison.OrdinalIgnoreCase))
            {
                withBrackets = true;
                css = css.Substring(1);
            }

            while (true)
            {
                css = css.Trim();
                if (css.Length == 0)
                {
                    if (withBrackets)
                    {
                        throw new Exception("Style block missing ending bracket");
                    }
                    break;
                }
                else if (css.StartsWith("}", StringComparison.OrdinalIgnoreCase))
                {
                    // end of block;
                    css = css.Substring(1);
                    break;
                }
                else if (css.StartsWith("@", StringComparison.OrdinalIgnoreCase))
                {
                    IgnoreRule(ref css);
                }
                else
                {
                    // must be a selector or error
                    CssStyleRule rule = CssStyleRule.Parse(ref css);

                    if (rule != null)
                    {
                        cssRules.Add(rule);
                    }
                    else
                    {
                        IgnoreRule(ref css);
                    }
                }
            }
        }

        /// <summary>
        /// Try the best and ignore the next rule
        /// </summary>
        private void IgnoreRule(ref string css)
        {
            int startBracket = css.IndexOf("{", StringComparison.OrdinalIgnoreCase);
            int endBracket = css.IndexOf("}", StringComparison.OrdinalIgnoreCase);
            int endSemiColon = css.IndexOf(";", StringComparison.OrdinalIgnoreCase);
            int endRule;

            if (endSemiColon > 0 && endSemiColon < startBracket)
            {
                endRule = endSemiColon;
            }
            else
            {
                endRule = endBracket;
            }

            if (endRule > -1)
            {
                css = css.Substring(endRule + 1);
            }
            else
            {
                throw new Exception("Error in style rule");
            }
        }

        /// <summary>
        /// Helper to sort styles based on their specificity
        /// </summary>
        private struct SortStyle
        {
            public int Specificity;
            public CssStyleDeclaration Style;
        }

        /// <summary>
        /// Get all style declarations that match the selector
        /// </summary>
        public List<CssStyleDeclaration> GetStylesForElement(XElement element)
        {
            List<SortStyle> foundStyles = new List<SortStyle>();

            foreach (var rule in cssRules)
            {
                var specifity = rule.MatchElement(element);

                if (specifity >= 0)
                {
                    var ss = new SortStyle { Specificity = specifity, Style = rule.Style };
                    foundStyles.Add(ss);
                }
            }

            // sort all matching styles, lowest specificity first
            foundStyles.Sort(delegate (SortStyle a, SortStyle b)
            {
                           var sa = a.Specificity;
                           var sb = b.Specificity;

                if (sa == sb)
                {
                    return 0;
                }

                return sa < sb ? -1 : 1;
            });

            return foundStyles.Select(x => x.Style).ToList();
        }
    }
}
