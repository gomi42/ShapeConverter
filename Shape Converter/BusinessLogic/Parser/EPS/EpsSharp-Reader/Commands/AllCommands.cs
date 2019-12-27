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

using EpsSharp.Eps.Commands.Arithmetic;
using EpsSharp.Eps.Commands.Array;
using EpsSharp.Eps.Commands.Boolean;
using EpsSharp.Eps.Commands.Combined;
using EpsSharp.Eps.Commands.Control;
using EpsSharp.Eps.Commands.Conversion;
using EpsSharp.Eps.Commands.DeviceSetup;
using EpsSharp.Eps.Commands.Dictionary;
using EpsSharp.Eps.Commands.File;
using EpsSharp.Eps.Commands.Font;
using EpsSharp.Eps.Commands.GraphicState;
using EpsSharp.Eps.Commands.Interpreter;
using EpsSharp.Eps.Commands.MatrixCmd;
using EpsSharp.Eps.Commands.Memory;
using EpsSharp.Eps.Commands.Misc;
using EpsSharp.Eps.Commands.Painting;
using EpsSharp.Eps.Commands.Path;
using EpsSharp.Eps.Commands.Pattern;
using EpsSharp.Eps.Commands.Resource;
using EpsSharp.Eps.Commands.Stack;
using EpsSharp.Eps.Commands.String;
using EpsSharp.Eps.Core;

namespace EpsSharp.Eps.Commands
{
    /// <summary>
    /// Register all commands
    /// </summary>
    static internal class AllCommands
    {
        /// <summary>
        /// Register all commands
        /// </summary>
        public static void Register(EpsInterpreter interpreter)
        {
            DictionaryCommands.Register(interpreter);
            StackCommands.Register(interpreter);
            ArithmeticCommands.Register(interpreter);
            CombinedCommands.Register(interpreter);
            PathCommands.Register(interpreter);
            MiscCommands.Register(interpreter);
            GraphicStateCommands.Register(interpreter);
            PaintingCommands.Register(interpreter);
            DeviceSetupCommands.Register(interpreter);
            ArrayCommands.Register(interpreter);
            MatrixCommands.Register(interpreter);
            ControlCommands.Register(interpreter);
            BooleanCommands.Register(interpreter);
            ConversionCommands.Register(interpreter);
            StringCommands.Register(interpreter);
            MemoryCommands.Register(interpreter);
            ResourceCommands.Register(interpreter);
            FontCommands.Register(interpreter);
            InterpreterCommands.Register(interpreter);
            FileCommands.Register(interpreter);
            PatternCommands.Register(interpreter);
        }
    }
}
