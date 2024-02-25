using System.Collections;
using System.Collections.Generic;


namespace AngeliA.Framework; 
public sealed class DefaultDialogueUI : DialogueUI {

	protected override IRect PanelRect => new(
		Renderer.CameraRect.x,
		Renderer.CameraRect.y,
		Renderer.CameraRect.width,
		Unify(300)
	);
	protected override IRect ContentRect => PanelRect.Shrink(IconRect.width + Unify(28), Unify(12), Unify(12), Unify(36 + NameFontSize));
	protected override IRect IconRect {
		get {
			var rect = PanelRect.Shrink(Unify(12));
			rect.width = rect.height;
			return rect;
		}
	}
	protected override IRect NameRect {
		get {
			var panelRect = PanelRect.Shrink(Unify(12));
			return new IRect(panelRect.x + panelRect.height + Unify(12), panelRect.yMax - Unify(30), panelRect.width, Unify(NameFontSize));
		}
	}
	protected override Color32 NameTint => Color32.GREY_196;

}


[EntityAttribute.Capacity(1, 1)]
public abstract class DialogueUI : EntityUI, IWindowEntityUI {




	#region --- VAR ---


	// Api
	protected abstract IRect PanelRect { get; }
	protected abstract IRect ContentRect { get; }
	protected abstract IRect IconRect { get; }
	protected abstract IRect NameRect { get; }
	protected virtual int NameFontSize => 28;
	protected virtual int ContentFontSize => 32;
	protected virtual int RollingSpeed => 16; // Character per Frame
	protected virtual Color32 NameTint => Color32.WHITE;
	protected virtual Color32 ContentTint => Color32.WHITE;
	public IRect BackgroundRect { get; private set; }

	// Data
	private int UpdatedFrame = int.MinValue;
	private int RolledFrame = int.MinValue;
	private int StartIndex = 0;
	private int EndIndex = 0;
	private int Identity = 0;
	private string Content = "";
	private Color32[] Colors = null;
	private readonly TextContent LabelName = new() { Alignment = Alignment.MidLeft, };
	private readonly TextContent LabelContent = new() { Wrap = true, Clip = true, Alignment = Alignment.TopLeft, };


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
		BackgroundRect = panelRect;
		Renderer.Draw(Const.PIXEL, panelRect, Color32.BLACK, 0);

		// Content
		LabelContent.Text = Content;
		LabelContent.CharSize = ContentFontSize;
		LabelContent.Tint = ContentTint;
		int cellStartIndex = Renderer.GetTextUsedCellCount();
		GUI.Label(LabelContent, contentRect, StartIndex, true, out _, out EndIndex);
		if (Renderer.GetTextCells(out var cells, out int count)) {
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
					cell.Color = Color32.CLEAR;
				}
				charIndex++;
			}
		}

		// Name
		LabelName.Text = Language.Get(Identity);
		LabelName.CharSize = NameFontSize;
		LabelName.Tint = NameTint;
		GUI.Label(LabelName, nameRect);

		// Icon
		if (Renderer.TryGetSprite(Identity, out var iconSprite)) {
			Renderer.Draw(iconSprite, iconRect.Fit(iconSprite), 1);
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