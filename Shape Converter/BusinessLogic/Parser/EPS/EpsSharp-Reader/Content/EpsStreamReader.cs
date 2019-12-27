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
    /// EPS stream reader is the source of a postscript scipt.
    /// This reader:
    /// - supplies the current char
    /// - supplies the next char
    /// - supplies info whether the current char is the first of a line
    /// - supplies info whether the position is at the end of a line
    /// - supplies the current line number for debugging purposes
    /// - move the position to the next char
    /// - move the position to the end of the line
    /// - move the position to the end of the stream
    /// </summary>
    public class EpsStreamReader
    {
        StreamReader reader;

        /// <summary>
        /// Constructor
        /// </summary>
        public EpsStreamReader(StreamReader reader)
        {
            this.reader = reader;
            IsAtEndOfLine = true;
        }

        /// <summary>
        /// The current character
        /// </summary>
        public char CurrentChar { get; private set; }

        /// <summary>
        /// The next character
        /// </summary>
        public char NextChar { get; private set; }

        /// <summary>
        /// The current line number (for debugging purpose)
        /// </summary>
        public int LineNumber { get; private set; }

        /// <summary>
        /// Gets a bool that indicates whether the current 
        /// character is at the beginning of a line
        /// </summary>
        public bool IsFirstCharOfLine { get; private set; }

        /// <summary>
        /// Gets a bool that indicates whether the current 
        /// character is at the end of a line
        /// </summary>
        public bool IsAtEndOfLine { get; private set; }

        /// <summary>
        /// Move to the next character and return it,
        /// ignore CR and LF
        /// </summary>
        public char Read()
        {
            if (reader.EndOfStream)
            {
                NextChar = Chars.EOF;
                return Chars.EOF;
            }

            CurrentChar = (char)reader.Read();
            NextChar = (char)reader.Peek();

            if (CurrentChar == Chars.CR)
            {
                if (NextChar == Chars.LF)
                {
                    // jump over the CR
                    var dummy = reader.Read();
                    NextChar = (char)reader.Peek();
                }

                CurrentChar = Chars.LF;
            }

            IsFirstCharOfLine = false;

            if (IsAtEndOfLine)
            {
                IsAtEndOfLine = false;
                LineNumber++;
                IsFirstCharOfLine = true;
            }

            if (CurrentChar == Chars.LF)
            {
                IsAtEndOfLine = true;
            }

            return CurrentChar;
        }

        /// <summary>
        /// Return the next character without moving the stream position
        /// </summary>
        public char Peek()
        {
            if (reader.EndOfStream)
            {
                return Chars.EOF;
            }

            return NextChar;
        }

        /// <summary>
        /// Read and return until the end of the line
        /// </summary>
        public string ReadLine()
        {
            if (reader.EndOfStream)
            {
                return string.Empty;
            }

            IsFirstCharOfLine = false;

            if (IsAtEndOfLine)
            {
                IsAtEndOfLine = false;
                LineNumber++;
                IsFirstCharOfLine = true;
            }

            IsAtEndOfLine = true;

            var line = reader.ReadLine();
            NextChar = (char)reader.Peek();

            return line;
        }

        /// <summary>
        /// Move the stream position to the very end
        /// </summary>
        public void GotoToEnd()
        {
            if (reader.EndOfStream)
            {
                return;
            }

            IsAtEndOfLine = true;

            reader.ReadToEnd();
        }
    }
}
