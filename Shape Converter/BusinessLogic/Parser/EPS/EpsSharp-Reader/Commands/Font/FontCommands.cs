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

namespace EpsSharp.Eps.Commands.Font
{
    /// <summary>
    /// Register font commands
    /// </summary>
    static internal class FontCommands
    {
        /// <summary>
        /// Register font commands
        /// </summary>
        static public void Register(EpsInterpreter interpreter)
        {
            interpreter.RegisterCommand("awidthshow", new FontNotingCmd());
            interpreter.RegisterCommand("ashow", new FontNotingCmd());
            interpreter.RegisterCommand("cshow", new FontNotingCmd());
            interpreter.RegisterCommand("findfont", new FindFontCmd());
            interpreter.RegisterCommand("show", new FontNotingCmd());
            interpreter.RegisterCommand("widthshow", new FontNotingCmd());
            interpreter.RegisterCommand("xshow", new FontNotingCmd());
            interpreter.RegisterCommand("xyshow", new FontNotingCmd());
            interpreter.RegisterCommand("yshow", new FontNotingCmd());
        }
    }
}
