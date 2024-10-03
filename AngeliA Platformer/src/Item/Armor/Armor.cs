using System.Collections;
using System.Collections.Generic;



using AngeliA;
namespace AngeliA.Platformer;



public enum HelmetWearingMode { Attach, Cover, }



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
			holder is not Character character ||
			character.Rendering is not PoseCharacterRenderer renderer ||
			renderer.TargetCharacter.AnimationType == CharacterAnimationType.Sleep ||
			renderer.TargetCharacter.AnimationType == CharacterAnimationType.PassOut
		) return;

		// Draw
		DrawArmor(renderer);

		// Hide Gadget
		if (HideEar) renderer.EarID.Override(0, 1, priority: 4096);
		if (HideHorn) renderer.HornID.Override(0, 1, priority: 4096);
		if (HideHair) renderer.HairID.Override(0, 1, priority: 4096);
		if (HideTail) renderer.TailID.Override(0, 1, priority: 4096);
		if (HideFace) renderer.FaceID.Override(0, 1, priority: 4096);
		if (HideWing) renderer.WingID.Override(0, 1, priority: 4096);

	}


	protected abstract void DrawArmor (PoseCharacterRenderer renderer);


	public override void OnTakeDamage_FromEquipment (Entity holder, Entity sender, ref int damage) {
		base.OnTakeDamage_FromEquipment(holder, sender, ref damage);
		var progItem = this as IProgressiveItem;
		if (progItem.PrevItemID != 0 && damage > 0) {
			int invID = holder is Character cHolder ? cHolder.InventoryID : holder.TypeID;
			Inventory.GetEquipment(invID, EquipmentType, out int oldEqCount);
			Inventory.SetEquipment(invID, EquipmentType, progItem.PrevItemID, oldEqCount);
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
		int invID = holder is Character character ? character.InventoryID : holder.TypeID;
		int tookCount = Inventory.FindAndTakeItem(invID, materialID, 1);
		if (tookCount <= 0) return false;
		Inventory.GetEquipment(invID, EquipmentType, out int oldEqCount);
		Inventory.SetEquipment(invID, EquipmentType, (this as IProgressiveItem).NextItemID, oldEqCount);
		InvokeItemLost(holder as Character, materialID);
		return true;
	}


}