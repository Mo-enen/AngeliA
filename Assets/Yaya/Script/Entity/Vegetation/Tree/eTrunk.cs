using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {


	public class eTrunkPalmV : eTrunk {
		protected override string TrunkCode => "Trunk Palm V";
	}


	public class eTrunkDarkV : eTrunk {
		protected override string TrunkCode => "Trunk Dark V";
	}


	public class eTrunkV : eTrunk {
		protected override string TrunkCode => "Trunk V";
	}


	public class eTrunkPalmH : eTrunk {
		protected override string TrunkCode => "Trunk Palm H";
	}


	public class eTrunkDarkH : eTrunk {
		protected override string TrunkCode => "Trunk Dark H";
	}


	public class eTrunkH : eTrunk {
		protected override string TrunkCode => "Trunk H";
	}


	[EntityAttribute.Bounds(0, 0, Const.CELL_SIZE, Const.CELL_SIZE)]
	[EntityAttribute.MapEditorGroup("Vegetation")]
	[EntityAttribute.Capacity(256)]
	[EntityAttribute.DrawBehind]
	public abstract class eTrunk : Entity {


		protected abstract string TrunkCode { get; }

		protected int TrunkArtworkCode { get; private set; } = 0;


		public override void OnActived () {
			base.OnActived();
            TrunkArtworkCode = CellRenderer.TryGetSpriteFromGroup(
                TrunkCode.AngeHash(), (X * 3 + Y * 11) / Const.CELL_SIZE, out var tSprite
			) ? tSprite.GlobalID : 0;
		}


		public override void FillPhysics () {
			base.FillPhysics();
			CellPhysics.FillEntity(YayaConst.LAYER_ENVIRONMENT, this);
		}


		public override void FrameUpdate () {
			base.FrameUpdate();
            CellRenderer.Draw(TrunkArtworkCode, base.Rect);
		}


	}
}
