//
// Author:
//   Michael Göricke
//
// Copyright (c) 2019-2020
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
using ShapeConverter.BusinessLogic.Helper;
using ShapeConverter.BusinessLogic.Parser.Svg.Helper;
using ShapeConverter.BusinessLogic.Parser.Svg.Main;

namespace ShapeConverter.BusinessLogic.Parser.Svg
{
    /// <summary>
    /// The ShapeParser parses a single shape into a GraphicVisual
    /// </summary>
    internal class ShapeParser
    {
        private GeometryParser geometryParser;
        private Clipping clipping;
        private Matrix currentTransformationMatrix;
        private CssStyleCascade cssStyleCascade;
        private Dictionary<string, XElement> globalDefinitions;
        private XNamespace svgNamespace;
        private DoubleParser doubleParser;

        /// <summary>
        /// Constructor
        /// </summary>
        public ShapeParser(XNamespace svgNamespace,
                           CssStyleCascade cssStyleCascade,
                           Dictionary<string, XElement> globalDefinitions)
        {
            this.svgNamespace = svgNamespace;
            this.cssStyleCascade = cssStyleCascade;
            this.globalDefinitions = globalDefinitions;

            geometryParser = new GeometryParser(cssStyleCascade);
            clipping = new Clipping(cssStyleCascade, globalDefinitions);
            doubleParser = new DoubleParser(cssStyleCascade);
        }

        /// <summary>
        /// Parse a single SVG shape
        /// </summary>
        public GraphicVisual Parse(XElement shape,
                                   Matrix currentTransformationMatrix)
        {
            GraphicVisual graphicVisual = null;

            cssStyleCascade.PushStyles(shape);

            var transformMatrix = cssStyleCascade.GetTransformMatrixFromTop();
            currentTransformationMatrix = transformMatrix * currentTransformationMatrix;

            var geometry = geometryParser.Parse(shape, currentTransformationMatrix);

            if (geometry != null)
            {
                var graphicPath = new GraphicPath();
                graphicPath.Geometry = geometry;
                graphicVisual = graphicPath;

                this.currentTransformationMatrix = currentTransformationMatrix;

                SetFillAndStroke(shape, graphicPath);

                if (clipping.IsClipPathSet())
                {
                    // shapes don't support clipping, create a group around it
                    var group = new GraphicGroup();
                    graphicVisual = group;
                    group.Childreen.Add(graphicPath);

                    clipping.SetClipPath(group, currentTransformationMatrix);
                }
            }

            cssStyleCascade.Pop();

            return graphicVisual;
        }

        /// <summary>
        /// Set all colors of the graphic path
        /// </summary>
        private void SetFillAndStroke(XElement path, GraphicPath graphicPath)
        {
            // fill
            graphicPath.FillBrush = CreateBrush(path, "fill", true, graphicPath);

            // stroke
            graphicPath.StrokeBrush = CreateBrush(path, "stroke", false, graphicPath);

            if (graphicPath.StrokeBrush != null)
            {
                graphicPath.StrokeThickness = MatrixUtilities.TransformScale(GetLengthPercentFromCascade("stroke-width", 1), currentTransformationMatrix);
                graphicPath.StrokeMiterLimit = MatrixUtilities.TransformScale(cssStyleCascade.GetNumber("stroke-miterlimit", 4), currentTransformationMatrix);
                graphicPath.StrokeLineCap = GetLineCap();
                graphicPath.StrokeLineJoin = GetLineJoin();
                graphicPath.StrokeDashOffset = MatrixUtilities.TransformScale(GetLengthPercentFromCascade("stroke-dashoffset", 0), currentTransformationMatrix) / graphicPath.StrokeThickness;
                graphicPath.StrokeDashes = GetDashes(graphicPath.StrokeThickness);
            }
        }

        /// <summary>
        /// Get a double attribute from a length or percent from the cascade
        /// </summary>
        private double GetLengthPercentFromCascade(string attrName, double defaultValue)
        {
            double retVal;

            var strVal = cssStyleCascade.GetProperty(attrName);

            if (!string.IsNullOrEmpty(strVal))
            {
                retVal = doubleParser.ParseLengthPercent(strVal, PercentBaseSelector.ViewBoxDiagonal);
            }
            else
            {
                retVal = defaultValue;
            }

            return retVal;
        }

