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

namespace EpsSharp.Eps.Commands.Arithmetic
{
    /// <summary>
    /// Mul
    /// </summary>
    internal class MulCmd : CommandOperand
    {
        public override void Execute(EpsInterpreter interpreter)
        {
            var operandStack = interpreter.OperandStack;

            var operand2 = operandStack.Pop();
            var operand1 = operandStack.Pop();

            var integer1 = operand1 as IntegerOperand;
            var integer2 = operand2 as IntegerOperand;

            if (integer1 != null && integer2 != null)
            {
                var res = new IntegerOperand();
                res.Value = integer1.Value * integer2.Value;
                operandStack.Push(res);
            }
            else
            {
                var double1 = OperandHelper.GetRealValue(operand1);
                var double2 = OperandHelper.GetRealValue(operand2);

                var res = new RealOperand();
                res.Value = double1 * double2;
                operandStack.Push(res);
            }
        }
    }
}
