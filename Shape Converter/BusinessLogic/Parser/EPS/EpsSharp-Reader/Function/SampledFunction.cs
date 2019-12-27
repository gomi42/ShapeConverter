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
using ShapeConverter.BusinessLogic.Helper;

namespace EpsSharp.Eps.Function
{
    /// <summary>
    /// The sampled function
    /// </summary>
    internal class SampledFunction : IFunction
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
            //var rangeArray = functionDict.Dictionary.GetArray(EpsKeys.Range);
            //var encodeArray = functionDict.Dictionary.GetArray(EpsKeys.Encode);
            var decodeArray = functionDict.Dictionary.GetArray(EpsKeys.Decode);
            var sizeArray = functionDict.Dictionary.GetArray(EpsKeys.Size);
            var dataSource = functionDict.Dictionary.GetString(EpsKeys.DataSource);
            var bitsPerSample = functionDict.Dictionary.GetInteger(EpsKeys.BitsPerSample);

            domain0 = OperandHelper.GetRealValue(domainArray.Values[0].Operand);
            domain1 = OperandHelper.GetRealValue(domainArray.Values[1].Operand);

            var decodes = OperandHelper.GetRealValues(decodeArray);
            var size = (IntegerOperand)sizeArray.Values[0].Operand;
            var data = RelativeSamplesDecoder.Decode(dataSource, bitsPerSample.Value);

            int valuesPerSample = data.Count / size.Value;

            c0 = GetSample(data, 0, valuesPerSample, decodes);
            c1 = GetSample(data, (data.Count - valuesPerSample), valuesPerSample, decodes);
        }

        /// <summary>
        /// Extract a sample from the data array
        /// </summary>
        /// <param name="data">the data array</param>
        /// <param name="offset">the offset where the requested sample starts</param>
        /// <param name="valuesPerSample">number of values per sample</param>
        /// <returns>list of values that descibe a single sample</returns>
        private List<double> GetSample(List<double> data, int offset, int valuesPerSample, List<double> decodes)
        {
            var list = new List<double>();

            for (int i = 0; i < valuesPerSample; i++)
            {
                var value = data[offset + i];
                var decoded = DoubleUtilities.Interpolate(value, 0, 1, decodes[i * 2], decodes[i * 2 + 1]);

                list.Add(decoded);
            }

            return list;
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
