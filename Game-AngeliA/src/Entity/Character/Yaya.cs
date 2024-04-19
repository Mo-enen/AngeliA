using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliaGame;

[EntityAttribute.DefaultSelectPlayer]
public class Yaya : Player {


	// SUB
	public class Face : AngeliA.Face { }
	public class Hair : AngeliA.Hair { }
	public class Tail : AngeliA.Tail { }
	public class Ear : AngeliA.Ear { }
	public class Wing : AngeliA.Wing { }
	public class BodySuit : BodyCloth { }
	public class HipSuit : HipCloth { }
	public class FootSuit : FootCloth { }

	// Api
	public override bool BodySuitAvailable => true;
	public override bool HelmetAvailable => true;
	public override int AttackStyleIndex => EquippingWeaponType == WeaponType.Hand ? 1 : base.AttackStyleIndex;

	// Data
	private GuaGua GuaGua = null;


	// MSG
	public Yaya () {

		JumpDownThoughOneway.BaseValue = true;
		SlideAvailable.BaseValue = true;
		SlideOnAnyBlock.BaseValue = true;
		CharacterHeight = 158;


		MaxHP.BaseValue = 1;

		HairColor = Color32.WHITE;
		SkinColor = Color32.WHITE;

	}

	public override void Update () {
		base.Update();
		// Summon GuaGua
		if (GuaGua == null || !GuaGua.Active) {
			GuaGua = Summon.CreateSummon<GuaGua>(this, X, Y);
		}



		//Renderer.DrawPixel(Renderer.CameraRect.Shrink(1024), Color32.BLACK_128);


	}

}
