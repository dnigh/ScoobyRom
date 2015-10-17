// DataView2DModelGtk.cs: Gtk.TreeModel for UI.

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
using System.Threading.Tasks;
using Gtk;
using Subaru.Tables;

namespace ScoobyRom
{
	// sort of ViewModel in M-V-VM (Model-View-ViewModel pattern)
	public sealed class DataView2DModelGtk : DataViewModelBaseGtk
	{
		public DataView2DModelGtk (Data data, int iconWidth, int iconHeight) : base (data, new PlotIcon2D (iconWidth, iconHeight))
		{
			data.ItemsChanged2D += OnDataItemsChanged;
		}

		public void ChangeTableType (Table2D table2D, TableType newType)
		{
			data.ChangeTableType (table2D, newType);
		}

		override protected void InitStore ()
		{
			// TODO avoid reflection
			int count = ((ColumnNr2D[])Enum.GetValues (typeof(ColumnNr2D))).Length;
			// WARNING: crashes when an array slot wasn't initialized
			Type[] types = new Type[count];

			// using enum in the store --> Gtk-WARNING **: Attempting to sort on invalid type GtkSharpValue
			// --> use int instead

			types [(int)ColumnNr2D.Category] = typeof(string);
			types [(int)ColumnNr2D.Toggle] = typeof(bool);
			types [(int)ColumnNr2D.Icon] = typeof(Gdk.Pixbuf);
			types [(int)ColumnNr2D.Title] = typeof(string);
			types [(int)ColumnNr2D.Type] = typeof(int);
			types [(int)ColumnNr2D.UnitY] = typeof(string);

			types [(int)ColumnNr2D.NameX] = typeof(string);
			types [(int)ColumnNr2D.UnitX] = typeof(string);

			types [(int)ColumnNr2D.CountX] = typeof(int);

			types [(int)ColumnNr2D.Xmin] = typeof(float);
			types [(int)ColumnNr2D.Xmax] = typeof(float);
			types [(int)ColumnNr2D.Ymin] = typeof(float);
			types [(int)ColumnNr2D.Yavg] = typeof(float);
			types [(int)ColumnNr2D.Ymax] = typeof(float);

			types [(int)ColumnNr2D.Location] = typeof(int);
			types [(int)ColumnNr2D.YPos] = typeof(int);

			types [(int)ColumnNr2D.Description] = typeof(string);

			types [(int)ColumnNr2D.Obj] = typeof(object);

			store = new ListStore (types);

			// not called on TreeView-built-in reorder! called a lot when re-populating store
			//store.RowsReordered += HandleTreeStoreRowsReordered;
			store.RowChanged += HandleTreeStoreRowChanged;
		}

		override protected void PopulateData ()
		{
			// performance, would get raised for each new row
			store.RowChanged -= HandleTreeStoreRowChanged;

			foreach (Table2D table2D in data.List2D) {
				TreeIter newNode = store.Append ();
				SetNodeContent (newNode, table2D);
			}

			store.RowChanged += HandleTreeStoreRowChanged;
		}

		public void SetNodeContent (TreeIter iter, Table2D table2D)
		{
			// TODO optimize when columns are final

			store.SetValue (iter, (int)ColumnNr2D.Obj, table2D);

			store.SetValue (iter, (int)ColumnNr2D.Category, table2D.Category);
			store.SetValue (iter, (int)ColumnNr2D.Toggle, false);
			store.SetValue (iter, (int)ColumnNr2D.Title, table2D.Title);
			store.SetValue (iter, (int)ColumnNr2D.UnitY, table2D.UnitY);

			store.SetValue (iter, (int)ColumnNr2D.NameX, table2D.NameX);
			store.SetValue (iter, (int)ColumnNr2D.UnitX, table2D.UnitX);


			store.SetValue (iter, (int)ColumnNr2D.CountX, table2D.CountX);

			store.SetValue (iter, (int)ColumnNr2D.Xmin, table2D.Xmin);
			store.SetValue (iter, (int)ColumnNr2D.Xmax, table2D.Xmax);

			store.SetValue (iter, (int)ColumnNr2D.Location, table2D.Location);
			store.SetValue (iter, (int)ColumnNr2D.YPos, table2D.RangeY.Pos);
			store.SetValue (iter, (int)ColumnNr2D.Description, table2D.Description);

			SetNodeContentTypeChanged (iter, table2D);
		}

		public void SetNodeContentTypeChanged (TreeIter iter, Table2D table2D)
		{
			store.SetValue (iter, (int)ColumnNr2D.Type, (int)table2D.TableType);
			store.SetValue (iter, (int)ColumnNr2D.Ymin, table2D.Ymin);
			store.SetValue (iter, (int)ColumnNr2D.Yavg, table2D.Yavg);
			store.SetValue (iter, (int)ColumnNr2D.Ymax, table2D.Ymax);

			if (iconsCached)
				CreateSetNewIcon (iter, table2D);
		}

		void CreateSetNewIcon (TreeIter iter, Table2D table2D)
		{
			store.SetValue (iter, (int)ColumnNr2D.Icon, plotIcon.CreateIcon (table2D));
		}

		protected override void UpdateModel (TreeIter iter)
		{
			Table2D table = store.GetValue (iter, (int)ColumnNr2D.Obj) as Table2D;
			if (table == null)
				return;
			table.Category = (string)store.GetValue (iter, (int)ColumnNr2D.Category);
			table.Title = (string)store.GetValue (iter, (int)ColumnNr2D.Title);
			table.UnitY = (string)store.GetValue (iter, (int)ColumnNr2D.UnitY);
			table.NameX = (string)store.GetValue (iter, (int)ColumnNr2D.NameX);
			table.UnitX = (string)store.GetValue (iter, (int)ColumnNr2D.UnitX);
			table.Description = (string)store.GetValue (iter, (int)ColumnNr2D.Description);
		}

		protected override void CreateAllIcons ()
		{
			CreateAllIcons ((int)ColumnNr2D.Obj, (int)ColumnNr2D.Icon);
		}
	}
}