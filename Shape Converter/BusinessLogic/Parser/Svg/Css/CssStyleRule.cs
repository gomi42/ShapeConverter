//
// Author:
//   Michael Göricke
//
// Copyright (c) 2020
// Full credit goes to SharpVectors:
// The idea and the logic of the xpath creation is taken from SharpVectors.
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
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;

namespace ShapeConverter.BusinessLogic.Parser.Svg.CSS
{
    /// <summary>
    /// A single style rule
    /// Although we parse selectors containing combinators we don't support 
    /// combinators in this simplified style engine. This is done just to ignore
    /// them and move on to the next selector.
    /// </summary>
    internal class CssStyleRule
    {
        private static string StyleRule = "^((?<selector>(" + CssSelector.SelectorRule + @")+)(?:\s*,\s*)?)+";
        private static Regex StyleRuleRegex = new Regex(StyleRule);

        private List<CssSelector> selectors;

        /// <summary>
        /// The declaration-block of this rule set.
        /// </summary>
        public CssStyleDeclaration Style { get; private set; }

        /// <summary>
        /// Parse and construct a CssStyleRule
        /// </summary>
        public static CssStyleRule Parse(ref string css)
        {
            Match match = StyleRuleRegex.Match(css);

            if (match.Success && match.Length > 0)
            {
                var rule = new CssStyleRule(match);
                css = css.Substring(match.Length);
                rule.Style = new CssStyleDeclaration(ref css);

                return rule;
            }

            return null;
        }

        /// <summary>
        /// The constructor for CssStyleRule
        /// </summary>
        private CssStyleRule(Match match)
        {
            Group group = match.Groups["selector"];
            selectors = new List<CssSelector>();

            foreach (Capture capture in group.Captures)
            {
                string str = capture.Value.Trim();

                if (str.Length > 0)
                {
                    selectors.Add(new CssSelector(str));
                }
            }
        }

        /// <summary>
        /// Tests whether this rule matches the given element
        /// </summary>
        public int MatchElement(XElement element)
        {
            var nav = element.CreateNavigator();
            int specificity = -1;

            foreach (var selector in selectors)
            {
                if (selector != null && selector.Match(nav))
                {
                    var spec = selector.Specificity;

                    if (spec >= specificity)
                    {
                        specificity = spec;
                    }
                }
            }

            return specificity;
        }
    }
}
