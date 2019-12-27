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
using System.Globalization;
#if GDI
using System.Drawing;
#endif
#if WPF
using System.Windows.Media;
#endif
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.IO;
using PdfSharp.Pdf.Internal;

namespace PdfSharp.Pdf
{
    /// <summary>
    /// Represents a PDF rectangle value, that is internally an array with 4 real values.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay}")]
    public sealed class PdfRectangle : PdfItem
    {
        // This class must behave like a value type. Therefore it cannot be changed (like System.String).

        /// <summary>
        /// Initializes a new instance of the PdfRectangle class.
        /// </summary>
        public PdfRectangle()
        { }

        /// <summary>
        /// Initializes a new instance of the PdfRectangle class with two points specifying
        /// two diagonally opposite corners. Notice that in contrast to GDI+ convention the 
        /// 3rd and the 4th parameter specify a point and not a width. This is so much confusing
        /// that this function is for internal use only.
        /// </summary>
        internal PdfRectangle(double x1, double y1, double x2, double y2)
        {
            _x1 = x1;
            _y1 = y1;
            _x2 = x2;
            _y2 = y2;
        }

        /// <summary>
        /// Initializes a new instance of the PdfRectangle class with the specified PdfArray.
        /// </summary>
        internal PdfRectangle(PdfItem item)
        {
            if (item == null || item is PdfNull)
                return;

            if (item is PdfReference)
                item = ((PdfReference)item).Value;

            PdfArray array = item as PdfArray;
            if (array == null)
                throw new InvalidOperationException("");

            _x1 = array.Elements.GetReal(0);
            _y1 = array.Elements.GetReal(1);
            _x2 = array.Elements.GetReal(2);
            _y2 = array.Elements.GetReal(3);
        }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        public new PdfRectangle Clone()
        {
            return (PdfRectangle)Copy();
        }

        /// <summary>
        /// Implements cloning this instance.
        /// </summary>
        protected override object Copy()
        {
            PdfRectangle rect = (PdfRectangle)base.Copy();
            return rect;
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        public override int GetHashCode()
        {
            // This code is from System.Drawing...
            return (int)(((((uint)_x1) ^ ((((uint)_y1) << 13) |
              (((uint)_y1) >> 0x13))) ^ ((((uint)_x2) << 0x1a) |
              (((uint)_x2) >> 6))) ^ ((((uint)_y2) << 7) |
              (((uint)_y2) >> 0x19)));
        }

        /// <summary>
        /// Gets or sets the x-coordinate of the first corner of this PdfRectangle.
        /// </summary>
        public double X1
        {
            get { return _x1; }
        }
        readonly double _x1;

        /// <summary>
        /// Gets or sets the y-coordinate of the first corner of this PdfRectangle.
        /// </summary>
        public double Y1
        {
            get { return _y1; }
        }
        readonly double _y1;

        /// <summary>
        /// Gets or sets the x-coordinate of the second corner of this PdfRectangle.
        /// </summary>
        public double X2
        {
            get { return _x2; }
        }
        readonly double _x2;

        /// <summary>
        /// Gets or sets the y-coordinate of the second corner of this PdfRectangle.
        /// </summary>
        public double Y2
        {
            get { return _y2; }
        }
        readonly double _y2;

        /// <summary>
        /// Gets X2 - X1.
        /// </summary>
        public double Width
        {
            get { return _x2 - _x1; }
        }

        /// <summary>
        /// Gets Y2 - Y1.
        /// </summary>
        public double Height
        {
            get { return _y2 - _y1; }
        }

        /// <summary>
        /// Returns the rectangle as a string in the form «[x1 y1 x2 y2]».
        /// </summary>
        public override string ToString()
        {
            const string format = Config.SignificantFigures3;
            return PdfEncoders.Format("[{0:" + format + "} {1:" + format + "} {2:" + format + "} {3:" + format + "}]", _x1, _y1, _x2, _y2);
        }

        /// <summary>
        /// Gets the DebuggerDisplayAttribute text.
        /// </summary>
        // ReSharper disable UnusedMember.Local
        string DebuggerDisplay
        // ReSharper restore UnusedMember.Local
        {
            get
            {
                const string format = Config.SignificantFigures10;
                return String.Format(CultureInfo.InvariantCulture,
                    "X1={0:" + format + "}, Y1={1:" + format + "}, X2={2:" + format + "}, Y2={3:" + format + "}", _x1, _y1, _x2, _y2);
            }
        }

        /// <summary>
        /// Represents an empty PdfRectangle.
        /// </summary>
        public static readonly PdfRectangle Empty = new PdfRectangle();
    }
}
