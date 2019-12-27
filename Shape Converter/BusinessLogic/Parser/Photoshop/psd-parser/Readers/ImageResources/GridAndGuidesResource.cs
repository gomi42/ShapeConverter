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

namespace Ntreev.Library.Psd.Readers.ImageResources
{
    [ResourceID("1032", DisplayName = "GridAndGuides")]
    class GridAndGuidesResource : LazyResourceDataReader, IResourceBlock
    {
        private int[] horizontalGuides;
        private int[] verticalGuides;
        private int horizontalGrid;
        private int verticalGrid;

        public GridAndGuidesResource(PsdReader reader, long length)
                : base(reader, length)
        {
        }

        public string ID
        {
            get
            {
                var attrs = typeof(GridAndGuidesResource).GetCustomAttributes(typeof(ResourceIDAttribute), true);
                ResourceIDAttribute attr = attrs.First() as ResourceIDAttribute;
                return attr.ID;
            }
        }

        public int HorizontalGrid
        {
            get
            {
                ReadData();
                return horizontalGrid;
            }

            set
            {
                horizontalGrid = value;
            }
        }

        public int VerticalGrid
        {
            get
            {
                ReadData();
                return verticalGrid;
            }

            set
            {
                verticalGrid = value;
            }
        }


        public int[] HorizontalGuides
        {
            get
            {
                ReadData();
                return horizontalGuides;
            }

            set
            {
                horizontalGuides = value;
            }
        }

        public int[] VerticalGuides
        {
            get
            {
                ReadData();
                return verticalGuides;
            }

            set
            {
                verticalGuides = value;
            }
        }

        protected override void ReadData(PsdReader reader, object userData)
        {
            Properties props = new Properties();

            int version = reader.ReadInt32();

            if (version != 1)
                throw new InvalidFormatException();

            HorizontalGrid = reader.ReadInt32();
            VerticalGrid = reader.ReadInt32();

            int guideCount = reader.ReadInt32();

            List<int> hg = new List<int>();
            List<int> vg = new List<int>();

            for (int i = 0; i < guideCount; i++)
            {
                int n = reader.ReadInt32();
                byte t = reader.ReadByte();

                if (t == 0)
                    vg.Add(n);
                else
                    hg.Add(n);
            }

            HorizontalGuides = hg.ToArray();
            VerticalGuides = vg.ToArray();
        }
    }
}
