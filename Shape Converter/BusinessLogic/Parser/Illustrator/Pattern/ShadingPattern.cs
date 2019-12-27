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
using System.Windows.Media;
using PdfSharp.Pdf;
using ShapeConverter.BusinessLogic.Parser.Pdf.Function;
using ShapeConverter.BusinessLogic.Parser.Pdf.Shading;
using ShapeConverter.Parser.Pdf;

namespace ShapeConverter.BusinessLogic.Parser.Pdf.Pattern
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
        public void Init(PdfDictionary patternDict)
        {
            var shadingDict = patternDict.Elements.GetDictionary(PdfKeys.Shading);
            shading = ShadingManager.ReadShading(shadingDict);

            var matrixArray = patternDict.Elements.GetArray(PdfKeys.Matrix);

            if (matrixArray != null)
            {
                matrix = PdfUtilities.GetMatrix(matrixArray);
            }
            else
            {
                matrix = Matrix.Identity;
            }

            // I think we don't need to handle the extGState here
            // because it can't change anything we are interested in
            var extGStateDict = patternDict.Elements.GetDictionary(PdfKeys.ExtGState);
        }

        /// <summary>
        /// Get a brush
        /// </summary>
        public GraphicBrush GetBrush(Matrix parentMatrix, PdfRect rect, double alpha, List<FunctionStop> softMask)
        {
            return shading.GetBrush(matrix * parentMatrix, rect, alpha, softMask);
        }
    }
}
