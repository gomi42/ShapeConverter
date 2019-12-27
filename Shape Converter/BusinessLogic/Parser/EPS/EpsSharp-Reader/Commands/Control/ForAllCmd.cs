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

namespace EpsSharp.Eps.Commands.Control
{
    /// <summary>
    /// ForAll
    /// </summary>
    internal class ForAllCmd : CommandOperand
    {
        public override void Execute(EpsInterpreter interpreter)
        {
            var operandStack = interpreter.OperandStack;

            var proc = operandStack.Pop();
            var operand = operandStack.Pop();
            interpreter.ResetExitCurrentLoop();

            switch (operand)
            {
                case DictionaryOperand dictionaryOperand:
                {
                    foreach (var kv in dictionaryOperand.Dictionary)
                    {
                        operandStack.Push(kv.Key);
                        operandStack.Push(kv.Value);
                        interpreter.Execute(proc);

                        if (interpreter.BreakCurrentLoop)
                        {
                            break;
                        }
                    }

                    break;
                }

                case ArrayOperand arrayOperand:
                {
                    foreach (var ao in arrayOperand.Values)
                    {
                        operandStack.Push(ao.Operand);
                        interpreter.Execute(proc);

                        if (interpreter.BreakCurrentLoop)
                        {
                            break;
                        }
                    }

                    break;
                }

                case StringOperand stringOperand:
                {
                    foreach (var ch in stringOperand.Value)
                    {
                        operandStack.Push(new IntegerOperand((int)ch));
                        interpreter.Execute(proc);

                        if (interpreter.BreakCurrentLoop)
                        {
                            break;
                        }
                    }

                    break;
                }
            }

            interpreter.ResetExitCurrentLoop();
        }
    }
}
