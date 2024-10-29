using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

[EntityAttribute.Capacity(2048, 0)]
[EntityAttribute.Layer(EntityLayer.ITEM)]
public class ItemHolder : Rigidbody, IActionTarget {




	#region --- SUB ---


	private class PipeComparer : IComparer<Int4> {
		public static readonly PipeComparer Instance = new();
		public int Compare (Int4 a, Int4 b) {
			bool validA = a.z != 0 && a.w > 0;
			bool validB = b.z != 0 && b.w > 0;
			return validA == validB ? 0 : validA ? -1 : 1;
		}
	}


	#endregion




	#region --- VAR ---


	// Const
	public static readonly int TYPE_ID = typeof(ItemHolder).AngeHash();
	private const int ITEM_PHYSICS_SIZE = Const.HALF;
	private const int ITEM_RENDER_SIZE = Const.CEL * 2 / 3;

	// Api
	public override int PhysicalLayer => PhysicsLayer.ITEM;
	public override int CollisionMask => PhysicsMask.MAP;
	public int ItemID { get; set; } = 0;
	public int ItemCount { get; set; } = 1;
	bool IActionTarget.AllowInvokeOnSquat => true;


	// Data
	private static readonly Dictionary<Int3, Pipe<Int4>> HoldingPool = [];


	#endregion




	#region --- MSG ---


	[OnGameUpdate]
	internal static void CheckForHoldingPool () {

		if (!ItemSystem.ItemPoolReady || Game.GlobalFrame % 30 != 0) return;

		// Check for Holding Pool
		var allPos = FrameworkUtil.ForAllWorldInRange(Stage.ViewRect.ToUnit(), Stage.ViewZ, out int posCount);
		for (int index = 0; index < posCount; index++) {

			var worldPos = allPos[index];
			if (!HoldingPool.TryGetValue(worldPos, out var pipe) || pipe.Length == 0) continue;

			bool requireSort = false;
			for (int i = 0; i < pipe.Length; i++) {
				var data = pipe[i];
				// Invalid Check
				if (data.z == 0 || data.w <= 0) {
					requireSort = true;
					continue;
				}
				// Spawn if in Range
				if (Stage.ViewRect.Contains(data.x, data.y)) {
					// Spawn
					if (ItemSystem.SpawnItem(data.z, data.x, data.y, data.w, jump: false) == null) {
						break;
					}
					// Clear
					data.z = 0;
					data.w = 0;
					pipe[i] = data;
					requireSort = true;
				}
			}
			if (requireSort) {
				pipe.Sort(PipeComparer.Instance);
			}
		}
	}


	[OnGameRestart]
	internal static void OnGameRestart () => HoldingPool.Clear();


	public override void OnActivated () {
		base.OnActivated();
		Width = ITEM_PHYSICS_SIZE;
		Height = ITEM_PHYSICS_SIZE;
		// Detect Item Element from World
		if (FromWorld) {
			int ele = WorldSquad.Front.GetBlockAt((X + 1).ToUnit(), (Y + 1).ToUnit(), BlockType.Element);
			if (ele != 0) {
				ItemID = ele;
				ItemCount = 1;
			}
		}
	}


	public override void OnInactivated () {
		base.OnInactivated();
		// Hold on Out of Range
		if (!FromWorld && ItemID != 0 && ItemCount > 0) {
			HoldToPool(ItemID, ItemCount, new Int3(X, Y, Stage.ViewZ));
		}
	}


	public override void FirstUpdate () => Physics.FillEntity(PhysicsLayer.ITEM, this, true);


