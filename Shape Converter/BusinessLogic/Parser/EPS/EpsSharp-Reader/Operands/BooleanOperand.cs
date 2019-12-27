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
    /// Represents an boolean value
    /// </summary>
    internal class BooleanOperand : Operand
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public BooleanOperand() : base("booleantype")
        {
        }

        public BooleanOperand(bool boolean) : this()
        {
            Value = boolean;
        }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public bool Value { get; set; }

        /// <summary>
        /// Create a copy
        /// </summary>
        protected override Operand CreateCopy()
        {
            return new BooleanOperand(Value);
        }

        /// <summary>
        /// Returns a describing string for debugging purposes only
        /// </summary>
        public override string ToString()
        {
            return string.Format("bool:{0}", Value.ToString());
        }
    }
}
