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

using EpsSharp.Eps.Core;
using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace EpsSharp.Eps.Helper
{
    /// <summary>
    /// Operand helper methods
    /// </summary>
    internal static class OperandHelper
    {
        /// <summary>
        /// Returns the string content of either a string
        /// or name operand
        /// </summary>
        /// <param name="stringOrName"></param>
        /// <returns></returns>
        public static string GetStringValue(Operand stringOrName)
        {
            string retVal = TryGetStringValue(stringOrName);

            if (retVal == null)
            {
                throw new Exception("illegal operand type");
            }

            return retVal;
        }

        /// <summary>
        /// Returns the string content of either a string
        /// or name operand
        /// </summary>
        /// <param name="stringOrName"></param>
        /// <returns></returns>
        public static string TryGetStringValue(Operand stringOrName)
        {
            switch (stringOrName)
            {
                case NameOperand nameOperand:
                {
                    return nameOperand.Value;
                }

                case StringOperand stringOperand:
                {
                    return stringOperand.Value;
                }

                default:
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Returns a double value from the given operand
        /// that is known to be a number (either real or int)
        /// </summary>
        /// <param name="realOrIntegerOperand"></param>
        /// <returns>a double value</returns>
        public static double GetRealValue(Operand realOrIntegerOperand)
        {
            switch (realOrIntegerOperand)
            {
                case RealOperand realOperand:
                {
                    return realOperand.Value;
                }

                case IntegerOperand intOperand:
                {
                    return (double)intOperand.Value;
                }

                default:
                    throw new Exception("no number operand");
            }
        }

        /// <summary>
        /// Get a matrix out of a sequence fof operands
        /// </summary>
        public static Matrix GetMatrix(ArrayOperand array)
        {
            var matrix = new Matrix(GetRealValue(array.Values[0].Operand),
                                    GetRealValue(array.Values[1].Operand),
                                    GetRealValue(array.Values[2].Operand),
                                    GetRealValue(array.Values[3].Operand),
                                    GetRealValue(array.Values[4].Operand),
                                    GetRealValue(array.Values[5].Operand));
            return matrix;
        }

        /// <summary>
        /// Convert an array operand to a list of doubles
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static List<double> GetRealValues(ArrayOperand array)
        {
            int num = array.Values.Count;
            List<double> values = new List<double>();

            for (int i = 0; i < num; i++)
            {
                values.Add(GetRealValue(array.Values[i].Operand));
            }

            return values;
        }

        /// <summary>
        /// Create a colos space description
        /// </summary>
        public static ArrayOperand CreateColorSpaceDescription(string colorSpaceName)
        {
            var array = new ArrayOperand();

            var name = colorSpaceName;
            var proxy = new OperandProxy(new NameOperand(name));
            array.Values.Add(proxy);

            return array;
        }
    }
}
