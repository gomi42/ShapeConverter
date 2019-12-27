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
using System.Windows.Media;
using EpsSharp.Eps.Core;
using EpsSharp.Eps.Function;
using ShapeConverter;

namespace EpsSharp.Eps.Pattern
{
    /// <summary>
    /// The IPattern interface
    /// </summary>
    internal interface IPattern
    {
        /// <summary>
        /// Gets the color precision
        /// </summary>
        GraphicColorPrecision ColorPrecision { get; }

        /// <summary>
        /// Init
        /// </summary>
        void Init(EpsInterpreter interpreter, DictionaryOperand dict);

        /// <summary>
        /// Gets a brush
        /// </summary>
        GraphicBrush GetBrush(Matrix matrix, EpsRect rect);
    }
}
