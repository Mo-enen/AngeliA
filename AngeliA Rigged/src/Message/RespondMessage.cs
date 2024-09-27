using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AngeliA;

namespace AngeliaRigged;

public class RigRespondMessage {




	#region --- SUB ---


	public struct GizmosRectData {
		public IRect Rect;
		public Color32 ColorTL;
		public Color32 ColorTR;
		public Color32 ColorBL;
		public Color32 ColorBR;
	}


	public struct GizmosLineData {
		public Int2 Start;
		public Int2 End;
		public int Thickness;
		public Color32 Color;
	}


	public struct GizmosMapData {
		public IRect Rect;
		public FRect Uv;
		public Int3 MapPos;
	}


	public class RenderingCellData {
		public int SpriteID;
		public char TextSpriteChar;
		public int FontIndex;
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


	public class RenderingLayerData (int capacity) {
		public int CellCount = 0;
		public readonly RenderingCellData[] Cells = new RenderingCellData[capacity].FillWithNewValue();
		public byte layerAlpha = 255;
	}


	#endregion




	#region --- VAR ---


	// Const
	public const int REQUIRE_CHAR_MAX_COUNT = 64;

	// Api
	public int RespondCount { get; private set; } = 0;

	// Pipe
	public int ViewX;
	public int ViewY;
	public int ViewZ;
	public int ViewWidth;
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
	public int RequireGizmosLineCount;
	public GizmosLineData[] RequireGizmosLines = new GizmosLineData[256 * 256];
	public RenderingLayerData[] Layers = new RenderingLayerData[RenderLayer.COUNT];
	public int[] RenderUsages = new int[RenderLayer.COUNT];
	public int[] EntityUsages = new int[EntityLayer.COUNT];
	public int[] RenderCapacities = new int[RenderLayer.COUNT];
	public int[] EntityCapacities = new int[EntityLayer.COUNT];
	public bool GamePlaying;
	public int MusicVolume;
	public int SoundVolume;
	public bool IsTyping;
	public int SelectingPlayerID;

	// Data
	private readonly Dictionary<uint, object> GizmosTexturePool = [];
	private int CachedScreenWidth = 1;
	private int CachedScreenHeight = 1;


	#endregion




	#region --- API ---


	public void TransationStart () {
		GizmosTexturePool.Clear();
		SkyTop.a = 255;
		SkyBottom.a = 255;
		RespondCount = 0;
	}


	public void Reset (bool clearLastRendering = false) {
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
		RequireGizmosLineCount = 0;
		if (clearLastRendering) {
			foreach (var layer in Layers) {
				if (layer == null) continue;
				layer.CellCount = 0;
				layer.layerAlpha = 255;
			}
		}
	}


