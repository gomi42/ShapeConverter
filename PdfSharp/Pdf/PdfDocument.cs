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
using System.IO;
#if NETFX_CORE
using System.Threading.Tasks;
#endif
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.Internal;
using PdfSharp.Pdf.IO;
using PdfSharp.Pdf.Security;

// ReSharper disable ConvertPropertyToExpressionBody

namespace PdfSharp.Pdf
{
    /// <summary>
    /// Represents a PDF document.
    /// </summary>
    public sealed class PdfDocument : PdfObject, IDisposable
    {
        internal DocumentState _state;
        internal PdfDocumentOpenMode _openMode;

        /// <summary>
        /// Creates a new PDF document in memory.
        /// To open an existing PDF file, use the PdfReader class.
        /// </summary>
        public PdfDocument()
        {
            _creation = DateTime.Now;
            _state = DocumentState.Created;
            _version = 14;
            Initialize();
        }

        internal PdfDocument(Lexer lexer)
        {
            //PdfDocument.Gob.AttatchDocument(Handle);

            _creation = DateTime.Now;
            _state = DocumentState.Imported;

            _irefTable = new PdfCrossReferenceTable(this);
            _lexer = lexer;
        }

        void Initialize()
        {
            _trailer = new PdfTrailer(this);
            _irefTable = new PdfCrossReferenceTable(this);
            _trailer.CreateNewDocumentIDs();
        }

        /// <summary>
        /// Disposes all references to this document stored in other documents. This function should be called
        /// for documents you finished importing pages from. Calling Dispose is technically not necessary but
        /// useful for earlier reclaiming memory of documents you do not need anymore.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            //GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing)
        {
            if (_state != DocumentState.Disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources.
                }
            }
            _state = DocumentState.Disposed;
        }

        internal bool CanModify
        {
            get { return true; }
        }

        internal bool HasVersion(string version)
        {
            return String.Compare(Catalog.Version, version) >= 0;
        }

        /// <summary>
        /// Gets the document options used for saving the document.
        /// </summary>
        public PdfDocumentOptions Options
        {
            get
            {
                if (_options == null)
                    _options = new PdfDocumentOptions(this);
                return _options;
            }
        }
        PdfDocumentOptions _options;

        /// <summary>
        /// Gets or sets the PDF version number. Return value 14 e.g. means PDF 1.4 / Acrobat 5 etc.
        /// </summary>
        public int Version
        {
            get { return _version; }
        }
        internal int _version;

        /// <summary>
        /// Gets the number of pages in the document.
        /// </summary>
        public int PageCount
        {
            get
            {
                if (CanModify)
                    return Pages.Count;
                // PdfOpenMode is InformationOnly
                PdfDictionary pageTreeRoot = (PdfDictionary)Catalog.Elements.GetObject(PdfCatalog.Keys.Pages);
                return pageTreeRoot.Elements.GetInteger(PdfPages.Keys.Count);
            }
        }

        /// <summary>
        /// Gets the file size of the document.
        /// </summary>
        public long FileSize
        {
            get { return _fileSize; }
        }
        internal long _fileSize; // TODO: make private

        /// <summary>
        /// Gets the full qualified file name if the document was read form a file, or an empty string otherwise.
        /// </summary>
        public string FullPath
        {
            get { return _fullPath; }
        }
        internal string _fullPath = String.Empty; // TODO: make private

        /// <summary>
        /// Gets a Guid that uniquely identifies this instance of PdfDocument.
        /// </summary>
        public Guid Guidx
        {
            get { return _guid; }
        }
        Guid _guid = Guid.NewGuid();

        internal DocumentHandle Handle
        {
            get
            {
                if (_handle == null)
                    _handle = new DocumentHandle(this);
                return _handle;
            }
        }
        DocumentHandle _handle;

        /// <summary>
        /// Returns a value indicating whether the document was newly created or opened from an existing document.
        /// Returns true if the document was opened with the PdfReader.Open function, false otherwise.
        /// </summary>
        public bool IsImported
        {
            get { return (_state & DocumentState.Imported) != 0; }
        }

