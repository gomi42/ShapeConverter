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

namespace ShapeConverter.Parser.Pdf
{
    /// <summary>
    /// The font manager
    /// </summary>
    internal class FontManager
    {
        Dictionary<string, IFont> fontDescriptors;

        /// <summary>
        /// Init
        /// </summary>
        public Dictionary<string, IFont> Init(PdfResources resourcesDict)
        {
            fontDescriptors = new Dictionary<string, IFont>();

            if (resourcesDict == null)
            {
                return fontDescriptors;
            }

            var fontsDict = resourcesDict.Fonts;

            if (fontsDict == null)
            {
                return fontDescriptors;
            }

            foreach (var fontKey in fontsDict.Elements.Keys)
            {
                var fontDict = fontsDict.Elements.GetDictionary(fontKey);
                var fontDescriptor = ReadFontDescriptor(fontDict);
                fontDescriptors.Add(fontKey, fontDescriptor);
            }

            return fontDescriptors;
        }

        /// <summary>
        /// Parse a single font dictionary and create an appropriate font discriptor
        /// </summary>
        public static IFont ReadFontDescriptor(PdfDictionary fontDict)
        {
            IFont fontDescriptor = null;

            var subtype = fontDict.Elements.GetName(PdfKeys.Subtype);

            switch (subtype)
            {
                case PdfKeys.TrueType:
                {
                    fontDescriptor = new TrueTypeFont();
                    fontDescriptor.Init(fontDict);
                    break;
                }

                case PdfKeys.Type1:
                {
                    fontDescriptor = new Type1Font();
                    fontDescriptor.Init(fontDict);
                    break;
                }
            }

            return fontDescriptor;
        }

        /// <summary>
        /// Get a font discriptor from a given font name
        /// </summary>
        public IFont GetFont(string fontName)
        {
            return fontDescriptors[fontName];
        }
    }
}
