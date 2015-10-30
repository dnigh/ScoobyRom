using System;
using Cairo;
using Gtk;
using System.Collections.Generic;

namespace GtkWidgets
{
	// .Sensitive works out of the box - does not accept e.g. ButtonPress event
	[System.ComponentModel.ToolboxItem(true)]
	public sealed class NavBarWidget : Gtk.DrawingArea
	{
		//public event EventHandler<EventArgs> Changed;

		const double LineWidth = 1;
		int minWidth = 50;
		const int minRectHeight = 16;
		const int padLeft = 10;
		const int padRight = padLeft;
		const int padTop = 4;
		const int padBottom = padTop;
		const int minHeight = padTop + minRectHeight + padBottom;

		// necessary for getting overridden OnXXX methods called
		//const int EventsUsed = (int)Gdk.EventMask.ButtonPressMask;

		int width, height, backWidth, backHeight;
		Cairo.Rectangle totalRect;
		Gdk.Rectangle clipping_area;
		int firstPos = 0, lastPos = 0;
		int currentPos;
		IList<Util.Region> regions;
		int[] markedPositions;
		double posFactor;
		Cairo.Color ColorBack = new Cairo.Color (1, 1, 1);
		Cairo.Color ColorFrame = new Cairo.Color (0, 0, 0);
		Cairo.Color ColorCurrentPos = new Cairo.Color (1, 0, 0, 0.9);
		Cairo.Color ColorMarkedPos = new Cairo.Color (0, 0.6, 0, 0.7);
		//Cairo.Color ColorMarker1 = new Cairo.Color (0, 0, 1);
		//Cairo.Color ColorMarker2 = new Cairo.Color (0, 0.5, 0);

		public NavBarWidget ()
		{
			Clear ();
			KeyPressEvent += OnKeyPressEvent;
			//this.CanFocus = true;
			//this.AddEvents (EventsUsed);
		}

		#region public properties and methods

		public void Clear ()
		{
			firstPos = 0;
			lastPos = 0;
			regions = null;
			markedPositions = null;
			QueueDraw ();
		}

		public void ZoomIn ()
		{
			this.minWidth = 2 * width;
			QueueResize ();
		}

		public void ZoomOut ()
		{
			this.minWidth = width / 2;
			QueueResize ();
		}

		public void ZoomReset ()
		{
			this.minWidth = 50;
			QueueResize ();
		}

		public int FirstPos {
			get { return firstPos; }
			set {
				if (firstPos == value)
					return;
				firstPos = value;
				UpdatePosRelated ();
			}
		}

		/// <summary>
		/// Gets or sets the last position.
		/// </summary>
		/// <value>
		/// The last pos < 1 means no data.
		/// </value>
		public int LastPos {
			get { return lastPos; }
			set {
				if (lastPos == value)
					return;
				lastPos = value;
				UpdatePosRelated ();
			}
		}

		public int CurrentPos {
			get { return currentPos; }
			set {
				if (currentPos == value)
					return;
				currentPos = value;
				QueueDraw ();
			}
		}

		public void SetRegions (IList<Util.Region> regions)
		{
			/*
			var r = new List<Util.Region> ();
			r.Add (regions [regions.Count - 300]);
			r.Add (regions [regions.Count - 200]);
			r.Add (regions [regions.Count - 1]);
			
			this.regions = r;
			*/
			this.regions = regions;
			QueueDraw ();
		}

		public void ClearMarkedPositions ()
		{
			markedPositions = null;
			QueueDraw ();
		}

		public void SetMarkedPositions (int[] positions)
		{
			markedPositions = positions != null && positions.Length == 0 ? null : positions;
			QueueDraw ();
		}

		public int[] GetMarkedPositions ()
		{
			return markedPositions;
		}

		int PosSize {
			get { return lastPos - firstPos + 1; }
		}

		bool NoData {
			get { return PosSize <= 0; }
		}

		bool RegionsToDisplay {
			get { return regions != null && regions.Count > 0; }
		}

		bool MarkedPositionsToDisplay {
			get { return markedPositions != null && markedPositions.Length > 0; }
		}

		// !!! Stetic designer seems to take every public setter,
		// generating code to set default value (0) in Build ().


		#endregion public properties and methods

		#region mono x64 optimizations

		// mono x64 asm tested: (ref Struct) to avoid stack activity at callsite

		static void SetColor (Cairo.Context cr, ref Cairo.Color color)
		{
			// Cairo.Context.Color = value calls SetSourceRGBA (,,,) internally
			cr.SetSourceRGBA (color.R, color.G, color.B, color.A);
		}