	public override void Update () {
		base.Update();

		if (!Active || ItemID == 0 || ItemCount <= 0) {
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
				if (hit.Entity is not ItemHolder holder) continue;
				if (!FromWorld && !holder.FromWorld && holder.ItemID == ItemID && holder.ItemCount > 0) {
					// Merge
					holder.Active = false;
					ItemCount += holder.ItemCount;
					holder.ItemCount = 0;
					continue;
				}
				if (hit.Rect.x > X) {
					dir--;
				} else if (hit.Rect.x < X) {
					dir++;
				} else if (hit.Entity.InstanceOrder > InstanceOrder) {
					dir--;
				} else if (hit.Entity.InstanceOrder < InstanceOrder) {
					dir++;
				} else {
					dir += Util.QuickRandom(0, 2) == 0 ? 1 : -1;
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

		// Draw
		var rect = new IRect(
			X + Width / 2 - ITEM_RENDER_SIZE / 2,
			Y, ITEM_RENDER_SIZE, ITEM_RENDER_SIZE
		);
		var renderingRect = rect.Shift(renderingOffsetX, 0).Expand(redneringSizeOffset);
		Cell cell;
		if (Renderer.TryGetSprite(ItemID, out var sprite, true) ||
			Renderer.TryGetSpriteFromGroup(ItemID, 0, out sprite)
		) {
			cell = Renderer.Draw(sprite, renderingRect.Fit(sprite));
		} else {
			cell = Renderer.Draw(BuiltInSprite.ICON_ENTITY, renderingRect);
		}

		// Shadow
		FrameworkUtil.DrawEnvironmentShadow(cell);

		// UI
		if (ItemCount > 1 && !TaskSystem.HasTask() && (PlayerMenuUI.Instance == null || !PlayerMenuUI.Instance.Active)) {
			if (ItemSystem.GetItem(ItemID) is HandTool wItem && wItem.UseStackAsUsage) {
				// Usage
				FrameworkUtil.DrawItemUsageBar(rect.EdgeDown(rect.height / 4), ItemCount, wItem.MaxStackCount);
			} else {
				// Count
				var labelRect = rect.Shrink(rect.width / 2, 0, 0, rect.height / 2);
				using (new UILayerScope()) {
					Renderer.DrawPixel(labelRect, Color32.BLACK);
					GUI.IntLabel(labelRect, ItemCount, GUISkin.Default.SmallLabel);
				}
			}
		}

		// Highlight
		(this as IActionTarget).BlinkIfHighlight(cell);
	}


	bool IActionTarget.Invoke () {
		var player = PlayerSystem.Selecting;
		bool collected = Collect(player, onlyStackOnExisting: false, ignoreEquipment: false);
		if (collected) {
			ItemSystem.SetItemUnlocked(ItemID, true);
		}
		return collected || player.EquippingToolType == ToolType.Throwing;
	}


	#endregion




	#region --- API ---


	public void Jump (int velocity = 42) {
		VelocityY = velocity;
		Y += velocity;
	}


	public bool Collect (Character character, bool onlyStackOnExisting = false, bool ignoreEquipment = false) {

		if (ItemID == 0 || character is null) return false;
		int invID = character.InventoryID;
		if (!Inventory.HasInventory(invID)) return false;

		var item = ItemSystem.GetItem(ItemID);
		if (item == null) return false;
		int oldItemID = ItemID;
		int oldCount = ItemCount;

		// Collect / Append
		if (ItemCount > 0) {
			int addCount = onlyStackOnExisting ?
				Inventory.FindAndAddItem(invID, ItemID, ItemCount, ignoreEquipment: false) :
				Inventory.CollectItem(invID, ItemID, ItemCount, ignoreEquipment);
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
			} else if (!onlyStackOnExisting && IsGrounded) {
				// Inventory is Full
				Jump();
			}
		}

		bool collected = oldCount > ItemCount;
		if (collected) {
			// Particle Hint
			FrameworkUtil.InvokeItemCollected(character, oldItemID, oldCount - ItemCount);
			// Remove from Map
			if (FromWorld) {
				FrameworkUtil.RemoveFromWorldMemory(this);
			}
		}

		return collected;
	}


	public static void ClearHoldingPool () => HoldingPool.Clear();


	#endregion




	#region --- LGC ---


	private static void HoldToPool (int id, int count, Int3 globalPos) {

		int unitX = globalPos.x.ToUnit();
		int unitY = globalPos.y.ToUnit();
		var worldPos = new Int3(
			unitX.UDivide(Const.MAP),
			unitY.UDivide(Const.MAP),
			globalPos.z
		);

		// Get or Create Data
		if (!HoldingPool.TryGetValue(worldPos, out var pipe)) {
			pipe = new Pipe<Int4>(256);
			HoldingPool.Add(worldPos, pipe);
		}

		// Remove if Full
		if (pipe.Length == pipe.Capacity) {
			pipe.TryPopHead(out _);
		}

		// Add to Data
		pipe.LinkToTail(new Int4(globalPos.x, globalPos.y, id, count));

	}


	#endregion




}