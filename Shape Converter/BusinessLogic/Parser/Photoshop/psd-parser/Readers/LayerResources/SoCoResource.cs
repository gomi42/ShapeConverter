//
// Author:
//   Michael Göricke
//
// Copyright (c) 2019
//
// This file is part of ShapeConverter.
//
// ShapeConverter is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see<http://www.gnu.org/licenses/>.

using Ntreev.Library.Psd.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Ntreev.Library.Psd.Readers.LayerResources
{
    /// <summary>
    /// Solid color backwards compatibility
    /// </summary>
    [ResourceID("vscg")]
    class vscgResource : SoCoResource
    {
        public vscgResource(PsdReader reader, long length)
            : base(reader, length)
        {
        }

        protected override void ReadData(PsdReader reader, object userData)
        {
            var id = reader.ReadType();
            base.ReadData(reader, userData);
        }
    }

    /// <summary>
    /// Solid color
    /// </summary>
    [ResourceID("SoCo")]
    public class SoCoResource : LazyDataReader, IResourceBlock
    {
        private SolidColor color;

        public SoCoResource(PsdReader reader, long length)
            : base(reader, length)
        {
        }

        public SolidColor Color
        {
            get
            {
                ReadData();
                return color;
            }

            set
            {
                color = value;
            }
        }

        protected override void ReadData(PsdReader reader, object userData)
        {
            var soCoDescriptor = new StructureDescriptor(reader, true);
            Color = ColorReader.GetSolidColor(soCoDescriptor);
        }
    }
}
