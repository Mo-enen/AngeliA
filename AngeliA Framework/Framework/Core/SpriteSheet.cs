using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;


namespace AngeliaFramework {


	public enum AtlasType {
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



	[JsonObject(MemberSerialization.OptIn)]
	public class AngeSprite {
		const float UV_SCALE = 10000000f;
		public int GlobalID;
		public int SortingZ;
		public FRect UvRect;
		public Float2 BottomRight;
		public Float2 TopLeft;
		public Byte4 SummaryTint;
		public Int4 GlobalBorder;
		public Float4 UvBorder; // xyzw => ldru
		public Float2 UvBottomLeft;
		public Float2 UvTopRight;
		public AtlasInfo Atlas;
		public GroupType GroupType;
		public bool IsTrigger;
		[JsonProperty("a")] public int GlobalWidth;
		[JsonProperty("b")] public int GlobalHeight;
		[JsonProperty("c")] public int LocalZ;
		[JsonProperty("d")] public int PivotX;
		[JsonProperty("e")] public int PivotY;
		[JsonProperty("f")] public int IsTriggerInt;
		[JsonProperty("g")] public int SummaryTintInt;
		[JsonProperty("h")] public int GlobalBorderL;
		[JsonProperty("i")] public int GlobalBorderR;
		[JsonProperty("j")] public int GlobalBorderD;
		[JsonProperty("k")] public int GlobalBorderU;
		[JsonProperty("l")] public int UvBorderL;
		[JsonProperty("m")] public int UvBorderR;
		[JsonProperty("n")] public int UvBorderD;
		[JsonProperty("o")] public int UvBorderU;
		[JsonProperty("p")] public int UvBottomLeftL;
		[JsonProperty("q")] public int UvBottomLeftD;
		[JsonProperty("r")] public int UvTopRightR;
		[JsonProperty("s")] public int UvTopRightU;
		[JsonProperty("t")] public string RealName;
		[JsonProperty("u")] public int AtlasIndex;
		[JsonProperty("v")] public int Rule;
		[JsonProperty("w")] public int Tag;

		public void Apply () {
			GlobalID = RealName.AngeHash();
			SortingZ = Atlas.AtlasZ * 1024 + LocalZ;
			UvRect = FRect.MinMaxRect(UvBottomLeftL / UV_SCALE, UvBottomLeftD / UV_SCALE, UvTopRightR / UV_SCALE, UvTopRightU / UV_SCALE);
			BottomRight = new(UvTopRightR / UV_SCALE, UvBottomLeftD / UV_SCALE);
			TopLeft = new(UvBottomLeftL / UV_SCALE, UvTopRightU / UV_SCALE);
			UvBorder = new(UvBorderL / UV_SCALE, UvBorderD / UV_SCALE, UvBorderR / UV_SCALE, UvBorderU / UV_SCALE);
			UvBottomLeft = new(UvBottomLeftL / UV_SCALE, UvBottomLeftD / UV_SCALE);
			UvTopRight = new(UvTopRightR / UV_SCALE, UvTopRightU / UV_SCALE);
			SummaryTint = Util.IntToColor(SummaryTintInt);
			GlobalBorder = new(GlobalBorderL, GlobalBorderR, GlobalBorderD, GlobalBorderU);
			IsTrigger = IsTriggerInt == 1;
		}

		public void Revert () {
			UvBottomLeftD = (int)(UvBottomLeft.y * UV_SCALE);
			UvBottomLeftL = (int)(UvBottomLeft.x * UV_SCALE);
			UvTopRightU = (int)(UvTopRight.y * UV_SCALE);
			UvTopRightR = (int)(UvTopRight.x * UV_SCALE);
			UvBorderL = (int)(UvBorder.x * UV_SCALE);
			UvBorderR = (int)(UvBorder.z * UV_SCALE);
			UvBorderD = (int)(UvBorder.y * UV_SCALE);
			UvBorderU = (int)(UvBorder.w * UV_SCALE);
			SummaryTintInt = Util.ColorToInt(SummaryTint);
			GlobalBorderL = GlobalBorder.left;
			GlobalBorderR = GlobalBorder.right;
			GlobalBorderD = GlobalBorder.down;
			GlobalBorderU = GlobalBorder.up;
			IsTriggerInt = IsTrigger ? 1 : 0;
		}

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
	public class SpriteGroup {
		public AngeSprite this[int i] => Sprites[i];
		public int Length => Sprites.Length;
		[JsonProperty("a")] public int ID;
		[JsonProperty("b")] public int LoopStart = 0;
		[JsonProperty("c")] public GroupType Type = GroupType.General;
		[JsonProperty("d")] public int[] SpriteIDs;
		public AngeSprite[] Sprites;
	}



	[JsonObject(MemberSerialization.OptIn)]
	public class AtlasInfo {
		[JsonProperty("a")] public string Name;
		[JsonProperty("b")] public AtlasType Type;
		[JsonProperty("c")] public int AtlasZ;
	}


	[JsonObject(MemberSerialization.OptIn)]
	public class Sheet : IJsonSerializationCallback {

		[JsonProperty] public AngeSprite[] Sprites;
		[JsonProperty] public SpriteGroup[] Groups;
		[JsonProperty] public AtlasInfo[] AtlasInfo;
		public readonly Dictionary<int, AngeSprite> SpritePool = new();
		public readonly Dictionary<int, SpriteGroup> GroupPool = new();

		public void Clear () {
			Sprites = new AngeSprite[0];
			Groups = new SpriteGroup[0];
			AtlasInfo = new AtlasInfo[0];
			SpritePool.Clear();
			GroupPool.Clear();
		}

		public void Apply () {

			// Add Sprites
			for (int i = 0; i < Sprites.Length; i++) {
				var sp = Sprites[i];
				sp.Atlas = AtlasInfo[sp.AtlasIndex];
				sp.Apply();
				SpritePool.TryAdd(sp.GlobalID, Sprites[i]);
			}

			// Add Sprite Groups
			for (int i = 0; i < Groups.Length; i++) {
				var group = Groups[i];
				if (group == null) continue;
				if (group.SpriteIDs != null && group.SpriteIDs.Length > 0) {
					group.Sprites = new AngeSprite[group.SpriteIDs.Length];
					for (int j = 0; j < group.SpriteIDs.Length; j++) {
						int id = group.SpriteIDs[j];
						if (SpritePool.TryGetValue(id, out var sp)) {
							sp.GroupType = group.Type;
							group.Sprites[j] = sp;
						}
					}
					GroupPool.TryAdd(group.ID, group);
				} else {
					group.Sprites = new AngeSprite[0];
				}
			}
		}

		void IJsonSerializationCallback.OnBeforeSaveToDisk () { }

		void IJsonSerializationCallback.OnAfterLoadedFromDisk () => Apply();

	}






	public class FlexSprite {
		public string Name;
		public Int2 AngePivot;
		public Float4 Border;
		public FRect Rect;
		public int AtlasZ;
		public string AtlasName;
		public AtlasType AtlasType;
	}




}