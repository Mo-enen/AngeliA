using System.Collections;
using System.Collections.Generic;


namespace AngeliA;
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

	// Data
	private int WarningFrame = int.MinValue;


	#endregion




	#region --- MSG ---


	public override void OnActivated () {
		base.OnActivated();
		Width = ITEM_PHYSICS_SIZE;
		Height = ITEM_PHYSICS_SIZE;
		WarningFrame = int.MinValue;
	}


	public override void FirstUpdate () => Physics.FillEntity(PhysicsLayer.ITEM, this, true);


	public override void Update () {
		base.Update();

		if (ItemID == 0 || ItemCount <= 0) {
			Active = false;
			return;
		}

		// Update
		if (ItemSystem.GetItem(ItemID) is Item item) {
			item?.OnItemUpdate_FromGround(this, ItemCount);
		}

		// Make Room
		if (IsGrounded) {
			int dir = 0;
			var hits = Physics.OverlapAll(PhysicsMask.ITEM, Rect, out int count, this, OperationMode.TriggerOnly);
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


	public override void LateUpdate () {

		base.LateUpdate();
		if (!Active) return;

		int renderingOffsetX = 0;
		int redneringSizeOffset = 0;

		// Warning
		bool warning = Game.GlobalFrame < WarningFrame + 42;
		if (warning) {
			float lerp10 = 1f - Ease.OutElastic((Game.GlobalFrame - WarningFrame) / 42f);
			renderingOffsetX = (lerp10 * Const.HALF).RoundToInt();
			redneringSizeOffset = Const.HALF / 4;
		}

		// Draw
		var rect = new IRect(
			X + Width / 2 - ITEM_RENDER_SIZE / 2,
			Y, ITEM_RENDER_SIZE, ITEM_RENDER_SIZE
		);
		var renderingRect = rect.Shift(renderingOffsetX, 0).Expand(redneringSizeOffset);
		var cell = Renderer.Draw(ItemID, renderingRect);
		
		// Count
		if (ItemCount > 1 && (PlayerMenuUI.Instance == null || !PlayerMenuUI.Instance.Active)) {
			var labelRect = rect.Shrink(rect.width / 2, 0, 0, rect.height / 2);
			using (new UILayerScope()) {
				Renderer.DrawPixel(labelRect, Color32.BLACK, int.MaxValue);
				GUI.IntLabel(labelRect, ItemCount);
			}
		}

		// Highlight
		if ((this as IActionTarget).IsHighlighted) {
			IActionTarget.HighlightBlink(cell);
		}
	}


	bool IActionTarget.Invoke () => Collect(Player.Selecting, onlyStackOnExisting: false, ignoreEquipment: false);


	#endregion




	#region --- API ---


	public void Jump (int velocity = 42) {
		VelocityY = velocity;
		Y += velocity;
	}


	public bool Collect (Character character, bool onlyStackOnExisting = false, bool ignoreEquipment = true) {

		if (ItemID == 0 || character is null) return false;
		int invID = character.TypeID;
		if (!Inventory.HasInventory(invID)) return false;

		var item = ItemSystem.GetItem(ItemID);
		if (item == null) return false;
		int oldItemID = ItemID;
		int oldCount = ItemCount;

		// Equipment Check
		if (
			!onlyStackOnExisting &&
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
		if (ItemCount > 0) {
			int addCount = onlyStackOnExisting ?
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
			} else if (!onlyStackOnExisting) {
				// Inventory Full Warning
				WarningFrame = Game.GlobalFrame;
			}
		}

		// Particle Hint
		if (oldCount > ItemCount) {
			OnItemCollected?.Invoke(character, oldItemID, oldCount - ItemCount);
		}

		return oldCount > ItemCount;
	}


	#endregion




	#region --- LGC ---



	#endregion




}