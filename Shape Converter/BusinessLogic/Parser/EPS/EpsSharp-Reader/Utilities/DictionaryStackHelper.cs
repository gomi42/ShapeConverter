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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EpsSharp.Eps.Core;

namespace EpsSharp.Eps.Helper
{
    /// <summary>
    /// Helpers for a DictionaryStack
    /// </summary>
    internal static class DictionaryStackHelper
    {
        /// <summary>
        /// Find a key in the dictionary stack from top to bottom
        /// </summary>
        /// <param name="dictionaryStack"></param>
        /// <param name="key"></param>
        /// <returns>the found operand or null</returns>
        public static Operand FindValue(EpsStack<EpsDictionary> dictionaryStack, Operand key)
        {
            for (int i = dictionaryStack.Count - 1; i >= 0; i--)
            {
                var dict = dictionaryStack[i];
                var op = dict.TryFind(key);

                if (op != null)
                {
                    return op;
                }
            }

            return null;
        }

        /// <summary>
        /// Find the dictionary within the dictionary stack that holds the given key
        /// </summary>
        /// <param name="dictionaryStack"></param>
        /// <param name="key"></param>
        /// <returns>the found dictionary or null</returns>
        public static EpsDictionary FindDictionary(EpsStack<EpsDictionary> dictionaryStack, Operand key)
        {
            for (int i = dictionaryStack.Count - 1; i >= 0; i--)
            {
                var dict = dictionaryStack[i];
                var op = dict.TryFind(key);

                if (op != null)
                {
                    return dict;
                }
            }

            return null;
        }

        /// <summary>
        /// Remove all non-permanet dictionaries from the dictionary stack
        /// </summary>
        /// <param name="dictionaryStack"></param>
        public static void RemoveNonPermanentDictionaries(EpsStack<EpsDictionary> dictionaryStack)
        {
            int i = 0;

            while (i < dictionaryStack.Count)
            {
                var dict = dictionaryStack[i];

                if (!dict.IsPermanent)
                {
                    dictionaryStack.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }
        }
    }
}
