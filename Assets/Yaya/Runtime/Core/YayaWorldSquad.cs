using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class YayaWorldSquad : WorldSquad {


		// VAR
		protected override Int4 CullingPadding =>
			CellRenderer.CameraShaking || FrameTask.IsTasking<TeleportTask>(YayaConst.TASK_ROUTE) ?
			new Int4(Const.CEL * 8, Const.CEL * 8, Const.CEL * 8, Const.CEL * 8) :
			Int4.Zero;
		public bool SpawnEntity { get; set; } = true;
		public bool SaveBeforeReload { get; set; } = false;
		public bool Enable { get; set; } = true;


		// API
		public YayaWorldSquad (bool behind = false) : base(behind) { }
		public override int LevelLayer => YayaConst.LAYER_LEVEL;


		protected override void BeforeWorldReload (World[,] worlds) {
			base.BeforeWorldReload(worlds);
			if (SaveBeforeReload && Channel == MapChannel.User) {
				SaveToFile(Const.UserMapRoot);
			}
		}


		protected override void DrawBackgroundBlock (int id, int unitX, int unitY) {
			if (!Enable) return;
			base.DrawBackgroundBlock(id, unitX, unitY);
			// Collider for Oneway
			if (
				CellRenderer.TryGetSprite(id, out var sp) &&
				CellRenderer.TryGetMeta(id, out var meta) &&
				AngeUtil.IsOnewayTag(meta.Tag)
			) {
				CellPhysics.FillBlock(
					YayaConst.LAYER_LEVEL, id,
					new RectInt(
						unitX * Const.CEL,
						unitY * Const.CEL,
						Const.CEL,
						Const.CEL
					).Shrink(
						sp.GlobalBorder.Left, sp.GlobalBorder.Right, sp.GlobalBorder.Down, sp.GlobalBorder.Up
					),
					true, meta.Tag
				);
			}
		}


		protected override void DrawLevelBlock (int id, int unitX, int unitY) {
			if (!Enable) return;
			base.DrawLevelBlock(id, unitX, unitY);
			// Damage
			if (CellRenderer.TryGetMeta(id, out var meta)) {
				if (meta.Tag == YayaConst.DAMAGE_TAG) {
					var rect = new RectInt(unitX * Const.CEL, unitY * Const.CEL, Const.CEL, Const.CEL);
					YayaCellPhysics.FillBlock_Damage(id, rect.Expand(1), 1);
				}
			}
		}


		protected override void DrawEntity (Game game, int id, int unitX, int unitY, int unitZ) {
			if (!Enable) return;
			if (SpawnEntity) {
				// Spawn Entity
				var entity = game.SpawnEntityFromWorld(id, unitX, unitY, unitZ);
				if (entity is eCharacter ch) {
					ch.X += ch.Width / 2;
				}
			} else {
				// Draw Entity
				var rect = new RectInt(unitX * Const.CEL, unitY * Const.CEL, Const.CEL, Const.CEL);
				if (CellRenderer.TryGetSprite(id, out var sprite)) {
					rect = rect.Fit(
						sprite.GlobalWidth, sprite.GlobalHeight,
						sprite.PivotX, sprite.PivotY
					);
				}
				if (CullingCameraRect.Overlaps(rect)) {
					CellRenderer.Draw(id, rect);
				}
			}
		}


		protected override void Draw_Behind (int id, int unitX, int unitY, bool fixRatio) {
			if (!Enable) return;
			base.Draw_Behind(id, unitX, unitY, SpawnEntity && fixRatio);
		}


		public FittingPose GetEntityPose (Entity entity, bool horizontal) => GetEntityPose(entity.TypeID, entity.X, entity.Y, horizontal);
		public FittingPose GetEntityPose (int typeID, int globalX, int globalY, bool horizontal) {
			int unitX = globalX.UDivide(Const.CEL);
			int unitY = globalY.UDivide(Const.CEL);
			bool n = GetBlockAt(horizontal ? unitX - 1 : unitX, horizontal ? unitY : unitY - 1, BlockType.Entity) == typeID;
			bool p = GetBlockAt(horizontal ? unitX + 1 : unitX, horizontal ? unitY : unitY + 1, BlockType.Entity) == typeID;
			return
				n && p ? FittingPose.Mid :
				!n && p ? FittingPose.Left :
				n && !p ? FittingPose.Right :
				FittingPose.Single;
		}
		public FittingPose GetEntityPose (Entity entity, bool horizontal, int mask, out Entity left_down, out Entity right_up, OperationMode mode = OperationMode.ColliderOnly, int tag = 0) {
			left_down = null;
			right_up = null;
			int unitX = entity.X.UDivide(Const.CEL);
			int unitY = entity.Y.UDivide(Const.CEL);
			int typeID = entity.TypeID;
			bool n = GetBlockAt(horizontal ? unitX - 1 : unitX, horizontal ? unitY : unitY - 1, BlockType.Entity) == typeID;
			bool p = GetBlockAt(horizontal ? unitX + 1 : unitX, horizontal ? unitY : unitY + 1, BlockType.Entity) == typeID;
			if (n) {
				left_down = CellPhysics.GetEntity(typeID, entity.Rect.Edge(horizontal ? Direction4.Left : Direction4.Down), mask, entity, mode, tag);
			}
			if (p) {
				right_up = CellPhysics.GetEntity(typeID, entity.Rect.Edge(horizontal ? Direction4.Right : Direction4.Up), mask, entity, mode, tag);
			}
			return
				n && p ? FittingPose.Mid :
				!n && p ? FittingPose.Left :
				n && !p ? FittingPose.Right :
				FittingPose.Single;
		}


	}
}
