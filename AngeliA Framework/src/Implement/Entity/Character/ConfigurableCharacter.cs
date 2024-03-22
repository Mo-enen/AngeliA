using System.Collections;
using System.Collections.Generic;


namespace AngeliA; 
public interface IConfigurableCharacter {


	// SUB
	public class CharacterConfig {

		private static readonly int[] DEFAULT_BODY_PART_ID = { "DefaultCharacter.Head".AngeHash(), "DefaultCharacter.Body".AngeHash(), "DefaultCharacter.Hip".AngeHash(), "DefaultCharacter.Shoulder".AngeHash(), "DefaultCharacter.Shoulder".AngeHash(), "DefaultCharacter.UpperArm".AngeHash(), "DefaultCharacter.UpperArm".AngeHash(), "DefaultCharacter.LowerArm".AngeHash(), "DefaultCharacter.LowerArm".AngeHash(), "DefaultCharacter.Hand".AngeHash(), "DefaultCharacter.Hand".AngeHash(), "DefaultCharacter.UpperLeg".AngeHash(), "DefaultCharacter.UpperLeg".AngeHash(), "DefaultCharacter.LowerLeg".AngeHash(), "DefaultCharacter.LowerLeg".AngeHash(), "DefaultCharacter.Foot".AngeHash(), "DefaultCharacter.Foot".AngeHash(), };

		public SavingInt CharacterHeight;

		// Body Part
		public SavingInt Head;
		public SavingInt Body;
		public SavingInt Hip;
		public SavingInt Shoulder;
		public SavingInt UpperArm;
		public SavingInt LowerArm;
		public SavingInt Hand;
		public SavingInt UpperLeg;
		public SavingInt LowerLeg;
		public SavingInt Foot;

		// Gadget
		public SavingInt Face;
		public SavingInt Hair;
		public SavingInt Ear;
		public SavingInt Tail;
		public SavingInt Wing;
		public SavingInt Horn;

		// Suit
		public SavingInt Suit_Head;
		public SavingInt Suit_Body;
		public SavingInt Suit_Hip;
		public SavingInt Suit_Hand;
		public SavingInt Suit_Foot;

		// Color
		public SavingInt SkinColor;
		public SavingInt HairColor;

		// MSG
		public CharacterConfig (string name) {

			CharacterHeight = new($"{name}.CharacterHeight", 160);

			Head = new($"{name}.Head", DEFAULT_BODY_PART_ID[0]);
			Body = new($"{name}.Body", DEFAULT_BODY_PART_ID[1]);
			Hip = new($"{name}.Hip", DEFAULT_BODY_PART_ID[2]);
			Shoulder = new($"{name}.Shoulder", DEFAULT_BODY_PART_ID[3]);
			UpperArm = new($"{name}.UpperArm", DEFAULT_BODY_PART_ID[5]);
			LowerArm = new($"{name}.LowerArm", DEFAULT_BODY_PART_ID[7]);
			Hand = new($"{name}.Hand", DEFAULT_BODY_PART_ID[9]);
			UpperLeg = new($"{name}.UpperLeg", DEFAULT_BODY_PART_ID[11]);
			LowerLeg = new($"{name}.LowerLeg", DEFAULT_BODY_PART_ID[13]);
			Foot = new($"{name}.Foot", DEFAULT_BODY_PART_ID[15]);

			Face = new($"{name}.Face", 0);
			Hair = new($"{name}.Hair", 0);
			Ear = new($"{name}.Ear", 0);
			Tail = new($"{name}.Tail", 0);
			Wing = new($"{name}.Wing", 0);
			Horn = new($"{name}.Horn", 0);

			Suit_Head = new($"{name}.Suit_Head", 0);
			Suit_Body = new($"{name}.Suit_Body", 0);
			Suit_Hip = new($"{name}.Suit_Hip", 0);
			Suit_Hand = new($"{name}.Suit_Hand", 0);
			Suit_Foot = new($"{name}.Suit_Foot", 0);

			SkinColor = new($"{name}.SkinColor", Util.ColorToInt(new Color32(245, 217, 196, 255)));
			HairColor = new($"{name}.HairColor", 858993663);
		}

	}


