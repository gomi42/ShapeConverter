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

namespace EpsSharp.Eps.Core
{
    /// <summary>
    /// Represents the base class for command operators.
    /// </summary>
    internal abstract class CommandOperand : Operand
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public CommandOperand() : base("operatortype")
        {
            IsExecutable = true;
        }

        /// <summary>
        /// Gets or sets the name of the operator
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Create a copy
        /// </summary>
        protected override Operand CreateCopy()
        {
            // derived classes usually don't do special things in order
            // to create a copy, create a new instance of the object in a general
            // way, exceptions still can override CreateCopy()
            var copy = (CommandOperand)Activator.CreateInstance(GetType());
            copy.Name = Name;

            return copy;
        }

        /// <summary>
        /// Returns a describing string for debugging purposes only
        /// </summary>
        public override string ToString()
        {
            return string.Format("operator:{0}", Name);
        }
    }
}
