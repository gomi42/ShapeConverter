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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ntreev.Library.Psd.Readers;
using Ntreev.Library.Psd.Readers.LayerResources;

namespace Ntreev.Library.Psd
{
    class LayerExtraRecordsX
    {
        private readonly LayerMaskBlock layerMask;
        private readonly LayerBlendingRangesBlock blendingRanges;
        private readonly LayerResourceBlock resources;
        private readonly string name;
        private SectionType sectionType;
        private Guid placedID;

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
            get { return this.resources.Resources; }
        }

        public LayerExtraRecordsX(LayerMaskBlock layerMask,
                                 LayerBlendingRangesBlock blendingRanges,
                                 LayerResourceBlock resources,
                                 string name)
        {
            this.layerMask = layerMask;
            this.blendingRanges = blendingRanges;
            this.resources = resources;
            this.name = name;

            IResourceBlock block = null;

            block = this.resources.Resources.FirstOrDefault(x => x is luniResource);

            if (block != null)
            {
                var luni = (luniResource)block;
                this.name = luni.Name;
            }

            block = this.resources.Resources.FirstOrDefault(x => x is lsctResource);

            if (block != null)
            {
                var lyvr = (lsctResource)block;
                this.sectionType = lyvr.SectionType;
            }

            //if (this.resources.Resources.Contains("SoLd.Idnt") == true)
            //          this.placedID = this.resources.Resources.ToGuid("SoLd.Idnt");
            //      else if (this.resources.Resources.Contains("SoLE.Idnt") == true)
            //          this.placedID = this.resources.Resources.ToGuid("SoLE.Idnt");
            placedID = Guid.Empty;
        }
    }
}
