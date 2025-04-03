using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA.Platformer;


/// <summary>
/// Entity that get collect when player touchs
/// </summary>
[EntityAttribute.Layer(EntityLayer.ENVIRONMENT)]
[EntityAttribute.MapEditorGroup("Collectable")]
public abstract class Collectable : Entity, IBlockEntity {

	// VAR
	/// <summary>
	/// Item it give to player when get collected
	/// </summary>
	protected virtual int ItemID => TypeID;
	/// <summary>
	/// How many item does it give at once
	/// </summary>
	protected virtual int ItemCount => 1;
	private Int4 TriggerExpand;

	// MSG
	public override void OnActivated () {
		base.OnActivated();
		TriggerExpand = default;
		if (Renderer.TryGetSprite(TypeID, out var sprite, false)) {
			TriggerExpand.left = TriggerExpand.right = (sprite.GlobalWidth - Const.CEL) / 2;
			TriggerExpand.down = TriggerExpand.up = (sprite.GlobalHeight - Const.CEL) / 2;
		}
	}

	public override void FirstUpdate () {
		base.FirstUpdate();
		Physics.FillEntity(PhysicsLayer.ENVIRONMENT, this, true);
	}

	public override void Update () {
		base.Update();
		if (Physics.GetEntity<Character>(
			Rect.Expand(TriggerExpand),
			PhysicsMask.CHARACTER, null, OperationMode.ColliderOnly
		) is Character character) {
			OnCollect(character);
		}
	}

	public override void LateUpdate () {
		base.LateUpdate();
		Renderer.Draw(TypeID, Rect.Expand(TriggerExpand));
	}

	/// <summary>
	/// Make this collectable be collect by given character
	/// </summary>
	/// <returns>True if sucessfuly collected</returns>
	public virtual bool OnCollect (Character collector) {
		Active = false;
		FrameworkUtil.RemoveFromWorldMemory(this);
		return !IgnoreReposition && Inventory.GiveItemToTarget(collector, ItemID, ItemCount);
	}

	/// <summary>
	/// True if given character can collect this collectable
	/// </summary>
	protected virtual bool AllowCollect (Character collector) => collector.InventoryType != CharacterInventoryType.None;

}