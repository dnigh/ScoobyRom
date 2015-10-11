# ScoobyRom

![](ScoobyRom/Images/AppIcon.png)

# Quick Facts

## CONTENTS

1.	License
2.	Purpose
3.	Further Details

---

## 1) License

GPLv3. See text file [COPYING.txt](COPYING.txt) for license text.

[http://fsf.org/](http://fsf.org/ "Free Software Foundation")

You can also get license text in different formats and further details there.

[http://www.gnu.org/licenses/gpl.html](http://www.gnu.org/licenses/gpl.html)

[http://www.gnu.org/licenses/gpl-faq.html](http://www.gnu.org/licenses/gpl-faq.html)

---

## 2) Purpose

*ScoobyRom* is a *Denso* ROM specific **data visualization viewer and metadata editor**.
Originally designed for *Subaru* Diesel (Euro 4 & 5) ROMs, some Subaru petrol models,
as well as ROMs from other brands like *Mazda*, *Nissan* etc. might work.
In general, different car models are equipped with different ECUs, therefore success varies.

Currently it can find and visualize 2D (x-y) and 3D (x-y-z) tables.
Also it has some checksumming calculations built in.

ROM memory model is supposed to be **32 bit, big endian**, others are unlikely to work.

ROM types confirmed working:

* *Engine Control Unit* (ECU):
	* petrol and diesel models, *Renesas* microcontrollers *SH7058* (1.0 MiB) and *SH7059* (1.5 MiB)

* *Transmission Control Unit* (TCU):
	* Automatic Transmission 5AT (1.0 MiB)

However you can try any file safely as it is being opened in **read-only** mode.
Worst thing that can happen is the app finds nothing at all or false items only.

This application is not a **ROM editor** (yet), you cannot change table values or modify a ROM in any way!
Remember, in this version the ROM file is only being read.
All additional data is saved into an extra XML file.
However, ScoobyRom has a *RomRaider ECU definition* export feature.

---

## 3) Further Details

See README files in project subfolders.

Main Documentation is available in multiple formats:

*	[Markdown](ScoobyRom/README.md) (on *GitHub* go there; source of all other formats)
*	[HTML](ScoobyRom/README.html)
*	[Plain Text](ScoobyRom/README.txt)
