#region PDFsharp - A .NET library for processing PDF
//
// Authors:
//   Stefan Lange
//
// Copyright (c) 2005-2017 empira Software GmbH, Cologne Area (Germany)
//
// http://www.pdfsharp.com
// http://sourceforge.net/projects/pdfsharp
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included
// in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Globalization;
using System.Diagnostics;
using System.Linq;

namespace EpsSharp.Eps.Content
{
    /// <summary>
    /// Lexical analyzer for EPS files.
    /// </summary>
    internal class Lexer : LexerBase
    {
        /// <summary>
        /// Gets or sets the current symbol.
        /// </summary>
        public LexerToken Symbol { get; private set; } = LexerToken.Eof;

        /// <summary>
        /// Reads the next token and returns its type.
        /// </summary>
        public LexerToken ScanNextToken(EpsStreamReader reader)
        {
            while (true)
            {
                ClearToken();
                char ch = MoveToNonWhiteSpace(reader);

                switch (ch)
                {
                    case Chars.EOF:
                    {
                        return Symbol = LexerToken.Eof;
                    }

                    case '%':
                    {
                        if (reader.IsFirstCharOfLine)
                        {
                            return Symbol = ScanDscComment(reader);
                        }

                        ScanComment(reader);
                        continue;
                    }

                    case '/':
                    {
                        if (reader.NextChar == '/')
                        {
                            ScanNextChar(reader);
                            ScanNextChar(reader);

                            ScanOperator(reader);

                            return Symbol = LexerToken.ImmediateOperator;
                        }

                        return Symbol = ScanName(reader);
                    }

                    case '[':
                    {
                        ScanNextChar(reader);
                        return Symbol = LexerToken.BeginArray;
                    }

                    case ']':
                    {
                        ScanNextChar(reader);
                        return Symbol = LexerToken.EndArray;
                    }

                    case '{':
                    {
                        ScanNextChar(reader);
                        return Symbol = LexerToken.BeginProcedure;
                    }

                    case '}':
                    {
                        ScanNextChar(reader);
                        return Symbol = LexerToken.EndProcedure;
                    }

                    case '(':
                    {
                        return Symbol = ScanLiteralString(reader);
                    }

                    case '<':
                    {
                        if (reader.NextChar == '<')
                        {
                            ScanNextChar(reader);
                            ScanNextChar(reader);
                            return Symbol = LexerToken.BeginDictionary;
                        }

                        if (reader.NextChar == '~')
                        {
                            return Symbol = ScanAsciiBase85String(reader);
                        }

                        return Symbol = ScanHexadecimalString(reader);
                    }

                    case '>':
                    {
                        if (reader.NextChar == '>')
                        {
                            ScanNextChar(reader);
                            ScanNextChar(reader);
                            return Symbol = LexerToken.EndDictionary;
                        }

                        throw new Exception($"Unexpected Character {ch}");
                    }
                }

                if (char.IsDigit(ch) || ch == '+' || ch == '-' || ch == '.')
                {
                    return Symbol = ScanNumberContinue(reader);
                }

                if (IsOperatorFirstChar(ch))
                {
                    return Symbol = ScanOperator(reader);
                }

                throw new Exception($"Unexpected Character {ch}");
            }
        }

        /// <summary>
        /// Scans a comment
        /// </summary>
        private LexerToken ScanComment(EpsStreamReader reader)
        {
            Debug.Assert(reader.CurrentChar == Chars.Percent);

            ScanNextChar(reader);
            ClearToken();
            char ch;

            do
            {
                ch = AppendAndScanNextChar(reader);
            }
            while (ch != Chars.LF && ch != Chars.EOF);

            return Symbol = LexerToken.Comment;
        }

        /// <summary>
        /// Scans a dsc comment token
        /// </summary>
        private LexerToken ScanDscComment(EpsStreamReader reader)
        {
            Debug.Assert(reader.CurrentChar == Chars.Percent);

            ClearToken();
            char ch;

            do
            {
                ch = AppendAndScanNextChar(reader);
            }
            while (!IsWhiteSpace(ch) && ch != Chars.EOF);

            return Symbol = LexerToken.DscComment;
        }

        /// <summary>
        /// Scans a name.
        /// </summary>
        private LexerToken ScanName(EpsStreamReader reader)
        {
            Debug.Assert(reader.CurrentChar == Chars.Slash);
            ScanNextChar(reader);

            ClearToken();
            char ch;

            do
            {
                ch = AppendAndScanNextChar(reader);
            }
            while (!IsWhiteSpace(ch) && !IsDelimiter(ch));

            return Symbol = LexerToken.Name;
        }

        /// <summary>
        /// Scans an operator.
        /// </summary>
        private LexerToken ScanOperator(EpsStreamReader reader)
        {
            ClearToken();
            char ch = reader.CurrentChar;

            while (IsOperatorFirstChar(ch) || char.IsDigit(ch) || ch == '-')
            {
                ch = AppendAndScanNextChar(reader);
            }

            return Symbol = LexerToken.Operator;
        }

