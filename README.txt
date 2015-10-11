

SCOOBYROM


[]



QUICK FACTS


CONTENTS

1.  License
2.  Purpose
3.  Further Details

------------------------------------------------------------------------


1) License

GPLv3. See text file COPYING.txt for license text.

http://fsf.org/

You can also get license text in different formats and further details
there.

http://www.gnu.org/licenses/gpl.html

http://www.gnu.org/licenses/gpl-faq.html

------------------------------------------------------------------------


2) Purpose

_ScoobyRom_ is a _Denso_ ROM specific DATA VISUALIZATION VIEWER AND
METADATA EDITOR. Originally designed for _Subaru_ Diesel (Euro 4 & 5)
ROMs, some Subaru petrol models, as well as ROMs from other brands like
_Mazda_, _Nissan_ etc. might work. In general, different car models are
equipped with different ECUs, therefore success varies.

Currently it can find and visualize 2D (x-y) and 3D (x-y-z) tables. Also
it has some checksumming calculations built in.

ROM memory model is supposed to be 32 BIT, BIG ENDIAN, others are
unlikely to work.

ROM types confirmed working:

-   _Engine Control Unit_ (ECU):
    -   petrol and diesel models, _Renesas_ microcontrollers _SH7058_
        (1.0 MiB) and _SH7059_ (1.5 MiB)
-   _Transmission Control Unit_ (TCU):
    -   Automatic Transmission 5AT (1.0 MiB)

However you can try any file safely as it is being opened in READ-ONLY
mode. Worst thing that can happen is the app finds nothing at all or
false items only.

This application is not a ROM EDITOR (yet), you cannot change table
values or modify a ROM in any way! Remember, in this version the ROM
file is only being read. All additional data is saved into an extra XML
file. However, ScoobyRom has a _RomRaider ECU definition_ export
feature.

------------------------------------------------------------------------


3) Further Details

See README files in project subfolders.

Main Documentation is available in multiple formats:

-   Markdown (on _GitHub_ go there; source of all other formats)
-   HTML
-   Plain Text

