using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGame {
	[EntityAttribute.DefaultSelectPlayer(1)]
	public class eHololiaMainPlayer : Player, IConfigurableCharacter {


		// Api
		IConfigurableCharacter.CharacterConfig IConfigurableCharacter.Config { get; set; } = new();
		int IConfigurableCharacter.LoadedSlot { get; set; } = -1;


		// MSG
		public override void OnActivated () {
			base.OnActivated();
			(this as IConfigurableCharacter).ReloadConfig();
		}


		IConfigurableCharacter.CharacterConfig IConfigurableCharacter.CreateNewConfig () => new() {

			Head = "Holo.Head".AngeHash(),
			Body = "Holo.Body".AngeHash(),
			Hip = "Holo.Hip".AngeHash(),
			Shoulder = "Holo.Shoulder".AngeHash(),
			UpperArm = "Holo.UpperArm".AngeHash(),
			LowerArm = "Holo.LowerArm".AngeHash(),
			Hand = "Holo.Hand".AngeHash(),
			UpperLeg = "Holo.UpperLeg".AngeHash(),
			LowerLeg = "Holo.LowerLeg".AngeHash(),
			Foot = "Holo.Foot".AngeHash(),

			CharacterHeight = 160,

			Face = 0,
			Hair = 0,
			Ear = 0,
			Tail = 0,
			Wing = 0,
			Horn = 0,

			Suit_Head = 0,
			Suit_Body = 0,
			Suit_Hip = 0,
			Suit_Hand = 0,
			Suit_Foot = 0,

			SkinColor = -272457473,
			HairColor = -1,

		};


	}
}