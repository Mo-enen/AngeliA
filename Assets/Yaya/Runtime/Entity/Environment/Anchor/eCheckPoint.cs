using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {

	public class CheckLalynnA : eCheckPoint { }
	public class CheckMage : eCheckPoint { }
	public class CheckElf : eCheckPoint { }
	public class CheckDragon : eCheckPoint { }
	public class CheckTorch : eCheckPoint { }
	public class CheckSlime : eCheckPoint { }
	public class CheckInsect : eCheckPoint { }
	public class CheckOrc : eCheckPoint { }
	public class CheckTako : eCheckPoint { }
	public class CheckShark : eCheckPoint { }
	public class CheckBone : eCheckPoint { }
	public class CheckFootman : eCheckPoint { }
	public class CheckKnight : eCheckPoint { }
	public class CheckJesus : eCheckPoint { }
	public class CheckShield : eCheckPoint { }
	public class CheckGamble : eCheckPoint { }
	public class CheckScience : eCheckPoint { }
	public class CheckSpider : eCheckPoint { }
	public class CheckStalactite : eCheckPoint { }
	public class CheckSword : eCheckPoint { }
	public class CheckSpace : eCheckPoint { }
	public class CheckMachineGun : eCheckPoint { }
	public class CheckKnowledge : eCheckPoint { }
	public class CheckCat : eCheckPoint { }


	[EntityAttribute.Capacity(1)]
	[EntityAttribute.MapEditorGroup("Check Point")]
	public abstract class eCheckPoint : Entity {



		private Int4 Border = default;


		public override void OnActived () {
			base.OnActived();
			Border = default;
			if (CellRenderer.TryGetSprite(TypeID, out var sprite)) {
				Border = sprite.GlobalBorder;
			}
		}


		public override void FillPhysics () {
			base.FillPhysics();
			CellPhysics.FillEntity(YayaConst.LAYER_ENVIRONMENT, this, true);
			CellPhysics.FillBlock(YayaConst.LAYER_ENVIRONMENT, TypeID, Rect.Shrink(Border), true, Const.ONEWAY_UP_TAG);
		}


		public override void FrameUpdate () {
			base.FrameUpdate();
			CellRenderer.Draw(TypeID, Rect);
		}


	}
}