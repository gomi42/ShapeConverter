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

using System.Text;
using EpsSharp.Eps.Content;
using EpsSharp.Eps.Core;
using EpsSharp.Eps.Helper;

namespace EpsSharp.Eps.Commands.File
{
    /// <summary>
    /// ReadString
    /// </summary>
    internal class ReadStringCmd : CommandOperand
    {
        public override void Execute(EpsInterpreter interpreter)
        {
            var operandStack = interpreter.OperandStack;

            var strOp = operandStack.PopString();
            var fileOp = operandStack.PopFile();

            var length = strOp.Value.Length;
            var sb = new StringBuilder();
            bool atEnd = false;

            while (length > 0 && !atEnd)
            {
                var ch = fileOp.Reader.Read();

                if (ch != Chars.EOF)
                {
                    sb.Append(ch);
                    length--;
                }
                else
                {
                    atEnd = true;
                }
            }

            var substrOp = new StringOperand(sb.ToString());
            operandStack.Push(substrOp);
            operandStack.Push(new BooleanOperand(!atEnd));
        }
    }
}
