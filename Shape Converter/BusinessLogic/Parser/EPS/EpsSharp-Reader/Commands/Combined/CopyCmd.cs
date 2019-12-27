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
using EpsSharp.Eps.Helper;

namespace EpsSharp.Eps.Commands.Combined
{
    /// <summary>
    /// Copy
    /// </summary>
    internal class CopyCmd : CommandOperand
    {
        public override void Execute(EpsInterpreter interpreter)
        {
            var operandStack = interpreter.OperandStack;

            var operand2 = operandStack.Pop();

            switch (operand2)
            {
                case IntegerOperand integerOperand:
                {
                    var count = integerOperand.Value;

                    if (count <= 0)
                    {
                        return;
                    }

                    var index = operandStack.Count - count;

                    for (int i = 0; i < count; i++, index++)
                    {
                        var operand = operandStack[index];
                        operandStack.Push(operand.Copy());
                    }

                    break;
                }

                case DictionaryOperand dict2:
                {
                    var dict1 = operandStack.PopDictionary();
                    dict2.Dictionary.Clear();
                    dict2.Dictionary.Add(dict1.Dictionary);
                    operandStack.Push(dict2);
                    break;
                }

                case ArrayOperand array2:
                {
                    var array1 = operandStack.PopArray();
                    array2.Values.Clear();
                    array2.Values.AddRange(array1.Values);
                    operandStack.Push(array2);
                    break;
                }

                case StringOperand string2:
                {
                    var string1 = operandStack.PopString();
                    string1.CopyTo(string2);
                    operandStack.Push(string2);
                    break;
                }

                case GraphicStateOperand graphicStateOperand:
                {

                    break;
                }
            }
        }
    }
}
