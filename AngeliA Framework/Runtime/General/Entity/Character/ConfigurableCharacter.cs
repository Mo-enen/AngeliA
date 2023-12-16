using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	public interface IConfigurableCharacter {


		// SUB
		[System.Serializable]
		public class CharacterConfig {

			private static readonly int[] DEFAULT_BODY_PART_ID = { "DefaultCharacter.Head".AngeHash(), "DefaultCharacter.Body".AngeHash(), "DefaultCharacter.Hip".AngeHash(), "DefaultCharacter.Shoulder".AngeHash(), "DefaultCharacter.Shoulder".AngeHash(), "DefaultCharacter.UpperArm".AngeHash(), "DefaultCharacter.UpperArm".AngeHash(), "DefaultCharacter.LowerArm".AngeHash(), "DefaultCharacter.LowerArm".AngeHash(), "DefaultCharacter.Hand".AngeHash(), "DefaultCharacter.Hand".AngeHash(), "DefaultCharacter.UpperLeg".AngeHash(), "DefaultCharacter.UpperLeg".AngeHash(), "DefaultCharacter.LowerLeg".AngeHash(), "DefaultCharacter.LowerLeg".AngeHash(), "DefaultCharacter.Foot".AngeHash(), "DefaultCharacter.Foot".AngeHash(), };

			public int CharacterHeight = 160;

			// Body Part
			public int Head = DEFAULT_BODY_PART_ID[0];
			public int Body = DEFAULT_BODY_PART_ID[1];
			public int Hip = DEFAULT_BODY_PART_ID[2];
			public int Shoulder = DEFAULT_BODY_PART_ID[3];
			public int UpperArm = DEFAULT_BODY_PART_ID[5];
			public int LowerArm = DEFAULT_BODY_PART_ID[7];
			public int Hand = DEFAULT_BODY_PART_ID[9];
			public int UpperLeg = DEFAULT_BODY_PART_ID[11];
			public int LowerLeg = DEFAULT_BODY_PART_ID[13];
			public int Foot = DEFAULT_BODY_PART_ID[15];

			// Gadget
			public int Face = 0;
			public int Hair = 0;
			public int Ear = 0;
			public int Tail = 0;
			public int Wing = 0;
			public int Horn = 0;

			// Suit
			public int Suit_Head = 0;
			public int Suit_Body = 0;
			public int Suit_Hip = 0;
			public int Suit_Hand = 0;
			public int Suit_Foot = 0;

			// Color
			public int SkinColor = Util.ColorToInt(new Pixel32(245, 217, 196, 255)); // #f5d9c4
			public int HairColor = 858993663;


		}


		// VAR
		public CharacterConfig Config { get; set; }
		public int LoadedSlot { get; set; }


		// API
		public CharacterConfig CreateNewConfig () => new();


		public sealed void ReloadConfig () {
			if (LoadedSlot != AngePath.CurrentSaveSlot) {
				LoadConfigFromFile();
			} else {
				LoadCharacterFromConfig();
			}
		}


		public sealed void LoadConfigFromFile () {
			LoadedSlot = AngePath.CurrentSaveSlot;
			Config ??= CreateNewConfig() ?? new();
			string name = GetType().AngeName();
			string path = Util.CombinePaths(AngePath.PlayerDataRoot, "Character Config");
			bool overrided = AngeUtil.OverrideJson(path, Config, name);
			if (!overrided) Config = CreateNewConfig();
			LoadCharacterFromConfig();
		}


		public sealed void SaveConfigToFile () {
			Config ??= CreateNewConfig();
			if (Config == null) return;
			SaveCharacterToConfig();
			string path = Util.CombinePaths(AngePath.PlayerDataRoot, "Character Config");
			AngeUtil.SaveJson(Config, path, GetType().AngeName(), true);
		}


		public sealed void LoadCharacterFromConfig () {

			if (this is not PoseCharacter character) return;

			var config = Config;

			character.CharacterHeight = config.CharacterHeight.Clamp(Const.MIN_CHARACTER_HEIGHT, Const.MAX_CHARACTER_HEIGHT);

			// Bodyparts
			if (character.BodyPartsReady) {
				character.Head.SetSpriteID(config.Head);
				character.Body.SetSpriteID(config.Body);
				character.Hip.SetSpriteID(config.Hip);
				character.ShoulderL.SetSpriteID(config.Shoulder);
				character.ShoulderR.SetSpriteID(config.Shoulder);
				character.UpperArmL.SetSpriteID(config.UpperArm);
				character.UpperArmR.SetSpriteID(config.UpperArm);
				character.LowerArmL.SetSpriteID(config.LowerArm);
				character.LowerArmR.SetSpriteID(config.LowerArm);
				character.HandL.SetSpriteID(config.Hand);
				character.HandR.SetSpriteID(config.Hand);
				character.UpperLegL.SetSpriteID(config.UpperLeg);
				character.UpperLegR.SetSpriteID(config.UpperLeg);
				character.LowerLegL.SetSpriteID(config.LowerLeg);
				character.LowerLegR.SetSpriteID(config.LowerLeg);
				character.FootL.SetSpriteID(config.Foot);
				character.FootR.SetSpriteID(config.Foot);
			}

			character.Suit_Head = config.Suit_Head;
			character.Suit_Body = config.Suit_Body;
			character.Suit_Hand = config.Suit_Hand;
			character.Suit_Foot = config.Suit_Foot;
			character.Suit_Hip = config.Suit_Hip;

			character.FaceID = config.Face;
			character.HairID = config.Hair;
			character.EarID = config.Ear;
			character.TailID = config.Tail;
			character.WingID = config.Wing;
			character.HornID = config.Horn;

			character.SkinColor = Util.IntToColor(config.SkinColor);
			character.HairColor = Util.IntToColor(config.HairColor);

		}


		public sealed void SaveCharacterToConfig () {

			if (this is not PoseCharacter character) return;
			var config = Config;

			config.CharacterHeight = character.CharacterHeight.Clamp(Const.MIN_CHARACTER_HEIGHT, Const.MAX_CHARACTER_HEIGHT);

			if (character.BodyPartsReady) {
				config.Head = character.Head.ID;
				config.Body = character.Body.ID;
				config.Hip = character.Hip.ID;
				config.Shoulder = character.ShoulderL.ID;
				config.UpperArm = character.UpperArmL.ID;
				config.LowerArm = character.LowerArmL.ID;
				config.Hand = character.HandL.ID;
				config.UpperLeg = character.UpperLegL.ID;
				config.LowerLeg = character.LowerLegL.ID;
				config.Foot = character.FootL.ID;
			}

			config.Face = character.FaceID;
			config.Hair = character.HairID;
			config.Ear = character.EarID;
			config.Tail = character.TailID;
			config.Wing = character.WingID;
			config.Horn = character.HornID;

			config.Suit_Head = character.Suit_Head;
			config.Suit_Body = character.Suit_Body;
			config.Suit_Hand = character.Suit_Hand;
			config.Suit_Foot = character.Suit_Foot;
			config.Suit_Hip = character.Suit_Hip;

			config.SkinColor = Util.ColorToInt(character.SkinColor);
			config.HairColor = Util.ColorToInt(character.HairColor);

		}


	}
}