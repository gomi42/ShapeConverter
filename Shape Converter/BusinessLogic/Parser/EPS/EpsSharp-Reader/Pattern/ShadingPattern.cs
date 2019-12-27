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
using EpsSharp.Eps.Helper;
using EpsSharp.Eps.Shading;
using ShapeConverter;

namespace EpsSharp.Eps.Pattern
{
    /// <summary>
    /// The shading pattern
    /// </summary>
    internal class ShadingPattern : IPattern
    {
        private IShading shading;
        private Matrix matrix;

        /// <summary>
        /// The color precision
        /// </summary>
        public GraphicColorPrecision ColorPrecision => shading.ColorPrecision;

        /// <summary>
        /// init
        /// </summary>
        public void Init(EpsInterpreter interpreter, DictionaryOperand patternDict)
        {
            var shadingDict = patternDict.Dictionary.GetDictionary(EpsKeys.Shading);
            shading = ShadingActivator.CreateShading(interpreter, shadingDict);

            var matrixArray = patternDict.Dictionary.GetArray(EpsKeys.Matrix);
            matrix = OperandHelper.GetMatrix(matrixArray);
        }

        /// <summary>
        /// Get a brush
        /// </summary>
        public GraphicBrush GetBrush(Matrix parentMatrix, EpsRect rect)
        {
            return shading.GetBrush(matrix * parentMatrix, rect);
        }
    }
}
