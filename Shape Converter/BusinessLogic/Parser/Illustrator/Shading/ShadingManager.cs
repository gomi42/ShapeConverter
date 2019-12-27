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
using PdfSharp.Pdf.Content.Objects;
using ShapeConverter.Parser.Pdf;

namespace ShapeConverter.BusinessLogic.Parser.Pdf.Shading
{
    internal class ShadingManager
    {
        Dictionary<string, IShading> shadings;

        /// <summary>
        /// Init
        /// </summary>
        public void Init(PdfResources resourcesDict)
        {
            shadings = new Dictionary<string, IShading>();

            if (resourcesDict == null)
            {
                return;
            }

            var shadingsDict = resourcesDict.Shadings;

            if (shadingsDict == null)
            {
                return;
            }

            foreach (var name in shadingsDict.Elements.Keys)
            {
                var shadingDict = shadingsDict.Elements.GetDictionary(name);

                var fd = ReadShading(shadingDict);
                shadings.Add(name, fd);
            }
        }

        /// <summary>
        /// Read one shading
        /// </summary>
        public static IShading ReadShading(PdfDictionary shadingDict)
        {
            IShading shadingDescriptor = null;
            var shadingtype = shadingDict.Elements.GetInteger(PdfKeys.ShadingType);

            switch (shadingtype)
            {
                // linear
                case 2:
                {
                    shadingDescriptor = new LinearGradientShading();
                    break;
                }

                // radial
                case 3:
                {
                    shadingDescriptor = new RadialGradientShading();
                    break;
                }

                // function based
                case 1:

                // free-form Gouraud-shaded triangle mesh
                case 4:

                // Lattice-form Gouraud-shaded triangle mesh
                case 5:

                // coons path mesh
                case 6:

                // tensor-product path mesh
                case 7:
                default:
                {
                    shadingDescriptor = new UnknownShading();
                    break;
                }
            }

            shadingDescriptor.Init(shadingDict);

            return shadingDescriptor; 
        }

        /// <summary>
        /// Get an extended graphics state from the given state name
        /// </summary>
        public IShading GetShading(string shadingName)
        {
            return shadings[shadingName];
        }

        /// <summary>
        /// Get an extended graphics state from the given state name
        /// </summary>
        public IShading GetShading(CSequence operands)
        {
            var shadingName = operands[0] as CName;
            return shadings[shadingName.Name];
        }
    }
}
