using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	public interface IConfigurableCharacter {


		// SUB
		[System.Serializable]
		public class CharacterConfig {

			public int CharacterHeight = 160;

			// Body Part
			public int Head = Character.DEFAULT_BODY_PART_ID[0];
			public int Body = Character.DEFAULT_BODY_PART_ID[1];
			public int Hip = Character.DEFAULT_BODY_PART_ID[2];
			public int Shoulder = Character.DEFAULT_BODY_PART_ID[3];
			public int UpperArm = Character.DEFAULT_BODY_PART_ID[5];
			public int LowerArm = Character.DEFAULT_BODY_PART_ID[7];
			public int Hand = Character.DEFAULT_BODY_PART_ID[9];
			public int UpperLeg = Character.DEFAULT_BODY_PART_ID[11];
			public int LowerLeg = Character.DEFAULT_BODY_PART_ID[13];
			public int Foot = Character.DEFAULT_BODY_PART_ID[15];

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
			public int SkinColor = -272457473;
			public int HairColor = 858993663;


		}


		// VAR
		public CharacterConfig Config { get; set; }
		public int LoadedSlot { get; set; }
		private static string ConfigFilePath => Util.CombinePaths(AngePath.PlayerDataRoot, "Character Config");


		// API
		public sealed void ReloadConfig () {
			if (LoadedSlot != AngePath.CurrentDataSlot) {
				LoadConfigFromFile();
			} else {
				LoadCharacterFromConfig();
			}
		}


		public sealed void LoadConfigFromFile () {
			LoadedSlot = AngePath.CurrentDataSlot;
			var config = Config;
			if (config == null) return;
			string name = GetType().Name;
			AngeUtil.OverrideJson(ConfigFilePath, config, name);
			LoadCharacterFromConfig();
		}


		public sealed void SaveConfigToFile () {
			if (Config == null) return;
			SaveCharacterToConfig();
			AngeUtil.SaveJson(Config, ConfigFilePath, GetType().Name, true);
		}


		public sealed void LoadCharacterFromConfig () {

			if (this is not Character character || character.RenderWithSheet) return;

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

			if (this is not Character character || character.RenderWithSheet) return;
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