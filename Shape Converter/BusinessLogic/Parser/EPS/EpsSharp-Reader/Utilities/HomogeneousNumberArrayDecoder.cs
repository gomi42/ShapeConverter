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
    internal static class HomogeneousNumberArrayDecoder
    {
        /// <summary>
        /// Converts an encoded number string to a list of double values
        /// </summary>
        /// <param name="stringOperand"></param>
        /// <returns></returns>
        public static List<double> Decode(StringOperand stringOperand)
        {
            byte[] bytes = null;

            switch (stringOperand.Type)
            {
                case StringType.Standard:
                {
                    throw new Exception("Encoded Number String expected");
                }

                case StringType.Hex:
                {
                    bytes = new byte[stringOperand.Length];

                    for (int i = 0; i < stringOperand.Length; i++)
                    {
                        bytes[i] = (byte)stringOperand.Value[i];
                    }
                }
                break;

                case StringType.AsciiBase85:
                {
                    bytes = Ascii85Decoder.Decode(stringOperand.Value);
                    break;
                }
            }

            return DecodeWorker(bytes);
        }
        /// <summary>
        /// Structure of an encoded number
        /// </summary>
        private const int TypeOffset = 0;
        private const int LengthOffset = 2;
        private const int DataOffset = 4;

        /// <summary>
        /// Converts an encoded number given in an byte array to a list of double values.
        /// The spec suggest that that integers must be returned as integers (when scale is
        /// set to 0). Right now the users of this method don't distinguish between integer and double
        /// values. In order to make things easier we only return doubles.
        /// </summary>
        private static List<double> DecodeWorker(byte[] bytes)
        {
            List<double> values = null;

            if (bytes[TypeOffset] != 149)
            {
                throw new Exception("Encoded Number String expected");
            }

            int representation = (int)(uint)bytes[1];

            if (representation < 32)
            {
                // 32 bit, high order byte first
                values = ReadInt32Values(bytes, false, representation);
            }
            else
            if (representation >= 32 && representation < 32 + 16)
            {
                // 16 bit, high order byte first
                values = ReadInt16Values(bytes, false, representation - 32);
            }
            else
            if (representation == 48)
            {
                // 32 IEEE float
                byte[] bin = new byte[4];
                float value;

                if (BitConverter.IsLittleEndian)
                {
                    bin[0] = bytes[DataOffset + 3];
                    bin[1] = bytes[DataOffset + 2];
                    bin[2] = bytes[DataOffset + 1];
                    bin[3] = bytes[DataOffset];
                    value = BitConverter.ToSingle(bin, 0);
                }
                else
                {
                    value = BitConverter.ToSingle(bin, DataOffset);
                }

                values = new List<double>();
                values.Add(value);
            }
            else
            if (representation == 49)
            {
                // nativ float
                float value = BitConverter.ToSingle(bytes, DataOffset);
                values = new List<double>();
                values.Add(value);
            }
            else
            if (representation >= 128 && representation < 128 + 32)
            {
                // 32 bit, low order byte first
                values = ReadInt32Values(bytes, true, representation - 128);
            }
            else
            if (representation >= 160 && representation < 160 + 16)
            {
                // 16 bit, low order byte first
                values = ReadInt16Values(bytes, true, representation - 160);
            }

            return values;
        }

        private static int GetNumberOfValues(byte[] bytes, bool takeFirstByteFirst)
        {
            int numberOfValues;

            if (takeFirstByteFirst)
            {
                numberOfValues = BitConverter.ToInt16(bytes, LengthOffset);
            }
            else
            {
                byte[] bin = new byte[2];

                bin[0] = bytes[LengthOffset + 1];
                bin[1] = bytes[LengthOffset];

                numberOfValues = BitConverter.ToInt16(bin, 0);
            }

            return numberOfValues;
        }

        private static List<double> ReadInt16Values(byte[] bytes, bool lowOrderByteFirst, int scale)
        {
            var takeFirstByteFirst = BitConverter.IsLittleEndian == lowOrderByteFirst;
            int numberOfValues = GetNumberOfValues(bytes, takeFirstByteFirst);
            var values = new List<double>();

            for (int i = 0; i < numberOfValues; i++)
            {
                if (takeFirstByteFirst)
                {
                    values.Add((double)BitConverter.ToInt16(bytes, i * 2 + DataOffset));
                }
                else
                {
                    byte[] bin = new byte[2];

                    bin[0] = bytes[i * 2 + DataOffset + 1];
                    bin[1] = bytes[i * 2 + DataOffset];

                    values.Add((double)BitConverter.ToInt16(bin, 0));
                }
            }

            Scale(values, scale);

            return values;
        }

        private static List<double> ReadInt32Values(byte[] bytes, bool lowOrderByteFirst, int scale)
        {
            var takeFirstByteFirst = BitConverter.IsLittleEndian == lowOrderByteFirst;
            int numberOfValues = GetNumberOfValues(bytes, takeFirstByteFirst);
            var values = new List<double>();

            for (int i = 0; i < numberOfValues; i++)
            {
                if (takeFirstByteFirst)
                {
                    values.Add((double)BitConverter.ToInt32(bytes, i * 4 + DataOffset));
                }
                else
                {
                    byte[] bin = new byte[4];

                    bin[0] = bytes[i * 4 + DataOffset + 3];
                    bin[1] = bytes[i * 4 + DataOffset + 2];
                    bin[2] = bytes[i * 4 + DataOffset + 1];
                    bin[3] = bytes[i * 4 + DataOffset];

                    values.Add((double)BitConverter.ToInt32(bin, 0));
                }
            }

            Scale(values, scale);

            return values;
        }

        private static void Scale(List<double> values, int scale)
        {
            for (int j = 0; j < values.Count; j++)
            {
                for (int i = 0; i < scale; i++)
                {
                    values[j] /= 2;
                }
            }
        }
    }
}
