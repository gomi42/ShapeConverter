#region PDFsharp - A .NET library for processing PDF
//
// Authors:
//   Stefan Lange
//
// Copyright (c) 2005-2017 empira Software GmbH, Cologne Area (Germany)
//
// http://www.pdfsharp.com
// http://sourceforge.net/projects/pdfsharp
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included
// in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

using System.Collections.Generic;

namespace PdfSharp.Pdf.Advanced
{
    /// <summary>
    /// Represents a PDF resource object.
    /// </summary>
    public sealed class PdfResources : PdfDictionary
    {
        // Resource management works roughly like this:
        // When the user creates an XFont and uses it in the XGraphics of a PdfPage, then at the first time
        // a PdfFont is created and cached in the document global font table. If the user creates a new
        // XFont object for an exisisting PdfFont, the PdfFont object is reused. When the PdfFont is added
        // to the resources of a PdfPage for the first time, it is added to the page local PdfResourceMap for 
        // fonts and automatically associated with a local resource name.

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfResources"/> class.
        /// </summary>
        /// <param name="document">The document.</param>
        public PdfResources(PdfDocument document)
            : base(document)
        {
            Elements[Keys.ProcSet] = new PdfLiteral("[/PDF/Text/ImageB/ImageC/ImageI]");
        }

        public PdfResources(PdfDictionary dict)
            : base(dict)
        { }

        /// <summary>
        /// Gets the fonts map.
        /// </summary>
        public PdfDictionary Fonts
        {
            get { return _fonts ?? (_fonts = Elements.GetDictionary(Keys.Font)); }
        }
        PdfDictionary _fonts;

        /// <summary>
        /// Gets the external objects map.
        /// </summary>
        public PdfDictionary XObjects
        {
            get { return _xObjects ?? (_xObjects = Elements.GetDictionary(Keys.XObject)); }
        }
        PdfDictionary _xObjects;

        // TODO: make own class
        public PdfDictionary ExtGStates
        {
            get
            {
                return _extGStates ?? (_extGStates = Elements.GetDictionary(Keys.ExtGState));
            }
        }
        PdfDictionary _extGStates;

        // TODO: make own class
        public PdfDictionary ColorSpaces
        {
            get { return _colorSpaces ?? (_colorSpaces = Elements.GetDictionary(Keys.ColorSpace)); }
        }
        PdfDictionary _colorSpaces;

        // TODO: make own class
        public PdfDictionary Patterns
        {
            get { return _patterns ?? (_patterns = Elements.GetDictionary(Keys.Pattern)); }
        }
        PdfDictionary _patterns;

        // TODO: make own class
        public PdfDictionary Shadings
        {
            get { return _shadings ?? (_shadings = Elements.GetDictionary(Keys.Shading)); }
        }
        PdfDictionary _shadings;

        // TODO: make own class
        public PdfDictionary Properties
        {
            get {return _properties ?? (_properties = Elements.GetDictionary(Keys.Properties));}
        }
        PdfDictionary _properties;

        /// <summary>
        /// Maps all PDFsharp resources to their local resource names.
        /// </summary>
        readonly Dictionary<PdfObject, string> _resources = new Dictionary<PdfObject, string>();

        /// <summary>
        /// Predefined keys of this dictionary.
        /// </summary>
        public sealed class Keys : KeysBase
        {
            /// <summary>
            /// (Optional) A dictionary that maps resource names to graphics state 
            /// parameter dictionaries.
            /// </summary>
            [KeyInfo(KeyType.Dictionary | KeyType.Optional, typeof(PdfDictionary))]
            public const string ExtGState = "/ExtGState";

            /// <summary>
            /// (Optional) A dictionary that maps each resource name to either the name of a
            /// device-dependent color space or an array describing a color space.
            /// </summary>
            [KeyInfo(KeyType.Dictionary | KeyType.Optional, typeof(PdfDictionary))]
            public const string ColorSpace = "/ColorSpace";

            /// <summary>
            /// (Optional) A dictionary that maps each resource name to either the name of a
            /// device-dependent color space or an array describing a color space.
            /// </summary>
            [KeyInfo(KeyType.Dictionary | KeyType.Optional, typeof(PdfDictionary))]
            public const string Pattern = "/Pattern";

            /// <summary>
            /// (Optional; PDF 1.3) A dictionary that maps resource names to shading dictionaries.
            /// </summary>
            [KeyInfo("1.3", KeyType.Dictionary | KeyType.Optional, typeof(PdfDictionary))]
            public const string Shading = "/Shading";

            /// <summary>
            /// (Optional) A dictionary that maps resource names to external objects.
            /// </summary>
            [KeyInfo(KeyType.Dictionary | KeyType.Optional, typeof(PdfDictionary))]
            public const string XObject = "/XObject";

            /// <summary>
            /// (Optional) A dictionary that maps resource names to font dictionaries.
            /// </summary>
            [KeyInfo(KeyType.Dictionary | KeyType.Optional, typeof(PdfDictionary))]
            public const string Font = "/Font";

            /// <summary>
            /// (Optional) An array of predefined procedure set names.
            /// </summary>
            [KeyInfo(KeyType.Array | KeyType.Optional)]
            public const string ProcSet = "/ProcSet";

            /// <summary>
            /// (Optional; PDF 1.2) A dictionary that maps resource names to property list
            /// dictionaries for marked content.
            /// </summary>
            [KeyInfo(KeyType.Dictionary | KeyType.Optional, typeof(PdfDictionary))]
            public const string Properties = "/Properties";

            /// <summary>
            /// Gets the KeysMeta for these keys.
            /// </summary>
            internal static DictionaryMeta Meta
            {
                get { return _meta ?? (_meta = CreateMeta(typeof(Keys))); }
            }
            static DictionaryMeta _meta;
        }

        /// <summary>
        /// Gets the KeysMeta of this dictionary type.
        /// </summary>
        internal override DictionaryMeta Meta
        {
            get { return Keys.Meta; }
        }
    }
}
