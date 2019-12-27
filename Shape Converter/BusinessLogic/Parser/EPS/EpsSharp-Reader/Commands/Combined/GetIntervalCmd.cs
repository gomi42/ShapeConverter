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
    /// GetInterval
    /// </summary>
    internal class GetIntervalCmd : CommandOperand
    {
        public override void Execute(EpsInterpreter interpreter)
        {
            var operandStack = interpreter.OperandStack;

            var count = operandStack.PopIntegerValue();
            var index = operandStack.PopIntegerValue();
            var operand = operandStack.Pop();

            switch (operand)
            {
                case ArrayOperand arrayOperand:
                {
                    var subArray = new ArrayOperand();

                    for (int i = index; i < index + count; i++)
                    {
                        subArray.Values.Add(arrayOperand.Values[i]);
                    }

                    operandStack.Push(subArray);
                    break;
                }

                case StringOperand stringOperand:
                {
                    var str = new StringOperand(stringOperand, index, count);
                    operandStack.Push(str);
                    break;
                }
            }
        }
    }
}
