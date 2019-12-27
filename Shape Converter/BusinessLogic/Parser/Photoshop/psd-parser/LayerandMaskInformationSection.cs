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

using Ntreev.Library.Psd.Readers;
using Ntreev.Library.Psd.Readers.LayerAndMaskInformation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ntreev.Library.Psd.Readers.LayerResources;

namespace Ntreev.Library.Psd
{
    class LayerAndMaskInformationSection : LazyDataReader
    {
        private LayerInfoBlock layerInfo;
        private GlobalLayerMaskInfoBlock globalLayerMask;
        private DocumentResourceBlock documentResources;

        private ILinkedLayer[] linkedLayers;

        public LayerAndMaskInformationSection(PsdReader reader, PsdDocument document)
        {
            Init(reader, true, document);
        }

        public LayerInfoBlock LayerInfo
        {
            get
            {
                ReadData();
                return layerInfo;
            }

            set
            {
                layerInfo = value;
            }
        }

        public GlobalLayerMaskInfoBlock GlobalLayerMaskInfo
        {
            get
            {
                ReadData();
                return globalLayerMask;
            }

            set
            {
                globalLayerMask = value;
            }
        }

        public DocumentResourceBlock DocumentResource
        {
            get
            {
                ReadData();
                return documentResources;
            }

            set
            {
                documentResources = value;
            }
        }

        public IResourceBlock[] Resources
        {
            get
            {
                ReadData();
                return documentResources.Resources;
            }
        }

        public PsdLayer[] Layers
        {
            get { return this.LayerInfo.Layers; }
        }

        public ILinkedLayer[] LinkedLayers
        {
            get
            {
                if (this.linkedLayers == null)
                {
                    List<ILinkedLayer> list = new List<ILinkedLayer>();

                    IResourceBlock block = null;

                    block = this.Resources.FirstOrDefault(x => x is lnk2Resource);

                    if (block != null)
                    {
                        var res = (lnk2Resource)block;
                        list.AddRange(res.LinkedLayers);
                    }

                    block = this.Resources.FirstOrDefault(x => x is lnk3Resource);

                    if (block != null)
                    {
                        var res = (lnk3Resource)block;
                        list.AddRange(res.LinkedLayers);
                    }

                    block = this.Resources.FirstOrDefault(x => x is lnkDResource);

                    if (block != null)
                    {
                        var res = (lnkDResource)block;
                        list.AddRange(res.LinkedLayers);
                    }

                    block = this.Resources.FirstOrDefault(x => x is lnkEResource);

                    if (block != null)
                    {
                        var res = (lnkEResource)block;
                        list.AddRange(res.EmbeddedLayers);
                    }

                    this.linkedLayers = list.ToArray();
                }

                return this.linkedLayers;
            }
        }

        protected override void ReadData(PsdReader reader, object userData)
        {
            PsdDocument document = userData as PsdDocument;

            var layerInfo = new LayerInfoBlock(reader, document);

            if (reader.Position + 4 >= this.EndPosition)
            {
                LayerInfo = layerInfo;
                GlobalLayerMaskInfo = null;
                DocumentResource = new DocumentResourceBlock();
            }
            else
            {
                GlobalLayerMaskInfoBlock globalLayerMask = new GlobalLayerMaskInfoBlock(reader);
                var documentResource = new DocumentResourceBlock(reader, this.EndPosition - reader.Position);

                LayerInfo = layerInfo;
                GlobalLayerMaskInfo = globalLayerMask;
                DocumentResource = documentResource;
            }
        }
    }
}
