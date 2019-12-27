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
using System.IO;
using EpsSharp.Eps.Content;
using EpsSharp.Eps.Helper;
using ShapeConverter;

namespace EpsSharp.Eps.Core
{
    /// <summary>
    /// The postscript runner
    /// The postscript interpreter is devided into 2 parts:
    /// - the PostScriptRunner
    /// - the interpreter engine
    /// This is done for a single reason. The engine needs to expose
    /// a lot of properties for the commands. On the other hand we don't
    /// want to expose these "local" properties to the user of the postscript
    /// interpreter. This way the runner just exposes the RunXXX methods
    /// and uses internally the engine to run the script.
    /// </summary>
    public class PostScriptRunner
    {
        /// <summary>
        /// Run the given formula with the given parameters
        /// </summary>
        /// <param name="script"></param>
        public List<double> RunFormula(string script, List<double> parameters)
        {
            List<double> results = null;

            byte[] bytes = System.Text.Encoding.ASCII.GetBytes(script);
            var ms = new MemoryStream(bytes);

            var scriptStream = new StreamReader(ms);

            try
            {
                var interpreter = new EpsInterpreter();
                var operandStack = interpreter.OperandStack;

                foreach (var parameter in parameters)
                {
                    operandStack.Push(new RealOperand(parameter));
                }

                interpreter.Run(scriptStream, 3);

                results = new List<double>();

                while (operandStack.Count > 0)
                {
                    results.Add(operandStack.PopRealValue());
                }

                results.Reverse();

            }
            catch
            {    
            }

            scriptStream.Dispose();
            ms.Dispose();

            return results;
        }

        /// <summary>
        /// Run the interpreter from the given string
        /// </summary>
        /// <param name="script"></param>
        public GraphicGroup RunFromString(string script)
        {
            GraphicGroup graph;
            var disposables = new List<IDisposable>();

            byte[] bytes = System.Text.Encoding.ASCII.GetBytes(script);
            var ms = new MemoryStream(bytes);
            disposables.Add(ms);

            var scriptStream = new StreamReader(ms);
            disposables.Add(scriptStream);

            Run(scriptStream, disposables, 3, out graph);

            return graph;
        }

        /// <summary>
        /// Run the interpreter from the given file
        /// The interpreter doesn't implement the full set of operators in all
        /// levels. It just implements those operators (and a few more) needed to 
        /// read the most common EPS files (e.g. exported from Adobe Illustrator).
        /// Most scripts check the level and use only that operator subset of that level.
        /// In case the file is not readable we try again with a lower postscript
        /// level with the hope those operators aren't needed in that level.
        /// </summary>
        /// <param name="file"></param>
        public GraphicGroup RunFromFile(string file)
        {
            int postscriptVersion = 3;
            GraphicGroup graph;
            bool success;

            do
            {
                var disposables = new List<IDisposable>();

                var reader = new FileStream(file, FileMode.Open, FileAccess.Read);
                disposables.Add(reader);

                var scriptStream = ScanHeader(reader, disposables);

                success = Run(scriptStream, disposables, postscriptVersion, out graph);
                postscriptVersion--;
            }
            while (!success && postscriptVersion > 0);

            return graph;
        }

        /// <summary>
        /// Run the interpreter and dispose everything
        /// </summary>
        private bool Run(StreamReader stream, List<IDisposable> disposables, int postscriptVersion, out GraphicGroup graph)
        {
            bool success = true;
            graph = null;

            try
            {
                var interpreter = new EpsInterpreter();
                graph = interpreter.Run(stream, postscriptVersion);
            }
            catch
            {
                success = false;
            }
            finally
            {
                foreach (var disposable in disposables)
                {
                    disposable.Dispose();
                }
            }

            return success;
        }

        /// <summary>
        /// Scan the header type
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="disposables"></param>
        /// <returns></returns>
        private StreamReader ScanHeader(FileStream reader, List<IDisposable> disposables)
        {
            byte[] buffer = new byte[4];

            reader.Read(buffer, 0, 4);

            if (buffer[0] == '%' && buffer[1] == '!' && buffer[2] == 'P' && buffer[3] == 'S')
            {
                int b;
                char ch;

                do
                {
                    b = reader.ReadByte();
                    ch = (char)b;
                }
                while (ch != Chars.CR && ch != Chars.LF && ch != Chars.EOF);

                var reader2 = new StreamReader(reader);
                disposables.Add(reader2);

                return reader2;
            }
            else
            if (buffer[0] == '\xC5' && buffer[1] == '\xD0' && buffer[2] == '\xD3' && buffer[3] == '\xC6')
            {
                byte[] header = new byte[28];

                reader.Read(header, 0, 28);

                ulong start = ((ulong)header[3] << 24) | ((ulong)header[2] << 16) | ((ulong)header[1] << 8) | (ulong)header[0];
                ulong length = ((ulong)header[7] << 24) | ((ulong)header[6] << 16) | ((ulong)header[5] << 8) | (ulong)header[4];

                var mem = new MemoryStream();
                disposables.Add(mem);

                reader.CopyTo(mem, (int)length);
                mem.Seek(0, SeekOrigin.Begin);

                var reader2 = new StreamReader(mem);
                disposables.Add(reader2);

                return reader2;
            }
            else
            {
                throw new Exception("invalid file format");
            }
        }
    }
}
