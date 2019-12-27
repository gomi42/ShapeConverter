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

namespace EpsSharp.Eps.Core
{
    /// <summary>
    /// Postscript keywords
    /// </summary>
    internal static class EpsKeys
    {
        // resources
        public const string Pattern        = "Pattern";
        public const string ColorSpace     = "ColorSpace";

        // color
        public const string DeviceRGB      = "DeviceRGB";
        public const string DeviceCMYK     = "DeviceCMYK";
        public const string DeviceGray     = "DeviceGray";
        public const string DevicePattern  = "Pattern";
        public const string ICCBased       = "ICCBased";
        public const string DeviceN        = "DeviceN";
        public const string Indexed        = "Indexed";
        public const string Separation     = "Separation";
        public const string CIEBasedABC    = "CIEBasedABC";
        public const string CIEBasedA      = "CIEBasedA";

        // pattern
        public const string PatternType    = "PatternType";
        public const string Shading        = "Shading";
        public const string Matrix         = "Matrix";

        // shading
        public const string ShadingType    = "ShadingType";
        public const string Extend         = "Extend";
        public const string Coords         = "Coords";

        // function
        public const string FunctionType   = "FunctionType";
        public const string Domain         = "Domain";
        public const string Range          = "Range";
        public const string Function       = "Function";
        public const string Functions      = "Functions";
        public const string C0             = "C0";
        public const string C1             = "C1";
        public const string Bounds         = "Bounds";
        public const string Encode         = "Encode";
        public const string Decode         = "Decode";
        public const string DataSource     = "DataSource";
        public const string BitsPerSample  = "BitsPerSample";
        public const string Size           = "Size";

        // DeviceN
        public const string N              = "N";
    }
}
