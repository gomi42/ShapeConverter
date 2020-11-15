//
// Author:
//   Michael Göricke
//
// Copyright (c) 2020
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

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

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
        internal static string SelectorRule =
            @"(?:[A-Za-z][A-Za-z0-9]*)?" +          //type
            @"(?:\.[A-Za-z][_A-Za-z0-9\-]*)?" +     //class
            @"(?:\#[A-Za-z][_A-Za-z0-9\-]*)?" +     //id
            @"(?:(\s*(\+|\>|\~)\s*)|(\s+))?";       //combinator
        private static string StyleRule = "^((?<selector>(" + SelectorRule + @")+)(?:\s*,\s*)?)+";
        private static Regex StyleRuleRegex = new Regex(StyleRule);

        /// <summary>
        /// Parse
        /// </summary>
        internal static CssStyleRule Parse(ref string css)
        {
            Match match = StyleRuleRegex.Match(css);

            if (match.Success && match.Length > 0)
            {
                CssStyleRule rule = new CssStyleRule(match);

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
            Group selectorMatches = match.Groups["selector"];
            var selectors = new List<string>();

            int len = selectorMatches.Captures.Count;

            for (int i = 0; i < len; i++)
            {
                string str = selectorMatches.Captures[i].Value.Trim();

                if (str.Length > 0)
                {
                    selectors.Add(str);
                    var sel = new CssSelector(str);
                }
            }

            Selectors = selectors.AsReadOnly();
        }

        /// <summary>
        /// The selectors of this rule
        /// </summary>
        public IReadOnlyList<string> Selectors { get; private set; }

        /// <summary>
        /// The declaration-block of this rule set.
        /// </summary>
        public CssStyleDeclaration Style { get; private set; }

        /// <summary>
        /// Tests whether this rule contains the specified selector
        /// </summary>
        public bool MatchSelector(string selectorToLookFor)
        {
            return Selectors.Any(x => x == selectorToLookFor);
        }
    }
}
