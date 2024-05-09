using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace AngeliA;

public class RiggedRespondMessage {


	public int GlobalFrame;
	public int PauselessFrame;
	public int RequireSetCursorIndex;
	public byte EffectEnable;
	public byte HasEffectParams;
	public float e_DarkenAmount;
	public float e_DarkenStep;
	public float e_LightenAmount;
	public float e_LightenStep;
	public Color32 e_TintColor;
	public float e_VigRadius;
	public float e_VigFeather;
	public float e_VigOffsetX;
	public float e_VigOffsetY;
	public float e_VigRound;
	public int RequirePlayMusicID;
	public byte AudioActionRequirement;
	public int RequireSetMusicVolume;
	public int RequirePlaySoundID;
	public float RequirePlaySoundVolume;
	public int RequireSetSoundVolume;



	// API
	public void Reset () {
		RequireSetCursorIndex = int.MinValue;
		HasEffectParams = 0;
		RequirePlayMusicID = 0;
		AudioActionRequirement = 0;
		RequireSetMusicVolume = -1;
		RequirePlaySoundID = 0;
		RequirePlaySoundVolume = -1f;
		RequireSetSoundVolume = -1;
	}


	public void SetDataToFramework () {

		if (RequireSetCursorIndex != int.MinValue) {
			if (RequireSetCursorIndex == -3) {
				Game.SetCursorToNormal();
			} else {
				Game.SetCursor(RequireSetCursorIndex);
			}
		}



		// Rendering





		// Audio




	}


	public void ReadDataFromPipe (BinaryReader reader) {

		GlobalFrame = reader.ReadInt32();
		PauselessFrame = reader.ReadInt32();
		RequireSetCursorIndex = reader.ReadInt32();
		EffectEnable = reader.ReadByte();
		HasEffectParams = reader.ReadByte();

		if (HasEffectParams.GetBit(Const.SCREEN_EFFECT_RETRO_DARKEN)) {
			e_DarkenAmount = reader.ReadSingle();
			e_DarkenStep = reader.ReadSingle();
		}
		if (HasEffectParams.GetBit(Const.SCREEN_EFFECT_RETRO_LIGHTEN)) {
			e_LightenAmount = reader.ReadSingle();
			e_LightenStep = reader.ReadSingle();
		}
		if (HasEffectParams.GetBit(Const.SCREEN_EFFECT_TINT)) {
			e_TintColor.r = reader.ReadByte();
			e_TintColor.g = reader.ReadByte();
			e_TintColor.b = reader.ReadByte();
			e_TintColor.a = reader.ReadByte();
		}
		if (HasEffectParams.GetBit(Const.SCREEN_EFFECT_VIGNETTE)) {
			e_VigRadius = reader.ReadSingle();
			e_VigFeather = reader.ReadSingle();
			e_VigOffsetX = reader.ReadSingle();
			e_VigOffsetY = reader.ReadSingle();
			e_VigRound = reader.ReadSingle();
		}

		RequirePlayMusicID = reader.ReadInt32();
		AudioActionRequirement = reader.ReadByte();
		RequireSetMusicVolume = reader.ReadInt32();
		RequirePlaySoundID = reader.ReadInt32();
		RequirePlaySoundVolume = reader.ReadSingle();
		RequireSetSoundVolume = reader.ReadInt32();




	}


	public void WriteDataToPipe (BinaryWriter writer) {

		writer.Write(GlobalFrame);
		writer.Write(PauselessFrame);
		writer.Write(RequireSetCursorIndex);
		writer.Write(EffectEnable);
		writer.Write(HasEffectParams);

		if (HasEffectParams.GetBit(Const.SCREEN_EFFECT_RETRO_DARKEN)) {
			writer.Write(e_DarkenAmount);
			writer.Write(e_DarkenStep);
		}
		if (HasEffectParams.GetBit(Const.SCREEN_EFFECT_RETRO_LIGHTEN)) {
			writer.Write(e_LightenAmount);
			writer.Write(e_LightenStep);
		}
		if (HasEffectParams.GetBit(Const.SCREEN_EFFECT_TINT)) {
			writer.Write(e_TintColor.r);
			writer.Write(e_TintColor.g);
			writer.Write(e_TintColor.b);
			writer.Write(e_TintColor.a);
		}
		if (HasEffectParams.GetBit(Const.SCREEN_EFFECT_VIGNETTE)) {
			writer.Write(e_VigRadius);
			writer.Write(e_VigFeather);
			writer.Write(e_VigOffsetX);
			writer.Write(e_VigOffsetY);
			writer.Write(e_VigRound);
		}

		writer.Write(RequirePlayMusicID);
		writer.Write(AudioActionRequirement);
		writer.Write(RequireSetMusicVolume);
		writer.Write(RequirePlaySoundID);
		writer.Write(RequirePlaySoundVolume);
		writer.Write(RequireSetSoundVolume);





	}


}