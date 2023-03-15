using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {


	public class AltarLalynnA : eCheckAltar { }
	public class AltarMage : eCheckAltar { }
	public class AltarElf : eCheckAltar { }
	public class AltarDragon : eCheckAltar { }
	public class AltarTorch : eCheckAltar { }
	public class AltarSlime : eCheckAltar { }
	public class AltarInsect : eCheckAltar { }
	public class AltarOrc : eCheckAltar { }
	public class AltarTako : eCheckAltar { }
	public class AltarShark : eCheckAltar { }
	public class AltarBone : eCheckAltar { }
	public class AltarFootman : eCheckAltar { }
	public class AltarKnight : eCheckAltar { }
	public class AltarJesus : eCheckAltar { }
	public class AltarShield : eCheckAltar { }
	public class AltarGamble : eCheckAltar { }
	public class AltarScience : eCheckAltar { }
	public class AltarSpider : eCheckAltar { }
	public class AltarStalactite : eCheckAltar { }
	public class AltarSword : eCheckAltar { }
	public class AltarSpace : eCheckAltar { }
	public class AltarMachineGun : eCheckAltar { }
	public class AltarKnowledge : eCheckAltar { }
	public class AltarCat : eCheckAltar { }





	[EntityAttribute.Bounds(0, 0, Const.CEL, Const.CEL * 2)]
	public abstract class eCheckAltar : eCheckPoint, IGlobalPosition {


		public override void OnActived () {
			base.OnActived();
			Height = Const.CEL * 2;
		}


	}
}