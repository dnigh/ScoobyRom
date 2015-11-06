// Data.cs: Main model class, should be independent of UI.

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
using System.Collections.Generic;
using System.Linq;
using Subaru.File;
using Subaru.Tables;

namespace ScoobyRom
{
	public sealed class Data
	{
		public event EventHandler<EventArgs> RomChanged;

		public event EventHandler<EventArgs> ItemsChanged2D;
		public event EventHandler<EventArgs> ItemsChanged3D;

		// only for loading ROM so far
		public event EventHandler<System.ComponentModel.ProgressChangedEventArgs> ProgressChanged;

		bool romLoaded = false;
		Subaru.File.Rom rom;
		RomMetadata romMetadata = new RomMetadata ();
		string calIDfromRom;

		// proper values can speed up searching a lot - e.g. 300 ms instead of several seconds
		Util.Range? tableSearchRange;

		IList<Table2D> list2D = new List<Table2D> (0);
		IList<Table3D> list3D = new List<Table3D> (0);

		// System.Collections.ObjectModel
		//ObservableCollection<Table3D> coll3D = new ObservableCollection<Table3D>();


		public IList<Table2D> List2D {
			get { return this.list2D; }
		}

		public IList<Table3D> List3D {
			get { return this.list3D; }
		}

		public IList<Table2D> List2DAnnotated ()
		{
			return list2D.Where (t => t.HasMetadata).AsParallel ().ToList ();
		}

		public IList<Table3D> List3DAnnotated ()
		{
			return list3D.Where (t => t.HasMetadata).AsParallel ().ToList ();
		}

		public IList<Table2D> List2DAnnotatedSorted ()
		{
			return list2D.Where (t => t.HasMetadata).OrderBy (t => t.Location).AsParallel ().ToList ();
		}

		public IList<Table3D> List3DAnnotatedSorted ()
		{
			return list3D.Where (t => t.HasMetadata).OrderBy (t => t.Location).AsParallel ().ToList ();
		}

		public IList<Table2D> List2DSelected ()
		{
			return list2D.Where (t => t.Selected).AsParallel ().ToList ();
		}

		public IList<Table3D> List3DSelected ()
		{
			return list3D.Where (t => t.Selected).AsParallel ().ToList ();
		}

		public IList<Table2D> List2DSelectedSorted ()
		{
			return list2D.Where (t => t.Selected).OrderBy (t => t.Location).AsParallel ().ToList ();
		}

		public IList<Table3D> List3DSelectedSorted ()
		{
			return list3D.Where (t => t.Selected).OrderBy (t => t.Location).AsParallel ().ToList ();
		}


		public Subaru.File.Rom Rom {
			get { return this.rom; }
		}

		public bool RomLoaded {
			get { return this.romLoaded; }
		}

		public string CalID {
			get { return this.calIDfromRom; }
		}

		public Util.Range? TableSearchRange {
			get { return this.tableSearchRange; }
		}

		public Data ()
		{
		}

		public void LoadRom (string path)
		{
			romLoaded = false;
			rom = new Subaru.File.Rom (path);

			rom.FindMetadata ();

			string xmlPath = PathWithNewExtension (path, ".xml");
			bool xmlExists = System.IO.File.Exists (xmlPath);

			Subaru.File.RomXml romXml = null;
			if (xmlExists) {
				Console.WriteLine ("Loading existing XML file " + xmlPath);
				romXml = new Subaru.File.RomXml ();
				romXml.Load (xmlPath);
				romMetadata = romXml.RomMetadata;
				tableSearchRange = romXml.TableSearchRange;
			} else {
				Console.WriteLine ("No existing XML file has been found!");
				romXml = null;
				romMetadata = new RomMetadata ();
				tableSearchRange = null;
			}

			romMetadata.Filesize = rom.Size;
			int calIDpos = romMetadata.CalibrationIDPos;

			calIDfromRom = calIDpos != 0 ? rom.ReadASCII (calIDpos, 8) : "Unknown";
			if (calIDfromRom != romMetadata.CalibrationID)
				Console.Error.WriteLine ("WARNING: Calibration ID mismatch");

			if (this.ProgressChanged != null)
				rom.ProgressChanged += OnProgressChanged;

			rom.FindMaps (tableSearchRange, out list2D, out list3D);

			rom.ProgressChanged -= OnProgressChanged;

			romLoaded = true;

			if (romXml != null) {
				romXml.RomStream = rom.Stream;
				romXml.TryMergeWith (list2D);
				romXml.TryMergeWith (list3D);
			}
		}

