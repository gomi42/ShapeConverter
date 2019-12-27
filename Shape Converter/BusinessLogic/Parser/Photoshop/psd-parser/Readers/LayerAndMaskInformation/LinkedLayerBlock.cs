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

using Ntreev.Library.Psd.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ntreev.Library.Psd.Readers.LayerAndMaskInformation
{
    class LinkedLayerBlock : LazyDataReader
    {
      private LinkedLayer linkedLayer;

        public LinkedLayerBlock(PsdReader reader)
        {
          Init(reader, true, null);
        }

      public LinkedLayer LinkedLayer
      {
        get
        {
          ReadData();
          return linkedLayer;
        }

        set
        {
          linkedLayer = value;
        }
      }

    protected override long ReadLength(PsdReader reader)
        {
            return (reader.ReadInt64() + 3) & (~3);
        }

        protected override void ReadData(PsdReader reader, object userData)
        {
            reader.ValidateSignature("liFD");
            int version = reader.ReadInt32();

            Guid id = new Guid(reader.ReadPascalString(1));
            string name = reader.ReadString();
            string type = reader.ReadType();
            string creator = reader.ReadType();
            long length = reader.ReadInt64();
            IProperties properties = reader.ReadBoolean() == true ? new StructureDescriptor(reader).Items : null;

            bool isDocument = this.IsDocument(reader);
            LinkedDocumentBlock documentReader = null;
            LinkedDocumnetFileHeaderBlock fileHeaderReader = null;

            if(length > 0 && isDocument == true)
            {
                long position = reader.Position;
                documentReader = new LinkedDocumentBlock(reader, length);
                reader.Position = position;
                fileHeaderReader = new LinkedDocumnetFileHeaderBlock(reader, length);
            }

          LinkedLayer = new LinkedLayer(name, id, documentReader, fileHeaderReader);
        }

        private bool IsDocument(PsdReader reader)
        {
            long position = reader.Position;
            try
            {
                return reader.ReadType() == "8BPS";
            }
            finally
            {
                reader.Position = position;
            }
        }
    }
}
