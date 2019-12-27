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

using System.Globalization;

namespace EpsSharp.Eps.Core
{
    /// <summary>
    /// Represents a real value
    /// </summary>
    internal class RealOperand : Operand
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public RealOperand() : base("realtype")
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public RealOperand(double real) : this()
        {
            Value = real;
        }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public double Value { get; set; }

        /// <summary>
        /// Create a copy
        /// </summary>
        protected override Operand CreateCopy()
        {
            return new RealOperand(Value);
        }

        /// <summary>
        /// Returns a describing string for debugging purposes only
        /// </summary>
        public override string ToString()
        {
            return string.Format("real:{0}", Value.ToString(CultureInfo.InvariantCulture));
        }
    }
}
