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

using System.IO;

namespace ShapeConverter.Helper
{
    /// <summary>
    /// 
    /// </summary>
    internal static class CommonHelper
    {
        const string SubDirName = "Shape Converter";
        static int filenameCounter = 1;

        /// <summary>
        /// Return the path of a temporary dicectory
        /// </summary>
        /// <returns></returns>
        private static string GetTempDirName()
        {
            var tempPath = Path.GetTempPath();
            var tempDir = Path.Combine(tempPath, SubDirName);

            return tempDir;
        }

        /// <summary>
        /// Return the temp directory
        /// </summary>
        /// <returns></returns>
        public static string GetTempDir()
        {
            var tempDir = GetTempDirName();

            if (!Directory.Exists(tempDir))
            {
                Directory.CreateDirectory(tempDir);
            }

            return tempDir;
        }

        /// <summary>
        /// Return a temp file name for a profile file
        /// </summary>
        /// <returns></returns>
        public static string GetTempProfileFilename()
        {
            var path = CommonHelper.GetTempDir();
            string filename;

            do
            {
                filename = Path.Combine(path, string.Format("Profile{0}.icc", filenameCounter++));
            }
            while (File.Exists(filename));

            return filename;
        }

        /// <summary>
        /// Remove the temporary dicectory and all files it contains
        /// </summary>
        public static void CleanUpTempDir()
        {
            var path = GetTempDirName();

            if (!Directory.Exists(path))
            {
                return;
            }

            var files = Directory.GetFiles(path);

            foreach (var file in files)
            {
                try
                {
                    File.Delete(file);
                }
                catch
                {
                }
            }

            try
            {
                Directory.Delete(path);
            }
            catch
            {
            }
        }
    }
}
