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
using System.IO;
using System.Windows;
using System.Windows.Media;
using PdfSharp.Pdf;
using ShapeConverter.Helper;
using ShapeConverter.Parser.Pdf;

namespace ShapeConverter.BusinessLogic.Parser.Pdf
{
    internal class FontTypeBase
    {
        private class StandardFontDescription
        {
            public StandardFontDescription(string fontName)
            {
                FontName = fontName;
                Style = FontStyles.Normal;
                Weight = FontWeights.Normal;
            }

            public StandardFontDescription(string fontName, FontStyle style, FontWeight weight)
            {
                FontName = fontName;
                Style = style;
                Weight = weight;
            }

            public string FontName { get; }
            public FontStyle Style { get; }
            public FontWeight Weight { get; }
        }

        //////////////////////////////////////////////////////////////////////////

        static int filenameCounter = 1;

        private Dictionary<string, StandardFontDescription> Standard14Fonts = new Dictionary<string, StandardFontDescription>
        {
            { "/Helvetica", new StandardFontDescription("Arial") },
            { "/Helvetica-Bold", new StandardFontDescription("Arial", FontStyles.Normal, FontWeights.Bold) },
            { "/Helvetica-Oblique", new StandardFontDescription("Arial", FontStyles.Oblique, FontWeights.Normal) },
            { "/Helvetica-BoldOblique", new StandardFontDescription("Arial", FontStyles.Oblique, FontWeights.Bold) },
            { "/Times-Roman", new StandardFontDescription("Times New Roman") },
            { "/Times-Bold", new StandardFontDescription("Times New Roman", FontStyles.Normal, FontWeights.Bold) },
            { "/Times-Italic", new StandardFontDescription("Times New Roman", FontStyles.Italic, FontWeights.Normal) },
            { "/Times-BoldItalic", new StandardFontDescription("Times New Roman", FontStyles.Italic, FontWeights.Bold) },
            { "/Courier", new StandardFontDescription("Courier") },
            { "/Courier-Bold", new StandardFontDescription("Courier", FontStyles.Normal, FontWeights.Bold) },
            { "/Courier-Oblique", new StandardFontDescription("Courier", FontStyles.Oblique, FontWeights.Normal) },
            { "/Courier-BoldOblique", new StandardFontDescription("Courier", FontStyles.Oblique, FontWeights.Bold) },
            { "/Symbol", new StandardFontDescription("Symbol") },
            { "/ZapfDingbats", new StandardFontDescription("Wingdings") }
        };

        private string filename;
        private string fontName;
        private FontStyle fontStyle = FontStyles.Normal;
        private FontWeight fontWeight = FontWeights.Normal;
        private FontStretch fontStretch = FontStretches.Normal;

