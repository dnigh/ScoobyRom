// Rom.cs: ROM class - read/analyze ROM file content.

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
using System.ComponentModel;
using System.IO;
using Subaru.Tables;

namespace Subaru.File
{
	public sealed class Rom : IDisposable
	{
		/// <summary>
		/// Progress info while ROM is being analyzed.
		/// </summary>
		public event EventHandler<ProgressChangedEventArgs> ProgressChanged;

		string path;
		FileStream fs;
		RomType romType;
		// ROM files are much smaller (couple MiB max) than 2 GiB so Int32 for pos, size etc. is sufficient
		int startPos, lastPos;
		int percentDoneLastReport;

		public string Path {
			get { return this.path; }
		}

		public FileStream Stream {
			get { return this.fs; }
		}

		public RomType RomType {
			get { return this.romType; }
		}

		public RomChecksumming RomChecksumming {
			get { return new RomChecksumming (this.romType, this.fs); }
		}

		public Rom (string path)
		{
			this.path = path;
			Open ();
		}

		void Open ()
		{
			fs = new FileStream (path, FileMode.Open, FileAccess.Read);
			romType = DetectRomType (fs);
		}

		/// <summary>
		/// Read raw bytes and convert to string.
		/// (Ex: Calibration ID)
		/// </summary>
		/// <param name="pos">
		/// A <see cref="System.Int32"/>
		/// </param>
		/// <param name="length">
		/// A <see cref="System.Int32"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.String"/>
		/// </returns>
		public string ReadASCII (int pos, int length)
		{
			fs.Seek (pos, SeekOrigin.Begin);
			byte[] bytes = new byte[length];
			fs.Read (bytes, 0, length);
			return System.Text.Encoding.ASCII.GetString (bytes);
		}

		public void Close ()
		{
			if (fs != null) {
				fs.Dispose ();
				fs = null;
			}
		}

		void CheckProgress (long pos)
		{
			if (ProgressChanged == null)
				return;

			float percent = 100f * (((float)pos - (float)startPos)) / ((float)lastPos - (float)startPos);
			int percentDone = (int)(percent);
			if (percentDone >= percentDoneLastReport + 10) {
				percentDoneLastReport = percentDone;
				OnProgressChanged (percentDone);
			}
		}

		void OnProgressChanged (int percentDone)
		{
			if (ProgressChanged == null)
				return;
			else
				ProgressChanged (this, new ProgressChangedEventArgs (percentDone, null));
		}

		/*
		public IList<Table3D> FindMaps3D ()
		{
			return FindMaps3D (0);
		}

		public IList<Table3D> FindMaps3D (int startPos)
		{
			// should stop at EOF
			return FindMaps3D (startPos, (int)fs.Length);
		}

		public IList<Table3D> FindMaps3D (int startPos, int lastPos)
		{
			this.startPos = startPos;
			lastPos = Math.Min (lastPos, (int)fs.Length);
			this.lastPos = lastPos;
			OnProgressChanged (0);
			this.percentDoneLastReport = 0;
			List<Table3D> list3D = new List<Table3D> (800);

			for (long pos = startPos; pos <= lastPos;) {
				// check for end of file
				if (pos >= fs.Length)
					break;
				fs.Position = pos;
				CheckProgress (pos);

				Table3D info3D = Table3D.TryParseValid (this.fs);
				if (info3D != null) {
					list3D.Add (info3D);
					pos = fs.Position;
				} else {
					// not valid, try at next possible location
					pos++;
				}
			}
			OnProgressChanged (100);
			return list3D;
		}
		*/

