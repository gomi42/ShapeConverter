//
// Author:
//   Michael Göricke
//
// Copyright (c) 2019
// inspired by
//   https://github.com/memononen/nanosvg/blob/master/src/nanosvg.h
//   https://code.google.com/p/canvg/
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

namespace ShapeConverter.BusinessLogic.ShapeConverter
{
    /// <summary>
    /// Arc to PathSegment Converter
    /// </summary>
    internal static class ArcToPathSegmentConverter
    {
        private const double Epsilon = 1e-6;

        /// <summary>
        /// Convert an arc definition to a series of bezier segments.
        /// Don't set the start point so that the arc can be part
        /// of a bigger segment list (path).
        /// </summary>
        public static List<GraphicPathSegment> ArcToPathSegments(Point startPoint,
                                                                 Point endPoint,
                                                                 Size size,
                                                                 double rotationAngle,
                                                                 bool isLargeArc,
                                                                 bool sweepDirection)
        {
            double radiusX = size.Width;
            double radiusY = size.Height;

            // start point
            double x1 = startPoint.X;
            double y1 = startPoint.Y;

            // end point
            double x2 = endPoint.X;
            double y2 = endPoint.Y;

            double dx = x1 - x2;
            double dy = y1 - y2;

            double d = Diagonal(dx, dy);

            if (d < Epsilon || radiusX < Epsilon || radiusY < Epsilon)
            {
                // The arc degenerates to a line
                var lineSegments = new List<GraphicPathSegment>();

                var line = new GraphicLineSegment();
                line.To = endPoint;
                lineSegments.Add(line);

                return lineSegments;
            }

            // x rotation angle
            double rotationAngleRad = rotationAngle / 180.0 * Math.PI;
            double sinRotationAngle = Math.Sin(rotationAngleRad);
            double cosRotationAngle = Math.Cos(rotationAngleRad);

            // Convert to center point parameterization.
            // http://www.w3.org/TR/SVG11/implnote.html#ArcImplementationNotes

            // 1) Compute x1', y1'

            double x1p = cosRotationAngle * dx / 2.0 + sinRotationAngle * dy / 2.0;
            double y1p = -sinRotationAngle * dx / 2.0 + cosRotationAngle * dy / 2.0;
            d = Sqr(x1p) / Sqr(radiusX) + Sqr(y1p) / Sqr(radiusY);

            if (d > 1)
            {
                d = Math.Sqrt(d);
                radiusX *= d;
                radiusY *= d;
            }

            // 2) Compute cx', cy'

            double s = 0.0;

            double sa = Sqr(radiusX) * Sqr(radiusY) - Sqr(radiusX) * Sqr(y1p) - Sqr(radiusY) * Sqr(x1p);
            double sb = Sqr(radiusX) * Sqr(y1p) + Sqr(radiusY) * Sqr(x1p);

            if (sa < 0.0)
            {
                sa = 0.0;
            }

            if (sb > 0.0)
            {
                s = Math.Sqrt(sa / sb);
            }

            if (isLargeArc == sweepDirection)
            {
                s = -s;
            }

            double cxp = s * radiusX * y1p / radiusY;
            double cyp = s * -radiusY * x1p / radiusX;

            // 3) Compute cx,cy from cx',cy'

            double cx = (x1 + x2) / 2.0 + cosRotationAngle * cxp - sinRotationAngle * cyp;
            double cy = (y1 + y2) / 2.0 + sinRotationAngle * cxp + cosRotationAngle * cyp;

            // 4) Calculate theta1, and delta theta.

            double ux = (x1p - cxp) / radiusX;
            double uy = (y1p - cyp) / radiusY;

            double vx = (-x1p - cxp) / radiusX;
            double vy = (-y1p - cyp) / radiusY;

            double initialAngle = VectorAngle(1.0, 0.0, ux, uy);
            double deltaAngle = VectorAngle(ux, uy, vx, vy);

            if (!sweepDirection && deltaAngle > 0)
            {
                deltaAngle -= 2 * Math.PI;
            }
            else if (sweepDirection && deltaAngle < 0)
            {
                deltaAngle += 2 * Math.PI;
            }

            Matrix matrix = new Matrix(cosRotationAngle,
                                       sinRotationAngle,
                                       -sinRotationAngle,
                                       cosRotationAngle,
                                       cx,
                                       cy);

            var (anglePerSegment, kappa, numberSegments) = GetSegmentData(deltaAngle);

            if (deltaAngle < 0.0)
            {
                kappa = -kappa;
            }

            var (segments, _, _) = CreateSegments(initialAngle, radiusX, radiusY, numberSegments, anglePerSegment, kappa, matrix);

            return segments;
        }

