//
// Adapter:
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
using System.IO;

namespace EpsSharp.Eps.Helper
{
    /// <summary>
    /// C# implementation of ASCII85 encoding. 
    /// Based on C code from http://www.stillhq.com/cgi-bin/cvsweb/ascii85/
    /// </summary>
    /// <remarks>
    /// Jeff Atwood
    /// http://www.codinghorror.com/blog/archives/000410.html
    /// </remarks>
    internal static class Ascii85Decoder
    {
        private const int asciiOffset = 33;

        private static uint[] pow85 = { 85 * 85 * 85 * 85, 85 * 85 * 85, 85 * 85, 85, 1 };

        /// <summary>
        /// Decodes an ASCII85 encoded string into the original binary data
        /// </summary>
        /// <param name="s">ASCII85 encoded string</param>
        /// <returns>byte array of decoded binary data</returns>
        public static byte[] Decode(string s)
        {
            byte[] encodedBlock = new byte[5];
            byte[] decodedBlock = new byte[4];
            uint tuple = 0;
            MemoryStream ms = new MemoryStream();
            int count = 0;
            bool processChar = false;

            foreach (char c in s)
            {
                switch (c)
                {
                    case 'z':
                    {
                        if (count != 0)
                        {
                            throw new Exception("The character 'z' is invalid inside an ASCII85 block.");
                        }

                        decodedBlock[0] = 0;
                        decodedBlock[1] = 0;
                        decodedBlock[2] = 0;
                        decodedBlock[3] = 0;
                        ms.Write(decodedBlock, 0, decodedBlock.Length);
                        processChar = false;
                        break;
                    }

                    case '\n':
                    case '\r':
                    case '\t':
                    case '\0':
                    case '\f':
                    case '\b':
                    {
                        processChar = false;
                        break;
                    }

                    default:
                    {
                        if (c < '!' || c > 'u')
                        {
                            throw new Exception("Bad character '" + c + "' found. ASCII85 only allows characters '!' to 'u'.");
                        }

                        processChar = true;
                        break;
                    }
                }

                if (processChar)
                {
                    tuple += ((uint)(c - asciiOffset) * pow85[count]);
                    count++;

                    if (count == encodedBlock.Length)
                    {
                        DecodeBlock(decodedBlock, tuple);
                        ms.Write(decodedBlock, 0, decodedBlock.Length);
                        tuple = 0;
                        count = 0;
                    }
                }
            }

            // if we have some bytes left over at the end..
            if (count != 0)
            {
                if (count == 1)
                {
                    throw new Exception("The last block of ASCII85 data cannot be a single byte.");
                }

                count--;
                tuple += pow85[count];
                DecodeBlock(decodedBlock, count, tuple);

                for (int i = 0; i < count; i++)
                {
                    ms.WriteByte(decodedBlock[i]);
                }
            }

            return ms.ToArray();
        }

        private static void DecodeBlock(byte[] decodedBlock, uint tuple)
        {
            DecodeBlock(decodedBlock, decodedBlock.Length, tuple);
        }

        private static void DecodeBlock(byte[] decodedBlock, int bytes, uint tuple)
        {
            for (int i = 0; i < bytes; i++)
            {
                decodedBlock[i] = (byte)(tuple >> 24 - (i * 8));
            }
        }
     }
}