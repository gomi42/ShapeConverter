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
using EpsSharp.Eps.Core;
using EpsSharp.Eps.Helper;

namespace EpsSharp.Eps.Commands.File
{
    /// <summary>
    /// Filter
    /// </summary>
    internal class FilterCmd : CommandOperand
    {
        public override void Execute(EpsInterpreter interpreter)
        {
            var operandStack = interpreter.OperandStack;

            var filterName = operandStack.PopName();

            switch (filterName.Value)
            {
                case "SubFileDecode":
                {
                    var str = operandStack.Pop();

                    switch (str)
                    {
                        case StringOperand stringOperand:
                        {
                            var count = operandStack.PopInteger();
                            var src = operandStack.Pop();

                            switch (src)
                            {
                                case FileOperand fileOperand:
                                    var file = new FileOperand();
                                    operandStack.Push(file);
                                    break;

                                default:
                                    throw new Exception($"filter source not implemented \"{filterName.Value}\"");
                            }
                            break;
                        }

                        default:
                            throw new Exception($"filter type not implemented \"{filterName.Value}\"");
                    }

                    break;
                }

                case "ASCII85Decode":
                case "RunLengthDecode":
                {
                    var src = operandStack.Pop();

                    var file = new FileOperand();
                    operandStack.Push(file);
                    break;
                }

                default:
                    throw new Exception($"filter not implemented \"{filterName.Value}\"");
            }
        }
    }
}
