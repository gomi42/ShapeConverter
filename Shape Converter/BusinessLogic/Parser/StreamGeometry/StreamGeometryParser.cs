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
using System.Windows.Media;
using System.Windows;

namespace ShapeConverter.Parser.StreamGeometry
{
    /// <summary>
    /// The stream geometry parser.
    /// This logic is "borrowed" from the WPF sources.
    /// ShapeConverter v1.x had a different architecture that implemented many IStreamCodeGenerator based
    /// classes. In v2.x we need only one implementation of this interface which in turn means this kind of 
    /// architecture isn't realy needed anymore. We keep the structure because it shows the partition 
    /// between the parser and the generator quite well.
    /// </summary>
    internal class StreamGeometryParser
    {
        const bool AllowSign = true;
        const bool AllowComma = true;
        const bool IsFilled = true;
        const bool IsClosed = true;
        const bool IsStroked = true;
        const bool IsSmoothJoin = true;

        IFormatProvider formatProvider;

        string pathString;        // Input string to be parsed
        int pathLength;
        int curIndex;             // Location to read next character from 
        bool figureStarted;       // StartFigure is effective

        Point lastStart;          // Last figure starting point
        Point lastPoint;          // Last point
        Point secondLastPoint;    // The point before last point

        char token;               // Non whitespace character returned by ReadToken

        IStreamCodeGenerator codeGenerator;

        /// <summary>
        /// Parse a PathGeometry string.
        /// </summary>
        public GraphicPathGeometry ParseGeometry(string pathString)
        {
            GraphicPathGenerator codeGenerator = new GraphicPathGenerator();

            codeGenerator.Init();
            ParseString(codeGenerator, pathString);
            codeGenerator.Terminate();

            return codeGenerator.Path;
        }

        /// <summary>
        /// Parses the string.
        /// </summary>
        private void ParseString(IStreamCodeGenerator codeGenerator, string pathString)
        {
            // Check to ensure that there's something to parse 
            if (pathString == null)
            {
                return;
            }

            int curIndex = 0;

            // skip any leading space
            while ((curIndex < pathString.Length) && Char.IsWhiteSpace(pathString, curIndex))
            {
                curIndex++;
            }

            // Is there anything to look at? 
            if (curIndex < pathString.Length)
            {
                // If so, we only care if the first non-WhiteSpace char encountered is 'F'
                if (pathString[curIndex] == 'F')
                {
                    curIndex++;

                    // Since we found 'F' the next non-WhiteSpace char must be 0 or 1 - look for it.
                    while ((curIndex < pathString.Length) && Char.IsWhiteSpace(pathString, curIndex))
                    {
                        curIndex++;
                    }

                    // If we ran out of text, this is an error, because 'F' cannot be specified without 0 or 1
                    // Also, if the next token isn't 0 or 1, this too is illegal 
                    if ((curIndex == pathString.Length) ||
                        ((pathString[curIndex] != '0') &&
                         (pathString[curIndex] != '1')))
                    {
                        throw new Exception("Format");
                    }

                    var fillRule = pathString[curIndex] == '0' ? FillRule.EvenOdd : FillRule.Nonzero;
                    codeGenerator.SetFillRule(fillRule);

                    // Increment curIndex to point to the next char
                    curIndex++;
                }
            }

            Parse(codeGenerator, pathString, curIndex);
        }

        /// <summary> 
        /// Throw unexpected token exception
        /// </summary>
        private void ThrowBadToken()
        {
            throw new Exception("Format");
        }

        bool More()
        {
            return curIndex < pathLength;
        }

        // Skip white space, one comma if allowed 
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

