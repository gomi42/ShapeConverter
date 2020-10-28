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
using System.Windows;
using System.Windows.Media;
using System.Xml.Linq;
using ShapeConverter.BusinessLogic.Generators;
using ShapeConverter.BusinessLogic.Parser.Svg.Helper;
using ShapeConverter.BusinessLogic.ShapeConverter;
using ShapeConverter.Parser.StreamGeometry;

namespace ShapeConverter.BusinessLogic.Parser.Svg.Main
{
    /// <summary>
    /// The GeometryParser parses a single shape into a GraphicPathGeometry
    /// </summary>
    internal class GeometryParser
    {
        private CssStyleCascade cssStyleCascade;
        private DoubleParser doubleParser;

        /// <summary>
        /// Constructor
        /// </summary>
        public GeometryParser(CssStyleCascade cssStyleCascade)
        {
            this.cssStyleCascade = cssStyleCascade;
            doubleParser = new DoubleParser(cssStyleCascade);
        }

        /// <summary>
        /// Parse an SVG shape and return a GraphicPathGeometry
        /// </summary>
        public GraphicPathGeometry Parse(XElement shape,
                                         Matrix currentTransformationMatrix)
        {
            GraphicPathGeometry geometry = null;

            switch (shape.Name.LocalName)
            {
                case "path":
                    geometry = ParsePath(shape);
                    break;

                case "polygon":
                    geometry = ParsePolygon(shape);
                    break;

                case "circle":
                    geometry = ParseCircle(shape);
                    break;

                case "ellipse":
                    geometry = ParseEllipse(shape);
                    break;

                case "rect":
                    geometry = ParseRect(shape);
                    break;

                case "line":
                    geometry = ParseLine(shape);
                    break;

                case "polyline":
                    geometry = ParsePolyline(shape);
                    break;

                case "text":
                    geometry = ParseText(shape);
                    break;
            }

            if (geometry != null)
            {
                var trans = new TransformVisual();
                geometry = trans.Transform(geometry, currentTransformationMatrix);
            }

            return geometry;
        }

        /// <summary>
        /// Parse a path
        /// </summary>
        private GraphicPathGeometry ParsePath(XElement pathElement)
        {
            XAttribute dAttr = pathElement.Attribute("d");

            var streamGeometryParser = new StreamGeometryParser();
            var path = streamGeometryParser.ParseGeometry(dAttr.Value);

            return path;
        }

        /// <summary>
        /// Parse a line
        /// </summary>
        private GraphicPathGeometry ParseLine(XElement path)
        {
            var x1 = GetLengthPercentAttr(path, "x1", PercentBaseSelector.ViewBoxWidth);
            var y1 = GetLengthPercentAttr(path, "y1", PercentBaseSelector.ViewBoxHeight);
            var x2 = GetLengthPercentAttr(path, "x2", PercentBaseSelector.ViewBoxWidth);
            var y2 = GetLengthPercentAttr(path, "y2", PercentBaseSelector.ViewBoxHeight);

            GraphicPathGeometry geometry = new GraphicPathGeometry();
            var move = new GraphicMoveSegment { StartPoint = new Point(x1, y1) };
            move.IsClosed = false;
            geometry.Segments.Add(move);

            var lineTo = new GraphicLineSegment { To = new Point(x2, y2) };
            geometry.Segments.Add(lineTo);

            return geometry;
        }

        /// <summary>
        /// Parse a polygon
        /// </summary>
        private GraphicPathGeometry ParsePolygon(XElement path)
        {
            XAttribute dAttr = path.Attribute("points");
            GraphicPathGeometry geometry = new GraphicPathGeometry();

            var points = doubleParser.GetPointList(dAttr.Value);

            if (points.Count > 1)
            {
                var move = new GraphicMoveSegment { StartPoint = points[0] };
                move.IsClosed = true;
                geometry.Segments.Add(move);

                for (int i = 1; i < points.Count; i++)
                {
                    var lineTo = new GraphicLineSegment { To = points[i] };
                    geometry.Segments.Add(lineTo);
                }
            }

            return geometry;
        }

        /// <summary>
        /// Parse  polyline
        /// </summary>
        private GraphicPathGeometry ParsePolyline(XElement path)
        {
            XAttribute dAttr = path.Attribute("points");
            GraphicPathGeometry geometry = new GraphicPathGeometry();

            var points = doubleParser.GetPointList(dAttr.Value);

            if (points.Count > 1)
            {
                var move = new GraphicMoveSegment { StartPoint = points[0] };
                geometry.Segments.Add(move);
                move.IsClosed = false;

                for (int i = 1; i < points.Count; i++)
                {
                    var lineTo = new GraphicLineSegment { To = points[i] };
                    geometry.Segments.Add(lineTo);
                }
            }

            return geometry;
        }

