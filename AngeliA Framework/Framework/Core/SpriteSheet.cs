using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;


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



	public class FlexSprite {
		public string Name;
		public Int2 AngePivot;
		public Float4 Border;
		public FRect Rect;
		public int SheetZ;
		public string SheetName;
		public SheetType SheetType;
	}



	[JsonObject(MemberSerialization.OptIn)]
	public class AngeSprite {

		// Const
		private const float UV_SCALE = 10000000f;

		// Api 
		public FRect UvRect => FRect.MinMaxRect(UvBottomLeft.x, UvBottomLeft.y, UvTopRight.x, UvTopRight.y);
		public Float2 BottomRight {
			get {
				if (!_BottomRight.HasValue) _BottomRight = new(UvTopRight.x, UvBottomLeft.y);
				return _BottomRight.Value;
			}
		}
		public Float2 TopLeft {
			get {
				if (!_TopLeft.HasValue) _TopLeft = new(UvBottomLeft.x, UvTopRight.y);
				return _TopLeft.Value;
			}
		}
		public Byte4 SummaryTint {
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
		public Int4 GlobalBorder {
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
		public Float4 UvBorder { // xyzw => ldru
			get => new(m / UV_SCALE, o / UV_SCALE, n / UV_SCALE, p / UV_SCALE);
			set {
				m = (int)(value.x * UV_SCALE);
				n = (int)(value.z * UV_SCALE);
				o = (int)(value.y * UV_SCALE);
				p = (int)(value.w * UV_SCALE);
			}
		}
		public Float2 UvBottomLeft {
			get => new(q / UV_SCALE, r / UV_SCALE);
			set {
				q = (int)(value.x * UV_SCALE);
				r = (int)(value.y * UV_SCALE);
			}
		}
		public Float2 UvTopRight {
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
		[JsonProperty] int a; // GlobalID
		[JsonProperty] int b; // MetaIndex
		[JsonProperty] int c; // GlobalWidth
		[JsonProperty] int d; // GlobalHeight
		[JsonProperty] int e; // SortingZ
		[JsonProperty] int f; // SummaryTint
		[JsonProperty] int g; // PivotX
		[JsonProperty] int h; // PivotY

		[JsonProperty] int i; // GlobalBorder L
		[JsonProperty] int j; // GlobalBorder R
		[JsonProperty] int k; // GlobalBorder D
		[JsonProperty] int l; // GlobalBorder U

		[JsonProperty] int m; // UvBorder L
		[JsonProperty] int n; // UvBorder R
		[JsonProperty] int o; // UvBorder D
		[JsonProperty] int p; // UvBorder U

		[JsonProperty] int q; // UvBottomLeft L
		[JsonProperty] int r; // UvBottomLeft D
		[JsonProperty] int s; // UvTopRight R
		[JsonProperty] int t; // UvTopRight U

		[JsonProperty] string u;  // Real Name
		[JsonProperty] int v;     // Sheet Name Index
		[JsonProperty] byte w;    // SheetType & GroupType

		// Data
		private Float2? _BottomRight = null;
		private Float2? _TopLeft = null;
		private Byte4? _SummaryTint = default;
		private SheetType? _SheetType = null;
		private GroupType? _GroupType = null;

		// API
		public IRect GetTextureRect (int tWidth, int tHeight) => new(
			(UvBottomLeft.x * tWidth).RoundToInt(),
			(UvBottomLeft.y * tHeight).RoundToInt(),
			((UvTopRight.x - UvBottomLeft.x) * tWidth).RoundToInt(),
			((UvTopRight.y - UvBottomLeft.y) * tHeight).RoundToInt()
		);

		public void GetSlicedUvBorder (Alignment alignment, out Float2 bl, out Float2 br, out Float2 tl, out Float2 tr) {
			bl = UvBottomLeft;
			br = BottomRight;
			tl = TopLeft;
			tr = UvTopRight;
			// Y
			switch (alignment) {
				case Alignment.TopLeft:
				case Alignment.TopMid:
				case Alignment.TopRight:
					bl.y = br.y = Util.LerpUnclamped(TopLeft.y, UvBottomLeft.y, UvBorder.w);
					break;
				case Alignment.MidLeft:
				case Alignment.MidMid:
				case Alignment.MidRight:
					tl.y = tr.y = Util.LerpUnclamped(TopLeft.y, UvBottomLeft.y, UvBorder.w);
					bl.y = br.y = Util.LerpUnclamped(UvBottomLeft.y, TopLeft.y, UvBorder.y);
					break;
				case Alignment.BottomLeft:
				case Alignment.BottomMid:
				case Alignment.BottomRight:
					tl.y = tr.y = Util.LerpUnclamped(UvBottomLeft.y, TopLeft.y, UvBorder.y);
					break;
			}
			// X
			switch (alignment) {
				case Alignment.TopLeft:
				case Alignment.MidLeft:
				case Alignment.BottomLeft:
					br.x = tr.x = Util.LerpUnclamped(UvBottomLeft.x, BottomRight.x, UvBorder.x);
					break;
				case Alignment.TopMid:
				case Alignment.MidMid:
				case Alignment.BottomMid:
					br.x = tr.x = Util.LerpUnclamped(BottomRight.x, UvBottomLeft.x, UvBorder.z);
					bl.x = tl.x = Util.LerpUnclamped(UvBottomLeft.x, BottomRight.x, UvBorder.x);
					break;
				case Alignment.TopRight:
				case Alignment.MidRight:
				case Alignment.BottomRight:
					bl.x = tl.x = Util.LerpUnclamped(BottomRight.x, UvBottomLeft.x, UvBorder.z);
					break;
			}
		}

	}



	[JsonObject(MemberSerialization.OptIn)]
	public class AngeSpriteChain {

		public int this[int i] => Chain[i];
		public int Count => Chain.Count;
		public string Name { get => N; set => N = value; }
		public int ID { get => I; set => I = value; }
		public GroupType Type { get => T; set => T = value; }
		public int LoopStart { get => L; set => L = value; }
		public List<int> Chain { get => C; set => C = value; }

		[JsonProperty] string N;
		[JsonProperty] int I;
		[JsonProperty] GroupType T;
		[JsonProperty] int L = 0;
		[JsonProperty] List<int> C = new(); // Sprite Index in Sheet, Not id

		public List<AngeSprite> GetSpriteChain (SpriteSheet sheet) {
			var result = new List<AngeSprite>();
			foreach (var index in Chain) {
				result.Add(sheet.Sprites[index]);
			}
			return result;
		}

	}


	[JsonObject(MemberSerialization.OptIn)]
	public class SpriteMeta {

		public int Rule { get => R; set => R = value; }
		public int Tag { get => T; set => T = value; }
		public bool IsTrigger { get => G; set => G = value; }

		[JsonProperty] int T = 0;
		[JsonProperty] int R = 0;
		[JsonProperty] bool G = false;

	}



	public class SpriteGroup {
		public int ID;
		public int[] SpriteIDs;
	}



	public class SpriteSheet {
		public AngeSprite[] Sprites = new AngeSprite[0];
		public AngeSpriteChain[] SpriteChains = new AngeSpriteChain[0];
		public SpriteGroup[] Groups = new SpriteGroup[0];
		public SpriteMeta[] Metas = new SpriteMeta[0];
		public string[] SheetNames = new string[0];
	}


}