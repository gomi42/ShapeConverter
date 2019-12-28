# Ntreev Photoshop Document Parser for .Net
This is a copy of the psd-parser version 1.1.18306.1005 from Ntreev Soft co., Ltd.
##### Changes
To simplify the handling of the parameters the image, layer and mask resource readers were changed to return specific types and not a universal type. That avoids casting and other tests in processing chain.
##### Additions
Some layer resource readers (e.g. GdFl, PlLd, PtFl, SoCo, vmsk, vstk) were added as well as the IccProfile resource reader.
