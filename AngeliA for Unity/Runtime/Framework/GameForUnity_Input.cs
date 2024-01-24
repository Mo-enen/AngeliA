using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using AngeliaFramework;
using UnityEngine.InputSystem.LowLevel;

namespace AngeliaForUnity {
	public sealed partial class GameForUnity {


		private static void Initialize_Input () {

			InputSystem.onDeviceChange -= OnDeviceChange;
			InputSystem.onDeviceChange += OnDeviceChange;

			if (!IsEdittime) HideCursor();

		}

		private static void OnDeviceChange (InputDevice device, InputDeviceChange change) {
			if (device is Keyboard && Keyboard.current != null) {
				Keyboard.current.onTextInput -= CellGUI.OnTextInput;
				Keyboard.current.onTextInput += CellGUI.OnTextInput;
			}
		}


		// Cursor
		protected override void _ShowCursor () {
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}
		protected override void _HideCursor () {
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}
		protected override bool _CursorVisible () => Cursor.visible;
		protected override void _SetCursor (int index) {
			var texture = Cursors[index];
			if (texture == null) return;
			var pivot = Vector2.zero;
			if (index < CursorPivots.Length) {
				pivot = CursorPivots[index];
			}
			Cursor.SetCursor(texture, pivot, CursorMode.Auto);
		}
		protected override void _SetCursorToNormal () => Cursor.SetCursor(null, default, CursorMode.Auto);

		// Input
		protected override bool _IsMouseAvailable () => Mouse.current != null && Mouse.current.enabled;
		protected override bool _IsGamepadAvailable () => Gamepad.current != null && Gamepad.current.enabled;
		protected override bool _IsKeyboardAvailable () => Keyboard.current != null && Keyboard.current.enabled;
		protected override bool _IsMouseLeftHolding () => Mouse.current.leftButton.isPressed;
		protected override bool _IsMouseMidHolding () => Mouse.current.middleButton.isPressed;
		protected override bool _IsMouseRightHolding () => Mouse.current.rightButton.isPressed;
		protected override int _GetMouseScrollDelta () {
			float scroll = Mouse.current.scroll.ReadValue().y;
			return scroll.Abs() < 0.1f ? 0 : scroll > 0f ? 1 : -1;
		}
		protected override Int2 _GetMouseScreenPosition () => Mouse.current.position.ReadValue().ToAngelia().RoundToInt();
		protected override bool _IsKeyboardKeyHolding (KeyboardKey key) => key != KeyboardKey.None && key != KeyboardKey.IMESelected && Keyboard.current[(Key)key].isPressed;
		protected override bool _IsGamepadKeyHolding (GamepadKey key) => Gamepad.current[(GamepadButton)key].isPressed;
		protected override bool _IsGamepadLeftStickHolding (Direction4 direction) => (direction switch {
			Direction4.Up => Gamepad.current.leftStick.up,
			Direction4.Down => Gamepad.current.leftStick.down,
			Direction4.Left => Gamepad.current.leftStick.left,
			Direction4.Right => Gamepad.current.leftStick.right,
			_ => Gamepad.current.leftStick.up,
		}).isPressed;
		protected override bool _IsGamepadRightStickHolding (Direction4 direction) => (direction switch {
			Direction4.Up => Gamepad.current.rightStick.up,
			Direction4.Down => Gamepad.current.rightStick.down,
			Direction4.Left => Gamepad.current.rightStick.left,
			Direction4.Right => Gamepad.current.rightStick.right,
			_ => Gamepad.current.rightStick.up,
		}).isPressed;
		protected override Float2 _GetGamepadLeftStickDirection () => Gamepad.current.leftStick.ReadValue().ToAngelia();
		protected override Float2 _GetGamepadRightStickDirection () => Gamepad.current.rightStick.ReadValue().ToAngelia();


	}
}