using System.Collections;
using System.Collections.Generic;
using System.IO;
using AngeliA;
using AngeliaRaylib;

namespace AngeliaRigged;

public class RiggedGame : Game {




	#region --- VAR ---


	// Api
	public readonly RiggedCallingMessage CallingMessage = new();
	public readonly RiggedRespondMessage RespondMessage = new();


	#endregion




	#region --- MSG ---


	public void UpdateWithPipe (BinaryReader reader, BinaryWriter writer) {

		CallingMessage.ReadDataFromPipe(reader);

		// Reset
		RespondMessage.Reset();
		RespondMessage.EffectEnable = CallingMessage.EffectEnable;

		// Update
		RespondMessage.GlobalFrame = CallingMessage.GlobalFrame;
		RespondMessage.PauselessFrame = CallingMessage.PauselessFrame;









		// Finish
		RespondMessage.WriteDataToPipe(writer);

	}


	// System
	protected override void _SetFullscreen (bool fullScreen) { }

	protected override int _GetScreenWidth () => CallingMessage.ScreenWidth;

	protected override int _GetScreenHeight () => CallingMessage.ScreenHeight;

	protected override void _QuitApplication () { }

	protected override void _OpenUrl (string url) { }

	// Window
	protected override bool _IsWindowFocused () => true;

	protected override void _MakeWindowFocused () { }

	protected override void _SetWindowPosition (int x, int y) { }

	protected override Int2 _GetWindowPosition () => Int2.zero;

	protected override void _SetWindowSize (int x, int y) { }

	protected override int _GetCurrentMonitor () => 0;

	protected override int _GetMonitorHeight (int monitor) => CallingMessage.MonitorWidth;

	protected override int _GetMonitorWidth (int monitor) => CallingMessage.MonitorHeight;

	protected override bool _GetWindowDecorated () => true;
	protected override void _SetWindowDecorated (bool decorated) { }

	protected override bool _GetWindowResizable () => true;
	protected override void _SetWindowResizable (bool resizable) { }

	protected override bool _GetWindowTopmost () => false;
	protected override void _SetWindowTopmost (bool topmost) { }

	protected override bool _GetWindowMaximized () => false;

	protected override void _SetWindowMaximized (bool maximized) { }

	protected override bool _GetWindowMinimized () => false;

	protected override void _SetWindowMinimized (bool minimized) { }

	protected override void _SetWindowTitle (string title) { }

	protected override void _SetWindowMinSize (int size) { }

	protected override void _SetEventWaiting (bool enable) { }

	// Render
	protected override void _OnLayerUpdate (int layerIndex, bool isUiLayer, Cell[] cells, int cellCount) {
		// TODO
	}

	protected override bool _GetEffectEnable (int effectIndex) => CallingMessage.EffectEnable.GetBit(effectIndex);

	protected override void _SetEffectEnable (int effectIndex, bool enable) => RespondMessage.EffectEnable.SetBit(effectIndex, enable);

	protected override void _Effect_SetDarkenParams (float amount, float step) {
		RespondMessage.HasEffectParams.SetBit(Const.SCREEN_EFFECT_RETRO_DARKEN, true);
		RespondMessage.e_DarkenAmount = amount;
		RespondMessage.e_DarkenStep = step;
	}

	protected override void _Effect_SetLightenParams (float amount, float step) {
		RespondMessage.HasEffectParams.SetBit(Const.SCREEN_EFFECT_RETRO_LIGHTEN, true);
		RespondMessage.e_LightenAmount = amount;
		RespondMessage.e_LightenStep = step;
	}

	protected override void _Effect_SetTintParams (Color32 color) {
		RespondMessage.HasEffectParams.SetBit(Const.SCREEN_EFFECT_TINT, true);
		RespondMessage.e_TintColor = color;
	}

	protected override void _Effect_SetVignetteParams (float radius, float feather, float offsetX, float offsetY, float round) {
		RespondMessage.HasEffectParams.SetBit(Const.SCREEN_EFFECT_VIGNETTE, true);
		RespondMessage.e_VigRadius = radius;
		RespondMessage.e_VigFeather = feather;
		RespondMessage.e_VigOffsetX = offsetX;
		RespondMessage.e_VigOffsetY = offsetY;
		RespondMessage.e_VigRound = round;
	}


	// Texture
	protected override object _GetTextureFromPixels (Color32[] pixels, int width, int height) {
		// TODO
		throw new System.NotImplementedException();
	}

	protected override Color32[] _GetPixelsFromTexture (object texture) {
		// TODO
		throw new System.NotImplementedException();
	}

	protected override void _FillPixelsIntoTexture (Color32[] pixels, object texture) {
		// TODO
	}

	protected override Int2 _GetTextureSize (object texture) {
		// TODO
		throw new System.NotImplementedException();
	}

