using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using AngeliaFramework;


namespace AngeliaForUnity {
	public sealed partial class GameForUnity {


		private static void Initialize_Input () {

			InputSystem.onDeviceChange -= OnDeviceChange;
			InputSystem.onDeviceChange += OnDeviceChange;

			if (!IsEdittime) HideCursor();

		}

		private static void OnDeviceChange (InputDevice device, InputDeviceChange change) {
			if (device is Keyboard && Keyboard.current != null) {
				Keyboard.current.onTextInput -= CellRendererGUI.OnTextInput;
				Keyboard.current.onTextInput += CellRendererGUI.OnTextInput;
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
			var pivot = Float2.zero;
			if (index < CursorPivots.Length) {
				pivot = CursorPivots[index];
			}
			Cursor.SetCursor(texture, pivot, CursorMode.Auto);
		}
		protected override void _SetCursorToNormal () => Cursor.SetCursor(null, default, CursorMode.Auto);


	}
}