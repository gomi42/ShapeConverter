# PdfSharp
The PdfSharpReader is based on version 1.5 of the PdfSharp project from Stefan Lange, empira Software GmbH.

The only modifications made are:

* remove all write functionality (classes and methods) to reduce the size of the library significantly
* make very few properties of the PdfPage `public`
* change the type of these properties to `PdfDictionary`
