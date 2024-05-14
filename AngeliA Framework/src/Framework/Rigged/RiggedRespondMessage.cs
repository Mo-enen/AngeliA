using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace AngeliA;

public class RiggedRespondMessage {




	#region --- SUB ---


	public struct GizmosRectData {
		public IRect Rect;
		public Color32 Color;
	}


	public struct GizmosTextureData {
		public IRect Rect;
		public FRect Uv;
		public bool Inverse;
		public uint TextureRigID;
		public int PngDataLength;
		public byte[] PngData;
	}



	public class RenderingCellData {
		public int SpriteID;
		public char TextSpriteChar;
		public int X;
		public int Y;
		public int Z;
		public int Width;
		public int Height;
		public int Rotation1000;
		public float PivotX;
		public float PivotY;
		public Color32 Color;
		public Int4 Shift;
		public Alignment BorderSide;
	}


	public class RenderingLayerData {
		public int CellCount;
		public readonly RenderingCellData[] Cells;
		public RenderingLayerData (int capacity) {
			CellCount = 0;
			Cells = new RenderingCellData[capacity].FillWithNewValue();
		}
	}


	#endregion




	#region --- VAR ---


	// Const
	public const int REQUIRE_CHAR_MAX_COUNT = 64;
	public const int REQUIRE_GIZMOS_MAX_COUNT = 1024;

	// Pipe
	public int ViewX;
	public int ViewY;
	public int ViewWidth;
	public int ViewHeight;
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
	public int CharRequiringCount;
	public char[] RequireChars = new char[REQUIRE_CHAR_MAX_COUNT];
	public int[] RequireCharsFontIndex = new int[REQUIRE_CHAR_MAX_COUNT];
	public int RequireGizmosRectCount;
	public GizmosRectData[] RequireGizmosRects = new GizmosRectData[REQUIRE_GIZMOS_MAX_COUNT];
	public int RequireGizmosTextureCount;
	public GizmosTextureData[] RequireGizmosTextures = new GizmosTextureData[REQUIRE_GIZMOS_MAX_COUNT];
	public RenderingLayerData[] Layers = new RenderingLayerData[RenderLayer.COUNT];

	// Data
	private readonly Dictionary<uint, object> GizmosTexturePool = new();


	#endregion




	#region --- API ---


	public void TransationStart () {
		GizmosTexturePool.Clear();
		for (int i = 0; i < RenderLayer.COUNT; i++) {
			if (Layers[i] == null) {
				Layers[i] = new RenderingLayerData(Renderer.GetLayerCapacity(i));
			}
			Layers[i].CellCount = 0;
		}
	}


	public void Reset () {
		RequireSetCursorIndex = int.MinValue;
		HasEffectParams = 0;
		RequirePlayMusicID = 0;
		AudioActionRequirement = 0;
		RequireSetMusicVolume = -1;
		RequirePlaySoundID = 0;
		RequirePlaySoundVolume = -1f;
		RequireSetSoundVolume = -1;
		CharRequiringCount = 0;
		RequireGizmosRectCount = 0;
		RequireGizmosTextureCount = 0;
	}


