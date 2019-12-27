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

using EpsSharp.Eps.Helper;

namespace EpsSharp.Eps.Core
{
    /// <summary>
    /// Represents a name.
    /// </summary>
    internal class NameOperand : Operand
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public NameOperand() : base("nametype")
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public NameOperand(string name) : this()
        {
            Value = name;
        }

        /// <summary>
        /// Gets or sets the name
        /// The name must not contain the leading slash
        /// which is just the syntax marker in the script
        /// </summary>
        public string Value { get; set; }

        public override void Execute(EpsInterpreter interpreter)
        {
            if (!IsExecutable)
            {
                interpreter.OperandStack.Push(Copy());
            }
            else
            {
                var name = new NameOperand(this.Value);
                var op = DictionaryStackHelper.FindValue(interpreter.DictionaryStack, name);

                if (op != null)
                {
                    interpreter.Execute(op);
                }
            }
        }

        /// <summary>
        /// We need to copy the Name because the executable flag can be changed
        /// locally on the stack but must not be changed at the orignal name
        /// (located in a procedure)
        /// </summary>
        protected override Operand CreateCopy()
        {
            return new NameOperand(Value);
        }

        /// <summary>
        /// Returns a describing string for debugging purposes only
        /// </summary>
        public override string ToString()
        {
            return $"name:{Value}";
        }
    }
}
