using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class YayaWorldSquad : WorldSquad {



		// Data
		private readonly YayaMeta YayaMeta = new();


		// API
		public YayaWorldSquad (string mapRoot, int maxParallax, YayaMeta yayaMeta) : base(mapRoot, maxParallax) {
			YayaMeta = yayaMeta;
		}


		protected override void DrawBackgroundBlock (int id, int unitX, int unitY, RectInt cameraRect, BackgroundBlockMeta meta) {
			base.DrawBackgroundBlock(id, unitX, unitY, cameraRect, meta);
			// Collider for Oneway
			if (meta != null && meta.Parallax == 0 && Const.IsOnewayTag(meta.Tag)) {
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


		protected override void DrawLevelBlock (int id, int unitX, int unitY, byte buildingAlpha, LevelBlockMeta meta) {
			base.DrawLevelBlock(id, unitX, unitY, buildingAlpha, meta);
			// Collider
			if (meta != null && !meta.IgnoreCollider && CellRenderer.TryGetSprite(id, out var sp)) {
				// Collider
				var rect = new RectInt(
					unitX * Const.CELL_SIZE, unitY * Const.CELL_SIZE, Const.CELL_SIZE, Const.CELL_SIZE
				).Shrink(
					sp.GlobalBorder.Left, sp.GlobalBorder.Right, sp.GlobalBorder.Down, sp.GlobalBorder.Up
				);
				CellPhysics.FillBlock(
					YayaConst.LAYER_LEVEL, rect, meta.IsTrigger, meta.Tag
				);
				// Damage
				if (meta.Tag == YayaConst.DAMAGE_TAG) {
					CellPhysics.FillBlock(
						YayaConst.LAYER_DAMAGE, rect.Expand(YayaMeta.LevelDamageExpand), true
					);
				}
			}
		}


	}
}
