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
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Xml.Linq;
using ShapeConverter.BusinessLogic.Generators;
using ShapeConverter.BusinessLogic.Parser.Svg.Helper;
using ShapeConverter.BusinessLogic.ShapeConverter;
using ShapeConverter.Parser.StreamGeometry;

namespace ShapeConverter.BusinessLogic.Parser.Svg.Main
{
    internal static class GeometryParser
    {
        /// <summary>
        /// Parse an SVG shape and return a GraphicPathGeometry
        /// </summary>
        public static GraphicPathGeometry Parse(XElement shape,
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
        private static GraphicPathGeometry ParsePath(XElement pathElement)
        {
            XAttribute dAttr = pathElement.Attribute("d");

            var streamGeometryParser = new StreamGeometryParser();
            var path = streamGeometryParser.ParseGeometry(dAttr.Value);

            return path;
        }

        /// <summary>
        /// Parse a line
        /// </summary>
        private static GraphicPathGeometry ParseLine(XElement path)
        {
            var x1 = GetDoubleAttr(path, "x1");
            var y1 = GetDoubleAttr(path, "y1");
            var x2 = GetDoubleAttr(path, "x2");
            var y2 = GetDoubleAttr(path, "y2");

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
        private static GraphicPathGeometry ParsePolygon(XElement path)
        {
            XAttribute dAttr = path.Attribute("points");
            GraphicPathGeometry geometry = new GraphicPathGeometry();

            var pointParser = new DoubleListParser();
            var points = pointParser.ParsePointList(dAttr.Value);

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
        private static GraphicPathGeometry ParsePolyline(XElement path)
        {
            XAttribute dAttr = path.Attribute("points");
            GraphicPathGeometry geometry = new GraphicPathGeometry();

            var pointParser = new DoubleListParser();
            var points = pointParser.ParsePointList(dAttr.Value);

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
        private static GraphicPathGeometry ParseRect(XElement path)
        {
            var x = GetDoubleAttr(path, "x");
            var y = GetDoubleAttr(path, "y");
            var width = GetDoubleAttr(path, "width");
            var height = GetDoubleAttr(path, "height");
            var rx = GetRadiusAttr(path, "rx");
            var ry = GetRadiusAttr(path, "ry");

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
                    if (rx.IsPercentage)
                    {
                        rxVal = width * rx.Value;
                    }
                    else
                    {
                        rxVal = rx.Value;
                    }
                }

                if (!ry.IsAuto)
                {
                    if (ry.IsPercentage)
                    {
                        ryVal = height * ry.Value;
                    }
                    else
                    {
                        ryVal = ry.Value;
                    }
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
        private static GraphicPathGeometry ParseCircle(XElement path)
        {
            var cx = GetDoubleAttr(path, "cx");
            var cy = GetDoubleAttr(path, "cy");
            var radius = GetDoubleAttr(path, "r");

            var ellipse = EllipseToGeometryConverter.EllipseToGeometry(new Point(cx, cy), radius, radius);

            return ellipse;
        }

        /// <summary>
        /// Parse an ellipse
        /// </summary>
        private static GraphicPathGeometry ParseEllipse(XElement path)
        {
            var cx = GetDoubleAttr(path, "cx");
            var cy = GetDoubleAttr(path, "cy");
            var rx = GetRadiusAttr(path, "rx");
            var ry = GetRadiusAttr(path, "ry");

            double rxVal;
            double ryVal;

            if (!rx.IsAuto)
            {
                rxVal = rx.Value;
            }
            else
            {
                rxVal = 0.0;
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

            var ellipse = EllipseToGeometryConverter.EllipseToGeometry(new Point(cx, cy), rxVal, ryVal);

            return ellipse;
        }

        /// <summary>
        /// Get a double attribute, if it doesn't exist it defaults to 0
        /// </summary>
        private static double GetDoubleAttr(XElement path, string attrName)
        {
            return DoubleAttributeParser.GetLength(path, attrName, 0.0);
        }

        /// <summary>
        /// A radius desciption
        /// </summary>
        private struct Radius
        {
            public bool IsAuto { get; set; }

            public bool IsPercentage { get; set; }

            public double Value { get; set; }
        }

        /// <summary>
        /// Get a radius attribute
        /// </summary>
        private static Radius GetRadiusAttr(XElement path, string attrName)
        {
            Radius radius = new Radius();

            XAttribute xAttr = path.Attribute(attrName);

            if (xAttr != null)
            {
                var strVal = xAttr.Value;

                if (strVal == "auto")
                {
                    radius.IsAuto = true;
                }
                else
                {
                    radius.IsPercentage = DoubleParser.ParseLengthPercent(strVal, out double retVal);
                    radius.Value = retVal;
                }
            }
            else
            {
                radius.IsAuto = true;
            }

            return radius;
        }
    }
}
