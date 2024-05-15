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

	// Pipe
	public int ViewX;
	public int ViewY;
	public int ViewHeight;
	public int RequireSetCursorIndex;
	public Color32 SkyTop;
	public Color32 SkyBottom;
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
	public GizmosRectData[] RequireGizmosRects = new GizmosRectData[256 * 256];
	public int RequireGizmosTextureCount;
	public GizmosTextureData[] RequireGizmosTextures = new GizmosTextureData[1024];
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
		SkyTop.a = 255;
		SkyBottom.a = 255;
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


	public void ApplyToEngine (
		RiggedCallingMessage callingMessage, int sheetIndex, bool renderingOnly, bool ignoreInput, int leftPadding
	) {

		if (renderingOnly) goto _RENDER_;

		// View
		ViewHeight = ViewHeight.GreaterOrEquel(Game.MinViewHeight);
		int oldViewHeight = Stage.ViewRect.height;
		Stage.SetViewRectImmediately(
			new IRect(ViewX, ViewY, Game.GetViewWidthFromViewHeight(ViewHeight), ViewHeight),
			remapAllRenderingCells: true
		);
		if (oldViewHeight != ViewHeight) {
			leftPadding = leftPadding * ViewHeight / oldViewHeight;
		}

		// Sky
		Sky.ForceSkyboxTint(SkyTop, SkyBottom, 3);

		// Cursor
		if (!ignoreInput && RequireSetCursorIndex != int.MinValue) {
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

		// Gizmos Rect
		for (int i = 0; i < RequireGizmosRectCount; i++) {
			var data = RequireGizmosRects[i];
			Game.DrawGizmosRect(data.Rect.Shift(leftPadding / 2, 0), data.Color);
		}

		// Gizmos Texture
		callingMessage.RequiringGizmosTextureIDCount = 0;
		for (int i = 0; i < RequireGizmosTextureCount; i++) {
			var data = RequireGizmosTextures[i];
			// Get Texture
			if (!GizmosTexturePool.TryGetValue(data.TextureRigID, out var texture)) {
				texture = null;
				if (data.PngDataLength > 0) {
					texture = Game.PngBytesToTexture(data.PngData);
					GizmosTexturePool.Add(data.TextureRigID, texture);
				} else if (callingMessage.RequiringGizmosTextureIDCount < RiggedCallingMessage.REQUIRE_GIZMOS_TEXTURE_MAX_COUNT) {
					// Require Back for the Texture
					callingMessage.RequiringGizmosTextureIDs[callingMessage.RequiringGizmosTextureIDCount] = data.TextureRigID;
					callingMessage.RequiringGizmosTextureIDCount++;
				}
			} else if (data.PngDataLength > 0) {
				// Override Texture
				Game.UnloadTexture(texture);
				texture = Game.PngBytesToTexture(data.PngData);
			}
			// Draw Texture
			if (texture != null) {
				Game.DrawGizmosTexture(data.Rect.Shift(leftPadding / 2, 0), data.Uv, texture, data.Inverse);
			}
		}

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


		_RENDER_:;

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
				} else if (Renderer.RequireCharForPool(cell.TextSpriteChar, out var charSprite)) {
					rCell = Renderer.DrawChar(charSprite, 0, 0, 1, 1, Color32.WHITE);
					if (rCell.TextSprite == null) rCell = null;
				}
				if (rCell == null) continue;
				rCell.X = cell.X + leftPadding / 2;
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


	public unsafe void ReadDataFromPipe (byte* pointer) {

		try {

			ViewX = Util.ReadInt(ref pointer);
			ViewY = Util.ReadInt(ref pointer);
			ViewHeight = Util.ReadInt(ref pointer);
			RequireSetCursorIndex = Util.ReadInt(ref pointer);

			SkyTop.r = Util.ReadByte(ref pointer);
			SkyTop.g = Util.ReadByte(ref pointer);
			SkyTop.b = Util.ReadByte(ref pointer);

			SkyBottom.r = Util.ReadByte(ref pointer);
			SkyBottom.g = Util.ReadByte(ref pointer);
			SkyBottom.b = Util.ReadByte(ref pointer);

			EffectEnable = Util.ReadByte(ref pointer);
			HasEffectParams = Util.ReadByte(ref pointer);

			if (HasEffectParams.GetBit(Const.SCREEN_EFFECT_RETRO_DARKEN)) {
				e_DarkenAmount = Util.ReadFloat(ref pointer);
				e_DarkenStep = Util.ReadFloat(ref pointer);
			}
			if (HasEffectParams.GetBit(Const.SCREEN_EFFECT_RETRO_LIGHTEN)) {
				e_LightenAmount = Util.ReadFloat(ref pointer);
				e_LightenStep = Util.ReadFloat(ref pointer);
			}
			if (HasEffectParams.GetBit(Const.SCREEN_EFFECT_TINT)) {
				e_TintColor.r = Util.ReadByte(ref pointer);
				e_TintColor.g = Util.ReadByte(ref pointer);
				e_TintColor.b = Util.ReadByte(ref pointer);
				e_TintColor.a = Util.ReadByte(ref pointer);
			}
			if (HasEffectParams.GetBit(Const.SCREEN_EFFECT_VIGNETTE)) {
				e_VigRadius = Util.ReadFloat(ref pointer);
				e_VigFeather = Util.ReadFloat(ref pointer);
				e_VigOffsetX = Util.ReadFloat(ref pointer);
				e_VigOffsetY = Util.ReadFloat(ref pointer);
				e_VigRound = Util.ReadFloat(ref pointer);
			}

			RequirePlayMusicID = Util.ReadInt(ref pointer);
			AudioActionRequirement = Util.ReadByte(ref pointer);
			RequireSetMusicVolume = Util.ReadInt(ref pointer);
			RequirePlaySoundID = Util.ReadInt(ref pointer);
			RequirePlaySoundVolume = Util.ReadFloat(ref pointer);
			RequireSetSoundVolume = Util.ReadInt(ref pointer);

			CharRequiringCount = Util.ReadInt(ref pointer);
			for (int i = 0; i < CharRequiringCount; i++) {
				RequireCharsFontIndex[i] = Util.ReadInt(ref pointer);
				RequireChars[i] = Util.ReadChar(ref pointer);
			}

			RequireGizmosRectCount = Util.ReadInt(ref pointer);
			for (int i = 0; i < RequireGizmosRectCount; i++) {
				int x = Util.ReadInt(ref pointer);
				int y = Util.ReadInt(ref pointer);
				int w = Util.ReadInt(ref pointer);
				int h = Util.ReadInt(ref pointer);
				byte r = Util.ReadByte(ref pointer);
				byte g = Util.ReadByte(ref pointer);
				byte b = Util.ReadByte(ref pointer);
				byte a = Util.ReadByte(ref pointer);
				var rect = new IRect(x, y, w, h);
				var color = new Color32(r, g, b, a);
				RequireGizmosRects[i] = new GizmosRectData() {
					Rect = rect,
					Color = color,
				};
			}

			RequireGizmosTextureCount = Util.ReadInt(ref pointer);
			for (int i = 0; i < RequireGizmosTextureCount; i++) {
				uint id = Util.ReadUInt(ref pointer);
				var rect = new IRect(Util.ReadInt(ref pointer), Util.ReadInt(ref pointer), Util.ReadInt(ref pointer), Util.ReadInt(ref pointer));
				var uv = new FRect(Util.ReadFloat(ref pointer), Util.ReadFloat(ref pointer), Util.ReadFloat(ref pointer), Util.ReadFloat(ref pointer));
				var inverse = Util.ReadBool(ref pointer);
				int pngLength = Util.ReadInt(ref pointer);
				var png = pngLength > 0 ? Util.ReadBytes(ref pointer, pngLength) : null;
				RequireGizmosTextures[i] = new GizmosTextureData() {
					TextureRigID = id,
					Rect = rect,
					Uv = uv,
					Inverse = inverse,
					PngDataLength = pngLength,
					PngData = png,
				};
			}

			for (int index = 0; index < RenderLayer.COUNT; index++) {
				var layer = Layers[index];
				layer.CellCount = Util.ReadInt(ref pointer);
				for (int i = 0; i < layer.CellCount; i++) {
					var cell = layer.Cells[i];
					cell.SpriteID = Util.ReadInt(ref pointer);
					cell.TextSpriteChar = Util.ReadChar(ref pointer);
					cell.X = Util.ReadInt(ref pointer);
					cell.Y = Util.ReadInt(ref pointer);
					cell.Z = Util.ReadInt(ref pointer);
					cell.Width = Util.ReadInt(ref pointer);
					cell.Height = Util.ReadInt(ref pointer);
					cell.Rotation1000 = Util.ReadInt(ref pointer);
					cell.PivotX = Util.ReadFloat(ref pointer);
					cell.PivotY = Util.ReadFloat(ref pointer);
					cell.Color.r = Util.ReadByte(ref pointer);
					cell.Color.g = Util.ReadByte(ref pointer);
					cell.Color.b = Util.ReadByte(ref pointer);
					cell.Color.a = Util.ReadByte(ref pointer);
					cell.Shift.left = Util.ReadInt(ref pointer);
					cell.Shift.right = Util.ReadInt(ref pointer);
					cell.Shift.down = Util.ReadInt(ref pointer);
					cell.Shift.up = Util.ReadInt(ref pointer);
					cell.BorderSide = (Alignment)Util.ReadInt(ref pointer);
				}
			}
		} catch (System.Exception ex) { Debug.LogException(ex); }

	}


	public unsafe void WriteDataToPipe (byte* pointer) {

		try {

			Util.Write(ref pointer, ViewX);
			Util.Write(ref pointer, ViewY);
			Util.Write(ref pointer, ViewHeight);
			Util.Write(ref pointer, RequireSetCursorIndex);

			Util.Write(ref pointer, SkyTop.r);
			Util.Write(ref pointer, SkyTop.g);
			Util.Write(ref pointer, SkyTop.b);

			Util.Write(ref pointer, SkyBottom.r);
			Util.Write(ref pointer, SkyBottom.g);
			Util.Write(ref pointer, SkyBottom.b);

			Util.Write(ref pointer, EffectEnable);
			Util.Write(ref pointer, HasEffectParams);

			if (HasEffectParams.GetBit(Const.SCREEN_EFFECT_RETRO_DARKEN)) {
				Util.Write(ref pointer, e_DarkenAmount);
				Util.Write(ref pointer, e_DarkenStep);
			}
			if (HasEffectParams.GetBit(Const.SCREEN_EFFECT_RETRO_LIGHTEN)) {
				Util.Write(ref pointer, e_LightenAmount);
				Util.Write(ref pointer, e_LightenStep);
			}
			if (HasEffectParams.GetBit(Const.SCREEN_EFFECT_TINT)) {
				Util.Write(ref pointer, e_TintColor.r);
				Util.Write(ref pointer, e_TintColor.g);
				Util.Write(ref pointer, e_TintColor.b);
				Util.Write(ref pointer, e_TintColor.a);
			}
			if (HasEffectParams.GetBit(Const.SCREEN_EFFECT_VIGNETTE)) {
				Util.Write(ref pointer, e_VigRadius);
				Util.Write(ref pointer, e_VigFeather);
				Util.Write(ref pointer, e_VigOffsetX);
				Util.Write(ref pointer, e_VigOffsetY);
				Util.Write(ref pointer, e_VigRound);
			}

			Util.Write(ref pointer, RequirePlayMusicID);
			Util.Write(ref pointer, AudioActionRequirement);
			Util.Write(ref pointer, RequireSetMusicVolume);
			Util.Write(ref pointer, RequirePlaySoundID);
			Util.Write(ref pointer, RequirePlaySoundVolume);
			Util.Write(ref pointer, RequireSetSoundVolume);

			Util.Write(ref pointer, CharRequiringCount);
			for (int i = 0; i < CharRequiringCount; i++) {
				Util.Write(ref pointer, RequireCharsFontIndex[i]);
				Util.Write(ref pointer, RequireChars[i]);
			}

			Util.Write(ref pointer, RequireGizmosRectCount);
			for (int i = 0; i < RequireGizmosRectCount; i++) {
				var data = RequireGizmosRects[i];
				Util.Write(ref pointer, data.Rect.x);
				Util.Write(ref pointer, data.Rect.y);
				Util.Write(ref pointer, data.Rect.width);
				Util.Write(ref pointer, data.Rect.height);
				Util.Write(ref pointer, data.Color.r);
				Util.Write(ref pointer, data.Color.g);
				Util.Write(ref pointer, data.Color.b);
				Util.Write(ref pointer, data.Color.a);
			}

			Util.Write(ref pointer, RequireGizmosTextureCount);
			for (int i = 0; i < RequireGizmosTextureCount; i++) {
				var data = RequireGizmosTextures[i];
				Util.Write(ref pointer, data.TextureRigID);
				Util.Write(ref pointer, data.Rect.x);
				Util.Write(ref pointer, data.Rect.y);
				Util.Write(ref pointer, data.Rect.width);
				Util.Write(ref pointer, data.Rect.height);
				Util.Write(ref pointer, data.Uv.x);
				Util.Write(ref pointer, data.Uv.y);
				Util.Write(ref pointer, data.Uv.width);
				Util.Write(ref pointer, data.Uv.height);
				Util.Write(ref pointer, data.Inverse);
				Util.Write(ref pointer, data.PngDataLength);
				if (data.PngDataLength > 0) {
					Util.Write(ref pointer, data.PngData);
				}
			}

			for (int index = 0; index < RenderLayer.COUNT; index++) {
				var layer = Layers[index];
				Util.Write(ref pointer, layer.CellCount);
				for (int i = 0; i < layer.CellCount; i++) {
					var cell = layer.Cells[i];
					Util.Write(ref pointer, cell.SpriteID);
					Util.Write(ref pointer, cell.TextSpriteChar);
					Util.Write(ref pointer, cell.X);
					Util.Write(ref pointer, cell.Y);
					Util.Write(ref pointer, cell.Z);
					Util.Write(ref pointer, cell.Width);
					Util.Write(ref pointer, cell.Height);
					Util.Write(ref pointer, cell.Rotation1000);
					Util.Write(ref pointer, cell.PivotX);
					Util.Write(ref pointer, cell.PivotY);
					Util.Write(ref pointer, cell.Color.r);
					Util.Write(ref pointer, cell.Color.g);
					Util.Write(ref pointer, cell.Color.b);
					Util.Write(ref pointer, cell.Color.a);
					Util.Write(ref pointer, cell.Shift.left);
					Util.Write(ref pointer, cell.Shift.right);
					Util.Write(ref pointer, cell.Shift.down);
					Util.Write(ref pointer, cell.Shift.up);
					Util.Write(ref pointer, (int)cell.BorderSide);
				}
			}
		} catch (System.Exception ex) { Debug.LogException(ex); }


	}


	#endregion




}