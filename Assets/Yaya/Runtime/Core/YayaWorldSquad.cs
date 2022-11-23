using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {


	public class tSetViewZTask : TaskItem {

		public static readonly int TYPE_ID = typeof(tSetViewZTask).AngeHash();
		public int Duration = 0;
		public int NewZ = 0;
		private bool Front = true;

		public override TaskResult FrameUpdate () {
			if (LocalFrame == 0) {
				// Player
				var yaya = Yaya.Current;
				var player = yaya.CurrentPlayer;
				if (player != null) {
					player.Renderer.EnterDoor(Duration, NewZ < yaya.ViewZ);
				}
				Front = NewZ > yaya.ViewZ;
			}
			if (LocalFrame == Duration / 2) {
				// Set View Z
				Game.Current.SetViewZ(NewZ);
			}
			if (LocalFrame == Duration / 2 + 1) {
				// Add Effect
				var yaya = Yaya.Current;
				int para = yaya.GameMeta.SquadBehindParallax;
				byte alpha = yaya.GameMeta.SquadBehindAlpha;
				var curve = yaya.YayaMeta.SquadTransitionCurve;
				var effect = fSquadTransition.Instance;
				effect.Duration = Duration / 2;
				effect.Scale = Front ? 1000f / para : para / 1000f;
				effect.Alpha = alpha / 255f;
				effect.Curve = curve;
				CellRenderer.RemoveEffect<fSquadTransition>();
				CellRenderer.AddEffect(effect);
			}
			return LocalFrame < Duration ? TaskResult.Continue : TaskResult.End;
		}

	}



	public class fSquadTransition : CellEffect {


		public static readonly fSquadTransition Instance = new();


		public float Scale { get; set; } = 1f;
		public float Alpha { get; set; } = 1f;
		public AnimationCurve Curve { get; set; } = null;


		public override void Perform (Cell[] cells, int cellCount, int layerIndex) {

			if (layerIndex == CellRenderer.LayerCount - 1) return;

			float z01 = Mathf.InverseLerp(0, Duration, LocalFrame);
			Vector2 center = CellRenderer.CameraRect.center;
			var scl = Mathf.LerpUnclamped(Scale, 1f, Curve.Evaluate(z01));

			// Behind
			PerformLogic(
				cells, center, 0, SortedIndex - 1, scl,
				Scale > 1f ? Mathf.LerpUnclamped(1f, Alpha, z01) : z01 * Alpha
			);

			// Current
			PerformLogic(cells, center, SortedIndex, cellCount - 1, scl, 1f);

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


		// VAR
		public override bool Culling => base.Culling && !FrameTask.HasTask(Const.TASK_ROUTE);


		// API
		protected override void DrawBackgroundBlock (int id, int unitX, int unitY) {
			base.DrawBackgroundBlock(id, unitX, unitY);
			if (!Behind) {
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
					unitX * Const.CEL, unitY * Const.CEL, Const.CEL, Const.CEL
				).Shrink(
					sp.GlobalBorder.Left, sp.GlobalBorder.Right, sp.GlobalBorder.Down, sp.GlobalBorder.Up
				);
				CellPhysics.FillBlock(YayaConst.LAYER_LEVEL, id, rect, isTrigger, tag);
				// Damage
				if (tag == YayaConst.DAMAGE_TAG) {
					YayaCellPhysics.FillBlock_Damage(id, rect.Expand(1), true, 1);
				}
			}
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
