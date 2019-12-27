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

namespace EpsSharp.Eps.Commands.Path
{
    /// <summary>
    /// Register path commands
    /// </summary>
    static internal class PathCommands
    {
        static public void Register(EpsInterpreter interpreter)
        {
            interpreter.RegisterCommand("arc", new ArcCmd());
            interpreter.RegisterCommand("arcn", new ArcnCmd());
            interpreter.RegisterCommand("arct", new ArctCmd());
            interpreter.RegisterCommand("closepath", new ClosePathCmd());
            interpreter.RegisterCommand("charpath", new CharPathCmd());
            interpreter.RegisterCommand("clip", new ClipCmd());
            interpreter.RegisterCommand("clippath", new ClipPathCmd());
            interpreter.RegisterCommand("currentpoint", new CurrentPointCmd());
            interpreter.RegisterCommand("curveto", new CurveToCmd());
            interpreter.RegisterCommand("eoclip", new EoClipCmd());
            interpreter.RegisterCommand("flattenpath", new FlattenPathCmd());
            interpreter.RegisterCommand("initclip", new InitClipCmd());
            interpreter.RegisterCommand("lineto", new LineToCmd());
            interpreter.RegisterCommand("moveto", new MoveToCmd());
            interpreter.RegisterCommand("newpath", new NewPathCmd());
            //interpreter.AddCmds(new PathBBox());
            //interpreter.AddCmds(new PathForAll());
            interpreter.RegisterCommand("rcurveto", new RcurveToCmd());
            interpreter.RegisterCommand("rectclip", new RectClipCmd());
            interpreter.RegisterCommand("reversepath", new ReversePathCmd());
            interpreter.RegisterCommand("rlineto", new RlineToCmd());
            interpreter.RegisterCommand("rmoveto", new RmoveToCmd());
            interpreter.RegisterCommand("setbbox", new SetBBoxCmd());
            interpreter.RegisterCommand("strokepath", new StrokePathCmd());
            //interpreter.AddCmds(new UCache());
            //interpreter.AddCmds(new UappendCmd());
            //interpreter.AddCmds(new UPathAll());
            interpreter.RegisterCommand("ustrokepath", new UstrokePathCmd());
        }
    }
}
