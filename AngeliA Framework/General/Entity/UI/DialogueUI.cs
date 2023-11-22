using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	public sealed class DefaultDialogueUI : DialogueUI {

		protected override RectInt PanelRect => new(
			CellRenderer.CameraRect.x,
			CellRenderer.CameraRect.y,
			CellRenderer.CameraRect.width,
			Unify(300)
		);
		protected override RectInt ContentRect => PanelRect.Shrink(IconRect.width + Unify(28), Unify(12), Unify(12), Unify(36 + NameFontSize));
		protected override RectInt IconRect {
			get {
				var rect = PanelRect.Shrink(Unify(12));
				rect.width = rect.height;
				return rect;
			}
		}
		protected override RectInt NameRect {
			get {
				var panelRect = PanelRect.Shrink(Unify(12));
				return new RectInt(panelRect.x + panelRect.height + Unify(12), panelRect.yMax - Unify(30), panelRect.width, Unify(NameFontSize));
			}
		}
		protected override Color32 NameTint => Const.GREY_196;

	}


	[EntityAttribute.Capacity(1, 1)]
	public abstract class DialogueUI : EntityUI {




		#region --- VAR ---


		// Api
		protected abstract RectInt PanelRect { get; }
		protected abstract RectInt ContentRect { get; }
		protected abstract RectInt IconRect { get; }
		protected abstract RectInt NameRect { get; }
		protected virtual int NameFontSize => 28;
		protected virtual int ContentFontSize => 32;
		protected virtual int RollingSpeed => 16; // Character per Frame
		protected virtual Color32 NameTint => Const.WHITE;
		protected virtual Color32 ContentTint => Const.WHITE;

		// Data
		private int UpdatedFrame = int.MinValue;
		private int RolledFrame = int.MinValue;
		private int StartIndex = 0;
		private int EndIndex = 0;
		private int Identity = 0;
		private string Content = "";
		private Color32[] Colors = null;
		private readonly CellContent LabelName = new() { Alignment = Alignment.MidLeft, };
		private readonly CellContent LabelContent = new() { Wrap = true, Clip = true, Alignment = Alignment.TopLeft, };


		#endregion




		#region --- MSG ---


		public override void OnActivated () {
			base.OnActivated();
			StartIndex = 0;
			EndIndex = 0;
			Content = "";
			Identity = 0;
			Colors = null;
			UpdatedFrame = Game.GlobalFrame;
			RolledFrame = Game.GlobalFrame;
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
			LabelContent.CharSize = ContentFontSize;
			LabelContent.Tint = ContentTint;
			int cellStartIndex = CellRenderer.GetTextUsedCellCount();
			CellRendererGUI.Label(LabelContent, contentRect, StartIndex, true, out _, out EndIndex);
			if (CellRenderer.GetTextCells(out var cells, out int count)) {
				int charIndex = StartIndex;
				int visibleIndex = StartIndex + (Game.GlobalFrame - RolledFrame) * RollingSpeed;
				for (int i = cellStartIndex; i < count; i++) {
					var cell = cells[i];
					// Config Tint
					if (charIndex < Colors.Length) {
						cell.Color = Colors[charIndex];
					}
					// Animation Tint
					if (charIndex > visibleIndex) {
						cell.Color = Const.CLEAR;
					}
					charIndex++;
				}
			}

			// Name
			LabelName.Text = Language.Get(Identity);
			LabelName.CharSize = NameFontSize;
			LabelName.Tint = NameTint;
			CellRendererGUI.Label(LabelName, nameRect);

			// Icon
			if (CellRenderer.TryGetSprite(Identity, out var iconSprite)) {
				CellRenderer.Draw(iconSprite.GlobalID, iconRect.Fit(iconSprite.GlobalWidth, iconSprite.GlobalHeight), 1);
			}

		}


		#endregion




		#region --- API ---


		public void Update () => UpdatedFrame = Game.GlobalFrame;


		public void SetData (string content, int identity, Color32[] colors) {
			Content = content;
			Identity = identity;
			Colors = colors;
		}


		public bool Roll () {
			RolledFrame = Game.GlobalFrame;
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