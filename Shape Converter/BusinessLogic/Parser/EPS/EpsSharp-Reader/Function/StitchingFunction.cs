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
using EpsSharp.Eps.Core;
using EpsSharp.Eps.Helper;

namespace EpsSharp.Eps.Function
{
    /// <summary>
    /// The stitching function
    /// </summary>
    internal class StitchingFunction : IFunction
    {
        private List<IFunction> functions;
        private List<double> bounds;
        private bool reverse;

        /// <summary>
        /// init
        /// </summary>
        /// <param name="functionDict"></param>
        public void Init(DictionaryOperand functionDict)
        {
            var domainArray = functionDict.Dictionary.GetArray(EpsKeys.Domain);
            var functionsArray = functionDict.Dictionary.GetArray(EpsKeys.Functions);
            var encodeArray = functionDict.Dictionary.GetArray(EpsKeys.Encode);

            var e0 = OperandHelper.GetRealValue(encodeArray.Values[0].Operand);
            var e1 = OperandHelper.GetRealValue(encodeArray.Values[1].Operand);

            // It doesn't make sense to revert only one function
            // we assume either none or all functions are reverted
            reverse = e0 > e1;

            var boundsArray = functionDict.Dictionary.GetArray(EpsKeys.Bounds);
            bounds = OperandHelper.GetRealValues(boundsArray);
            bounds.Insert(0, OperandHelper.GetRealValue(domainArray.Values[0].Operand));
            bounds.Add(OperandHelper.GetRealValue(domainArray.Values[1].Operand));

            functions = new List<IFunction>();

            for (int i = 0; i < functionsArray.Values.Count; i++)
            {
                var fDict = (DictionaryOperand)functionsArray.Values[i].Operand;
                var function = FunctionActivator.CreateFunction(fDict);
                functions.Add(function);
            }
        }

        /// <summary>
        /// Get the boundary values over all sub-functions and combine
        /// them to one single array. Don't add the boundaries of
        /// two functions twice.
        /// Here we assume stitching functions are not nested, but they
        /// only "point" to "simple/leaf" functions (sampled or exponential).
        /// </summary>
        public List<FunctionStop> GetBoundaryValues()
        {
            var stops = new List<FunctionStop>();

            for (int i = 0; i < functions.Count; i++)
            {
                var boundary = functions[i].GetBoundaryValues();
                double bound0 = bounds[i];
                double bound1 = bounds[i + 1];

                if (!reverse)
                {
                    boundary[0].Stop = bound0;
                    boundary[1].Stop = bound1;
                }
                else
                {
                    boundary[0].Stop = 1 - bound0;
                    boundary[1].Stop = 1 - bound1;
                }

                if (i == 0)
                {
                    stops.Add(boundary[0]);
                    stops.Add(boundary[1]);
                }
                else
                {
                    stops.Add(boundary[1]);
                }
            }

            return stops;
        }
    }
}
