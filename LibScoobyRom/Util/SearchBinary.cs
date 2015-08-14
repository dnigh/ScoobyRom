// SearchBinary.cs: Search for bytes, strings etc.

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
using System.IO;

namespace Util
{
	public static class SearchBinary
	{

		public static int? FindBytes (Stream stream, byte[] target)
		{
			int firstByteTarget = target [0];
			int currentByte;
			bool match;
			while ((currentByte = stream.ReadByte ()) >= 0) {
				if (currentByte == firstByteTarget) {
					match = true;
					for (int i = 1; i < target.Length; i++) {
						if (stream.ReadByte () != target [i]) {
							match = false;
							stream.Seek (-i, SeekOrigin.Current);
							break;
						}
					}
					if (match)
						return (int)(stream.Position) - target.Length;
				}
			}
			return null;
		}

		public static int? FindASCII (Stream stream, string target)
		{
			return FindBytes (stream, System.Text.Encoding.ASCII.GetBytes (target));
		}

	}
}