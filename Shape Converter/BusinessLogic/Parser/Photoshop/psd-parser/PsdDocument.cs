//Released under the MIT License.
//
//Copyright (c) 2015 Ntreev Soft co., Ltd.
//
//Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
//documentation files (the "Software"), to deal in the Software without restriction, including without limitation the 
//rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit 
//persons to whom the Software is furnished to do so, subject to the following conditions:
//
//The above copyright notice and this permission notice shall be included in all copies or substantial portions of the 
//Software.
//
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE 
//WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR 
//COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR 
//OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using Ntreev.Library.Psd.Readers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Ntreev.Library.Psd.Readers.ImageResources;

namespace Ntreev.Library.Psd
{
    public class PsdDocument : IPsdLayer, IDisposable
    {
        private FileHeaderSection fileHeaderSection;
        private ColorModeDataSection colorModeDataSection;
        private ImgageResourcesSection imageResourcesSection;
        private LayerAndMaskInformationSection layerAndMaskSection;
        private ImageDataSection imageDataSection;

        private PsdReader reader;
        //private Uri baseUri;

        public PsdDocument()
        {

        }

        public static PsdDocument Create(string filename)
        {
            return PsdDocument.Create(filename, new PathResolver());
        }

        public static PsdDocument Create(string filename, PsdResolver resolver)
        {
            PsdDocument document = new PsdDocument();
            FileInfo fileInfo = new FileInfo(filename);
            FileStream stream = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
            document.Read(stream, resolver, new Uri(fileInfo.DirectoryName));
            return document;
        }

        public static PsdDocument Create(Stream stream)
        {
            return PsdDocument.Create(stream, null);
        }

        public static PsdDocument Create(Stream stream, PsdResolver resolver)
        {
            PsdDocument document = new PsdDocument();
            document.Read(stream, resolver, new Uri(Directory.GetCurrentDirectory()));
            return document;
        }

        public void Dispose()
        {
            if (this.reader == null)
                return;

            this.reader.Dispose();
            this.reader = null;
            this.OnDisposed(EventArgs.Empty);
        }

        public FileHeaderSection FileHeaderSection
        {
            get { return this.fileHeaderSection; }
        }

        public byte[] ColorModeData
        {
            get { return this.colorModeDataSection.ColorModeData; }
        }

        public int Width
        {
            get { return this.fileHeaderSection.Width; }
        }

        public int Height
        {
            get { return this.fileHeaderSection.Height; }
        }

        public int Depth
        {
            get { return this.fileHeaderSection.Depth; }
        }

        public IPsdLayer[] Childreen
        {
            get { return this.layerAndMaskSection.Layers; }
        }

        public IEnumerable<ILinkedLayer> LinkedLayers
        {
            get { return this.layerAndMaskSection.LinkedLayers; }
        }

        public IResourceBlock[] Resources
        {
            get { return this.layerAndMaskSection.Resources; }
        }

        public IResourceBlock[] ImageResources
        {
            get { return this.imageResourcesSection.Resources; }
        }

        public bool HasImage
        {
            get
            {
                var block = this.imageResourcesSection.Resources.FirstOrDefault(x => x is VersionInfoResource) as VersionInfoResource;

                if (block == null)
                {
                    return false;
                }

                return block.HasCompatibilityImage;
            }
        }

        public bool IsVisible => true;

        public event EventHandler Disposed;

        protected virtual void OnDisposed(EventArgs e)
        {
            if (this.Disposed != null)
            {
                this.Disposed(this, e);
            }
        }

        internal void Read(Stream stream, PsdResolver resolver, Uri uri)
        {
            this.reader = new PsdReader(stream, resolver, uri);
            this.reader.ReadDocumentHeader();

            this.fileHeaderSection = new FileHeaderSection(this.reader);
            this.colorModeDataSection = new ColorModeDataSection(this.reader);
            this.imageResourcesSection = new ImgageResourcesSection(this.reader);
            this.layerAndMaskSection = new LayerAndMaskInformationSection(this.reader, this);
            this.imageDataSection = new ImageDataSection(this.reader, this);
        }

        IPsdLayer IPsdLayer.Parent
        {
            get { return null; }
        }

        bool IPsdLayer.IsClipping
        {
            get { return false; }
        }

        PsdDocument IPsdLayer.Document
        {
            get { return this; }
        }

        ILinkedLayer IPsdLayer.LinkedLayer
        {
            get { return null; }
        }

        string IPsdLayer.Name
        {
            get { return "Document"; }
        }

        int IPsdLayer.Left
        {
            get { return 0; }
        }

        int IPsdLayer.Top
        {
            get { return 0; }
        }

        int IPsdLayer.Right
        {
            get { return this.Width; }
        }

        int IPsdLayer.Bottom
        {
            get { return this.Height; }
        }

        BlendMode IPsdLayer.BlendMode
        {
            get { return BlendMode.Normal; }
        }

        IChannel[] IImageSource.Channels
        {
            get { return this.imageDataSection.Channels; }
        }

        float IImageSource.Opacity
        {
            get { return 1.0f; }
        }

        bool IImageSource.HasMask
        {
            get { return this.FileHeaderSection.NumberOfChannels > 4; }
        }
    }
}
