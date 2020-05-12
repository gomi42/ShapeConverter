//
// Author:
//   Michael Göricke
//
// Copyright (c) 2019
// Inspired by SharpVectors
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
using System.Text.RegularExpressions;
using System.Windows.Media;

namespace ShapeConverter.BusinessLogic.Parser.Svg
{
    internal static class TransformMatrixParser
    {
        private static Regex _regExtract = new Regex("([A-Za-z]+)\\s*\\(([\\-0-9\\.eE+-\\,\\s]+)\\)", RegexOptions.Compiled);

        /// <summary>
        /// Parse multiple transformations and return the resulting matrix
        /// </summary>
        public static Matrix GetTransformMatrix(string transform)
        {
            Matrix result = Matrix.Identity;

            if (!string.IsNullOrWhiteSpace(transform))
            {
                Match match = _regExtract.Match(transform);

                while (match.Success)
                {
                    var transformMatrix = GetSingleTransformMatrix(match.Value);
                    result = transformMatrix * result;

                    match = match.NextMatch();
                }
            }

            return result;
        }

        /// <summary>
        /// Parse single transformation
        /// </summary>
        static Matrix GetSingleTransformMatrix(string str)
        {
            Matrix matrix = Matrix.Identity;

            int start = str.IndexOf("(", StringComparison.OrdinalIgnoreCase);
            string type = str.Substring(0, start);
            string valuesList = (str.Substring(start + 1, str.Length - start - 2)).Trim();
            Regex re = new Regex("[\\s\\,]+");
            valuesList = re.Replace(valuesList, ",");

            string[] valuesStr = valuesList.Split(new char[] { ',' });
            int len = valuesStr.GetLength(0);
            double[] values = new double[len];

            try
            {
                for (int i = 0; i < len; i++)
                {
                    values[i] = double.Parse(valuesStr[i], System.Globalization.CultureInfo.InvariantCulture);
                }
            }
            catch
            {
                return matrix;
            }

            switch (type.Trim())
            {
                case "translate":
                    switch (len)
                    {
                        case 1:
                            matrix.Translate(values[0], 0);
                            break;
                        case 2:
                            matrix.Translate(values[0], values[1]);
                            break;
                        default:
                            throw new ApplicationException("Wrong number of arguments in translate transform");
                    }
                    break;

                case "rotate":
                    switch (len)
                    {
                        case 1:
                            matrix.Rotate(values[0]);
                            break;
                        case 3:
                            matrix.RotateAt(values[0], values[1], values[2]);
                            break;
                        default:
                            throw new ApplicationException("Wrong number of arguments in rotate transform");
                    }
                    break;

                case "scale":
                    switch (len)
                    {
                        case 1:
                            matrix.Scale(values[0], values[0]);
                            break;
                        case 2:
                            matrix.Scale(values[0], values[1]);
                            break;
                        default:
                            throw new ApplicationException("Wrong number of arguments in scale transform");
                    }
                    break;

                case "skewX":
                    if (len != 1)
                        throw new ApplicationException("Wrong number of arguments in skewX transform");

                    matrix.Skew(values[0], 0);
                    break;

                case "skewY":
                    if (len != 1)
                        throw new ApplicationException("Wrong number of arguments in skewY transform");

                    matrix.Skew(0, values[0]);
                    break;

                case "matrix":
                    if (len != 6)
                        throw new ApplicationException("Wrong number of arguments in matrix transform");

                    matrix = new Matrix(values[0], values[1], values[2], values[3], values[4], values[5]);
                    break;

                default:
                    break;
            }

            return matrix;
        }
    }
}
