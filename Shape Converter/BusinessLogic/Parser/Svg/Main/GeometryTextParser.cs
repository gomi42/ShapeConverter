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
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Xml.Linq;
using ShapeConverter.BusinessLogic.Generators;
using ShapeConverter.BusinessLogic.Parser.Svg.Helper;

namespace ShapeConverter.BusinessLogic.Parser.Svg.Main
{
    /// <summary>
    /// The text parser
    /// </summary>
    internal class GeometryTextParser
    {
        /// <summary>
        /// The text anchor
        /// </summary>
        protected enum TextAnchor
        {
            Start,
            Middle,
            End
        }

        /// <summary>
        /// A position block
        /// </summary>
        protected class PositionBlock
        {
            public PositionBlock()
            {
                Characters = new List<GraphicPath>();
            }

            public TextAnchor TextAnchor;
            public List<GraphicPath> Characters;
        }

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
            var positionBlocks = new List<PositionBlock>();
            var position = new CharacterPositions();

            var xList = GetLengthPercentList(element, "x", PercentBaseSelector.ViewBoxWidth);
            var dxList = GetLengthPercentList(element, "dx", PercentBaseSelector.ViewBoxWidth);
            position.X.SetParentValues(xList, dxList);

            var yList = GetLengthPercentList(element, "y", PercentBaseSelector.ViewBoxHeight);
            var dyList = GetLengthPercentList(element, "dy", PercentBaseSelector.ViewBoxHeight);
            position.Y.SetParentValues(yList, dyList);

            var fontSize = GetFontSize();
            var typeface = GetTypeface();
            var textAnchor = GetTextAnchor();
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
                            var tspanAnchor = GetTextAnchor();
                            rotation.ChildValues = GetRotate(xElement);

                            Vectorize(positionBlocks, xElement.Value, tspanAnchor, position, beginOfLine, hasSuccessor, tspanTypeface, tspanFontSize, rotation, currentTransformationMatrix);

                            rotation.ChildValues = null;
                            cssStyleCascade.Pop();
                        }
                        break;
                    }

                    case XText xText:
                    {
                        Vectorize(positionBlocks, xText.Value, textAnchor, position, beginOfLine, hasSuccessor, typeface, fontSize, rotation, currentTransformationMatrix);
                        break;
                    }
                }

                beginOfLine = false;
                node = nextNode;
            }

            AdjustPosition(positionBlocks);

            return MakeSingleGeometry(positionBlocks);
        }

        /// <summary>
        /// Combine all characters to a single geometry
        /// </summary>
        /// <param name="positionBlocks"></param>
        /// <returns></returns>
        private GraphicPathGeometry MakeSingleGeometry(List<PositionBlock> positionBlocks)
        {
            var geometry = new GraphicPathGeometry();
            geometry.FillRule = GraphicFillRule.NoneZero;

            foreach (var block in positionBlocks)
            {
                geometry.Segments.AddRange(block.Characters.SelectMany(x => x.Geometry.Segments));
            }

            return geometry;
        }

        /// <summary>
        /// Vectorize a partial string
        /// remove all leading and trailing blanks
        /// shrink multiple blanks in a row to one blank
        protected List<GraphicPath> Vectorize(List<PositionBlock> positionBlocks,
                              string strVal,
                              TextAnchor textAnchor,
                              CharacterPositions position,
                              bool beginOfLine,
                              bool hasSuccessor,
                              Typeface typeface,
                              double fontSize,
                              ParentChildPriorityList rotation,
                              Matrix currentTransformationMatrix)
        {
            bool blankFound = false;
            var charBlock = new List<GraphicPath>();

            ////////

            void VectorizeChar(char character)
            {
                var graphicPath = new GraphicPath();
                graphicPath.Geometry.FillRule = GraphicFillRule.NoneZero;
                charBlock.Add(graphicPath);
                PositionBlock positionBlock;

                if (position.X.IsCurrentFromAbsoluteList)
                {
                    positionBlock = new PositionBlock();
                    positionBlock.TextAnchor = textAnchor;
                    positionBlocks.Add(positionBlock);
                }
                else
                {
                    positionBlock = positionBlocks.Last();
                }

                positionBlock.Characters.Add(graphicPath);

                (position.X.Current, position.Y.Current) = TextVectorizer.Vectorize(graphicPath.Geometry, character.ToString(), position.X.Current, position.Y.Current, typeface, fontSize, rotation.GetCurrentOrLast(), currentTransformationMatrix);
                position.Next();
                rotation.Next();
            }

            ////////

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
                        VectorizeChar(ch);
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
                        if (blankFound)
                        {
                            VectorizeChar(' ');
                            blankFound = false;
                        }

                        VectorizeChar(ch);
                    }
                }
            }

            if (hasSuccessor && blankFound)
            {
                VectorizeChar(' ');
            }

            return charBlock;
        }

        /// <summary>
        /// Adjust the position of each character according to the anchor of its block
        /// </summary>
        protected void AdjustPosition(List<PositionBlock> positionBlocks)
        {
            foreach (var block in positionBlocks)
            {
                if (block.TextAnchor == TextAnchor.Start)
                {
                    continue;
                }

                Rect blockBounds = Rect.Empty;

                foreach (var ch in block.Characters)
                {
                    var bounds = ch.Geometry.Bounds;
                    blockBounds = Rect.Union(blockBounds, bounds);
                }

                double translateX = 0.0;

                switch (block.TextAnchor)
                {
                    case TextAnchor.Middle:
                        translateX = -blockBounds.Width / 2;
                        break;

                    case TextAnchor.End:
                        translateX = -blockBounds.Width;
                        break;
                }

                var matrix = Matrix.Identity;
                matrix.Translate(translateX, 0);

                var transformVisual = new TransformVisual();

                foreach (var ch in block.Characters)
                {
                    ch.Geometry = transformVisual.Transform(ch.Geometry, matrix);
                }
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
        /// Get the text anchor
        /// </summary>
        protected TextAnchor GetTextAnchor()
        {
            TextAnchor textAnchor = TextAnchor.Start;
            var anchor = cssStyleCascade.GetProperty("text-anchor");

            if (anchor == null)
            {
                return textAnchor;
            }

            switch (anchor)
            {
                case "start":
                    textAnchor = TextAnchor.Start;
                    break;

                case "middle":
                    textAnchor = TextAnchor.Middle;
                    break;

                case "end":
                    textAnchor = TextAnchor.End;
                    break;
            }

            return textAnchor;
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
                        var familyNames = family.FamilyNames.Values;

                        if (familyNames.Contains(fontName))
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

            if (DoubleParser.TryGetNumber(strVal, out double number))
            {
                if (number <= 100)
                {
                    return FontWeights.Thin;
                }

                if (number <= 300)
                {
                    return FontWeights.ExtraLight;
                }

                if (number <= 400)
                {
                    return FontWeights.Normal;
                }

                if (number <= 500)
                {
                    return FontWeights.Medium;
                }

                if (number <= 600)
                {
                    return FontWeights.SemiBold;
                }

                if (number <= 700)
                {
                    return FontWeights.Bold;
                }

                if (number <= 800)
                {
                    return FontWeights.ExtraBold;
                }

                if (number <= 900)
                {
                    return FontWeights.Black;
                }

                return FontWeights.UltraBlack;
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
