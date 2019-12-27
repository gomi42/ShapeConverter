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
using System.Windows.Media;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Content.Objects;

namespace ShapeConverter.Parser.Pdf
{
    /// <summary>
    /// PDF helpers
    /// </summary>
    internal static class PdfUtilities
    {
        /// <summary>
        /// Get double from either a real or integer pdf type
        /// </summary>
        public static double GetDouble(PdfItem number)
        {
            if (number is PdfReal real)
            {
                return real.Value;
            }

            if (number is PdfInteger integer)
            {
                return (double)integer.Value;
            }

            return 0;
        }

        /// <summary>
        /// Get double from either a real or integer content type
        /// </summary>
        public static double GetDouble(CObject number)
        {
            if (number is CReal real)
            {
                return real.Value;
            }

            if (number is CInteger integer)
            {
                return (double)integer.Value;
            }

            return 0;
        }

        /// <summary>
        /// Get integer from integer content type
        /// </summary>
        public static int GetInteger(CObject number)
        {
            if (number is CInteger integer)
            {
                return integer.Value;
            }

            return 0;
        }

        /// <summary>
        /// Get a matrix out of a sequence fof operands
        /// </summary>
        public static Matrix GetMatrix(CSequence operands)
        {
            var matrix = new Matrix(PdfUtilities.GetDouble(operands[0]),
                                    PdfUtilities.GetDouble(operands[1]),
                                    PdfUtilities.GetDouble(operands[2]),
                                    PdfUtilities.GetDouble(operands[3]),
                                    PdfUtilities.GetDouble(operands[4]),
                                    PdfUtilities.GetDouble(operands[5]));
            return matrix;
        }

        /// <summary>
        /// Get a matrix out of a sequence fof operands
        /// </summary>
        public static Matrix GetMatrix(PdfArray array)
        {
            var matrix = new Matrix(array.Elements.GetReal(0),
                                    array.Elements.GetReal(1),
                                    array.Elements.GetReal(2),
                                    array.Elements.GetReal(3),
                                    array.Elements.GetReal(4),
                                    array.Elements.GetReal(5));
            return matrix;
        }

        /// <summary>
        /// Convert a operands array to a simple double array
        /// </summary>
        public static List<double> CreateDoubleArray(CSequence operands)
        {
            int num = operands.Count;
            List<double> values = new List<double>();

            for (int i = 0; i < num; i++)
            {
                values.Add(GetDouble(operands[i]));
            }

            return values;
        }

        /// <summary>
        /// Convert a PdfArray to a simple double array
        /// </summary>
        public static List<double> CreateDoubleList(PdfArray array)
        {
            int num = array.Elements.Count;
            List<double> values = new List<double>();

            for (int i = 0; i < num; i++)
            {
                values.Add(array.Elements.GetReal(i));
            }

            return values;
        }

        /// <summary>
        /// Convert a rendering mode number to an enum value
        /// </summary>
        public static FontRenderingMode GetRenderingMode(CObject number)
        {
            var mode = (int)PdfUtilities.GetDouble(number);
            FontRenderingMode fontRenderingMode = FontRenderingMode.Unknown;

            switch (mode)
            {
                case 0:
                    fontRenderingMode = FontRenderingMode.Fill;
                    break;

                case 1:
                    fontRenderingMode = FontRenderingMode.Stroke;
                    break;

                case 2:
                    fontRenderingMode = FontRenderingMode.FillAndStroke;
                    break;

                default:
                    fontRenderingMode = FontRenderingMode.Fill;
                    break;
            }

            return fontRenderingMode;
        }
    }
}
