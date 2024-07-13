using System.Collections;
using System.Collections.Generic;


namespace AngeliA;

public abstract class ActionMapGenerator : MapGenerator, IActionTarget {




	#region --- VAR ---


	// Const
	private static readonly LanguageCode HINT_ENTER = ("CtrlHint.MapGenerator.Enter", "Enter Level");
	private static readonly LanguageCode HINT_GENERATE = ("CtrlHint.MapGenerator.Generate", "Generate Level");
	private static readonly LanguageCode HINT_GENERATING = ("CtrlHint.MapGenerator.Generating", "Generating");
	private static readonly LanguageCode HINT_NOTIFY = ("Notify.MapGeneratedNotify", "Map Generated");

	// Api
	protected virtual bool ShowGeneratingHint => true;


	#endregion




	#region --- MSG ---


	public override void OnActivated () {
		base.OnActivated();

	}


	public override void FirstUpdate () {
		base.FirstUpdate();
		Physics.FillEntity(PhysicsLayer.ENVIRONMENT, this, true);
	}


	public override void LateUpdate () {
		base.LateUpdate();
		var cell = DrawArtwork();
		if (!IsGenerating) {
			// Normal
			if ((this as IActionTarget).IsHighlighted) {
				IActionTarget.HighlightBlink(cell, 0.5f, 0.5f);
				// Hint
				ControlHintUI.DrawGlobalHint(
					X, Y + Const.CEL * 2 + Const.HALF, Gamekey.Action,
					HasMapInDisk ? HINT_ENTER : HINT_GENERATE,
					true
				);
			}
		} else {
			// Generating
			if (ShowGeneratingHint) {
				GUI.BackgroundLabel(
					new IRect(X - Const.CEL, Y + Const.CEL * 2, Const.CEL * 3, Const.CEL),
					HINT_GENERATING, Color32.BLACK, GUI.Unify(6)
				);
			}
		}
	}


	protected override void AfterMapGenerate () {
		base.AfterMapGenerate();
		NotificationUI.SpawnNotification(HINT_NOTIFY, TypeID);
	}


	protected virtual Cell DrawArtwork () => Renderer.Draw(TypeID, Rect);


	bool IActionTarget.Invoke () {
		if (!HasMapInDisk) {
			GenerateAsync();
		} else {
			Enter();
		}
		return true;
	}


	bool IActionTarget.AllowInvoke () => !IsGenerating;


	private void Enter () {
		if (Task.HasTask()) return;
		TeleportTask.Teleport(
			X + Width / 2,
			Y + Height / 2,
			0, 0, 0,
			waitDuration: 30,
			duration: 60,
			useVignette: true,
			newChannel: MapChannel.Procedure,
			channelName: GetType().AngeName()
		);
	}


	#endregion




}