        /// <summary>
        /// Read the next non whitespace character 
        /// </summary>
        /// <returns>True if not end of string</returns> 
        private bool ReadToken()
        {
            SkipWhiteSpace(!AllowComma);

            // Check for end of string
            if (More())
            {
                token = pathString[curIndex++];

                return true;
            }
            else
            {
                return false;
            }
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
                SkipDigits(!AllowSign);

                // Optional period, followed by more digits
                if (More() && (pathString[curIndex] == '.'))
                {
                    simple = false;
                    curIndex++;
                    SkipDigits(!AllowSign);
                }

                // Exponent
                if (More() && ((pathString[curIndex] == 'E') || (pathString[curIndex] == 'e')))
                {
                    simple = false;
                    curIndex++;
                    SkipDigits(AllowSign);
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
                    return System.Convert.ToDouble(subString, formatProvider);
                }
#pragma warning disable CS0168 // Variable is declared but never used
                catch (FormatException except)
#pragma warning restore CS0168 // Variable is declared but never used
                {
                    throw new Exception("Format");
                }
            }
        }

        /// <summary> 
        /// Read a bool: 1 or 0
        /// </summary> 
        /// <returns></returns> 
        bool ReadBool()
        {
            SkipWhiteSpace(AllowComma);

            if (More())
            {
                token = pathString[curIndex++];

                if (token == '0')
                {
                    return false;
                }
                else if (token == '1')
                {
                    return true;
                }
            }

            ThrowBadToken();

            return false;
        }

        /// <summary> 
        /// Read a relative point
        /// </summary> 
        /// <returns></returns> 
        private Point ReadPoint(char cmd, bool allowcomma)
        {
            double x = ReadNumber(allowcomma);
            double y = ReadNumber(AllowComma);

            if (cmd >= 'a') // 'A' < 'a'. lower case for relative 
            {
                x += lastPoint.X;
                y += lastPoint.Y;
            }

            return new Point(x, y);
        }

        /// <summary> 
        /// Reflect _secondLastPoint over _lastPoint to get a new point for smooth curve
        /// </summary> 
        /// <returns></returns> 
        private Point Reflect()
        {
            return new Point(2 * lastPoint.X - secondLastPoint.X,
                             2 * lastPoint.Y - secondLastPoint.Y);
        }

        private void EnsureFigure()
        {
            if (!figureStarted)
            {
                codeGenerator.BeginFigure(lastStart, IsFilled, !IsClosed);
                figureStarted = true;
            }
        }

