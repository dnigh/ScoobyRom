// TableWidget2D.cs: Builds a Gtk.Table showing 2D table data values.

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


using System;
using Gtk;

namespace GtkWidgets
{
	public sealed class TableWidget2D
	{
		const int DataColLeft = 1;
		const int DataRowTop = 1;

		readonly int countX, cols, rows;
		string axisMarkup = "X Axis [-]";
		string valuesMarkup = "Y Axis [-]";
		string formatValues = "0.000";
		readonly float[] axisX, values;
		readonly float axisXmin, axisXmax, valuesMax, valuesMin;

		Gtk.Table table;
		Widget[] axisWidgets, valueWidgets;

		readonly Util.Coloring coloring;

		/// <summary>
		///	Create Gtk.Table for 2D table data.
		/// </summary>
		public TableWidget2D (Util.Coloring coloring, float[] axisX, float[] valuesY, float axisXmin, float axisXmax, float valuesYmin, float valuesYmax)
		{
			this.coloring = coloring;
			this.axisX = axisX;
			this.axisXmin = axisXmin;
			this.axisXmax = axisXmax;
			this.values = valuesY;
			this.valuesMin = valuesYmin;
			this.valuesMax = valuesYmax;

			this.countX = this.axisX.Length;

			if (axisX.Length != valuesY.Length)
				throw new ArgumentException ("axisX.Length != valuesY.Length");

			this.cols = DataColLeft + 2 + 1;
			this.rows = this.countX + DataRowTop;
		}

		public string AxisMarkup {
			get { return this.axisMarkup; }
			set { axisMarkup = value; }
		}

		public string ValuesMarkup {
			get { return this.valuesMarkup; }
			set { valuesMarkup = value; }
		}

		public string HeaderAxisMarkup { get; set; }

		public string HeaderValuesMarkup { get; set; }

		public string FormatValues {
			get { return this.formatValues; }
			set { formatValues = value; }
		}

		public Gtk.Widget Create ()
		{
			table = new Gtk.Table ((uint)rows, (uint)cols, false);

			// could add some spacing so cell content won't touch
			// table.ColumnSpacing = table.RowSpacing = 0;

			const uint PadX = 2;
			const uint PadY = 2;

			// axis header, left
			Gtk.Label headerLeft = new Gtk.Label ();
			headerLeft.Markup = "<b>" + HeaderAxisMarkup + "</b>";
			table.Attach (headerLeft, DataColLeft, DataColLeft + 1, DataRowTop - 1, DataRowTop, AttachOptions.Shrink, AttachOptions.Shrink, 0, PadY);

			// table header, right
			Gtk.Label headerRight = new Gtk.Label ();
			headerRight.Markup = "<b>" + HeaderValuesMarkup + "</b>";
			table.Attach (headerRight, DataColLeft + 1, DataColLeft + 2, DataRowTop - 1, DataRowTop, AttachOptions.Shrink, AttachOptions.Shrink, 0, PadY);

			// x axis title
			Gtk.Label titleLeft = new Gtk.Label ();
			titleLeft.Angle = 90;
			titleLeft.Markup = "<b>" + this.axisMarkup + "</b>";
			table.Attach (titleLeft, 0, 1, 0, (uint)rows, AttachOptions.Shrink, AttachOptions.Shrink, 0, 0);

			// y axis title
			Gtk.Label titleRight = new Gtk.Label ();
			titleRight.Angle = 90;
			titleRight.Markup = "<b>" + this.valuesMarkup + "</b>";
			table.Attach (titleRight, DataColLeft + 2, DataColLeft + 3, 0, (uint)rows, AttachOptions.Shrink, AttachOptions.Shrink, 0, 0);

			// x values
			axisWidgets = new Widget[countX];
			for (uint i = 0; i < countX; i++) {
				float val = axisX [i];

				Gtk.Label label = new Label ();
				label.Text = val.ToString ();
				label.SetAlignment (1f, 0f);

				BorderWidget widget = new BorderWidget ();
				widget.Color = CalcAxisColor (val);
				widget.Add (label);

				axisWidgets [i] = widget;
				table.Attach (widget, DataColLeft, DataColLeft + 1, DataRowTop + i, DataRowTop + 1 + i, AttachOptions.Fill, AttachOptions.Shrink, PadX, PadY);
			}

			// y values
			int count = values.Length;
			valueWidgets = new Widget[count];
			for (uint i = 0; i < count; i++) {
				float val = values [i];

				Gtk.Widget label = new Label (val.ToString (this.formatValues));
				BorderWidget widget = new BorderWidget ();

				// ShadowType appearance differences might be minimal
				if (val >= this.valuesMax)
					widget.ShadowType = ShadowType.EtchedOut;
				else if (val <= this.valuesMin)
					widget.ShadowType = ShadowType.EtchedIn;

				widget.Color = CalcValueColor (val);
				widget.Add (label);

				valueWidgets [i] = widget;
				uint row = DataRowTop + i;
				uint col = DataColLeft + 1;

				table.Attach (widget, col, col + 1, row, row + 1, AttachOptions.Fill, AttachOptions.Fill, 0, 0);
			}
			return table;
		}

		Cairo.Color CalcValueColor (float val)
		{
			double factor = (val - valuesMin) / (valuesMax - valuesMin);
			// should be able to handle division by zero (NaN)
			return coloring.GetColor (factor);
		}

		Cairo.Color CalcAxisColor (float val)
		{
			double factor = (val - axisXmin) / (axisXmax - axisXmin);
			// should be able to handle division by zero (NaN)
			return coloring.GetColor (factor);
		}
	}
}