	public void ApplyToEngine (RigCallingMessage callingMessage, bool ignoreMouseInput) {

		Game.MusicVolume = MusicVolume;
		Game.SoundVolume = SoundVolume;
		CachedScreenWidth = callingMessage.ScreenWidth;
		CachedScreenHeight = callingMessage.ScreenHeight;

		// Cursor
		if (!ignoreMouseInput) {
			if (RequireSetCursorIndex > -3) {
				Cursor.SetCursor(RequireSetCursorIndex, int.MinValue + 1);
			}
		}

		// Char Requirement
		int fontIndexOffset = Game.BuiltInFontCount;
		callingMessage.CharRequiredCount = CharRequiringCount;
		for (int i = 0; i < CharRequiringCount; i++) {
			char c = RequireChars[i];
			int fontIndex = RequireCharsFontIndex[i];
			if (Game.GetCharSprite(fontIndex + fontIndexOffset, c, out var sprite)) {
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

	}


	public void ApplyRenderingToEngine (Universe universe, int sheetIndex, int leftPadding) {

		// View
		var info = universe.Info;
		ViewHeight = ViewHeight.GreaterOrEquel(info.MinViewHeight);
		int oldViewHeight = Stage.ViewRect.height;
		var engineViewRect = new IRect(ViewX, ViewY, ViewWidth, ViewHeight);
		Stage.SetViewRectImmediately(engineViewRect, remapAllRenderingCells: true);
		if (oldViewHeight != ViewHeight) {
			leftPadding = leftPadding * ViewHeight / oldViewHeight;
		}

		// Gizmos
		int cameraExpand = (info.ViewRatio * ViewHeight / 1000 - engineViewRect.width) / 2;
		int gizmosOffsetX = leftPadding / 2 - cameraExpand;

		// Gizmos Rect
		for (int i = 0; i < RequireGizmosRectCount; i++) {
			var data = RequireGizmosRects[i];
			var rect = data.Rect;
			rect.x = data.Rect.x + gizmosOffsetX;
			Game.DrawGizmosRect(rect, data.ColorTL, data.ColorTR, data.ColorBL, data.ColorBR);
		}

		// Gizmos Line
		for (int i = 0; i < RequireGizmosLineCount; i++) {
			var data = RequireGizmosLines[i];
			int startX = data.Start.x + gizmosOffsetX;
			int endX = data.End.x + gizmosOffsetX;
			Game.DrawGizmosLine(startX, data.Start.y, endX, data.End.y, data.Thickness, data.Color);
		}

		int oldLayer = Renderer.CurrentLayerIndex;
		using (new SheetIndexScope(sheetIndex)) {

			// Message Layer/Cells >> Renderer Layer/Cells
			int fontIndexOffset = Game.BuiltInFontCount;
			for (int layer = 0; layer < RenderLayer.COUNT; layer++) {
				if (Layers[layer] == null) {
					Layers[layer] = new RenderingLayerData(Renderer.GetLayerCapacity(layer));
				}
				var layerData = Layers[layer];
				int count = layerData.CellCount;
				Renderer.SetLayer(layer);
				Renderer.SetLayerAlpha(layer, layerData.layerAlpha);
				for (int i = 0; i < count; i++) {
					var cell = layerData.Cells[i];
					Cell rCell = null;
					if (cell.SpriteID != 0) {
						if (Renderer.TryGetSprite(cell.SpriteID, out var sprite, ignoreAnimation: true)) {
							rCell = Renderer.Draw(sprite, default);
						}
					} else if (cell.TextSpriteChar != '\0') {
						if (Renderer.RequireCharForPool(cell.TextSpriteChar, cell.FontIndex + fontIndexOffset, out var charSprite)) {
							rCell = Renderer.DrawChar(charSprite, 0, 0, 1, 1, Color32.WHITE);
							rCell.TextSprite = charSprite;
							if (rCell.TextSprite == null) rCell = null;
						}
					}
					if (rCell == null) continue;
					rCell.X = cell.X + leftPadding / 2 - cameraExpand;
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
		}

		// Black Side Border
		Renderer.SetLayer(RenderLayer.UI);
		if (CachedScreenWidth * 1000 / CachedScreenHeight > info.ViewRatio) {
			var cameraRect = Renderer.CameraRect.ShrinkLeft(leftPadding);
			int borderWidth = Util.RemapUnclamped(
				0, CachedScreenWidth,
				0, cameraRect.width,
				CachedScreenWidth / 2 - CachedScreenHeight * info.ViewRatio / 2000
			);
			Renderer.DrawPixel(new IRect(cameraRect.x, cameraRect.y, borderWidth, cameraRect.height), Color32.BLACK, int.MaxValue);
			Renderer.DrawPixel(new IRect(cameraRect.x + cameraRect.width - borderWidth, cameraRect.y, borderWidth, cameraRect.height), Color32.BLACK, int.MaxValue);
		}

		// Set Layer Back
		Renderer.SetLayer(oldLayer);

		// Effect
		for (int i = 0; i < Const.SCREEN_EFFECT_COUNT; i++) {
			Game.SetEffectEnable(i, EffectEnable.GetBit(i));
		}
		if (Game.GetEffectEnable(Const.SCREEN_EFFECT_CHROMATIC_ABERRATION)) {
			Game.PassEffect_ChromaticAberration(1);
		}
		if (Game.GetEffectEnable(Const.SCREEN_EFFECT_GREYSCALE)) {
			Game.PassEffect_Greyscale(1);
		}
		if (Game.GetEffectEnable(Const.SCREEN_EFFECT_INVERT)) {
			Game.PassEffect_Invert(1);
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

		RespondCount++;

		try {

			byte* end = pointer + Const.RIG_BUFFER_SIZE - 2;

			ViewX = Util.ReadInt(ref pointer, end);
			ViewY = Util.ReadInt(ref pointer, end);
			ViewZ = Util.ReadInt(ref pointer, end);
			ViewWidth = Util.ReadInt(ref pointer, end);
			ViewHeight = Util.ReadInt(ref pointer, end);
			RequireSetCursorIndex = Util.ReadInt(ref pointer, end);

			SkyTop.r = Util.ReadByte(ref pointer, end);
			SkyTop.g = Util.ReadByte(ref pointer, end);
			SkyTop.b = Util.ReadByte(ref pointer, end);

			SkyBottom.r = Util.ReadByte(ref pointer, end);
			SkyBottom.g = Util.ReadByte(ref pointer, end);
			SkyBottom.b = Util.ReadByte(ref pointer, end);

			EffectEnable = Util.ReadByte(ref pointer, end);
			HasEffectParams = Util.ReadByte(ref pointer, end);

			if (HasEffectParams.GetBit(Const.SCREEN_EFFECT_RETRO_DARKEN)) {
				e_DarkenAmount = Util.ReadFloat(ref pointer, end);
				e_DarkenStep = Util.ReadFloat(ref pointer, end);
			}
			if (HasEffectParams.GetBit(Const.SCREEN_EFFECT_RETRO_LIGHTEN)) {
				e_LightenAmount = Util.ReadFloat(ref pointer, end);
				e_LightenStep = Util.ReadFloat(ref pointer, end);
			}
			if (HasEffectParams.GetBit(Const.SCREEN_EFFECT_TINT)) {
				e_TintColor.r = Util.ReadByte(ref pointer, end);
				e_TintColor.g = Util.ReadByte(ref pointer, end);
				e_TintColor.b = Util.ReadByte(ref pointer, end);
				e_TintColor.a = Util.ReadByte(ref pointer, end);
			}
			if (HasEffectParams.GetBit(Const.SCREEN_EFFECT_VIGNETTE)) {
				e_VigRadius = Util.ReadFloat(ref pointer, end);
				e_VigFeather = Util.ReadFloat(ref pointer, end);
				e_VigOffsetX = Util.ReadFloat(ref pointer, end);
				e_VigOffsetY = Util.ReadFloat(ref pointer, end);
				e_VigRound = Util.ReadFloat(ref pointer, end);
			}

			RequirePlayMusicID = Util.ReadInt(ref pointer, end);
			AudioActionRequirement = Util.ReadByte(ref pointer, end);
			RequireSetMusicVolume = Util.ReadInt(ref pointer, end);
			RequirePlaySoundID = Util.ReadInt(ref pointer, end);
			RequirePlaySoundVolume = Util.ReadFloat(ref pointer, end);
			RequireSetSoundVolume = Util.ReadInt(ref pointer, end);

			CharRequiringCount = Util.ReadInt(ref pointer, end);
			for (int i = 0; i < CharRequiringCount; i++) {
				RequireCharsFontIndex[i] = Util.ReadInt(ref pointer, end);
				RequireChars[i] = Util.ReadChar(ref pointer, end);
			}

			RequireGizmosRectCount = Util.ReadInt(ref pointer, end);
			for (int i = 0; i < RequireGizmosRectCount; i++) {
				int x = Util.ReadInt(ref pointer, end);
				int y = Util.ReadInt(ref pointer, end);
				int w = Util.ReadInt(ref pointer, end);
				int h = Util.ReadInt(ref pointer, end);
				var rect = new IRect(x, y, w, h);
				var colorTL = new Color32(
					Util.ReadByte(ref pointer, end),
					Util.ReadByte(ref pointer, end),
					Util.ReadByte(ref pointer, end),
					Util.ReadByte(ref pointer, end)
				);
				var colorTR = new Color32(
					Util.ReadByte(ref pointer, end),
					Util.ReadByte(ref pointer, end),
					Util.ReadByte(ref pointer, end),
					Util.ReadByte(ref pointer, end)
				);
				var colorBL = new Color32(
					Util.ReadByte(ref pointer, end),
					Util.ReadByte(ref pointer, end),
					Util.ReadByte(ref pointer, end),
					Util.ReadByte(ref pointer, end)
				);
				var colorBR = new Color32(
					Util.ReadByte(ref pointer, end),
					Util.ReadByte(ref pointer, end),
					Util.ReadByte(ref pointer, end),
					Util.ReadByte(ref pointer, end)
				);
				RequireGizmosRects[i] = new GizmosRectData() {
					Rect = rect,
					ColorTL = colorTL,
					ColorTR = colorTR,
					ColorBL = colorBL,
					ColorBR = colorBR,
				};
			}

			RequireGizmosLineCount = Util.ReadInt(ref pointer, end);
			for (int i = 0; i < RequireGizmosLineCount; i++) {
				int startX = Util.ReadInt(ref pointer, end);
				int startY = Util.ReadInt(ref pointer, end);
				int endX = Util.ReadInt(ref pointer, end);
				int endY = Util.ReadInt(ref pointer, end);
				int thickness = Util.ReadInt(ref pointer, end);
				var color = new Color32(
					Util.ReadByte(ref pointer, end),
					Util.ReadByte(ref pointer, end),
					Util.ReadByte(ref pointer, end),
					Util.ReadByte(ref pointer, end)
				);
				RequireGizmosLines[i] = new GizmosLineData() {
					Start = new Int2(startX, startY),
					End = new Int2(endX, endY),
					Thickness = thickness,
					Color = color,
				};
			}

			for (int index = 0; index < RenderLayer.COUNT; index++) {
				if (Layers[index] == null) {
					Layers[index] = new RenderingLayerData(Renderer.GetLayerCapacity(index));
				}
				var layer = Layers[index];
				layer.CellCount = Util.ReadInt(ref pointer, end);
				layer.layerAlpha = Util.ReadByte(ref pointer, end);
				for (int i = 0; i < layer.CellCount; i++) {
					var cell = layer.Cells[i];
					cell.SpriteID = Util.ReadInt(ref pointer, end);
					cell.TextSpriteChar = Util.ReadChar(ref pointer, end);
					cell.FontIndex = Util.ReadInt(ref pointer, end);
					cell.X = Util.ReadInt(ref pointer, end);
					cell.Y = Util.ReadInt(ref pointer, end);
					cell.Z = Util.ReadInt(ref pointer, end);
					cell.Width = Util.ReadInt(ref pointer, end);
					cell.Height = Util.ReadInt(ref pointer, end);
					cell.Rotation1000 = Util.ReadInt(ref pointer, end);
					cell.PivotX = Util.ReadFloat(ref pointer, end);
					cell.PivotY = Util.ReadFloat(ref pointer, end);
					cell.Color.r = Util.ReadByte(ref pointer, end);
					cell.Color.g = Util.ReadByte(ref pointer, end);
					cell.Color.b = Util.ReadByte(ref pointer, end);
					cell.Color.a = Util.ReadByte(ref pointer, end);
					cell.Shift.left = Util.ReadInt(ref pointer, end);
					cell.Shift.right = Util.ReadInt(ref pointer, end);
					cell.Shift.down = Util.ReadInt(ref pointer, end);
					cell.Shift.up = Util.ReadInt(ref pointer, end);
					cell.BorderSide = (Alignment)Util.ReadInt(ref pointer, end);
				}
			}

			for (int i = 0; i < RenderLayer.COUNT; i++) {
				RenderUsages[i] = Util.ReadInt(ref pointer, end);
				RenderCapacities[i] = Util.ReadInt(ref pointer, end);
			}

			for (int i = 0; i < EntityLayer.COUNT; i++) {
				EntityUsages[i] = Util.ReadInt(ref pointer, end);
				EntityCapacities[i] = Util.ReadInt(ref pointer, end);
			}

			GamePlaying = Util.ReadBool(ref pointer, end);
			MusicVolume = Util.ReadInt(ref pointer, end);
			SoundVolume = Util.ReadInt(ref pointer, end);
			IsTyping = Util.ReadBool(ref pointer, end);
			SelectingPlayerID = Util.ReadInt(ref pointer, end);

		} catch (System.Exception ex) { Debug.LogException(ex); }

	}


	public unsafe void WriteDataToPipe (byte* pointer) {

		try {

			byte* end = pointer + Const.RIG_BUFFER_SIZE - 2;

			Util.Write(ref pointer, ViewX, end);
			Util.Write(ref pointer, ViewY, end);
			Util.Write(ref pointer, ViewZ, end);
			Util.Write(ref pointer, ViewWidth, end);
			Util.Write(ref pointer, ViewHeight, end);
			Util.Write(ref pointer, RequireSetCursorIndex, end);

			Util.Write(ref pointer, SkyTop.r, end);
			Util.Write(ref pointer, SkyTop.g, end);
			Util.Write(ref pointer, SkyTop.b, end);

			Util.Write(ref pointer, SkyBottom.r, end);
			Util.Write(ref pointer, SkyBottom.g, end);
			Util.Write(ref pointer, SkyBottom.b, end);

			Util.Write(ref pointer, EffectEnable, end);
			Util.Write(ref pointer, HasEffectParams, end);

			if (HasEffectParams.GetBit(Const.SCREEN_EFFECT_RETRO_DARKEN)) {
				Util.Write(ref pointer, e_DarkenAmount, end);
				Util.Write(ref pointer, e_DarkenStep, end);
			}
			if (HasEffectParams.GetBit(Const.SCREEN_EFFECT_RETRO_LIGHTEN)) {
				Util.Write(ref pointer, e_LightenAmount, end);
				Util.Write(ref pointer, e_LightenStep, end);
			}
			if (HasEffectParams.GetBit(Const.SCREEN_EFFECT_TINT)) {
				Util.Write(ref pointer, e_TintColor.r, end);
				Util.Write(ref pointer, e_TintColor.g, end);
				Util.Write(ref pointer, e_TintColor.b, end);
				Util.Write(ref pointer, e_TintColor.a, end);
			}
			if (HasEffectParams.GetBit(Const.SCREEN_EFFECT_VIGNETTE)) {
				Util.Write(ref pointer, e_VigRadius, end);
				Util.Write(ref pointer, e_VigFeather, end);
				Util.Write(ref pointer, e_VigOffsetX, end);
				Util.Write(ref pointer, e_VigOffsetY, end);
				Util.Write(ref pointer, e_VigRound, end);
			}

			Util.Write(ref pointer, RequirePlayMusicID, end);
			Util.Write(ref pointer, AudioActionRequirement, end);
			Util.Write(ref pointer, RequireSetMusicVolume, end);
			Util.Write(ref pointer, RequirePlaySoundID, end);
			Util.Write(ref pointer, RequirePlaySoundVolume, end);
			Util.Write(ref pointer, RequireSetSoundVolume, end);

			Util.Write(ref pointer, CharRequiringCount, end);
			for (int i = 0; i < CharRequiringCount; i++) {
				Util.Write(ref pointer, RequireCharsFontIndex[i], end);
				Util.Write(ref pointer, RequireChars[i], end);
			}

			Util.Write(ref pointer, RequireGizmosRectCount, end);
			for (int i = 0; i < RequireGizmosRectCount; i++) {
				var data = RequireGizmosRects[i];
				Util.Write(ref pointer, data.Rect.x, end);
				Util.Write(ref pointer, data.Rect.y, end);
				Util.Write(ref pointer, data.Rect.width, end);
				Util.Write(ref pointer, data.Rect.height, end);
				Util.Write(ref pointer, data.ColorTL.r, end);
				Util.Write(ref pointer, data.ColorTL.g, end);
				Util.Write(ref pointer, data.ColorTL.b, end);
				Util.Write(ref pointer, data.ColorTL.a, end);
				Util.Write(ref pointer, data.ColorTR.r, end);
				Util.Write(ref pointer, data.ColorTR.g, end);
				Util.Write(ref pointer, data.ColorTR.b, end);
				Util.Write(ref pointer, data.ColorTR.a, end);
				Util.Write(ref pointer, data.ColorBL.r, end);
				Util.Write(ref pointer, data.ColorBL.g, end);
				Util.Write(ref pointer, data.ColorBL.b, end);
				Util.Write(ref pointer, data.ColorBL.a, end);
				Util.Write(ref pointer, data.ColorBR.r, end);
				Util.Write(ref pointer, data.ColorBR.g, end);
				Util.Write(ref pointer, data.ColorBR.b, end);
				Util.Write(ref pointer, data.ColorBR.a, end);
			}

			Util.Write(ref pointer, RequireGizmosLineCount, end);
			for (int i = 0; i < RequireGizmosLineCount; i++) {
				var data = RequireGizmosLines[i];
				Util.Write(ref pointer, data.Start.x, end);
				Util.Write(ref pointer, data.Start.y, end);
				Util.Write(ref pointer, data.End.x, end);
				Util.Write(ref pointer, data.End.y, end);
				Util.Write(ref pointer, data.Thickness, end);
				Util.Write(ref pointer, data.Color.r, end);
				Util.Write(ref pointer, data.Color.g, end);
				Util.Write(ref pointer, data.Color.b, end);
				Util.Write(ref pointer, data.Color.a, end);
			}

			for (int index = 0; index < RenderLayer.COUNT; index++) {
				if (Layers[index] == null) {
					Layers[index] = new RenderingLayerData(Renderer.GetLayerCapacity(index));
				}
				var layer = Layers[index];
				Util.Write(ref pointer, layer.CellCount, end);
				Util.Write(ref pointer, layer.layerAlpha, end);
				for (int i = 0; i < layer.CellCount; i++) {
					var cell = layer.Cells[i];
					Util.Write(ref pointer, cell.SpriteID, end);
					Util.Write(ref pointer, cell.TextSpriteChar, end);
					Util.Write(ref pointer, cell.FontIndex, end);
					Util.Write(ref pointer, cell.X, end);
					Util.Write(ref pointer, cell.Y, end);
					Util.Write(ref pointer, cell.Z, end);
					Util.Write(ref pointer, cell.Width, end);
					Util.Write(ref pointer, cell.Height, end);
					Util.Write(ref pointer, cell.Rotation1000, end);
					Util.Write(ref pointer, cell.PivotX, end);
					Util.Write(ref pointer, cell.PivotY, end);
					Util.Write(ref pointer, cell.Color.r, end);
					Util.Write(ref pointer, cell.Color.g, end);
					Util.Write(ref pointer, cell.Color.b, end);
					Util.Write(ref pointer, cell.Color.a, end);
					Util.Write(ref pointer, cell.Shift.left, end);
					Util.Write(ref pointer, cell.Shift.right, end);
					Util.Write(ref pointer, cell.Shift.down, end);
					Util.Write(ref pointer, cell.Shift.up, end);
					Util.Write(ref pointer, (int)cell.BorderSide, end);
				}
			}

			for (int i = 0; i < RenderLayer.COUNT; i++) {
				Util.Write(ref pointer, RenderUsages[i], end);
				Util.Write(ref pointer, RenderCapacities[i], end);
			}

			for (int i = 0; i < EntityLayer.COUNT; i++) {
				Util.Write(ref pointer, EntityUsages[i], end);
				Util.Write(ref pointer, EntityCapacities[i], end);
			}

			Util.Write(ref pointer, GamePlaying, end);
			Util.Write(ref pointer, MusicVolume, end);
			Util.Write(ref pointer, SoundVolume, end);
			Util.Write(ref pointer, IsTyping, end);
			Util.Write(ref pointer, SelectingPlayerID, end);

		} catch (System.Exception ex) { Debug.LogException(ex); }


	}


	#endregion




}