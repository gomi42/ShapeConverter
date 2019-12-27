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

using System.Collections.Generic;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;

namespace ShapeConverter.BusinessLogic.Parser.Pdf.XObject
{
    internal class XObjectManager
    {
        Dictionary<string, PdfDictionary> xObjects;

        /// <summary>
        /// Init
        /// Note: Compared to the ExtGStates we cannot precompile the XObjects
        /// to GraphicPaths. Just because the result depends on the settings 
        /// where the XObjects are used. The coordinates and the transparency
        /// of the parent influence the resulting coordinates and transparencies.
        /// </summary>
        public void Init(PdfResources resourcesDict)
        {
            xObjects = new Dictionary<string, PdfDictionary>();

            if (resourcesDict == null)
            {
                return;
            }

            var xObjectsDict = resourcesDict.XObjects;

            if (xObjectsDict == null)
            {
                return;
            }

            foreach (var name in xObjectsDict.Elements.Keys)
            {
                var xObjectDict = xObjectsDict.Elements.GetDictionary(name);
                xObjects.Add(name, xObjectDict);
            }

        }

        /// <summary>
        /// Get the XObject with the given  name
        /// </summary>
        public PdfDictionary GetXObject(string name)
        {
            return xObjects[name];
        }
    }
}