        /// <summary>
        /// Gets the line cap
        /// </summary>
        /// <returns></returns>
        private GraphicLineCap GetLineCap()
        {
            GraphicLineCap lineCap = GraphicLineCap.Flat;

            var strVal = cssStyleCascade.GetProperty("stroke-linecap");

            if (string.IsNullOrEmpty(strVal))
            {
                return lineCap;
            }

            switch (strVal)
            {
                case "butt":
                    lineCap = GraphicLineCap.Flat;
                    break;

                case "round":
                    lineCap = GraphicLineCap.Round;
                    break;

                case "square":
                    lineCap = GraphicLineCap.Square;
                    break;
            }

            return lineCap;
        }

        /// <summary>
        /// Get the line join method
        /// </summary>
        /// <returns></returns>
        private GraphicLineJoin GetLineJoin()
        {
            GraphicLineJoin lineJoin = GraphicLineJoin.Miter;

            var strVal = cssStyleCascade.GetProperty("stroke-linejoin");

            if (string.IsNullOrEmpty(strVal))
            {
                return lineJoin;
            }

            switch (strVal)
            {
                case "miter":
                case "miter-clip":
                case "arcs":
                    lineJoin = GraphicLineJoin.Miter;
                    break;

                case "round":
                    lineJoin = GraphicLineJoin.Round;
                    break;

                case "bevel":
                    lineJoin = GraphicLineJoin.Bevel;
                    break;
            }

            return lineJoin;
        }

        /// <summary>
        /// Creates the dashes list. The dashes are convertered from SVG
        /// absolute size to size relative to the given thickness
        /// </summary>
        private List<double> GetDashes(double thickness)
        {
            List<double> dashes = null;

            var strVal = cssStyleCascade.GetProperty("stroke-dasharray");

            if (string.IsNullOrEmpty(strVal) || strVal == "none")
            {
                return dashes;
            }

            var parser = new DoubleListParser();
            var dbls = parser.ParseDoubleList(strVal);

            dashes = new List<double>();

            foreach (var dbl in dbls)
            {
                var dash = MatrixUtilities.TransformScale(dbl, currentTransformationMatrix);
                dashes.Add(dash / thickness);
            }

            return dashes;
        }

        /// <summary>
        /// Create the brush for the given attribute name
        /// </summary>
        private GraphicBrush CreateBrush(XElement element, string name, bool setDefault, GraphicPath graphicPath)
        {
            var strVal = cssStyleCascade.GetProperty(name);

            // 1: there is no value in the cascade
            if (string.IsNullOrEmpty(strVal))
            {
                if (setDefault)
                {
                    var alphad = GetAlpha(name);

                    return new GraphicSolidColorBrush { Color = Color.FromArgb((byte)(alphad * 255), 0x00, 0x00, 0x00) };
                }

                return null;
            }

            strVal = strVal.Trim();

            // 2: "none" is specified
            if (strVal == "none")
            {
                return null;
            }

            var alpha = GetAlpha(name);

            // 3: an url is specified
            if (strVal.StartsWith("url(", StringComparison.OrdinalIgnoreCase))
            {
                int endUri = strVal.IndexOf(")", StringComparison.OrdinalIgnoreCase);
                var uri = strVal.Substring(4, endUri - 4);
                uri = uri.Trim();
                var id = uri.Substring(1);

                var bounds = graphicPath.Geometry.Bounds;

                return CreateGradientBrush(id, alpha, bounds);
            }

            // 4: use the current color
            if (strVal == "currentColor")
            {
                return CreateBrush(element, "color", setDefault, graphicPath);
            }

            // 5: try color formats of different flavours (hex, rgb, name)
            var success = ColorParser.TryParseColor(strVal, alpha, out Color color);

            if (success)
            {
                return new GraphicSolidColorBrush { Color = color };
            }

            return null;
        }

