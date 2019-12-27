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

using Ntreev.Library.Psd;
using Ntreev.Library.Psd.Readers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShapeConverter.Parser.Psd.psd_parser.Readers.ImageResources
{
    [ResourceID("1039", DisplayName = "ICC Profile")]
    class IccProfileResource : LazyDataReader, IResourceBlock
    {
        /// <summary>
        /// Constructur
        /// </summary>
        public IccProfileResource(PsdReader reader, long length)
            : base(reader, length)
        {
        }

        /// <summary>
        /// The profile raw data
        /// </summary>
        public byte[] ProfileData
        {
            get
            {
                var reader = BeginReadData();
                var bytes = reader.ReadBytes((int)this.Length);
                return bytes;
            }
        }

        /// <summary>
        /// Read the data from file
        /// </summary>
        protected override void ReadData(PsdReader reader, object userData)
        {
        }
    }
}