	// VAR
	public CharacterConfig Config { get; set; }


	// API
	public sealed void LoadCharacterFromConfig () {

		if (this is not PoseCharacter character) return;

		var config = Config ??= new(GetType().AngeName());

		character.CharacterHeight = config.CharacterHeight.Value.Clamp(Const.MIN_CHARACTER_HEIGHT, Const.MAX_CHARACTER_HEIGHT);

		// Bodyparts
		if (character.BodyPartsReady) {
			character.Head.SetSpriteID(config.Head.Value);
			character.Body.SetSpriteID(config.Body.Value);
			character.Hip.SetSpriteID(config.Hip.Value);
			character.ShoulderL.SetSpriteID(config.Shoulder.Value);
			character.ShoulderR.SetSpriteID(config.Shoulder.Value);
			character.UpperArmL.SetSpriteID(config.UpperArm.Value);
			character.UpperArmR.SetSpriteID(config.UpperArm.Value);
			character.LowerArmL.SetSpriteID(config.LowerArm.Value);
			character.LowerArmR.SetSpriteID(config.LowerArm.Value);
			character.HandL.SetSpriteID(config.Hand.Value);
			character.HandR.SetSpriteID(config.Hand.Value);
			character.UpperLegL.SetSpriteID(config.UpperLeg.Value);
			character.UpperLegR.SetSpriteID(config.UpperLeg.Value);
			character.LowerLegL.SetSpriteID(config.LowerLeg.Value);
			character.LowerLegR.SetSpriteID(config.LowerLeg.Value);
			character.FootL.SetSpriteID(config.Foot.Value);
			character.FootR.SetSpriteID(config.Foot.Value);
		}

		character.Suit_Head = config.Suit_Head.Value;
		character.Suit_Body = config.Suit_Body.Value;
		character.Suit_Hand = config.Suit_Hand.Value;
		character.Suit_Foot = config.Suit_Foot.Value;
		character.Suit_Hip = config.Suit_Hip.Value;

		character.FaceID = config.Face.Value;
		character.HairID = config.Hair.Value;
		character.EarID = config.Ear.Value;
		character.TailID = config.Tail.Value;
		character.WingID = config.Wing.Value;
		character.HornID = config.Horn.Value;

		character.SkinColor = Util.IntToColor(config.SkinColor.Value);
		character.HairColor = Util.IntToColor(config.HairColor.Value);

	}


	public sealed void SaveCharacterToConfig () {

		if (this is not PoseCharacter character) return;
		var config = Config;

		config.CharacterHeight.Value = character.CharacterHeight.Clamp(Const.MIN_CHARACTER_HEIGHT, Const.MAX_CHARACTER_HEIGHT);

		if (character.BodyPartsReady) {
			config.Head.Value = character.Head.ID;
			config.Body.Value = character.Body.ID;
			config.Hip.Value = character.Hip.ID;
			config.Shoulder.Value = character.ShoulderL.ID;
			config.UpperArm.Value = character.UpperArmL.ID;
			config.LowerArm.Value = character.LowerArmL.ID;
			config.Hand.Value = character.HandL.ID;
			config.UpperLeg.Value = character.UpperLegL.ID;
			config.LowerLeg.Value = character.LowerLegL.ID;
			config.Foot.Value = character.FootL.ID;
		}

		config.Face.Value = character.FaceID;
		config.Hair.Value = character.HairID;
		config.Ear.Value = character.EarID;
		config.Tail.Value = character.TailID;
		config.Wing.Value = character.WingID;
		config.Horn.Value = character.HornID;

		config.Suit_Head.Value = character.Suit_Head;
		config.Suit_Body.Value = character.Suit_Body;
		config.Suit_Hand.Value = character.Suit_Hand;
		config.Suit_Foot.Value = character.Suit_Foot;
		config.Suit_Hip.Value = character.Suit_Hip;

		config.SkinColor.Value = Util.ColorToInt(character.SkinColor);
		config.HairColor.Value = Util.ColorToInt(character.HairColor);

	}


}