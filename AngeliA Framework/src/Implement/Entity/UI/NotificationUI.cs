using System.Collections;
using System.Collections.Generic;


namespace AngeliA.Framework;
[EntityAttribute.ForceSpawn]
[EntityAttribute.Capacity(6, 0)]
public class NotificationUI : EntityUI {

	// VAR
	private static readonly int TYPE_ID = typeof(NotificationUI).AngeHash();
	private static int CurrentNotificationCount = 0;
	private string Content = "";
	private int Icon = 0;
	private int NotificationIndex = 0;
	private int CurrentOffsetY = 0;

	// MSG
	public override void OnActivated () {
		base.OnActivated();
		CurrentOffsetY = 0;
	}

	public override void BeforePhysicsUpdate () {
		base.BeforePhysicsUpdate();
		CurrentNotificationCount = 0;
	}

	public override void PhysicsUpdate () {
		base.PhysicsUpdate();
		NotificationIndex = CurrentNotificationCount;
		CurrentNotificationCount++;
	}

	public override void UpdateUI () {

		base.UpdateUI();

		const int EASE_DURATION = 30;
		const int STAY_DURATION = 300;

		int localFrame = Game.GlobalFrame - SpawnFrame;
		if (localFrame > STAY_DURATION) {
			Active = false;
			return;
		}
		int panelWidth = Unify(500);
		int panelHeight = Unify(48);
		CurrentOffsetY = CurrentOffsetY.LerpTo((panelHeight + Unify(30)) * NotificationIndex, 60);
		var fromRect = new IRect(
			Renderer.CameraRect.CenterX() - panelWidth / 2,
			Renderer.CameraRect.yMax,
			panelWidth, panelHeight
		);
		var toRect = new IRect(
			Renderer.CameraRect.CenterX() - panelWidth / 2,
			Renderer.CameraRect.yMax - panelHeight - Unify(24) - CurrentOffsetY,
			panelWidth, panelHeight
		);
		var panelRect =
			localFrame < EASE_DURATION ? fromRect.LerpTo(toRect, Ease.OutBack((float)localFrame / EASE_DURATION)) :
			localFrame > STAY_DURATION - EASE_DURATION ? fromRect.LerpTo(toRect, Ease.OutBack((float)(STAY_DURATION - localFrame) / EASE_DURATION)) :
			toRect;

		// BG
		Renderer.Draw(Const.PIXEL, panelRect.Expand(Unify(12)), Color32.BLACK, int.MaxValue - 1);

		// Icon
		if (Renderer.TryGetSprite(Icon, out var icon)) {
			Renderer.Draw(
				Icon,
				new IRect(panelRect.x, panelRect.y, panelRect.height, panelRect.height).Fit(icon),
				int.MaxValue
			);
		}

		// Label
		GUI.Label(
			panelRect.Shrink(panelRect.height + Unify(12), 0, 0, 0),
			Content
		);

	}

	public static void SpawnNotification (string content, int icon) {
		if (Stage.SpawnEntity(TYPE_ID, 0, 0) is not NotificationUI ui) return;
		ui.Content = content;
		ui.Icon = icon;
	}

}