        private LexerToken ScanLiteralString(EpsStreamReader reader)
        {
            Debug.Assert(reader.CurrentChar == Chars.ParenLeft);

            ClearToken();
            int parenLevel = 0;
            char ch = ScanNextChar(reader);

            while (true)
            {
            SkipChar:
                switch (ch)
                {
                    case '(':
                        parenLevel++;
                        break;

                    case ')':
                        if (parenLevel == 0)
                        {
                            ScanNextChar(reader);
                            return Symbol = LexerToken.String;
                        }

                        parenLevel--;
                        break;

                    case '\\':
                    {
                        ch = ScanNextChar(reader);
                        switch (ch)
                        {
                            case 'n':
                                ch = Chars.LF;
                                break;

                            case 'r':
                                ch = Chars.CR;
                                break;

                            case 't':
                                ch = Chars.HT;
                                break;

                            case 'b':
                                ch = Chars.BS;
                                break;

                            case 'f':
                                ch = Chars.FF;
                                break;

                            case '(':
                                ch = Chars.ParenLeft;
                                break;

                            case ')':
                                ch = Chars.ParenRight;
                                break;

                            case '\\':
                                ch = Chars.BackSlash;
                                break;

                            case Chars.LF:
                                ch = ScanNextChar(reader);
                                goto SkipChar;

                            default:
                                if (char.IsDigit(ch))
                                {
                                    // Octal character code.
                                    int n = ch - '0';

                                    if (char.IsDigit(reader.NextChar))
                                    {
                                        n = n * 8 + ScanNextChar(reader) - '0';

                                        if (char.IsDigit(reader.NextChar))
                                        {
                                            n = n * 8 + ScanNextChar(reader) - '0';
                                        }
                                    }

                                    ch = (char)n;
                                }
                                break;
                        }
                        break;
                    }

                    default:
                        // Every other char is appended to the token.
                        break;
                }

                Token.Append(ch);
                ch = ScanNextChar(reader);
            }
        }

        private LexerToken ScanHexadecimalString(EpsStreamReader reader)
        {
            Debug.Assert(reader.CurrentChar == Chars.Less);

            ClearToken();
            char[] hex = new char[2];
            ScanNextChar(reader);

            while (true)
            {
                MoveToNonWhiteSpace(reader);

                if (reader.CurrentChar == '>')
                {
                    ScanNextChar(reader);
                    break;
                }

                if (char.IsLetterOrDigit(reader.CurrentChar))
                {
                    hex[0] = char.ToUpper(reader.CurrentChar);

                    var nextChar = reader.NextChar;

                    if (nextChar != '>')
                    {
                        hex[1] = char.ToUpper(nextChar);
                    }
                    else
                    {
                        hex[1] = '0';
                    }

                    int ch = int.Parse(new string(hex), NumberStyles.AllowHexSpecifier);
                    Token.Append(Convert.ToChar(ch));
                    ScanNextChar(reader);
                    ScanNextChar(reader);
                }
            }

            string chars = Token.ToString();
            int count = chars.Length;

            if (count > 2 && chars[0] == (char)0xFE && chars[1] == (char)0xFF)
            {
                Debug.Assert(count % 2 == 0);
                Token.Length = 0;

                for (int idx = 2; idx < count; idx += 2)
                {
                    Token.Append((char)(chars[idx] * 256 + chars[idx + 1]));
                }
            }

            return Symbol = LexerToken.HexString;
        }

        /// <summary>
        /// Scan ASCII base85 string
        /// </summary>
        /// <param name="reader"></param>
        private LexerToken ScanAsciiBase85String(EpsStreamReader reader)
        {
            ClearToken();
            ScanNextChar(reader);
            ScanNextChar(reader);

            char ch = reader.CurrentChar;

            while (ch != '~' && ch != Chars.EOF)
            {
                ch = AppendAndScanNextChar(reader);
            }

            ScanNextChar(reader);
            ScanNextChar(reader);

            return Symbol = LexerToken.AsciiBase85String;
        }

        private char[] operatorChars = new char[] { Chars.Asterisk, Chars.QuoteSingle, Chars.QuoteDbl, '_', '&', '$', '=', '?', '@', '#'};

        /// <summary>
        /// Indicates whether the specified character is an operator character.
        /// </summary>
        private bool IsOperatorFirstChar(char ch)
        {
            return char.IsLetter(ch) || operatorChars.Contains(ch);
        }

        private char[] delimiterChars = new char[] { '(', ')', '<', '>', '[', ']', '{', '}', '/', '%', '='};

        /// <summary>
        /// Indicates whether the specified character is a delimiter character.
        /// </summary>
        private bool IsDelimiter(char ch)
        {
            return delimiterChars.Contains(ch);
        }
    }
}