        protected void Init(PdfDictionary fontDict, string fileExtension)
        {
            if (fontDict.Elements.Keys.Contains(PdfKeys.BaseFont))
            {
                var baseFont = fontDict.Elements.GetValue(PdfKeys.BaseFont) as PdfName;

                if (baseFont != null && Standard14Fonts.TryGetValue(baseFont.Value, out StandardFontDescription fontDescription))
                {
                    fontName = fontDescription.FontName;
                    fontStyle = fontDescription.Style;
                    fontWeight = fontDescription.Weight;
                }
            }

            foreach (var fontKey in fontDict.Elements.Keys)
            {
                switch (fontKey)
                {
                    case PdfKeys.FirstChar:
                    {
                        break;
                    }

                    case PdfKeys.LastChar:
                    {
                        break;
                    }

                    case PdfKeys.Width:
                    {
                        break;
                    }

                    case PdfKeys.FontDescriptor:
                    {
                        ReadFontDescriptor(fontDict.Elements.GetDictionary(fontKey), fileExtension);
                        break;
                    }

                    case PdfKeys.Encoding:
                    {
                        break;
                    }

                    case PdfKeys.ToUnicode:
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Parse a single PDF font descriptor
        /// </summary>
        protected void ReadFontDescriptor(PdfDictionary fontDescriptorDict, string fileExtension)
        {
            fontWeight = FontWeights.Normal;

            foreach (var fontDescriptorKey in fontDescriptorDict.Elements.Keys)
            {
                switch (fontDescriptorKey)
                {
                    case PdfKeys.FontFile:
                    case PdfKeys.FontFile2:
                    case PdfKeys.FontFile3:
                    {
                        var fontFileDict = fontDescriptorDict.Elements.GetDictionary(fontDescriptorKey);

                        var path = CommonHelper.GetTempDir();
                        filename = Path.Combine(path, string.Format("Font{0}.{1}", filenameCounter++, fileExtension));

                        using (var stream = File.Open(filename, FileMode.Create))
                        {
                            using (BinaryWriter writer = new BinaryWriter(stream))
                            {
                                writer.Write(fontFileDict.Stream.UnfilteredValue);
                            }
                        }
                        break;
                    }

                    case PdfKeys.FontFamily:
                    {
                        //fontFamily = fontDescriptorDict.Elements.GetString(fontDescriptorKey);
                        break;
                    }

                    case PdfKeys.FontName:
                    {
                        var cname = fontDescriptorDict.Elements.GetName(fontDescriptorKey);
                        fontName = cname.Substring(1, cname.Length - 1);
                        break;
                    }

                    case PdfKeys.FontStretch:
                    {
                        var cname = fontDescriptorDict.Elements.GetName(fontDescriptorKey);

                        switch (cname)
                        {
                            case "UltraCondensed":
                                fontStretch = FontStretches.UltraCondensed;
                                break;

                            case "ExtraCondensed":
                                fontStretch = FontStretches.ExtraCondensed;
                                break;

                            case "Condensed":
                                fontStretch = FontStretches.Condensed;
                                break;

                            case "SemiCondensed":
                                fontStretch = FontStretches.SemiCondensed;
                                break;

                            case "Normal":
                                fontStretch = FontStretches.Normal;
                                break;

                            case "SemiExpanded":
                                fontStretch = FontStretches.SemiExpanded;
                                break;

                            case "Expanded":
                                fontStretch = FontStretches.Expanded;
                                break;

                            case "ExtraExpanded":
                                fontStretch = FontStretches.ExtraExpanded;
                                break;

                            case "UltraExpanded":
                                fontStretch = FontStretches.UltraExpanded;
                                break;
                        }
                        break;
                    }

                    case PdfKeys.FontWeight:
                    {
                        var pdfWeight = fontDescriptorDict.Elements.GetValue(fontDescriptorKey) as PdfInteger;

                        if (pdfWeight == null)
                        {
                            break;
                        }

                        int weight = pdfWeight.Value;

                        if (weight <= 100)
                        {
                            fontWeight = FontWeights.Thin;
                        }
                        else
                        if (weight <= 200)
                        {
                            fontWeight = FontWeights.ExtraLight;
                        }
                        else
                        if (weight <= 300)
                        {
                            fontWeight = FontWeights.Light;
                        }
                        else
                        if (weight <= 400)
                        {
                            fontWeight = FontWeights.Normal;
                        }
                        else
                        if (weight <= 500)
                        {
                            fontWeight = FontWeights.Medium;
                        }
                        else
                        if (weight <= 600)
                        {
                            fontWeight = FontWeights.DemiBold;
                        }
                        else
                        if (weight <= 700)
                        {
                            fontWeight = FontWeights.Bold;
                        }
                        else
                        if (weight <= 800)
                        {
                            fontWeight = FontWeights.ExtraBold;
                        }
                        else
                        if (weight <= 900)
                        {
                            fontWeight = FontWeights.ExtraLight;
                        }
                        else
                        {
                            fontWeight = FontWeights.UltraBlack;
                        }
                        break;
                    }

                    case PdfKeys.ItalicAngle:
                    {
                        var pdfStyle = fontDescriptorDict.Elements.GetValue(fontDescriptorKey) as PdfInteger;

                        if (pdfStyle == null)
                        {
                            break;
                        }

                        if (pdfStyle.Value != 0)
                        {
                            fontStyle = FontStyles.Italic;
                        }
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Set a default font
        /// </summary>
        protected void SetDefaultFont()
        {
            fontName = "Arial";
        }

        /// <summary>
        /// Get the type face
        /// </summary>
        public Typeface GetTypeFace()
        {
            Typeface typeFace;

            if (filename == null)
            {
                var ff = new FontFamily(fontName);
                typeFace = new Typeface(ff, fontStyle, fontWeight, fontStretch);
            }
            else
            {
                string baseDir = Path.GetDirectoryName(filename);
                baseDir = baseDir.Replace('\\', '/');

                var shortName = fontName.Substring(7, fontName.Length - 7);

                string uri = string.Format("file:///{0}/#{1},{2}", baseDir, fontName, shortName);
                var ff = new FontFamily(uri);
                typeFace = new Typeface(ff, FontStyles.Normal, fontWeight, fontStretch);
            }

            return typeFace;
        }
    }
}