        /// <summary>
        /// Get the opacity value for the specified property
        /// </summary>
        private double GetAlpha(string name)
        {
            var alpha1 = cssStyleCascade.GetNumberPercentFromTop($"{name}-opacity", 1);
            double alpha2 = cssStyleCascade.GetNumberPercentFromTop("opacity", 1);
            var alpha = alpha1 * alpha2;

            return alpha;
        }

        /// <summary>
        /// Create a gradient brush of the specified id
        /// </summary>
        private GraphicBrush CreateGradientBrush(string gradientId, double opacity, Rect bounds)
        {
            GraphicBrush brush = null;

            if (!globalDefinitions.ContainsKey(gradientId))
            {
                return brush;
            }

            var gradientElem = globalDefinitions[gradientId];

            switch (gradientElem.Name.LocalName)
            {
                case "linearGradient":
                {
                    var gradient = new GraphicLinearGradientBrush();
                    brush = gradient;

                    ReadGradientProperties(gradientElem, opacity, gradient);

                    Matrix matrix = GetGradientTransformMatrix(gradientElem);

                    var x = doubleParser.GetLength(gradientElem, "x1", 0);
                    var y = doubleParser.GetLength(gradientElem, "y1", 0);
                    gradient.StartPoint = new Point(x, y) * matrix;

                    x = doubleParser.GetLength(gradientElem, "x2", 1);
                    y = doubleParser.GetLength(gradientElem, "y2", 0);
                    gradient.EndPoint = new Point(x, y) * matrix;

                    // see comment in LinearGradientShading.cs of the pdf parser in GetBrush for more details

                    if (gradient.MappingMode == GraphicBrushMappingMode.Absolute)
                    {
                        gradient.MappingMode = GraphicBrushMappingMode.RelativeToBoundingBox;

                        gradient.StartPoint = GetRelativePosition(bounds, gradient.StartPoint);
                        gradient.EndPoint = GetRelativePosition(bounds, gradient.EndPoint);
                    }

                    break;
                }

                case "radialGradient":
                {
                    var gradient = new GraphicRadialGradientBrush();
                    brush = gradient;

                    ReadGradientProperties(gradientElem, opacity, gradient);

                    Matrix matrix = GetGradientTransformMatrix(gradientElem);

                    var x = doubleParser.GetLength(gradientElem, "cx", 0.5);
                    var y = doubleParser.GetLength(gradientElem, "cy", 0.5);
                    gradient.EndPoint = new Point(x, y) * matrix;

                    x = doubleParser.GetLength(gradientElem, "fx", x);
                    y = doubleParser.GetLength(gradientElem, "fy", y);
                    gradient.StartPoint = new Point(x, y) * matrix;

                    var r = doubleParser.GetLength(gradientElem, "r", 0);
                    gradient.RadiusX = r;
                    gradient.RadiusY = r;

                    // see comment in LinearGradientShading.cs of the pdf parser in GetBrush for more details

                    if (gradient.MappingMode == GraphicBrushMappingMode.Absolute)
                    {
                        gradient.MappingMode = GraphicBrushMappingMode.RelativeToBoundingBox;

                        // calculate the start position relative to the object rectangle
                        gradient.StartPoint = GetRelativePosition(bounds, gradient.StartPoint);

                        // calculate the end position relative to the object rectangle
                        gradient.EndPoint = GetRelativePosition(bounds, gradient.EndPoint);

                        // get the center point and a point on the outer ring
                        // in user space coordinates
                        var centerPointUserSpace = new Point(0, 0) * matrix;
                        var outerPointUserSpace = new Point(1, 1) * matrix;

                        // get the radii in user space
                        var gradientRadiusXUserSpace = Math.Abs(outerPointUserSpace.X - centerPointUserSpace.X);
                        var gradientRadiusYUserSpace = Math.Abs(outerPointUserSpace.Y - centerPointUserSpace.Y);

                        // get the object's size in the user space, we need the radii relative to this size
                        var objectWidth = Math.Abs(bounds.Right - bounds.Left);
                        var objectHeight = Math.Abs(bounds.Bottom - bounds.Top);

                        // calculate the relative radius X
                        var relativeRadiusX = gradientRadiusXUserSpace / objectWidth;
                        gradient.RadiusX = gradient.RadiusX * relativeRadiusX;

                        // calculate the relative radius Y
                        var relativeRadiusY = gradientRadiusYUserSpace / objectHeight;
                        gradient.RadiusY = gradient.RadiusY * relativeRadiusY;
                    }

                    break;
                }
            }

            return brush;
        }