	public void ApplyToEngine (RiggedCallingMessage callingMessage, int sheetIndex) {

		// View
		ViewWidth = ViewWidth.GreaterOrEquel(Const.CEL * 4);
		ViewHeight = ViewHeight.GreaterOrEquel(Const.CEL * 4);
		Stage.SetViewRectImmediately(
			new IRect(ViewX, ViewY, ViewWidth, ViewHeight),
			remapAllRenderingCells: true
		);

		// Cursor
		if (RequireSetCursorIndex != int.MinValue) {
			if (RequireSetCursorIndex == -3) {
				Game.SetCursorToNormal();
			} else {
				Game.SetCursor(RequireSetCursorIndex);
			}
		}

		// Char Requirement
		callingMessage.CharRequiredCount = CharRequiringCount;
		for (int i = 0; i < CharRequiringCount; i++) {
			char c = RequireChars[i];
			int fontIndex = RequireCharsFontIndex[i];
			if (Game.GetCharSprite(fontIndex, c, out var sprite)) {
				callingMessage.RequiredChars[i] = new() {
					Valid = true,
					Char = c,
					FontIndex = fontIndex,
					Advance = sprite.Advance,
					Offset = sprite.Offset,
				};
			} else {
				callingMessage.RequiredChars[i] = new() {
					Char = c,
					FontIndex = fontIndex,
					Valid = false,
				};
			}
		}

		// Gizmos
		callingMessage.RequiringGizmosTextureIDCount = 0;
		for (int i = 0; i < RequireGizmosRectCount; i++) {
			var data = RequireGizmosRects[i];
			Game.DrawGizmosRect(data.Rect, data.Color);
		}
		for (int i = 0; i < RequireGizmosTextureCount; i++) {
			var data = RequireGizmosTextures[i];
			if (!GizmosTexturePool.TryGetValue(data.TextureRigID, out var texture)) {
				texture = null;
				if (data.PngData != null) {
					// Add Texture to Pool
					texture = Game.PngBytesToTexture(data.PngData);
					GizmosTexturePool.Add(data.TextureRigID, texture);
				} else if (callingMessage.RequiringGizmosTextureIDCount < RiggedCallingMessage.REQUIRE_GIZMOS_TEXTURE_MAX_COUNT) {
					// Require Back for the Texture
					callingMessage.RequiringGizmosTextureIDs[callingMessage.RequiringGizmosTextureIDCount] = data.TextureRigID;
					callingMessage.RequiringGizmosTextureIDCount++;
				}
			}
			// Draw Texture
			if (texture != null) {
				Game.DrawGizmosTexture(data.Rect, data.Uv, texture, data.Inverse);
			}
		}

		// Message Layer/Cells >> Renderer Layer/Cells
		int oldLayer = Renderer.CurrentLayerIndex;
		int oldSheetIndex = Renderer.CurrentSheetIndex;
		Renderer.CurrentSheetIndex = sheetIndex;
		for (int layer = 0; layer < RenderLayer.COUNT; layer++) {
			var layerData = Layers[layer];
			int count = layerData.CellCount;
			Renderer.SetLayer(layer);
			for (int i = 0; i < count; i++) {
				var cell = layerData.Cells[i];
				Cell rCell = null;
				if (cell.SpriteID != 0) {
					if (Renderer.TryGetSprite(cell.SpriteID, out var sprite, ignoreAnimation: true)) {
						rCell = Renderer.Draw(sprite, default);
					}
				} else if (cell.TextSpriteChar != '\0') {
					rCell = Renderer.DrawChar(cell.TextSpriteChar, 0, 0, 1, 1, Color32.WHITE);
					if (rCell.TextSprite == null) rCell = null;
				}
				if (rCell == null) continue;
				rCell.X = cell.X;
				rCell.Y = cell.Y;
				rCell.Z = cell.Z;
				rCell.Width = cell.Width;
				rCell.Height = cell.Height;
				rCell.Rotation1000 = cell.Rotation1000;
				rCell.PivotX = cell.PivotX;
				rCell.PivotY = cell.PivotY;
				rCell.Color = cell.Color;
				rCell.Shift = cell.Shift;
				rCell.BorderSide = cell.BorderSide;
			}
		}
		Renderer.SetLayer(oldLayer);
		Renderer.CurrentSheetIndex = oldSheetIndex;

		// Audio
		if (RequirePlayMusicID != 0) {
			Game.PlayMusic(RequirePlayMusicID);
		}
		if (RequirePlaySoundID != 0) {
			Game.PlaySound(RequirePlaySoundID, RequirePlaySoundVolume);
		}
		if (AudioActionRequirement.GetBit(0)) {
			Game.StopMusic();
		}
		if (AudioActionRequirement.GetBit(1)) {
			Game.PauseMusic();
		}
		if (AudioActionRequirement.GetBit(2)) {
			Game.UnpauseMusic();
		}
		if (AudioActionRequirement.GetBit(3)) {
			Game.StopAllSounds();
		}
		if (RequireSetMusicVolume >= 0) {
			Game.SetMusicVolume(RequireSetMusicVolume);
		}
		if (RequireSetSoundVolume >= 0) {
			Game.SetSoundVolume(RequireSetSoundVolume);
		}

		// Effect
		for (int i = 0; i < Const.SCREEN_EFFECT_COUNT; i++) {
			Game.SetEffectEnable(i, EffectEnable.GetBit(i));
		}
		if (Game.GetEffectEnable(Const.SCREEN_EFFECT_RETRO_DARKEN) && HasEffectParams.GetBit(Const.SCREEN_EFFECT_RETRO_DARKEN)) {
			Game.PassEffect_RetroDarken(e_DarkenAmount, e_DarkenStep, 1);
		}
		if (Game.GetEffectEnable(Const.SCREEN_EFFECT_RETRO_LIGHTEN) && HasEffectParams.GetBit(Const.SCREEN_EFFECT_RETRO_LIGHTEN)) {
			Game.PassEffect_RetroLighten(e_LightenAmount, e_LightenStep, 1);
		}
		if (Game.GetEffectEnable(Const.SCREEN_EFFECT_TINT) && HasEffectParams.GetBit(Const.SCREEN_EFFECT_TINT)) {
			Game.PassEffect_Tint(e_TintColor, 1);
		}
		if (Game.GetEffectEnable(Const.SCREEN_EFFECT_VIGNETTE) && HasEffectParams.GetBit(Const.SCREEN_EFFECT_VIGNETTE)) {
			Game.PassEffect_Vignette(e_VigRadius, e_VigFeather, e_VigOffsetX, e_VigOffsetY, e_VigRound, 1);
		}


	}


