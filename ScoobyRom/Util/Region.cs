using System;
using System.ComponentModel;

namespace Util
{
	public sealed class Region : IEquatable<Region>
	{
		int pos1, pos2;
		RegionType regionType;

		public RegionType RegionType {
			get { return this.regionType; }
			set { regionType = value; }
		}

		public int Pos1 {
			get { return this.pos1; }
			set {
				if (pos1 != value) {
					pos1 = value;
				}
			}
		}

		public int Pos2 {
			get { return this.pos2; }
			set {
				if (pos2 != value) {
					pos2 = value;
				}
			}
		}

		public int Size {
			get { return pos2 - pos1 + 1; }
		}

		public bool Contains (int pos)
		{
			Sort ();
			return pos1 <= pos && pos <= pos2;
		}

		public Region ()
		{
		}

		public Region (int pos1, int pos2, RegionType regionType)
		{
			this.pos1 = pos1;
			this.pos2 = pos2;
			this.regionType = regionType;
		}

		public void Reset ()
		{
			pos1 = pos2 = 0;
		}

		/// <summary>
		/// Deep copy except event.
		/// </summary>
		public Region Copy ()
		{
			Sort ();
			Region copy = new Region ();
			copy.pos1 = pos1;
			copy.pos2 = pos2;
			return copy;
		}

		public override string ToString ()
		{
			return string.Format ("[Range: Pos1={0}, Pos2={1}, Size={2}]", Pos1, Pos2, Size);
		}

		public override int GetHashCode ()
		{
			return this.pos1 ^ (this.pos2 << 3);
		}

		public void Sort ()
		{
			int p1 = Math.Min (pos1, pos2);
			int p2 = Math.Max (pos1, pos2);
			Pos1 = p1;
			Pos2 = p2;
		}

		#region IEquatable<Region> implementation

		public bool Equals (Region other)
		{
			return this.pos1 == other.pos1 && this.pos2 == other.pos2;
		}

		#endregion
	}
}