        /// <summary>
        /// Convert an arc definition to a series of bezier segments.
        /// </summary>
        /// <param name="centerPoint">the center point of the arc</param>
        /// <param name="radius">the radius of the arc</param>
        /// <param name="angle1">the start angle</param>
        /// <param name="angle2">the end angle</param>
        /// <param name="sweepDirection">true = counterclockwise</param>
        /// <returns>list of segments, start point, end point</returns>
        public static (List<GraphicPathSegment>, Point, Point) ArcToPathSegments(Point centerPoint,
                                                                                 double radius,
                                                                                 double angle1,
                                                                                 double angle2,
                                                                                 bool sweepDirection)
        {
            double radiusX = radius;
            double radiusY = radius;

            angle1 -= Math.Truncate(angle1 / 360) * 360;
            angle2 -= Math.Truncate(angle2 / 360) * 360;

            double angle1Rad = angle1 / 180.0 * Math.PI;
            double angle2Rad = angle2 / 180.0 * Math.PI;

            double deltaAngle;

            if (sweepDirection)
            {
                if (angle2Rad <= angle1Rad)
                {
                    angle2Rad += 2 * Math.PI;
                }

                deltaAngle = angle2Rad - angle1Rad;
            }
            else
            {
                if (angle2Rad >= angle1Rad)
                {
                    angle2Rad -= 2 * Math.PI;
                }

                deltaAngle = angle1Rad - angle2Rad;
            }

            Matrix matrix = Matrix.Identity;
            matrix.Rotate(angle1);
            matrix.Translate(centerPoint.X, centerPoint.Y);

            var (anglePerSegment, kappa, numberSegments) = GetSegmentData(deltaAngle);

            if (!sweepDirection)
            {
                kappa = -kappa;
                anglePerSegment = -anglePerSegment;
            }

            return CreateSegments(0, radiusX, radiusY, numberSegments, anglePerSegment, kappa, matrix);
        }

        /// <summary>
        /// Split arc into max 90 degree segments
        /// </summary>
        /// <param name="deltaAngle">the delta angle of the arc</param>
        /// <returns>angle per segment, kappa for the bezier control points, number of segments</returns>
        private static (double, double, int) GetSegmentData(double deltaAngle)
        {
            int numberSegments = 0;
            var testAngle = Math.Abs(deltaAngle);

            do
            {
                numberSegments++;
                testAngle -= Math.PI / 2;
            }
            while (testAngle > Epsilon);

            double anglePerSegment = deltaAngle / (double)numberSegments;
            double anglePerHalfSegment = anglePerSegment / 2.0;
            double kappa = Math.Abs(4.0 / 3.0 * (1.0 - Math.Cos(anglePerHalfSegment)) / Math.Sin(anglePerHalfSegment));

            return (anglePerSegment, kappa, numberSegments);
        }

        /// <summary>
        /// Create the segments that build up the arc
        /// </summary>
        private static (List<GraphicPathSegment>, Point, Point) CreateSegments(double initialAngle,
                                                                               double radiusX,
                                                                               double radiusY,
                                                                               int numberSegments,
                                                                               double anglePerSegment,
                                                                               double kappa,
                                                                               Matrix matrix)
        {
            var segments = new List<GraphicPathSegment>();

            double currentAngle = initialAngle;

            var dx = Math.Cos(currentAngle);
            var dy = Math.Sin(currentAngle);

            Point startPoint = matrix.Transform(new Point(dx * radiusX, dy * radiusY));
            var bezierCurrentPoint = startPoint;
            var previousControlPointTransformation = matrix.Transform(new Vector(-dy * radiusX * kappa, dx * radiusY * kappa));

            for (int i = 0; i < numberSegments; i++)
            {
                currentAngle += anglePerSegment;

                dx = Math.Cos(currentAngle);
                dy = Math.Sin(currentAngle);

                var bezierEndPoint = matrix.Transform(new Point(dx * radiusX, dy * radiusY));
                var controlPointTransformation = matrix.Transform(new Vector(-dy * radiusX * kappa, dx * radiusY * kappa));

                var bezierSegment = new GraphicCubicBezierSegment();

                bezierSegment.ControlPoint1 = new Point(bezierCurrentPoint.X + previousControlPointTransformation.X, bezierCurrentPoint.Y + previousControlPointTransformation.Y);
                bezierSegment.ControlPoint2 = new Point(bezierEndPoint.X - controlPointTransformation.X, bezierEndPoint.Y - controlPointTransformation.Y);
                bezierSegment.EndPoint = bezierEndPoint;

                segments.Add(bezierSegment);

                bezierCurrentPoint = bezierEndPoint;
                previousControlPointTransformation = controlPointTransformation;
            }

            return (segments, startPoint, bezierCurrentPoint);
        }

        /// <summary>
        /// Calculates the square
        /// </summary>
        private static double Sqr(double x)
        {
            return x * x;
        }

        /// <summary>
        /// Calculate the vector angle
        /// </summary>
        private static double VectorAngle(double ux, double uy, double vx, double vy)
        {
            double sin = ux * vy - vx * uy;
            double cos = ux * vx + uy * vy;

            return Math.Atan2(sin, cos);
        }

        /// <summary>
        /// Calculate the diagonal
        /// </summary>
        private static double Diagonal(double x, double y)
        {
            return Math.Sqrt(x * x + y * y);
        }
    }
}