	public unsafe void ReadDataFromPipe (byte* start, int maxSize) {

		//ViewX = reader.ReadInt32();
		//ViewY = reader.ReadInt32();
		//ViewWidth = reader.ReadInt32();
		//ViewHeight = reader.ReadInt32();
		//RequireSetCursorIndex = reader.ReadInt32();
		//EffectEnable = reader.ReadByte();
		//HasEffectParams = reader.ReadByte();
		//
		//if (HasEffectParams.GetBit(Const.SCREEN_EFFECT_RETRO_DARKEN)) {
		//	e_DarkenAmount = reader.ReadSingle();
		//	e_DarkenStep = reader.ReadSingle();
		//}
		//if (HasEffectParams.GetBit(Const.SCREEN_EFFECT_RETRO_LIGHTEN)) {
		//	e_LightenAmount = reader.ReadSingle();
		//	e_LightenStep = reader.ReadSingle();
		//}
		//if (HasEffectParams.GetBit(Const.SCREEN_EFFECT_TINT)) {
		//	e_TintColor.r = reader.ReadByte();
		//	e_TintColor.g = reader.ReadByte();
		//	e_TintColor.b = reader.ReadByte();
		//	e_TintColor.a = reader.ReadByte();
		//}
		//if (HasEffectParams.GetBit(Const.SCREEN_EFFECT_VIGNETTE)) {
		//	e_VigRadius = reader.ReadSingle();
		//	e_VigFeather = reader.ReadSingle();
		//	e_VigOffsetX = reader.ReadSingle();
		//	e_VigOffsetY = reader.ReadSingle();
		//	e_VigRound = reader.ReadSingle();
		//}
		//
		//RequirePlayMusicID = reader.ReadInt32();
		//AudioActionRequirement = reader.ReadByte();
		//RequireSetMusicVolume = reader.ReadInt32();
		//RequirePlaySoundID = reader.ReadInt32();
		//RequirePlaySoundVolume = reader.ReadSingle();
		//RequireSetSoundVolume = reader.ReadInt32();
		//
		//CharRequiringCount = reader.ReadInt32();
		//for (int i = 0; i < CharRequiringCount; i++) {
		//	RequireCharsFontIndex[i] = reader.ReadInt32();
		//	RequireChars[i] = reader.ReadChar();
		//}
		//
		//RequireGizmosRectCount = reader.ReadInt32();
		//for (int i = 0; i < RequireGizmosRectCount; i++) {
		//	var rect = new IRect(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
		//	var color = new Color32(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
		//	RequireGizmosRects[i] = new GizmosRectData() {
		//		Rect = rect,
		//		Color = color,
		//	};
		//}
		//
		//RequireGizmosTextureCount = reader.ReadInt32();
		//for (int i = 0; i < RequireGizmosTextureCount; i++) {
		//	uint id = reader.ReadUInt32();
		//	var rect = new IRect(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
		//	var uv = new FRect(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
		//	var inverse = reader.ReadBoolean();
		//	int pngLength = reader.ReadInt32();
		//	var png = pngLength > 0 ? reader.ReadBytes(pngLength) : null;
		//	RequireGizmosTextures[i] = new GizmosTextureData() {
		//		TextureRigID = id,
		//		Rect = rect,
		//		Uv = uv,
		//		Inverse = inverse,
		//		PngDataLength = pngLength,
		//		PngData = png,
		//	};
		//}
		//
		//for (int index = 0; index < RenderLayer.COUNT; index++) {
		//	var layer = Layers[index];
		//	layer.CellCount = reader.ReadInt32();
		//	for (int i = 0; i < layer.CellCount; i++) {
		//		var cell = layer.Cells[i];
		//		cell.SpriteID = reader.ReadInt32();
		//		cell.TextSpriteChar = reader.ReadChar();
		//		cell.X = reader.ReadInt32();
		//		cell.Y = reader.ReadInt32();
		//		cell.Z = reader.ReadInt32();
		//		cell.Width = reader.ReadInt32();
		//		cell.Height = reader.ReadInt32();
		//		cell.Rotation1000 = reader.ReadInt32();
		//		cell.PivotX = reader.ReadSingle();
		//		cell.PivotY = reader.ReadSingle();
		//		cell.Color.r = reader.ReadByte();
		//		cell.Color.g = reader.ReadByte();
		//		cell.Color.b = reader.ReadByte();
		//		cell.Color.a = reader.ReadByte();
		//		cell.Shift.left = reader.ReadInt32();
		//		cell.Shift.right = reader.ReadInt32();
		//		cell.Shift.down = reader.ReadInt32();
		//		cell.Shift.up = reader.ReadInt32();
		//		cell.BorderSide = (Alignment)reader.ReadInt32();
		//	}
		//}

	}


