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

namespace ShapeConverter.BusinessLogic.Parser.Svg
{
    /// <summary>
    /// Class for parsing a point (x, y coordinate)
    /// This logic is "borrowed" from the WPF sources.
    /// </summary>
    internal class DoubleListParser
    {
        string pathString;        // Input string to be parsed
        int pathLength;
        int curIndex;          // Location to read next character from
        char token;

        /// <summary>
        /// Parse a list of doubles and return a list of points
        /// </summary>
        internal List<Point> ParsePointList(string pathString)
        {
            this.pathString = pathString;
            pathLength = pathString.Length;
            curIndex = 0;
            var points = new List<Point>();

            while (ReadToken())
            {
                double x = ReadNumber(true);
                double y = ReadNumber(true);

                points.Add(new Point(x, y));
            }

            return points;
        }

        /// <summary>
        /// Parse a list of doubles and return it
        /// </summary>
        internal List<double> ParseDoubleList(string pathString)
        {
            this.pathString = pathString;
            pathLength = pathString.Length;
            curIndex = 0;
            var dbls = new List<double>();

            while (ReadToken())
            {
                double dbl = ReadNumber(true);
                dbls.Add(dbl);
            }

            return dbls;
        }

        /// <summary>
        /// Skip white space, one comma if allowed 
        /// </summary>
        bool More()
        {
            return curIndex < pathLength;
        }

        /// <summary>
        /// Read the next non whitespace character 
        /// </summary>
        /// <returns>True if not end of string</returns> 
        private bool ReadToken()
        {
            SkipWhiteSpace(true);

            // Check for end of string
            if (More())
            {
                token = pathString[curIndex];

                return true;
            }
            else
            {
                return false;
            }
        }

        private void ThrowBadToken()
        {
            throw new Exception("Format");
        }

        private bool SkipWhiteSpace(bool allowComma)
        {
            bool commaMet = false;

            while (More())
            {
                char ch = pathString[curIndex];

                switch (ch)
                {
                    case ' ':
                    case '\n':
                    case '\r':
                    case '\t': // SVG whitespace 
                        break;

                    case ',':
                        if (allowComma)
                        {
                            commaMet = true;
                            allowComma = false; // one comma only 
                        }
                        else
                        {
                            ThrowBadToken();
                        }
                        break;

                    default:
                        // Avoid calling IsWhiteSpace for ch in (' ' .. 'z'] 
                        if (((ch > ' ') && (ch <= 'z')) || !Char.IsWhiteSpace(ch))
                        {
                            return commaMet;
                        }
                        break;
                }

                curIndex++;
            }

            return commaMet;
        }

        private bool IsNumber(bool allowComma)
        {
            bool commaMet = SkipWhiteSpace(allowComma);

            if (More())
            {
                token = pathString[curIndex];

                // Valid start of a number
                if ((token == '.') || (token == '-') || (token == '+') || ((token >= '0') && (token <= '9'))
                    || (token == 'I')  // Infinity 
                    || (token == 'N')) // NaN
                {
                    return true;
                }
            }

            if (commaMet) // Only allowed between numbers
            {
                ThrowBadToken();
            }

            return false;
        }

        void SkipDigits(bool signAllowed)
        {
            // Allow for a sign 
            if (signAllowed && More() && ((pathString[curIndex] == '-') || pathString[curIndex] == '+'))
            {
                curIndex++;
            }

            while (More() && (pathString[curIndex] >= '0') && (pathString[curIndex] <= '9'))
            {
                curIndex++;
            }
        }

        /// <summary> 
        /// Read a floating point number
        /// </summary> 
        /// <returns></returns>
        double ReadNumber(bool allowComma)
        {
            if (!IsNumber(allowComma))
            {
                ThrowBadToken();
            }

            bool simple = true;
            int start = curIndex;

            //
            // Allow for a sign 
            //
            // There are numbers that cannot be preceded with a sign, for instance, -NaN, but it's 
            // fine to ignore that at this point, since the CLR parser will catch this later. 
            //
            if (More() && ((pathString[curIndex] == '-') || pathString[curIndex] == '+'))
            {
                curIndex++;
            }

            // Check for Infinity (or -Infinity).
            if (More() && (pathString[curIndex] == 'I'))
            {
                //
                // Don't bother reading the characters, as the CLR parser will 
                // do this for us later.
                //
                curIndex = Math.Min(curIndex + 8, pathLength); // "Infinity" has 8 characters
                simple = false;
            }
            // Check for NaN 
            else if (More() && (pathString[curIndex] == 'N'))
            {
                // 
                // Don't bother reading the characters, as the CLR parser will
                // do this for us later.
                //
                curIndex = Math.Min(curIndex + 3, pathLength); // "NaN" has 3 characters 
                simple = false;
            }
            else
            {
                SkipDigits(!true);

                // Optional period, followed by more digits
                if (More() && (pathString[curIndex] == '.'))
                {
                    simple = false;
                    curIndex++;
                    SkipDigits(!true);
                }

                // Exponent
                if (More() && ((pathString[curIndex] == 'E') || (pathString[curIndex] == 'e')))
                {
                    simple = false;
                    curIndex++;
                    SkipDigits(true);
                }
            }

            if (simple && (curIndex <= (start + 8))) // 32-bit integer
            {
                int sign = 1;

                if (pathString[start] == '+')
                {
                    start++;
                }
                else if (pathString[start] == '-')
                {
                    start++;
                    sign = -1;
                }

                int value = 0;

                while (start < curIndex)
                {
                    value = value * 10 + (pathString[start] - '0');
                    start++;
                }

                return value * sign;
            }
            else
            {
                string subString = pathString.Substring(start, curIndex - start);

                try
                {
                    return System.Convert.ToDouble(subString, System.Globalization.CultureInfo.InvariantCulture);
                }
#pragma warning disable CS0168 // Variable is declared but never used
                catch (FormatException except)
#pragma warning restore CS0168 // Variable is declared but never used
                {
                    throw new Exception("Format");
                }
            }
        }
    }
}
