﻿// DataViewModelBaseGtk.cs: Gtk.TreeModel common functionality for UI

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


#define UseBackGroundTask

using System;
using System.Threading;
using System.Threading.Tasks;
using Gtk;

namespace ScoobyRom
{
	// sort of ViewModel in M-V-VM (Model-View-ViewModel pattern)
	public abstract class DataViewModelBaseGtk
	{
		protected CancellationTokenSource tokenSource = new CancellationTokenSource();
		protected Task task;

		// generates icons
		protected PlotIconBase plotIcon;

		protected readonly Data data;

		// main TreeStore, most of the core data is being copied into this
		public ListStore store;

		protected bool iconsCached;

		public TreeModel TreeModel {
			get { return this.store; }
		}

		protected abstract int ColumnNrIcon { get; }
		protected abstract int ColumnNrObj { get; }

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

		public void ChangeTableType (Subaru.Tables.Table table, Subaru.Tables.TableType newType)
		{
			data.ChangeTableType (table, newType);
		}


		/// <summary>
		/// Creates icons if not done already, otherwise returns immediatly.
		/// Icon creation happens in background.
		/// </summary>
		public void RequestIcons ()
		{
			if (iconsCached)
				return;
			RefreshIcons ();
		}

		public void IncreaseIconSize ()
		{
			plotIcon.IncreaseIconSize ();
			RefreshIcons ();
		}

		public void DecreaseIconSize ()
		{
			plotIcon.DecreaseIconSize ();
			RefreshIcons ();
		}

		public void RefreshIcons ()
		{
			iconsCached = false;

			#if !UseBackGroundTask

			CreateAllIcons ();
			iconsCached = true;

			#else

			if (task != null && !task.IsCompleted)
			{
				tokenSource.Cancel ();
				// "It is not necessary to wait on tasks that have canceled."
				// https://msdn.microsoft.com/en-us/library/dd537607%28v=vs.100%29.aspx
				//task.Wait (200);
				task = null;
			}

			tokenSource = new CancellationTokenSource ();
			var token = tokenSource.Token;
			task = Task.Factory.StartNew (() => CreateAllIcons (token), token);

			#endif
		}

		protected void CreateAllIcons (CancellationToken ct)
		{
			int objColumnNr = ColumnNrObj;
			int iconColumnNr = ColumnNrIcon;

			TreeIter iter;
			if (!store.GetIterFirst (out iter))
				return;

			if (ct.IsCancellationRequested)
			{
				return;
			}

			do {
				Subaru.Tables.Table table = (Subaru.Tables.Table)store.GetValue (iter, objColumnNr);
				Gdk.Pixbuf pixbuf = plotIcon.CreateIcon (table);

				// update model reference in GUI Thread to make sure UI display is ok
				// HACK Application.Invoke causes wrong iters ???
				// IA__gtk_list_store_set_value: assertion 'VALID_ITER (iter, list_store)' failed
				//Application.Invoke (delegate {
					store.SetValue (iter, iconColumnNr, pixbuf);
				//});
				if (ct.IsCancellationRequested)
				{
					return;
				}
			} while (store.IterNext (ref iter));
			iconsCached = true;
			plotIcon.CleanupTemp ();
		}

		public abstract void SetNodeContentTypeChanged (TreeIter iter, Subaru.Tables.Table table);

		protected void CreateSetNewIcon (TreeIter iter, Subaru.Tables.Table table)
		{
			store.SetValue (iter, ColumnNrIcon, plotIcon.CreateIcon (table));
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

		abstract protected void InitStore ();

		abstract protected void PopulateData ();

		abstract protected void UpdateModel (TreeIter iter);
	}
}