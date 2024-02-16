using System.Collections;
using System.Collections.Generic;
using AngeliA;
using AngeliA.Framework;



namespace AngeliaGame; 
[EntityAttribute.DefaultSelectPlayer]
public class Yaya : Player {


	// SUB
	public class Face : AngeliA.Framework.Face { }
	public class Hair : AngeliA.Framework.Hair { }
	public class Tail : AngeliA.Framework.Tail { }
	public class Ear : AngeliA.Framework.Ear { }
	public class Wing : AngeliA.Framework.Wing { }
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

	public override void PhysicsUpdate () {
		base.PhysicsUpdate();
		// Summon GuaGua
		if (GuaGua == null || !GuaGua.Active) {
			GuaGua = Summon.CreateSummon<GuaGua>(this, X, Y);
		}
	}

}
