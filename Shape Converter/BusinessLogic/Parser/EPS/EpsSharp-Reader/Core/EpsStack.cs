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
using System.Collections;
using System.Collections.Generic;

namespace EpsSharp.Eps.Core
{
    /// <summary>
    /// EpsStack
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EpsStack<T> : IEnumerable<T>
    {
        private List<T> list = new List<T>();

        /// <summary>
        /// Number of elements on the stack
        /// </summary>
        public int Count => list.Count;

        /// <summary>
        /// Gets the element at the given index
        /// </summary>
        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= Count)
                {
                    throw new Exception("Stack index out of range");
                }

                return list[index];
            }
        }

        /// <summary>
        /// Push the given element on top of the stack
        /// </summary>
        public void Push(T operand)
        {
            list.Add(operand);
        }

        /// <summary>
        /// Gets the top element and removes it from the stack
        /// </summary>
        public T Pop()
        {
            var lastIndex = list.Count - 1;
            var op = list[lastIndex];
            list.RemoveAt(lastIndex);

            return op;
        }

        /// <summary>
        /// Returns the top element
        /// </summary>
        public T Peek()
        {
            var lastIndex = list.Count - 1;
            var op = list[lastIndex];

            return op;
        }

        public void RemoveAt(int index)
        {
            list.RemoveAt(index);
        }

        /// <summary>
        /// Clear the stack
        /// </summary>
        public void Clear()
        {
            list.Clear();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
