using System.Collections;
using System.Collections.Generic;



namespace AngeliA;



public enum HelmetWearingMode { Attach, Cover, }


public abstract class Helmet<P, N> : Armor<P, N> where P : Equipment where N : Equipment {
	public sealed override EquipmentType EquipmentType => EquipmentType.Helmet;
	private int SpriteFront { get; init; } = 0;
	private int SpriteBack { get; init; } = 0;
	protected abstract HelmetWearingMode WearingMode { get; }
	public Helmet () {
		string basicName = GetType().AngeName();
		SpriteFront = $"{basicName}.Main".AngeHash();
		SpriteBack = $"{basicName}.Back".AngeHash();
		if (!Renderer.HasSprite(SpriteFront)) SpriteFront = 0;
		if (!Renderer.HasSprite(SpriteBack)) SpriteBack = 0;
	}
	protected override void DrawArmor (PoseCharacter character) {

		var head = character.Head;
		int spriteID = head.FrontSide ? SpriteFront : SpriteBack;
		if (spriteID == 0 || !Renderer.TryGetSprite(spriteID, out var sprite)) return;

		// Draw Helmet
		switch (WearingMode) {
			case HelmetWearingMode.Attach:
				// Attach
				Cloth.AttachClothOn(
					head, sprite, 500, 1000, 34 - head.Z, Scale, Scale, defaultHideLimb: false
				);
				break;
			default: {
				// Cover
				var cells = Cloth.CoverClothOn(head, spriteID, 34 - head.Z, Color32.WHITE, false);
				// Grow Padding
				if (!sprite.GlobalBorder.IsZero && cells != null) {
					var center = head.GetGlobalCenter();
					int widthAbs = head.Width.Abs();
					int heightAbs = head.Height.Abs();
					float scaleX = (widthAbs + sprite.GlobalBorder.horizontal) / (float)widthAbs.GreaterOrEquel(1);
					float scaleY = (heightAbs + sprite.GlobalBorder.vertical) / (float)heightAbs.GreaterOrEquel(1);
					foreach (var cell in cells) {
						cell.ReturnPosition(center.x, center.y);
						cell.Width = (int)(cell.Width * scaleX);
						cell.Height = (int)(cell.Height * scaleY);
					}
				}
				break;
			}
		}

	}
}