		static void Rectangle (Cairo.Context cr, ref Rectangle rect)
		{
			cr.Rectangle (rect.X, rect.Y, rect.Width, rect.Height);
		}

		#endregion

		#region events

		protected override bool OnExposeEvent (Gdk.EventExpose ev)
		{
			base.OnExposeEvent (ev);
			// Insert drawing code here.
			using (Cairo.Context cr = Gdk.CairoHelper.Create (ev.Window)) {
				DrawEverything (cr);
			}
			return true;
		}

		protected override void OnSizeAllocated (Gdk.Rectangle allocation)
		{
			width = allocation.Width;
			height = allocation.Height;
			Console.WriteLine ("OnSizeAllocated: {0}x{1}", width, height);

			backWidth = width - padLeft - padRight;
			backHeight = height - padTop - padBottom;

			UpdatePosRelated ();

			totalRect = new Cairo.Rectangle (PosXLeft (0), padTop, backWidth, backHeight);
			clipping_area = new Gdk.Rectangle (0, 0, width, height);

			base.OnSizeAllocated (allocation);
			// Insert layout code here.
		}

		protected override void OnSizeRequested (ref Gtk.Requisition requisition)
		{
			// Calculate desired size here.
			requisition.Width = minWidth;
			requisition.Height = minHeight;
			Console.WriteLine ("OnSizeRequested -> Requisition: {0}x{1}", requisition.Width, requisition.Height);
		}

		void OnKeyPressEvent (object o, KeyPressEventArgs args)
		{
			const Gdk.ModifierType modifier = Gdk.ModifierType.Button1Mask;
			Gdk.Key key = args.Event.Key;

			if ((args.Event.State & modifier) != 0) {
				if (key == Gdk.Key.Key_0 || key == Gdk.Key.KP_0) {
					ZoomReset ();
					return;
				} else if (key == Gdk.Key.plus || key == Gdk.Key.KP_Add) {
					ZoomIn ();
					return;
				} else if (key == Gdk.Key.minus || key == Gdk.Key.KP_Subtract) {
					ZoomOut ();
					return;
				}
			}
		}

		#endregion events

		void UpdatePosRelated ()
		{
			if (lastPos == 0)
				return;
			posFactor = (double)backWidth / (double)lastPos;
		}

		void DrawEverything (Cairo.Context cr)
		{
			// using combination of Gtk.Style.Paint... and Cairo commands

			cr.LineCap = LineCap.Butt;
			cr.LineJoin = LineJoin.Miter;
			cr.LineWidth = LineWidth;
			// Cairo: black is default, like cr.SetSourceRGB (0, 0, 0);
			DrawBack (cr);

			if (NoData)
				return;

			if (RegionsToDisplay) {
				foreach (var r in this.regions) {
					Cairo.Color color = Util.Coloring.RegionColor (r.RegionType);
					DrawRegion (cr, ref color, r.Pos1, r.Pos2);
					//DrawRangeMarker (cr, ref ColorMarker2, index2, ArrowType.Left);
					//DrawRangeMarker (pos2, ArrowType.Left);

					//DrawRangeMarker (cr, ref ColorMarker1, index1, ArrowType.Right);
					//DrawRangeMarker (pos1, ArrowType.Right);
				}
			}

			cr.LineWidth = LineWidth;
			if (MarkedPositionsToDisplay) {
				SetColor (cr, ref ColorMarkedPos);
				if (markedPositions.Length == PosSize) {
					// all positions are marked
					cr.Rectangle (padLeft, 0, totalRect.Width, height);
					cr.Fill ();
				} else {
					foreach (int i in markedPositions) {
						cr.MoveTo (PosXLeft (i), 0);
						cr.RelLineTo (0, height);
					}
					// single call is much faster
					cr.Stroke ();
				}
			}

			DrawMarker (cr, ref ColorCurrentPos, currentPos);
			//DrawMarker (sampleIndex);
		}

		/*
		void DrawBack ()
		{
			// PaintFlatBox: null → nothing; "tooltip" → frame, "button" → solid
			//Gtk.Style.PaintFlatBox (this.Style, this.GdkWindow, StateType.Normal, ShadowType.None, clipping_area, this, "tooltip",
			//                    padLeft, padTop, backWidth, backHeight);

			// PaintBox: null → frame
			Gtk.Style.PaintBox (this.Style, this.GdkWindow, StateType.Normal, ShadowType.None, clipping_area, this, null,
			                    padLeft, padTop, backWidth, backHeight);
		}
		*/

