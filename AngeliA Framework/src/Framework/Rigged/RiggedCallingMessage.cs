using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace AngeliA;

public class RiggedCallingMessage {




	#region --- SUB ---


	public struct CharRequirementData {
		public bool Valid;
		public char Char;
		public int FontIndex;
		public FRect Offset;
		public float Advance;
	}


	#endregion




	#region --- VAR ---


	// Const
	public const int REQUIRE_CHAR_MAX_COUNT = 128;
	public const int REQUIRE_GIZMOS_TEXTURE_MAX_COUNT = 128;
	private static readonly int KeyboardKeyCount = typeof(KeyboardKey).EnumLength();
	private static readonly int GamepadKeyCount = typeof(GamepadKey).EnumLength();

	// Pipe
	public bool CursorInScreen;
	public int MonitorWidth;
	public int MonitorHeight;
	public int ScreenWidth;
	public int ScreenHeight;
	public byte EffectEnable;
	public bool IsMusicPlaying;
	public byte DeviceData;
	public int MouseScrollDelta;
	public int MousePosX;
	public int MousePosY;
	public byte HoldingKeyboardKeyCount;
	public byte HoldingGamepadKeyCount;
	public readonly int[] HoldingKeyboardKeys = new int[16];
	public readonly int[] HoldingGamepadKeys = new int[16];
	public byte GamepadStickHolding;
	public float GamepadLeftStickDirectionX;
	public float GamepadLeftStickDirectionY;
	public float GamepadRightStickDirectionX;
	public float GamepadRightStickDirectionY;
	public int CharRequiredCount;
	public CharRequirementData[] RequiredChars = new CharRequirementData[REQUIRE_CHAR_MAX_COUNT];
	public int RequiringGizmosTextureIDCount = 0;
	public uint[] RequiringGizmosTextureIDs = new uint[REQUIRE_GIZMOS_TEXTURE_MAX_COUNT];
	public byte RequireGameMessageInvoke;
	public byte PressedCharCount;
	public readonly char[] PressedChars = new char[256];
	public byte PressedKeyCount;
	public readonly int[] PressedGuiKeys = new int[256];


	#endregion




	#region --- API ---


	public void LoadDataFromEngine (bool ignoreInput, int leftPadding) {

		int mouseScroll = Game.MouseScrollDelta;
		var mousePos = Game.MouseScreenPosition;
		var stickL = Game.GamepadLeftStickDirection;
		var stickR = Game.GamepadRightStickDirection;
		int screenLeftPadding = leftPadding * Game.ScreenWidth / Renderer.CameraRect.width;
		mousePos.x -= screenLeftPadding;

		CursorInScreen = Game.CursorInScreen;
		MonitorWidth = Game.MonitorWidth;
		MonitorHeight = Game.MonitorHeight;
		ScreenWidth = Game.ScreenWidth - screenLeftPadding;
		ScreenHeight = Game.ScreenHeight;

		for (int i = 0; i < Const.SCREEN_EFFECT_COUNT; i++) {
			EffectEnable.SetBit(i, Game.GetEffectEnable(i));
		}
		IsMusicPlaying = Game.IsMusicPlaying;
		DeviceData.SetBit(0, Game.IsMouseAvailable);
		DeviceData.SetBit(1, !ignoreInput && Game.IsMouseLeftHolding);
		DeviceData.SetBit(2, !ignoreInput && Game.IsMouseRightHolding);
		DeviceData.SetBit(3, !ignoreInput && Game.IsMouseMidHolding);
		DeviceData.SetBit(4, !ignoreInput && mouseScroll != 0);
		DeviceData.SetBit(5, Game.IsKeyboardAvailable);
		DeviceData.SetBit(6, Game.IsGamepadAvailable);
		MouseScrollDelta = ignoreInput ? 0 : mouseScroll;
		MousePosX = mousePos.x;
		MousePosY = mousePos.y;
		HoldingKeyboardKeyCount = 0;
		HoldingGamepadKeyCount = 0;
		if (!ignoreInput) {
			for (int i = 0; i < KeyboardKeyCount; i++) {
				if (Game.IsKeyboardKeyHolding((KeyboardKey)i)) {
					HoldingKeyboardKeys[HoldingKeyboardKeyCount] = i;
					HoldingKeyboardKeyCount++;
					if (HoldingKeyboardKeyCount >= HoldingKeyboardKeys.Length) break;
				}
			}
			for (int i = 0; i < GamepadKeyCount; i++) {
				if (Game.IsGamepadKeyHolding((GamepadKey)i)) {
					HoldingGamepadKeys[HoldingGamepadKeyCount] = i;
					HoldingGamepadKeyCount++;
					if (HoldingGamepadKeyCount >= HoldingGamepadKeys.Length) break;
				}
			}
			GamepadStickHolding.SetBit(0, Game.IsGamepadLeftStickHolding(Direction4.Left));
			GamepadStickHolding.SetBit(1, Game.IsGamepadLeftStickHolding(Direction4.Right));
			GamepadStickHolding.SetBit(2, Game.IsGamepadLeftStickHolding(Direction4.Down));
			GamepadStickHolding.SetBit(3, Game.IsGamepadLeftStickHolding(Direction4.Up));
			GamepadStickHolding.SetBit(4, Game.IsGamepadRightStickHolding(Direction4.Left));
			GamepadStickHolding.SetBit(5, Game.IsGamepadRightStickHolding(Direction4.Right));
			GamepadStickHolding.SetBit(6, Game.IsGamepadRightStickHolding(Direction4.Down));
			GamepadStickHolding.SetBit(7, Game.IsGamepadRightStickHolding(Direction4.Up));
			GamepadLeftStickDirectionX = stickL.x;
			GamepadLeftStickDirectionY = stickL.y;
			GamepadRightStickDirectionX = stickR.x;
			GamepadRightStickDirectionY = stickR.y;
		} else {
			GamepadStickHolding = 0;
			GamepadLeftStickDirectionX = 0f;
			GamepadLeftStickDirectionY = 0f;
			GamepadRightStickDirectionX = 0f;
			GamepadRightStickDirectionY = 0f;
		}

		PressedCharCount = 0;
		foreach (char c in Game.ForAllPressingCharsThisFrame()) {
			PressedChars[PressedCharCount] = c;
			PressedCharCount++;
			if (PressedCharCount >= PressedChars.Length) break;
		}

		PressedKeyCount = 0;
		foreach (var key in Game.ForAllPressingKeysThisFrame()) {
			PressedGuiKeys[PressedKeyCount] = (int)key;
			PressedKeyCount++;
			if (PressedKeyCount >= PressedGuiKeys.Length) break;
		}

	}


