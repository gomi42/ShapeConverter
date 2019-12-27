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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ntreev.Library.Psd.Readers.LayerResources
{
    /// <summary>
    /// Base class for a bezier descriptor
    /// </summary>
    public abstract class BezierDescriptor
    {
        /// <summary>
        /// The descriptor (type) of the record
        /// </summary>
        public int Descriptor { get; set; }
    }

    /// <summary>
    /// A sub path record
    /// </summary>
    public class BezierSubpathRecord : BezierDescriptor
    {
        public bool IsClosed { get; set; }
        public int NumPoints { get; set; }
    }

    /// <summary>
    /// A bezier knot
    /// </summary>
    public class BezierKnot : BezierDescriptor
    {
        public double Point1X { get; set; }
        public double Point1Y { get; set; }
        public double Point2X { get; set; }
        public double Point2Y { get; set; }
        public double Point3X { get; set; }
        public double Point3Y { get; set; }
    }

    /// <summary>
    /// A fill rule record
    /// </summary>
    public class BezierPathFillRuleRecord : BezierDescriptor
    {
    }

    /// <summary>
    /// A clipboard record
    /// </summary>
    public class BezierClipboardRecord : BezierDescriptor
    {
    }

    /// <summary>
    /// An initial fill rule record
    /// </summary>
    public class BezierInitialFillRuleRecord : BezierDescriptor
    {
        public int FillStartMode { get; set; }
    }

    /// <summary>
    /// Reader for Photoshop 6 backwards compatibility
    /// </summary>
    [ResourceID("vsms")]
    class VsmsResource : vmskResource
    {
        public VsmsResource(PsdReader reader, long length)
            : base(reader, length)
        {
        }
    }

    /// <summary>
    /// Reader of forms
    /// </summary>
    [ResourceID("vmsk")]
    class vmskResource : LazyDataReader, IResourceBlock
    {
        private BezierDescriptor[] beziers;

        public vmskResource(PsdReader reader, long length)
               : base(reader, length)
        {
        }

        public BezierDescriptor[] Beziers
        {
            get
            {
                ReadData();
                return beziers;
            }

            set
            {
                beziers = value;
            }
        }

        protected override void ReadData(PsdReader reader, object userData)
        {
            var version = reader.ReadInt32();
            var flags = reader.ReadUInt32();

            var bezierDescriptors = new List<BezierDescriptor>();

            while (reader.Position + 26 <= EndPosition)
            {
                var descriptor = reader.ReadUInt16();

                switch (descriptor)
                {
                    case 0:
                    {
                        var subpath = new BezierSubpathRecord();
                        bezierDescriptors.Add(subpath);

                        subpath.Descriptor = descriptor;
                        subpath.IsClosed = true;
                        subpath.NumPoints = reader.ReadInt16();
                        reader.ReadBytes(22);
                        break;
                    }

                    case 3:
                    {
                        var subpath = new BezierSubpathRecord();
                        bezierDescriptors.Add(subpath);

                        subpath.Descriptor = descriptor;
                        subpath.NumPoints = reader.ReadInt16();
                        reader.ReadBytes(22);
                        break;
                    }

                    case 1:
                    case 2:
                    case 4:
                    case 5:
                    {
                        var knot = new BezierKnot();
                        bezierDescriptors.Add(knot);

                        knot.Descriptor = descriptor;
                        knot.Point1Y = ReadBezierPoint(reader);
                        knot.Point1X = ReadBezierPoint(reader);
                        knot.Point2Y = ReadBezierPoint(reader);
                        knot.Point2X = ReadBezierPoint(reader);
                        knot.Point3Y = ReadBezierPoint(reader);
                        knot.Point3X = ReadBezierPoint(reader);
                        break;
                    }

                    case 6:
                    {
                        var record = new BezierPathFillRuleRecord();
                        bezierDescriptors.Add(record);

                        record.Descriptor = descriptor;
                        reader.ReadBytes(24);
                        break;
                    }

                    case 7:
                    {
                        var record = new BezierClipboardRecord();
                        bezierDescriptors.Add(record);

                        record.Descriptor = descriptor;
                        reader.ReadBytes(24);
                        break;
                    }

                    case 8:
                    {
                        var record = new BezierInitialFillRuleRecord();
                        bezierDescriptors.Add(record);

                        record.Descriptor = descriptor;
                        record.FillStartMode = reader.ReadInt16();
                        reader.ReadBytes(22);
                        break;
                    }

                    default:
                        reader.ReadBytes(24);
                        break;
                }
            }

            Beziers = bezierDescriptors.ToArray();
        }

        /// <summary>
        /// Read and convert a single point
        /// </summary>
        private double ReadBezierPoint(PsdReader reader)
        {
            var byte1 = reader.ReadByte();
            bool isNegativ = (byte1 & 0xF0) != 0;
            byte intPart = (byte)(byte1 & 0x0F);
            int intVal;

            if (isNegativ)
            {
                intVal = intPart - 16;
            }
            else
            {
                intVal = intPart;
            }

            byte[] bytes = new byte[4];

            bytes[3] = 0;
            bytes[2] = reader.ReadByte();
            bytes[1] = reader.ReadByte();
            bytes[0] = reader.ReadByte();

            var fraction = BitConverter.ToInt32(bytes, 0);

            double ret = intVal + (double)fraction / 16777216.0;

            return ret;
        }
    }
}
