using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	[EntityAttribute.EntityCapacity(8)]
	[EntityAttribute.EntityBounds(0, 0, Const.CELL_SIZE, Const.CELL_SIZE * 2)]
	public class eCheckPoint : Entity {


		// Const
		private static readonly int ARTWORK_STATUE_CODE = "Check Statue".AngeHash();
		private static readonly int ARTWORK_ALTAR_CODE = "Check Altar".AngeHash();

		// Data
		private int ArtCode = 0;
		private bool IsAltar = false;


		// MSG
		public override void OnActived () {
			base.OnActived();
			var globalUnitPos = new Vector2Int(X.UDivide(Const.CELL_SIZE), Y.UDivide(Const.CELL_SIZE));
			if (Yaya.CpPool.TryGetValue(globalUnitPos, out var _cpData) && _cpData.Index >= 0) {
				IsAltar = _cpData.IsAltar;
			} else {
				IsAltar = false;
				Active = false;
			}
			Width = Const.CELL_SIZE;
			Height = IsAltar ? Const.CELL_SIZE * 2 : Const.CELL_SIZE;
			ArtCode = 0;
		}


		public override void FillPhysics () {
			base.FillPhysics();
			Physics.FillEntity(YayaConst.LAYER_ENVIRONMENT, this, true, Const.ONEWAY_UP_TAG);
		}


		public override void PhysicsUpdate () {
			base.PhysicsUpdate();
			if (ArtCode == 0) {
				int artIndex = 0;
				var globalUnitPos = new Vector2Int(X.UDivide(Const.CELL_SIZE), Y.UDivide(Const.CELL_SIZE));
				if (Yaya.CpPool.TryGetValue(globalUnitPos, out var _cpData)) {
					artIndex = _cpData.Index;
				}
				if (AngeliaFramework.Renderer.TryGetSpriteFromGroup(IsAltar ? ARTWORK_ALTAR_CODE : ARTWORK_STATUE_CODE, artIndex, out var sprite, false)) {
                    ArtCode = sprite.GlobalID;
				} else {
                    ArtCode = -1;
				}
			}
		}


		public override void FrameUpdate () {
			base.FrameUpdate();
            AngeliaFramework.Renderer.Draw(ArtCode, base.Rect);
		}


		public static bool TryGetAltarPosition (int index, out Vector2Int unitPos) => Yaya.CpAltarPool.TryGetValue(index, out unitPos);


	}
}