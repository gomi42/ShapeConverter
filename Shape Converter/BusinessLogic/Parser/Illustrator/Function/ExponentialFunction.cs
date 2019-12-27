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
using ShapeConverter.BusinessLogic.Helper;
using ShapeConverter.Parser.Pdf;

namespace ShapeConverter.BusinessLogic.Parser.Pdf.Function
{
    /// <summary>
    /// The exponential function
    /// </summary>
    internal class ExponentialFunction : IFunction
    {
        private double domain0;
        private double domain1;
        private List<double> c0;
        private List<double> c1;

        /// <summary>
        /// init
        /// </summary>
        public void Init(PdfDictionary functionDict)
        {
            var domainArray = functionDict.Elements.GetArray(PdfKeys.Domain);
            //var rangeArray = functionDict.Elements.GetArray(PdfKeys.Range);
            var c0Array = functionDict.Elements.GetArray(PdfKeys.C0);
            var c1Array = functionDict.Elements.GetArray(PdfKeys.C1);
            var n = functionDict.Elements.GetReal(PdfKeys.N);

            domain0 = domainArray.Elements.GetReal(0);
            domain1 = domainArray.Elements.GetReal(1);

            c0 = PdfUtilities.CreateDoubleList(c0Array);
            c1 = PdfUtilities.CreateDoubleList(c1Array);
        }

        /// <summary>
        /// Get the boundary values of this function
        /// </summary>
        public List<FunctionStop> GetBoundaryValues()
        {
            return new List<FunctionStop>
                         {
                             new FunctionStop { Stop = domain0, Value = c0 },
                             new FunctionStop { Stop = domain1, Value = c1 }
                         };
        }

        /// <summary>
        /// Calculate result values based on the input values
        /// </summary>
        public List<double> Calculate(List<double> values)
        {
            var results = new List<double>();

            for (int i = 0; i < c0.Count; i++)
            {
                var y2 = DoubleUtilities.Interpolate(values[i], domain0, domain1, c0[i], c1[i]);
                results.Add(y2);
            }

            return results;
        }
    }
}
