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
//
// With modifications and addition from:
//   Michael Göricke
//
// Copyright (c) 2019
//


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ntreev.Library.Psd.Structures
{
    /// <summary>
    /// One entry in an resource array
    /// </summary>
    public class ArrayResourceItem
    {
        public string Type1 { get; set; }

        public string EnumName { get; set; }

        public UnitType Unit { get; set; }

        public double[] Values { get; set; }
    }

    /// <summary>
    /// An object array
    /// </summary>
    class StructureObjectArray : StructureItem
    {
        public StructureObjectArray(PsdReader reader)
        {
            int version = reader.ReadInt32();
            Name = reader.ReadString();
            ClassId = reader.ReadKey();

            int count = reader.ReadInt32();

            Items = new List<ArrayResourceItem>();

            for (int i = 0; i < count; i++)
            {
                ArrayResourceItem props = new ArrayResourceItem();
                props.Type1 = reader.ReadKey();
                props.EnumName = reader.ReadType();

                props.Unit = PsdUtility.ToUnitType(reader.ReadType());
                int d4 = reader.ReadInt32();
                props.Values = reader.ReadDoubles(d4);

                Items.Add(props);
            }
        }

        public string Name { get; set; }

        public string ClassId { get; set; }

        public List<ArrayResourceItem> Items { get; set; }
    }
}
