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
using System.Windows.Media;

namespace Ntreev.Library.Psd.Readers.LayerResources
{
    [ResourceID("GdFl")]
    public class GdFlResource : LazyDataReader, IResourceBlock
    {
        private ColorBase color;

        public GdFlResource(PsdReader reader, long length)
                : base(reader, length)
        {
        }

        public ColorBase Color
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
            var colorDescriptor = new StructureDescriptor(reader, true);
            var typeDesc = (StructureEnumerate)colorDescriptor.Items["Type"];

            switch (typeDesc.Value)
            {
                // linear
                case "Lnr ":
                {
                    Color = ColorReader.GetLinearGradientColor(colorDescriptor);
                    break;
                }

                // radial
                case "Rdl ":
                {
                    Color = ColorReader.GetRadialGradientColor(colorDescriptor);
                    break;
                }

                // angle
                case "Angl":

                //reflected
                case "Rflc":

                // diamand
                case "Dmnd":
                default:
                {
                    var solid = new SolidColor();
                    Color = solid;

                    solid.Color = ColorReader.GetUnknownColor();
                    break;
                }
            }
        }
    }
}
