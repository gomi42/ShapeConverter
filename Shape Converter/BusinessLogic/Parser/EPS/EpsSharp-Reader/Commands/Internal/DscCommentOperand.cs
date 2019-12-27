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

using System.Windows;
using System.Windows.Media;
using EpsSharp.Eps.Content;
using EpsSharp.Eps.Core;

namespace EpsSharp.Eps.Commands
{

    /// <summary>
    /// Represents a DSC comment
    /// </summary>
    internal class DscCommentOperand : AlwaysExecuteOperand
    {
        /// <summary>
        /// The parameters
        /// </summary>
        public string Parameters { get; set; }

        /// <summary>
        /// Execute the operand
        /// </summary>
        public override void Execute(EpsInterpreter interpreter)
        {
            switch (Name)
            {
                case "%%EOF":
                {
                    interpreter.FileReader.GotoToEnd();
                    break;
                }

                case "%ADOBeginClientInjection:":
                {
                    // This comment is not an official dsc comment. Illustrator uses it
                    // to enclose weird code. We ignore that code here which makes things
                    // a bit easier.

                    SkipUntil(interpreter.FileReader, "%ADOEndClientInjection");
                    break;
                }

                case "%%BeginData:":
                {
                    SkipUntil(interpreter.FileReader, "%%EndData");
                    break;
                }

                case "%%BeginBinary:":
                {
                    SkipUntil(interpreter.FileReader, "%%EndBinary");
                    break;
                }

                case "%BeginPhotoshop:":
                {
                    SkipUntil(interpreter.FileReader, "%EndPhotoshop");
                    break;
                }

                case "%%BoundingBox:":
                {
                    GetBoundingBox(interpreter, Parameters);
                    break;
                }
            }
        }

        /// <summary>
        /// Read over until the given end is found
        /// </summary>
        private void SkipUntil(EpsStreamReader reader, string end)
        {
            string line = reader.ReadLine();

            while (!line.StartsWith(end))
            {
                line = reader.ReadLine();
            }
        }

        /// <summary>
        /// Get the bounding box
        /// </summary>
        private void GetBoundingBox(EpsInterpreter interpreter, string parameters)
        {
            var parser = new DscParameterParser(parameters);

            // we ignore the (atend) case for a moment and expect 4 ints
            // atend seems to be a very rare case, do we need to take of it at all?

            int x1 = parser.GetNextInt();
            int y1 = parser.GetNextInt();
            int x2 = parser.GetNextInt();
            int y2 = parser.GetNextInt();

            var rect = new Rect(new Point(x1, y1), new Point(x2, y2));
            interpreter.BoundingBox = rect;

            interpreter.GraphicState.Mirror = new Matrix(1, 0, 0, -1, 0, interpreter.BoundingBox.Bottom);


            parser.Dispose();
        }
    }
}
