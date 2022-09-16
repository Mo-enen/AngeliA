using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace PixelJelly {
	public class Cactus : JellyBehaviour {



		// SUB
		public enum CactusStyle {
			Hierarchy = 0,
			Straight = 1,
			Wall = 2,
		}



		// Api
		public override string DisplayGroup => "Vegetation";
		public override string DisplayName => "Cactus";

		public override int MaxFrameCount => 1;
		public override int MaxSpriteCount => 128;
		public CactusStyle Style => m_Style;
		public Vector2Int Size { get => m_Size; set => m_Size = value; }
		public int CactusHeight { get => m_CactusHeight; set => m_CactusHeight = value; }
		public Vector2 Dilapidated { get => m_Dilapidated; set => m_Dilapidated = value; }
		public Color32 Spike { get => m_Spike; set => m_Spike = value; }
		public Color32 Normal { get => m_Normal; set => m_Normal = value; }
		public Color32 Dark { get => m_Dark; set => m_Dark = value; }


		// Ser
		[SerializeField, ArrowNumber] private CactusStyle m_Style = CactusStyle.Hierarchy;
		[SerializeField, MinMaxNumber(1)] private Vector2Int m_Size = new Vector2Int(2, 3);
		[SerializeField, MinMaxNumber(1)] private int m_CactusHeight = 26;
		[SerializeField, MinMaxNumber(0f, 1f, 0.1f)] private Vector2 m_Dilapidated = new Vector2(0f, 0.4f);
		[SerializeField] Color32 m_Spike = new Color32(151, 245, 83, 255);
		[SerializeField] Color32 m_Normal = new Color32(81, 166, 58, 255);
		[SerializeField] Color32 m_Dark = new Color32(59, 115, 61, 255);


		// MSG
		protected override void OnCreated () {
			Width = 32;
			Height = 32;
			FrameCount = 1;
			SpriteCount = 6;
		}


		protected override void BeforePopulateAllPixels () {
			FrameCount = 1;
		}


		protected override void OnPopulatePixels (int width, int height, int frame, int frameCount, int sprite, int spriteCount, out Vector2 pivot, out RectOffset border) {
			pivot = new Vector2(width / 2f, 0);
			border = new RectOffset(0, 0, 0, 0);
			if (width <= 0 || height <= 0 || frameCount <= 0) { return; }
			if (frame == 0 && sprite == 0) {
				CheckInspectorMessages(width);
			}
			switch (m_Style) {
				default:
				case CactusStyle.Hierarchy: {
					float dilapidated = m_Dilapidated.RandomBetween(Random);
					int size = m_Size.RandomBetween(Random) * 2 - 1;
					int l = width / 2 - size / 2;
					var queue = new Queue<(int l, int r, int d, int u)>();
					int nodeHeight = GetNodeHeight();
					DrawCactusBody(l, l + size - 1, 0, nodeHeight, dilapidated);
					DrawCactusHead(l, l + size - 1, nodeHeight + 1, dilapidated);
					queue.Enqueue((l, l + size - 1, 0, nodeHeight));
					size--;
					for (int safeCount = 0; queue.Count > 0 && safeCount < 1024; safeCount++) {
						var (_l, _r, _d, _u) = queue.Dequeue();
						bool added = false;
						// Add More
						bool _right = Random.Next(0, int.MaxValue) % 2 == 0;
						for (int dir = 0; dir < 2 && _u - _d >= size; dir++) {
							if (safeCount > 1 && Random.NextDouble() < dilapidated) { continue; }
							int _horizontal = Random.Next(4, 8);
							int _newD = dir == 0 ? Random.Next(_d + size, _u + size) : _u + size;
							int _newU = Mathf.Clamp(_newD + GetNodeHeight(), _newD + 1, height - size - 2);
							int _newLAlt = _right ? _r + _horizontal : _l - _horizontal - size;
							int _newRAlt = _right ? _r + _horizontal + size : _l - _horizontal;
							if (_newD >= 0 && _newU < height - size && _newLAlt >= 0 && _newRAlt < width) {
								// Add
								DrawCactusBody(_newLAlt, _newRAlt, _newD, _newU, dilapidated);
								DrawCactusHead(_newLAlt, _newRAlt, _newU + 1, dilapidated);
								DrawCactusJoint(_l, _r, _newD - size, _right, true, dilapidated);
								DrawCactusHorizontal(
									_right ? _r : _newRAlt,
									_right ? _newLAlt : _l,
									_newD - size, _newD, dilapidated
								);
								DrawCactusJoint(_newLAlt, _newRAlt, _newD - size, !_right, false, dilapidated);
								queue.Enqueue((_newLAlt, _newRAlt, _newD, _newU));
								added = true;
							}
							_right = !_right;
						}
						if (!added || size <= 3) { break; }
						size -= 2;
					}
					break;
					// Func
					int GetNodeHeight () => Mathf.Clamp(
						Random.Next(
							Mathf.Max(height / 10, 6),
							Mathf.Max(height / 2, 24)
						), size, height / 2
					);
				}
				case CactusStyle.Straight: {
					int basicSize = m_Size.RandomBetween(Random);
					int size = basicSize * 2 - 1;
					int l = width / 2 - size / 2;
					int cHeight = Mathf.Max(m_CactusHeight - basicSize - 1, 0);
					float dilapidated = m_Dilapidated.RandomBetween(Random);
					DrawCactusBody(l, l + size - 1, 0, cHeight - 1, dilapidated);
					DrawCactusHead(l, l + size - 1, cHeight, dilapidated);
					break;
				}
				case CactusStyle.Wall: {
					float dilapidated = m_Dilapidated.RandomBetween(Random);
					int basicSize = m_Size.RandomBetween(Random);
					int size = Mathf.Max(basicSize, 2) * 2 - 1;
					int r;
					int xGap = Mathf.Clamp(width / 20, 1, 32);
					int heightGap = Mathf.Clamp(height / 5, 1, 32);
					bool reverse = Random.Next(0, int.MaxValue) % 2 == 0;
					for (int l = 1; l < width - 1; l = r + 3) {
						r = l + Random.Next(size - xGap, size + xGap);
						r = Mathf.Clamp(r, 0, width - 1);
						if (r > width - size) {
							r = width - 2;
						}
						if (l >= r) { break; }
						int cHeight = m_CactusHeight - basicSize - 1;
						cHeight = Random.Next(cHeight - heightGap, cHeight + heightGap);
						cHeight = Mathf.Clamp(cHeight, 0, height - 1);
						if (!reverse) {
							DrawCactusBody(l, r, 0, cHeight - 1, dilapidated);
							DrawCactusHead(l, r, cHeight, dilapidated);
						} else {
							DrawCactusBody(width - 1 - r, width - 1 - l, 0, cHeight - 1, dilapidated);
							DrawCactusHead(width - 1 - r, width - 1 - l, cHeight, dilapidated);
						}
					}
					break;
				}
			}
		}


		protected void DrawCactusBody (int l, int r, int d, int u, float dilapidated) {
			// Body
			for (int y = d; y <= u; y++) {
				for (int x = l; x <= r; x++) {
					if ((x == l || x == r) && Random.NextDouble() < dilapidated) {
						continue;
					}
					SetColor(x, y, (x - l) % 2 == 0 ? m_Dark : m_Normal);
				}
			}
			// Spike
			for (int x = l - 1; x <= r + 1; x += 2) {
				for (int y = d + Random.Next(1, 4); y <= u; y += Random.Next(2, 5)) {
					if (Random.NextDouble() > dilapidated) {
						SetColor(x, y, m_Spike);
					}
				}
			}
			// Mid
			for (int x = l + 2; x <= r - 2; x += 2) {
				for (int y = d; y <= u; y++) {
					if (Random.NextDouble() < dilapidated) {
						SetColor(x, y, m_Normal);
					}
				}
			}
		}


		protected void DrawCactusHorizontal (int l, int r, int d, int u, float dilapidated) {
			// Body
			for (int y = d; y <= u; y++) {
				for (int x = l; x <= r; x++) {
					if ((y == d || y == u) && Random.NextDouble() < dilapidated) {
						continue;
					}
					SetColor(x, y, (y - d) % 2 == 0 ? m_Dark : m_Normal);
				}
			}
			// Spike
			for (int y = d - 1; y <= u + 1; y += 2) {
				for (int x = l + Random.Next(1, 4); x <= r; x += Random.Next(2, 5)) {
					if (Random.NextDouble() > dilapidated) {
						SetColor(x, y, m_Spike);
					}
				}
			}
			// Mid
			for (int y = d + 2; y <= u - 2; y += 2) {
				for (int x = l; x <= r; x++) {
					if (Random.NextDouble() < dilapidated) {
						SetColor(x, y, m_Normal);
					}
				}
			}
		}


		protected void DrawCactusHead (int l, int r, int d, float dilapidated) {
			int u = d + (r - l) / 2;
			using var _mask = new MaskScope(new RectInt(l, d, r - l + 1, u - d + 2));
			var shape = new Circle(r - l + 1, new Vector2Int(0, u - d));
			// Normal
			DrawShape(shape, l, d, m_Normal);
			// Dark
			for (int x = l; x <= r; x += 2) {
				for (int y = d; y <= u; y++) {
					if (!shape.PixelCheck(x - l + shape.Pivot.x, y - d + shape.Pivot.y)) { continue; }
					if (Random.NextDouble() < dilapidated) {
						continue;
					}
					SetColor(x, y, m_Dark);
				}
			}
			// Spike
			for (int x = l - 1; x <= r + 1; x += 2) {
				for (int y = d + Random.Next(1, 4); y <= u - 2; y += Random.Next(2, 5)) {
					if (Random.NextDouble() > dilapidated && shape.PixelCheck(x - l + shape.Pivot.x, y - d + shape.Pivot.y)) {
						SetColor(x, y, m_Spike);
					}
				}
			}
			for (int x = l; x <= r; x += 2) {
				if (Random.NextDouble() < dilapidated) { continue; }
				for (int y = u; y >= d; y--) {
					if (shape.PixelCheck(x - l + shape.Pivot.x, y - d + shape.Pivot.y)) {
						SetColor(x, y + 1, m_Spike);
						break;
					}
				}
			}
		}


		protected void DrawCactusJoint (int l, int r, int d, bool right, bool up, float dilapidated) {
			if (l == r) { return; }
			int size = r - l + 1;
			int u = d + size - 1;
			using var _mask = new MaskScope(new RectInt(l, d, size, size));
			var shape = new Circle(size * 2, Vector2Int.zero);
			int rotFixX = right ? 0 : size;
			int rotFixY = up ? size : 0;
			// Normal Dark
			for (int i = 0; i < size; i++) {
				for (int x = l; x <= r; x++) {
					for (int y = d; y <= u; y++) {
						if (!shape.PixelCheck(
							x - l + rotFixX + (right ? -i : i),
							y - d + rotFixY + (up ? i : -i)
						)) { continue; }
						if (i % 2 == 0) {
							// Dark / Normal
							if (Random.NextDouble() > dilapidated) {
								SetColor(x, y, m_Dark);
							} else {
								SetColor(x, y, m_Normal);
							}
						} else {
							// Normal / Spike
							if (
								(x - l + Random.Next(0, 2)) % 3 == 0 &&
								(y - d + Random.Next(0, 2)) % 2 == 0 &&
								Random.NextDouble() > dilapidated
							) {
								SetColor(x, y, m_Spike);
							} else {
								SetColor(x, y, m_Normal);
							}
						}
					}
				}
			}
		}


		private void CheckInspectorMessages (int width) {
			if (m_Size.x > width) {
				AddInspectorMessage("Size is too large.", MessageType.Warning, "m_Size");
			}
			if (m_Dilapidated.x > 0.5f) {
				AddInspectorMessage("Dilapidated is too large.", MessageType.Warning, "m_Dilapidated");
			}
			if (m_Style != CactusStyle.Hierarchy && m_CactusHeight < m_Size.x) {
				AddInspectorMessage("Cactus Height is too small, it should be larger than Size.", MessageType.Warning, "m_CactusHeight");
			}
		}


	}
}