public abstract class BodyArmor<P, N> : Armor<P, N> where P : Equipment where N : Equipment {
	public sealed override EquipmentType EquipmentType => EquipmentType.BodyArmor;
	private int BodyId { get; init; } = 0;
	private int BodyIdAlt { get; init; } = 0;
	private int HipID { get; init; } = 0;
	private int ShoulderID { get; init; } = 0;
	private int UpperArmID { get; init; } = 0;
	private int LowerArmID { get; init; } = 0;
	private int UpperLegID { get; init; } = 0;
	private int LowerLegID { get; init; } = 0;
	private readonly bool IsSkirt = false;
	public BodyArmor () {
		string basicName = GetType().AngeName();
		IsSkirt = false;
		BodyId = BodyIdAlt = $"{basicName}.Body".AngeHash();
		HipID = $"{basicName}.Hip".AngeHash();
		ShoulderID = $"{basicName}.Shoulder".AngeHash();
		UpperArmID = $"{basicName}.UpperArm".AngeHash();
		LowerArmID = $"{basicName}.LowerArm".AngeHash();
		UpperLegID = $"{basicName}.UpperLeg".AngeHash();
		LowerLegID = $"{basicName}.LowerLeg".AngeHash();
		if (!Renderer.HasSpriteGroup(BodyId)) {
			BodyId = $"{basicName}.BodyL".AngeHash();
			BodyIdAlt = $"{basicName}.BodyR".AngeHash();
		}
		if (!Renderer.HasSpriteGroup(HipID) && !Renderer.HasSprite(HipID)) {
			HipID = $"{basicName}.Skirt".AngeHash();
			IsSkirt = true;
		}
		if (!Renderer.HasSprite(BodyId) && !Renderer.HasSpriteGroup(BodyId)) BodyId = 0;
		if (!Renderer.HasSprite(BodyIdAlt) && !Renderer.HasSpriteGroup(BodyIdAlt)) BodyIdAlt = 0;
		if (!Renderer.HasSprite(HipID)) HipID = 0;
		if (!Renderer.HasSprite(ShoulderID)) ShoulderID = 0;
		if (!Renderer.HasSprite(UpperArmID)) UpperArmID = 0;
		if (!Renderer.HasSprite(LowerArmID)) LowerArmID = 0;
		if (!Renderer.HasSprite(UpperLegID)) UpperLegID = 0;
		if (!Renderer.HasSprite(LowerLegID)) LowerLegID = 0;
	}
	protected override void DrawArmor (PoseCharacter character) {

		// Body
		if (BodyId != 0 || BodyIdAlt != 0) {
			BodyCloth.DrawClothForBody(character, BodyId, BodyIdAlt, 8, 200);
		}

		// Hip
		if (HipID != 0) {
			if (IsSkirt) {
				HipCloth.DrawClothForSkirt(character, HipID, 7);
			} else {
				HipCloth.DrawClothForHip(character, HipID, 2);
			}
		}

		// Cape
		BodyCloth.DrawCape(character, BodyId);

		// Shoulder
		if (ShoulderID != 0 && Renderer.TryGetSprite(ShoulderID, out var shoulderSprite)) {
			Cloth.AttachClothOn(character.ShoulderL, shoulderSprite, 1000, 1000, 3);
			Cloth.AttachClothOn(character.ShoulderR, shoulderSprite, 1000, 1000, 3);
		}

		// Arm
		BodyCloth.DrawClothForUpperArm(character, UpperArmID, 3);
		BodyCloth.DrawClothForLowerArm(character, LowerArmID, 3);

		// Leg
		HipCloth.DrawClothForUpperLeg(character, UpperLegID, 3);
		HipCloth.DrawClothForLowerLeg(character, LowerLegID, 3);

	}
}



public abstract class Gloves<P, N> : Armor<P, N> where P : Equipment where N : Equipment {
	public sealed override EquipmentType EquipmentType => EquipmentType.Gloves;
	private int SpriteID { get; init; } = 0;
	public Gloves () {
		string basicName = GetType().AngeName();
		SpriteID = $"{basicName}.Main".AngeHash();
		if (!Renderer.HasSprite(SpriteID)) SpriteID = 0;
	}
	protected override void DrawArmor (PoseCharacter character) => HandCloth.DrawClothForHand(character, SpriteID, 2);
}



public abstract class Shoes<P, N> : Armor<P, N> where P : Equipment where N : Equipment {
	public sealed override EquipmentType EquipmentType => EquipmentType.Shoes;
	private int SpriteID { get; init; } = 0;
	public Shoes () {
		string basicName = GetType().AngeName();
		SpriteID = $"{basicName}.Main".AngeHash();
		if (!Renderer.HasSprite(SpriteID)) SpriteID = 0;
	}
	protected override void DrawArmor (PoseCharacter character) => FootCloth.DrawClothForFoot(character, SpriteID, 2);
}



public abstract class Armor<P, N> : Equipment, IProgressiveItem where P : Equipment where N : Equipment {

	protected virtual System.Type[] RepairMaterials => null;
	protected virtual int Scale => 1000;
	protected virtual bool HideEar => false;
	protected virtual bool HideHorn => false;
	protected virtual bool HideHair => false;
	protected virtual bool HideTail => false;
	protected virtual bool HideFace => false;
	protected virtual bool HideWing => false;
	int IProgressiveItem.Progress { get; set; } = 0;
	int IProgressiveItem.TotalProgress { get; set; } = 1;
	int IProgressiveItem.PrevItemID { get; set; } = 0;
	int IProgressiveItem.NextItemID { get; set; } = 0;

	private readonly int[] RepairMaterialsID;


