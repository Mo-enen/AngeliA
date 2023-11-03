using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	[EntityAttribute.DontSpawnFromWorld]
	[EntityAttribute.ExcludeInMapEditor]
	[EntityAttribute.Capacity(1024, 0)]
	[EntityAttribute.Layer(Const.ENTITY_LAYER_ITEM)]
	public class ItemHolder : Entity, IActionTarget {




		#region --- VAR ---


		// Const
		public static readonly int TYPE_ID = typeof(ItemHolder).AngeHash();
		private const int ITEM_PHYSICS_SIZE = Const.HALF;
		private const int ITEM_RENDER_SIZE = Const.CEL * 2 / 3;
		private const int GRAVITY = 5;
		private const int MAX_GRAVITY_SPEED = 64;

		// Api
		public static int CollectParticleID { get; set; } = typeof(ItemCollectParticle).AngeHash();
		public int ItemID { get; set; } = 0;
		public int ItemCount { get; set; } = 1;
		bool IActionTarget.AllowInvokeOnSquat => true;

		// Data
		private int VelocityY = 0;
		private bool MakingRoom = false;


		#endregion




		#region --- MSG ---


		public override void OnActivated () {
			base.OnActivated();
			Width = ITEM_PHYSICS_SIZE;
			Height = ITEM_PHYSICS_SIZE;
			MakingRoom = false;
		}


		public override void FillPhysics () {
			base.FillPhysics();
			CellPhysics.FillEntity(Const.LAYER_ITEM, this, true);
		}


		public override void PhysicsUpdate () {
			base.PhysicsUpdate();

			if (ItemID == 0 || ItemCount <= 0) {
				Active = false;
				return;
			}

			// Fall
			bool grounded =
				VelocityY <= 0 &&
				(!CellPhysics.RoomCheck(Const.MASK_MAP, Rect, this, Direction4.Down) ||
				!CellPhysics.RoomCheckOneway(Const.MASK_MAP, Rect, this, Direction4.Down));
			if (!grounded) {
				if (VelocityY != 0) {
					var rect = Rect;
					rect.position = CellPhysics.Move(
						Const.MASK_MAP, rect.position, 0, VelocityY, rect.size, this, out _, out bool stopY
					);
					Y = rect.y;
					if (stopY) VelocityY = 0;
				}
				VelocityY = Mathf.Clamp(VelocityY - GRAVITY, -MAX_GRAVITY_SPEED, MAX_GRAVITY_SPEED);
				MakingRoom = true;
			} else {
				VelocityY = 0;
			}

			// Make Room
			if (grounded) {
				var makeRoomRect = Rect.Expand(Const.HALF / 2, Const.HALF / 2, 0, 0);
				if (!MakingRoom) {
					MakingRoom =
						(Game.GlobalFrame - SpawnFrame) % 30 == 0 ||
						CellPhysics.Overlap(Const.MASK_ITEM, makeRoomRect, this, OperationMode.TriggerOnly);
				}
				if (
					MakingRoom &&
					!MakeRoomFromItems(makeRoomRect) &&
					!MakeRoomFromSlope(makeRoomRect)
				) {
					MakingRoom = false;
				}

			}
		}


		public override void FrameUpdate () {
			base.FrameUpdate();
			if (!Active) return;
			// Draw
			var rect = new RectInt(
				X + Width / 2 - ITEM_RENDER_SIZE / 2,
				Y, ITEM_RENDER_SIZE, ITEM_RENDER_SIZE
			);
			byte rgb = (byte)Util.RemapUnclamped(0, 120, 225, 255, Game.GlobalFrame.PingPong(120));
			var cell = CellRenderer.Draw(
				CellRenderer.HasSprite(ItemID) ? ItemID : Const.PIXEL,
				rect,
				new Color32(rgb, rgb, rgb, 255)
			);
			// Count
			if (ItemCount > 1 && (PlayerMenuUI.Instance == null || !PlayerMenuUI.Instance.Active)) {
				var labelRect = rect.Shrink(rect.width / 2, 0, 0, rect.height / 2);
				CellRenderer.SetLayerToUI();
				CellRenderer.Draw(Const.PIXEL, labelRect, Const.BLACK, int.MaxValue);
				CellRendererGUI.Label(
					CellContent.Get(CellRendererGUI.GetNumberCache(ItemCount)), labelRect
				);
			}
			CellRenderer.SetLayerToDefault();
			// Highlight
			if ((this as IActionTarget).IsHighlighted) {
				IActionTarget.HighlightBlink(cell);
			}
		}


		void IActionTarget.Invoke () => Collect(Player.Selecting, false);


		#endregion




		#region --- API ---


		public void Jump (int velocity = 96) {
			VelocityY = velocity;
			Y += velocity;
		}


		public void Collect (Player player, bool append = false) {

			if (ItemID == 0 || player is null) return;
			int invID = player.TypeID;
			if (!Inventory.HasInventory(invID)) return;

			var item = ItemSystem.GetItem(ItemID);
			if (item == null) return;
			int oldItemID = ItemID;
			int oldCount = ItemCount;

			// Collect / Append
			int addCount = append ?
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
				item.OnCollect(player);
			}

			// Particle Hint
			if (CollectParticleID != 0 && oldCount != ItemCount) {
				if (Stage.SpawnEntity(
					CollectParticleID,
					player.X,
					player.Y + Const.CEL * 2
				) is Particle particle) {
					particle.UserData = oldItemID;
				}
			}

		}


		#endregion




		#region --- LGC ---


		private bool MakeRoomFromItems (RectInt roomRect) {
			var hits = CellPhysics.OverlapAll(
				Const.MASK_ITEM, roomRect, out int count,
				this, OperationMode.TriggerOnly
			);
			int pressureL = 0;
			int pressureR = 0;
			for (int i = 0; i < count; i++) {
				var hit = hits[i];
				if (hit.Entity is not ItemHolder hitItem) continue;
				int dis = (hitItem.X - X).Abs();
				if (hitItem.X > X) {
					pressureR += roomRect.width - dis;
				} else {
					pressureL += roomRect.width - dis;
				}
			}
			if (pressureL + pressureR != 0 && pressureL != pressureR) {
				var rect = Rect;
				rect.position = CellPhysics.MoveIgnoreOneway(
					Const.MASK_MAP, rect.position,
					(pressureL - pressureR).Sign3() * 6, 0,
					rect.size, this
				);
				X = rect.x;
				Y = Mathf.Min(rect.y, Y);
				return true;
			}
			return false;
		}


		private bool MakeRoomFromSlope (RectInt roomRect) {
			var slope = CellPhysics.GetEntity<Slope>(roomRect, Const.MASK_MAP, this, OperationMode.TriggerOnly);
			if (slope == null) return false;
			var rect = Rect;
			rect.position = CellPhysics.MoveIgnoreOneway(
				Const.MASK_MAP, rect.position,
				slope.DirectionHorizontal == Direction2.Right ? 6 : -6, 0,
				rect.size, this
			);
			X = rect.x;
			Y = Mathf.Min(rect.y, Y);
			return true;
		}


		#endregion




	}
}