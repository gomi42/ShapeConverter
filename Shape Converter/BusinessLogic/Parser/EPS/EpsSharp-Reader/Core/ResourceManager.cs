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
    /// A single resource entry
    /// </summary>
    internal class ResourceEntry
    {
        public string Category { get; set; }

        public Operand Key { get; set; }

        public Operand Instance { get; set; }
    }

    /// <summary>
    /// The resource manager
    /// </summary>
    internal class ResourceManager
    {
        private Dictionary<string, EpsDictionary> categories = new Dictionary<string, EpsDictionary>();

        /// <summary>
        /// Define a resource
        /// </summary>
        public void DefineResource(string category, Operand key, Operand resource)
        {
            EpsDictionary categoryDict;

            if (!categories.TryGetValue(category, out categoryDict))
            {
                categoryDict = new EpsDictionary();
                categories.Add(category, categoryDict);
            }

            categoryDict.Add(key, resource);
        }

        /// <summary>
        /// Remove the given resource
        /// </summary>
        /// <param name="category"></param>
        /// <param name="key"></param>
        public void UndefineResource(string category, Operand key)
        {
            var categoryDict = categories[category];
            categoryDict.Remove(key);
        }

        /// <summary>
        /// Return the specified resource
        /// </summary>
        public Operand FindResource(string category, Operand key)
        {
            var categoryDict = categories[category];
            var resource = categoryDict.Find(key);

            return resource;
        }

        /// <summary>
        /// Return the specified resource or null if it doesn't exist
        /// </summary>
        public Operand TryFindResource(string category, Operand key)
        {
            Operand resource = null;
            EpsDictionary categoryDict;

            if (categories.TryGetValue(category, out categoryDict))
            {
                resource = categoryDict.TryFind(key);
            }

            return resource;
        }

        /// <summary>
        /// Get a filtered list of the given category.
        /// For the time being we ignore the filter (no crucial feature)
        /// </summary>
        /// <param name="category"></param>
        /// <param name="template"></param>
        /// <returns></returns>
        public List<Operand> GetFilteredResources(string category, string template)
        {
            var list = new List<Operand>();
            var categoryDict = categories[category];

            foreach (var entry in categoryDict)
            {
                //if (keyName != null) // add template filter here
                {
                    list.Add(entry.Key);
                }
            }

            return list;
        }

#if NEEDED
        /// <summary>
        /// Gets an enumerator
        /// Experimental version, most likely this will be done differently:
        /// Add a filter method for the ResourceForAll command and return
        /// a list of results (ResourceEntry?) that can be enumerated.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<ResourceEntry> GetEnumerator()
        {
            return new ResourceEnumerator(categories);
        }

        /// <summary>
        /// The private resource enumerator
        /// </summary>
        private class ResourceEnumerator : IEnumerator<ResourceEntry>
        {
            bool isInit;
            string currentCategory;
            Dictionary<string, EpsDictionary> resources;
            Dictionary<string, EpsDictionary>.Enumerator categoryEnumerator;
            IEnumerator<KeyValue> keyEnumerator;

            public ResourceEnumerator(Dictionary<string, EpsDictionary> resources)
            {
                this.resources = resources;
            }

            public ResourceEntry Current { get; private set; }

            object IEnumerator.Current => Current;

            public bool MoveNext()
            {
                bool valid;

                if (!isInit)
                {
                    isInit = true;
                    categoryEnumerator = resources.GetEnumerator();
                    valid = categoryEnumerator.MoveNext();

                    if (!valid)
                    {
                        return false;
                    }

                    var pair = categoryEnumerator.Current;
                    currentCategory = pair.Key;
                    keyEnumerator = pair.Value.GetEnumerator();
                }

                valid = keyEnumerator.MoveNext();

                if (!valid)
                {
                    valid = categoryEnumerator.MoveNext();

                    if (!valid)
                    {
                        return false;
                    }

                    var pair = categoryEnumerator.Current;
                    keyEnumerator = pair.Value.GetEnumerator();

                    valid = keyEnumerator.MoveNext();
                }

                if (valid)
                {
                    var pair = keyEnumerator.Current;

                    var resourceEntry = new ResourceEntry();

                    resourceEntry.Category = currentCategory;
                    resourceEntry.Key = pair.Key;
                    resourceEntry.Instance = pair.Value;

                    Current = resourceEntry;
                }

                return valid;
            }

            public void Reset()
            {
                isInit = false;
            }

            public void Dispose()
            {
            }
        }
#endif
    }
}
