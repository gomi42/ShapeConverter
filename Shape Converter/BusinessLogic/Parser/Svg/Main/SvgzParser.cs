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
using System.IO;
using System.IO.Compression;
using System.Xml.Linq;
using ShapeConverter.Parser;

namespace ShapeConverter.BusinessLogic.Parser.Svg
{
    /// <summary>
    /// The SVGZ parser
    /// </summary>
    internal class SvgzParser : IFileParser
    {
        /// <summary>
        /// Parse the given file
        /// </summary>
        GraphicVisual IFileParser.Parse(string filename)
        {
            GraphicVisual visual = null;

            try
            {
                using (var file = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    using (GZipStream decompressionStream = new GZipStream(file, CompressionMode.Decompress))
                    {
                        var root = XElement.Load(decompressionStream);

                        var svgParser = new SvgParser();
                        visual = svgParser.ParseRoot(root);
                    }
                }
            }
#pragma warning disable CS0168 // Variable is declared but never used
            catch (Exception e)
#pragma warning restore CS0168 // Variable is declared but never used
            {
            }

            return visual;
        }
    }
}
