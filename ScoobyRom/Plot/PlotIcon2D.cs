// PlotIcon2D.cs: Create line graph bitmaps using NPlot.

/* Copyright (C) 2011-2015 SubaruDieselCrew
 *
 * This file is part of ScoobyRom.
 *
 * ScoobyRom is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * ScoobyRom is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with ScoobyRom.  If not, see <http://www.gnu.org/licenses/>.
 */


using NPlot;

namespace ScoobyRom
{
	/// <summary>
	/// Creates NPlot 2D graphs without any annotation, useful for icons.
	/// Methods are not thread safe!
	/// </summary>
	public sealed class PlotIcon2D : PlotIconBase
	{
		// Default = None
		const System.Drawing.Drawing2D.SmoothingMode SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

		readonly System.Drawing.Pen pen;

		public PlotIcon2D () : this (DefaultWidth, DefaultHeight)
		{
		}

		public PlotIcon2D (int width, int height) : base (width, height)
		{
			pen = new System.Drawing.Pen (System.Drawing.Color.Red, width >= 32 ? 2f : 1f);
		}

		public Gdk.Pixbuf CreateIcon2D (Subaru.Tables.Table2D table)
		{
			if (table.Ymin == table.Ymax)
				return GetNoDataPixBuf;

			plotSurface.Clear ();
			plotSurface.SmoothingMode = SmoothingMode;

			// y-values, x-values (!)
			LinePlot lp = new LinePlot (table.GetValuesYasFloats (), table.ValuesX);
			lp.Pen = pen;

			plotSurface.Add (lp);

			plotSurface.XAxis1.Hidden = true;
			plotSurface.YAxis1.Hidden = true;

			return DrawAndConvert ();
		}
	}
}