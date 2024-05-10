using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AngeliA;

public class RiggedCallingMessage {


	// SUB
	public struct CharRequirementData {
		public bool Valid;
		public char Char;
		public int FontIndex;
		public FRect Offset;
		public float Advance;
	}


	// Const
	public const int REQUIRE_CHAR_MAX_COUNT = 128;
	public const int REQUIRE_GIZMOS_TEXTURE_MAX_COUNT = 128;

	// Init
	public int FontCount;
	private readonly int KeyboardKeyCount;
	private readonly int GamepadKeyCount;

	// Data
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
	public readonly byte[] KeyboardHolding;
	public readonly byte[] GamepadHolding;
	public byte GamepadStickHolding;
	public float GamepadLeftStickDirectionX;
	public float GamepadLeftStickDirectionY;
	public float GamepadRightStickDirectionX;
	public float GamepadRightStickDirectionY;
	public int CharRequiredCount;
	public CharRequirementData[] RequiredChars = new CharRequirementData[REQUIRE_CHAR_MAX_COUNT];
	public int RequiringGizmosTextureIDCount = 0;
	public uint[] RequiringGizmosTextureIDs = new uint[REQUIRE_GIZMOS_TEXTURE_MAX_COUNT];


	// API
	public RiggedCallingMessage () {
		KeyboardKeyCount = typeof(KeyboardKey).EnumLength();
		KeyboardHolding = new byte[KeyboardKeyCount / 8 + 1];
		GamepadKeyCount = typeof(GamepadKey).EnumLength();
		GamepadHolding = new byte[GamepadKeyCount / 8 + 1];
	}


	public void LoadDataFromFramework () {

		int mouseScroll = Game.MouseScrollDelta;
		var mousePos = Game.MouseScreenPosition;
		var stickL = Game.GamepadLeftStickDirection;
		var stickR = Game.GamepadRightStickDirection;

		CursorInScreen = Game.CursorInScreen;
		MonitorWidth = Game.MonitorWidth;
		MonitorHeight = Game.MonitorHeight;
		ScreenWidth = Game.ScreenWidth;
		ScreenHeight = Game.ScreenHeight;
		FontCount = Game.FontCount;
		for (int i = 0; i < Const.SCREEN_EFFECT_COUNT; i++) {
			EffectEnable.SetBit(i, Game.GetEffectEnable(i));
		}
		IsMusicPlaying = Game.IsMusicPlaying;
		DeviceData.SetBit(0, Game.IsMouseAvailable);
		DeviceData.SetBit(1, Game.IsMouseLeftHolding);
		DeviceData.SetBit(2, Game.IsMouseRightHolding);
		DeviceData.SetBit(3, Game.IsMouseMidHolding);
		DeviceData.SetBit(4, mouseScroll != 0);
		DeviceData.SetBit(5, Game.IsKeyboardAvailable);
		DeviceData.SetBit(6, Game.IsGamepadAvailable);
		MouseScrollDelta = mouseScroll;
		MousePosX = mousePos.x;
		MousePosY = mousePos.y;
		for (int i = 0; i < KeyboardKeyCount; i++) {
			KeyboardHolding[i / 8].SetBit(i % 8, Game.IsKeyboardKeyHolding((KeyboardKey)i));
		}
		for (int i = 0; i < GamepadKeyCount; i++) {
			GamepadHolding[i / 8].SetBit(i % 8, Game.IsGamepadKeyHolding((GamepadKey)i));
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

	}


	public void ReadDataFromPipe (BinaryReader reader) {

		// Updating Data
		CursorInScreen = reader.ReadBoolean();
		MonitorWidth = reader.ReadInt32();
		MonitorHeight = reader.ReadInt32();
		ScreenWidth = reader.ReadInt32();
		ScreenHeight = reader.ReadInt32();
		FontCount = reader.ReadInt32();
		EffectEnable = reader.ReadByte();
		IsMusicPlaying = reader.ReadBoolean();
		DeviceData = reader.ReadByte();
		MouseScrollDelta = DeviceData.GetBit(4) ? reader.ReadInt32() : 0;
		MousePosX = reader.ReadInt32();
		MousePosY = reader.ReadInt32();
		for (int i = 0; i < KeyboardHolding.Length; i++) {
			KeyboardHolding[i] = reader.ReadByte();
		}
		for (int i = 0; i < GamepadHolding.Length; i++) {
			GamepadHolding[i] = reader.ReadByte();
		}
		GamepadStickHolding = reader.ReadByte();
		GamepadLeftStickDirectionX = reader.ReadSingle();
		GamepadLeftStickDirectionY = reader.ReadSingle();
		GamepadRightStickDirectionX = reader.ReadSingle();
		GamepadRightStickDirectionY = reader.ReadSingle();

		CharRequiredCount = reader.ReadInt32();
		for (int i = 0; i < CharRequiredCount; i++) {
			char c = reader.ReadChar();
			int fontIndex = reader.ReadInt32();
			bool valid = reader.ReadBoolean();
			if (valid) {
				float advance = reader.ReadSingle();
				var offset = new FRect(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
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

		RequiringGizmosTextureIDCount = reader.ReadInt32();
		for (int i = 0; i < RequiringGizmosTextureIDCount; i++) {
			RequiringGizmosTextureIDs[i] = reader.ReadUInt32();
		}


	}


	public void WriteDataToPipe (BinaryWriter writer) {

		writer.Write(CursorInScreen);
		writer.Write(MonitorWidth);
		writer.Write(MonitorHeight);
		writer.Write(ScreenWidth);
		writer.Write(ScreenHeight);
		writer.Write(FontCount);
		writer.Write(EffectEnable);
		writer.Write(IsMusicPlaying);
		writer.Write(DeviceData);
		if (DeviceData.GetBit(4)) writer.Write(MouseScrollDelta);
		writer.Write(MousePosX);
		writer.Write(MousePosY);
		for (int i = 0; i < KeyboardHolding.Length; i++) {
			writer.Write(KeyboardHolding[i]);
		}
		for (int i = 0; i < GamepadHolding.Length; i++) {
			writer.Write(GamepadHolding[i]);
		}
		writer.Write(GamepadStickHolding);
		writer.Write(GamepadLeftStickDirectionX);
		writer.Write(GamepadLeftStickDirectionY);
		writer.Write(GamepadRightStickDirectionX);
		writer.Write(GamepadRightStickDirectionY);

		writer.Write(CharRequiredCount);
		for (int i = 0; i < CharRequiredCount; i++) {
			var data = RequiredChars[i];
			writer.Write(data.Char);
			writer.Write(data.FontIndex);
			writer.Write(data.Valid);
			if (data.Valid) {
				writer.Write(data.Advance);
				writer.Write(data.Offset.x);
				writer.Write(data.Offset.y);
				writer.Write(data.Offset.width);
				writer.Write(data.Offset.height);
			}
		}

		writer.Write(RequiringGizmosTextureIDCount);
		for (int i = 0; i < RequiringGizmosTextureIDCount; i++) {
			writer.Write(RequiringGizmosTextureIDs[i]);
		}

	}


}