        /// <summary>
        /// Get relative position
        /// </summary>
        private Point GetRelativePosition(Rect rect, Point absPosition)
        {
            var x = (absPosition.X - rect.Left) / (rect.Right - rect.Left);
            var y = (absPosition.Y - rect.Top) / (rect.Bottom - rect.Top);

            return new Point(x, y);
        }

        /// <summary>
        /// Get the transformation matrix of the element
        /// </summary>
        private Matrix GetGradientTransformMatrix(XElement gradientElem)
        {
            Matrix matrix = Matrix.Identity;
            var attrs = gradientElem.Attribute("gradientTransform");

            if (attrs != null)
            {
                matrix = TransformMatrixParser.GetTransformMatrix(attrs.Value);
            }

            return matrix;
        }

        private readonly XNamespace xlink = "http://www.w3.org/1999/xlink";

        /// <summary>
        /// Read the common properties and include the href chain recursively
        /// </summary>
        private void ReadGradientProperties(XElement gradientElem, double opacity, GraphicGradientBrush gradientBrush)
        {
            void ReadHrefGradientPropertiesRecurse(XElement gradientElemRecurse, double opacityRecurse, GraphicGradientBrush gradientBrushRecurse)
            {
                var hrefAttribute = gradientElemRecurse.Attribute(xlink + "href");

                if (hrefAttribute == null)
                {
                    hrefAttribute = gradientElemRecurse.Attribute("href");
                }

                if (hrefAttribute != null)
                {
                    var id = hrefAttribute.Value.Substring(1);

                    if (globalDefinitions.ContainsKey(id))
                    {
                        var hrefElem = globalDefinitions[id];
                        ReadHrefGradientPropertiesRecurse(hrefElem, opacityRecurse, gradientBrushRecurse);
                    }
                }

                GetCommonGradientProperties(gradientElemRecurse, opacityRecurse, gradientBrushRecurse);
            }

            gradientBrush.GradientStops = new List<GraphicGradientStop>();
            gradientBrush.MappingMode = GraphicBrushMappingMode.RelativeToBoundingBox;
            ReadHrefGradientPropertiesRecurse(gradientElem, opacity, gradientBrush);
        }

        /// <summary>
        /// Get common gradient properties for linear and radial gradient
        /// </summary>
        private void GetCommonGradientProperties(XElement gradientElem, double opacity, GraphicGradientBrush gradientBrush)
        {
            var gradientUnitsAttribute = gradientElem.Attribute("gradientUnits");

            if (gradientUnitsAttribute != null)
            {
                switch (gradientUnitsAttribute.Value)
                {
                    case "userSpaceOnUse":
                        gradientBrush.MappingMode = GraphicBrushMappingMode.Absolute;
                        break;

                    case "objectBoundingBox":
                        gradientBrush.MappingMode = GraphicBrushMappingMode.RelativeToBoundingBox;
                        break;
                }
            }

            foreach (var stopElem in gradientElem.Elements(svgNamespace + "stop"))
            {
                var stop = new GraphicGradientStop();
                gradientBrush.GradientStops.Add(stop);

                double retVal;
                (_, retVal) = doubleParser.GetNumberPercent(stopElem, "offset", 0);
                stop.Position = retVal;

                (_, retVal) = doubleParser.GetNumberPercent(stopElem, "stop-opacity", 1);
                var stopOpacity = retVal;

                XAttribute colorAttr = stopElem.Attribute("stop-color");

                if (colorAttr != null
                    && ColorParser.TryParseColor(colorAttr.Value, opacity * stopOpacity, out Color color))
                {
                    stop.Color = color;
                }
                else
                {
                    stop.Color = Colors.Black;
                }
            }
        }
    }
}
