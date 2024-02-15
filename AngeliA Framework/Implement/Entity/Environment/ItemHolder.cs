using System.Collections;
using System.Collections.Generic;


namespace AngeliA.Framework; 
[EntityAttribute.DontSpawnFromWorld]
[EntityAttribute.ExcludeInMapEditor]
[EntityAttribute.Capacity(1024, 0)]
[EntityAttribute.Layer(EntityLayer.ITEM)]
public class ItemHolder : EnvironmentRigidbody, IActionTarget {




	#region --- VAR ---


	// Const
	public static readonly int TYPE_ID = typeof(ItemHolder).AngeHash();
	private const int ITEM_PHYSICS_SIZE = Const.HALF;
	private const int ITEM_RENDER_SIZE = Const.CEL * 2 / 3;

	// Api
	public delegate void ItemCollectedHandler (Entity collector, int itemID, int count);
	public static event ItemCollectedHandler OnItemCollected;
	protected override int PhysicalLayer => PhysicsLayer.ITEM;
	protected override int CollisionMask => PhysicsMask.MAP;
	public int ItemID { get; set; } = 0;
	public int ItemCount { get; set; } = 1;
	bool IActionTarget.AllowInvokeOnSquat => true;


	#endregion




	#region --- MSG ---


	public override void OnActivated () {
		base.OnActivated();
		Width = ITEM_PHYSICS_SIZE;
		Height = ITEM_PHYSICS_SIZE;
	}


	public override void FillPhysics () => CellPhysics.FillEntity(PhysicsLayer.ITEM, this, true);


	public override void PhysicsUpdate () {
		base.PhysicsUpdate();

		if (ItemID == 0 || ItemCount <= 0) {
			Active = false;
			return;
		}

		// Make Room
		if (IsGrounded) {
			int dir = 0;
			var hits = CellPhysics.OverlapAll(PhysicsMask.ITEM, Rect, out int count, this, OperationMode.TriggerOnly);
			for (int i = 0; i < count; i++) {
				var hit = hits[i];
				if (hit.Entity == null) continue;
				if (hit.Rect.x > X) {
					dir--;
				} else if (hit.Rect.x < X) {
					dir++;
				} else if (hit.Entity.InstanceOrder > InstanceOrder) {
					dir--;
				} else {
					dir++;
				}
			}
			if (dir != 0) {
				PerformMove(dir * 4, 0);
			}
		}
	}


	public override void FrameUpdate () {
		base.FrameUpdate();
		if (!Active) return;
		// Draw
		var rect = new IRect(
			X + Width / 2 - ITEM_RENDER_SIZE / 2,
			Y, ITEM_RENDER_SIZE, ITEM_RENDER_SIZE
		);
		byte rgb = (byte)Util.RemapUnclamped(0, 120, 225, 255, Game.GlobalFrame.PingPong(120));
		var cell = CellRenderer.Draw(
			ItemID,
			rect,
			new Color32(rgb, rgb, rgb, 255)
		);
		// Count
		if (ItemCount > 1 && (PlayerMenuUI.Instance == null || !PlayerMenuUI.Instance.Active)) {
			var labelRect = rect.Shrink(rect.width / 2, 0, 0, rect.height / 2);
			CellRenderer.SetLayerToUI();
			CellRenderer.Draw(Const.PIXEL, labelRect, Color32.BLACK, int.MaxValue);
			CellGUI.Label(CellGUI.GetNumberCache(ItemCount), labelRect, charSize: 20);
		}
		CellRenderer.SetLayerToDefault();
		// Highlight
		if ((this as IActionTarget).IsHighlighted) {
			IActionTarget.HighlightBlink(cell);
		}
	}


	void IActionTarget.Invoke () => Collect(Player.Selecting, onlyAppendExisting: false, ignoreEquipment: false);


	#endregion




	#region --- API ---


	public void Jump (int velocity = 42) {
		VelocityY = velocity;
		Y += velocity;
	}


	public void Collect (Character character, bool onlyAppendExisting = false, bool ignoreEquipment = true) {

		if (ItemID == 0 || character is null) return;
		int invID = character.TypeID;
		if (!Inventory.HasInventory(invID)) return;

		var item = ItemSystem.GetItem(ItemID);
		if (item == null) return;
		int oldItemID = ItemID;
		int oldCount = ItemCount;

		// Equipment Check
		if (
			!onlyAppendExisting &&
			!ignoreEquipment &&
			ItemCount > 0 &&
			ItemSystem.IsEquipment(ItemID, out var equipmentType) &&
			Inventory.GetEquipment(invID, equipmentType) == 0
		) {
			if (Inventory.SetEquipment(invID, equipmentType, ItemID)) {
				ItemCount--;
			}
		}

		// Collect / Append
		int addCount = onlyAppendExisting ?
			Inventory.FindAndAddItem(invID, ItemID, ItemCount) :
			Inventory.CollectItem(invID, ItemID, ItemCount);
		if (addCount > 0) {
			int newCount = ItemCount - addCount;
			if (newCount <= 0) {
				ItemID = 0;
				ItemCount = 0;
				Active = false;
			} else {
				ItemCount = newCount;
			}
			item.OnCollect(character);
		}

		// Particle Hint
		OnItemCollected?.Invoke(character, oldItemID, oldCount - ItemCount);

	}


	#endregion




	#region --- LGC ---



	#endregion




}