//Released under the MIT License.
//
//Copyright (c) 2015 Ntreev Soft co., Ltd.
//
//Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
//documentation files (the "Software"), to deal in the Software without restriction, including without limitation the 
//rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit 
//persons to whom the Software is furnished to do so, subject to the following conditions:
//
//The above copyright notice and this permission notice shall be included in all copies or substantial portions of the 
//Software.
//
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE 
//WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR 
//COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR 
//OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ntreev.Library.Psd
{
    class LayerMaskBlock : LazyDataReader
    {
        private int top;
        private int right;
        private int bottom;
        private byte color;
        private byte flag;

        public LayerMaskBlock(PsdReader reader)
        {
            Init(reader, true, null);
        }

        public int Left { get; set; }

        public int Top
        {
            get
            {
                ReadData();
                return top;
            }

            set
            {
                top = value;
            }
        }

        public int Right
        {
            get
            {
                ReadData();
                return right;
            }

            set
            {
                right = value;
            }
        }

        public int Bottom
        {
            get
            {
                ReadData();
                return bottom;
            }

            set
            {
                bottom = value;
            }
        }

        public byte Color
        {
            get
            {
                ReadData();
                return color;
            }

            set
            {
                color = value;
            }
        }

        public byte Flag
        {
            get
            {
                ReadData();
                return flag;
            }

            set
            {
                flag = value;
            }
        }

        public int Width
        {
            get
            {
                ReadData();
                return this.Right - this.Left;
            }
        }

        public int Height
        {
            get
            {
                ReadData();
                return this.Bottom - this.Top;
            }
        }

        public static LayerMaskBlock Read(PsdReader reader)
        {
            LayerMaskBlock instance = new LayerMaskBlock(reader);
            return instance;
        }

        protected override long ReadLength(PsdReader reader)
        {
            return reader.ReadInt32();
        }

        protected override void ReadData(PsdReader reader, object userData)
        {
            Top = reader.ReadInt32();
            Left = reader.ReadInt32();
            Bottom = reader.ReadInt32();
            Right = reader.ReadInt32();
            Color = reader.ReadByte();
            Flag = reader.ReadByte();
        }
    }
}
