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

namespace EpsSharp.Eps.Commands.Control
{
    /// <summary>
    /// For
    /// </summary>
    internal class ForCmd : CommandOperand
    {
        public override void Execute(EpsInterpreter interpreter)
        {
            var operandStack = interpreter.OperandStack;

            var proc = operandStack.Pop();
            var limit = operandStack.Pop();
            var increment = operandStack.Pop();
            var initial = operandStack.Pop();
            interpreter.ResetExitCurrentLoop();

            if (initial is IntegerOperand intInitial && increment is IntegerOperand intIncrement && limit is IntegerOperand intLimit)
            {
                ExecuteInt(interpreter, intInitial.Value, intIncrement.Value, intLimit.Value, proc);
            }
            else
            {
                var realLimit = OperandHelper.GetRealValue(limit);
                var realIncrement = OperandHelper.GetRealValue(increment);
                var realInitial = OperandHelper.GetRealValue(initial);

                ExecuteReal(interpreter, realInitial, realLimit, realIncrement, proc);
            }

            interpreter.ResetExitCurrentLoop();
        }

        private void ExecuteInt(EpsInterpreter interpreter, int inital, int increment, int limit, Operand proc)
        {
            var operandStack = interpreter.OperandStack;

            if (increment > 0)
            {
                for (int i = inital; i <= limit; i += increment)
                {
                    operandStack.Push(new IntegerOperand(i));
                    interpreter.Execute(proc);

                    if (interpreter.BreakCurrentLoop)
                    {
                        break;
                    }
                }
            }
            else
            if (increment < 0)
            {
                for (int i = inital; i >= limit; i -= increment)
                {
                    operandStack.Push(new IntegerOperand(i));
                    interpreter.Execute(proc);

                    if (interpreter.BreakCurrentLoop)
                    {
                        break;
                    }
                }
            }
        }

        private void ExecuteReal(EpsInterpreter interpreter, double inital, double increment, double limit, Operand proc)
        {
            var operandStack = interpreter.OperandStack;

            if (increment > 0)
            {
                for (double d = inital; d <= limit; d += increment)
                {
                    operandStack.Push(new RealOperand(d));
                    interpreter.Execute(proc);

                    if (interpreter.BreakCurrentLoop)
                    {
                        break;
                    }
                }
            }
            else
            if (increment < 0)
            {
                for (double d = inital; d >= limit; d -= increment)
                {
                    operandStack.Push(new RealOperand(d));
                    interpreter.Execute(proc);

                    if (interpreter.BreakCurrentLoop)
                    {
                        break;
                    }
                }
            }
        }
    }
}