	public unsafe void ReadDataFromPipe (byte* pointer) {

		try {

			byte* end = pointer + Const.RIG_BUFFER_SIZE - 2;

			CursorInScreen = Util.ReadBool(ref pointer, end);

			MonitorWidth = Util.ReadInt(ref pointer, end);
			MonitorHeight = Util.ReadInt(ref pointer, end);
			ScreenWidth = Util.ReadInt(ref pointer, end);
			ScreenHeight = Util.ReadInt(ref pointer, end);

			EffectEnable = Util.ReadByte(ref pointer, end);
			IsMusicPlaying = Util.ReadBool(ref pointer, end);
			DeviceData = Util.ReadByte(ref pointer, end);

			MouseScrollDelta = DeviceData.GetBit(4) ? Util.ReadInt(ref pointer, end) : 0;

			MousePosX = Util.ReadInt(ref pointer, end);
			MousePosY = Util.ReadInt(ref pointer, end);

			HoldingKeyboardKeyCount = Util.ReadByte(ref pointer, end);
			for (int i = 0; i < HoldingKeyboardKeyCount && i < HoldingKeyboardKeys.Length; i++) {
				HoldingKeyboardKeys[i] = Util.ReadInt(ref pointer, end);
			}
			HoldingGamepadKeyCount = Util.ReadByte(ref pointer, end);
			for (int i = 0; i < HoldingGamepadKeyCount && i < HoldingGamepadKeys.Length; i++) {
				HoldingGamepadKeys[i] = Util.ReadInt(ref pointer, end);
			}
			GamepadStickHolding = Util.ReadByte(ref pointer, end);
			GamepadLeftStickDirectionX = Util.ReadFloat(ref pointer, end);
			GamepadLeftStickDirectionY = Util.ReadFloat(ref pointer, end);
			GamepadRightStickDirectionX = Util.ReadFloat(ref pointer, end);
			GamepadRightStickDirectionY = Util.ReadFloat(ref pointer, end);

			CharRequiredCount = Util.ReadInt(ref pointer, end);
			for (int i = 0; i < CharRequiredCount && i < RequiredChars.Length; i++) {
				char c = Util.ReadChar(ref pointer, end);
				int fontIndex = Util.ReadInt(ref pointer, end);
				bool valid = Util.ReadBool(ref pointer, end);
				if (valid) {
					float advance = Util.ReadFloat(ref pointer, end);
					var offset = new FRect(
						Util.ReadFloat(ref pointer, end),
						Util.ReadFloat(ref pointer, end),
						Util.ReadFloat(ref pointer, end),
						Util.ReadFloat(ref pointer, end)
					);
					RequiredChars[i] = new CharRequirementData() {
						Char = c,
						Advance = advance,
						FontIndex = fontIndex,
						Offset = offset,
						Valid = valid,
					};
				} else {
					RequiredChars[i] = new CharRequirementData() {
						Char = c,
						FontIndex = fontIndex,
						Valid = valid,
						Advance = default,
						Offset = default,
					};
				}
			}

			RequiringGizmosTextureIDCount = Util.ReadInt(ref pointer, end);
			for (int i = 0; i < RequiringGizmosTextureIDCount && i < REQUIRE_GIZMOS_TEXTURE_MAX_COUNT; i++) {
				RequiringGizmosTextureIDs[i] = Util.ReadUInt(ref pointer, end);
			}

			RequireGameMessageInvoke = Util.ReadByte(ref pointer, end);

			PressedCharCount = Util.ReadByte(ref pointer, end);
			for (int i = 0; i < PressedCharCount && i < PressedChars.Length; i++) {
				PressedChars[i] = Util.ReadChar(ref pointer, end);
			}

			PressedKeyCount = Util.ReadByte(ref pointer, end);
			for (int i = 0; i < PressedKeyCount && i < PressedGuiKeys.Length; i++) {
				PressedGuiKeys[i] = Util.ReadInt(ref pointer, end);
			}

		} catch (System.Exception ex) { Debug.LogException(ex); }

	}


