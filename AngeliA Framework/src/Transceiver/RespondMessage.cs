﻿using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

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


	public struct DoodleRectData {
		public FRect Rect;
		public Color32 Color;
	}


	public struct DoodleWorldData {
		public FRect ScreenRect;
		public IRect WorldUnitRange;
		public int Z;
		public byte IgnoreMask;
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
		public Color32 layerTint = Color32.WHITE;
	}


	public class PlaySoundRequirement {
		public int ID;
		public float Volume;
		public float Pitch;
		public float Pan;
	}


	#endregion




	#region --- VAR ---


	// Const
	public const int REQUIRE_CHAR_MAX_COUNT = 64;

	// Pipe
	public int GlobalFrame;
	public float FrameDurationMilliSecond;
	public int ViewX;
	public int ViewY;
	public int ViewZ;
	public int ViewWidth;
	public int ViewHeight;
	public int RequireSetCursorIndex;
	public int TargetFramerate;
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
	public bool RequirePlayMusicFromStart;
	public byte AudioActionRequirement;
	public int RequireSetMusicVolume;

	public int RequirePlaySoundCount;
	public PlaySoundRequirement[] PlaySoundRequirements = new PlaySoundRequirement[16].FillWithNewValue();
	public int RequireSetSoundVolume;

	public int CharRequiringCount;
	public char[] RequireChars = new char[REQUIRE_CHAR_MAX_COUNT];
	public int[] RequireCharsFontIndex = new int[REQUIRE_CHAR_MAX_COUNT];

	public int RequireGizmosRectCount;
	public GizmosRectData[] RequireGizmosRects = new GizmosRectData[256 * 256];
	public int RequireGizmosLineCount;
	public GizmosLineData[] RequireGizmosLines = new GizmosLineData[256 * 256];
	public bool RequireShowDoodle;
	public bool RequireResetDoodle;
	public Color32 RequireResetDoodleColor;
	public Float2 RequireDoodleRenderingOffset;
	public float RequireDoodleRenderingZoom;
	public int RequireDoodleRectCount;
	public DoodleRectData[] RequireDoodleRects = new DoodleRectData[64];
	public int RequireDoodleWorldCount;
	public DoodleWorldData[] RequireDoodleWorlds = new DoodleWorldData[16];

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
	private WorldStream DoodlingStream = null;


	#endregion




	#region --- API ---


	public void TransationStart () {
		GizmosTexturePool.Clear();
		SkyTop.a = 255;
		SkyBottom.a = 255;
		DoodlingStream = null;
		Game.ResetDoodle(Color32.CLEAR);
	}


	public void Reset (bool clearLastRendering = false) {
		RequireSetCursorIndex = int.MinValue;
		HasEffectParams = 0;
		RequirePlayMusicID = 0;
		RequirePlayMusicFromStart = false;
		AudioActionRequirement = 0;
		RequireSetMusicVolume = -1;
		RequirePlaySoundCount = 0;
		RequireSetSoundVolume = -1;
		CharRequiringCount = 0;
		RequireGizmosRectCount = 0;
		RequireGizmosLineCount = 0;
		RequireDoodleRectCount = 0;
		RequireDoodleWorldCount = 0;
		RequireShowDoodle = false;
		RequireResetDoodle = false;
		RequireResetDoodleColor = Color32.CLEAR;
		TargetFramerate = 60;
		RequireDoodleRenderingOffset = default;
		RequireDoodleRenderingZoom = 1f;
		if (clearLastRendering) {
			foreach (var layer in Layers) {
				if (layer == null) continue;
				layer.CellCount = 0;
				layer.layerTint = Color32.WHITE;
			}
		}
	}


	public void ApplyToEngine (RigCallingMessage callingMessage, bool ignoreMouseInput) {

		Game.MusicVolume = MusicVolume;
		Game.SoundVolume = SoundVolume;
		CachedScreenWidth = callingMessage.ScreenWidth;
		CachedScreenHeight = callingMessage.ScreenHeight;
		Game.ForceTargetFramerate(TargetFramerate, 1);

		// Cursor
		if (!ignoreMouseInput) {
			if (RequireSetCursorIndex > -3) {
				Cursor.SetCursor(RequireSetCursorIndex, 1);
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
			Game.PlayMusic(RequirePlayMusicID, RequirePlayMusicFromStart);
		}
		for (int i = 0; i < RequirePlaySoundCount; i++) {
			var req = PlaySoundRequirements[i];
			Game.PlaySound(req.ID, req.Volume, req.Pitch, req.Pan);
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


	public void ApplyRenderingToEngine (Universe universe, int sheetIndex, int leftPadding, int rightPadding, bool ignoreInGameGizmos, bool ignoreViewRect) {

		// View
		var info = universe.Info;
		int oldViewHeight = Stage.ViewRect.height;
		var engineViewRect = new IRect(ViewX, ViewY, ViewWidth, ViewHeight);
		if (!ignoreViewRect) {
			Stage.SetViewRectImmediately(engineViewRect, remapAllRenderingCells: true);
			Stage.SetViewZ(ViewZ, true);
		}
		if (oldViewHeight != ViewHeight) {
			leftPadding = leftPadding * ViewHeight / oldViewHeight;
			rightPadding = rightPadding * ViewHeight / oldViewHeight;
		}
		int paddingShiftX = (leftPadding - rightPadding) / 2;

		// Gizmos Rect
		if (!ignoreInGameGizmos) {
			for (int i = 0; i < RequireGizmosRectCount; i++) {
				var data = RequireGizmosRects[i];
				var rect = data.Rect;
				rect.x = data.Rect.x + paddingShiftX;
				if (data.ColorTL == data.ColorTR && data.ColorBL == data.ColorBR) {
					if (data.ColorTL == data.ColorBL) {
						Game.DrawGizmosRect(rect, data.ColorTL);
					} else {
						Game.DrawGizmosRect(rect, data.ColorTL, data.ColorBL);
					}
				} else {
					Game.DrawGizmosRect(rect, data.ColorTL, data.ColorTR, data.ColorBL, data.ColorBR);
				}
			}

			// Gizmos Line
			for (int i = 0; i < RequireGizmosLineCount; i++) {
				var data = RequireGizmosLines[i];
				int startX = data.Start.x + paddingShiftX;
				int endX = data.End.x + paddingShiftX;
				Game.DrawGizmosLine(startX, data.Start.y, endX, data.End.y, data.Thickness, data.Color);
			}
		}

		// Doodle
		DoodlingStream ??= WorldStream.GetOrCreateStreamFromPool(universe.BuiltInMapRoot);
		if (RequireShowDoodle) {
			Game.ShowDoodle();
		} else {
			Game.HideDoodle();
		}
		if (RequireResetDoodle) {
			Game.ResetDoodle(RequireResetDoodleColor);
			DoodlingStream.DiscardAllChanges(forceDiscard: true);
		}
		RequireShowDoodle = false;
		RequireResetDoodle = false;
		RequireResetDoodleColor = default;
		float doodleShiftX = (float)leftPadding * Game.ScreenWidth / Renderer.CameraRect.width;
		Game.DoodleScreenPadding = Int4.Direction(doodleShiftX.FloorToInt(), 0, 0, 0);
		Game.SetDoodleOffset(RequireDoodleRenderingOffset);
		Game.SetDoodleZoom(RequireDoodleRenderingZoom);

		// Doodle Rect
		for (int i = 0; i < RequireDoodleRectCount; i++) {
			var data = RequireDoodleRects[i];
			var rect = data.Rect;
			Game.DoodleRect(rect, data.Color);
		}

		// Doodle World
		using (new SheetIndexScope(sheetIndex)) {
			for (int i = 0; i < RequireDoodleWorldCount; i++) {
				var data = RequireDoodleWorlds[i];
				Game.DoodleWorld(
					DoodlingStream,
					data.ScreenRect,
					data.WorldUnitRange, data.Z,
					data.IgnoreMask.GetBit(0),
					data.IgnoreMask.GetBit(1),
					data.IgnoreMask.GetBit(2),
					data.IgnoreMask.GetBit(3)
				);
			}
		}

		// Cells
		int oldLayer = Renderer.CurrentLayerIndex;
		using (new SheetIndexScope(sheetIndex)) {

			// Message Layer/Cells >> Renderer Layer/Cells
			int fontIndexOffset = Game.BuiltInFontCount;
			for (int layer = 0; layer < RenderLayer.COUNT; layer++) {
				var layerData = Layers[layer];
				if (layerData == null) continue;
				int count = layerData.CellCount;
				Renderer.SetLayer(layer);
				Renderer.SetLayerTint(layer, layerData.layerTint);
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
					rCell.X = cell.X + paddingShiftX;
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

		try {

			byte* end = pointer + Const.RIG_BUFFER_SIZE - 2;

			GlobalFrame = Util.ReadInt(ref pointer, end);
			FrameDurationMilliSecond = Util.ReadFloat(ref pointer, end);
			ViewX = Util.ReadInt(ref pointer, end);
			ViewY = Util.ReadInt(ref pointer, end);
			ViewZ = Util.ReadInt(ref pointer, end);
			ViewWidth = Util.ReadInt(ref pointer, end);
			ViewHeight = Util.ReadInt(ref pointer, end);
			RequireSetCursorIndex = Util.ReadInt(ref pointer, end);
			TargetFramerate = Util.ReadInt(ref pointer, end);

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
			RequirePlayMusicFromStart = Util.ReadBool(ref pointer, end);
			AudioActionRequirement = Util.ReadByte(ref pointer, end);
			RequireSetMusicVolume = Util.ReadInt(ref pointer, end);
			RequireSetSoundVolume = Util.ReadInt(ref pointer, end);
			RequirePlaySoundCount = Util.ReadByte(ref pointer, end);
			for (int i = 0; i < RequirePlaySoundCount; i++) {
				var req = PlaySoundRequirements[i];
				req.ID = Util.ReadInt(ref pointer, end);
				req.Volume = Util.ReadFloat(ref pointer, end);
				req.Pitch = Util.ReadFloat(ref pointer, end);
				req.Pan = Util.ReadFloat(ref pointer, end);
			}

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

			RequireShowDoodle = Util.ReadBool(ref pointer, end);
			RequireResetDoodle = Util.ReadBool(ref pointer, end);
			if (RequireResetDoodle) {
				RequireResetDoodleColor = new Color32(
					Util.ReadByte(ref pointer, end),
					Util.ReadByte(ref pointer, end),
					Util.ReadByte(ref pointer, end),
					Util.ReadByte(ref pointer, end)
				);
			}
			RequireDoodleRenderingOffset.x = Util.ReadFloat(ref pointer, end);
			RequireDoodleRenderingOffset.y = Util.ReadFloat(ref pointer, end);
			RequireDoodleRenderingZoom = Util.ReadFloat(ref pointer, end);

			RequireDoodleRectCount = Util.ReadInt(ref pointer, end);
			for (int i = 0; i < RequireDoodleRectCount; i++) {
				float x = Util.ReadFloat(ref pointer, end);
				float y = Util.ReadFloat(ref pointer, end);
				float w = Util.ReadFloat(ref pointer, end);
				float h = Util.ReadFloat(ref pointer, end);
				var rect = new FRect(x, y, w, h);
				var color = new Color32(
					Util.ReadByte(ref pointer, end),
					Util.ReadByte(ref pointer, end),
					Util.ReadByte(ref pointer, end),
					Util.ReadByte(ref pointer, end)
				);
				RequireDoodleRects[i] = new DoodleRectData() {
					Rect = rect,
					Color = color,
				};
			}

			RequireDoodleWorldCount = Util.ReadInt(ref pointer, end);
			for (int i = 0; i < RequireDoodleWorldCount; i++) {
				float x = Util.ReadFloat(ref pointer, end);
				float y = Util.ReadFloat(ref pointer, end);
				float w = Util.ReadFloat(ref pointer, end);
				float h = Util.ReadFloat(ref pointer, end);
				var rect = new FRect(x, y, w, h);
				int _x = Util.ReadInt(ref pointer, end);
				int _y = Util.ReadInt(ref pointer, end);
				int _w = Util.ReadInt(ref pointer, end);
				int _h = Util.ReadInt(ref pointer, end);
				var range = new IRect(_x, _y, _w, _h);
				int z = Util.ReadInt(ref pointer, end);
				byte mask = Util.ReadByte(ref pointer, end);
				RequireDoodleWorlds[i] = new DoodleWorldData() {
					ScreenRect = rect,
					WorldUnitRange = range,
					Z = z,
					IgnoreMask = mask,
				};
			}

			for (int index = 0; index < RenderLayer.COUNT; index++) {
				var layerData = Layers[index];
				int targetCapacity = Util.ReadInt(ref pointer, end);
				if (layerData == null || layerData.Cells.Length != targetCapacity) {
					Layers[index] = layerData = new RenderingLayerData(targetCapacity);
				}
				layerData.CellCount = Util.ReadInt(ref pointer, end);
				byte _r = Util.ReadByte(ref pointer, end);
				byte _g = Util.ReadByte(ref pointer, end);
				byte _b = Util.ReadByte(ref pointer, end);
				byte _a = Util.ReadByte(ref pointer, end);
				var layerTint = new Color32(_r, _g, _b, _a);
				layerData.layerTint = layerTint;
				for (int i = 0; i < layerData.CellCount; i++) {
					var cell = layerData.Cells[i];
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

			Util.Write(ref pointer, GlobalFrame, end);
			Util.Write(ref pointer, FrameDurationMilliSecond, end);
			Util.Write(ref pointer, ViewX, end);
			Util.Write(ref pointer, ViewY, end);
			Util.Write(ref pointer, ViewZ, end);
			Util.Write(ref pointer, ViewWidth, end);
			Util.Write(ref pointer, ViewHeight, end);
			Util.Write(ref pointer, RequireSetCursorIndex, end);
			Util.Write(ref pointer, TargetFramerate, end);

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
			Util.Write(ref pointer, RequirePlayMusicFromStart, end);
			Util.Write(ref pointer, AudioActionRequirement, end);
			Util.Write(ref pointer, RequireSetMusicVolume, end);
			Util.Write(ref pointer, RequireSetSoundVolume, end);

			Util.Write(ref pointer, (byte)RequirePlaySoundCount, end);
			for (int i = 0; i < RequirePlaySoundCount; i++) {
				var req = PlaySoundRequirements[i];
				Util.Write(ref pointer, req.ID, end);
				Util.Write(ref pointer, req.Volume, end);
				Util.Write(ref pointer, req.Pitch, end);
				Util.Write(ref pointer, req.Pan, end);
			}

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

			Util.Write(ref pointer, RequireShowDoodle, end);
			Util.Write(ref pointer, RequireResetDoodle, end);
			if (RequireResetDoodle) {
				Util.Write(ref pointer, RequireResetDoodleColor.r, end);
				Util.Write(ref pointer, RequireResetDoodleColor.g, end);
				Util.Write(ref pointer, RequireResetDoodleColor.b, end);
				Util.Write(ref pointer, RequireResetDoodleColor.a, end);
			}
			Util.Write(ref pointer, RequireDoodleRenderingOffset.x, end);
			Util.Write(ref pointer, RequireDoodleRenderingOffset.y, end);
			Util.Write(ref pointer, RequireDoodleRenderingZoom, end);

			Util.Write(ref pointer, RequireDoodleRectCount, end);
			for (int i = 0; i < RequireDoodleRectCount; i++) {
				var data = RequireDoodleRects[i];
				Util.Write(ref pointer, data.Rect.x, end);
				Util.Write(ref pointer, data.Rect.y, end);
				Util.Write(ref pointer, data.Rect.width, end);
				Util.Write(ref pointer, data.Rect.height, end);
				Util.Write(ref pointer, data.Color.r, end);
				Util.Write(ref pointer, data.Color.g, end);
				Util.Write(ref pointer, data.Color.b, end);
				Util.Write(ref pointer, data.Color.a, end);
			}

			Util.Write(ref pointer, RequireDoodleWorldCount, end);
			for (int i = 0; i < RequireDoodleWorldCount; i++) {
				var data = RequireDoodleWorlds[i];
				Util.Write(ref pointer, data.ScreenRect.x, end);
				Util.Write(ref pointer, data.ScreenRect.y, end);
				Util.Write(ref pointer, data.ScreenRect.width, end);
				Util.Write(ref pointer, data.ScreenRect.height, end);
				Util.Write(ref pointer, data.WorldUnitRange.x, end);
				Util.Write(ref pointer, data.WorldUnitRange.y, end);
				Util.Write(ref pointer, data.WorldUnitRange.width, end);
				Util.Write(ref pointer, data.WorldUnitRange.height, end);
				Util.Write(ref pointer, data.Z, end);
				Util.Write(ref pointer, data.IgnoreMask, end);
			}

			for (int index = 0; index < RenderLayer.COUNT; index++) {
				var layerData = Layers[index];
				int targetCapacity = Renderer.GetLayerCapacity(index);
				if (layerData == null || layerData.Cells.Length != targetCapacity) {
					Layers[index] = layerData = new RenderingLayerData(targetCapacity);
				}
				Util.Write(ref pointer, layerData.Cells.Length, end);
				Util.Write(ref pointer, layerData.CellCount, end);
				Util.Write(ref pointer, layerData.layerTint.r, end);
				Util.Write(ref pointer, layerData.layerTint.g, end);
				Util.Write(ref pointer, layerData.layerTint.b, end);
				Util.Write(ref pointer, layerData.layerTint.a, end);
				for (int i = 0; i < layerData.CellCount; i++) {
					var cell = layerData.Cells[i];
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