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
using EpsSharp.Eps.Core;

namespace EpsSharp.Eps.Helper
{
    /// <summary>
    /// Decoder for relative samples
    /// </summary>
    internal static class RelativeSamplesDecoder
    {
        /// <summary>
        /// Decodes relative samples given in a string operand
        /// </summary>
        /// <param name="stringOperand"></param>
        /// <returns>list of doubles in range 0..1</returns>
        public static List<double> Decode(StringOperand stringOperand, int bitsPerSample)
        {
            var bytes = GetBytes(stringOperand);
            return ConvertToSamples(bytes, bitsPerSample);
        }

        /// <summary>
        /// Convert the given encoded string to a byte array
        /// <summary>
        private static byte[] GetBytes(StringOperand stringOp)
        {
            byte[] bytes;

            switch (stringOp.Type)
            {
                case StringType.Hex:
                {
                    bytes = new byte[stringOp.Length];

                    for (int i = 0; i < stringOp.Length; i++)
                    {
                        bytes[i] = (byte)stringOp.Value[i];
                    }

                    break;
                }

                case StringType.AsciiBase85:
                {
                    bytes = Ascii85Decoder.Decode(stringOp.Value);
                    break;
                }

                default:
                {
                    throw new Exception("Encoded Number String expected");
                }
            }

            return bytes;
        }

        /// <summary>
        /// Convert the given encoded byte array to a list of doubles
        /// <summary>
        private static List<double> ConvertToSamples(byte[] bytes, int bitsPerSample)
        {
            int maxSampleValue = (1 << bitsPerSample) - 1;
            var list = new List<double>();

            switch (bitsPerSample)
            {
                case 1:
                case 2:
                case 4:
                {
                    foreach (var b in bytes)
                    {
                        int shift = 8 - bitsPerSample;

                        for (int i = 0; i < 8 / bitsPerSample; i++)
                        {
                            var d = (b >> shift) & maxSampleValue;
                            list.Add(((double)d) / maxSampleValue);

                            shift -= bitsPerSample;
                        }
                    }

                    break;
                }

                case 8:
                {
                    foreach (var b in bytes)
                    {
                        // optimized version
                        list.Add(((double)(uint)b) / maxSampleValue);
                    }

                    break;
                }

                case 16:
                case 24:
                case 32:
                {
                    // the byte order is unclear: assume big endian
                    var bytesPerSample = bitsPerSample / 8;
                    var numSamples = bytes.Length / bytesPerSample;

                    for (int i = 0; i < numSamples; i++)
                    {
                        uint b = 0;

                        for (int j = 0; j < bytesPerSample; j++)
                        {
                            b = (b << 8) | (uint)bytes[i * 2 + j];
                        }

                        list.Add(((double)b) / maxSampleValue);
                    }

                    break;
                }

                case 12:
                {
                    // the byte order is unclear: little or big endian?
                    // we ignore these weird encoding for a moment
                    break;
                }
            }

            return list;
        }
    }
}