        /// <summary>
        /// Get the pages dictionary.
        /// </summary>
        public PdfPages Pages
        {
            get
            {
                if (_pages == null)
                    _pages = Catalog.Pages;
                return _pages;
            }
        }
        PdfPages _pages;  // never changes if once created

        /// <summary>
        /// Gets or sets a value specifying the page layout to be used when the document is opened.
        /// </summary>
        public PdfPageLayout PageLayout
        {
            get { return Catalog.PageLayout; }
        }

        /// <summary>
        /// Gets or sets a value specifying how the document should be displayed when opened.
        /// </summary>
        public PdfPageMode PageMode
        {
            get { return Catalog.PageMode; }
        }

        /// <summary>
        /// Gets the viewer preferences of this document.
        /// </summary>
        public PdfViewerPreferences ViewerPreferences
        {
            get { return Catalog.ViewerPreferences; }
        }

        /// <summary>
        /// Gets the root of the outline (or bookmark) tree.
        /// </summary>
        public PdfOutlineCollection Outlines
        {
            get { return Catalog.Outlines; }
        }

        /// <summary>
        /// Gets or sets the default language of the document.
        /// </summary>
        public string Language
        {
            get { return Catalog.Language; }
        }

        /// <summary>
        /// Gets the security settings of this document.
        /// </summary>
        public PdfSecuritySettings SecuritySettings
        {
            get { return _securitySettings ?? (_securitySettings = new PdfSecuritySettings(this)); }
        }
        internal PdfSecuritySettings _securitySettings;

        /// <summary>
        /// Gets the PdfCatalog of the current document.
        /// </summary>
        internal PdfCatalog Catalog
        {
            get { return _catalog ?? (_catalog = _trailer.Root); }
        }
        PdfCatalog _catalog;  // never changes if once created

        /// <summary>
        /// Gets the PdfInternals object of this document, that grants access to some internal structures
        /// which are not part of the public interface of PdfDocument.
        /// </summary>
        public new PdfInternals Internals
        {
            get { return _internals ?? (_internals = new PdfInternals(this)); }
        }
        PdfInternals _internals;

        /// <summary>
        /// Gets the security handler.
        /// </summary>
        public PdfStandardSecurityHandler SecurityHandler
        {
            get { return _trailer.SecurityHandler; }
        }

        internal PdfTrailer _trailer;
        internal PdfCrossReferenceTable _irefTable;

        // Imported Document
        internal Lexer _lexer;

        internal DateTime _creation;

        //internal static GlobalObjectTable Gob = new GlobalObjectTable();

        /// <summary>
        /// Gets the ThreadLocalStorage object. It is used for caching objects that should created
        /// only once.
        /// </summary>
        internal static ThreadLocalStorage Tls
        {
            get { return tls ?? (tls = new ThreadLocalStorage()); }
        }
        [ThreadStatic]
        static ThreadLocalStorage tls;

        [DebuggerDisplay("(ID={ID}, alive={IsAlive})")]
        internal class DocumentHandle
        {
            public DocumentHandle(PdfDocument document)
            {
                _weakRef = new WeakReference(document);
                ID = document._guid.ToString("B").ToUpper();
            }

            public bool IsAlive
            {
                get { return _weakRef.IsAlive; }
            }

            public PdfDocument Target
            {
                get { return _weakRef.Target as PdfDocument; }
            }
            readonly WeakReference _weakRef;

            public string ID;

            public override bool Equals(object obj)
            {
                DocumentHandle handle = obj as DocumentHandle;
                if (!ReferenceEquals(handle, null))
                    return ID == handle.ID;
                return false;
            }

            public override int GetHashCode()
            {
                return ID.GetHashCode();
            }

            public static bool operator ==(DocumentHandle left, DocumentHandle right)
            {
                if (ReferenceEquals(left, null))
                    return ReferenceEquals(right, null);
                return left.Equals(right);
            }

            public static bool operator !=(DocumentHandle left, DocumentHandle right)
            {
                return !(left == right);
            }
        }
    }
}