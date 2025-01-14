using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

[EntityAttribute.Capacity(2048, 0)]
[EntityAttribute.Layer(EntityLayer.ITEM)]
public class ItemHolder : Rigidbody, IAutoTrackWalker {




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
	private const int RENDERING_Z = 35000;
	public static readonly int TYPE_ID = typeof(ItemHolder).AngeHash();
	private const int ITEM_PHYSICS_SIZE = Const.HALF;
	private const int ITEM_RENDER_SIZE = Const.CEL * 2 / 3;
	private static readonly LanguageCode HINT_PICK = ("Hint.PickItem", "Pick Item");

	// Api
	public override int PhysicalLayer => PhysicsLayer.ITEM;
	public override int CollisionMask => PhysicsMask.MAP;
	public int ItemID { get; set; } = 0;
	public int ItemCount { get; set; } = 1;
	public override bool CarryOtherOnTop => false;
	int IAutoTrackWalker.LastWalkingFrame { get; set; }
	int IAutoTrackWalker.WalkStartFrame { get; set; }
	Direction8 IRouteWalker.CurrentDirection { get; set; }
	Int2 IRouteWalker.TargetPosition { get; set; }

	// Data
	private static readonly Dictionary<Int3, Pipe<Int4>> HoldingPool = [];
	private bool TouchingPlayer = false;


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
		TouchingPlayer = false;
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


	public override void FirstUpdate () {
		FillAsTrigger();
		base.FirstUpdate();
	}


	public override void BeforeUpdate () {
		base.BeforeUpdate();
		// Check for Player Collect
		var player = PlayerSystem.Selecting;
		TouchingPlayer = player != null && player.Rect.Overlaps(Rect);
		if (TouchingPlayer) {
			bool collected = false;

			// Auto Collect
			if (Inventory.HasItem(player.InventoryID, ItemID)) {
				collected = Collect(player);
				if (collected) {
					ItemSystem.SetItemUnlocked(ItemID, true);
				}
			}

			// Collect
			if (!collected && player.Movement.IsSquatting) {
				collected = Collect(player);
				if (collected) {
					ItemSystem.SetItemUnlocked(ItemID, true);
				}
			}

			// Hint
			ControlHintUI.AddHint(Gamekey.Down, HINT_PICK);
		}
		// Eject Inside Ground
		if (IsInsideGround) {
			FrameworkUtil.TryEjectOutsideGround(this);
		}
	}


	public override void Update () {
		base.Update();

		if (!Active || ItemID == 0 || ItemCount <= 0) {
			Active = false;
			return;
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

		if (!Active || ItemCount <= 0 || ItemID == 0) return;

		var _item = ItemSystem.GetItem(ItemID);

		// Draw
		var rect = new IRect(
			X + Width / 2 - ITEM_RENDER_SIZE / 2,
			Y, ITEM_RENDER_SIZE, ITEM_RENDER_SIZE
		);
		using (new EnvironmentShadowScope()) {
			_item.DrawItem(rect, Color32.WHITE, RENDERING_Z);
		}

		// Update
		if (_item is Item item) {
			item?.OnItemUpdate_FromItemHolder(this, ItemCount);
		}

		// UI
		if (ItemCount > 1 && !TaskSystem.HasTask() && !PlayerMenuUI.ShowingUI) {
			if (_item is HandTool tool && tool.UseStackAsUsage) {
				// Usage
				FrameworkUtil.DrawItemUsageBar(rect.EdgeDown(rect.height / 4), ItemCount, tool.MaxStackCount);
			} else {
				// Count
				var labelRect = rect.Shrink(rect.width / 2, 0, 0, rect.height / 2);
				var bg = Renderer.DrawPixel(labelRect, Color32.BLACK);
				bg.Z = RENDERING_Z + 1;
				using (new CellZScope(RENDERING_Z + 2)) {
					GUI.IntLabel(labelRect, ItemCount, out var bounds, GUISkin.Default.SmallLabel);
					bg.SetRect(bounds);
				}
			}
		}

	}


	#endregion




	#region --- API ---


	public void Jump (int velocity = 42) {
		VelocityY = velocity;
		Y += velocity;
	}


	public bool Collect (Character character) {

		if (ItemID == 0 || character is null) return false;
		int invID = character.InventoryID;
		if (!Inventory.HasInventory(invID)) return false;

		var item = ItemSystem.GetItem(ItemID);
		if (item == null) return false;
		int oldItemID = ItemID;
		int oldCount = ItemCount;

		// Collect / Append
		if (ItemCount > 0) {
			int addCount = Inventory.CollectItem(
				invID,
				ItemID,
				count: ItemCount,
				ignoreEquipment: false,
				dontCollectIntoEmptyEquipmentSlot: true
			);
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
			} else if (IsGrounded) {
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


	protected override bool InsideGroundCheck () => Physics.Overlap(CollisionMask, IRect.Point(X + OffsetX + Width / 2, Y + OffsetY + Height / 2), this);


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