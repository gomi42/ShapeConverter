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

namespace EpsSharp.Eps.Commands.Boolean
{
    /// <summary>
    /// And
    /// </summary>
    internal class AndCmd : CommandOperand
    {
        public override void Execute(EpsInterpreter interpreter)
        {
            var operandStack = interpreter.OperandStack;

            var operand1 = operandStack.Pop();
            var operand2 = operandStack.Pop();

            switch (operand1)
            {
                case BooleanOperand boolOperand1:
                {
                    var boolOperand2 = (BooleanOperand)operand2;
                    var boolean = new BooleanOperand(boolOperand1.Value && boolOperand2.Value);
                    operandStack.Push(boolean);
                    break;
                }

                case IntegerOperand intOperand1:
                {
                    var intOperand2 = (IntegerOperand)operand2;
                    var boolean = new IntegerOperand(intOperand1.Value & intOperand2.Value);
                    operandStack.Push(boolean);
                    break;
                }

                default:
                    throw new Exception("illegal operator");
            }
        }
    }
}
