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
using System.Windows;
using System.Windows.Media;
using System.Xml.Linq;
using ShapeConverter.BusinessLogic.Parser.Svg.Helper;

namespace ShapeConverter.BusinessLogic.Parser.Svg.Main
{
    /// <summary>
    /// The text parser
    /// </summary>
    internal class GeometryTextParser
    {
        protected CssStyleCascade cssStyleCascade;
        protected DoubleParser doubleParser;

        public GeometryTextParser(CssStyleCascade cssStyleCascade,
                                  DoubleParser doubleParser)
        {
            this.cssStyleCascade = cssStyleCascade;
            this.doubleParser = doubleParser;
        }

        /// <summary>
        /// Parse a text into a single geometry
        /// </summary>
        public GraphicPathGeometry ParseTextGeometry(XElement element,
                                                      Matrix currentTransformationMatrix)
        {
            var x = doubleParser.GetLengthPercent(element, "x", 0.0, PercentBaseSelector.ViewBoxWidth);
            var y = doubleParser.GetLengthPercent(element, "y", 0.0, PercentBaseSelector.ViewBoxHeight);

            var fontSize = GetFontSize();
            var typeface = GetTypeface();
            var (_, rotation) = GetRotate(element);
            int rotationIndex = 0;

            GraphicPathGeometry textGeometry = new GraphicPathGeometry();
            textGeometry.FillRule = GraphicFillRule.NoneZero;

            XNode node = element.FirstNode;
            bool prefixBlank = false;

            while (node != null)
            {
                switch (node)
                {
                    case XElement xElement:
                    {
                        if (xElement.Name.LocalName == "tspan")
                        {
                            cssStyleCascade.PushStyles(xElement);

                            var tspanFontSize = GetFontSize();
                            var tspanTypeface = GetTypeface();
                            bool hasRotation;
                            List<double> tspanRotation;

                            (hasRotation, tspanRotation) = GetRotate(xElement);

                            if (!hasRotation)
                            {
                                Vectorize(textGeometry, xElement.Value, ref x, y, prefixBlank, tspanTypeface, tspanFontSize, rotation, ref rotationIndex, currentTransformationMatrix);
                            }
                            else
                            {
                                int rotationIndex2 = 0;
                                rotationIndex += Vectorize(textGeometry, xElement.Value, ref x, y, prefixBlank, tspanTypeface, tspanFontSize, tspanRotation, ref rotationIndex2, currentTransformationMatrix);

                                if (rotationIndex >= rotation.Count)
                                {
                                    rotationIndex = rotation.Count - 1;
                                }
                            }

                            cssStyleCascade.Pop();
                        }
                        break;
                    }

                    case XText xText:
                    {
                        Vectorize(textGeometry, xText.Value, ref x, y, prefixBlank, typeface, fontSize, rotation, ref rotationIndex, currentTransformationMatrix);
                        break;
                    }
                }

                prefixBlank = true;
                node = node.NextNode;
            }

            return textGeometry;
        }

        /// <summary>
        /// Vectorize a partial string
        /// remove all leading and trailing blanks
        /// shrink multiple blanks in a row to one blank
        protected int Vectorize(GraphicPathGeometry textGeometry,
                              string strVal,
                              ref double x,
                              double y,
                              bool prefixBlank,
                              Typeface typeface,
                              double fontSize,
                              List<double> rotation,
                              ref int rotationIndex,
                              Matrix currentTransformationMatrix)
        {
            int numberChars = 0;
            bool evalPrefixBlank = true;
            bool blankFound = false;

            foreach (var ch in strVal)
            {
                if (!char.IsControl(ch))
                {
                    if (char.IsWhiteSpace(ch))
                    {
                        blankFound = true;
                    }
                    else
                    {
                        string str;

                        if (evalPrefixBlank && prefixBlank || !evalPrefixBlank && blankFound)
                        {
                            numberChars++;
                            x = TextVectorizer.Vectorize(textGeometry, " ", x, y, typeface, fontSize, rotation[rotationIndex], currentTransformationMatrix);

                            rotationIndex++;

                            if (rotationIndex >= rotation.Count)
                            {
                                rotationIndex = rotation.Count - 1;
                            }
                        }

                        numberChars++;
                        str = ch.ToString();
                        x = TextVectorizer.Vectorize(textGeometry, ch.ToString(), x, y, typeface, fontSize, rotation[rotationIndex], currentTransformationMatrix);

                        blankFound = false;
                        evalPrefixBlank = false;
                        rotationIndex++;

                        if (rotationIndex >= rotation.Count)
                        {
                            rotationIndex = rotation.Count - 1;
                        }
                    }
                }
            }

            return numberChars;
        }

