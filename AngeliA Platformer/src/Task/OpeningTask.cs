using System.Collections;
using System.Collections.Generic;

using AngeliA;
namespace AngeliA.Platformer;

public class OpeningTask : Task {



	// Const
	public static readonly int TYPE_ID = typeof(OpeningTask).AngeHash();
	private const int FADE_OUT_DURATION = 60;
	private const int DURATION = 180;
	private const int BLACK_DURATION = 120;
	private const int SKIP_DURATION = 12;
	private const int DOLLY_HEIGHT = Const.CEL * 10;

	// Api
	public int TargetViewX { get; set; } = 0;
	public int TargetViewY { get; set; } = 0;
	public int TargetViewZ { get; set; } = 0;
	public bool FadeOut { get; set; } = true;

	// Data
	private int SkipFrame = int.MaxValue;
	private int SkipY = 0;
	private int PlayerSpawnX = 0;
	private int PlayerSpawnY = 0;


	// MSG
	public override void OnStart () {
		base.OnStart();
		SkipFrame = int.MaxValue;
		PlayerSpawnX = TargetViewX;
		PlayerSpawnY = TargetViewY;
		TargetViewY += Stage.ViewRect.height / 2 - PlayerSystem.GetCameraShiftOffset(Stage.ViewRect.height);
		Stage.SetViewSizeDelay(Universe.BuiltInInfo.DefaultViewHeight, 1000, int.MaxValue);
		if (FadeOut) {
			Game.PassEffect_RetroDarken(1f);
		}
	}


	public override TaskResult FrameUpdate () {

		int localFrame = LocalFrame;

		// Fade Out
		if (FadeOut) {
			if (localFrame < FADE_OUT_DURATION) {
				var view = Stage.ViewRect;
				Stage.SetViewPositionDelay(view.x, view.y, 1000, 0);
				Game.PassEffect_RetroDarken((float)localFrame / FADE_OUT_DURATION);
				return TaskResult.Continue;
			} else {
				localFrame -= FADE_OUT_DURATION;
			}
		}

		if (FadeOut && localFrame < 2) {
			Game.PassEffect_RetroDarken(1f);
		}

		// Frame 1
		if (localFrame == 1) {
			var player = PlayerSystem.Selecting;
			if (player != null) {
				player.Active = false;
				player.SetCharacterState(CharacterState.GamePlay);
			}
			SetViewPosition(TargetViewX, TargetViewY + DOLLY_HEIGHT);
			Stage.SetViewZ(TargetViewZ);
		}

		// Frame 2
		if (localFrame == 3) {
			var player = PlayerSystem.Selecting;
			if (player != null) {
				if (!player.Active) {
					player = Stage.SpawnEntity(player.TypeID, PlayerSpawnX, PlayerSpawnY) as Character;
				} else {
					player.X = PlayerSpawnX;
					player.Y = PlayerSpawnY;
				}
				player?.OnActivated();
				if (
					player != null &&
					Physics.GetEntity<Bed>(
						IRect.Point(PlayerSpawnX, PlayerSpawnY),
						PhysicsMask.ENVIRONMENT, null,
						OperationMode.ColliderAndTrigger
					) is Bed bed
				) {
					bed.GetTargetOnBed(PlayerSystem.Selecting);
					TargetViewX = player.Rect.CenterX();
					TargetViewY = player.Y + Stage.ViewRect.height / 2 - PlayerSystem.GetCameraShiftOffset(Stage.ViewRect.height);
					SetViewPosition(TargetViewX, TargetViewY + DOLLY_HEIGHT);
				}
			}
		}

		if (localFrame < SkipFrame) {
			// Slow 
			if (localFrame > 2 && Input.AnyKeyDown) {
				SkipFrame = localFrame;
				SkipY = (int)Util.Remap(0f, DURATION, TargetViewY + DOLLY_HEIGHT, TargetViewY, localFrame);
			}
			// Black FadeIn
			if (localFrame <= BLACK_DURATION) {
				Game.PassEffect_RetroDarken(1f - (float)localFrame / BLACK_DURATION);
			}
			if (localFrame < DURATION) {
				// Camera Down
				SetViewPosition(
					TargetViewX,
					(int)Util.Remap(0f, DURATION, TargetViewY + DOLLY_HEIGHT, TargetViewY, localFrame)
				);
				return TaskResult.Continue;
			} else {
				// End
				return TaskResult.End;
			}
		} else {
			// Quick
			if (localFrame < SKIP_DURATION + SkipFrame) {
				SetViewPosition(
					TargetViewX,
					(int)Util.Remap((float)SkipFrame, SKIP_DURATION + SkipFrame, SkipY, TargetViewY, localFrame)
				);
				return TaskResult.Continue;
			} else {
				// End
				return TaskResult.End;
			}
		}
	}


	[OnGameRestart]
	public static void OnGameRestart () {

		if (PlayerSystem.Selecting == null) return;

		// Get Start Position
		var startUnitPosition = PlayerSystem.GetPlayerFinalRespawnUnitPosition();

		// Start Opening
		if (
			TaskSystem.TryAddToLast(TYPE_ID, out var task) &&
			task is OpeningTask oTask
		) {
			Stage.SetViewSizeDelay(Universe.BuiltInInfo.DefaultViewHeight, 1000, int.MaxValue);
			oTask.TargetViewX = startUnitPosition.x.ToGlobal();
			oTask.TargetViewY = startUnitPosition.y.ToGlobal();
			oTask.TargetViewZ = startUnitPosition.z;
			oTask.FadeOut = false;
		}
	}


	// LGC
	private void SetViewPosition (int x, int y) => Stage.SetViewPositionDelay(
		x - Stage.ViewRect.width / 2,
		y - Stage.ViewRect.height / 2,
		1000, 0
	);


}