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
    /// The string type
    /// </summary>
    internal enum StringType
    {
        Standard,
        Hex,
        AsciiBase85
    }

    /// <summary>
    /// The string operand
    /// </summary>
    internal class StringOperand : Operand
    {
        /// <summary>
        /// The string proxy
        /// </summary>
        class StringProxy
        {
            public string Value { get; set; }
        }

        private bool isOriginalValue;
        private StringProxy stringProxy;
        private int startIndex;

        /// <summary>
        /// Constructor
        /// </summary>
        public StringOperand() : base("stringtype")
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public StringOperand(string str) : this()
        {
            Value = str;
        }

        /// <summary>
        /// Construct a substring
        /// </summary>
        public StringOperand(StringOperand originalOperand, int startIndex, int length) : this()
        {
            isOriginalValue = false;
            stringProxy = originalOperand.stringProxy;
            this.startIndex = originalOperand.startIndex + startIndex;
            Length = length;
        }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public string Value 
        {
            get
            {
                if (!isOriginalValue)
                {
                    return stringProxy.Value.Substring(startIndex, Length);
                }

                return stringProxy.Value;
            }

            set
            {
                isOriginalValue = true;
                stringProxy = new StringProxy { Value = value };
                startIndex = 0;
                Length = value.Length;
            }
        }

        /// <summary>
        /// The length of the string
        /// </summary>
        public int Length { get; private set; }
        
        /// <summary>
        /// Get or sets the type of the string
        /// </summary>
        public StringType Type { get; set; }

        /// <summary>
        /// Replace the substring starting at index with the substring
        /// </summary>
        public void ReplaceSubString(int index, string newSubString)
        {
            index += startIndex;
            var pre = stringProxy.Value.Substring(0, index);
            var startPostIndex = index + newSubString.Length;
            var post = stringProxy.Value.Substring(startPostIndex, stringProxy.Value.Length - startPostIndex);
            stringProxy.Value = pre + newSubString + post;
        }

        /// <summary>
        /// CopyTo
        /// </summary>
        public void CopyTo(StringOperand copy)
        {
            copy.Type = Type;
            copy.isOriginalValue = isOriginalValue;
            copy.stringProxy = stringProxy;
            copy.startIndex = startIndex;
            copy.Length = Length;
        }

        /// <summary>
        /// Create a copy
        /// </summary>
        protected override Operand CreateCopy()
        {
            var copy = new StringOperand();
            CopyTo(copy);

            return copy;
        }

        /// <summary>
        /// Returns a describing string for debugging purposes only
        /// </summary>
        public override string ToString()
        {
            return string.Format("string:{0}", Value);
        }
    }
}