        /// <summary>
        /// Get the rotation list
        /// </summary>
        protected (bool, List<double>) GetRotate(XElement element)
        {
            XAttribute xAttr = element.Attribute("rotate");

            if (xAttr == null)
            {
                return (false, new List<double>() { 0.0 });
            }

            return (true, doubleParser.GetNumberList(xAttr.Value));
        }

        /// <summary>
        /// Get the type face
        /// </summary>
        protected Typeface GetTypeface()
        {
            var fontFamily = GetFontFamily();
            var fontStyle = GetFontStyle();
            var fontWeight = GetFontWeight();
            var fontStretch = GetFontStretch();

            return new Typeface(fontFamily, fontStyle, fontWeight, fontStretch);
        }

        /// <summary>
        /// Get the font family
        /// </summary>
        protected FontFamily GetFontFamily()
        {
            var fontFamily = cssStyleCascade.GetProperty("font-family");

            if (string.IsNullOrEmpty(fontFamily))
            {
                fontFamily = "Segoe UI";
            }

            return new FontFamily(fontFamily);
        }

        /// <summary>
        /// Get the font size
        /// </summary>
        protected double GetFontSize()
        {
            var fontSizeStr = cssStyleCascade.GetProperty("font-size");
            double fontSize = 16.0;

            if (!string.IsNullOrEmpty(fontSizeStr))
            {
                fontSize = doubleParser.GetLengthPercent(fontSizeStr, PercentBaseSelector.ViewBoxDiagonal);
            }

            return fontSize;
        }

        /// <summary>
        /// Get the font style
        /// </summary>
        private FontStyle GetFontStyle()
        {
            FontStyle fontStyle = FontStyles.Normal;

            var strVal = cssStyleCascade.GetProperty("font-style");

            if (string.IsNullOrEmpty(strVal))
            {
                return fontStyle;
            }

            switch (strVal)
            {
                case "normal":
                    fontStyle = FontStyles.Normal;
                    break;

                case "italic":
                    fontStyle = FontStyles.Italic;
                    break;

                case "oblige":
                    fontStyle = FontStyles.Oblique;
                    break;
            }

            return fontStyle;
        }

        /// <summary>
        /// Get the font weight
        /// </summary>
        private FontWeight GetFontWeight()
        {
            var fontWeight = FontWeights.Normal;

            var strVal = cssStyleCascade.GetProperty("font-weight");

            if (string.IsNullOrEmpty(strVal))
            {
                return fontWeight;
            }

            switch (strVal)
            {
                case "normal":
                    fontWeight = FontWeights.Normal;
                    break;

                case "bold":
                    fontWeight = FontWeights.Bold;
                    break;

                case "bolder":
                    fontWeight = FontWeights.ExtraBold;
                    break;

                case "lighter":
                    fontWeight = FontWeights.Light;
                    break;
            }

            return fontWeight;
        }

        /// <summary>
        /// Get the font stretch
        /// </summary>
        private FontStretch GetFontStretch()
        {
            var fontStretch = FontStretches.Normal;

            var strVal = cssStyleCascade.GetProperty("font-stretch");

            if (string.IsNullOrEmpty(strVal))
            {
                return fontStretch;
            }

            switch (strVal)
            {
                case "normal":
                    fontStretch = FontStretches.Normal;
                    break;

                case "ultra-condensed":
                    fontStretch = FontStretches.UltraCondensed;
                    break;

                case "extra-condensed":
                    fontStretch = FontStretches.ExtraCondensed;
                    break;

                case "condensed":
                    fontStretch = FontStretches.Condensed;
                    break;

                case "semi-condensed":
                    fontStretch = FontStretches.SemiCondensed;
                    break;

                case "semi-expanded":
                    fontStretch = FontStretches.SemiExpanded;
                    break;

                case "expanded":
                    fontStretch = FontStretches.Expanded;
                    break;

                case "extra-expanded":
                    fontStretch = FontStretches.ExtraExpanded;
                    break;

                case "ultra-expanded":
                    fontStretch = FontStretches.UltraExpanded;
                    break;

                default:
                    fontStretch = FontStretches.Normal;
                    break;
            }

            return fontStretch;
        }

    }
}
