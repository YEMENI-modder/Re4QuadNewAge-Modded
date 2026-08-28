using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using System.Runtime.InteropServices;

namespace NsMultiselectTreeView // from https://github.com/DavidSM64/Quad64/blob/master/src/Forms/MultiselectTree/MultiselectTreeView.cs
								// from original source https://www.codeproject.com/Articles/20581/Multiselect-Treeview-Implementation
{
	public class MultiselectTreeView : TreeView
	{
		private const int WM_ERASEBKGND = 0x0014;
		private const int WM_NCCALCSIZE = 0x0083;
		private const int WM_NCPAINT = 0x0085;
		private const int WM_NCACTIVATE = 0x0086;
		private const uint WS_VSCROLL = 0x00200000;
		private const int GWL_STYLE = -16;
		private const int SM_CXVSCROLL = 2;
		private const uint SWP_NOSIZE = 0x0001;
		private const uint SWP_NOMOVE = 0x0002;
		private const uint SWP_NOZORDER = 0x0004;
		private const uint SWP_NOACTIVATE = 0x0010;
		private const uint SWP_FRAMECHANGED = 0x0020;

		/// <summary>
		/// From-scratch dark scrollbar. The native vertical scrollbar is made
		/// INVISIBLE at the source: WM_NCCALCSIZE returns its reserved strip to
		/// the client rectangle, so Windows has NO non-client region to draw any
		/// classic or themed bar into - not even the message-bypassing draws that
		/// happen inside SetScrollInfo. All pixels live in DarkScrollBarOverlay,
		/// a regular child control that drives scrolling through WM_VSCROLL.
		/// </summary>
		private bool darkScrollBar;
		public bool DarkScrollBar
		{
			get { return darkScrollBar; }
			set
			{
				if (darkScrollBar == value) return;
				darkScrollBar = value;

				if (!darkScrollBar)
				{
					DestroyDarkBarOverlay();
				}
				else if (IsHandleCreated)
				{
					EnsureDarkBarOverlay();
					try { SetWindowPos(Handle, IntPtr.Zero, 0, 0, 0, 0,
						SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_NOACTIVATE | SWP_FRAMECHANGED); } catch { }
				}
			}
		}

		private DarkScrollBarOverlay darkBarOverlay;

		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);
			if (darkScrollBar)
			{
				EnsureDarkBarOverlay();
				try { SetWindowPos(Handle, IntPtr.Zero, 0, 0, 0, 0,
					SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_NOACTIVATE | SWP_FRAMECHANGED); } catch { }
			}
		}

		protected override void OnParentChanged(EventArgs e)
		{
			base.OnParentChanged(e);
			if (darkScrollBar) ReparentOverlay();
		}

		protected override void OnSizeChanged(EventArgs e)
		{
			base.OnSizeChanged(e);
			if (darkBarOverlay != null) darkBarOverlay.SyncFromTree();
		}

		// Empty theme kills comctl double buffering -> fast scrolling tears.
		// Re-enable it explicitly via the extended-style message.
		private const int TVM_SETEXTENDEDSTYLE = 0x112C;
		private const int TVS_EX_DOUBLEBUFFER = 0x0004;
		[DllImport("user32.dll")]
		private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

		private void EnsureDarkBarOverlay()
		{
			if (!darkScrollBar || !IsHandleCreated) return;
			SendMessage(Handle, TVM_SETEXTENDEDSTYLE, (IntPtr)TVS_EX_DOUBLEBUFFER, (IntPtr)TVS_EX_DOUBLEBUFFER);

			if (darkBarOverlay == null || darkBarOverlay.IsDisposed)
				darkBarOverlay = new DarkScrollBarOverlay(this);

			ReparentOverlay();
		}

		private void ReparentOverlay()
		{
			if (darkBarOverlay == null || darkBarOverlay.IsDisposed) return;

			Control host = Parent;
			if (host == null)
			{
				darkBarOverlay.Visible = false; // wait until the tree is parented
				return;
			}

			if (darkBarOverlay.Parent != host)
				darkBarOverlay.Parent = host;
			darkBarOverlay.BringToFront();
			darkBarOverlay.SyncFromTree();
		}

		private void DestroyDarkBarOverlay()
		{
			if (darkBarOverlay != null)
			{
				darkBarOverlay.DestroySelf();
				darkBarOverlay = null;
			}
			if (IsHandleCreated)
			{
				try { SetWindowPos(Handle, IntPtr.Zero, 0, 0, 0, 0,
					SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_NOACTIVATE | SWP_FRAMECHANGED); } catch { }
			}
		}

		protected override void WndProc(ref Message msg)
		{
			if (msg.Msg == WM_ERASEBKGND)
			{
				return;
			}

			if (darkScrollBar && (msg.Msg == WM_NCPAINT || msg.Msg == WM_NCACTIVATE))
			{
				// No non-client area exists while the owned bar is active.
				return;
			}

			if (darkScrollBar && msg.Msg == WM_NCCALCSIZE && msg.WParam != IntPtr.Zero &&
				IsHandleCreated &&
				(unchecked((uint)GetWindowLongW(Handle, GWL_STYLE)) & WS_VSCROLL) != 0)
			{
				base.WndProc(ref msg);
				try
				{
					// Give the reserved scrollbar strip back to the client
					// rectangle. With no strip left, Windows cannot paint ANY
					// scrollbar there - classic, themed, direct-drawn: nothing.
					var rc = (RECT)Marshal.PtrToStructure(msg.LParam, typeof(RECT));
					rc.Right += Math.Max(10, GetSystemMetrics(SM_CXVSCROLL));
					Marshal.StructureToPtr(rc, msg.LParam, false);
				}
				catch { }
				return;
			}

			base.WndProc(ref msg);
		}

		/// <summary>
		/// Fully custom dark scrollbar drawn as a normal child control. Because
		/// it is a real control, nothing native can paint over it, and because
		/// the tree's native bar strip was removed via WM_NCCALCSIZE, there is
		/// no second scrollbar anywhere. Dragging sends WM_VSCROLL messages.
		/// </summary>
		private sealed class DarkScrollBarOverlay : Control
		{
			private const int SB_LINEUP = 0;
			private const int SB_LINEDOWN = 1;
			private const int SB_VERT = 1;
			private const int SB_THUMBPOSITION = 4;
			// TreeView scrolls live on THUMBTRACK but often ignores THUMBPOSITION.
			private const int SB_THUMBTRACK = 5;
			private const int SB_ENDSCROLL = 8;
			private const int SB_PAGEUP = 2;
			private const int SB_PAGEDOWN = 3;
			private const int WM_MOUSEWHEEL = 0x020A;
			private const int WM_VSCROLL = 0x0115;
			private const uint SIF_ALL = 0x17;
			private const int SM_CYVSCROLL = 20;

			private readonly MultiselectTreeView owner;
			private readonly System.Windows.Forms.Timer syncTimer;
			private System.Windows.Forms.Timer pageTimer;
			private bool pageDownDirection;
			private int pageRepeatCount;
			private bool draggingThumb;
			private int dragTargetPos;
			private int dragOffset;
			private bool hoverTrack;

			public DarkScrollBarOverlay(MultiselectTreeView ownerTree)
			{
				owner = ownerTree;
				SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
						 ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.Opaque, true);
				// Non-selectable so focus/keys stay with the tree; painting styles above must be true.
				SetStyle(ControlStyles.Selectable, false);
				Width = Math.Max(12, GetSystemMetrics(SM_CXVSCROLL));
				Cursor = Cursors.Arrow;
				TabStop = false;

				syncTimer = new System.Windows.Forms.Timer { Interval = 33 };
				syncTimer.Tick += delegate { SyncFromTree(); };
				syncTimer.Start();
			}

			public void DestroySelf()
			{
				syncTimer.Stop();
				syncTimer.Dispose();
				StopPageTimer();
				if (Parent != null) Parent.Controls.Remove(this);
				Dispose();
			}

			protected override void Dispose(bool disposing)
			{
				if (disposing)
				{
					StopPageTimer();
					if (syncTimer != null) { syncTimer.Stop(); syncTimer.Dispose(); }
				}
				base.Dispose(disposing);
			}

			protected override void WndProc(ref Message m)
			{
				// Wheel over the bar keeps scrolling the tree seamlessly.
				if (m.Msg == WM_MOUSEWHEEL && owner != null && owner.IsHandleCreated)
				{
					SendMessage(owner.Handle, m.Msg, m.WParam, m.LParam);
					m.Result = IntPtr.Zero;
					SyncFromTree();
					return;
				}
				base.WndProc(ref m);
			}

			public void SyncFromTree()
			{
				try
				{
					if (owner == null || owner.IsDisposed || !owner.IsHandleCreated || owner.Parent == null || Disposing || IsDisposed)
						return;

					if (owner.ClientSize.Width <= 0 || owner.ClientSize.Height <= 0)
						return;

					int w = Math.Max(12, GetSystemMetrics(SM_CXVSCROLL));
					if (Width != w) Width = w;

					Point ownerTL = owner.PointToScreen(Point.Empty);
					Point mine = Parent.PointToClient(ownerTL);
					Location = new Point(mine.X + owner.ClientSize.Width - w, mine.Y);
					Size = new Size(w, Math.Max(0, owner.ClientSize.Height));

					SI si;
					bool needed = ReadScroll(out si) && BarNeeded(si);
					bool shouldBeVisible = needed && owner.Visible;
					if (Visible != shouldBeVisible)
						Visible = shouldBeVisible;
					else if (Visible)
						Invalidate();
				if (draggingThumb) DriveTowardTarget();
				}
				catch { }
			}

			// Dark blue tones matched to the graphite-blue shell so the bar
			// blends with the theme yet stays clearly visible.
			private static readonly Color ThumbIdle = Color.FromArgb(58, 92, 138);
			private static readonly Color ThumbHover = Color.FromArgb(69, 119, 181);
			private static readonly Color ThumbDrag = Color.FromArgb(95, 153, 224);

			protected override void OnPaint(PaintEventArgs e)
			{
				// Track + full outline so the bar reads as a distinct element.
				using (Brush b = new SolidBrush(Re4QuadExtremeEditor.DarkTheme.Surface2))
					e.Graphics.FillRectangle(b, ClientRectangle);
				using (Pen p = new Pen(Re4QuadExtremeEditor.DarkTheme.Border))
					e.Graphics.DrawRectangle(p, 0, 0, Width - 1, Height - 1);

				SI si;
				if (!ReadScroll(out si)) return;

				bool enabled;
				Rectangle thumbRect = LayoutThumb(ClientSize.Height, si, out enabled);
				if (!enabled) return;

				Color thumbColor = draggingThumb ? ThumbDrag
					: hoverTrack ? ThumbHover
					: ThumbIdle;

				using (Brush b = new SolidBrush(thumbColor))
					e.Graphics.FillRectangle(b, thumbRect);
				using (Pen p = new Pen(Re4QuadExtremeEditor.DarkTheme.Border))
					e.Graphics.DrawRectangle(p, thumbRect);
			}

			protected override void OnMouseDown(MouseEventArgs e)
			{
				base.OnMouseDown(e);
				if (e.Button != MouseButtons.Left) return;

				SI si;
				if (!ReadScroll(out si)) return;

				bool enabled;
				Rectangle thumbRect = LayoutThumb(ClientSize.Height, si, out enabled);
				if (!enabled) return;

				FocusOwner();

				if (thumbRect.Contains(e.Location))
				{
					draggingThumb = true;

					dragOffset = e.Y - thumbRect.Top;
				dragTargetPos = si.pos;
					Capture = true;
				}
				else
				{
					pageDownDirection = e.Y > thumbRect.Bottom;
					pageRepeatCount = 0;
					SendPage();
					StartPageTimer();
				}
				Invalidate();
			}

			protected override void OnMouseMove(MouseEventArgs e)
			{
				base.OnMouseMove(e);
				hoverTrack = ClientRectangle.Contains(e.Location);

				if (draggingThumb)
				{
					SI si;
					if (ReadScroll(out si))
					{
						bool enabled;
						Rectangle thumbRect = LayoutThumb(ClientSize.Height, si, out enabled);
						int usable = ClientSize.Height - thumbRect.Height;
						if (usable > 0)
						{
							int y = e.Y - dragOffset;
							float ratio = (float)y / usable;
							int range = Math.Max(1, si.max - si.min + 1);
							int maxPos = Math.Max(1, range - si.page);
							int pos = (int)Math.Round(ratio * maxPos);
							pos = Math.Max(0, Math.Min(maxPos, pos)) + si.min;
							dragTargetPos = pos;
						}
					}
				}
				Invalidate();
			}

			protected override void OnMouseLeave(EventArgs e)
			{
				base.OnMouseLeave(e);
				hoverTrack = false;
				Invalidate();
			}

			protected override void OnMouseUp(MouseEventArgs e)
			{
				base.OnMouseUp(e);
				if (draggingThumb)
				{
					draggingThumb = false;
					Capture = false;
					SendScroll(SB_ENDSCROLL, 0);
				}
				StopPageTimer();
				Invalidate();
			}

			private void FocusOwner()
			{
				try { if (owner != null && owner.IsHandleCreated) owner.Focus(); } catch { }
			}

			private struct SI { public int min, max, page, pos; }

			private bool ReadScroll(out SI result)
			{
				result = new SI();
				try
				{
					if (owner == null || !owner.IsHandleCreated) return false;
					SCROLLINFO si = new SCROLLINFO();
					si.cbSize = (uint)Marshal.SizeOf(typeof(SCROLLINFO));
					si.fMask = SIF_ALL;
					if (!GetScrollInfo(owner.Handle, SB_VERT, ref si)) return false;
					result.min = si.nMin;
					result.max = si.nMax;
					result.page = si.nPage > int.MaxValue ? int.MaxValue : (int)si.nPage;
					result.pos = si.nPos;
					return true;
				}
				catch { return false; }
			}

			private static bool BarNeeded(SI si)
			{
				return si.page > 0 ? (si.max - si.min + 1) > si.page : (si.max > si.min);
			}

			private Rectangle LayoutThumb(int height, SI si, out bool enabled)
			{
				enabled = BarNeeded(si);
				int h = Math.Max(1, height);
				int thumbMin = Math.Max(18, GetSystemMetrics(SM_CYVSCROLL));

				if (!enabled) return Rectangle.Empty;

				int range = Math.Max(1, si.max - si.min + 1);
				int page = Math.Max(0, si.page);
				int thumb = page > 0
					? Math.Max(thumbMin, (int)((long)h * page / Math.Max(range, page + 1)))
					: thumbMin;
				thumb = Math.Min(h, thumb);

				int maxPos = Math.Max(1, range - page);
				int trackRange = Math.Max(1, h - thumb);
				int pos = Math.Max(0, Math.Min(maxPos, si.pos - si.min));
				int y = (int)((long)trackRange * pos / maxPos);

				int w = ClientSize.Width;
				return new Rectangle(Math.Max(1, w / 8), y + 2,
					Math.Max(4, w - 2 * Math.Max(1, w / 8)), Math.Max(12, thumb - 4));
			}

			private void DriveTowardTarget()
			{
				try
				{
						SI si;
						if (!ReadScroll(out si)) return;
						int diff = dragTargetPos - si.pos;
						if (diff == 0) return;
						// Forged THUMB positions are rejected by this control while native
						// LINE/PAGE commands are always honored: stream them closed-loop.
						int pageStep = Math.Max(1, si.page - 1);
						int reps;
						if (diff > 0)
						{
							reps = diff >= pageStep ? Math.Min(2, diff / pageStep) : Math.Min(4, diff);
							for (int i = 0; i < reps; i++) SendScroll(diff >= pageStep ? SB_PAGEDOWN : SB_LINEDOWN, 0);
						}
						else
						{
							int mag = -diff;
							reps = mag >= pageStep ? Math.Min(2, mag / pageStep) : Math.Min(4, mag);
							for (int i = 0; i < reps; i++) SendScroll(mag >= pageStep ? SB_PAGEUP : SB_LINEUP, 0);
						}
				}
				catch { }
			}

			private void SendScroll(int command, int pos)
			{
				try
				{
					if (owner == null || !owner.IsHandleCreated) return;
					SendMessage(owner.Handle, WM_VSCROLL, (IntPtr)((pos << 16) | command), IntPtr.Zero);
					SyncFromTreeSoon();
				}
				catch { }
			}

			private void SyncFromTreeSoon()
			{
				try { Invalidate(); } catch { }
			}

			private void SendPage()
			{
				SendScroll(pageDownDirection ? SB_PAGEDOWN : SB_PAGEUP, 0);
			}

			private void StartPageTimer()
			{
				if (pageTimer == null)
				{
					pageTimer = new System.Windows.Forms.Timer { Interval = 250 };
					pageTimer.Tick += delegate
					{
						pageRepeatCount++;
						if (pageRepeatCount == 2) pageTimer.Interval = 60;
						SendPage();
					};
				}
				pageRepeatCount = 0;
				pageTimer.Interval = 250;
				pageTimer.Start();
			}

			private void StopPageTimer()
			{
				if (pageTimer != null) pageTimer.Stop();
			}

			[DllImport("user32.dll")]
			private static extern bool PostMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);
			[DllImport("user32.dll")]
			private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);
			[DllImport("user32.dll")] private static extern int GetSystemMetrics(int nIndex);
			[DllImport("user32.dll")] private static extern bool GetScrollInfo(IntPtr hWnd, int nBar, ref SCROLLINFO lpsi);
			[DllImport("user32.dll", EntryPoint = "GetWindowLongW", SetLastError = true)]
			private static extern int GetWindowLongW(IntPtr hWnd, int nIndex);

			[StructLayout(LayoutKind.Sequential)]
			private struct SCROLLINFO
			{
				public uint cbSize;
				public uint fMask;
				public int nMin;
				public int nMax;
				public uint nPage;
				public int nPos;
				public int nTrackPos;
			}
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct RECT { public int Left, Top, Right, Bottom; }

		[DllImport("user32.dll")] private static extern int GetSystemMetrics(int nIndex);
		[DllImport("user32.dll", EntryPoint = "GetWindowLongW", SetLastError = true)]
		private static extern int GetWindowLongW(IntPtr hWnd, int nIndex);
		[DllImport("user32.dll")] private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndAfter, int X, int Y, int cx, int cy, uint flags);
		#region Selected Node(s) Properties


		private Color selectedNodeBackColor = SystemColors.Highlight;
		private bool useThemedSelectedNodeBackColor = false;

		public Color SelectedNodeBackColor { get => selectedNodeBackColor; set => selectedNodeBackColor = value; }
		public bool UseThemedSelectedNodeBackColor { get => useThemedSelectedNodeBackColor; set => useThemedSelectedNodeBackColor = value; }

		private System.Collections.Generic.Dictionary<int, TreeNode> m_SelectedNodes = null;
		/// <summary>
		/// hashCode, TreeNode
		/// </summary>
		public Dictionary<int, TreeNode> SelectedNodes
		{
			get
			{
				return m_SelectedNodes;
			}
			set
			{
				if (value != null)
				{
					m_SelectedNodes.Clear();
                    foreach (var item in value)
                    {
						m_SelectedNodes.Add(item.Key, item.Value);
					}
					m_SelectedNode = null;
					if (m_SelectedNodes.Count != 0)
                    {
						m_SelectedNode = m_SelectedNodes.Last().Value;
					}
					OnAfterSelect(new TreeViewEventArgs(m_SelectedNode));
				}
				else
				{
					m_SelectedNodes.Clear();
					m_SelectedNode = null;
					OnAfterSelect(new TreeViewEventArgs(null));
				}
			}
		}

		// Note we use the new keyword to Hide the native treeview's SelectedNode property.
		private TreeNode m_SelectedNode;
		public new TreeNode SelectedNode
		{
			get { return m_SelectedNode; }
			set
			{
				ClearSelectedNodes();
				if (value != null)
				{
					SelectNode(value);
					OnAfterSelect(new TreeViewEventArgs(m_SelectedNode));
				}
				else
				{
					m_SelectedNode = null;
					m_SelectedNodes.Clear();
					OnAfterSelect(new TreeViewEventArgs(null));
				}
			}
		}

		public void SelectedNodesClearNoRedraw()
		{
			ClearSelectedNodes();
		}

		#endregion

		public MultiselectTreeView()
		{
			m_SelectedNodes = new Dictionary<int, TreeNode>();
			m_SelectedNode = null;
			base.SelectedNode = null;
			DrawMode = TreeViewDrawMode.OwnerDrawText;
		}

		#region Overridden Events

		protected override void OnGotFocus( EventArgs e )
		{
			// Make sure at least one node has a selection
			// this way we can tab to the ctrl and use the 
			// keyboard to select nodes
			try
			{
				if( m_SelectedNode == null && this.TopNode != null )
				{
					ToggleNode( this.TopNode, true );
				}

				base.OnGotFocus( e );
			}
			catch( Exception ex )
			{
				HandleException( ex );
			}
		}

		protected override void OnMouseDown( MouseEventArgs e )
		{
			// If the user clicks on a node that was not
			// previously selected, select it now.

			try
			{
				// disclosure chevron toggles expand/collapse without selecting
				if (e.Button == MouseButtons.Left)
				{
					TreeNode hitNode = GetNodeAt(e.Location);
					if (hitNode != null && hitNode.Nodes.Count > 0 && DisclosureRect(hitNode).Contains(e.Location))
					{
						if (hitNode.IsExpanded) hitNode.Collapse();
						else hitNode.Expand();
						Invalidate();
						base.OnMouseDown(e);
						return;
					}
				}

				// Shift+Left starts an Explorer-style rubber-band selection
				// instead of the normal click-select behavior.
				if (e.Button == MouseButtons.Left && ModifierKeys == Keys.Shift)
				{
					StartNodeBand(e.Location);
					base.OnMouseDown(e);
					return;
				}

				base.SelectedNode = null;

				TreeNode node = this.GetNodeAt( e.Location );
				if( node != null )
				{
					Font font = this.Font;
					if (node.NodeFont != null)
					{
						font = node.NodeFont;
					}

					string altText = node.Text;
					if (node is IAltNode obj)
					{
						altText = obj.AltText;
					}

					int leftBound = node.Bounds.X; // - 20; // Allow user to click on image
					int rightBound = TextRenderer.MeasureText(altText, font).Width + node.Bounds.X; //node.Bounds.Right + 10; // Give a little extra room
					if ( e.Location.X > leftBound && e.Location.X < rightBound )
					{
						if (ModifierKeys == Keys.None && (m_SelectedNodes.ContainsValue(node)))
						{
							// Potential Drag Operation
							// Let Mouse Up do select
						}
						else
						{
							SelectNode(node);
						}
					}
				}

				base.OnMouseDown( e );
			}
			catch( Exception ex )
			{
				HandleException( ex );
			}
		}

		protected override void OnMouseUp( MouseEventArgs e )
		{
			// If the clicked on a node that WAS previously
			// selected then, reselect it now. This will clear
			// any other selected nodes. e.g. A B C D are selected
			// the user clicks on B, now A C & D are no longer selected.
			try
			{
				if (nodeBandActive && (e.Button & MouseButtons.Left) != 0)
				{
					FinishNodeBand(e.Location);
					base.OnMouseUp(e);
					return;
				}

				// Check to see if a node was clicked on 
				TreeNode node = this.GetNodeAt( e.Location );
				if( node != null )
				{
					if( ModifierKeys == Keys.None && m_SelectedNodes.ContainsValue( node ) && m_SelectedNodes.Count > 1)
					{
						Font font = this.Font;
						if (node.NodeFont != null)
						{
							font = node.NodeFont;
						}

						string altText = node.Text;
						if (node is IAltNode obj)
						{
							altText = obj.AltText;
						}

						int leftBound = node.Bounds.X; // -20; // Allow user to click on image
						int rightBound = TextRenderer.MeasureText(altText, font).Width + node.Bounds.X; //node.Bounds.Right + 10; // Give a little extra room
						if( e.Location.X > leftBound && e.Location.X < rightBound )
						{
							SelectNode( node );
						}
					}
				}

				base.OnMouseUp( e );
			}
			catch( Exception ex )
			{
				HandleException( ex );
			}
		}

		// ---------------- Shift+drag rubber-band selection ----------------

		private bool nodeBandActive = false;
		private Point nodeBandStart;
		private Rectangle nodeBandLastFrame = Rectangle.Empty;
		private Dictionary<int, TreeNode> nodeBandSaved = null;
		private TreeNode nodeBandSavedActive = null;

		protected override void OnMouseMove(MouseEventArgs e)
		{
			UpdateTreeHover(e.Location);

			if (nodeBandActive && (e.Button & MouseButtons.Left) != 0)
			{
				UpdateNodeBand(e.Location);
				base.OnMouseMove(e);
				return;
			}
			base.OnMouseMove(e);
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			if (hoverDisclosureNode != null)
			{
				hoverDisclosureNode = null;
				Invalidate();
			}
			base.OnMouseLeave(e);
		}

		protected override void OnMouseCaptureChanged(EventArgs e)
		{
			if (nodeBandActive && !Capture)
			{
				CancelNodeBand();
			}
			base.OnMouseCaptureChanged(e);
		}

		private void StartNodeBand(Point pt)
		{
			nodeBandActive = true;
			nodeBandStart = pt;
			nodeBandLastFrame = Rectangle.Empty;
			nodeBandSaved = new Dictionary<int, TreeNode>(m_SelectedNodes);
			nodeBandSavedActive = m_SelectedNode;
			Capture = true;
		}

		private void UpdateNodeBand(Point pt)
		{
			EraseNodeBandFrame();
			nodeBandLastFrame = MakeNormalizedRect(nodeBandStart, pt);
			DrawNodeBandFrame(nodeBandLastFrame);

			// live preview: swap the selection dictionary in place (no events),
			// listeners fire once when the band finishes
			m_SelectedNodes.Clear();
			CollectBandLeaves(nodeBandLastFrame, m_SelectedNodes);
			Invalidate();
		}

		private void FinishNodeBand(Point pt)
		{
			nodeBandActive = false;

			Rectangle rect = MakeNormalizedRect(nodeBandStart, pt);
			bool tinyDrag = rect.Width < 4 && rect.Height < 4;

			EraseNodeBandFrame();
			nodeBandLastFrame = Rectangle.Empty;
			if (Capture) Capture = false; // fires OnMouseCaptureChanged, already inactive

			if (tinyDrag)
			{
				// plain shift-click on the same spot: restore what was selected
				RestoreNodeBandSnapshot();
			}
			else
			{
				m_SelectedNodes.Clear();
				CollectBandLeaves(rect, m_SelectedNodes);
			}

			m_SelectedNode = null;
			if (m_SelectedNodes.Count != 0)
			{
				m_SelectedNode = m_SelectedNodes.Last().Value;
			}

			OnAfterSelect(new TreeViewEventArgs(m_SelectedNode));
			Invalidate();
		}

		private void CancelNodeBand()
		{
			nodeBandActive = false;
			EraseNodeBandFrame();
			nodeBandLastFrame = Rectangle.Empty;
			RestoreNodeBandSnapshot();
			OnAfterSelect(new TreeViewEventArgs(m_SelectedNode));
			Invalidate();
		}

		private void RestoreNodeBandSnapshot()
		{
			m_SelectedNodes.Clear();
			if (nodeBandSaved != null)
			{
				foreach (KeyValuePair<int, TreeNode> kv in nodeBandSaved)
				{
					m_SelectedNodes[kv.Key] = kv.Value;
				}
			}
			m_SelectedNode = nodeBandSavedActive;
		}

		private void EraseNodeBandFrame()
		{
			DrawNodeBandFrame(nodeBandLastFrame);
			nodeBandLastFrame = Rectangle.Empty;
		}

		private void DrawNodeBandFrame(Rectangle rect)
		{
			if (rect.Width >= 2 || rect.Height >= 2)
			{
				ControlPaint.DrawReversibleFrame(
					RectangleToScreen(rect), Color.FromArgb(160, 200, 255), FrameStyle.Dashed);
			}
		}

		private static Rectangle MakeNormalizedRect(Point a, Point b)
		{
			return new Rectangle(
				Math.Min(a.X, b.X), Math.Min(a.Y, b.Y),
				Math.Abs(a.X - b.X), Math.Abs(a.Y - b.Y));
		}

		/// <summary>
		/// Fills the target with every VISIBLE leaf row whose bounds intersect
		/// the band rectangle. Row-based like Explorer: only vertical overlap
		/// matters. Collapsed branches are naturally skipped.
		/// </summary>
		private void CollectBandLeaves(Rectangle rect, Dictionary<int, TreeNode> target)
		{
			int bottomLimit = ClientSize.Height + ItemHeight;
			TreeNode cur = TopNode;
			while (cur != null)
			{
				Rectangle b = cur.Bounds;
				if (b.Height > 0 && b.Bottom >= rect.Top && b.Top <= bottomLimit)
				{
					if (cur.Nodes.Count == 0)
					{
						target[cur.GetHashCode()] = cur;
					}
				}
				cur = cur.NextVisibleNode;
			}
		}

		// ---------------- modern disclosure chevrons ----------------

		private TreeNode hoverDisclosureNode = null;

		private Rectangle DisclosureRect(TreeNode node)
		{
			Rectangle b = node.Bounds;
			int cy = b.Y + b.Height / 2;
			int cx = Math.Max(b.X - 13, 10);
			return new Rectangle(cx - 7, cy - 7, 14, 14);
		}

		private void UpdateTreeHover(Point pt)
		{
			TreeNode newHover = null;
			TreeNode row = GetNodeAt(pt);
			if (row != null && row.Nodes.Count > 0 && DisclosureRect(row).Contains(pt))
			{
				newHover = row;
			}
			if (!ReferenceEquals(newHover, hoverDisclosureNode))
			{
				hoverDisclosureNode = newHover;
				Invalidate();
			}
		}

		private static void DrawDisclosureGlyph(Graphics g, Rectangle r, bool expanded, bool hover)
		{
			g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

			if (hover)
			{
				using (SolidBrush hb = new SolidBrush(Color.FromArgb(26, 255, 255, 255)))
				{
					g.FillEllipse(hb, r.X - 1, r.Y - 1, r.Width + 2, r.Height + 2);
				}
			}

			Color c = hover ? Color.FromArgb(214, 220, 232) : Color.FromArgb(124, 132, 150);
			using (Pen p = new Pen(c, 1.65f))
			{
				p.StartCap = System.Drawing.Drawing2D.LineCap.Round;
				p.EndCap = System.Drawing.Drawing2D.LineCap.Round;
				p.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;

				float cx = r.X + r.Width / 2f;
				float cy = r.Y + r.Height / 2f;
				PointF a1, tip, a3;
				if (expanded)
				{
					a1 = new PointF(cx - 3.2f, cy - 1.6f);
					tip = new PointF(cx, cy + 2.2f);
					a3 = new PointF(cx + 3.2f, cy - 1.6f);
				}
				else
				{
					a1 = new PointF(cx - 1.6f, cy - 3.4f);
					tip = new PointF(cx + 2.2f, cy);
					a3 = new PointF(cx - 1.6f, cy + 3.4f);
				}
				g.DrawLines(p, new[] { a1, tip, a3 });
			}
		}

		protected override void OnItemDrag( ItemDragEventArgs e )
		{
			// If the user drags a node and the node being dragged is NOT
			// selected, then clear the active selection, select the
			// node being dragged and drag it. Otherwise if the node being
			// dragged is selected, drag the entire selection.
			try
			{
				TreeNode node = e.Item as TreeNode;

				if( node != null )
				{
					if( !m_SelectedNodes.ContainsValue( node ) )
					{
						SelectSingleNode( node );
						ToggleNode( node, true );
					}
				}

				base.OnItemDrag( e );
			}
			catch( Exception ex )
			{
				HandleException( ex );
			}
		}

		protected override void OnBeforeSelect( TreeViewCancelEventArgs e )
		{
			// Never allow base.SelectedNode to be set!
			try
			{
				base.SelectedNode = null;
				e.Cancel = true;

				base.OnBeforeSelect( e );
			}
			catch( Exception ex )
			{
				HandleException( ex );
			}
		}

		protected override void OnAfterSelect( TreeViewEventArgs e )
		{
			// Never allow base.SelectedNode to be set!
			try
			{
				base.OnAfterSelect( e );
				base.SelectedNode = null;
			
			}
			catch( Exception ex )
			{
				HandleException( ex );
			}
			
			this.Refresh();
		}

		protected override void OnKeyDown( KeyEventArgs e )
		{
			// Handle all possible key strokes for the control.
			// including navigation, selection, etc.

			base.OnKeyDown( e );

			if( e.KeyCode == Keys.ShiftKey ) return;

			//this.BeginUpdate();
			bool bShift = ( ModifierKeys == Keys.Shift );

			try
			{
				// Nothing is selected in the tree, this isn't a good state
				// select the top node
				if( m_SelectedNode == null && this.TopNode != null)
				{
					ToggleNode( this.TopNode, true );
				}

				// Nothing is still selected in the tree, 
				// this isn't a good state, leave.
				if (m_SelectedNode == null) return;

				if (e.KeyCode == Keys.Left)
				{
					if (m_SelectedNode.IsExpanded && m_SelectedNode.Nodes.Count > 0)
					{
						// Collapse an expanded node that has children
						m_SelectedNode.Collapse();
					}
					else if (m_SelectedNode.Parent != null)
					{
						// Node is already collapsed, try to select its parent.
						SelectSingleNode(m_SelectedNode.Parent);
					}
				}
				else if (e.KeyCode == Keys.Right)
				{
					if (!m_SelectedNode.IsExpanded)
					{
						// Expand a collapsed node's children
						m_SelectedNode.Expand();
					}
					else
					{
						// Node was already expanded, select the first child
						SelectSingleNode(m_SelectedNode.FirstNode);
					}
				}
				else if (e.KeyCode == Keys.Up)
				{
					// Select the previous node
					if (m_SelectedNode.PrevVisibleNode != null)
					{
						SelectNode(m_SelectedNode.PrevVisibleNode);
					}
				}
				else if (e.KeyCode == Keys.Down)
				{
					// Select the next node
					if (m_SelectedNode.NextVisibleNode != null)
					{
						SelectNode(m_SelectedNode.NextVisibleNode);
					}
				}
				else if (e.KeyCode == Keys.Home)
				{
					if (bShift)
					{
						if (m_SelectedNode.Parent == null)
						{
							// Select all of the root nodes up to this point
							if (this.Nodes.Count > 0)
							{
								SelectNode(this.Nodes[0]);
							}
						}
						else
						{
							// Select all of the nodes up to this point under 
							// this nodes parent
							SelectNode(m_SelectedNode.Parent.FirstNode);
						}
					}
					else
					{
						// Select this first node in the tree
						if (this.Nodes.Count > 0)
						{
							SelectSingleNode(this.Nodes[0]);
						}
					}
				}
				else if (e.KeyCode == Keys.End)
				{
					if (bShift)
					{
						if (m_SelectedNode.Parent == null)
						{
							// Select the last ROOT node in the tree
							if (this.Nodes.Count > 0)
							{
								SelectNode(this.Nodes[this.Nodes.Count - 1]);
							}
						}
						else
						{
							// Select the last node in this branch
							SelectNode(m_SelectedNode.Parent.LastNode);
						}
					}
					else
					{
						if (this.Nodes.Count > 0)
						{
							// Select the last node visible node in the tree.
							// Don't expand branches incase the tree is virtual
							TreeNode ndLast = this.Nodes[0].LastNode;
							while (ndLast.IsExpanded && (ndLast.LastNode != null))
							{
								ndLast = ndLast.LastNode;
							}
							SelectSingleNode(ndLast);
						}
					}
				}
				else if (e.KeyCode == Keys.PageUp)
				{
					// Select the highest node in the display
					int nCount = this.VisibleCount;
					TreeNode ndCurrent = m_SelectedNode;
					while ((nCount) > 0 && (ndCurrent.PrevVisibleNode != null))
					{
						ndCurrent = ndCurrent.PrevVisibleNode;
						nCount--;
					}
					SelectSingleNode(ndCurrent);
				}
				else if (e.KeyCode == Keys.PageDown)
				{
					// Select the lowest node in the display
					int nCount = this.VisibleCount;
					TreeNode ndCurrent = m_SelectedNode;
					while ((nCount) > 0 && (ndCurrent.NextVisibleNode != null))
					{
						ndCurrent = ndCurrent.NextVisibleNode;
						nCount--;
					}
					SelectSingleNode(ndCurrent);
				}
				else
				{
					// Assume this is a search character a-z, A-Z, 0-9, etc.
					// Select the first node after the current node that
					// starts with this character
					/*string sSearch = ((char)e.KeyValue).ToString();

					TreeNode ndCurrent = m_SelectedNode;
					while ((ndCurrent.NextVisibleNode != null))
					{
						ndCurrent = ndCurrent.NextVisibleNode;
						if (ndCurrent.Text.StartsWith(sSearch))
						{
							SelectSingleNode(ndCurrent);
							break;
						}
					}
					*/
				}

			}
			catch( Exception ex )
			{
				HandleException( ex );
			}
			finally
			{
				//this.EndUpdate();
				//this.Refresh();
			}
		}

		public void EnableDrawNode() 
		{
			DrawNodeRender = true;
		}
		public void DisableDrawNode() 
		{
			DrawNodeRender = false;
		}

		private bool DrawNodeRender = true;

		// devido de ao limpar nodes e colocar, fica selecinados no design, os que não estão selecionados, então subistitu-o a pintura
		// tive muito problema com a propriedade "Text", que causave muito lag, no treeview.
		// descobri que a melhor solução é pegar o texto de outro lugar e deixar o "Text" em branco. 
		protected override void OnDrawNode(DrawTreeNodeEventArgs e)
        {
			//Console.WriteLine(e.Node.Name + ' ' + e.Bounds.Y);
			//base.OnDrawNode(e); // não usado

			e.DrawDefault = false; // false o sistema não renderiza, true é renderizado o texto

			if (DrawNodeRender)
			{
				if (e.Bounds.Y <= Height && e.Bounds.Y >= 0)
				{
					Font font = Font;
					if (e.Node.NodeFont != null)
					{
						font = e.Node.NodeFont;
					}

					string altText = e.Node.Text;
					Color altForeColor = e.Node.ForeColor;
					if (e.Node is IAltNode obj)
					{
						altText = obj.AltText;
						altForeColor = obj.AltForeColor;
					}

					// Dark-mode fix: some dynamically generated object names use
					// Color.Black / very dark colors instead of inheriting the TreeView
					// ForeColor. The owner-draw path uses that color directly, so those
					// names become almost invisible on the charcoal background. Keep
					// intentional bright category colors (red/green/yellow/etc.), but
					// promote near-black text to a readable light color, but only
					// while the dark theme is active - in Light Mode those same
					// dark category colors (LIT/EFF tables/QuadCustom) must stay
					// dark so they remain visible on the white background.
					if (IsDarkModeColor(altForeColor) && !Re4QuadExtremeEditor.UiTheme.IsLight)
					{
						altForeColor = Color.FromArgb(232, 235, 240);
					}

					//tampa o texto/seleção de fundo
					e.Graphics.FillRectangle(new SolidBrush(BackColor), e.Bounds);

					// modern disclosure chevron for expandable rows; root-level
					// groups get a small text indent so the glyph stays inside
					bool hasChildren = e.Node.Nodes.Count > 0;
					int textOffsetX = 0;
					if (hasChildren)
					{
						if (DisclosureRect(e.Node).X < 8) textOffsetX = 18;
						DrawDisclosureGlyph(e.Graphics, DisclosureRect(e.Node),
							e.Node.IsExpanded, ReferenceEquals(hoverDisclosureNode, e.Node));
					}

					//se é um node selecionado
					if (m_SelectedNodes.ContainsValue(e.Node) && e.Node.Parent != null)
					{
						Color selectionColor = useThemedSelectedNodeBackColor
							? (Re4QuadExtremeEditor.UiTheme.IsLight
								? System.Drawing.Color.FromArgb(228, 238, 251)
								: Color.FromArgb(45, 49, 57))
							: selectedNodeBackColor;
						e.Graphics.FillRectangle(new SolidBrush(selectionColor),
							new Rectangle(e.Bounds.X + textOffsetX, e.Bounds.Y, TextRenderer.MeasureText(altText, font).Width, e.Bounds.Height));
					}

					//renderiza o texto
					TextRenderer.DrawText(e.Graphics, altText, font, new Point(e.Bounds.Left + textOffsetX, e.Bounds.Top), altForeColor, TextFormatFlags.GlyphOverhangPadding);
				}
			}
        }

        #endregion

        private static bool IsDarkModeColor(Color color)
        {
            // Treat near-black/very dark neutral text as normal UI text.
            // Bright colored RE4 categories are left untouched.
            int max = Math.Max(color.R, Math.Max(color.G, color.B));
            int min = Math.Min(color.R, Math.Min(color.G, color.B));
            int spread = max - min;
            return max < 105 || (max < 125 && spread < 24);
        }

        #region Helper Methods

        public void ToSelectSingleNode(TreeNode node) 
		{
			ClearSelectedNodes();
			if (node != null && node.Parent != null)
			{
				m_SelectedNodes.Add(node.GetHashCode(), node);
				m_SelectedNode = node;
				node.EnsureVisible();
				OnAfterSelect(new TreeViewEventArgs(m_SelectedNode));
			}
			else 
			{
				OnAfterSelect(new TreeViewEventArgs(null));
			}
		}

		public void ToSelectMultiNode(TreeNode node)
		{
			if (node != null && node.Parent != null)
			{
				if (m_SelectedNodes.ContainsKey(node.GetHashCode()))
				{
					m_SelectedNodes.Remove(node.GetHashCode());
					if (m_SelectedNodes.Count >= 1)
					{
						m_SelectedNode = m_SelectedNodes.Last().Value;
						m_SelectedNode.EnsureVisible();
					}
					else
					{
						m_SelectedNode = null;
					}

					OnAfterSelect(new TreeViewEventArgs(m_SelectedNode));
				}
				else
				{
					m_SelectedNodes.Add(node.GetHashCode(),node);
					m_SelectedNode = node;
					node.EnsureVisible();
					OnAfterSelect(new TreeViewEventArgs(m_SelectedNode));
				}
			}
			else
			{
				OnAfterSelect(new TreeViewEventArgs(m_SelectedNode));
			}

		}

		/// <summary>
		/// Bulk selection used by box-select / duplicate-all flows.
		/// Skips the per-node EnsureVisible scrolling and fires AfterSelect
		/// exactly once, so listeners like the PropertyGrid rebuild only once.
		/// </summary>
		public void ToSelectNodesBatch(IEnumerable<TreeNode> nodes)
		{
			m_SelectedNodes.Clear();
			m_SelectedNode = null;

			bool any = false;
			foreach (TreeNode node in nodes)
			{
				if (node == null || node.Parent == null) continue;
				int key = node.GetHashCode();
				if (!m_SelectedNodes.ContainsKey(key))
				{
					m_SelectedNodes.Add(key, node);
				}
				m_SelectedNode = node;
				any = true;
			}

			if (any)
			{
				m_SelectedNode.EnsureVisible();
			}
			OnAfterSelect(new TreeViewEventArgs(m_SelectedNode));
		}




		private void SelectNode( TreeNode node )
        {
            if (node == null)
            {
				ClearSelectedNodes();
				OnAfterSelect(new TreeViewEventArgs(null));
				return;
			}
            if (node.Parent == null)
            {
				return;
			}
            try
			{
				//this.BeginUpdate();

				if( m_SelectedNode == null || ModifierKeys == Keys.Control )
				{
					// Ctrl+Click selects an unselected node, or unselects a selected node.
					bool bIsSelected = m_SelectedNodes.ContainsValue( node );
					ToggleNode( node, !bIsSelected );
				}
				else if( ModifierKeys == Keys.Shift )
				{
					this.BeginUpdate();

					// Shift+Click selects nodes between the selected node and here.
					TreeNode ndStart = m_SelectedNode;
					TreeNode ndEnd = node;

					if( ndStart.Parent == ndEnd.Parent )
					{
						// Selected node and clicked node have same parent, easy case.
						if( ndStart.Index < ndEnd.Index )
						{							
							// If the selected node is beneath the clicked node walk down
							// selecting each Visible node until we reach the end.
							while( ndStart != ndEnd )
							{
								ndStart = ndStart.NextVisibleNode;
								if( ndStart == null ) break;
								ToggleNode( ndStart, true );
							}
						}
						else if( ndStart.Index == ndEnd.Index )
						{
							// Clicked same node, do nothing
						}
						else
						{
							// If the selected node is above the clicked node walk up
							// selecting each Visible node until we reach the end.
							while( ndStart != ndEnd )
							{
								ndStart = ndStart.PrevVisibleNode;
								if( ndStart == null ) break;
								ToggleNode( ndStart, true );
							}
						}
					}
					else
					{
						// Selected node and clicked node have same parent, hard case.
						// We need to find a common parent to determine if we need
						// to walk down selecting, or walk up selecting.

						TreeNode ndStartP = ndStart;
						TreeNode ndEndP = ndEnd;
						int startDepth = Math.Min( ndStartP.Level, ndEndP.Level );

						// Bring lower node up to common depth
						while( ndStartP.Level > startDepth )
						{
							ndStartP = ndStartP.Parent;
						}

						// Bring lower node up to common depth
						while( ndEndP.Level > startDepth )
						{
							ndEndP = ndEndP.Parent;
						}

						// Walk up the tree until we find the common parent
						while( ndStartP.Parent != ndEndP.Parent )
						{
							ndStartP = ndStartP.Parent;
							ndEndP = ndEndP.Parent;
						}

						// Select the node
						if( ndStartP.Index < ndEndP.Index )
						{
							// If the selected node is beneath the clicked node walk down
							// selecting each Visible node until we reach the end.
							while( ndStart != ndEnd )
							{
								ndStart = ndStart.NextVisibleNode;
								if( ndStart == null ) break;
								ToggleNode( ndStart, true );
							}
						}
						else if( ndStartP.Index == ndEndP.Index )
						{
							if( ndStart.Level < ndEnd.Level )
							{
								while( ndStart != ndEnd )
								{
									ndStart = ndStart.NextVisibleNode;
									if( ndStart == null ) break;
									ToggleNode( ndStart, true );
								}
							}
							else
							{
								while( ndStart != ndEnd )
								{
									ndStart = ndStart.PrevVisibleNode;
									if( ndStart == null ) break;
									ToggleNode( ndStart, true );
								}
							}
						}
						else
						{
							// If the selected node is above the clicked node walk up
							// selecting each Visible node until we reach the end.
							while( ndStart != ndEnd )
							{
								ndStart = ndStart.PrevVisibleNode;
								if( ndStart == null ) break;
								ToggleNode( ndStart, true );
							}
						}
					}
					this.EndUpdate();
					this.Refresh();
				}
				else
				{
					// Just clicked a node, select it
					SelectSingleNode( node );
				}

				OnAfterSelect(new TreeViewEventArgs( m_SelectedNode ));
			}
			finally
			{
				//this.EndUpdate();
				//this.Refresh();
			}
		}

		private void ClearSelectedNodes()
		{
			m_SelectedNodes.Clear();
			m_SelectedNode = null;
		}

        private void SelectSingleNode( TreeNode node )
        {
            if ( node == null || node.Parent == null)
            {
                return;
			}

			ClearSelectedNodes();
			ToggleNode( node, true );
			node.EnsureVisible();
		}

		private void ToggleNode( TreeNode node, bool bSelectNode )
		{
            if (node == null || node.Parent == null)
            {
                return;
            }
			if( bSelectNode )
			{
				m_SelectedNode = node;
				if( !m_SelectedNodes.ContainsKey( node.GetHashCode() ) )
				{
					m_SelectedNodes.Add( node.GetHashCode(), node );
				}
				
			}
			else
			{
				m_SelectedNodes.Remove( node.GetHashCode() );
			}
			//this.Refresh(); // lag
		}

		private void HandleException( Exception ex )
		{
			// Perform some error handling here.
			// We don't want to bubble errors to the CLR. 
			MessageBox.Show(ex.Message , "MultiselectTreeView Error");
		}

        #endregion
    }
}
