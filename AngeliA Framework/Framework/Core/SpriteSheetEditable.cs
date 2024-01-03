using System.Collections;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;


namespace AngeliaFramework {


	// Comparer
	public class AtlasComparer : IComparer<EditableAtlas> {
		public static readonly AtlasComparer Instance = new();
		public int Compare (EditableAtlas a, EditableAtlas b) => a.Order.CompareTo(b.Order);
	}


	public class FlexUnitComparer : IComparer<EditableUnit> {
		public static readonly FlexUnitComparer Instance = new();
		public int Compare (EditableUnit a, EditableUnit b) => a.Order.CompareTo(b.Order);
	}


	public class EditableSpriteComparer : IComparer<EditableSprite> {
		public static readonly EditableSpriteComparer Instance = new();
		public int Compare (EditableSprite a, EditableSprite b) => a.Order.CompareTo(b.Order);
	}


	// Data
	[JsonObject(MemberSerialization.OptIn)]
	public class EditableAtlas {
		public List<EditableUnit> Units = new();
		public string Guid = "";
		public bool IsDirty = false;
		public bool Unfold = false;
		[JsonProperty] public int Order = 0;
		[JsonProperty] public int SheetZ = 0;
		[JsonProperty] public string Name = "";
		[JsonProperty] public SheetType SheetType = SheetType.General;
	}


	[JsonObject(MemberSerialization.OptIn)]
	public class EditableUnit {
		public List<EditableSprite> Sprites = new();
		public string Guid = "";
		public int GlobalID = 0;
		public bool IsDirty = false;
		[JsonProperty] public int Order = 0;
		[JsonProperty] public string Name = "";
		[JsonProperty] public GroupType GroupType = GroupType.General;
	}


	[JsonObject(MemberSerialization.OptIn)]
	public class EditableSprite {

		[JsonProperty] public int Order;
		[JsonProperty] public int GlobalID = 0;
		[JsonProperty] public int AngePivotX;
		[JsonProperty] public int AngePivotY;
		[JsonProperty] public int BorderL;
		[JsonProperty] public int BorderR;
		[JsonProperty] public int BorderD;
		[JsonProperty] public int BorderU;
		[JsonProperty] public int Width;
		[JsonProperty] public int Height;
		[JsonProperty] public bool IsTrigger = false;
		[JsonProperty] public bool NoCollider = false;
		[JsonProperty] public string TagString = "";
		[JsonProperty] public string RuleString = "";
		[JsonProperty] public bool LoopStart = false;
		[JsonProperty] public int OffsetZ = 0;

		public string Guid = "";
		public bool IsDirty = false;
		public Byte4[] Pixels;

		private static readonly StringBuilder NameBuilder = new();

		public string GetFullName (EditableAtlas atlas, EditableUnit unit, int index) {

			NameBuilder.Clear();
			NameBuilder.Append(unit.Name);
			if (index >= 0) {
				NameBuilder.Append(' ');
				NameBuilder.Append(index);
			}

			if (IsTrigger) {
				NameBuilder.Append(" #isTrigger");
			}

			if (!string.IsNullOrWhiteSpace(TagString)) {
				NameBuilder.Append($" #tag={TagString}");
			}

			if (unit.GroupType != GroupType.General) {
				switch (unit.GroupType) {
					case GroupType.Rule:
						if (!string.IsNullOrWhiteSpace(RuleString)) {
							NameBuilder.Append($" #rule={RuleString}");
						}
						break;
					case GroupType.Random:
						NameBuilder.Append(" #ran");
						break;
					case GroupType.Animated:
						NameBuilder.Append(" #ani");
						if (LoopStart) {
							NameBuilder.Append(" #loopStart");
						}
						break;
				}
			}

			if (atlas.SheetType == SheetType.Level && NoCollider) {
				NameBuilder.Append(" #noCollider");
			}

			if (OffsetZ != 0) {
				NameBuilder.Append($" #z={OffsetZ}");
			}

			return NameBuilder.ToString();
		}

	}

}