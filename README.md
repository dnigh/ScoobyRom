# ScoobyRom

![](ScoobyRom/Images/AppIcon.png)

Author: <http://subdiesel.wordpress.com/>

Project homepage on *GitHub*: <http://github.com/SubaruDieselCrew/ScoobyRom/>

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

*ScoobyRom* is a car control unit ([ECU](http://en.wikipedia.org/wiki/Engine_Control_Unit), [TCU](http://en.wikipedia.org/wiki/Transmission_Control_Unit)) firmware (ROM) **data visualization viewer and metadata editor**.
Currently it is very [*Denso*](http://en.wikipedia.org/wiki/Denso) specific.

Originally designed for *Subaru* Diesel (Euro 4 & 5) ROMs, some *Subaru* petrol models, as well as ROMs from other manufacturers like *Mazda*, *Mitsubishi*, *Nissan* etc. have been tested working (where *Denso* supplied control units).
In general, different car models are equipped with different ECUs (hardware and/or firmware), therefore success varies.

Currently it can find and visualize 2D (x-y) and 3D (x-y-z) tables ("maps").
Also it displays and verifies checksums.

ROM memory model is supposed to be **32 bit, big endian**, others are unlikely to work.

ROM types confirmed working:

* *Engine Control Unit* (ECU):
	*	petrol and diesel models
	*	*Renesas* microcontrollers
		*	*SH7055* (512 KiB)
		*	*SH7058, SH7058S* (1.0 MiB)
		*	*SH7059* (1.5 MiB)

* *Transmission Control Unit* (TCU):
	*	Automatic Transmission (*Subaru 5AT*) (*SH7058*, 1.0 MiB)

However you can try any file safely as it is being opened in **read-only** mode.
Worst thing that can happen is the app finds nothing at all or false items only.

This application is **not a ROM editor** (yet), you cannot change table values or modify a ROM in any way!
Remember, in this version the ROM file is only being read.
All additional data is saved into an extra XML file.
However, ScoobyRom has a *RomRaider ECU definition* export feature.

>*RomRaider* is a free, open source tuning suite created for viewing, logging and tuning of modern Subaru Engine Control Units and some older BMW M3 (MS41/42/43) DME."
<http://www.romraider.com>

---

## 3) Further Details

See `README.md` and other documentation files (`*.md`) in project subfolders.

Main documentation:

*	[ScoobyRom/README.md](ScoobyRom/README.md) (click this on *GitHub* - it will render as HTML in web browser)

Following formats are also included in binary download package (`.ZIP`) for convenience, automatically generated from above Markdown source:
 
*	ScoobyRom/README.html
