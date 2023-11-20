using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	public sealed class DefaultDialogueUI : DialogueUI {

		protected override RectInt PanelRect => new(
			CellRenderer.CameraRect.x,
			CellRenderer.CameraRect.y,
			CellRenderer.CameraRect.width,
			Unify(200)
		);
		protected override RectInt ContentRect => PanelRect.Shrink(Unify(8 + 200 - 8 - 8), Unify(8), Unify(8), Unify(8));
		protected override RectInt IconRect {
			get {
				var rect = PanelRect.Shrink(Unify(8));
				rect.width = rect.height;
				rect.height -= Unify(24);
				rect.y += Unify(24);
				return rect;
			}
		}
		protected override RectInt NameRect {
			get {
				var rect = PanelRect.Shrink(Unify(8));
				rect.width = rect.height;
				rect.height = Unify(24);
				return rect;
			}
		}

	}


	[EntityAttribute.Capacity(1, 1)]
	public abstract class DialogueUI : EntityUI {




		#region --- VAR ---


		// Api
		protected abstract RectInt PanelRect { get; }
		protected abstract RectInt ContentRect { get; }
		protected abstract RectInt IconRect { get; }
		protected abstract RectInt NameRect { get; }

		// Data
		private int UpdatedFrame = int.MinValue;
		private int StartIndex = 0;
		private int EndIndex = 0;
		private string Content = "";
		private int Identity = 0;
		private Color32[] Colors = null;
		private int[] Sizes = null;
		private readonly CellContent LabelContent = new() {
			Wrap = true,
			Clip = true,
			Alignment = Alignment.TopLeft,
			CharSize = 28,
		};


		#endregion




		#region --- MSG ---


		public override void OnActivated () {
			base.OnActivated();
			StartIndex = 0;
			EndIndex = 0;
			Content = "";
			Identity = 0;
			Colors = null;
			Sizes = null;
			UpdatedFrame = Game.GlobalFrame;
		}


		public override void UpdateUI () {
			base.UpdateUI();
			if (Game.GlobalFrame > UpdatedFrame + 1) {
				Active = false;
				return;
			}
			// Hide Hint
			ControlHintUI.ForceHideGamepad();
			for (int i = 0; i < 8; i++) {
				ControlHintUI.AddHint((Gamekey)i, "", int.MaxValue);
			}
			// Render
			DrawDialogue();
		}


		private void DrawDialogue () {

			var panelRect = PanelRect;
			var contentRect = ContentRect;
			var iconRect = IconRect;
			var nameRect = NameRect;

			// BG
			CellRenderer.Draw(Const.PIXEL, panelRect, Const.BLACK, 0);

			// Content
			LabelContent.Text = Content;
			CellRendererGUI.Label(LabelContent, contentRect, StartIndex, true, out _, out EndIndex);

			// Icon
			if (CellRenderer.TryGetSprite(Identity, out var iconSprite)) {
				CellRenderer.Draw(iconSprite.GlobalID, iconRect.Fit(iconSprite.GlobalWidth, iconSprite.GlobalHeight), 1);
			}

			// Name
			CellRendererGUI.Label(CellContent.Get(Language.Get(Identity)), nameRect);

		}


		#endregion




		#region --- API ---


		public void Update () => UpdatedFrame = Game.GlobalFrame;


		public void SetData (string content, int identity, Color32[] colors, int[] sizes) {
			Content = content;
			Identity = identity;
			Colors = colors;
			Sizes = sizes;
		}


		public bool Roll () {
			if (EndIndex >= Content.Length - 1) {
				// End
				StartIndex = 0;
				Content = "";
				return true;
			} else {
				// Half Way
				StartIndex = EndIndex;
				return false;
			}
		}


		#endregion




		#region --- LGC ---



		#endregion




	}
}