        /// <summary>
        /// Parse a PathFigureCollection string 
        /// </summary> 
        internal void Parse(IStreamCodeGenerator codeGenerator, string pathString, int startIndex)
        {
            // [BreakingChange] Dev10 Bug #453199 
            // We really should throw an ArgumentNullException here for context and pathString.

            // From original code 
            // This is only used in call to Double.Parse
            formatProvider = System.Globalization.CultureInfo.InvariantCulture;

            this.codeGenerator = codeGenerator;
            this.pathString = pathString;
            pathLength = pathString.Length;
            curIndex = startIndex;

            secondLastPoint = new Point(0, 0);
            lastPoint = new Point(0, 0);
            lastStart = new Point(0, 0);

            figureStarted = false;

            bool first = true;

            char last_cmd = ' ';

            while (ReadToken()) // Empty path is allowed in XAML
            {
                char cmd = token;

                if (first)
                {
                    if ((cmd != 'M') && (cmd != 'm'))  // Path starts with M|m
                    {
                        ThrowBadToken();
                    }

                    first = false;
                }

                switch (cmd)
                {
                    case 'm':
                    case 'M':
                        // XAML allows multiple points after M/m 
                        lastPoint = ReadPoint(cmd, !AllowComma);

                        codeGenerator.BeginFigure(lastPoint, IsFilled, !IsClosed);
                        figureStarted = true;
                        lastStart = lastPoint;
                        last_cmd = 'M';

                        while (IsNumber(AllowComma))
                        {
                            lastPoint = ReadPoint(cmd, !AllowComma);

                            codeGenerator.LineTo(lastPoint, IsStroked, !IsSmoothJoin);
                            last_cmd = 'L';
                        }
                        break;

                    case 'l':
                    case 'L':
                    case 'h':
                    case 'H':
                    case 'v':
                    case 'V':
                        EnsureFigure();

                        do
                        {
                            switch (cmd)
                            {
                                case 'l': lastPoint = ReadPoint(cmd, !AllowComma); break;
                                case 'L': lastPoint = ReadPoint(cmd, !AllowComma); break;
                                case 'h': lastPoint.X += ReadNumber(!AllowComma); break;
                                case 'H': lastPoint.X = ReadNumber(!AllowComma); break;
                                case 'v': lastPoint.Y += ReadNumber(!AllowComma); break;
                                case 'V': lastPoint.Y = ReadNumber(!AllowComma); break;
                            }

                            codeGenerator.LineTo(lastPoint, IsStroked, !IsSmoothJoin);
                        }
                        while (IsNumber(AllowComma));

                        last_cmd = 'L';
                        break;

                    case 'c':
                    case 'C': // cubic Bezier
                    case 's':
                    case 'S': // smooth cublic Bezier 
                        EnsureFigure();
                        codeGenerator.BeginBezier();

                        do
                        {
                            Point p;

                            if ((cmd == 's') || (cmd == 'S'))
                            {
                                if (last_cmd == 'C')
                                {
                                    p = Reflect();
                                }
                                else
                                {
                                    p = lastPoint;
                                }

                                secondLastPoint = ReadPoint(cmd, !AllowComma);
                            }
                            else
                            {
                                p = ReadPoint(cmd, !AllowComma);

                                secondLastPoint = ReadPoint(cmd, AllowComma);
                            }

                            lastPoint = ReadPoint(cmd, AllowComma);

                            codeGenerator.BezierTo(p, secondLastPoint, lastPoint, IsStroked, !IsSmoothJoin);

                            last_cmd = 'C';
                        }
                        while (IsNumber(AllowComma));

                        codeGenerator.EndBezier();
                        break;

                    case 'q':
                    case 'Q': // quadratic Bezier
                    case 't':
                    case 'T': // smooth quadratic Bezier 
                        EnsureFigure();

                        do
                        {
                            if ((cmd == 't') || (cmd == 'T'))
                            {
                                if (last_cmd == 'Q')
                                {
                                    secondLastPoint = Reflect();
                                }
                                else
                                {
                                    secondLastPoint = lastPoint;
                                }

                                lastPoint = ReadPoint(cmd, !AllowComma);
                            }
                            else
                            {
                                secondLastPoint = ReadPoint(cmd, !AllowComma);
                                lastPoint = ReadPoint(cmd, AllowComma);
                            }

                            codeGenerator.QuadraticBezierTo(secondLastPoint, lastPoint, IsStroked, !IsSmoothJoin);

                            last_cmd = 'Q';
                        }
                        while (IsNumber(AllowComma));

                        break;

                    case 'a':
                    case 'A':
                        EnsureFigure();

                        do
                        {
                            // A 3,4 5, 0, 0, 6,7 
                            double w = ReadNumber(!AllowComma);
                            double h = ReadNumber(AllowComma);
                            double rotation = ReadNumber(AllowComma);
                            bool large = ReadBool();
                            bool sweep = ReadBool();

                            var startPoint = lastPoint;
                            lastPoint = ReadPoint(cmd, AllowComma);

                            codeGenerator.ArcTo(
                                startPoint,
                                lastPoint,
                                new Size(w, h),
                                rotation,
                                large,
                                sweep,
                                IsStroked,
                                !IsSmoothJoin
                                );
                        }
                        while (IsNumber(AllowComma));

                        last_cmd = 'A';
                        break;

                    case 'z':
                    case 'Z':
                        EnsureFigure();
                        codeGenerator.SetClosedState(IsClosed);

                        figureStarted = false;
                        last_cmd = 'Z';

                        lastPoint = lastStart; // Set reference point to be first point of current figure 
                        break;

                    default:
                        ThrowBadToken();
                        break;
                }
            }
        }
    }
}

