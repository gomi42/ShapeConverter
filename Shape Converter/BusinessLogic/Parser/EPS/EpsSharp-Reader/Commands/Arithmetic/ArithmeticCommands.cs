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

namespace EpsSharp.Eps.Commands.Arithmetic
{
    /// <summary>
    /// Register arithmetic commands
    /// </summary>
    static internal class ArithmeticCommands
    {
        /// <summary>
        /// Register arithmetic commands
        /// </summary>
        static public void Register(EpsInterpreter interpreter)
        {
            interpreter.RegisterCommand("abs", new AbsCmd());
            interpreter.RegisterCommand("add", new AddCmd());
            interpreter.RegisterCommand("ceiling", new CeilingCmd());
            interpreter.RegisterCommand("div", new DivCmd());
            interpreter.RegisterCommand("floor", new FloorCmd());
            interpreter.RegisterCommand("idiv", new IdivCmd());
            interpreter.RegisterCommand("sub", new SubCmd());
            interpreter.RegisterCommand("mul", new MulCmd());
            interpreter.RegisterCommand("mod", new ModCmd());
            interpreter.RegisterCommand("neg", new NegCmd());
            interpreter.RegisterCommand("round", new RoundCmd());
            interpreter.RegisterCommand("sqrt", new SqrtCmd());
            interpreter.RegisterCommand("truncate", new TruncateCmd());
            interpreter.RegisterCommand("atan", new AtanCmd());
            interpreter.RegisterCommand("cos", new CosCmd());
            interpreter.RegisterCommand("sin", new SinCmd());
            interpreter.RegisterCommand("exp", new ExpCmd());
            interpreter.RegisterCommand("ln", new LnCmd());
            interpreter.RegisterCommand("log", new LogCmd());
            interpreter.RegisterCommand("rand", new RandCmd());
            interpreter.RegisterCommand("srand", new SrandCmd());
            interpreter.RegisterCommand("rrand", new RrandCmd());
        }
    }
}
