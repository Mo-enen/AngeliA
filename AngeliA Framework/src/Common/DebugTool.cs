using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

/// <summary>
/// Provide extra tools for debugging. eg. Drag with mouse middle button to move player.
/// </summary>
public static class DebugTool {


	// Api
	/// <summary>
	/// True if allow user use middle mouse button to move player for debug. This feature is not include after the game publish.
	/// </summary>
	public static readonly FrameBasedBool DragPlayerInMiddleButtonToMove = new(true);
	/// <summary>
	/// True if use hotkeys to debug. eg. Ctrl+Alt+I to make player invincible
	/// </summary>
	public static readonly FrameBasedBool UseDebugHotkey = new(true);


#if DEBUG

	// SUB
	private class TestPoseAnimation : PoseAnimation {
		public static readonly int TYPE_ID = typeof(TestPoseAnimation).AngeHash();
		public override void Animate (PoseCharacterRenderer renderer) {
			base.Animate(renderer);

			// Main
			QTest.SetCurrentWindow(0, "Main");

			Movement.FacingRight = QTest.Bool("FacingRight", true);
			renderer.PoseRootX += QTest.Int("Root X", 0, -256, 256);
			renderer.PoseRootY += QTest.Int("Root Y", 0, -256, 256);

			Head.Rotation = QTest.Int("Head", 0, -90, 90);
			Body.Rotation = QTest.Int("Body", 0, -90, 90);
			renderer.HeadTwist = QTest.Int("Head Twist", 0, -1000, 1000);
			renderer.BodyTwist = QTest.Int("Body Twist", 0, -1000, 1000);

			QTest.Group("Grab");
			renderer.HandGrabRotationL.Override(QTest.Int("Grab Rot L", 0, -180, 180), 1, 9000);
			renderer.HandGrabRotationR.Override(QTest.Int("Grab Rot R", 0, -180, 180), 1, 9000);
			renderer.HandGrabScaleL.Override(QTest.Int("Grab Scl L", 1000, 0, 2000), 1, 9000);
			renderer.HandGrabScaleR.Override(QTest.Int("Grab Scl R", 1000, 0, 2000), 1, 9000);

			// Limb
			QTest.SetCurrentWindow(1, "Limb");

			QTest.Group("Arm");
			UpperArmL.LimbRotate(QTest.Int("ArmU L", 0, -180, 180));
			UpperArmR.LimbRotate(QTest.Int("ArmU R", 0, -180, 180));
			LowerArmL.LimbRotate(QTest.Int("ArmL L", 0, -180, 180));
			LowerArmR.LimbRotate(QTest.Int("ArmL R", 0, -180, 180));
			HandL.LimbRotate(QTest.Int("Hand L", 0, -180, 180));
			HandR.LimbRotate(QTest.Int("Hand R", 0, -180, 180));

			QTest.Group("Leg");
			UpperLegL.LimbRotate(QTest.Int("LegU L", 0, -180, 180));
			UpperLegR.LimbRotate(QTest.Int("LegU R", 0, -180, 180));
			LowerLegL.LimbRotate(QTest.Int("LegL L", 0, -180, 180));
			LowerLegR.LimbRotate(QTest.Int("LegL R", 0, -180, 180));
			FootL.LimbRotate(QTest.Int("Foot L", 0, -180, 180));
			FootR.LimbRotate(QTest.Int("Foot R", 0, -180, 180));

		}
	}


	// VAR
	private static int Zooming = -1;
	private static bool EditPlayerAnimation = false;
	private static int TargetMinViewHeight;


	// MSG
	[OnGameInitialize]
	internal static void OnGameInitialize () {
		TargetMinViewHeight = Universe.BuiltInInfo.MinViewHeight;
	}


	[OnGameUpdateLater]
	internal static void OnGameUpdateLater () {

		if (Game.IsToolApplication) return;

		UpdateDebugDragging();
		UpdateDebugHotkey();
		UpdateZoom();
		PlayerPoseAnimation();

	}


	[CheatCode("Zoom10")] internal static void ZoomCheatCode10 () => Zooming = 10;
	[CheatCode("Zoom20")] internal static void ZoomCheatCode20 () => Zooming = 20;
	[CheatCode("Zoom30")] internal static void ZoomCheatCode30 () => Zooming = 30;
	[CheatCode("Zoom40")] internal static void ZoomCheatCode40 () => Zooming = 40;
	[CheatCode("Zoom50")] internal static void ZoomCheatCode50 () => Zooming = 50;
	[CheatCode("Zoom60")] internal static void ZoomCheatCode60 () => Zooming = 60;
	[CheatCode("Zoom70")] internal static void ZoomCheatCode70 () => Zooming = 70;
	[CheatCode("Zoom80")] internal static void ZoomCheatCode80 () => Zooming = 80;
	[CheatCode("Zoom90")] internal static void ZoomCheatCode90 () => Zooming = 90;
	[CheatCode("Zoom100")]
	internal static void ZoomCheatCode100 () {
		Zooming = -1;
		Universe.BuiltInInfo.MinViewHeight = TargetMinViewHeight;
	}
	[CheatCode("Zoom")]
	internal static void ZoomCheatCode () {
		Zooming = -1;
		Universe.BuiltInInfo.MinViewHeight = TargetMinViewHeight;
	}
	[CheatCode("Zoom150")] internal static void ZoomCheatCode150 () => Zooming = 150;
	[CheatCode("Zoom200")] internal static void ZoomCheatCode200 () => Zooming = 200;
	[CheatCode("PlayerAnimation")]
	internal static void PlayerAnimationCheatCode () {
		EditPlayerAnimation = !EditPlayerAnimation;
		Zooming = EditPlayerAnimation ? 20 : -1;
	}


