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

namespace ShapeConverter.Parser.Pdf
{
    /// <summary>
    /// The PDF key words
    /// </summary>
    internal static class PdfKeys
    {
        public const char NameIndicator    = '/';

        // page
        public const string Resources      = "/Resources";
        public const string MediaBox       = "/MediaBox";
        public const string CropBox        = "/CropBox";
        public const string Rotate         = "/Rotate";

        // resources
        public const string Pattern        = "/Pattern";
        public const string ColorSpace     = "/ColorSpace";
        public const string Font           = "/Font";
        public const string ExtGState      = "/ExtGState";

        public const string Subtype        = "/Subtype";
        public const string Image          = "/Image";
        public const string Form           = "/Form";

        // font
        public const string TrueType       = "/TrueType";
        public const string Type1          = "/Type1";
        public const string BaseFont       = "/BaseFont";
        public const string FirstChar      = "/FirstChar";
        public const string LastChar       = "/LastChar";
        public const string Width          = "/Width";
        public const string FontDescriptor = "/FontDescriptor";
        public const string Encoding       = "/Encoding";
        public const string ToUnicode      = "/ToUnicode";
        public const string Name           = "/Name";

        // font descriptor
        public const string FontFile       = "/FontFile";
        public const string FontFile2      = "/FontFile2";
        public const string FontFile3      = "/FontFile3";
        public const string FontFamily     = "/FontFamily";
        public const string FontName       = "/FontName";
        public const string FontStretch    = "/FontStretch";
        public const string ItalicAngle    = "/ItalicAngle";
        public const string FontWeight     = "/FontWeight";

        // color
        public const string DeviceRGB      = "/DeviceRGB";
        public const string DeviceCMYK     = "/DeviceCMYK";
        public const string DeviceGray     = "/DeviceGray";
        public const string DefaultRGB     = "/DefaultRGB";
        public const string DefaultCMYK    = "/DefaultCMYK";
        public const string DefaultGray    = "/DefaultGray";
        public const string DevicePattern  = "/Pattern";
        public const string ICCBased       = "/ICCBased";
        public const string DeviceN        = "/DeviceN";
        public const string Indexed        = "/Indexed";
        public const string CalGray        = "/CalGray";
        public const string CalRGB         = "/CalRGB";
        public const string Lab            = "/Lab";

        // pattern
        public const string PatternType    = "/PatternType";
        public const string Shading        = "/Shading";
        public const string Matrix         = "/Matrix";

        // shading
        public const string ShadingType    = "/ShadingType";
        public const string Extend         = "/Extend";
        public const string Coords         = "/Coords";

        // function
        public const string FunctionType   = "/FunctionType";
        public const string Domain         = "/Domain";
        public const string Range          = "/Range";
        public const string Function       = "/Function";
        public const string Functions      = "/Functions";
        public const string C0             = "/C0";
        public const string C1             = "/C1";
        public const string Bounds         = "/Bounds";
        public const string Encode         = "/Encode";

        // DeviceN
        public const string N              = "/N";

        // ExtGState
        public const string LW             = "/LW";
        public const string J              = "/J";
        public const string j              = "/j";
        public const string d              = "/d";
        public const string CA             = "/CA";
        public const string ca             = "/ca";
        public const string SMask          = "/SMask";

        // Mask
        public const string G              = "/G";

        // optional content
        public const string OCGs           = "/OCGs";
        public const string D              = "/D";
        public const string BaseState      = "/BaseState";
        public const string ON             = "/ON";
        public const string OFF            = "/OFF";
    }
}
