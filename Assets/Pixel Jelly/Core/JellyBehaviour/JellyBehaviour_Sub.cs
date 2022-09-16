using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PixelJelly {



	public class CommentData {
		public string Content;
		public int X;
		public int Y;
		public Color32 FontColor;
		public Color32 BackColor;
		public CommentData (string content, int x, int y) : this(content, x, y, new Color32(255, 255, 255, 255), new Color32(0, 0, 0, 86)) { }
		public CommentData (string content, int x, int y, Color32 fontColor) : this(content, x, y, fontColor, new Color32(0, 0, 0, 86)) { }
		public CommentData (string content, int x, int y, Color32 fontColor, Color32 backColor) {
			Content = content;
			X = x;
			Y = y;
			FontColor = fontColor;
			BackColor = backColor;
		}
	}



	public class MessageData {
		public string Message;
		public MessageType Type;
		public string HighlightParam;
		public MessageData (string message, MessageType type, string highlightParam = "") {
			Message = message;
			Type = type;
			HighlightParam = highlightParam;
		}
	}



	public abstract partial class JellyBehaviour {



		public class LayerScope : System.IDisposable {
			private readonly JellyBehaviour Behaviour;
			private static Color32[] GlobalLayerPixels = new Color32[MAX_SIZE * MAX_SIZE];
			private Color32[] LayerPixels = null;
			private static int GlobalDeep = 0;
			public LayerScope () {
				var behaviour = PopulatingBehaviour;
				if (behaviour == null) { return; }
				Behaviour = behaviour;
				int realLen = FillingWidth * FillingHeight;
				if (GlobalDeep == 0) {
					System.Array.Clear(GlobalLayerPixels, 0, realLen);
					(FillingPixels, GlobalLayerPixels) = (GlobalLayerPixels, FillingPixels);
				} else {
					LayerPixels = new Color32[realLen];
					(FillingPixels, LayerPixels) = (LayerPixels, FillingPixels);
				}
				GlobalDeep++;
			}
			public void Dispose () {
				if (Behaviour == null) { return; }
				GlobalDeep--;
				Color32[] pixels;
				int realLen = FillingWidth * FillingHeight;
				if (LayerPixels == null) {
					(FillingPixels, GlobalLayerPixels) = (GlobalLayerPixels, FillingPixels);
					pixels = GlobalLayerPixels;
				} else {
					(FillingPixels, LayerPixels) = (LayerPixels, FillingPixels);
					pixels = LayerPixels;
				}
				for (int i = 0; i < realLen; i++) {
					FillingPixels[i] = Util.Blend_OneMinusAlpha(FillingPixels[i], pixels[i]);
				}
			}
		}



		public class MaskScope : System.IDisposable {
			private readonly JellyBehaviour Behaviour;
			public MaskScope (RectInt rect, bool reverse = false) {
				var behaviour = PopulatingBehaviour;
				if (behaviour == null) { return; }
				Behaviour = behaviour;
				MaskingRange = rect;
				ReverseMask = reverse;
			}
			public void Dispose () {
				if (Behaviour == null) { return; }
				MaskingRange = null;
				ReverseMask = false;
			}
		}



		public class LockClearScope : System.IDisposable {
			private readonly JellyBehaviour Behaviour;
			private readonly bool OldLock;
			public LockClearScope () {
				var behaviour = PopulatingBehaviour;
				if (behaviour == null) { return; }
				Behaviour = behaviour;
				OldLock = LockClear;
				LockClear = true;
			}
			public void Dispose () {
				if (Behaviour == null) { return; }
				LockClear = OldLock;
			}
		}


		public class LockPixelScope : System.IDisposable {
			private readonly JellyBehaviour Behaviour;
			private readonly bool OldLock;
			public LockPixelScope () {
				var behaviour = PopulatingBehaviour;
				if (behaviour == null) { return; }
				Behaviour = behaviour;
				OldLock = LockPixel;
				LockPixel = true;
			}
			public void Dispose () {
				if (Behaviour == null) { return; }
				LockPixel = OldLock;
			}
		}




	}

}