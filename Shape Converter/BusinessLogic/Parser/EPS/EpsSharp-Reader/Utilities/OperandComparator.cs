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
using EpsSharp.Eps.Core;

namespace EpsSharp.Eps.Helper
{
    internal static class OperandComparator
    {
        /// <summary>
        /// Compares 2 operands whether they are equal or not
        /// </summary>
        /// <param name="operand1"></param>
        /// <param name="operand2"></param>
        /// <returns>true if equal</returns>
        public static bool IsEqual(Operand operand1, Operand operand2)
        {
            bool ret;

            switch (operand1)
            {
                case BooleanOperand boolOperand1:
                {
                    switch (operand2)
                    {
                        case BooleanOperand boolOperand2:
                        {
                            ret = boolOperand1.Value == boolOperand2.Value;
                            break;
                        }

                        case NullOperand nullOperand:
                        {
                            ret = false;
                            break;
                        }

                        default:
                            throw new Exception("illegal operator");
                    }

                    break;
                }

                case NameOperand nameOperand1:
                {
                    switch (operand2)
                    {
                        case NameOperand nameOperand2:
                        {
                            ret = nameOperand1.Value == nameOperand2.Value;
                            break;
                        }

                        case StringOperand stringOperand2:
                        {
                            ret = nameOperand1.Value == stringOperand2.Value;
                            break;
                        }

                        case NullOperand nullOperand:
                        {
                            ret = false;
                            break;
                        }

                        default:
                            throw new Exception("illegal operator");
                    }

                    break;
                }

                case StringOperand stringOperand1:
                {
                    switch (operand2)
                    {
                        case NameOperand nameOperand2:
                        {
                            ret = stringOperand1.Value == nameOperand2.Value;
                            break;
                        }

                        case StringOperand stringOperand2:
                        {
                            ret = stringOperand1.Value == stringOperand2.Value;
                            break;
                        }

                        case NullOperand nullOperand:
                        {
                            ret = false;
                            break;
                        }

                        default:
                            throw new Exception("illegal operator");
                    }

                    break;
                }

                case IntegerOperand intOperand1:
                {
                    switch (operand2)
                    {
                        case IntegerOperand intOperand2:
                        {
                            ret = intOperand1.Value == intOperand2.Value;
                            break;
                        }

                        case RealOperand realOperand2:
                        {
                            ret = intOperand1.Value == realOperand2.Value;
                            break;
                        }

                        case NullOperand nullOperand:
                        {
                            ret = false;
                            break;
                        }

                        default:
                            throw new Exception("illegal operator");
                    }

                    break;
                }

                case RealOperand realOperand1:
                {
                    switch (operand2)
                    {
                        case IntegerOperand intOperand2:
                        {
                            ret = realOperand1.Value == intOperand2.Value;
                            break;
                        }

                        case RealOperand realOperand2:
                        {
                            ret = realOperand1.Value == realOperand2.Value;
                            break;
                        }

                        case NullOperand nullOperand:
                        {
                            ret = false;
                            break;
                        }

                        default:
                            throw new Exception("illegal operator");
                    }

                    break;
                }

                case ArrayOperand arrayOperand1:
                {
                    switch (operand2)
                    {
                        case ArrayOperand arrayOperand2:
                        {
                            ret = arrayOperand1 == arrayOperand2;
                            break;
                        }

                        case NullOperand nullOperand:
                        {
                            ret = false;
                            break;
                        }

                        default:
                            throw new Exception("illegal operator");
                    }

                    break;
                }

                case DictionaryOperand dictOperand1:
                {
                    switch (operand2)
                    {
                        case DictionaryOperand dictOperand2:
                        {
                            ret = dictOperand1.Dictionary == dictOperand2.Dictionary;
                            break;
                        }

                        case NullOperand nullOperand:
                        {
                            ret = false;
                            break;
                        }

                        default:
                            throw new Exception("illegal operator");
                    }

                    break;
                }

                case NullOperand nullOperand1:
                {
                    switch (operand2)
                    {
                        case NullOperand nullOperand2:
                        {
                            ret = true;
                            break;
                        }

                        default:
                            ret = false;
                            break;
                    }

                    break;
                }

                default:
                    throw new Exception("illegal operator");
            }

            return ret;
        }

        /// <summary>
        /// Compares 2 operands whether the first is less than the second
        /// </summary>
        /// <param name="operand1"></param>
        /// <param name="operand2"></param>
        /// <returns>true if less</returns>
        public static bool IsLess(Operand operand1, Operand operand2)
        {
            bool ret;

            switch (operand1)
            {
                case StringOperand stringOperand1:
                {
                    switch (operand2)
                    {
                        case StringOperand stringOperand2:
                        {
                            ret = string.CompareOrdinal(stringOperand1.Value, stringOperand2.Value) < 0;
                            break;
                        }

                        default:
                            throw new Exception("illegal operator");
                    }

                    break;
                }

                case IntegerOperand intOperand1:
                {
                    switch (operand2)
                    {
                        case IntegerOperand intOperand2:
                        {
                            ret = intOperand1.Value < intOperand2.Value;
                            break;
                        }

                        case RealOperand realOperand2:
                        {
                            ret = intOperand1.Value < realOperand2.Value;
                            break;
                        }

                        default:
                            throw new Exception("illegal operator");
                    }

                    break;
                }

                case RealOperand realOperand1:
                {
                    switch (operand2)
                    {
                        case IntegerOperand intOperand2:
                        {
                            ret = realOperand1.Value < intOperand2.Value;
                            break;
                        }

                        case RealOperand realOperand2:
                        {
                            ret = realOperand1.Value < realOperand2.Value;
                            break;
                        }

                        default:
                            throw new Exception("illegal operator");
                    }

                    break;
                }

                default:
                    throw new Exception("illegal operator");
            }

            return ret;
        }

        /// <summary>
        /// Compares 2 operands whether the first is greater than the second
        /// </summary>
        /// <param name="operand1"></param>
        /// <param name="operand2"></param>
        /// <returns>true if greater</returns>
        public static bool IsGreater(Operand operand1, Operand operand2)
        {
            bool ret;

            switch (operand1)
            {
                case StringOperand stringOperand1:
                {
                    switch (operand2)
                    {
                        case StringOperand stringOperand2:
                        {
                            ret = string.CompareOrdinal(stringOperand1.Value, stringOperand2.Value) > 0;
                            break;
                        }

                        default:
                            throw new Exception("illegal operator");
                    }

                    break;
                }

                case IntegerOperand intOperand1:
                {
                    switch (operand2)
                    {
                        case IntegerOperand intOperand2:
                        {
                            ret = intOperand1.Value > intOperand2.Value;
                            break;
                        }

                        case RealOperand realOperand2:
                        {
                            ret = intOperand1.Value > realOperand2.Value;
                            break;
                        }

                        default:
                            throw new Exception("illegal operator");
                    }

                    break;
                }

                case RealOperand realOperand1:
                {
                    switch (operand2)
                    {
                        case IntegerOperand intOperand2:
                        {
                            ret = realOperand1.Value > intOperand2.Value;
                            break;
                        }

                        case RealOperand realOperand2:
                        {
                            ret = realOperand1.Value > realOperand2.Value;
                            break;
                        }

                        default:
                            throw new Exception("illegal operator");
                    }

                    break;
                }

                default:
                    throw new Exception("illegal operator");
            }

            return ret;
        }
    }
}
