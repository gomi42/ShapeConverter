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
    /// <summary>
    /// A bool
    /// </summary>
    class StructureBool : StructureItem
    {
        public StructureBool(PsdReader reader)
        {
            Value = reader.ReadBoolean();
        }

        public bool Value { get; set; }
    }
}
