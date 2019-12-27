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

namespace EpsSharp.Eps.Content
{
    /// <summary>
    /// DCS comment parameter parser
    /// </summary>
    internal class DscParameterParser : IDisposable
    {
        private MemoryStream ms;
        private StreamReader stream;
        private EpsStreamReader reader;
        private LexerBase lexer;

        /// <summary>
        /// Contructor
        /// </summary>
        /// <param name="parameters"></param>
        public DscParameterParser(string parameters)
        {
            byte[] bytes = System.Text.Encoding.ASCII.GetBytes(parameters);
            ms = new MemoryStream(bytes);
            stream = new StreamReader(ms);
            this.reader = new EpsStreamReader(stream);

            lexer = new LexerBase();
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            stream.Dispose();
            ms.Dispose();
        }

        /// <summary>
        /// Returns the next integer
        /// </summary>
        public int GetNextInt()
        {
            var token = lexer.ScanNumber(reader);

            if (token != LexerToken.Integer)
            {
                throw new Exception($"integer expected");
            }

            return lexer.IntegerToken;
        }
    }
}
