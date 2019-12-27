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
using EpsSharp.Eps.Helper;

namespace EpsSharp.Eps.Core
{
    /// <summary>
    /// The key/value pair stored in the EpsDictionary
    /// </summary>
    internal class KeyValue
    {
        public Operand Key { get; set; }

        public Operand Value { get; set; }
    }

    /// <summary>
    /// PsDictionary
    /// </summary>
    internal class EpsDictionary : IEnumerable<KeyValue>
    {
        private List<KeyValue> list = new List<KeyValue>();

        /// <summary>
        /// True if the dictionary is permanent
        /// </summary>
        public bool IsPermanent { get; set; }

        /// <summary>
        /// Number of elements on the dictionary
        /// </summary>
        public int Count => list.Count;

        /// <summary>
        /// Clear the dictionary
        /// </summary>
        public void Clear()
        {
            list.Clear();
        }

        /// <summary>
        /// Add an entry to the dictionary
        /// </summary>
        public void Add(Operand key, Operand value)
        {
            var foundIndex = FindKeyValue(key);

            if (foundIndex >= 0)
            {
                var existingKey = list[foundIndex];
                existingKey.Value = value;
            }
            else
            {
                var newKv = new KeyValue { Key = key, Value = value };
                list.Add(newKv);
            }
        }

        /// <summary>
        /// Add elements from a given dict to this dict
        /// </summary>
        /// <param name="dict"></param>
        public void Add(EpsDictionary dict)
        {
            foreach (var kv in dict.list)
            {
                list.Add(kv);
            }
        }

        /// <summary>
        /// Removes the given key
        /// </summary>
        public void Remove(Operand key)
        {
            var foundIndex = FindKeyValue(key);

            if (foundIndex >= 0)
            {
                list.RemoveAt(foundIndex);
            }
        }

        /// <summary>
        /// Tests whether the dictionary contains the given key
        /// </summary>
        public bool Contains(Operand key)
        {
            var foundIndex = FindKeyValue(key);

            return foundIndex >= 0 ? true : false;
        }

        /// <summary>
        /// Find a key and return its value
        /// Return null if the key is not found
        /// </summary>
        public Operand TryFind(string key)
        {
            var keyOp = new NameOperand { Value = key };
            return TryFind(keyOp);
        }

        /// <summary>
        /// Find a key and return its value
        /// Throw an exception if the key is not found
        /// </summary>
        public Operand Find(Operand key)
        {
            var value = TryFind(key);

            if (value == null)
            {
                throw new Exception("key not found");
            }

            return value;
        }

        /// <summary>
        /// Find a key and return its value
        /// Return null if the key is not found
        /// </summary>
        public Operand TryFind(Operand key)
        {
            Operand ret = null;

            var foundIndex = FindKeyValue(key);

            if (foundIndex >= 0)
            {
                var existingKey = list[foundIndex];
                ret = existingKey.Value;
            }

            return ret;
        }

        /// <summary>
        /// Find a key and return its key/value pair
        /// </summary>
        private int FindKeyValue(Operand key)
        {
            int ret = -1;

            for (int i = 0; i < list.Count; i++)
            {
                KeyValue kv = list[i];

                if (OperandComparator.IsEqual(key, kv.Key))
                {
                    ret = i;
                    break;
                }
            }

            return ret;
        }

        /// <summary>
        /// GetEnumerator
        /// </summary>
        public IEnumerator<KeyValue> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        /// <summary>
        /// GetEnumerator
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        // some convenience methods to increase readability of the using code

        /// <summary>
        /// Find a key and return its value
        /// Throw an exception if the key is not found
        /// </summary>
        public Operand Find(string key)
        {
            var keyOp = new NameOperand { Value = key };
            return Find(keyOp);
        }

        /// <summary>
        /// Find a key and return its value as an integer operand.
        /// Throw an exception if the key is not found
        /// </summary>
        public IntegerOperand GetInteger(string key)
        {
            return (IntegerOperand)Find(key);
        }

        /// <summary>
        /// Find a key and return its value as a string operand.
        /// Throw an exception if the key is not found
        /// </summary>
        public StringOperand GetString(string key)
        {
            return (StringOperand)Find(key);
        }

        /// <summary>
        /// Find a key and return its value as an array operand.
        /// Throw an exception if the key is not found
        /// </summary>
        public ArrayOperand GetArray(string key)
        {
            return (ArrayOperand)Find(key);
        }

        /// <summary>
        /// Find a key and return its value as a Dictionary operand.
        /// Throw an exception if the key is not found
        /// </summary>
        public DictionaryOperand GetDictionary(string key)
        {
            return (DictionaryOperand)Find(key);
        }
    }
}
