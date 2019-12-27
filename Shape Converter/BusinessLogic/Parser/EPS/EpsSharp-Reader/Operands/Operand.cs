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

namespace EpsSharp.Eps.Core
{
    /// <summary>
    /// Base class for all operands
    /// </summary>
    internal abstract class Operand
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public Operand(string typename)
        {
            TypeName = typename;
        }

        /// <summary>
        /// The operand is executable
        /// </summary>
        public bool IsExecutable { get; set; }

        /// <summary>
        /// If true execute the operator always even if the interpreter
        /// is collecting operands of a procedure
        /// </summary>
        public bool AlwaysExecute { get; set; }

        /// <summary>
        /// The type name according to the EPS spec of the operand
        /// </summary>
        public string TypeName { get; private set; }

        /// <summary>
        /// The line number the operands occurs in the source file
        /// </summary>
        public int LineNumber { get; set; }

        /// <summary>
        /// Execute the operator
        /// </summary>
        /// <param name="interpreter"></param>
        public virtual void Execute(EpsInterpreter interpreter)
        {
        }

        /// <summary>
        /// Perform a binding operation
        /// </summary>
        /// <param name="interpreter"></param>
        public virtual void Bind(EpsInterpreter interpreter)
        {
        }

        /// <summary>
        /// Copy the operand
        /// </summary>
        public Operand Copy()
        {
            var copy = CreateCopy();

            copy.TypeName = TypeName;
            copy.IsExecutable = IsExecutable;
            copy.AlwaysExecute = AlwaysExecute;
            copy.LineNumber = LineNumber;

            return copy;
        }

        /// <summary>
        /// Create a copy
        /// </summary>
        protected abstract Operand CreateCopy();

        /// <summary>
        /// Returns a describing string for debugging purposes only
        /// </summary>
        public override string ToString()
        {
            return $"type \"{TypeName}\"";
        }
    }
}