	public unsafe void WriteDataToPipe (byte* pointer) {

		try {

			byte* end = pointer + Const.RIG_BUFFER_SIZE - 2;

			Util.Write(ref pointer, CursorInScreen, end);
			Util.Write(ref pointer, MonitorWidth, end);
			Util.Write(ref pointer, MonitorHeight, end);
			Util.Write(ref pointer, ScreenWidth, end);
			Util.Write(ref pointer, ScreenHeight, end);

			Util.Write(ref pointer, EffectEnable, end);
			Util.Write(ref pointer, IsMusicPlaying, end);
			Util.Write(ref pointer, DeviceData, end);
			if (DeviceData.GetBit(4)) Util.Write(ref pointer, MouseScrollDelta, end);
			Util.Write(ref pointer, MousePosX, end);
			Util.Write(ref pointer, MousePosY, end);
			Util.Write(ref pointer, HoldingKeyboardKeyCount, end);
			for (int i = 0; i < HoldingKeyboardKeyCount; i++) {
				Util.Write(ref pointer, HoldingKeyboardKeys[i], end);
			}
			Util.Write(ref pointer, HoldingGamepadKeyCount, end);
			for (int i = 0; i < HoldingGamepadKeyCount; i++) {
				Util.Write(ref pointer, HoldingGamepadKeys[i], end);
			}
			Util.Write(ref pointer, GamepadStickHolding, end);
			Util.Write(ref pointer, GamepadLeftStickDirectionX, end);
			Util.Write(ref pointer, GamepadLeftStickDirectionY, end);
			Util.Write(ref pointer, GamepadRightStickDirectionX, end);
			Util.Write(ref pointer, GamepadRightStickDirectionY, end);

			Util.Write(ref pointer, CharRequiredCount, end);
			for (int i = 0; i < CharRequiredCount; i++) {
				var data = RequiredChars[i];
				Util.Write(ref pointer, data.Char, end);
				Util.Write(ref pointer, data.FontIndex, end);
				Util.Write(ref pointer, data.Valid, end);
				if (data.Valid) {
					Util.Write(ref pointer, data.Advance, end);
					Util.Write(ref pointer, data.Offset.x, end);
					Util.Write(ref pointer, data.Offset.y, end);
					Util.Write(ref pointer, data.Offset.width, end);
					Util.Write(ref pointer, data.Offset.height, end);
				}
			}

			Util.Write(ref pointer, RequiringGizmosTextureIDCount, end);
			for (int i = 0; i < RequiringGizmosTextureIDCount; i++) {
				Util.Write(ref pointer, RequiringGizmosTextureIDs[i], end);
			}

			Util.Write(ref pointer, RequireGameMessageInvoke, end);
			RequireGameMessageInvoke = 0;

			Util.Write(ref pointer, PressedCharCount, end);
			for (int i = 0; i < PressedCharCount; i++) {
				Util.Write(ref pointer, PressedChars[i], end);
			}

			Util.Write(ref pointer, PressedKeyCount, end);
			for (int i = 0; i < PressedKeyCount; i++) {
				Util.Write(ref pointer, PressedGuiKeys[i], end);
			}

		} catch (System.Exception ex) { Debug.LogException(ex); }

	}


	#endregion




}