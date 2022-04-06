#define TEST
//
// Author:
//   Michael G�ricke
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
using System.Text;
using System.Windows.Media;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Content;
using PdfSharp.Pdf.IO;
using ShapeConverter.BusinessLogic.Generators;
using ShapeConverter.BusinessLogic.Parser.Pdf.Main;
using ShapeConverter.Helper;

namespace ShapeConverter.Parser.Pdf
{
    /// <summary>
    /// The PDF and AI parser
    /// </summary>
    internal class PdfParser : IFileParser
    {
        /// <summary>
        /// Parse the given file and convert it to a list of graphic paths
        /// </summary>
        GraphicVisual IFileParser.Parse(string filename)
        {
            var group = new GraphicGroup();
            PdfDocument inputDocument = PdfReader.Open(filename);
            var invisibleGroups = GetVisibleGroups(inputDocument.OCPropperties);

            for (int i = 0; i < inputDocument.Pages.Count; i++)
            {
                var page = inputDocument.Pages[i];
                var geometry = Parse(page, invisibleGroups);
                group.Children.Add(geometry);
            }

            var visual = OptimizeVisual.Optimize(group);
            CommonHelper.CleanUpTempDir();

            return visual;
        }

        /// <summary>
        /// Parse a single PDF page
        /// </summary>
        private GraphicVisual Parse(PdfPage page, PdfDictionary[] invisibleGroups)
        {
            var currentGraphicsState = new GraphicsState();
            currentGraphicsState.TransformationMatrix = Matrix.Identity;

            var mediaBox = page.MediaBox;
            currentGraphicsState.Mirror = new Matrix(1, 0, 0, -1, 0, mediaBox.Y2);

#if TEST1
            string content = "/OC /MC0 BDC /TT0 1 Tf /CS0 cs 0.4 0.4 0.4  scn /GS0 gs q 0 758 1356 -758 re W n Q BT 0.302 0.302 0.302  scn 14.9825 0 0 14.8992 1120.2744 600.5273 Tm (Sample01)Tj ET BT 0 0 0  scn 12.8421 0 0 12.7707 623.9248 497.3711 Tm (Sample02)Tj ET BT 12.6358 0 0 12.5656 295.2646 566.7402 Tm 0.302 0.302 0.302  scn /TT1 1 Tf 14.9825 0 0 14.8992 580.3262 600.0195 Tm (No.)Tj 3.279 0.034 Td [(T)55.3(ype)] TJ 16.097 -0.034 Td [(T)111(est)] TJ -8.617 0.034 Td (SID)Tj 0 0 0  scn -0.018 Tw 12.8421 0 0 12.7707 853.9824 497.3711 Tm (T3 T4 TSH)Tj 0 -3.083 TD (T3 T4 TSH)Tj 0 -3 TD (T3 T4 TSH)Tj 0 Tw 1.427 -2.75 Td (TSH)Tj 0.278 -2.583 Td (fT3)Tj -0.278 17 Td (TSH)Tj 0 -2.833 TD (TSH)Tj 0.416 -16.917 Td (T3)Tj -0.416 -2.667 Td (TSH)Tj 0 -2.667 TD (TSH)Tj 0 -2.833 TD (TSH)Tj -1.419 -2.833 Td (TSH  )Tj 2.874 0 Td (1:10)Tj -20.796 22.083 Td (Sample03)Tj 0 8.578 TD (Sample04)Tj 0 -2.765 TD (Sample05)Tj 0 -8.812 TD (Sample06)Tj 0 -2.833 TD (Sample07)Tj 0 -2.667 TD (Sample08)Tj 0 -2.667 TD (Sample09)Tj 0 -2.667 TD (Sample10)Tj 0 -2.583 TD (Sample11)Tj 0 -2.917 TD (Sample12)Tj 0 -2.75 TD (Sample13)Tj 7.905 30.755 Td (28907255)Tj 0 -2.838 TD (27306241)Tj 0 -2.75 TD (12994100)Tj 0 -3.083 TD (22459120)Tj 0.037 -3 Td [(321)74(17122)] TJ -0.037 -2.833 Td (92987201)Tj 0 -2.667 TD (82294172)Tj 0 -2.667 TD (72984377)Tj 0.037 -2.667 Td [(529141)74.1(10)] TJ -0.037 -2.583 Td (52692171)Tj 0.037 -2.917 Td [(10791)74.1(190)] TJ 0 -2.75 TD [(75901)74.1(177)] TJ ET q BT 0.2 0.2 0.2  scn /TT0 1 Tf 18.0001 0 0 17.9 127.2764 575.3311 Tm ET BT 12.8421 0 0 12.7707 623.9248 497.3711 Tm (Sample15)Tj 0.302 0.302 0.302  scn 14.9825 0 0 14.8992 629.4551 600.5273 Tm [(T)55.3(ype)] TJ 16.097 -0.034 Td [(T)111(est)] TJ -8.617 0.034 Td (SID)Tj 0 0 0  scn 12.8421 0 0 12.7707 623.9248 457.9961 Tm (Sample16)Tj 0 -3 TD (Sample17)Tj 0 -2.833 TD (Sample18)Tj 0 -2.667 TD (Sample19)Tj 0 -2.667 TD (Sample20)Tj T* (Sample21) Tj 0 -2.583 TD (Sample22)Tj 0 -2.917 TD (Sample23)Tj 0 -2.75 TD (Sample24)Tj ET BT 12.7987 0 0 12.7707 586.6543 568.7363 Tm (1)Tj -0.018 Tw 12.8421 0 0 12.7707 853.9824 497.3711 Tm (T3 T4 TSH)Tj 0 -3.083 TD (T3 T4 TSH)Tj 0 -3 TD (T3 T4 TSH)Tj 0 Tw 1.427 -2.75 Td (TSH)Tj 0.278 -2.583 Td (fT3)Tj -0.278 17 Td (TSH)Tj 0.416 -19.75 Td (T3)Tj -0.416 -2.667 Td (TSH)Tj 0 -2.667 TD (TSH)Tj 0 -2.833 TD (TSH)Tj -1.419 -2.833 Td (TSH  )Tj 2.874 0 Td (1:10)Tj -20.796 30.661 Td (Sample25)Tj 7.905 0.094 Td (28907255)Tj 0 -5.588 TD (12994100)Tj 0 -3.083 TD (22459120)Tj 0.037 -3 Td [(321)74(17122)] TJ -0.037 -2.833 Td (92987201)Tj 0 -2.667 TD (82294172)Tj 0 -2.667 TD (72984377)Tj 0.037 -2.667 Td [(529141)74.1(10)] TJ -0.037 -2.583 Td (52692171)Tj 0.037 -2.917 Td [(10791)74.1(190)] TJ 0 -2.75 TD [(75901)74.1(177)] TJ 12.7747 0 0 12.7707 587.6758 532.2715 Tm (2)Tj 12.8421 0 0 12.7707 623.9248 532.0723 Tm (Sample26)Tj 27.513 2.808 Td (ID)Tj ET BT 0 0 0  scn 14.9825 0 0 14.8992 1121.5254 537.0703 Tm (Control)Tj -0.778 -2.081 Td (Calibration)Tj ET BT 14.9825 0 0 14.8992 1120.2744 568.0703 Tm (Sample27)Tj ET";
            content = "1 0 0 -1 0 0 cm q 0 1 1 rg 0 1 1 RG Q q 1 0 1 rg 1 0 1 RG Q q 1 1 0 rg 1 1 0 RG Q q 0 0 0 rg 0 0 0 RG Q q 1 0 0 1 0 0 cm q n q 0 0 m 0 138.24 l 254.16 138.24 l 254.16 0 l h n 90.9868 67.4906 m 90.8822 67.4552 90.7863 67.4047 90.7863 67.2836 c 90.7863 67.215 90.8274 67.1559 90.9197 67.0896 c 91.012 67.0232 106.105 56.3781 106.105 56.3781 c 107.856 60.2659 108.831 64.5785 108.831 69.1189 c 108.831 70.5966 108.727 72.0511 108.528 73.474 c 108.528 73.474 91.0913 67.5251 90.9868 67.4906 c h 77.7852 100.165 m 62.226 100.165 49.3417 88.7188 47.0881 73.7906 c 58.6591 75.4053 l 58.838 75.4291 58.9109 75.5236 58.9541 75.6491 c 61.2481 82.3328 67.0304 87.4199 74.1981 88.7231 c 71.4684 87.3932 69.587 84.5915 69.587 81.3492 c 69.587 76.8217 73.2569 73.1509 77.7852 73.1509 c 82.3126 73.1509 85.9826 76.8217 85.9826 81.3492 c 85.9826 84.5915 84.1019 87.3932 81.3715 88.7231 c 88.5392 87.4199 94.3214 82.3328 96.6155 75.6491 c 96.6588 75.5236 96.7316 75.4291 96.9104 75.4053 c 108.481 73.7906 l 106.228 88.7188 93.3436 100.165 77.7852 100.165 c h 46.739 69.1189 m 46.739 64.5785 47.7133 60.2659 49.465 56.3781 c 49.465 56.3781 64.5583 67.0232 64.6498 67.0896 c 64.7421 67.1559 64.7833 67.215 64.7833 67.2836 c 64.7833 67.4047 64.6873 67.4552 64.5828 67.4906 c 64.4782 67.5251 47.0412 73.474 47.0412 73.474 c 46.8421 72.0511 46.739 70.5966 46.739 69.1189 c h 60.8738 43.0783 m 60.8738 43.0783 67.8273 60.2255 67.8619 60.3113 c 67.8907 60.3834 67.898 60.4282 67.898 60.4642 c 67.898 60.5623 67.8208 60.6366 67.7278 60.6366 c 67.6751 60.6366 67.6247 60.6279 67.5705 60.6142 c 67.4401 60.5803 49.5631 56.1617 49.5631 56.1617 c 52.0273 50.8042 55.9728 46.2681 60.8738 43.0783 c h 77.6719 38.0728 m 77.6719 38.0728 74.3878 55.5329 74.2254 56.3889 c 74.198 56.5338 74.1137 56.5822 74.0358 56.5822 c 73.9586 56.5822 73.8973 56.531 73.813 56.4423 c 73.3976 56.0067 61.0664 42.9544 61.0664 42.9544 c 65.8628 39.8829 71.5593 38.0951 77.6719 38.0728 c h 94.5032 42.9544 m 94.5032 42.9544 82.172 56.0067 81.7573 56.4423 c 81.6722 56.531 81.6116 56.5822 81.5337 56.5822 c 81.4566 56.5822 81.3715 56.5338 81.3441 56.3889 c 81.1819 55.5329 77.8976 38.0728 77.8976 38.0728 c 84.0109 38.0951 89.7067 39.8829 94.5032 42.9544 c h 106.006 56.1617 m 106.006 56.1617 88.1296 60.5803 87.9997 60.6142 c 87.9442 60.6279 87.8944 60.6366 87.8418 60.6366 c 87.7487 60.6366 87.6716 60.5623 87.6716 60.4642 c 87.6716 60.4282 87.6788 60.3834 87.7077 60.3113 c 87.7423 60.2255 94.6957 43.0783 94.6957 43.0783 c 99.5975 46.2681 103.542 50.8042 106.006 56.1617 c h 77.7858 36.8194 m 59.9457 36.8194 45.4834 51.2802 45.4834 69.1204 c 45.4834 86.9584 59.9457 101.421 77.7859 101.421 c 95.6246 101.421 110.085 86.9584 110.085 69.1204 c 110.085 51.2802 95.6246 36.8194 77.7858 36.8194 c h 184.269 52.9353 m 177.123 52.9353 l 177.123 85.5232 l 184.269 85.5232 l 195.175 85.5232 201.256 79.076 201.256 69.2285 c 201.256 59.3089 195.279 52.9353 184.269 52.9353 c h 184.748 92.3238 m 184.748 92.3238 182.599 92.3238 173.376 92.3238 c 171.297 92.3238 169.633 90.7546 169.633 88.5796 c 169.633 49.8134 l 169.633 47.732 171.297 46.1383 173.376 46.1383 c 184.748 46.1383 l 200.164 46.1383 208.677 56.4697 208.677 69.2285 c 208.677 81.9917 200.072 92.3238 184.748 92.3238 c h 147.99 71.3098 m 134.335 71.3098 l 134.335 85.4598 l 134.335 85.4598 138.144 85.4598 147.99 85.4598 c 153.46 85.4598 155.972 81.9211 155.972 78.3852 c 155.972 74.8479 153.494 71.3098 147.99 71.3098 c h 146.294 52.9353 m 146.294 52.9353 147.094 52.9353 134.335 52.9353 c 134.335 64.5857 l 141.548 64.5857 146.294 64.5857 146.294 64.5857 c 151.036 64.5857 153.269 61.9477 153.269 58.758 c 153.269 55.4998 151.174 52.9353 146.294 52.9353 c h 147.722 92.3238 m 130.59 92.3238 l 128.51 92.3238 126.915 90.6594 126.915 88.5796 c 126.914 49.8134 l 126.914 47.7321 128.51 46.1383 130.589 46.1383 c 146.611 46.1383 l 155.417 46.1383 160.687 51.2607 160.687 57.9185 c 160.687 64.6491 156.574 67.0478 155.14 67.567 c 157.363 68.1266 163.392 70.6781 163.392 79.097 c 163.392 86.31 157.574 92.3238 147.722 92.3238 c 1 1 1 rg 1 1 1 RG f Q Q Q";
            content = "q 0 1 1 rg 0 1 1 RG Q q 1 0 1 rg 1 0 1 RG Q q 1 1 0 rg 1 1 0 RG Q q 0 0 0 rg 0 0 0 RG Q 1 0 0 -1 0 0 cm 1 0 0 1 0 -138.24 cm q 1 0 0 1 0 0 cm q n q 0 0 m 0 138.24 l 254.16 138.24 l 254.16 0 l h n 90.9868 67.4906 m 90.8822 67.4552 90.7863 67.4047 90.7863 67.2836 c 90.7863 67.215 90.8274 67.1559 90.9197 67.0896 c 91.012 67.0232 106.105 56.3781 106.105 56.3781 c 107.856 60.2659 108.831 64.5785 108.831 69.1189 c 108.831 70.5966 108.727 72.0511 108.528 73.474 c 108.528 73.474 91.0913 67.5251 90.9868 67.4906 c h 77.7852 100.165 m 62.226 100.165 49.3417 88.7188 47.0881 73.7906 c 58.6591 75.4053 l 58.838 75.4291 58.9109 75.5236 58.9541 75.6491 c 61.2481 82.3328 67.0304 87.4199 74.1981 88.7231 c 71.4684 87.3932 69.587 84.5915 69.587 81.3492 c 69.587 76.8217 73.2569 73.1509 77.7852 73.1509 c 82.3126 73.1509 85.9826 76.8217 85.9826 81.3492 c 85.9826 84.5915 84.1019 87.3932 81.3715 88.7231 c 88.5392 87.4199 94.3214 82.3328 96.6155 75.6491 c 96.6588 75.5236 96.7316 75.4291 96.9104 75.4053 c 108.481 73.7906 l 106.228 88.7188 93.3436 100.165 77.7852 100.165 c h 46.739 69.1189 m 46.739 64.5785 47.7133 60.2659 49.465 56.3781 c 49.465 56.3781 64.5583 67.0232 64.6498 67.0896 c 64.7421 67.1559 64.7833 67.215 64.7833 67.2836 c 64.7833 67.4047 64.6873 67.4552 64.5828 67.4906 c 64.4782 67.5251 47.0412 73.474 47.0412 73.474 c 46.8421 72.0511 46.739 70.5966 46.739 69.1189 c h 60.8738 43.0783 m 60.8738 43.0783 67.8273 60.2255 67.8619 60.3113 c 67.8907 60.3834 67.898 60.4282 67.898 60.4642 c 67.898 60.5623 67.8208 60.6366 67.7278 60.6366 c 67.6751 60.6366 67.6247 60.6279 67.5705 60.6142 c 67.4401 60.5803 49.5631 56.1617 49.5631 56.1617 c 52.0273 50.8042 55.9728 46.2681 60.8738 43.0783 c h 77.6719 38.0728 m 77.6719 38.0728 74.3878 55.5329 74.2254 56.3889 c 74.198 56.5338 74.1137 56.5822 74.0358 56.5822 c 73.9586 56.5822 73.8973 56.531 73.813 56.4423 c 73.3976 56.0067 61.0664 42.9544 61.0664 42.9544 c 65.8628 39.8829 71.5593 38.0951 77.6719 38.0728 c h 94.5032 42.9544 m 94.5032 42.9544 82.172 56.0067 81.7573 56.4423 c 81.6722 56.531 81.6116 56.5822 81.5337 56.5822 c 81.4566 56.5822 81.3715 56.5338 81.3441 56.3889 c 81.1819 55.5329 77.8976 38.0728 77.8976 38.0728 c 84.0109 38.0951 89.7067 39.8829 94.5032 42.9544 c h 106.006 56.1617 m 106.006 56.1617 88.1296 60.5803 87.9997 60.6142 c 87.9442 60.6279 87.8944 60.6366 87.8418 60.6366 c 87.7487 60.6366 87.6716 60.5623 87.6716 60.4642 c 87.6716 60.4282 87.6788 60.3834 87.7077 60.3113 c 87.7423 60.2255 94.6957 43.0783 94.6957 43.0783 c 99.5975 46.2681 103.542 50.8042 106.006 56.1617 c h 77.7858 36.8194 m 59.9457 36.8194 45.4834 51.2802 45.4834 69.1204 c 45.4834 86.9584 59.9457 101.421 77.7859 101.421 c 95.6246 101.421 110.085 86.9584 110.085 69.1204 c 110.085 51.2802 95.6246 36.8194 77.7858 36.8194 c h 184.269 52.9353 m 177.123 52.9353 l 177.123 85.5232 l 184.269 85.5232 l 195.175 85.5232 201.256 79.076 201.256 69.2285 c 201.256 59.3089 195.279 52.9353 184.269 52.9353 c h 184.748 92.3238 m 184.748 92.3238 182.599 92.3238 173.376 92.3238 c 171.297 92.3238 169.633 90.7546 169.633 88.5796 c 169.633 49.8134 l 169.633 47.732 171.297 46.1383 173.376 46.1383 c 184.748 46.1383 l 200.164 46.1383 208.677 56.4697 208.677 69.2285 c 208.677 81.9917 200.072 92.3238 184.748 92.3238 c h 147.99 71.3098 m 134.335 71.3098 l 134.335 85.4598 l 134.335 85.4598 138.144 85.4598 147.99 85.4598 c 153.46 85.4598 155.972 81.9211 155.972 78.3852 c 155.972 74.8479 153.494 71.3098 147.99 71.3098 c h 146.294 52.9353 m 146.294 52.9353 147.094 52.9353 134.335 52.9353 c 134.335 64.5857 l 141.548 64.5857 146.294 64.5857 146.294 64.5857 c 151.036 64.5857 153.269 61.9477 153.269 58.758 c 153.269 55.4998 151.174 52.9353 146.294 52.9353 c h 147.722 92.3238 m 130.59 92.3238 l 128.51 92.3238 126.915 90.6594 126.915 88.5796 c 126.914 49.8134 l 126.914 47.7321 128.51 46.1383 130.589 46.1383 c 146.611 46.1383 l 155.417 46.1383 160.687 51.2607 160.687 57.9185 c 160.687 64.6491 156.574 67.0478 155.14 67.567 c 157.363 68.1266 163.392 70.6781 163.392 79.097 c 163.392 86.31 157.574 92.3238 147.722 92.3238 c 1 1 1 rg 1 1 1 RG f Q Q Q";
            //content = "q 0 1 1 rg 0 1 1 RG Q q 1 0 1 rg 1 0 1 RG Q q 1 1 0 rg 1 1 0 RG Q q 0 0 0 rg 0 0 0 RG Q 1 0 0 -1 0 0 cm q 1 0 0 1 0 0 cm q n q 0 0 m 0 138.24 l 254.16 138.24 l 254.16 0 l h n 90.9868 67.4906 m 90.8822 67.4552 90.7863 67.4047 90.7863 67.2836 c 90.7863 67.215 90.8274 67.1559 90.9197 67.0896 c 91.012 67.0232 106.105 56.3781 106.105 56.3781 c 107.856 60.2659 108.831 64.5785 108.831 69.1189 c 108.831 70.5966 108.727 72.0511 108.528 73.474 c 108.528 73.474 91.0913 67.5251 90.9868 67.4906 c h 77.7852 100.165 m 62.226 100.165 49.3417 88.7188 47.0881 73.7906 c 58.6591 75.4053 l 58.838 75.4291 58.9109 75.5236 58.9541 75.6491 c 61.2481 82.3328 67.0304 87.4199 74.1981 88.7231 c 71.4684 87.3932 69.587 84.5915 69.587 81.3492 c 69.587 76.8217 73.2569 73.1509 77.7852 73.1509 c 82.3126 73.1509 85.9826 76.8217 85.9826 81.3492 c 85.9826 84.5915 84.1019 87.3932 81.3715 88.7231 c 88.5392 87.4199 94.3214 82.3328 96.6155 75.6491 c 96.6588 75.5236 96.7316 75.4291 96.9104 75.4053 c 108.481 73.7906 l 106.228 88.7188 93.3436 100.165 77.7852 100.165 c h 46.739 69.1189 m 46.739 64.5785 47.7133 60.2659 49.465 56.3781 c 49.465 56.3781 64.5583 67.0232 64.6498 67.0896 c 64.7421 67.1559 64.7833 67.215 64.7833 67.2836 c 64.7833 67.4047 64.6873 67.4552 64.5828 67.4906 c 64.4782 67.5251 47.0412 73.474 47.0412 73.474 c 46.8421 72.0511 46.739 70.5966 46.739 69.1189 c h 60.8738 43.0783 m 60.8738 43.0783 67.8273 60.2255 67.8619 60.3113 c 67.8907 60.3834 67.898 60.4282 67.898 60.4642 c 67.898 60.5623 67.8208 60.6366 67.7278 60.6366 c 67.6751 60.6366 67.6247 60.6279 67.5705 60.6142 c 67.4401 60.5803 49.5631 56.1617 49.5631 56.1617 c 52.0273 50.8042 55.9728 46.2681 60.8738 43.0783 c h 77.6719 38.0728 m 77.6719 38.0728 74.3878 55.5329 74.2254 56.3889 c 74.198 56.5338 74.1137 56.5822 74.0358 56.5822 c 73.9586 56.5822 73.8973 56.531 73.813 56.4423 c 73.3976 56.0067 61.0664 42.9544 61.0664 42.9544 c 65.8628 39.8829 71.5593 38.0951 77.6719 38.0728 c h 94.5032 42.9544 m 94.5032 42.9544 82.172 56.0067 81.7573 56.4423 c 81.6722 56.531 81.6116 56.5822 81.5337 56.5822 c 81.4566 56.5822 81.3715 56.5338 81.3441 56.3889 c 81.1819 55.5329 77.8976 38.0728 77.8976 38.0728 c 84.0109 38.0951 89.7067 39.8829 94.5032 42.9544 c h 106.006 56.1617 m 106.006 56.1617 88.1296 60.5803 87.9997 60.6142 c 87.9442 60.6279 87.8944 60.6366 87.8418 60.6366 c 87.7487 60.6366 87.6716 60.5623 87.6716 60.4642 c 87.6716 60.4282 87.6788 60.3834 87.7077 60.3113 c 87.7423 60.2255 94.6957 43.0783 94.6957 43.0783 c 99.5975 46.2681 103.542 50.8042 106.006 56.1617 c h 77.7858 36.8194 m 59.9457 36.8194 45.4834 51.2802 45.4834 69.1204 c 45.4834 86.9584 59.9457 101.421 77.7859 101.421 c 95.6246 101.421 110.085 86.9584 110.085 69.1204 c 110.085 51.2802 95.6246 36.8194 77.7858 36.8194 c h 184.269 52.9353 m 177.123 52.9353 l 177.123 85.5232 l 184.269 85.5232 l 195.175 85.5232 201.256 79.076 201.256 69.2285 c 201.256 59.3089 195.279 52.9353 184.269 52.9353 c h 184.748 92.3238 m 184.748 92.3238 182.599 92.3238 173.376 92.3238 c 171.297 92.3238 169.633 90.7546 169.633 88.5796 c 169.633 49.8134 l 169.633 47.732 171.297 46.1383 173.376 46.1383 c 184.748 46.1383 l 200.164 46.1383 208.677 56.4697 208.677 69.2285 c 208.677 81.9917 200.072 92.3238 184.748 92.3238 c h 147.99 71.3098 m 134.335 71.3098 l 134.335 85.4598 l 134.335 85.4598 138.144 85.4598 147.99 85.4598 c 153.46 85.4598 155.972 81.9211 155.972 78.3852 c 155.972 74.8479 153.494 71.3098 147.99 71.3098 c h 146.294 52.9353 m 146.294 52.9353 147.094 52.9353 134.335 52.9353 c 134.335 64.5857 l 141.548 64.5857 146.294 64.5857 146.294 64.5857 c 151.036 64.5857 153.269 61.9477 153.269 58.758 c 153.269 55.4998 151.174 52.9353 146.294 52.9353 c h 147.722 92.3238 m 130.59 92.3238 l 128.51 92.3238 126.915 90.6594 126.915 88.5796 c 126.914 49.8134 l 126.914 47.7321 128.51 46.1383 130.589 46.1383 c 146.611 46.1383 l 155.417 46.1383 160.687 51.2607 160.687 57.9185 c 160.687 64.6491 156.574 67.0478 155.14 67.567 c 157.363 68.1266 163.392 70.6781 163.392 79.097 c 163.392 86.31 157.574 92.3238 147.722 92.3238 c 1 1 1 rg 1 1 1 RG f Q Q Q";
            content = "q 1 0 0 0 k 1 0 0 0 K Q q 0 1 0 0 k 0 1 0 0 K Q q 0 0 1 0 k 0 0 1 0 K Q q 0 0 0 1 k 0 0 0 1 K Q 1 0 0 -1 0 0 cm 1 0 0 1 0 -277 cm q 1 0 0 1 0 0 cm q n q 0 0 m 0 277 l 376 277 l 376 0 l h n 8 269 m 8 8 l 368 8 l 368 269 l 8 269 l h 0.71765 0.05882 0.67059 rg 0.71765 0.05882 0.67059 RG f 376 0 m 0 0 l 0 277 l 376 277 l 376 0 l 376 0 l h 360 16 m 360 261 l 16 261 l 16 16 l 360 16 l 0 0.05098 0.98824 rg 0 0.05098 0.98824 RG f Q Q Q  ";

            StringBuilder sb = new StringBuilder();
            using (System.IO.StreamReader sr = new System.IO.StreamReader(@"D:\Programme\Tools\Shape Converter\Testdaten\kombiniert\Zeus.txt"))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    sb.Append(line);
                }
            }
            content = sb.ToString();

