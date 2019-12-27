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

using System.Collections.Generic;
using EpsSharp.Eps.Core;
using EpsSharp.Eps.Helper;

namespace EpsSharp.Eps.Commands.Stack
{
    /// <summary>
    /// MarkCmd
    /// </summary>
    internal class RollCmd : CommandOperand
    {
        public override void Execute(EpsInterpreter interpreter)
        {
            var operandStack = interpreter.OperandStack;

            var motion = operandStack.PopIntegerValue();
            var count = operandStack.PopIntegerValue();

            if (motion == 0 || count == 0)
            {
                return;
            }

            var list = new List<Operand>();

            for (int i = 0; i < count; i++)
            {
                var operand = operandStack.Pop();
                list.Add(operand);
            }

            if (motion > 0)
            {
                var index = motion - 1;

                for (int i = 0; i < count; i++)
                {
                    if (index < 0)
                    {
                        index += count;
                    }

                    var operand = list[index];
                    operandStack.Push(operand);

                    index--;
                }
            }
            else
            {
                var index = -motion;

                for (int i = 0; i < count; i++)
                {
                    if (index >= count)
                    {
                        index -= count;
                    }

                    var operand = list[count - index - 1];
                    operandStack.Push(operand);

                    index++;
                }
            }
        }
    }
}