	protected override object _PngBytesToTexture (byte[] bytes) {
		// TODO
		throw new System.NotImplementedException();
	}

	protected override byte[] _TextureToPngBytes (object texture) {
		// TODO
		throw new System.NotImplementedException();
	}

	protected override void _UnloadTexture (object texture) {
		// TODO
	}


	// GL Gizmos
	protected override void _DrawGizmosRect (IRect rect, Color32 color) {
		// TODO
	}

	protected override void _DrawGizmosTexture (IRect rect, FRect uv, object texture, bool inverse) {
		// TODO
	}


	// Text
	protected override int _GetFontCount () => CallingMessage.FontCount;

	protected override string _GetClipboardText () => RayUtil.GetClipboardText();

	protected override void _SetClipboardText (string text) => RayUtil.SetClipboardText(text);

	protected override bool _GetCharSprite (int fontIndex, char c, out CharSprite result) {

		// TODO

		// 

		throw new System.NotImplementedException();

	}


	// Music
	protected override void _PlayMusic (int id) => RespondMessage.RequirePlayMusicID = id;

	protected override void _StopMusic () => RespondMessage.AudioActionRequirement.SetBit(0, true);

	protected override void _PauseMusic () => RespondMessage.AudioActionRequirement.SetBit(1, true);

	protected override void _UnPauseMusic () => RespondMessage.AudioActionRequirement.SetBit(2, true);

	protected override void _SetMusicVolume (int volume) => RespondMessage.RequireSetMusicVolume = volume;

	protected override bool _IsMusicPlaying () => CallingMessage.IsMusicPlaying;


	// Sounds
	protected override void _PlaySound (int id, float volume) {
		RespondMessage.RequirePlaySoundID = id;
		RespondMessage.RequirePlaySoundVolume = volume;
	}
	protected override void _StopAllSounds () => RespondMessage.AudioActionRequirement.SetBit(3, true);
	protected override void _SetSoundVolume (int volume) => RespondMessage.RequireSetSoundVolume = volume;


	// Cursor
	protected override void _ShowCursor () { }
	protected override void _HideCursor () { }
	protected override void _CenterCursor () { }
	protected override bool _CursorVisible () => true;
	protected override void _SetCursor (int index) => RespondMessage.RequireSetCursorIndex = index;
	protected override void _SetCursorToNormal () => RespondMessage.RequireSetCursorIndex = -3;
	protected override bool _CursorInScreen () => CallingMessage.CursorInScreen;


	// Mouse
	protected override bool _IsMouseAvailable () => CallingMessage.DeviceData.GetBit(0);
	protected override bool _IsMouseLeftHolding () => CallingMessage.DeviceData.GetBit(1);
	protected override bool _IsMouseRightHolding () => CallingMessage.DeviceData.GetBit(2);
	protected override bool _IsMouseMidHolding () => CallingMessage.DeviceData.GetBit(3);
	protected override int _GetMouseScrollDelta () => CallingMessage.MouseScrollDelta;
	protected override Int2 _GetMouseScreenPosition () => new(CallingMessage.MousePosX, CallingMessage.MousePosY);


	// Keyboard
	protected override bool _IsKeyboardAvailable () => CallingMessage.DeviceData.GetBit(5);
	protected override bool _IsKeyboardKeyHolding (KeyboardKey key) {
		int index = (int)key;
		return CallingMessage.KeyboardHolding[index / 8].GetBit(index % 8);
	}


	// Gamepad
	protected override bool _IsGamepadAvailable () => CallingMessage.DeviceData.GetBit(6);
	protected override bool _IsGamepadKeyHolding (GamepadKey key) {
		int index = (int)key;
		return CallingMessage.GamepadHolding[index / 8].GetBit(index % 8);
	}
	protected override bool _IsGamepadLeftStickHolding (Direction4 direction) => CallingMessage.GamepadStickHolding.GetBit(direction switch {
		Direction4.Left => 0,
		Direction4.Right => 1,
		Direction4.Down => 2,
		Direction4.Up => 3,
		_ => 0,
	});
	protected override bool _IsGamepadRightStickHolding (Direction4 direction) => CallingMessage.GamepadStickHolding.GetBit(direction switch {
		Direction4.Left => 4,
		Direction4.Right => 5,
		Direction4.Down => 6,
		Direction4.Up => 7,
		_ => 4,
	});
	protected override Float2 _GetGamepadLeftStickDirection () => new(CallingMessage.GamepadLeftStickDirectionX, CallingMessage.GamepadLeftStickDirectionY);
	protected override Float2 _GetGamepadRightStickDirection () => new(CallingMessage.GamepadRightStickDirectionX, CallingMessage.GamepadRightStickDirectionY);



	#endregion




	#region --- LGC ---



	#endregion




}