//
// Author:
//   Michael Göricke
//
// Copyright (c) 2019
//

using Ntreev.Library.Psd;
using Ntreev.Library.Psd.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ntreev.Library.Psd.Structures
{
    public class StructureDouble : StructureItem
    {
        public StructureDouble(PsdReader reader)
        {
            Value = reader.ReadDouble();
        }

        public double Value { get; set; }
    }
}
