// DataViewModelBaseGtk.cs: Gtk.TreeModel common functionality for UI

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

namespace ScoobyRom
{
	// sort of ViewModel in M-V-VM (Model-View-ViewModel pattern)
	public abstract class DataViewModelBaseGtk
	{
		// generates icons
		protected readonly PlotIconBase plotIcon;

		protected readonly Data data;

		// main TreeStore, most of the core data is being copied into this
		public ListStore store;

		protected bool iconsCached;

		public TreeModel TreeModel {
			get { return this.store; }
		}

		public DataViewModelBaseGtk (Data data, PlotIconBase plotIcon)
		{
			this.data = data;
			this.plotIcon = plotIcon;
			InitStore ();
		}

		protected void OnDataItemsChanged (object sender, EventArgs e)
		{
			this.store.Clear ();
			PopulateData ();
		}

		/// <summary>
		/// Creates icons if not done already, otherwise returns immediatly.
		/// Icon creation happens in background.
		/// </summary>
		public void RequestIcons ()
		{
			if (iconsCached)
				return;
			Task task = new Task (() => CreateAllIcons ());
			task.ContinueWith (t => iconsCached = true);
			task.Start ();
		}

		protected void CreateAllIcons (int objColumnNr, int iconColumnNr)
		{
			TreeIter iter;
			if (!store.GetIterFirst (out iter))
				return;
			do {
				Subaru.Tables.Table table = (Subaru.Tables.Table)store.GetValue (iter, objColumnNr);
				Gdk.Pixbuf pixbuf = plotIcon.CreateIcon (table);

				// update model reference in GUI Thread to make sure UI display is ok
				Application.Invoke (delegate {
					store.SetValue (iter, iconColumnNr, pixbuf);
				});
			} while (store.IterNext (ref iter));
			plotIcon.CleanupTemp ();
		}

		#region TreeStore event handlers

		// called for each changed column!
		protected void HandleTreeStoreRowChanged (object o, RowChangedArgs args)
		{
			//Console.WriteLine ("TreeStoreRowChanged");
			UpdateModel (args.Iter);
		}

		// not called when treeView.Reorderable = true !!!
		// called when clicking column headers
		//		void HandleTreeStoreRowsReordered (object o, RowsReorderedArgs args)
		//		{
		//			Console.WriteLine ("TreeStore3D: RowsReordered");
		//		}

		#endregion TreeStore event handlers

		abstract protected void CreateAllIcons ();

		abstract protected void InitStore ();

		abstract protected void PopulateData ();

		abstract protected void UpdateModel (TreeIter iter);
	}
}