	private static void UpdateDebugDragging () {

		var player = PlayerSystem.Selecting;
		if (MapEditor.IsEditing || !DragPlayerInMiddleButtonToMove || player == null || !Input.MouseMidButtonHolding) {
			// End Hook Task
			if (TaskSystem.GetCurrentTask() is EntityHookTask hTask) {
				hTask.UserData = null;
			}
			return;
		}

		// Revive
		if (player.CharacterState == CharacterState.PassOut) {
			player.Health.Heal(player.Health.MaxHP);
			player.SetCharacterState(CharacterState.GamePlay);
		}

		// Move Player to Cursor Pos
		var mousePos = Input.MouseGlobalPosition;
		player.X = mousePos.x;
		player.Y = mousePos.y - Const.CEL * 2;
		PlayerSystem.IgnorePlayerView.True(1, 4096);
		if (player.Rendering is PoseCharacterRenderer pRenderer) {
			pRenderer.ManualPoseAnimate(PoseAnimation_Idle.TYPE_ID, 1);
		}
		player.IgnorePhysics.True(1, 4096);
		player.Health.MakeInvincible(1);

		// Shift Layer
		if (Input.KeyboardDown(KeyboardKey.S)) {
			Stage.SetViewZ(Stage.ViewZ - 1);
		}
		if (Input.KeyboardDown(KeyboardKey.W)) {
			Stage.SetViewZ(Stage.ViewZ + 1);
		}

		// Move View when Hover on Edge
		var cameraRect = Renderer.CameraRect;
		var center = cameraRect.CenterInt();
		int startMoveDis = cameraRect.height / 4;
		int mouseCenterDis = Util.DistanceInt(mousePos, center);
		if (mouseCenterDis > startMoveDis) {
			// Move
			var viewRect = Stage.ViewRect;
			var delta = (Float2)(mousePos - center);
			float targetMag = (delta.Magnitude - startMoveDis) / 16f;
			delta = delta.Normalized * targetMag;
			viewRect.x += delta.x.RoundToInt();
			viewRect.y += delta.y.RoundToInt() * cameraRect.width / cameraRect.height;
			Stage.SetViewPositionDelay(viewRect.x, viewRect.y, priority: 4096);
			PlayerSystem.AimViewX = viewRect.x;
			PlayerSystem.AimViewY = viewRect.y;
		}

		// Task
		if (TaskSystem.GetCurrentTask() is not EntityHookTask) {
			TaskSystem.AddToFirst(EntityHookTask.TYPE_ID, player);
		}

	}


	private static void UpdateDebugHotkey () {

		if (!UseDebugHotkey) return;

		// Make Player Invincible
		var player = PlayerSystem.Selecting;
		if (player != null && Input.KeyboardDown(KeyboardKey.I) && Input.HoldingCtrl && Input.HoldingAlt) {
			if (player.Health.IsInvincible) {
				player.Health.MakeInvincible(-1);
				NotificationUI.SpawnNotification("Cancel player invincible (Ctrl + Alt + I)");
			} else {
				player.Health.MakeInvincible(int.MaxValue - Game.GlobalFrame - 1);
				NotificationUI.SpawnNotification("Active player invincible (Ctrl + Alt + I)");
			}
		}

	}


	private static void UpdateZoom () {

		if (Zooming < 0) return;

		PlayerSystem.TargetViewHeight.Override(
			Universe.BuiltInInfo.DefaultViewHeight * Zooming / 100,
			duration: 1,
			priority: 9000
		);
		Universe.BuiltInInfo.MinViewHeight = Const.CEL;

	}


	private static void PlayerPoseAnimation () {

		var player = PlayerSystem.Selecting;
		if (
			!EditPlayerAnimation || MapEditor.IsEditing ||
			player == null || !player.Active || player.Rendering is not PoseCharacterRenderer rendering
		) {
			if (EditPlayerAnimation) {
				EditPlayerAnimation = false;
				Zooming = -1;
				Universe.BuiltInInfo.MinViewHeight = TargetMinViewHeight;
			}
			return;
		}

		DragPlayerInMiddleButtonToMove.False(1, 9000);
		rendering.ManualPoseAnimate(TestPoseAnimation.TYPE_ID, 1);

	}


#endif
}
