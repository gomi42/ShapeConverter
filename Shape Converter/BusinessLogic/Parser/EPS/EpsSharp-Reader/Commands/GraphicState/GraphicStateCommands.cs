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

namespace EpsSharp.Eps.Commands.GraphicState
{
    /// <summary>
    /// Register the graphic commands
    /// </summary>
    static internal class GraphicStateCommands
    {
        /// <summary>
        /// Register the graphic commands
        /// </summary>
        static public void Register(EpsInterpreter interpreter)
        {
            interpreter.RegisterCommand("clipsave", new GSaveOperand());
            interpreter.RegisterCommand("cliprestore", new GRestoreOperand());
            interpreter.RegisterCommand("currentblackgeneration", new CurrentBlackGenerationOperand());
            interpreter.RegisterCommand("currentcolortransfer", new CurrentColorTransferOperand());
            interpreter.RegisterCommand("currentcmykcolor", new CurrentCMYKColorOperand());
            interpreter.RegisterCommand("currentcolor", new CurrentColorOperand());
            interpreter.RegisterCommand("currentflat", new CurrentFlatOperand());
            interpreter.RegisterCommand("currentgray", new CurrentGrayOperand());
            interpreter.RegisterCommand("currentcolorspace", new CurrentColorSpaceOperand());
            interpreter.RegisterCommand("currenthalftone", new CurrentHalfToneOperand());
            interpreter.RegisterCommand("currentscreen", new CurrentScreenOperand());
            interpreter.RegisterCommand("currentrgbcolor", new CurrentRGBColorOperand());
            interpreter.RegisterCommand("currenttransfer", new CurrentTransferOperand());
            interpreter.RegisterCommand("currentundercolorremoval", new CurrentBlackGenerationOperand());
            interpreter.RegisterCommand("grestore", new GRestoreOperand());
            interpreter.RegisterCommand("grestoreall", new GRestoreOperand());
            interpreter.RegisterCommand("gsave", new GSaveOperand());
            interpreter.RegisterCommand("setoverprint", new SetOverPrintOperand());
            interpreter.RegisterCommand("restore", new RestoreOperand());
            interpreter.RegisterCommand("save", new SaveOperand());
            interpreter.RegisterCommand("setdash", new SetDashOperand());
            interpreter.RegisterCommand("setcolor", new SetColorOperand());
            interpreter.RegisterCommand("setcolorspace", new SetColorSpaceOperand());
            interpreter.RegisterCommand("setgray", new SetGrayOperand());
            interpreter.RegisterCommand("setlinecap", new SetLineCapOperand());
            interpreter.RegisterCommand("setlinejoin", new SetLineJoinOperand());
            interpreter.RegisterCommand("setlinewidth", new SetLineWidthOperand());
            interpreter.RegisterCommand("setmiterlimit", new SetMiterLimitOperand());
            interpreter.RegisterCommand("setrgbcolor", new SetRgbColorOperand());
            interpreter.RegisterCommand("setcmykcolor", new SetCmykColorOperand());
            interpreter.RegisterCommand("setstrokeadjust", new SetStrokeAdjustOperand());
            interpreter.RegisterCommand("setflat", new SetFlatOperand());
            interpreter.RegisterCommand("settransfer", new SetTransferOperand());
            interpreter.RegisterCommand("shfill", new ShFillOperand());
        }
    }
}
