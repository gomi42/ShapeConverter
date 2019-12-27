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

using System.Collections.Generic;
using System.Linq;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;
using ShapeConverter.BusinessLogic.Parser.Pdf.Function;

namespace ShapeConverter.Parser.Pdf.ExtendedStates
{
    /// <summary>
    /// The extended states manager
    /// </summary>
    internal class ExtendedStatesManager
    {
        /// <summary>
        /// Base class for a single state
        /// </summary>
        abstract class State
        {
        }

        /// <summary>
        /// line width
        /// </summary>
        class LineWidthState : State
        {
            public double LineWidth;
        }

        /// <summary>
        /// fill alpha
        /// </summary>
        class FillAlphaState : State
        {
            public double Alpha;
        }

        /// <summary>
        /// stoke alpha
        /// </summary>
        class StrokeAlphaState : State
        {
            public double Alpha;
        }

        /// <summary>
        /// font
        /// </summary>
        class FontState : State
        {
            public IFont FontDescriptor;
            public double FontSize;
        }

        /// <summary>
        /// font mask
        /// </summary>
        class SoftMaskState : State
        {
            public List<FunctionStop> SoftMask;
        }

        Dictionary<string, List<State>> extendedGStates;

        /// <summary>
        /// Init
        /// </summary>
        public void Init(PdfResources resourcesDict)
        {
            extendedGStates = new Dictionary<string, List<State>>();

            if (resourcesDict == null)
            {
                return;
            }

            var extGStatesDict = resourcesDict.ExtGStates;

            if (extGStatesDict == null)
            {
                return;
            }

            foreach (var element in extGStatesDict.Elements.Keys)
            {
                var settingsDict = extGStatesDict.Elements.GetDictionary(element);
                extendedGStates[element] = ReadExtendedState(settingsDict);
            }
        }

        /// <summary>
        /// Read an extended state
        /// Note: I'm still not sure whether its worth to precompile the states, it 
        /// might be enough to interprete the states each time they are used.
        /// </summary>
        private List<State> ReadExtendedState(PdfDictionary extGStateDict)
        {
            var extendedState = new List<State>();

            foreach (var command in extGStateDict.Elements.Keys)
            {
                // mögliche Inhalte in den settings siehe in der PDF Reference 1.7, Kapitel 8.4.5, Seite 128, Tabelle 58
                switch (command)
                {
                    case PdfKeys.LW:
                    {
                        var lineWidth = new LineWidthState();
                        extendedState.Add(lineWidth);

                        lineWidth.LineWidth = extGStateDict.Elements.GetReal(command);
                        break;
                    }

                    case PdfKeys.ca:
                    {
                        var fillAlpha = new FillAlphaState();
                        extendedState.Add(fillAlpha);

                        fillAlpha.Alpha = extGStateDict.Elements.GetReal(command);
                        break;
                    }

                    case PdfKeys.CA:
                    {
                        var strokeAlpha = new StrokeAlphaState();
                        extendedState.Add(strokeAlpha);

                        strokeAlpha.Alpha = extGStateDict.Elements.GetReal(command);
                        break;
                    }

                    case PdfKeys.Font:
                    {
                        var font = new FontState();
                        extendedState.Add(font);

                        var fontArray = extGStateDict.Elements.GetArray(command);
                        var fontDict = fontArray.Elements.GetDictionary(0);

                        font.FontDescriptor = FontManager.ReadFontDescriptor(fontDict);
                        font.FontSize = fontArray.Elements.GetReal(1);
                        break;
                    }

                    case PdfKeys.SMask:
                    {
                        var softMaskState = new SoftMaskState();
                        extendedState.Add(softMaskState);

                        var smaskDict = extGStateDict.Elements.GetDictionary(command);

                        if (smaskDict != null)
                        {
                            softMaskState.SoftMask = GetSoftMask(smaskDict);
                        }
                        break;
                    }
                }
            }

            return extendedState;
        }

        /// <summary>
        /// Get the soft mask.
        /// The soft mask is a quite tricky think. Basically it describes a surface and any number
        /// of transition functions with relative stop positions 0..1 and a transparency value for each stop.
        /// Illustrator creates exactly the same area as for the gradient object this soft mask
        /// is applied to. That's why we can shorten thinks and ignore the area, path, ... and 
        /// just read the stop values from the transition function(s).
        /// That makes sense because the Illustrator user creates only one set of stops per
        /// gradient. Each stop contains the position, the color and the transparency.
        /// </summary>
        private List<FunctionStop> GetSoftMask(PdfDictionary smaskDict)
        {
            var transparencyGroupDict = smaskDict.Elements.GetDictionary(PdfKeys.G);

            var resourcesDict = transparencyGroupDict.Elements.GetDictionary(PdfKeys.Resources);

            if (resourcesDict == null)
            {
                return null;
            }

            var shadingsDict = resourcesDict.Elements.GetDictionary(PdfKeys.Shading);

            if (shadingsDict == null)
            {
                return null;
            }

            var key = shadingsDict.Elements.First().Key;
            var shadingDict = shadingsDict.Elements.GetDictionary(key);
            var functionDict = shadingDict.Elements.GetDictionary(PdfKeys.Function);

            var function = FunctionManager.ReadFunction(functionDict);
            var stops = function.GetBoundaryValues();

            return stops;
        }

        /// <summary>
        /// Set the graphic state of the given state name
        /// </summary>
        public void SetExtendedGraphicState(GraphicsState currentGraphicsState, Pdf.FontState fontState, string stateName)
        {
            var extGState = extendedGStates[stateName];

            foreach (var state in extGState)
            {
                switch (state)
                {
                    case LineWidthState lineWidth:
                        currentGraphicsState.LineWidth = lineWidth.LineWidth;
                        break;

                    case FillAlphaState fillAlpha:
                        currentGraphicsState.FillAlpha.Object = fillAlpha.Alpha;
                        break;

                    case StrokeAlphaState strokeAlpha:
                        currentGraphicsState.StrokeAlpha.Object = strokeAlpha.Alpha;
                        break;

                    case FontState font:
                        fontState.FontDescriptor = font.FontDescriptor;
                        fontState.FontSize = font.FontSize;
                        break;

                    case SoftMaskState softMask:
                        currentGraphicsState.SoftMask = softMask.SoftMask;
                        break;
                }
            }
        }
    }
}
