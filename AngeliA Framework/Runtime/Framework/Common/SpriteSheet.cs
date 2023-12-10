using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {


	public enum SheetType {
		General = 0,
		Level = 1,
		Background = 2,
	}


	public enum GroupType {
		General = 0,
		Rule = 1,
		Random = 2,
		Animated = 3,
	}



	[System.Serializable]
	public class FlexSprite {
		public string Name;
		public string SheetName;
		public int SheetZ;
		public Rect Rect;
		public Vector2Int AngePivot;
		public Vector4 Border;
		public SheetType SheetType;
	}



	[System.Serializable]
	public class AngeSprite {

		// Const
		private const float UV_SCALE = 10000000f;

		// Api
		public Rect UvRect => Rect.MinMaxRect(UvBottomLeft.x, UvBottomLeft.y, UvTopRight.x, UvTopRight.y);
		public Vector2 BottomRight {
			get {
				if (!_BottomRight.HasValue) _BottomRight = new(UvTopRight.x, UvBottomLeft.y);
				return _BottomRight.Value;
			}
		}
		public Vector2 TopLeft {
			get {
				if (!_TopLeft.HasValue) _TopLeft = new(UvBottomLeft.x, UvTopRight.y);
				return _TopLeft.Value;
			}
		}
		public Color32 SummaryTint {
			get {
				if (!_SummaryTint.HasValue) {
					_SummaryTint = Util.IntToColor(f);
				}
				return _SummaryTint.Value;
			}
			set {
				f = Util.ColorToInt(value);
				_SummaryTint = null;
			}
		}
		public int GlobalID { get => a; set => a = value; }
		public int MetaIndex { get => b; set => b = value; }
		public int GlobalWidth { get => c; set => c = value; }
		public int GlobalHeight { get => d; set => d = value; }
		public int SortingZ { get => e; set => e = value; }
		public Vector4Int GlobalBorder {
			get => new(i, j, k, l);
			set {
				i = value.left;
				j = value.right;
				k = value.down;
				l = value.up;
			}
		}
		public int PivotX { get => g; set => g = value; }
		public int PivotY { get => h; set => h = value; }
		public Vector4 UvBorder { // xyzw => ldru
			get => new(m / UV_SCALE, o / UV_SCALE, n / UV_SCALE, p / UV_SCALE);
			set {
				m = (int)(value.x * UV_SCALE);
				n = (int)(value.z * UV_SCALE);
				o = (int)(value.y * UV_SCALE);
				p = (int)(value.w * UV_SCALE);
			}
		}
		public Vector2 UvBottomLeft {
			get => new(q / UV_SCALE, r / UV_SCALE);
			set {
				q = (int)(value.x * UV_SCALE);
				r = (int)(value.y * UV_SCALE);
			}
		}
		public Vector2 UvTopRight {
			get => new(s / UV_SCALE, t / UV_SCALE);
			set {
				s = (int)(value.x * UV_SCALE);
				t = (int)(value.y * UV_SCALE);
			}
		}
		public string RealName {
			get => u;
			set => u = value;
		}
		public int SheetNameIndex {
			get => v;
			set => v = value;
		}
		public SheetType SheetType {
			get => _SheetType ??= (SheetType)(w & 0b00000011);
			set {
				_SheetType = value;
				w = (byte)((w & 0b00001100) | (int)value);
			}
		}
		public GroupType GroupType {
			get => _GroupType ??= (GroupType)((w & 0b00001100) >> 2);
			set {
				_GroupType = value;
				w = (byte)((w & 0b00000011) | ((int)value << 2));
			}
		}

		// Ser
		[SerializeField] int a; // GlobalID
		[SerializeField] int b; // MetaIndex
		[SerializeField] int c; // GlobalWidth
		[SerializeField] int d; // GlobalHeight
		[SerializeField] int e; // SortingZ
		[SerializeField] int f; // SummaryTint
		[SerializeField] int g; // PivotX
		[SerializeField] int h; // PivotY

		[SerializeField] int i; // GlobalBorder L
		[SerializeField] int j; // GlobalBorder R
		[SerializeField] int k; // GlobalBorder D
		[SerializeField] int l; // GlobalBorder U

		[SerializeField] int m; // UvBorder L
		[SerializeField] int n; // UvBorder R
		[SerializeField] int o; // UvBorder D
		[SerializeField] int p; // UvBorder U

		[SerializeField] int q; // UvBottomLeft L
		[SerializeField] int r; // UvBottomLeft D
		[SerializeField] int s; // UvTopRight R
		[SerializeField] int t; // UvTopRight U

		[SerializeField] string u;  // Real Name
		[SerializeField] int v;     // Sheet Name Index
		[SerializeField] byte w;    // SheetType & GroupType

		// Data
		[System.NonSerialized] Vector2? _BottomRight = null;
		[System.NonSerialized] Vector2? _TopLeft = null;
		[System.NonSerialized] Color32? _SummaryTint = default;
		[System.NonSerialized] SheetType? _SheetType = null;
		[System.NonSerialized] GroupType? _GroupType = null;

		// API
		public RectInt GetTextureRect (int tWidth, int tHeight) => new(
			(UvBottomLeft.x * tWidth).RoundToInt(),
			(UvBottomLeft.y * tHeight).RoundToInt(),
			((UvTopRight.x - UvBottomLeft.x) * tWidth).RoundToInt(),
			((UvTopRight.y - UvBottomLeft.y) * tHeight).RoundToInt()
		);

		public void GetSlicedUvBorder (Alignment alignment, out Vector2 bl, out Vector2 br, out Vector2 tl, out Vector2 tr) {
			bl = UvBottomLeft;
			br = BottomRight;
			tl = TopLeft;
			tr = UvTopRight;
			// Y
			switch (alignment) {
				case Alignment.TopLeft:
				case Alignment.TopMid:
				case Alignment.TopRight:
					bl.y = br.y = Mathf.LerpUnclamped(TopLeft.y, UvBottomLeft.y, UvBorder.w);
					break;
				case Alignment.MidLeft:
				case Alignment.MidMid:
				case Alignment.MidRight:
					tl.y = tr.y = Mathf.LerpUnclamped(TopLeft.y, UvBottomLeft.y, UvBorder.w);
					bl.y = br.y = Mathf.LerpUnclamped(UvBottomLeft.y, TopLeft.y, UvBorder.y);
					break;
				case Alignment.BottomLeft:
				case Alignment.BottomMid:
				case Alignment.BottomRight:
					tl.y = tr.y = Mathf.LerpUnclamped(UvBottomLeft.y, TopLeft.y, UvBorder.y);
					break;
			}
			// X
			switch (alignment) {
				case Alignment.TopLeft:
				case Alignment.MidLeft:
				case Alignment.BottomLeft:
					br.x = tr.x = Mathf.LerpUnclamped(UvBottomLeft.x, BottomRight.x, UvBorder.x);
					break;
				case Alignment.TopMid:
				case Alignment.MidMid:
				case Alignment.BottomMid:
					br.x = tr.x = Mathf.LerpUnclamped(BottomRight.x, UvBottomLeft.x, UvBorder.z);
					bl.x = tl.x = Mathf.LerpUnclamped(UvBottomLeft.x, BottomRight.x, UvBorder.x);
					break;
				case Alignment.TopRight:
				case Alignment.MidRight:
				case Alignment.BottomRight:
					bl.x = tl.x = Mathf.LerpUnclamped(BottomRight.x, UvBottomLeft.x, UvBorder.z);
					break;
			}
		}

	}



	[System.Serializable]
	public class AngeSpriteChain {

		public int this[int i] => Chain[i];
		public int Count => Chain.Count;
		public string Name { get => N; set => N = value; }
		public int ID { get => I; set => I = value; }
		public GroupType Type { get => T; set => T = value; }
		public int LoopStart { get => L; set => L = value; }
		public List<int> Chain { get => C; set => C = value; }

		[SerializeField] string N;
		[SerializeField] int I;
		[SerializeField] GroupType T;
		[SerializeField] int L = 0;
		[SerializeField] List<int> C = new(); // Sprite Index in Sheet, Not id

		public List<AngeSprite> GetSpriteChain (SpriteSheet sheet) {
			var result = new List<AngeSprite>();
			foreach (var index in Chain) {
				result.Add(sheet.Sprites[index]);
			}
			return result;
		}

	}


	[System.Serializable]
	public class SpriteMeta {

		public int Rule { get => R; set => R = value; }
		public int Tag { get => T; set => T = value; }
		public bool IsTrigger { get => G; set => G = value; }

		[SerializeField] int T = 0;
		[SerializeField] int R = 0;
		[SerializeField] bool G = false;

	}



	[System.Serializable]
	public class SpriteGroup {
		public int ID;
		public int[] SpriteIDs;
	}



	[System.Serializable]
	public class SpriteSheet {
		public AngeSprite[] Sprites = new AngeSprite[0];
		public AngeSpriteChain[] SpriteChains = new AngeSpriteChain[0];
		public SpriteGroup[] Groups = new SpriteGroup[0];
		public SpriteMeta[] Metas = new SpriteMeta[0];
		public string[] SheetNames = new string[0];
	}


}