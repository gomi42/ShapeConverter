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

namespace EpsSharp.Eps.Commands.Internal
{
    /// <summary>
    /// Begin of procedure
    /// </summary>
    internal class BeginProcedureOperand : AlwaysExecuteOperand
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public BeginProcedureOperand()
        {
            Name = "{"; // for debugging purpose only
        }

        /// <summary>
        /// The procedure creation nesting level
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// Execute the operand
        /// </summary>
        public override void Execute(EpsInterpreter interpreter)
        {
            interpreter.EnterProcedureCreation();

            this.LineNumber = LineNumber;
            Level = interpreter.ProcedureCreationLevel;

            // The postscript spec suggests to push an array mark
            // onto the stack. By doing so I don't see a way to 
            // distinguish the beginning of an array and the beginning
            // of a procdedure when the procedure contains an array.
            // On the other hand I don't see the real need to push
            // an array marker onto the stack. It cannot be found by
            // the marker operands (e.g. cleartomark) because the 
            // beginning of a procedure turns off the execution of 
            // operators. In other words operators don't run after
            // this token. That's why I use a special operand.

            interpreter.OperandStack.Push(this);
        }
    }
}