        /// <summary>
        /// Parse a rectangle
        /// </summary>
        private GraphicPathGeometry ParseRect(XElement path)
        {
            var x = GetLengthPercentAttr(path, "x", PercentBaseSelector.ViewBoxWidth);
            var y = GetLengthPercentAttr(path, "y", PercentBaseSelector.ViewBoxHeight);
            var width = GetLengthPercentAttr(path, "width", PercentBaseSelector.ViewBoxWidth);
            var height = GetLengthPercentAttr(path, "height", PercentBaseSelector.ViewBoxHeight);
            var rx = doubleParser.GetLengthPercentAuto(path, "rx", PercentBaseSelector.ViewBoxWidth);
            var ry = doubleParser.GetLengthPercentAuto(path, "ry", PercentBaseSelector.ViewBoxHeight);

            double rxVal = 0.0;
            double ryVal;

            if (rx.IsAuto && ry.IsAuto)
            {
                rxVal = 0.0;
                ryVal = 0.0;
            }
            else
            {
                if (!rx.IsAuto)
                {
                    rxVal = rx.Value;
                }

                if (!ry.IsAuto)
                {
                    ryVal = ry.Value;
                }
                else
                {
                    ryVal = rxVal;
                }

                if (rx.IsAuto)
                {
                    rxVal = ryVal;
                }

                if (rxVal > width / 2)
                {
                    rxVal = width / 2;
                }

                if (ryVal > height / 2)
                {
                    ryVal = height / 2;
                }
            }

            var rectangle = RectToGeometryConverter.RectToGeometry(new Rect(x, y, width, height), rxVal, ryVal);

            return rectangle;
        }

        /// <summary>
        /// Parse a circle
        /// </summary>
        private GraphicPathGeometry ParseCircle(XElement path)
        {
            var cx = GetLengthPercentAttr(path, "cx", PercentBaseSelector.ViewBoxWidth);
            var cy = GetLengthPercentAttr(path, "cy", PercentBaseSelector.ViewBoxHeight);
            var radius = GetLengthPercentAttr(path, "r", PercentBaseSelector.ViewBoxDiagonal);

            var ellipse = EllipseToGeometryConverter.EllipseToGeometry(new Point(cx, cy), radius, radius);

            return ellipse;
        }

        /// <summary>
        /// Parse an ellipse
        /// </summary>
        private GraphicPathGeometry ParseEllipse(XElement path)
        {
            var cx = GetLengthPercentAttr(path, "cx", PercentBaseSelector.ViewBoxWidth);
            var cy = GetLengthPercentAttr(path, "cy", PercentBaseSelector.ViewBoxHeight);
            var rx = doubleParser.GetLengthPercentAuto(path, "rx", PercentBaseSelector.ViewBoxWidth);
            var ry = doubleParser.GetLengthPercentAuto(path, "ry", PercentBaseSelector.ViewBoxHeight);

            double rxVal;
            double ryVal;

            if (rx.IsAuto)
            {
                rxVal = 0.0;
            }
            else
            {
                rxVal = rx.Value;
            }

            if (ry.IsAuto)
            {
                ryVal = rxVal;
            }
            else
            {
                ryVal = ry.Value;
            }

            if (rx.IsAuto)
            {
                rxVal = ryVal;
            }

            var ellipse = EllipseToGeometryConverter.EllipseToGeometry(new Point(cx, cy), rxVal, ryVal);

            return ellipse;
        }

        /// <summary>
        /// Parse a path
        /// </summary>
        private GraphicPathGeometry ParseText(XElement path)
        {
            var strVal = path.Value.Replace("\n", string.Empty);
            strVal = strVal.Trim();

            var x = GetLengthPercentAttr(path, "x", PercentBaseSelector.ViewBoxWidth);
            var y = GetLengthPercentAttr(path, "y", PercentBaseSelector.ViewBoxHeight);

            var fontFamily = GetFontFamily();
            var fontSize = GetFontSize();
            var fontStyle = GetFontStyle();
            var fontWeight = GetFontWeight();
            var fontStretch = GetFontStretch();

            var typeFace = new Typeface(fontFamily, fontStyle, fontWeight, fontStretch);
            var textGeometry = TextVectorizer.Vectorize(strVal, x, y, typeFace, fontSize);

            return textGeometry;
        }
                                                                        
        /// <summary>
        /// Get the font family
        /// </summary>
        private FontFamily GetFontFamily()
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
        private double GetFontSize()
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

        /// <summary>
        /// Get a double attribute, if it doesn't exist it defaults to 0
        /// </summary>
        private double GetLengthPercentAttr(XElement path, string attrName, PercentBaseSelector percentBaseSelector)
        {
            return doubleParser.GetLengthPercent(path, attrName, 0.0, percentBaseSelector);
        }
    }
}
