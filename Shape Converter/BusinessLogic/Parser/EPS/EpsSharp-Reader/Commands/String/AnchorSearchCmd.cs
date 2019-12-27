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

using EpsSharp.Eps.Core;
using EpsSharp.Eps.Helper;

namespace EpsSharp.Eps.Commands.String
{
    /// <summary>
    /// AnchorSearchCmd
    /// </summary>
    internal class AnchorSearchCmd : CommandOperand
    {
        public override void Execute(EpsInterpreter interpreter)
        {
            var operandStack = interpreter.OperandStack;

            var stringOp = operandStack.PopString();
            var stringVal = stringOp.Value;
            var seekOp = operandStack.PopString();
            var seekVal = seekOp.Value;

            if (stringVal.StartsWith(seekVal))
            {
                var post = stringVal.Substring(seekVal.Length);
                var postOp = new StringOperand(post);
                operandStack.Push(postOp);

                operandStack.Push(seekOp);

                var boolOp = new BooleanOperand(true);
                operandStack.Push(boolOp);
            }
            else
            {
                operandStack.Push(stringOp);

                var boolOp = new BooleanOperand(false);
                operandStack.Push(boolOp);
            }
        }
    }
}
