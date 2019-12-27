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
using ShapeConverter.Parser.Pdf;

namespace ShapeConverter.BusinessLogic.Parser.Pdf.Pattern
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
        void Init(PdfDictionary dict);

        /// <summary>
        /// Gets a brush
        /// </summary>
        GraphicBrush GetBrush(Matrix matrix, PdfRect rect, double alpha, List<FunctionStop> softMask);
    }
}
