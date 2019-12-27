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
using EpsSharp.Eps.Helper;

namespace EpsSharp.Eps.Commands
{
    /// <summary>
    /// The operator operand serves as an indirect reference to either a build-in operand
    /// or an operand defined at runtime. We need this indirection because not-bound 
    /// operands are evaluated each time at the time when they are executed. And the 
    /// environment might change between several executions.
    /// </summary>
    internal class OperatorOperand : CommandOperand
    {
        /// <summary>
        /// The bound operand if set otherwise null
        /// </summary>
        protected Operand BoundOperand { get; set; }

        /// <summary>
        /// Execute the operand
        /// </summary>
        public override void Execute(EpsInterpreter interpreter)
        {
            if (interpreter.BreakCurrentLoop)
            {
                return;
            }

            if (BoundOperand != null)
            {
                interpreter.Execute(BoundOperand);
            }
            else
            {
                var name = new NameOperand { Value = this.Name };
                var op = DictionaryStackHelper.FindValue(interpreter.DictionaryStack, name);

                if (op != null)
                {
                    interpreter.Execute(op);
                }
                else
                {
                    // Throwing an exception when the interpreter is within a stopped context
                    // would also be ok and is the prefered behavior. We test for the stopped
                    // context here to make debugging a bit easier. So that the debugger
                    // doesn't stop when the "exception" is handled in the postscript script,
                    // e.g. when tests are made if a command is available.

                    if (!interpreter.IsInStoppedContext)
                    {
                        throw new Exception($"unknown command \"{Name}\"");
                    }

                    interpreter.StopProcedure();
                }
            }
        }

        /// <summary>
        /// Bind the operand
        /// </summary>
        public override void Bind(EpsInterpreter interpreter)
        {
            var name = new NameOperand { Value = this.Name };
            BoundOperand = DictionaryStackHelper.FindValue(interpreter.DictionaryStack, name);
        }

        /// <summary>
        /// Create a clone
        /// </summary>
        protected override Operand CreateCopy()
        {
            var clone = new OperatorOperand();
            clone.Name = Name;
            clone.BoundOperand = BoundOperand;

            return this;
        }
    }
}
