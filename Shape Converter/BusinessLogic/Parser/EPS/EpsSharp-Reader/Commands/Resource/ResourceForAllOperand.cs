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

namespace EpsSharp.Eps.Commands.Resource
{
    /// <summary>
    /// ResourceForAll
    /// </summary>
    internal class ResourceForAllOperand : CommandOperand
    {
        public override void Execute(EpsInterpreter interpreter)
        {
            var operandStack = interpreter.OperandStack;

            var category = operandStack.PopStringValue();
            var scratch = operandStack.PopString();
            var proc = operandStack.Pop();
            var template = operandStack.PopStringValue();

            var list = interpreter.ResourceManager.GetFilteredResources(category, template);

            foreach (var key in list)
            {
                var keyName = OperandHelper.TryGetStringValue(key);

                if (keyName != null)
                {
                    scratch.Value = keyName;
                    operandStack.Push(scratch);
                }
                else
                {
                    operandStack.Push(key);
                }

                interpreter.Execute(proc);

                if (interpreter.BreakCurrentLoop)
                {
                    break;
                }
            }
        }
    }
}