	public unsafe void WriteDataToPipe (byte* start, int maxSize) {

		//writer.Write(ViewX);
		//writer.Write(ViewY);
		//writer.Write(ViewWidth);
		//writer.Write(ViewHeight);
		//writer.Write(RequireSetCursorIndex);
		//writer.Write(EffectEnable);
		//writer.Write(HasEffectParams);
		//
		//if (HasEffectParams.GetBit(Const.SCREEN_EFFECT_RETRO_DARKEN)) {
		//	writer.Write(e_DarkenAmount);
		//	writer.Write(e_DarkenStep);
		//}
		//if (HasEffectParams.GetBit(Const.SCREEN_EFFECT_RETRO_LIGHTEN)) {
		//	writer.Write(e_LightenAmount);
		//	writer.Write(e_LightenStep);
		//}
		//if (HasEffectParams.GetBit(Const.SCREEN_EFFECT_TINT)) {
		//	writer.Write(e_TintColor.r);
		//	writer.Write(e_TintColor.g);
		//	writer.Write(e_TintColor.b);
		//	writer.Write(e_TintColor.a);
		//}
		//if (HasEffectParams.GetBit(Const.SCREEN_EFFECT_VIGNETTE)) {
		//	writer.Write(e_VigRadius);
		//	writer.Write(e_VigFeather);
		//	writer.Write(e_VigOffsetX);
		//	writer.Write(e_VigOffsetY);
		//	writer.Write(e_VigRound);
		//}
		//
		//writer.Write(RequirePlayMusicID);
		//writer.Write(AudioActionRequirement);
		//writer.Write(RequireSetMusicVolume);
		//writer.Write(RequirePlaySoundID);
		//writer.Write(RequirePlaySoundVolume);
		//writer.Write(RequireSetSoundVolume);
		//
		//writer.Write(CharRequiringCount);
		//for (int i = 0; i < CharRequiringCount; i++) {
		//	writer.Write(RequireCharsFontIndex[i]);
		//	writer.Write(RequireChars[i]);
		//}
		//
		//writer.Write(RequireGizmosRectCount);
		//for (int i = 0; i < RequireGizmosRectCount; i++) {
		//	var data = RequireGizmosRects[i];
		//	writer.Write(data.Rect.x);
		//	writer.Write(data.Rect.y);
		//	writer.Write(data.Rect.width);
		//	writer.Write(data.Rect.height);
		//	writer.Write(data.Color.r);
		//	writer.Write(data.Color.g);
		//	writer.Write(data.Color.b);
		//	writer.Write(data.Color.a);
		//}
		//
		//writer.Write(RequireGizmosTextureCount);
		//for (int i = 0; i < RequireGizmosTextureCount; i++) {
		//	var data = RequireGizmosTextures[i];
		//	writer.Write(data.TextureRigID);
		//	writer.Write(data.Rect.x);
		//	writer.Write(data.Rect.y);
		//	writer.Write(data.Rect.width);
		//	writer.Write(data.Rect.height);
		//	writer.Write(data.Uv.x);
		//	writer.Write(data.Uv.y);
		//	writer.Write(data.Uv.width);
		//	writer.Write(data.Uv.height);
		//	writer.Write(data.Inverse);
		//	writer.Write(data.PngDataLength);
		//	if (data.PngDataLength > 0) {
		//		writer.Write(data.PngData);
		//	}
		//}
		//
		//for (int index = 0; index < RenderLayer.COUNT; index++) {
		//	var layer = Layers[index];
		//	writer.Write(layer.CellCount);
		//	for (int i = 0; i < layer.CellCount; i++) {
		//		var cell = layer.Cells[i];
		//		writer.Write(cell.SpriteID);
		//		writer.Write(cell.TextSpriteChar);
		//		writer.Write(cell.X);
		//		writer.Write(cell.Y);
		//		writer.Write(cell.Z);
		//		writer.Write(cell.Width);
		//		writer.Write(cell.Height);
		//		writer.Write(cell.Rotation1000);
		//		writer.Write(cell.PivotX);
		//		writer.Write(cell.PivotY);
		//		writer.Write(cell.Color.r);
		//		writer.Write(cell.Color.g);
		//		writer.Write(cell.Color.b);
		//		writer.Write(cell.Color.a);
		//		writer.Write(cell.Shift.left);
		//		writer.Write(cell.Shift.right);
		//		writer.Write(cell.Shift.down);
		//		writer.Write(cell.Shift.up);
		//		writer.Write((int)cell.BorderSide);
		//	}
		//}


	}


	#endregion




}