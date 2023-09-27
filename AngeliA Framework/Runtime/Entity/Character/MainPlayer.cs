using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	[EntityAttribute.Capacity(1, 1)]
	[EntityAttribute.MapEditorGroup("System")]
	public sealed class MainPlayer : Player {




		#region --- SUB ---


		[System.Serializable]
		private class MainPlayerConfig {

			public Int3 HomeUnitPosition = new(int.MinValue, int.MinValue, int.MinValue);
			public int CharacterHeight = 160;
			public int CharacterBoobSize = 300;

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
			public int Boob = 0;

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


		#endregion




		#region --- VAR ---


		// Api
		public override Color32 SkinColor => _SkinColor;
		public override Color32 HairColor => _HairColor;
		public override int CharacterHeight => _CharacterHeight;
		public Int3? HomeUnitPosition { get; private set; } = null;

		// Data
		private MainPlayerConfig Config = new();
		private Color32 _SkinColor = new(239, 194, 160, 255);
		private Color32 _HairColor = new(51, 51, 51, 255);
		private int _CharacterHeight = 160;


		#endregion




		#region --- MSG ---


		public MainPlayer () => LoadConfigFromFile();


		public override void OnActivated () {
			base.OnActivated();
			LoadCharacterFromConfig();
		}


		public override void FrameUpdate () {
			base.FrameUpdate();
			// Sleep
			if (SleepFrame == FULL_SLEEP_DURATION) {
				HomeUnitPosition = new Int3(X.ToUnit(), Y.ToUnit(), Stage.ViewZ);
				SaveConfigToFile();
			}
			// Attack

			//////////////////// TEMP /////////////////////
			if (AttackStartAtCurrentFrame) {
				var paw = Stage.SpawnEntity("YayaPaw".AngeHash(), X, Y) as Bullet;
				paw?.Release(this, AttackTargetTeam, FacingRight ? 1 : -1, 0, 0, 0);
			}
			//////////////////// TEMP /////////////////////

		}


		#endregion




		#region --- API ---


		public void LoadConfigFromFile () {
			Config ??= new();
			AngeUtil.OverrideJson(Const.PlayerDataRoot, Config);
			LoadCharacterFromConfig();
		}


		public void SaveConfigToFile () {
			SaveCharacterToConfig();
			AngeUtil.SaveJson(Config, Const.PlayerDataRoot, true);
		}


		public void SetSkinColor (Color32 newColor) => _SkinColor = newColor;
		public void SetHairColor (Color32 newColor) => _HairColor = newColor;


		#endregion




		#region --- LGC ---


		private void LoadCharacterFromConfig () {

			HomeUnitPosition = Config.HomeUnitPosition.x != int.MinValue ? Config.HomeUnitPosition : null;
			_CharacterHeight = Config.CharacterHeight.Clamp(100, 200);
			
			// Bodyparts
			if (BodyPartsReady) {
				Head.SetSpriteID(Config.Head);
				Body.SetSpriteID(Config.Body);
				Hip.SetSpriteID(Config.Hip);
				ShoulderL.SetSpriteID(Config.Shoulder);
				ShoulderR.SetSpriteID(Config.Shoulder);
				UpperArmL.SetSpriteID(Config.UpperArm);
				UpperArmR.SetSpriteID(Config.UpperArm);
				LowerArmL.SetSpriteID(Config.LowerArm);
				LowerArmR.SetSpriteID(Config.LowerArm);
				HandL.SetSpriteID(Config.Hand);
				HandR.SetSpriteID(Config.Hand);
				UpperLegL.SetSpriteID(Config.UpperLeg);
				UpperLegR.SetSpriteID(Config.UpperLeg);
				LowerLegL.SetSpriteID(Config.LowerLeg);
				LowerLegR.SetSpriteID(Config.LowerLeg);
				FootL.SetSpriteID(Config.Foot);
				FootR.SetSpriteID(Config.Foot);
			}

			Suit_Head = Config.Suit_Head;
			Suit_Body = Config.Suit_Body;
			Suit_Hand = Config.Suit_Hand;
			Suit_Foot = Config.Suit_Foot;
			Suit_Hip = Config.Suit_Hip;

			FaceID = Config.Face;
			HairID = Config.Hair;
			EarID = Config.Ear;
			TailID = Config.Tail;
			WingID = Config.Wing;
			HornID = Config.Horn;
			BoobID = Config.Boob;

			_SkinColor = Util.IntToColor(Config.SkinColor);
			_HairColor = Util.IntToColor(Config.HairColor);

			// Movement
			FlyAvailable.Value = WingID != 0;
			FlyGlideAvailable.Value = WingID != 0 && !Wing.IsPropellerWing(WingID);

		}


		private void SaveCharacterToConfig () {

			Config.HomeUnitPosition = HomeUnitPosition ?? new Int3(int.MinValue, int.MinValue, int.MinValue);
			Config.CharacterHeight = _CharacterHeight.Clamp(100, 200);
			
			if (BodyPartsReady) {
				Config.Head = Head.ID;
				Config.Body = Body.ID;
				Config.Hip = Hip.ID;
				Config.Shoulder = ShoulderL.ID;
				Config.UpperArm = UpperArmL.ID;
				Config.LowerArm = LowerArmL.ID;
				Config.Hand = HandL.ID;
				Config.UpperLeg = UpperLegL.ID;
				Config.LowerLeg = LowerLegL.ID;
				Config.Foot = FootL.ID;
			}

			Config.Face = FaceID;
			Config.Hair = HairID;
			Config.Ear = EarID;
			Config.Tail = TailID;
			Config.Wing = WingID;
			Config.Horn = HornID;
			Config.Boob = BoobID;

			Config.Suit_Head = Suit_Head;
			Config.Suit_Body = Suit_Body;
			Config.Suit_Hand = Suit_Hand;
			Config.Suit_Foot = Suit_Foot;
			Config.Suit_Hip = Suit_Hip;

			Config.SkinColor = Util.ColorToInt(_SkinColor);
			Config.HairColor = Util.ColorToInt(_HairColor);

			// Movement
			FlyAvailable.Value = WingID != 0;
			FlyGlideAvailable.Value = WingID != 0 && !Wing.IsPropellerWing(WingID);
		}


		#endregion



	}
}