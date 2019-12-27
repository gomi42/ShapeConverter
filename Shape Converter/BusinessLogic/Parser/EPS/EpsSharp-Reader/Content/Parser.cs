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
using EpsSharp.Eps.Commands;
using EpsSharp.Eps.Commands.Internal;
using EpsSharp.Eps.Core;

namespace EpsSharp.Eps.Content
{
    /// <summary>
    /// The EPS parser
    /// </summary>
    internal class Parser
    {
        readonly Lexer lexer;
        EpsStreamReader reader;

        /// <summary>
        /// Constructor
        /// </summary>
        public Parser(EpsStreamReader reader)
        {
            // The reader is passed in here but you should know
            // the reader is used at other places as well. The parser
            // has by design no exclusive access to it. That means
            // the state of the reader might change between calls to 
            // the parser and to the lexer.

            this.reader = reader;
            lexer = new Lexer();
        }

        /// <summary>
        /// Gets the next operand
        /// </summary>
        public Operand GetNextOperand()
        {
            var symbol = lexer.ScanNextToken(reader);

            switch (symbol)
            {
                case LexerToken.Eof:
                {
                    return null;
                }

                case LexerToken.DscComment:
                {
                    // We handle each comment that starts at the beginning of a
                    // line as a dsc comment. That enables us to handle some
                    // special Illustrator comments.

                    DscCommentOperand dscOp = new DscCommentOperand();
                    dscOp.LineNumber = reader.LineNumber;
                    dscOp.Name = lexer.StringToken;

                    if (!reader.IsAtEndOfLine)
                    {
                        // The number and types and syntax of the parameters depend
                        // on the dsc comment and vary pretty much. That's why we 
                        // cannot scan and parse the parameters here. Just pass the
                        // pure rest of the line to the dsc comment, it needs to do
                        // the rest :-(

                        var restOfLine = reader.ReadLine();
                        dscOp.Parameters = restOfLine.Trim();
                    }

                    return dscOp;
                }

                case LexerToken.Integer:
                {
                    IntegerOperand n = new IntegerOperand();
                    n.LineNumber = reader.LineNumber;
                    n.Value = lexer.IntegerToken;
                    return n;
                }

                case LexerToken.Real:
                {
                    RealOperand r = new RealOperand();
                    r.LineNumber = reader.LineNumber;
                    r.Value = lexer.RealToken;
                    return r;
                }

                case LexerToken.String:
                {
                    var s = new StringOperand();
                    s.LineNumber = reader.LineNumber;
                    s.Value = lexer.StringToken;
                    s.Type = StringType.Standard;
                    return s;
                }

                case LexerToken.HexString:
                {
                    var s = new StringOperand();
                    s.LineNumber = reader.LineNumber;
                    s.Value = lexer.StringToken;
                    s.Type = StringType.Hex;
                    return s;
                }

                case LexerToken.AsciiBase85String:
                {
                    var s = new StringOperand();
                    s.LineNumber = reader.LineNumber;
                    s.Value = lexer.StringToken;
                    s.Type = StringType.AsciiBase85;
                    return s;
                }

                case LexerToken.Name:
                {
                    NameOperand name = new NameOperand();
                    name.LineNumber = reader.LineNumber;
                    name.Value = lexer.StringToken;
                    return name;
                }

                case LexerToken.Operator:
                {
                    var op = new OperatorOperand();
                    op.LineNumber = reader.LineNumber;
                    op.Name = lexer.StringToken;
                    return op;
                }

                case LexerToken.ImmediateOperator:
                {
                    var op = new ImmediateOperand();
                    op.LineNumber = reader.LineNumber;
                    op.Name = lexer.StringToken;
                    return op;
                }

                case LexerToken.BeginDictionary:
                {
                    var op = new MarkOperand();
                    op.LineNumber = reader.LineNumber;
                    return op;
                }

                case LexerToken.EndDictionary:
                {
                    var op = new EndDictionaryOperand();
                    op.LineNumber = reader.LineNumber;
                    return op;
                }

                case LexerToken.BeginArray:
                {
                    var op = new MarkOperand();
                    op.LineNumber = reader.LineNumber;
                    return op;
                }

                case LexerToken.EndArray:
                {
                    var op = new EndArrayOperand();
                    op.LineNumber = reader.LineNumber;
                    return op;
                }

                case LexerToken.BeginProcedure:
                {
                    var op = new BeginProcedureOperand();
                    op.LineNumber = reader.LineNumber;
                    return op;
                }

                case LexerToken.EndProcedure:
                {
                    var op = new EndProcedureOperand();
                    op.LineNumber = reader.LineNumber;
                    return op;
                }

                default:
                    throw new Exception("unhandled token");
            }
        }
    }
}
