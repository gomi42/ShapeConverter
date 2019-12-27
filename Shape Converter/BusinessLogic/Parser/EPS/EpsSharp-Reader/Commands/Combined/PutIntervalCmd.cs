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
    internal class PutIntervalCmd : CommandOperand
    {
        public override void Execute(EpsInterpreter interpreter)
        {
            var operandStack = interpreter.OperandStack;

            var operand2 = operandStack.Pop();
            var index = operandStack.PopIntegerValue();
            var operand1 = operandStack.Pop();

            switch (operand2)
            {
                case ArrayOperand arrayOperand2:
                {
                    var arrayOperand1 = (ArrayOperand)operand1;
                    int i = index;

                    foreach (var operand in arrayOperand2.Values)
                    {
                        arrayOperand1.Values[i++] = operand;
                    }

                    break;
                }

                case StringOperand stringOperand2:
                {
                    var stringOperand1 = (StringOperand)operand1;
                    stringOperand1.ReplaceSubString(index, stringOperand2.Value);
                    
                    break;
                }
            }
        }
    }
}
