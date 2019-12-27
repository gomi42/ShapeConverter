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
        /// <param name="functionDict"></param>
        public void Init(DictionaryOperand functionDict)
        {
            var domainArray = functionDict.Dictionary.GetArray(EpsKeys.Domain);
            var c0Array = functionDict.Dictionary.GetArray(EpsKeys.C0);
            var c1Array = functionDict.Dictionary.GetArray(EpsKeys.C1);
            //var rangeArray = functionDict.Dictionary.GetArray(EpsKeys.Range);
            //var n = functionDict.Dictionary.GetInteger(EpsKeys.N);

            domain0 = OperandHelper.GetRealValue(domainArray.Values[0].Operand);
            domain1 = OperandHelper.GetRealValue(domainArray.Values[1].Operand);

            c0 = OperandHelper.GetRealValues(c0Array);
            c1 = OperandHelper.GetRealValues(c1Array);
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
    }
}
