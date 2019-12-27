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

namespace EpsSharp.Eps.Core
{
    /// <summary>
    /// The operand proxy
    /// </summary>
    internal class OperandProxy
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public OperandProxy(Operand operand)
        {
            Operand = operand;
        }

        public Operand Operand { get; set; }

        /// <summary>
        /// Returns a describing string for debugging purposes only
        /// </summary>
        public override string ToString()
        {
            return Operand.ToString();
        }
    }

    /// <summary>
    /// Represents an array of operands
    /// </summary>
    internal class ArrayOperand : Operand
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ArrayOperand() : base("arraytype")
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public ArrayOperand(string typename) : base(typename)
        {
        }

        // We need to use the OperandProxy here. There is a command that extracts
        // parts of the array into another array. Changes made in one array must
        // be visible in the other array in both directions. Changes include 
        // replacing the operand with a whole new (different) operand.
        public List<OperandProxy> Values { get; private set; } = new List<OperandProxy>();

        /// <summary>
        /// Execute the operands
        /// </summary>
        /// <param name="interpreter"></param>
        public override void Execute(EpsInterpreter interpreter)
        {
            if (!IsExecutable || interpreter.BreakCurrentLoop)
            {
                return;
            }

            foreach (var op in Values)
            {
                interpreter.Execute(op.Operand);

                if (interpreter.BreakCurrentLoop)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Bind the operators
        /// </summary>
        /// <param name="interpreter"></param>
        public override void Bind(EpsInterpreter interpreter)
        {
            foreach (var op in Values)
            {
                op.Operand.Bind(interpreter);
            }
        }

        /// <summary>
        /// Create a copy to decouple execution flags from the original object
        /// </summary>
        protected override Operand CreateCopy()
        {
            var copy = new ArrayOperand();
            copy.Values = Values;

            return copy;
        }

        /// <summary>
        /// Returns a describing string for debugging purposes only
        /// </summary>
        public override string ToString()
        {
            return string.Format("{0}:{1}", IsExecutable ? "proc" : "array", Values.Count.ToString());
        }
    }

    /// <summary>
    /// The packed array
    /// </summary>
    internal class PackedArrayOperand : ArrayOperand
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public PackedArrayOperand() : base("packedarraytype")
        {
        }
    }
}
