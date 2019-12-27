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

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ShapeConverter.BusinessLogic.Parser.Svg.CSS
{
    internal class CssStyleRule
    {
        internal static string nsPattern = @"([A-Za-z\*][A-Za-z0-9]*)?\|";
        internal static string attributeValueCheck = "(?<attname>(" + nsPattern + ")?[_a-zA-Z0-9\\-]+)\\s*(?<eqtype>[\\~\\^\\$\\*\\|]?)=\\s*(\"|\')?(?<attvalue>.*?)(\"|\')?";

        internal static string sSelector = "(?<ns>" + nsPattern + ")?" +
            @"(?<type>([A-Za-z\*][A-Za-z0-9]*))?" +
            @"((?<class>\.[A-Za-z][_A-Za-z0-9\-]*)+)?" +
            @"(?<id>\#[A-Za-z][_A-Za-z0-9\-]*)?" +
            @"((?<predicate>\[\s*(" +
            @"(?<attributecheck>(" + nsPattern + ")?[a-zA-Z0-9]+)" +
            @"|" +
            "(?<attributevaluecheck>" + attributeValueCheck + ")" +
            @")\s*\])+)?" +
            @"((?<pseudoclass>\:[a-z\-]+(\([^\)]+\))?)+)?" +
            @"(?<pseudoelements>(\:\:[a-z\-]+)+)?" +
            @"(?<seperator>(\s*(\+|\>|\~)\s*)|(\s+))?";
        private static string sStyleRule = "^((?<selector>(" + sSelector + @")+)(\s*,\s*)?)+";
        private static Regex regex = new Regex(sStyleRule);


        /// <summary>
        /// Parse
        /// </summary>
        internal static CssStyleRule Parse(ref string css)
        {
            Match match = regex.Match(css);

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
        internal CssStyleRule(Match match)
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
