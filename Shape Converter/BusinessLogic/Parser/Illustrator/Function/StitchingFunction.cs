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
using ShapeConverter.Parser.Pdf;

namespace ShapeConverter.BusinessLogic.Parser.Pdf.Function
{
    /// <summary>
    /// The stiching function
    /// </summary>
    internal class StitchingFunction : IFunction
    {
        private List<IFunction> functions;
        private List<double> bounds;
        private List<double> encode;

        /// <summary>
        /// init
        /// </summary>
        public void Init(PdfDictionary functionDict)
        {
            var domainArray = functionDict.Elements.GetArray(PdfKeys.Domain);
            var rangeArray = functionDict.Elements.GetArray(PdfKeys.Range);
            var functionsArray = functionDict.Elements.GetArray(PdfKeys.Functions);

            var encodeArray = functionDict.Elements.GetArray(PdfKeys.Encode);
            encode = PdfUtilities.CreateDoubleList(encodeArray);

            var boundsArray = functionDict.Elements.GetArray(PdfKeys.Bounds);
            bounds = PdfUtilities.CreateDoubleList(boundsArray);
            bounds.Insert(0, domainArray.Elements.GetReal(0));
            bounds.Add(domainArray.Elements.GetReal(1));

            functions = new List<IFunction>();

            for (int i = 0; i < functionsArray.Elements.Count; i++)
            {
                var fDict = functionsArray.Elements.GetDictionary(i);
                var function = FunctionManager.ReadFunction(fDict);
                functions.Add(function);
            }
        }

        /// <summary>
        /// Get the boundary values over all sub-functions and combine
        /// them to one single array. Don't add the boundaries of
        /// two functions twice.
        /// Illustrator works always in the relative range 0..1 or 1..0. We
        /// use that knowledge and keep the code simple.
        /// </summary>
        public List<FunctionStop> GetBoundaryValues()
        {
            var stops = new List<FunctionStop>();

            for (int i = 0; i < functions.Count; i++)
            {
                var boundary = functions[i].GetBoundaryValues();
                double bound0 = bounds[i];
                double bound1 = bounds[i + 1];

                boundary[0].Stop = bound0;
                boundary[1].Stop = bound1;

                // only value pairs 0/1 or 1/0 are assumed for encode
                var reverse = encode[i*2] > encode[i*2 + 1];

                if (i == 0)
                {
                    if (!reverse)
                    {
                        boundary[0].Stop = bound0;
                        boundary[1].Stop = bound1;

                        stops.Add(boundary[0]);
                        stops.Add(boundary[1]);
                    }
                    else
                    {
                        boundary[0].Stop = bound1;
                        boundary[1].Stop = bound0;

                        stops.Add(boundary[1]);
                        stops.Add(boundary[0]);
                    }
                }
                else
                {
                    if (!reverse)
                    {
                        boundary[1].Stop = bound1;
                        stops.Add(boundary[1]);
                    }
                    else
                    {
                        boundary[0].Stop = bound1;
                        stops.Add(boundary[0]);
                    }
                }
            }

            return stops;
        }

        /// <summary>
        /// Calculate result values based on the input values
        /// </summary>
        public List<double> Calculate(List<double> values)
        {
            double x = values[0];
            double y = 0;
            int count = functions.Count;

            for (int i = 0; i < count; i++)
            {
                if (i == count - 1 || x < bounds[i])
                {
                    y = functions[i].Calculate(values)[0];
                    break;
                }
            }

            return new List<double> { y };
        }
    }
}