            currentGraphicsState.Mirror = new Matrix(1, 0, 0, -1, 0, 277);

            byte[] bytes = System.Text.Encoding.ASCII.GetBytes(content);
            var sequence = ContentReader.ReadContent(bytes);
#else
            var sequence = ContentReader.ReadContent(page);
#endif

            var interpreter = new ContentInterpreter();
            return interpreter.Run(page, sequence, currentGraphicsState, invisibleGroups);
        }

        /// <summary>
        /// Get all invisible groups
        /// </summary>
        private PdfDictionary[] GetVisibleGroups(PdfDictionary prop)
        {
            if (prop == null)
            {
                return null;
            }

            var visibleGroups = new List<PdfDictionary>();
            var ocgs = prop.Elements.GetArray(PdfKeys.OCGs);

            var d = prop.Elements.GetDictionary(PdfKeys.D);
            var baseState = d.Elements.GetName(PdfKeys.BaseState);

            if (string.IsNullOrEmpty(baseState) || baseState == PdfKeys.ON)
            {
                for (int i = 0; i < ocgs.Elements.Count; i++)
                {
                    visibleGroups.Add(ocgs.Elements.GetDictionary(i));
                }
            }

            var on = d.Elements.GetArray(PdfKeys.ON);

            if (on != null)
            {
                for (int i = 0; i < on.Elements.Count; i++)
                {
                    var dict = on.Elements.GetDictionary(i);

                    if (!visibleGroups.Contains(dict))
                    {
                        visibleGroups.Add(dict);
                    }
                }
            }

            var off = d.Elements.GetArray(PdfKeys.OFF);

            if (off != null)
            {
                for (int i = 0; i < off.Elements.Count; i++)
                {
                    var dict = off.Elements.GetDictionary(i);

                    if (visibleGroups.Contains(dict))
                    {
                        visibleGroups.Remove(dict);
                    }
                }
            }

            return visibleGroups.ToArray();
        }
    }
}

