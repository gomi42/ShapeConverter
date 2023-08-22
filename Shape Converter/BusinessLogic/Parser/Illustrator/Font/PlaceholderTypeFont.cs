﻿//
// Author:
//   Michael Göricke
//
// Copyright (c) 2023
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

using System.Windows;
using System.Windows.Media;
using PdfSharp.Pdf;

namespace ShapeConverter.Parser.Pdf
{
    internal class PlaceholderTypeFont : IFont
    {
        public void Init(PdfDictionary fontDict)
        {
        }

        /// <summary>
        /// Get the type face
        /// </summary>
        public Typeface GetTypeFace()
        {
            Typeface typeFace;

            var ff = new FontFamily("Arial");
            typeFace = new Typeface(ff, FontStyles.Normal, FontWeights.Bold, FontStretches.Normal);

            return typeFace;
        }
    }
}
