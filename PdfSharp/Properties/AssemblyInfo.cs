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

using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
#if WPF
using System.Windows.Markup;
#endif

[assembly: AssemblyTitle(PdfSharp.VersionInfo.Title)]
[assembly: AssemblyVersion(PdfSharp.VersionInfo.Version)]
[assembly: AssemblyDescription(PdfSharp.VersionInfo.Description)]
[assembly: AssemblyConfiguration(PdfSharp.VersionInfo.Configuration)]
[assembly: AssemblyCompany(PdfSharp.VersionInfo.Company)]
#if DEBUG
[assembly: AssemblyProduct(PdfSharp.ProductVersionInfo.Product + " (Debug Build)")]
#else
  [assembly: AssemblyProduct(PdfSharp.ProductVersionInfo.Product)]
#endif
[assembly: AssemblyCopyright(PdfSharp.VersionInfo.Copyright)]
[assembly: AssemblyTrademark(PdfSharp.VersionInfo.Trademark)]
[assembly: AssemblyCulture(PdfSharp.VersionInfo.Culture)]

[assembly: NeutralResourcesLanguage("en-US")]

[assembly: ComVisible(false)]
