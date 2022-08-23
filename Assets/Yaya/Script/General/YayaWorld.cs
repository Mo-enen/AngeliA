using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class YayaWorldSquad : WorldSquad {



		// Data
		private readonly YayaMeta YayaMeta = new();


		// API
		public YayaWorldSquad (string mapRoot, YayaMeta yayaMeta) : base(mapRoot) {
			YayaMeta = yayaMeta;
		}


		protected override void DrawBackgroundBlock (int id, int unitX, int unitY) {
			base.DrawBackgroundBlock(id, unitX, unitY);
			if (!Behind) {
				// Collider for Oneway
				if (CellRenderer.TryGetMeta(id, out var meta) && Const.IsOnewayTag(meta.Tag)) {
					CellPhysics.FillBlock(
						YayaConst.LAYER_LEVEL,
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
				CellPhysics.FillBlock(
					YayaConst.LAYER_LEVEL, rect, isTrigger, tag
				);
				// Damage
				if (tag == YayaConst.DAMAGE_TAG) {
					CellPhysics.FillBlock(
						YayaConst.LAYER_DAMAGE, rect.Expand(YayaMeta.LevelDamageExpand), true, 1
					);
				}
			}
		}


	}
}
