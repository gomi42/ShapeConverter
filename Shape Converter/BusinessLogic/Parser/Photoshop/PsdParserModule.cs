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

namespace ShapeConverter.Parser.Psd
{
    /// <summary>
    /// The psd parser plugin definition
    /// </summary>
    /// <seealso cref="ShapeConverter.Parser.IParserPlugIn" />
    internal class PsdParserPlugIn : IParserPlugIn
    {
        public IParserModule[] ParserModules => new IParserModule[] { new PsdParserModule() };
    }

    /// <summary>
    /// The psd parser module definition
    /// </summary>
    internal class PsdParserModule : IParserModule
    {
        /// <summary>
        /// Gets the supported file extensions.
        /// </summary>
        public string[] SupportedFileExtensions => new string[] { ".psd", ".psb" };

        /// <summary>
        /// Gets the file parser.
        /// </summary>
        public IFileParser FileParser => new PsdParser();
    }

}
