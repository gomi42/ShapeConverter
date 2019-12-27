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
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Ntreev.Library.Psd
{
    /// <summary>
    /// LazyDataReader
    /// </summary>
    public abstract class LazyDataReader
    {
        private PsdReader reader;
        private int readerVersion;
        private long position;
        private long length;
        private object userData;
        private bool isRead;

        protected LazyDataReader()
        {
        }

        public LazyDataReader(PsdReader reader, bool hasLength, object userData)
        {
            Init(reader, hasLength, userData);
        }

        public LazyDataReader(PsdReader reader, long length, object userData)
        {
            Init(reader, length, userData);
        }

        public LazyDataReader(PsdReader reader, long length)
        {
            Init(reader, length, null);
        }

        protected void Init(PsdReader reader, bool hasLength, object userData)
        {
            if (hasLength == true)
            {
                this.length = this.ReadLength(reader);
            }

            this.reader = reader;
            this.readerVersion = reader.Version;
            this.position = reader.Position;
            this.userData = userData;

            if (hasLength == false)
            {
                this.ReadData();
                this.length = reader.Position - this.position;
            }

            this.reader.Position = this.position + this.length;
        }

        protected void Init(PsdReader reader, long length, object userData)
        {
            if (length < 0)
            {
                throw new InvalidFormatException();
            }

            this.reader = reader;
            this.length = length;
            this.readerVersion = reader.Version;
            this.position = reader.Position;
            this.userData = userData;

            if (this.length == 0)
            {
                this.ReadData();
                this.length = reader.Position - this.position;
            }

            this.reader.Position = this.position + this.length;
        }

        protected void ReadData()
        {
            if (this.reader == null || this.isRead)
            {
                return;
            }

            this.isRead = true;

            this.reader.Position = this.position;
            this.reader.Version = this.readerVersion;
            this.ReadData(this.reader, this.userData);

            if (this.length > 0)
            {
                this.reader.Position = this.position + this.length;
            }
        }

        protected PsdReader BeginReadData()
        {
            this.reader.Position = this.position;

            return this.reader;
        }

        protected long Length
        {
            get
            {
                return this.length;
            }
        }

        protected long EndPosition
        {
            get
            {
                return this.position + this.length;
            }
        }

        protected virtual long ReadLength(PsdReader reader)
        {
            return reader.ReadLength();
        }

        protected abstract void ReadData(PsdReader reader, object userData);
    }
}