# ShapeConverter
The ShapeConverter converts vector data into WPF/XAML shapes. The main intention is to convert small icons ans logos.

##### Supported input file types
* Adobe Illustrator (\*.ai)
* Adobe Photoshop (\*.psd, \*.psb)
* Encapsulated Postscript (\*.eps)
* Scalable Vector Graphic (\*.svg, \*.svgz)

##### Multiple output formats

* StreamGeometry for single color shapes, allows to set the color at runtime
* DrawingBrush for multi color shapes
* C# source code to generate a Geometry with variable dimensions at runtime

##### All input files support

* linear and radial gradients
* transparency (exept EPS) plus transperant gradients

##### More features

* the generated data do not contain any transformations
* selectively deselect parts of the shape if the shape consists of multiple shapes
* clipping regions are minimized and optimized
* single executable
* support

##### Export

* Icon (\*.ico) with all resolutions 16x16, 32x32, 64x64, 128x128, 256x256
* Image (\*.png, \*.jpg, \*.tiff, \*.bmp)
* GIF (\*.giff)
* Scalable Vector Graphic (\*.svg)
* Encapsulated Postscript (\*.eps)


##### The ShapeConverter uses the following open source libraries

* PdfSharp from Stefan Lange, empira Software GmbH
* psd-parser from Ntreev Soft co., Ltd.
