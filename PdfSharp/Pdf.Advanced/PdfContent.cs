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

using System;
using System.Diagnostics;
using PdfSharp.Pdf.Filters;

namespace PdfSharp.Pdf.Advanced
{
    /// <summary>
    /// Represents the content of a page. PDFsharp supports only one content stream per page.
    /// If an imported page has an array of content streams, the streams are concatenated to
    /// one single stream.
    /// </summary>
    public sealed class PdfContent : PdfDictionary
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PdfContent"/> class.
        /// </summary>
        public PdfContent(PdfDocument document)
            : base(document)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfContent"/> class.
        /// </summary>
        internal PdfContent(PdfPage page)
            : base(page != null ? page.Owner : null)
        {
            //_pageContent = new PageContent(page);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfContent"/> class.
        /// </summary>
        /// <param name="dict">The dict.</param>
        public PdfContent(PdfDictionary dict) // HACK PdfContent
            : base(dict)
        {
            // A PdfContent dictionary is always unfiltered.
            Decode();
        }

        /// <summary>
        /// Sets a value indicating whether the content is compressed with the ZIP algorithm.
        /// </summary>
        public bool Compressed
        {
            set
            {
                if (value)
                {
                    PdfItem filter = Elements["/Filter"];
                    if (filter == null)
                    {
                        byte[] bytes = Filtering.FlateDecode.Encode(Stream.Value, _document.Options.FlateEncodeMode);
                        Stream.Value = bytes;
                        Elements.SetInteger("/Length", Stream.Length);
                        Elements.SetName("/Filter", "/FlateDecode");
                    }
                }
            }
        }

        /// <summary>
        /// Unfilters the stream.
        /// </summary>
        void Decode()
        {
            if (Stream != null && Stream.Value != null)
            {
                PdfItem item = Elements["/Filter"];
                if (item != null)
                {
                    byte[] bytes = Filtering.Decode(Stream.Value, item);
                    if (bytes != null)
                    {
                        Stream.Value = bytes;
                        Elements.Remove("/Filter");
                        Elements.SetInteger("/Length", Stream.Length);
                    }
                }
            }
        }

        /// <summary>
        /// Predefined keys of this dictionary.
        /// </summary>
        internal sealed class Keys : PdfStream.Keys
        {
            /// <summary>
            /// Gets the KeysMeta for these keys.
            /// </summary>
            public static DictionaryMeta Meta
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
