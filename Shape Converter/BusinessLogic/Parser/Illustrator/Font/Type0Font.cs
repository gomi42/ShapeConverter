//
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

using PdfSharp.Pdf;
using ShapeConverter.BusinessLogic.Parser.Pdf;

namespace ShapeConverter.Parser.Pdf
{
    internal class Type0Font : FontTypeBase, IFont
    {
        public void Init(PdfDictionary fontDict)
        {
            var descendents = fontDict.Elements.GetArray(PdfKeys.DescendantFonts);
            var innerFontDict = descendents.Elements.GetDictionary(0);
            var fontDescriptorDict = innerFontDict.Elements.GetDictionary(PdfKeys.FontDescriptor);

            var file1 = fontDescriptorDict.Elements.GetDictionary(PdfKeys.FontFile);

            if (file1 != null)
            {
                ReadFontDescriptor(fontDescriptorDict, "fon");
                return;
            }

            var file2 = fontDescriptorDict.Elements.GetDictionary(PdfKeys.FontFile2);

            if (file2 != null)
            {
                ReadFontDescriptor(fontDescriptorDict, "ttf");
                return;
            }

            SetDefaultFont();
        }
    }
}
