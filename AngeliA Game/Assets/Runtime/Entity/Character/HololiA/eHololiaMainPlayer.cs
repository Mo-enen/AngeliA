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

			CharacterHeight = 157,

			Face = nameof(FriendAFace).AngeHash(),
			Hair = nameof(FriendAHair).AngeHash(),
			Ear = 0,
			Tail = 0,
			Wing = 0,
			Horn = 0,

			Suit_Head = 0,
			Suit_Body = nameof(FriendABodySuit).AngeHash(),
			Suit_Hip = nameof(FriendAHipSuit).AngeHash(),
			Suit_Hand = 0,
			Suit_Foot = nameof(FriendAFootSuit).AngeHash(),

			SkinColor = Util.ColorToInt(new Color32(245, 217, 196, 255)), // #f5d9c4
			HairColor = Util.ColorToInt(Const.WHITE),

		};


	}
}