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

namespace ShapeConverter.BusinessLogic.Generators.GeometrySourceGenerator
{
    /// <summary>
    /// Defines the aspect to normalize
    /// </summary>
    public enum NormalizeGeometrySourceAspect
    {
        /// <summary>
        /// Normalize width
        /// </summary>
        Width,

        /// <summary>
        /// Normalize height
        /// </summary>
        Height,

        /// <summary>
        /// Normalize width and height individually
        /// </summary>
        Individual
    }

    /// <summary>
    /// Geometry source code generator interface
    /// </summary>
    interface IGeometrySourceGenerator
    {
        /// <summary>
        /// Specifies which aspect to normalize
        /// </summary>
        NormalizeGeometrySourceAspect NormalizeAspect { get; set; }

        /// <summary>
        /// Specifies whether to include an offset for all coordinates in the generated code
        /// </summary>
        bool IncludeOffset { get; set; }

        /// <summary>
        /// The filename to include in the generated source code
        /// </summary>
        string Filename { get; set; }

        /// <summary>
        /// The source code generator engine
        /// </summary>
        string GenerateSource(GraphicVisual drawing);
    }
}
