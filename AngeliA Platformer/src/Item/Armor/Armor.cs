using System.Collections;
using System.Collections.Generic;



using AngeliA;

namespace AngeliA.Platformer;

/// <summary>
/// Equipment as body protection
/// </summary>
/// <typeparam name="P">Type of the item this armor will become after take damage for once</typeparam>
/// <typeparam name="N">Type of the item this armor will become after being repair for once</typeparam>
public abstract class Armor<P, N> : Equipment, IProgressiveItem where P : Equipment where N : Equipment {

	/// <summary>
	/// Items required for repair this armor. System will find any available item from start to end inside this array, and only consume one item to repair for once.
	/// </summary>
	protected virtual System.Type[] RepairMaterials => null;
	/// <summary>
	/// Size scale for rendering only
	/// </summary>
	protected virtual int Scale => 1000;
	/// <summary>
	/// True if the armor hide ear gadget 
	/// </summary>
	protected virtual bool HideEar => false;
	/// <summary>
	/// True if the armor hide horn gadget
	/// </summary>
	protected virtual bool HideHorn => false;
	/// <summary>
	/// True if the armor hide hair gadget
	/// </summary>
	protected virtual bool HideHair => false;
	/// <summary>
	/// True if the armor hide tail gadget
	/// </summary>
	protected virtual bool HideTail => false;
	/// <summary>
	/// True if the armor hide face gadget
	/// </summary>
	protected virtual bool HideFace => false;
	/// <summary>
	/// True if the armor hide wing gadget
	/// </summary>
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


	public override void OnPoseAnimationUpdate_FromEquipment (PoseCharacterRenderer rendering) {
		base.OnPoseAnimationUpdate_FromEquipment(rendering);

		// Gate
		if (
			rendering.TargetCharacter.AnimationType == CharacterAnimationType.Sleep ||
			rendering.TargetCharacter.AnimationType == CharacterAnimationType.PassOut
		) return;

		// Draw
		DrawArmor(rendering);

		// Hide Gadget
		if (HideEar) rendering.EarID.Override(0, 1, priority: 4096);
		if (HideHorn) rendering.HornID.Override(0, 1, priority: 4096);
		if (HideHair) rendering.HairID.Override(0, 1, priority: 4096);
		if (HideTail) rendering.TailID.Override(0, 1, priority: 4096);
		if (HideFace) rendering.FaceID.Override(0, 1, priority: 4096);
		if (HideWing) rendering.WingID.Override(0, 1, priority: 4096);

	}


	/// <summary>
	/// Render the armor onto given character
	/// </summary>
	protected abstract void DrawArmor (PoseCharacterRenderer renderer);


	public override void OnTakeDamage_FromEquipment (Character character, ref Damage damage) {
		base.OnTakeDamage_FromEquipment(character, ref damage);
		var progItem = this as IProgressiveItem;
		if (progItem.PrevItemID != 0 && damage.Amount > 0) {
			int invID = character.InventoryID;
			Inventory.GetEquipment(invID, EquipmentType, out int oldEqCount);
			Inventory.SetEquipment(invID, EquipmentType, progItem.PrevItemID, oldEqCount);
			FrameworkUtil.InvokeItemDamage(character, TypeID, progItem.PrevItemID);
			damage.Amount--;
		}
	}


	public override bool TryRepairEquipment (Character character) {
		base.TryRepairEquipment(character);
		if ((this as IProgressiveItem).NextItemID == 0) return false;
		foreach (int materialID in RepairMaterialsID) {
			if (RepairArmor(character, materialID)) {
				return true;
			}
		}
		return false;
	}


	/// <summary>
	/// Repair the armor for once
	/// </summary>
	/// <param name="character">Character that holding this armor</param>
	/// <param name="materialID">Item that will get consume</param>
	/// <returns>True if the armor get repair for once</returns>
	public virtual bool RepairArmor (Character character, int materialID) {
		if (materialID == 0) return false;
		int invID = character.InventoryID;
		int tookCount = Inventory.FindAndTakeItem(invID, materialID, 1);
		if (tookCount <= 0) return false;
		Inventory.GetEquipment(invID, EquipmentType, out int oldEqCount);
		Inventory.SetEquipment(invID, EquipmentType, (this as IProgressiveItem).NextItemID, oldEqCount);
		FrameworkUtil.InvokeItemLost(character, materialID);
		return true;
	}


}