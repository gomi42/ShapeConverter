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

using Ntreev.Library.Psd.Readers.LayerAndMaskInformation;
using System;
using System.Linq;
using Ntreev.Library.Psd.Readers;
using Ntreev.Library.Psd.Readers.LayerResources;

namespace Ntreev.Library.Psd
{
    class LayerRecords
    {
        private Channel[] channels;
        private LayerMaskBlock layerMask;
        private LayerBlendingRangesBlock blendingRanges;
        private string name;
        private SectionType sectionType;
        private Guid placedID;
        private int version;
        IResourceBlock[] resources;

        public int Left { get; set; }

        public int Top { get; set; }

        public int Right { get; set; }

        public int Bottom { get; set; }

        public int Width
        {
            get { return this.Right - this.Left; }
        }

        public int Height
        {
            get { return this.Bottom - this.Top; }
        }

        public int ChannelCount
        {
            get
            {
                if (this.channels == null)
                {
                    return 0;
                }

                return this.channels.Length;
            }
            set
            {
                if (value > 0x38)
                {
                    throw new Exception(string.Format("Too many channels : {0}", value));
                }

                this.channels = new Channel[value];

                for (int i = 0; i < value; i++)
                {
                    this.channels[i] = new Channel();
                }
            }
        }

        public Channel[] Channels
        {
            get { return this.channels; }
        }

        public BlendMode BlendMode { get; set; }

        public byte Opacity { get; set; }

        public bool Clipping { get; set; }

        public LayerFlags Flags { get; set; }

        public int Filter { get; set; }

        public long ChannelSize
        {
            get { return this.channels.Select(item => item.Size).Aggregate((v, n) => v + n); }
        }

        public SectionType SectionType
        {
            get { return this.sectionType; }
        }

        public Guid PlacedID
        {
            get { return this.placedID; }
        }

        public string Name
        {
            get { return this.name; }
        }

        public LayerMaskBlock Mask
        {
            get { return this.layerMask; }
        }

        public object BlendingRanges
        {
            get { return this.blendingRanges; }
        }

        public IResourceBlock[] Resources
        {
            get { return this.resources; }
        }

        public int Version
        {
            get { return this.version; }
        }

        public void SetExtraRecords(LayerMaskBlock layerMask, LayerBlendingRangesBlock blendingRanges, IResourceBlock[] resources, string name)
        {
            this.layerMask = layerMask;
            this.blendingRanges = blendingRanges;
            this.resources = resources;
            this.name = name;

            IResourceBlock block = null;

            block = this.Resources.FirstOrDefault(x => x is luniResource);

            if (block != null)
            {
                var luni = (luniResource)block;
                this.name = luni.Name;
            }

            block = this.Resources.FirstOrDefault(x => x is lyvrResource);

            if (block != null)
            {
                var lyvr = (lyvrResource)block;
                this.version = lyvr.Version;
            }

            block = this.Resources.FirstOrDefault(x => x is lsctResource);

            if (block != null)
            {
                var lyvr = (lsctResource)block;
                this.sectionType = lyvr.SectionType;
            }
            else
            {
                block = this.Resources.FirstOrDefault(x => x is lsdkResource);

                if (block != null)
                {
                    var lyvr = (lsdkResource)block;
                    this.sectionType = lyvr.SectionType;
                }
            }

            //if (this.resources.Contains("SoLd.Idnt") == true)
            //       this.placedID = this.resources.ToGuid("SoLd.Idnt");
            //   else if (this.resources.Contains("SoLE.Idnt") == true)
            //       this.placedID = this.resources.ToGuid("SoLE.Idnt");
            placedID = Guid.Empty;

            foreach (var item in this.channels)
            {
                switch (item.Type)
                {
                    case ChannelType.Mask:
                    {
                        if (this.layerMask != null)
                        {
                            item.Width = this.layerMask.Width;
                            item.Height = this.layerMask.Height;
                        }
                    }
                    break;

                    case ChannelType.Alpha:
                    {
                        var iOpablock = this.Resources.FirstOrDefault(x => x is iOpaResource) as iOpaResource;

                        if (iOpablock != null)
                        {
                            var iOpa = (iOpaResource)block;
                            byte opa = iOpa.Opacity;
                            item.Opacity = opa / 255.0f;
                        }
                    }
                    break;
                }
            }
        }

        public void ValidateSize()
        {
            int width = this.Right - Left;
            int height = this.Bottom - this.Top;

            if ((width > 0x3000) || (height > 0x3000))
            {
                throw new NotSupportedException(string.Format("Invalidated size ({0}, {1})", width, height));
            }
        }
    }
}