		void DrawBack (Cairo.Context cr)
		{
			Rectangle (cr, ref totalRect);
			if (!NoData && Sensitive) {
				SetColor (cr, ref ColorBack);
				cr.FillPreserve ();
			}
			SetColor (cr, ref ColorFrame);
			cr.Stroke ();
		}

		void DrawMarker (Cairo.Context cr, ref Cairo.Color color, int sampleIndex)
		{
			SetColor (cr, ref color);
			//cr.Rectangle (PosXLeft (sampleIndex), 0, LineWidth, height);
			//cr.Fill ();

			cr.MoveTo (PosXLeft (sampleIndex), 0);
			cr.RelLineTo (0, height);
			cr.Stroke ();
		}

		/*
		void DrawMarker (int sampleIndex)
		{
			int x = Convert.ToInt32 (PosXLeft (sampleIndex));
			int height = Convert.ToInt32 (totalRect.Height);
			int y = Convert.ToInt32 (totalRect.Y);

			StateType state;
			if (this.HasFocus)
				state = StateType.Active;
			else
				state = StateType.Normal;
			Gtk.Style.PaintVline (this.Style, this.GdkWindow, state, clipping_area, this, null,
				y, y + height, x);

			// "grip", "paned"
			//Gtk.Style.PaintHandle (this.Style, this.GdkWindow, state, ShadowType.None, clipping_area, this, "paned",
			//	x, y, 3, height, Orientation.Vertical);
		}
		*/

		void DrawRangeMarker (int sampleIndex, ArrowType arrowType)
		{
			int x = Convert.ToInt32 (PosXLeft (sampleIndex));
			int height = Convert.ToInt32 (totalRect.Height);
			int y = Convert.ToInt32 (totalRect.Y);
			int dx = height / 2 + 2;

			StateType state;
			if (this.HasFocus)
				state = StateType.Normal;
			else
				state = StateType.Normal;
			//Gtk.Style.PaintDiamond (this.Style, this.GdkWindow, state, ShadowType.None, clipping_area, this, "diamond",
			//                    (int)x, (int)y, (int)height, (int)totalRect.Height);

			if (arrowType == ArrowType.Right)
				x -= dx;
			Gtk.Style.PaintArrow (this.Style, this.GdkWindow, state, ShadowType.Out, clipping_area, this, "",
				arrowType, false,
				x, y, dx, height);
		}

		/*
		void DrawRangeMarker (Cairo.Context cr, ref Cairo.Color color, int sampleIndex, ArrowType arrowType)
		{
			double x = PosXLeft (sampleIndex);
			double height = totalRect.Height;
			double y = totalRect.Y;
			double dx = 0.25 * height;

			if (arrowType == ArrowType.Left)
				dx = -dx;
			SetColor (cr, ref color);
			cr.MoveTo (x + dx, y);
			cr.LineTo (x, y + 0.5 * height);
			cr.LineTo (x + dx, y + height);
			cr.Stroke ();
		}
		*/

		/*
		void DrawRange (int index1, int index2)
		{
			int x = Convert.ToInt32 (PosXLeft (index1));
			int right = Convert.ToInt32 (PosXLeft (index2));
			int y = Convert.ToInt32 (totalRect.Y);
			int width = Math.Max (1, right - x);
			int height = Convert.ToInt32 (totalRect.Height);

			StateType state;
			if (this.HasFocus)
				state = StateType.Selected;
			else
				state = StateType.Insensitive;

			// styles: "bar" → Progressbar fill pattern; "button" → rounded rect; "tooltip" → filled solid; null → frame only
			Gtk.Style.PaintBox (this.Style, this.GdkWindow, state, ShadowType.None, clipping_area, this, "bar",
				x, y, width, height);
		}
		*/

		void DrawRegion (Cairo.Context cr, ref Cairo.Color color, int pos1, int pos2)
		{
			const double LineWidthRegion = 0;

			double left = PosXLeft (pos1);
			double right = PosXLeft (pos2);
			double width = Math.Max (LineWidthRegion, right - left);

			cr.LineWidth = LineWidthRegion;
			SetColor (cr, ref color);
			cr.Rectangle (left, totalRect.Y + LineWidth, width, totalRect.Height - (2 * LineWidth));
			cr.Fill ();
		}

		double PosXLeft (int pos)
		{
			return padLeft + posFactor * pos;
		}
	}
}
