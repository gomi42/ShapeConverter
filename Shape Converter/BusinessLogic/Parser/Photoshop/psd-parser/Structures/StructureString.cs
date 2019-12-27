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
    class StructureString : StructureItem
    {
        public StructureString(PsdReader reader)
        {
            Value = reader.ReadString();
        }

        public string Value { get; set; }
    }
}