		public void FindMaps (int startPos, int lastPos, out IList<Table2D> list2D, out IList<Table3D> list3D)
		{
			this.startPos = startPos;
			lastPos = Math.Min (lastPos, (int)fs.Length);
			this.lastPos = lastPos;

			// Restricting pointers to a range can improve detection accuracy.
			// Using conservative settings here:
			Table.PosMin = 8 * 1024;
			Table.PosMax = (int)fs.Length - 1;

			// default capacities suitable for current SH7059 diesel ROMs
			list2D = new List<Table2D> (1000);
			list3D = new List<Table3D> (1200);

			OnProgressChanged (0);
			this.percentDoneLastReport = 0;

			for (long pos = startPos; pos <= lastPos;) {
				// check for end of file
				if (pos >= fs.Length)
					break;
				fs.Position = pos;
				CheckProgress (pos);

				// try Table3D first as it contains more struct info; more info to validate = better detection
				Table3D info3D = Table3D.TryParseValid (this.fs);
				if (info3D != null) {
					list3D.Add (info3D);
					pos = fs.Position;
				} else {
					// must back off
					fs.Position = pos;
					// not 3D, try 2D
					Table2D info2D = Table2D.TryParseValid (this.fs);
					if (info2D != null) {
						list2D.Add (info2D);
						pos = fs.Position;
					} else {
						// nothing valid, try at next possible location
						pos++;
					}
				}
			}
			OnProgressChanged (100);
		}

		public void FindMetadata ()
		{
			const int Pos_SH7058_Diesel = 0x4000;
			const int RomIDlongLength = 32;
			const int CIDLength = 8;

			int pos = 0;
			switch (RomType) {
			case RomType.SH7058:
			case RomType.SH7059:
				pos = Pos_SH7058_Diesel;
				break;
			default:
				Console.WriteLine ("Unknown RomType");
				return;
			}


			string romIDlong = ReadASCII (pos, RomIDlongLength).TrimEnd();
			Console.WriteLine ("RomIDlong[{0}]: {1}", romIDlong.Length.ToString(), romIDlong);

			string CID = romIDlong.Substring (romIDlong.Length - CIDLength);
			Console.WriteLine ("CID: {0}", CID);

			fs.Position = pos + RomIDlongLength;
			int year = fs.ReadByte () + 2000;
			int month = fs.ReadByte ();
			int day = fs.ReadByte ();
			DateTime romDate = new DateTime (year, month, day);
			Console.WriteLine ("RomDate: {0}", romDate.ToString("yyyy-MM-dd"));

			byte[] searchBytes = new byte[] { 0xA2, 0x10, 0x14 };
			fs.Position = 0;
			var ssmIDpos = Util.SearchBinary.FindBytes (fs, searchBytes);
			Console.WriteLine ("Diesel SSMID pos: 0x{0:X}", ssmIDpos);

			const int RomIDLength = 5;
			byte[] romid = new byte[RomIDLength];
			if (ssmIDpos.HasValue) {
				fs.Position = ssmIDpos.Value + 3;
				if (fs.Read (romid, 0, RomIDLength) == RomIDLength)
					Console.WriteLine ("ROMID: {0}", romid.ToString());

				const string DieselASCII = "2.0 DIESEL";
				fs.Position = 0;
				var stringPos = Util.SearchBinary.FindASCII (fs, DieselASCII);
				Console.WriteLine ("{0} pos: 0x{1:X}", DieselASCII, stringPos);
			}


		}

		#region IDisposable implementation

		void IDisposable.Dispose ()
		{
			Close ();
		}

		#endregion

		/// <summary>
		/// Currently only looks at size.
		/// </summary>
		/// <param name="stream">
		/// A <see cref="Stream"/>
		/// </param>
		/// <returns>
		/// A <see cref="RomType"/>
		/// </returns>
		public RomType DetectRomType (Stream stream)
		{
			const int KiB = 1024;

			try {
				int length = (int)stream.Length;
				switch (length) {
				case 512 * KiB:
					return RomType.SH7055;
				case 1024 * KiB:
					return RomType.SH7058;
				case (1024 + 512) * KiB:
					return RomType.SH7059;
				default:
					return RomType.Unknown;
				}
			} catch (NotSupportedException) {
				return RomType.Unknown;
			}
		}
	}
}
