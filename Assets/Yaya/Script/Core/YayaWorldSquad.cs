using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {


	public class fSquadTransition : CellEffect {


		private float Scale { get; init; } = 1f;
		private float Alpha { get; init; } = 1f;
		private AnimationCurve Curve { get; init; } = null;


		public fSquadTransition (int duration, float scale, float alpha, AnimationCurve curve) : base(duration) {
			Scale = scale;
			Curve = curve;
			Alpha = alpha;
		}


		public override void Perform (Cell[] cells, int cellCount, int layerIndex) {

			if (layerIndex == YayaConst.SHADER_UI) return;

			float z01 = Mathf.InverseLerp(0, Duration, LocalFrame);
			Vector2 center = CellRenderer.CameraRect.center;
			var scl = Mathf.LerpUnclamped(Scale, 1f, Curve.Evaluate(z01));

			// Behind
			PerformLogic(
				cells, center, 0, ZSortBegin - 1, scl,
				Scale > 1f ? Mathf.LerpUnclamped(1f, Alpha, z01) : z01 * Alpha
			);

			// Current
			PerformLogic(
				cells, center, ZSortBegin, cellCount - 1, scl,
				1f//Scale > 1f ? z01 : Mathf.LerpUnclamped(Alpha, 1f, z01)
			);

		}


		private void PerformLogic (Cell[] cells, Vector2 center, int startIndex, int endIndex, float scale, float alpha) {
			Color32 c;
			for (int i = startIndex; i <= endIndex; i++) {
				var cell = cells[i];
				c = cell.Color;
				c.a = (byte)(alpha * 255);
				cell.Color = c;
				cell.X = cell.X.LerpTo(center.x.FloorToInt(), 1f - scale);
				cell.Y = cell.Y.LerpTo(center.y.FloorToInt(), 1f - scale);
				cell.Width = (cell.Width * scale).CeilToInt();
				cell.Height = (cell.Height * scale).CeilToInt();
			}
		}


	}



	public class YayaWorldSquad : WorldSquad {


		// API
		protected override void DrawBackgroundBlock (int id, int unitX, int unitY) {
			base.DrawBackgroundBlock(id, unitX, unitY);
			if (!Behind) {
				// Collider for Oneway
				if (CellRenderer.TryGetMeta(id, out var meta) && AngeUtil.IsOnewayTag(meta.Tag)) {
					CellPhysics.FillBlock(
						YayaConst.LAYER_LEVEL, id,
						new RectInt(
							unitX * Const.CELL_SIZE,
							unitY * Const.CELL_SIZE,
							Const.CELL_SIZE,
							Const.CELL_SIZE
						),
						true, meta.Tag
					);
				}
			}
		}


		protected override void DrawLevelBlock (int id, int unitX, int unitY) {
			base.DrawLevelBlock(id, unitX, unitY);
			if (!Behind) {
				// Collider
				if (!CellRenderer.TryGetSprite(id, out var sp)) return;
				bool isTrigger = false;
				int tag = 0;
				if (CellRenderer.TryGetMeta(id, out var meta)) {
					isTrigger = meta.IsTrigger;
					tag = meta.Tag;
				}
				var rect = new RectInt(
					unitX * Const.CELL_SIZE, unitY * Const.CELL_SIZE, Const.CELL_SIZE, Const.CELL_SIZE
				).Shrink(
					sp.GlobalBorder.Left, sp.GlobalBorder.Right, sp.GlobalBorder.Down, sp.GlobalBorder.Up
				);
				CellPhysics.FillBlock(YayaConst.LAYER_LEVEL, id, rect, isTrigger, tag);
				// Damage
				if (tag == YayaConst.DAMAGE_TAG) {
					YayaCellPhysics.FillBlock_Damage(id, rect.Expand(1), true, 1);
					YayaCellPhysics.FillBlock_Damage(id, rect.Expand(1), false, 1);
				}
			}
		}


		public FittingPose GetEntityPose (Entity entity, bool horizontal) => GetEntityPose(entity.TypeID, entity.X, entity.Y, horizontal);
		public FittingPose GetEntityPose (int typeID, int globalX, int globalY, bool horizontal) {
			int unitX = globalX.UDivide(Const.CELL_SIZE);
			int unitY = globalY.UDivide(Const.CELL_SIZE);
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
			int unitX = entity.X.UDivide(Const.CELL_SIZE);
			int unitY = entity.Y.UDivide(Const.CELL_SIZE);
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
