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
        static int filenameCounter = 1;

        string filename;
        string fontFamily;
        string fontName;

        protected void Init(PdfDictionary fontDict, string fileExtension)
        {
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
        private void ReadFontDescriptor(PdfDictionary fontDescriptorDict, string fileExtension)
        {
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
                        fontFamily = fontDescriptorDict.Elements.GetString(fontDescriptorKey);
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
                        break;
                    }

                    case PdfKeys.ItalicAngle:
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Get the type face
        /// </summary>
        public Typeface GetTypeFace()
        {
            string baseDir = System.IO.Path.GetDirectoryName(filename);
            baseDir = baseDir.Replace('\\', '/');

            var shortName = fontName.Substring(7, fontName.Length - 7);

            string uri = string.Format("file:///{0}/#{1},{2}", baseDir, fontName, shortName);
            var ff = new FontFamily(uri);
            var typeFace = new Typeface(ff, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);

            return typeFace;
        }
    }
}
