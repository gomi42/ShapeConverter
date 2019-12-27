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
using System.Collections.Generic;
using System.Diagnostics;
using System.Collections;
using PdfSharp.Pdf.IO;
using PdfSharp.Pdf.Advanced;

namespace PdfSharp.Pdf
{
    /// <summary>
    /// Represents the pages of the document.
    /// </summary>
    [DebuggerDisplay("(PageCount={Count})")]
    public sealed class PdfPages : PdfDictionary, IEnumerable<PdfPage>
    {
        internal PdfPages(PdfDocument document)
            : base(document)
        {
            Elements.SetName(Keys.Type, "/Pages");
            Elements[Keys.Count] = new PdfInteger(0);
        }

        internal PdfPages(PdfDictionary dictionary)
            : base(dictionary)
        { }

        /// <summary>
        /// Gets the number of pages.
        /// </summary>
        public int Count
        {
            get { return PagesArray.Elements.Count; }
        }

        /// <summary>
        /// Gets the page with the specified index.
        /// </summary>
        public PdfPage this[int index]
        {
            get
            {
                if (index < 0 || index >= Count)
                    throw new ArgumentOutOfRangeException("index");

                PdfDictionary dict = (PdfDictionary)((PdfReference)PagesArray.Elements[index]).Value;
                if (!(dict is PdfPage))
                    dict = new PdfPage(dict);
                return (PdfPage)dict;
            }
        }

        static PdfReference RemapReference(PdfPage[] newPages, PdfPage[] impPages, PdfReference iref)
        {
            // Directs the iref to a one of the imported pages?
            for (int idx = 0; idx < newPages.Length; idx++)
            {
                if (impPages[idx].Reference == iref)
                    return newPages[idx].Reference;
            }
            return null;
        }

        /// <summary>
        /// Gets a PdfArray containing all pages of this document. The array must not be modified.
        /// </summary>
        public PdfArray PagesArray
        {
            get
            {
                if (_pagesArray == null)
                    _pagesArray = (PdfArray)Elements.GetValue(Keys.Kids, VCF.None);
                return _pagesArray;
            }
        }
        PdfArray _pagesArray;

        /// <summary>
        /// Replaces the page tree by a flat array of indirect references to the pages objects.
        /// </summary>
        internal void FlattenPageTree()
        {
            // Acrobat creates a balanced tree if the number of pages is roughly more than ten. This is
            // not difficult but obviously also not necessary. I created a document with 50000 pages with
            // PDF4NET and Acrobat opened it in less than 2 seconds.

            //PdfReference xrefRoot = Document.Catalog.Elements[PdfCatalog.Keys.Pages] as PdfReference;
            //PdfDictionary[] pages = GetKids(xrefRoot, null);

            // Promote inheritable values down the page tree
            PdfPage.InheritedValues values = new PdfPage.InheritedValues();
            PdfPage.InheritValues(this, ref values);
            PdfDictionary[] pages = GetKids(Reference, values, null);

            // Replace /Pages in catalog by this object
            // xrefRoot.Value = this;

            PdfArray array = new PdfArray(Owner);
            foreach (PdfDictionary page in pages)
            {
                // Fix the parent
                page.Elements[PdfPage.Keys.Parent] = Reference;
                array.Elements.Add(page.Reference);
            }

            Elements.SetName(Keys.Type, "/Pages");
#if true
            // Direct array.
            Elements.SetValue(Keys.Kids, array);
#else
            // Indirect array.
            Document.xrefTable.Add(array);
            Elements.SetValue(Keys.Kids, array.XRef);
#endif
            Elements.SetInteger(Keys.Count, array.Elements.Count);
        }

        /// <summary>
        /// Recursively converts the page tree into a flat array.
        /// </summary>
        PdfDictionary[] GetKids(PdfReference iref, PdfPage.InheritedValues values, PdfDictionary parent)
        {
            // TODO: inherit inheritable keys...
            PdfDictionary kid = (PdfDictionary)iref.Value;

#if true
            string type = kid.Elements.GetName(Keys.Type);
            if (type == "/Page")
            {
                PdfPage.InheritValues(kid, values);
                return new PdfDictionary[] { kid };
            }

            if (string.IsNullOrEmpty(type))
            {
                // Type is required. If type is missing, assume it is "/Page" and hope it will work.
                // TODO Implement a "Strict" mode in PDFsharp and don't do this in "Strict" mode.
                PdfPage.InheritValues(kid, values);
                return new PdfDictionary[] { kid };
            }

#else
            if (kid.Elements.GetName(Keys.Type) == "/Page")
            {
                PdfPage.InheritValues(kid, values);
                return new PdfDictionary[] { kid };
            }
#endif

            Debug.Assert(kid.Elements.GetName(Keys.Type) == "/Pages");
            PdfPage.InheritValues(kid, ref values);
            List<PdfDictionary> list = new List<PdfDictionary>();
            PdfArray kids = kid.Elements["/Kids"] as PdfArray;

            if (kids == null)
            {
                PdfReference xref3 = kid.Elements["/Kids"] as PdfReference;
                if (xref3 != null)
                    kids = xref3.Value as PdfArray;
            }

            foreach (PdfReference xref2 in kids)
                list.AddRange(GetKids(xref2, values, kid));
            int count = list.Count;
            Debug.Assert(count == kid.Elements.GetInteger("/Count"));
            return list.ToArray();
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        public new IEnumerator<PdfPage> GetEnumerator()
        {
            return new PdfPagesEnumerator(this);
        }

        class PdfPagesEnumerator : IEnumerator<PdfPage>
        {
            internal PdfPagesEnumerator(PdfPages list)
            {
                _list = list;
                _index = -1;
            }

            public bool MoveNext()
            {
                if (_index < _list.Count - 1)
                {
                    _index++;
                    _currentElement = _list[_index];
                    return true;
                }
                _index = _list.Count;
                return false;
            }

            public void Reset()
            {
                _currentElement = null;
                _index = -1;
            }

            object IEnumerator.Current
            {
                get { return Current; }
            }

            public PdfPage Current
            {
                get
                {
                    if (_index == -1 || _index >= _list.Count)
                        throw new InvalidOperationException();
                    return _currentElement;
                }
            }

            public void Dispose()
            {
                // Nothing to do.
            }

            PdfPage _currentElement;
            int _index;
            readonly PdfPages _list;
        }

        /// <summary>
        /// Predefined keys of this dictionary.
        /// </summary>
        internal sealed class Keys : PdfPage.InheritablePageKeys
        {
            /// <summary>
            /// (Required) The type of PDF object that this dictionary describes; 
            /// must be Pages for a page tree node.
            /// </summary>
            [KeyInfo(KeyType.Name | KeyType.Required, FixedValue = "Pages")]
            public const string Type = "/Type";

            /// <summary>
            /// (Required except in root node; must be an indirect reference)
            /// The page tree node that is the immediate parent of this one.
            /// </summary>
            [KeyInfo(KeyType.Dictionary | KeyType.Required)]
            public const string Parent = "/Parent";

            /// <summary>
            /// (Required) An array of indirect references to the immediate children of this node.
            /// The children may be page objects or other page tree nodes.
            /// </summary>
            [KeyInfo(KeyType.Array | KeyType.Required)]
            public const string Kids = "/Kids";

            /// <summary>
            /// (Required) The number of leaf nodes (page objects) that are descendants of this node 
            /// within the page tree.
            /// </summary>
            [KeyInfo(KeyType.Integer | KeyType.Required)]
            public const string Count = "/Count";

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
