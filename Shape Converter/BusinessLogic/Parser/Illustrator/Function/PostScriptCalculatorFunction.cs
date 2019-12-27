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
using PdfSharp.Pdf;
using ShapeConverter.BusinessLogic.Parser.Pdf.Function;
using ShapeConverter.Parser.Pdf;

namespace ShapeConverter.BusinessLogic.Parser.Pdf.Function
{
    /// <summary>
    /// The postscript calculator function
    /// </summary>
    internal class PostScriptCalculatorFunction : IFunction
    {
        private List<double> domains;
        private List<double> ranges;
        private string script;

        public void Init(PdfDictionary functionDict)
        {
            var domainArray = functionDict.Elements.GetArray(PdfKeys.Domain);
            var rangeArray = functionDict.Elements.GetArray(PdfKeys.Range);

            domains = PdfUtilities.CreateDoubleList(domainArray);
            ranges = PdfUtilities.CreateDoubleList(rangeArray);

            script = functionDict.Stream.ToString();
            script = script.Trim();

            if (script[0] == '{')
            {
                script = script.Substring(1, script.Length - 2);
            }
        }

        /// <summary>
        /// Get the boundary values of this function
        /// </summary>
        public List<FunctionStop> GetBoundaryValues()
        {
            var c0 = new List<double>();
            var c1 = new List<double>();

            for (int i = 0; i < ranges.Count / 2; i += 2)
            {
                c0.Add(ranges[i]);
                c1.Add(ranges[i + 1]);
            }

            return new List<FunctionStop>
                         {
                             new FunctionStop { Stop = domains[0], Value = c0 },
                             new FunctionStop { Stop = domains[1], Value = c1 }
                         };
        }

        /// <summary>
        /// Calculate result values based on the input values
        /// </summary>
        public List<double> Calculate(List<double> values)
        {
            var scriptRunner = new EpsSharp.Eps.Core.PostScriptRunner();
            var results = scriptRunner.RunFormula(script, values);

            return results;
        }
    }
}
