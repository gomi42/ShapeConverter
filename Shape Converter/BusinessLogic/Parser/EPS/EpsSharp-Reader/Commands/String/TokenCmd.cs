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

using System.IO;
using EpsSharp.Eps.Content;
using EpsSharp.Eps.Core;
using EpsSharp.Eps.Helper;

namespace EpsSharp.Eps.Commands.String
{
    /// <summary>
    /// Token
    /// </summary>
    internal class TokenCmd : CommandOperand
    {
        public override void Execute(EpsInterpreter interpreter)
        {
            var operandStack = interpreter.OperandStack;

            var stringVal = operandStack.PopStringValue();

            byte[] bytes = System.Text.Encoding.ASCII.GetBytes(stringVal);
            var memoryStream = new MemoryStream(bytes);
            var scriptReader = new StreamReader(memoryStream);

            var fileReader = new EpsStreamReader(scriptReader);
            var parser = new Parser(fileReader);

            var operand = parser.GetNextOperand();

            if (operand != null)
            {
                var post = scriptReader.ReadToEnd();
                var postOp = new StringOperand(post);
                operandStack.Push(postOp);

                operandStack.Push(operand);

                var boolOp = new BooleanOperand(true);
                operandStack.Push(boolOp);
            }
            else
            {
                var boolOp = new BooleanOperand(false);
                operandStack.Push(boolOp);
            }

            scriptReader.Dispose();
            memoryStream.Dispose();
        }
    }
}
