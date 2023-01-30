using AngeliaFramework;
namespace Yaya {
	public abstract class eYayaMenuUI : MenuUI {
		public override void FrameUpdate () {

			var cameraRect = CellRenderer.CameraRect;
			X = cameraRect.x + cameraRect.width / 2 - 250 * UNIT;
			Width = 650 * UNIT;

			base.FrameUpdate();

			// Ctrl Hint
			if (SelectionAdjustable) {
				eControlHintUI.DrawHint(GameKey.Left, GameKey.Right, WORD.HINT_ADJUST);
			} else {
				eControlHintUI.DrawHint(GameKey.Action, WORD.HINT_USE);
			}
			eControlHintUI.DrawHint(GameKey.Down, GameKey.Up, WORD.HINT_MOVE);

			Y = cameraRect.y + cameraRect.height / 2 - Height / 2;

		}
	}
	public class eYayaGenericMenuUI : GenericMenuUI {
		public override void FrameUpdate () {

			var cameraRect = CellRenderer.CameraRect;
			X = cameraRect.x + cameraRect.width / 2 - 250 * UNIT;
			Width = 650 * UNIT;

			base.FrameUpdate();

			// Ctrl Hint
			if (SelectionAdjustable) {
				eControlHintUI.DrawHint(GameKey.Left, GameKey.Right, WORD.HINT_ADJUST);
			} else {
				eControlHintUI.DrawHint(GameKey.Action, WORD.HINT_USE);
			}
			eControlHintUI.DrawHint(GameKey.Down, GameKey.Up, WORD.HINT_MOVE);

			Y = cameraRect.y + cameraRect.height / 2 - Height / 2;

		}
	}
}