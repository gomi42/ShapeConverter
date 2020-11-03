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
            var position = new CharacterPositions();
            var xList = GetLengthPercentList(element, "x", PercentBaseSelector.ViewBoxWidth);
            var dxList = GetLengthPercentList(element, "dx", PercentBaseSelector.ViewBoxWidth);
            position.X.SetParentValues(xList, dxList);

            var yList = GetLengthPercentList(element, "y", PercentBaseSelector.ViewBoxHeight);
            var dyList = GetLengthPercentList(element, "dy", PercentBaseSelector.ViewBoxHeight);
            position.Y.SetParentValues(yList, dyList);

            var fontSize = GetFontSize();
            var typeface = GetTypeface();
            var rotation = new ParentChildPriorityList();
            rotation.ParentValues = GetRotate(element);

            GraphicPathGeometry textGeometry = new GraphicPathGeometry();
            textGeometry.FillRule = GraphicFillRule.NoneZero;

            XNode node = element.FirstNode;
            bool beginOfLine = true;

            while (node != null)
            {
                var nextNode = node.NextNode;
                var hasSuccessor = nextNode != null;

                switch (node)
                {
                    case XElement xElement:
                    {
                        if (xElement.Name.LocalName == "tspan")
                        {
                            cssStyleCascade.PushStyles(xElement);

                            var tspanFontSize = GetFontSize();
                            var tspanTypeface = GetTypeface();
                            rotation.ChildValues = GetRotate(xElement);

                            Vectorize(textGeometry, xElement.Value, position, beginOfLine, hasSuccessor, tspanTypeface, tspanFontSize, rotation, currentTransformationMatrix);

                            rotation.ChildValues = null;
                            cssStyleCascade.Pop();
                        }
                        break;
                    }

                    case XText xText:
                    {
                        Vectorize(textGeometry, xText.Value, position, beginOfLine, hasSuccessor, typeface, fontSize, rotation, currentTransformationMatrix);
                        break;
                    }
                }

                beginOfLine = false;
                node = nextNode;
            }

            return textGeometry;
        }

        /// <summary>
        /// Vectorize a partial string
        /// remove all leading and trailing blanks
        /// shrink multiple blanks in a row to one blank
        protected void Vectorize(GraphicPathGeometry textGeometry,
                              string strVal,
                              CharacterPositions position,
                              bool beginOfLine,
                              bool hasSuccessor,
                              Typeface typeface,
                              double fontSize,
                              ParentChildPriorityList rotation,
                              Matrix currentTransformationMatrix)
        {
            bool blankFound = false;

            foreach (var ch in strVal)
            {
                if (beginOfLine)
                {
                    if (char.IsWhiteSpace(ch) || char.IsControl(ch))
                    {
                        continue;
                    }
                    else
                    {
                        beginOfLine = false;
                        var str = ch.ToString();
                        (position.X.Current, position.Y.Current) = TextVectorizer.Vectorize(textGeometry, ch.ToString(), position.X.Current, position.Y.Current, typeface, fontSize, rotation.GetCurrentOrLast(), currentTransformationMatrix);
                        position.Next();
                        rotation.Next();
                    }
                }
                else
                if (ch == '\n')
                {
                    blankFound = true;
                }
                else
                if (!char.IsControl(ch))
                {
                    if (char.IsWhiteSpace(ch))
                    {
                        blankFound = true;
                    }
                    else
                    {
                        string str;

                        if (blankFound)
                        {
                            (position.X.Current, position.Y.Current) = TextVectorizer.Vectorize(textGeometry, " ", position.X.Current, position.Y.Current, typeface, fontSize, rotation.GetCurrentOrLast(), currentTransformationMatrix);
                            position.Next();
                            rotation.Next();
                            blankFound = false;
                        }

                        str = ch.ToString();
                        (position.X.Current, position.Y.Current) = TextVectorizer.Vectorize(textGeometry, ch.ToString(), position.X.Current, position.Y.Current, typeface, fontSize, rotation.GetCurrentOrLast(), currentTransformationMatrix);
                        position.Next();
                        rotation.Next();
                    }
                }
            }

            if (hasSuccessor && blankFound)
            {
                (position.X.Current, position.Y.Current) = TextVectorizer.Vectorize(textGeometry, " ", position.X.Current, position.Y.Current, typeface, fontSize, rotation.GetCurrentOrLast(), currentTransformationMatrix);
                position.Next();
                rotation.Next();
            }
        }

        /// <summary>
        /// Return a length/percent list or null if the attribute is not set
        /// </summary>
        protected List<double> GetLengthPercentList(XElement element, string attrName, PercentBaseSelector percentBaseSelector)
        {
            XAttribute attr = element.Attribute(attrName);

            if (attr == null)
            {
                return null;
            }

            return doubleParser.GetLengthPercentList(attr.Value, percentBaseSelector);
        }

        /// <summary>
        /// Get the rotation list
        /// </summary>
        protected List<double> GetRotate(XElement element)
        {
            XAttribute xAttr = element.Attribute("rotate");

            if (xAttr == null)
            {
                return null;
            }

            return doubleParser.GetNumberList(xAttr.Value);
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
                return new FontFamily("Global User Interface");
            }

            string[] fontNames = fontFamily.Split(new char[] { ',' });
            FontFamily family;
            family = new FontFamily();

            foreach (string rawFontname in fontNames)
            {
                string fontName = rawFontname.Trim(new char[] { ' ', '\'', '"' });

                switch (fontName)
                {
                    case "serif":
                        return new FontFamily("Global Serif");

                    case "sans-serif":
                        return new FontFamily("Global Sans Serif");

                    case "monospace":
                        return new FontFamily("Global Monospace");

                    default:
                        family = new FontFamily(fontName);
                        var fn = family.FamilyNames.Values;

                        if (fn.Contains(fontName))
                        {
                            return family;
                        }
                        break;
                }
            }

            return new FontFamily("Global User Interface");
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
