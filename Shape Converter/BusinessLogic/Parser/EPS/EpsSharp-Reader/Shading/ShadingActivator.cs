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
using EpsSharp.Eps.Core;

namespace EpsSharp.Eps.Shading
{
    /// <summary>
    /// The shading activator
    /// </summary>
    internal static class ShadingActivator
    {
        /// <summary>
        /// Read one shading
        /// </summary>
        public static IShading CreateShading(EpsInterpreter interpreter, DictionaryOperand shadingDict)
        {
            IShading shadingDescriptor = null;
            var shadingtype = (IntegerOperand)shadingDict.Dictionary.Find(EpsKeys.ShadingType);

            switch (shadingtype.Value)
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

            shadingDescriptor.Init(interpreter, shadingDict);

            return shadingDescriptor;
        }
    }
}
