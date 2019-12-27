﻿//
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

namespace EpsSharp.Eps.Commands.Arithmetic
{
    /// <summary>
    /// Round
    /// </summary>
    internal class RoundCmd : CommandOperand
    {
        public override void Execute(EpsInterpreter interpreter)
        {
            var operandStack = interpreter.OperandStack;
            var operand = operandStack.Pop();

            if (operand is IntegerOperand integer)
            {
                operandStack.Push(operand);
            }
            else
            {
                var real = (RealOperand)operand;
                var res = new RealOperand();
                res.Value = Math.Round(real.Value);
                operandStack.Push(res);
            }
        }
    }
}
