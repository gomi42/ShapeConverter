﻿//Released under the MIT License.
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

using Ntreev.Library.Psd.Readers;
using Ntreev.Library.Psd.Readers.ImageResources;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Ntreev.Library.Psd.Readers
{
    /// <summary>
    /// The image resources section
    /// </summary>
    class ImgageResourcesSection : LazyDataReader
    {
        private IResourceBlock[] resources;

        public ImgageResourcesSection(PsdReader reader)
        {
          Init(reader, true, null);
        }

        public IResourceBlock[] Resources
        {
            get
            {
                ReadData();
                return resources;
            }

            set
            {
                resources = value;
            }
        }

        protected override long ReadLength(PsdReader reader)
        {
            return reader.ReadInt32();
        }

        protected override void ReadData(PsdReader reader, object userData)
        {
            var list = new List<IResourceBlock>();

            while(reader.Position < this.EndPosition)
            {
                reader.ValidateSignature();

                string resourceID = reader.ReadInt16().ToString();
                string name = reader.ReadPascalString(2);
                long length = reader.ReadInt32();
                length += (length % 2);

                var resourceReader = ReaderCollector.CreateReader(resourceID, reader, length);
                string resourceName = ReaderCollector.GetDisplayName(resourceID);

                list.Add(resourceReader);
            }

            Resources = list.ToArray();
        }
    }
}
