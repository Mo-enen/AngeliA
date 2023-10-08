using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	[EntityAttribute.DontDestroyOnSquadTransition]
	[EntityAttribute.DontDestroyOutOfRange]
	public class PlayerQuickMenuUI : EntityUI {




		#region --- VAR ---


		// Const
		private static readonly int FRAME_CODE = "Frame16".AngeHash();
		private static readonly int ITEM_FRAME_CODE = "UI.ItemFrame".AngeHash();
		private const int WINDOW_PADDING = 12;
		private const int ITEM_SIZE = 64;
		private const int ANIMATION_DURATION = 12;

		// Api
		public static PlayerQuickMenuUI Instance { get; private set; } = null;
		public static bool ShowingUI => Instance != null && Instance.Active;
		public bool IsDirty { get; private set; } = false;

		// Data


		#endregion




		#region --- MSG ---


		public PlayerQuickMenuUI () => Instance = this;


		public override void OnActivated () {
			base.OnActivated();
			X = CellRenderer.CameraRect.CenterX();
			Y = CellRenderer.CameraRect.CenterY();
			IsDirty = false;
		}


		public override void OnInactivated () {
			base.OnInactivated();
			IsDirty = false;
		}


		public override void UpdateUI () {
			base.UpdateUI();

			if (!Active || Player.Selecting == null || FrameTask.HasTask()) {
				Active = false;
				return;
			}

			// Close Menu Check
			if (
				!FrameInput.GameKeyHolding(Gamekey.Start) &&
				!FrameInput.GameKeyHolding(Gamekey.Select)
			) {
				if (IsDirty) {
					FrameInput.UseGameKey(Gamekey.Start);
					FrameInput.UseGameKey(Gamekey.Select);
				}
				Active = false;
			}

			// Draw
			DrawMenu();

		}


		private void DrawMenu () {

			if (FrameInput.GameKeyDownGUI(Gamekey.Left)) {

				IsDirty = true;
			}
			if (FrameInput.GameKeyDownGUI(Gamekey.Right)) {

				IsDirty = true;
			}

			var cameraRect = CellRenderer.CameraRect;
			int localAnimationFrame = Game.GlobalFrame - SpawnFrame;
			var panelRect = new RectInt(
				cameraRect.x, cameraRect.y, 256, 256
			);
			panelRect.ClampPositionInside(cameraRect);



			CellRenderer.Draw(Const.PIXEL, panelRect);

		}


		#endregion




		#region --- API ---


		public static PlayerQuickMenuUI OpenMenu () {
			var ins = Instance;
			if (ins == null) return null;
			if (!ins.Active) {
				Stage.SpawnEntity(ins.TypeID, 0, 0);
			} else {
				ins.OnInactivated();
				ins.OnActivated();
			}
			return ins;
		}


		#endregion




		#region --- LGC ---



		#endregion




	}
}