	public Armor () {
		var progItem = this as IProgressiveItem;
		progItem.PrevItemID = typeof(P).AngeHash();
		progItem.NextItemID = typeof(N).AngeHash();
		progItem.Progress = GetProgress(GetType(), out int totalProgressive);
		progItem.TotalProgress = totalProgressive;
		if (progItem.PrevItemID == TypeID) progItem.PrevItemID = 0;
		if (progItem.NextItemID == TypeID) progItem.NextItemID = 0;
		RepairMaterialsID = GetAngeHashs(RepairMaterials);
		// Func
		static int GetProgress (System.Type armorType, out int totalProgress) {
			int progressive = 0;
			totalProgress = 1;
			// Backward
			var type = armorType;
			for (int safe = 0; safe < 1024; safe++) {
				var genericArgs = type.BaseType.GenericTypeArguments;
				if (genericArgs.Length < 2 || genericArgs[0] == type) break;
				type = genericArgs[0];
				totalProgress++;
				progressive++;
#if DEBUG
				if (safe == 1023) Debug.LogWarning($"Armor {armorType} is having a progressive loop.");
#endif
			}
			// Forward
			type = armorType;
			for (int safe = 0; safe < 1024; safe++) {
				var genericArgs = type.BaseType.GenericTypeArguments;
				if (genericArgs.Length < 2 || genericArgs[1] == type) break;
				type = genericArgs[1];
				totalProgress++;
#if DEBUG
				if (safe == 1023) Debug.LogWarning($"Armor {armorType} is having a progressive loop.");
#endif
			}
			return progressive;
		}
		// Func
		static int[] GetAngeHashs (System.Type[] types) {
			if (types == null || types.Length == 0) return [];
			var results = new int[types.Length];
			for (int i = 0; i < results.Length; i++) {
				var _type = types[i];
				results[i] = _type != null ? _type.AngeHash() : 0;
			}
			return results;
		}
	}


	public override void PoseAnimationUpdate_FromEquipment (Entity holder) {
		base.PoseAnimationUpdate_FromEquipment(holder);

		// Gate
		if (
			holder is not PoseCharacter character ||
			character.AnimationType == CharacterAnimationType.Sleep ||
			character.AnimationType == CharacterAnimationType.PassOut
		) return;

		// Draw
		DrawArmor(character);

		// Hide Gadget
		if (HideEar) character.EarID.Override(0, 1, priority: 4096);
		if (HideHorn) character.HornID.Override(0, 1, priority: 4096);
		if (HideHair) character.HairID.Override(0, 1, priority: 4096);
		if (HideTail) character.TailID.Override(0, 1, priority: 4096);
		if (HideFace) character.FaceID.Override(0, 1, priority: 4096);
		if (HideWing) character.WingID.Override(0, 1, priority: 4096);

	}


	protected abstract void DrawArmor (PoseCharacter character);


	public override void OnTakeDamage_FromEquipment (Entity holder, Entity sender, ref int damage) {
		base.OnTakeDamage_FromEquipment(holder, sender, ref damage);
		var progItem = this as IProgressiveItem;
		if (progItem.PrevItemID != 0 && damage > 0) {
			Inventory.GetEquipment(holder.TypeID, EquipmentType, out int oldEqCount);
			Inventory.SetEquipment(holder.TypeID, EquipmentType, progItem.PrevItemID, oldEqCount);
			InvokeItemDamage(holder as Character, TypeID, progItem.PrevItemID);
			damage--;
		}
	}


	public override bool TryRepair (Entity holder) {
		base.TryRepair(holder);
		if ((this as IProgressiveItem).NextItemID == 0) return false;
		foreach (int materialID in RepairMaterialsID) {
			if (RepairArmor(holder, materialID)) {
				return true;
			}
		}
		return false;
	}


	public virtual bool RepairArmor (Entity holder, int materialID) {
		if (materialID == 0) return false;
		int tookCount = Inventory.FindAndTakeItem(holder.TypeID, materialID, 1);
		if (tookCount <= 0) return false;
		Inventory.GetEquipment(holder.TypeID, EquipmentType, out int oldEqCount);
		Inventory.SetEquipment(holder.TypeID, EquipmentType, (this as IProgressiveItem).NextItemID, oldEqCount);
		InvokeItemLost(holder as Character, materialID);
		return true;
	}


}