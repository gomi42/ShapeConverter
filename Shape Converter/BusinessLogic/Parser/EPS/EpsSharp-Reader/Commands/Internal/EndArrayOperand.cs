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

namespace EpsSharp.Eps.Commands
{
    /// <summary>
    /// The end of array
    /// </summary>
    internal class EndArrayOperand : CommandOperand
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public EndArrayOperand()
        {
            Name = "]"; // for debugging purpose only
        }

        /// <summary>
        /// Execute the operand
        /// </summary>
        public override void Execute(EpsInterpreter interpreter)
        {
            var operandStack = interpreter.OperandStack;

            ArrayOperand array = new ArrayOperand();
            array.LineNumber = LineNumber;

            var op = operandStack.Pop();

            while (!(op is MarkOperand))
            {
                var proxy = new OperandProxy(op);
                array.Values.Add(proxy);
                op = operandStack.Pop();
            }

            array.Values.Reverse();
            operandStack.Push(array);
        }
    }
}
