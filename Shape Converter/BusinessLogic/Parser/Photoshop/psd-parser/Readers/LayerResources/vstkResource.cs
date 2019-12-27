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
    /// <summary>
    /// Stroke definitions
    /// </summary>
    [ResourceID("vstk")]
    public class vstkResource : LazyDataReader, IResourceBlock
    {
        private ColorBase color;
        private double width;
        private UnitType widthUnit;

        public vstkResource(PsdReader reader, long length)
          : base(reader, length)
        {
        }

        protected override void ReadData(PsdReader reader, object userData)
        {
            var vstkDescriptor = new StructureDescriptor(reader, true);
            var strokeStyleContentDescriptor = (StructureDescriptor)vstkDescriptor.Items["strokeStyleContent"];

            switch (strokeStyleContentDescriptor.ClassId)
            {
                case "solidColorLayer":
                {
                    Color = ColorReader.GetSolidColor(strokeStyleContentDescriptor);
                    break;
                }

                case "gradientLayer":
                {
                    Color = ColorReader.GetLinearGradientColor(strokeStyleContentDescriptor);
                    break;
                }

                case "patternLayer":
                {
                    var solidColor = new SolidColor();
                    solidColor.Color = ColorReader.GetUnknownColor();
                    Color = solidColor;
                    break;
                }
            }

            var slwv = (StructureUnitFloat)vstkDescriptor.Items["strokeStyleLineWidth"];

            WidthUnit = slwv.Unit;
            Width = slwv.Value;
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

        public double Width
        {
            get
            {
                ReadData();
                return width;
            }

            set
            {
                width = value;
            }
        }

        public UnitType WidthUnit
        {
            get
            {
                ReadData();
                return widthUnit;
            }

            set
            {
                widthUnit = value;
            }
        }
    }
}
