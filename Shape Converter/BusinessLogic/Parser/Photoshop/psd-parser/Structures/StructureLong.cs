//
// Author:
//   Michael Göricke
//
// Copyright (c) 2019
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ntreev.Library.Psd.Structures
{
    class StructureLong : StructureItem
    {
        public StructureLong(PsdReader reader)
        {
            Value = reader.ReadInt32();
        }

        public long Value { get; set; }
    }
}
