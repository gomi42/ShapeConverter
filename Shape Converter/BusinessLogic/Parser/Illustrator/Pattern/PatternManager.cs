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
using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;
using ShapeConverter.Parser.Pdf;

namespace ShapeConverter.BusinessLogic.Parser.Pdf.Pattern
{
    /// <summary>
    /// The pattern manager
    /// </summary>
    internal class PatternManager
    {
        private Dictionary<string, IPattern> patterns;

        /// <summary>
        /// Init
        /// </summary>
        public void Init(PdfResources resourcesDict)
        {
            if (resourcesDict == null)
            {
                return;
            }

            var patternDict = resourcesDict.Patterns;

            if (patternDict == null)
            {
                return;
            }

            ReadResourceDefinedPatterns(patternDict);
        }

        /// <summary>
        /// Return true if the color can be accurately computed
        /// otherwise the color is estimated
        /// </summary>
        private void ReadResourceDefinedPatterns(PdfDictionary patternsDict)
        {
            patterns = new Dictionary<string, IPattern>();

            foreach (var name in patternsDict.Elements.Keys)
            {
                var patternDict = patternsDict.Elements.GetDictionary(name);
                var pattern = ReadPattern(patternDict);
                patterns.Add(name, pattern);
            }
        }

        /// <summary>
        /// Read and create a single pattern
        /// </summary>
        private IPattern ReadPattern(PdfDictionary patternDict)
        {
            IPattern pattern = null;
            var type = patternDict.Elements.GetInteger(PdfKeys.PatternType);

            switch (type)
            {
                case 1:
                {
                    pattern = new TilingPattern();
                    break;
                }

                case 2:
                {
                    pattern = new ShadingPattern();
                    break;
                }

                default:
                {
                    pattern = new UnknownPattern();
                    break;
                }
            }

            pattern.Init(patternDict);

            return pattern;
        }

        /// <summary>
        /// Get a pattern by name
        /// </summary>
        public IPattern GetPattern(string name)
        {
            return patterns[name];
        }
    }
}