		public static string PathWithNewExtension (string path, string extension)
		{
			return System.IO.Path.Combine (System.IO.Path.GetDirectoryName (path), System.IO.Path.GetFileNameWithoutExtension (path) + extension);
		}

		public void SaveXml ()
		{
			SaveXml (PathWithNewExtension (rom.Path, ".xml"));
		}

		public void SaveXml (string path)
		{
			var romXml = new Subaru.File.RomXml ();
			romXml.TableSearchRange = tableSearchRange;
			romXml.WriteXml (path, romMetadata, list2D, list3D);
		}

		public void SaveAsRomRaiderXml (string path)
		{
			// TODO add filtering options

			// only export annotated tables:
			/*
			var list2D = List2DAnnotated;
			var list3D = List3DAnnotated;
			*/
			// export everything:
			var list2D = this.List2DAnnotatedSorted ();
			var list3D = this.List3DAnnotatedSorted ();

			// provide record location as name, otherwise RomRaider names them "Unamed " + ValuesLocation
			// HACK
			foreach (var item in list2D) {
				if (string.IsNullOrEmpty (item.Title))
					item.Title = string.Format ("Record 0x{0:X}", item.Location);
			}

			foreach (var item in list3D) {
				if (string.IsNullOrEmpty (item.Title))
					item.Title = string.Format ("Record 0x{0:X}", item.Location);
			}

			Subaru.File.RomRaiderEcuDefXml.WriteRRXmlFile (path, romMetadata.XElement, list2D, list3D);
		}

		public void ChangeTableType (Table table, TableType newType)
		{
			table.ChangeTypeToAndReload (newType, rom.Stream);
		}

		public void UpdateUI ()
		{
			if (RomChanged != null)
				RomChanged (this, new EventArgs ());
			if (ItemsChanged3D != null)
				ItemsChanged3D (this, new EventArgs ());
			if (ItemsChanged2D != null)
				ItemsChanged2D (this, new EventArgs ());
		}

		void OnProgressChanged (object sender, System.ComponentModel.ProgressChangedEventArgs e)
		{
			if (this.ProgressChanged != null) {
				this.ProgressChanged (sender, e);
			}
		}

		public static int AutomaticMinDigits (float[] values)
		{
			const int MaxDecimals = 8;

			int digits = 0;
			do {
				bool found = true;
				for (int i = 0; i < values.Length; i++) {
					float value = values [i];
					float rounded = Convert.ToSingle (Math.Round (value, digits));
					if (Math.Abs (value - rounded) > float.Epsilon) {
						++digits;
						found = false;
						break;
					}
				}
				if (found)
					break;
			} while (digits <= MaxDecimals);
			return digits;
		}

		public static string ValueFormat (int decimals)
		{
			if (decimals < 1)
				return "0";
			return "0." + new string ('0', decimals);
		}

		// HACK might need better algorithm
		public static string AutomaticValueFormat (float[] values, float valuesMin, float valuesMax)
		{
			int digits = ScoobyRom.Data.AutomaticMinDigits (values);
			if (digits > 3) {
				digits = valuesMax < 30 ? 2 : 1;
				if (valuesMax < 10)
					digits = 3;
			}
			return ValueFormat (digits);
		}

		// some x-axis is shared many times with both 2D and 3D tables
		public IList<Table> FindTablesSameAxisX (Table table)
		{
			var r2 = list2D.Where (t => t.RangeX.Pos == table.RangeX.Pos).Cast <Table> ();
			var r3 = list3D.Where (t => t.RangeX.Pos == table.RangeX.Pos).Cast <Table> ();

			var r = r2.Concat (r3).ToList ();

			for (int i = 0; i < r.Count; i++) {
				Console.WriteLine ("#{0}: {1}", i, r [i]);
			}
			return r;
		}
	}
}