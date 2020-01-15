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
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace EpsSharp.Eps.Content
{
    /// <summary>
    /// The lexer base class
    /// </summary>
    internal class LexerBase
    {
        private Regex doubleRegex = new Regex(@"^([+-]?(?:\d+\.?|\d*\.\d+))(?:[Ee][+-]?\d+)?$");
        private Regex integerRegex = new Regex(@"^([+-]?\d+)$");
        private Regex radixRegex = new Regex(@"^(\d+)#(\d+)$");

        protected StringBuilder Token { get; } = new StringBuilder();

        public string StringToken => Token.ToString();

        public int IntegerToken { get; protected set; }

        /// <summary>
        /// Gets the current token.
        /// </summary>
        public double RealToken { get; protected set; }

        /// <summary>
        /// Scans an integer or real number.
        /// </summary>
        public LexerToken ScanNumber(EpsStreamReader reader)
        {
            ScanNextChar(reader);
            return ScanNumberContinue(reader);
        }

        /// <summary>
        /// Continues scanning an integer or real number when the first char is already read
        /// </summary>
        /// <param name="reader"></param>
        protected LexerToken ScanNumberContinue(EpsStreamReader reader)
        {
            ClearToken();
            var ch = MoveToNonWhiteSpace(reader);

            while (char.IsDigit(ch) || ch == '+' || ch == '-' || ch == 'e' || ch == 'E' || ch == '.' || ch == '#')
            {
                ch = AppendAndScanNextChar(reader);
            }

            var match = radixRegex.Match(StringToken);

            if (match.Success)
            {
                var radix = int.Parse(match.Groups[1].Value);
                IntegerToken = Convert.ToInt32(match.Groups[2].Value, radix);

                return LexerToken.Integer;
            }

            match = integerRegex.Match(StringToken);

            if (match.Success)
            {
                var dbl = double.Parse(StringToken);

                if (int.MinValue <= dbl && dbl <= int.MaxValue)
                {
                    IntegerToken = int.Parse(StringToken);
                 
                    return LexerToken.Integer;
                }

                RealToken = dbl;
                return LexerToken.Real;
            }

            match = doubleRegex.Match(StringToken);

            if (match.Success)
            {
                RealToken = double.Parse(StringToken, CultureInfo.InvariantCulture);

                return LexerToken.Real;
            }

            return LexerToken.Name;
        }

        /// <summary>
        /// Move current position one character further in content stream.
        /// </summary>
        protected char ScanNextChar(EpsStreamReader reader)
        {
            var currChar = reader.Read();

            return currChar;
        }

        private char[] whiteSpaceChars = new char[] { Chars.NUL, Chars.HT, Chars.LF, Chars.FF, Chars.CR, Chars.SP };

        /// <summary>
        /// MoveToNonWhiteSpace
        /// </summary>
        protected char MoveToNonWhiteSpace(EpsStreamReader reader)
        {
            var currChar = reader.CurrentChar;

            while (currChar != Chars.EOF)
            {
                if (!whiteSpaceChars.Contains(currChar))
                {
                    return currChar;
                }

                currChar = ScanNextChar(reader);
            }

            return currChar;
        }

        /// <summary>
        /// Indicates whether the specified character is a content stream white-space character.
        /// </summary>
        internal bool IsWhiteSpace(char ch)
        {
            return whiteSpaceChars.Contains(ch);
        }

        /// <summary>
        /// Appends current character to the token and reads next one.
        /// </summary>
        protected char AppendAndScanNextChar(EpsStreamReader reader)
        {
            Token.Append(reader.CurrentChar);
            return ScanNextChar(reader);
        }

        /// <summary>
        /// Resets the current token to the empty string.
        /// </summary>
        protected void ClearToken()
        {
            Token.Clear();
        }
    }
}
