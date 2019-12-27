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
using System.Collections.Generic;
using EpsSharp.Eps.Core;

namespace EpsSharp.Eps.Commands.Control
{
    /// <summary>
    /// Register control commands
    /// </summary>
    static internal class ControlCommands
    {
        /// <summary>
        /// Register control commands
        /// </summary>
        static public void Register(EpsInterpreter interpreter)
        {
            interpreter.RegisterCommand("countexecstack", new CountExecStackCmd());
            interpreter.RegisterCommand("exec", new ExecCmd());
            interpreter.RegisterCommand("execstack", new ExecStackCmd());
            interpreter.RegisterCommand("exit", new ExitCmd());
            interpreter.RegisterCommand("if", new IfCmd());
            interpreter.RegisterCommand("ifelse", new IfElseCmd());
            interpreter.RegisterCommand("for", new ForCmd());
            interpreter.RegisterCommand("forall", new ForAllCmd());
            interpreter.RegisterCommand("loop", new LoopCmd());
            interpreter.RegisterCommand("quit", new QuitCmd());
            interpreter.RegisterCommand("repeat", new RepeatCmd());
            interpreter.RegisterCommand("stopped", new StoppedCmd());
            interpreter.RegisterCommand("stop", new StopCmd());
        }
    }
}
