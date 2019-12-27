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
using ShapeConverter.BusinessLogic.Parser.EPS;
using ShapeConverter.BusinessLogic.Parser.Svg;
using ShapeConverter.Parser.Pdf;
using ShapeConverter.Parser.Psd;

namespace ShapeConverter.Parser
{
    /// <summary>
    /// The file parser
    /// </summary>
    internal class FileParser
    {
        private List<IParserPlugIn> plugins = new List<IParserPlugIn> { new AiParserPlugIn(), new PsdParserPlugIn(), new SvgParserPlugIn(), new EpsParserPlugIn() };

        private Dictionary<string, IParserModule> parserModules;
        private string[] extensions;

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        public void Init()
        {
            parserModules = new Dictionary<string, IParserModule>();
            var exts = new List<string>();

            foreach (var plugin in plugins)
            {
                foreach (var module in plugin.ParserModules)
                {
                    foreach (var ext in module.SupportedFileExtensions)
                    {
                        exts.Add(ext.ToLower());
                        parserModules.Add(ext, module);
                    }
                }
            }

            extensions = exts.ToArray();
        }

        /// <summary>
        /// Gets the supported file extensions.
        /// </summary>
        /// <returns>the supported file extensions</returns>
        public string[] GetSupportedFileExtensions()
        {
            return extensions;
        }

        /// <summary>
        /// Parses the specified filename.
        /// </summary>
        public GraphicVisual Parse(string filename)
        {
            GraphicVisual graphicVisual;

            try
            {
                var ext = System.IO.Path.GetExtension(filename).ToLower();

                parserModules.TryGetValue(ext, out IParserModule parserModule);

                if (parserModule != null)
                {
                    graphicVisual = parserModule.FileParser.Parse(filename);
                }
                else
                {
                    graphicVisual = null;
                }
            }
#pragma warning disable CS0168 // Variable is declared but never used
            catch (Exception e)
#pragma warning restore CS0168 // Variable is declared but never used
            {
                graphicVisual = null;
            }

            return graphicVisual;
        }
    }
}
