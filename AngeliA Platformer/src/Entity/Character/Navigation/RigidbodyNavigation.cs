using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA.Platformer;


internal enum RigidbodyNavigationState { Idle, Operation, Fly, }


/// <summary>
/// Behavior that handles auto movement for a target rigidbody
/// </summary>
public class RigidbodyNavigation (Rigidbody target) {




	#region --- VAR ---


	// Api
	public readonly Rigidbody Target = target;
	/// <summary>
	/// Current target position this navigation want to go for
	/// </summary>
	public Int2 NavigationAim { get; set; } = default;
	/// <summary>
	/// True if the aim position is touching ground
	/// </summary>
	public bool NavigationAimGrounded { get; set; } = default;
	/// <summary>
	/// True if there is still any operation to perform
	/// </summary>
	public bool HasPerformableOperation => CurrentNavOperationIndex < CurrentNavOperationCount || CurrentNavOperationCount == 0;
	internal RigidbodyNavigationState NavigationState { get; set; } = RigidbodyNavigationState.Idle;

	// Override
	/// <summary>
	/// True if this behavior should work currently
	/// </summary>
	public virtual bool NavigationEnable => true;
	/// <summary>
	/// True if the target should only be inside the stage spawn rect
	/// </summary>
	public virtual bool ClampInSpawnRect => false;
	/// <summary>
	/// True if the target immediately goes to the aim position when they can not fly
	/// </summary>
	public virtual bool TeleportWhenCantFly => false;
	public virtual int DefaultRunSpeed => 12;
	public virtual int DefaultFlySpeed => 32;
	/// <summary>
	/// Position offset for distinguish multiple instance with same type
	/// </summary>
	public virtual int InstanceShift => 0;
	/// <summary>
	/// Start to move when distance between target ans aim position is larger than this value
	/// </summary>
	public virtual int StartMoveDistance => Const.CEL * 4;
	/// <summary>
	/// Stop moving when distance between target ans aim position is smaller than this value
	/// </summary>
	public virtual int EndMoveDistance => Const.CEL * 1;
	/// <summary>
	/// Start to fly when distance between target ans aim position is larger than this value
	/// </summary>
	public virtual int StartFlyDistance => Const.CEL * 18;
	/// <summary>
	/// Stop flying when distance between target ans aim position is smaller than this value
	/// </summary>
	public virtual int EndFlyDistance => Const.CEL * 1;
	/// <summary>
	/// At least fly this frames long
	/// </summary>
	public virtual int MinimumFlyDuration => 60;
	public virtual int JumpSpeed => 32;
	public virtual int MaxJumpDuration => 80;
	/// <summary>
	/// Duration in frame for slowly stop moving
	/// </summary>
	public virtual int EndMoveSlowDown => 26;

	// Short
	private IRect Rect => Target.Rect;
	private int X { get => Target.X; set => Target.X = value; }
	private int Y { get => Target.Y; set => Target.Y = value; }
	private int VelocityX { get => Target.VelocityX; set => Target.VelocityX = value; }
	private int VelocityY { get => Target.VelocityY; set => Target.VelocityY = value; }
	private CharacterMovement Movement => (Target is IWithCharacterMovement wMove) ? wMove.CurrentMovement : null;

	// Data
	private readonly Navigation.Operation[] NavOperations = new Navigation.Operation[64];
	private static bool DrawDebugGizmos = false;
	private int CurrentNavOperationIndex = 0;
	private int CurrentNavOperationCount = 0;
	private int NavFlyStartFrame = int.MinValue;
	private int NavJumpFrame = 0;
	private int NavJumpDuration = 0;
	private bool NavMoveDoneX = false;
	private bool NavMoveDoneY = false;
	private Int2 NavJumpFromPosition = default;


	#endregion




	#region --- MSG ---

#if DEBUG
	[CheatCode("DrawNavGizmos")]
	internal static void DebugGizmosCheat () => DrawDebugGizmos = true;
#endif

	public virtual void OnActivated () => ResetNavigation();


	public virtual void PhysicsUpdate () {

		if (!NavigationEnable) return;

		// Clamp In Range
		if (ClampInSpawnRect) {
			var range = Stage.SpawnRect;
			if (!range.Overlaps(Rect)) {
				X = X.Clamp(range.xMin, range.xMax);
				Y = Y.Clamp(range.yMin, range.yMax);
				NavigationState = RigidbodyNavigationState.Fly;
				NavFlyStartFrame = Game.GlobalFrame;
			}
		}

		// Refresh
		NavUpdate_State();

		// Physics
		if (NavigationState != RigidbodyNavigationState.Idle) {
			Target.IgnorePhysics.True(1);
		}

		// Perform Navigate
		if (CurrentNavOperationIndex >= CurrentNavOperationCount) {
			CurrentNavOperationCount = 0;
			CurrentNavOperationIndex = 0;
			if (NavigationState == RigidbodyNavigationState.Operation) {
				Navigation.NavigateTo(
					NavOperations, out CurrentNavOperationCount, X, Y, NavigationAim.x, NavigationAim.y
				);
				if (CurrentNavOperationCount == 0) {
					NavigationState = RigidbodyNavigationState.Idle;
				}
				NavMoveDoneX = false;
				NavMoveDoneY = false;
			}
		}

		// Move to Target
		switch (NavigationState) {
			case RigidbodyNavigationState.Idle:
				NavUpdate_Movement_Idle();
				break;
			case RigidbodyNavigationState.Operation:
				NavUpdate_Movement_Operating();
				break;
			case RigidbodyNavigationState.Fly:
				NavUpdate_Movement_Flying();
				break;
		}

		Movement?.UpdateLater();

		// Debug Gizmos
		if (DrawDebugGizmos) {
			var targetRect = Target.Rect;
			var pos = targetRect.CenterInt();
			if (NavigationState == RigidbodyNavigationState.Operation) {
				for (int i = CurrentNavOperationIndex; i < CurrentNavOperationCount; i++) {
					var operation = NavOperations[i];
					Game.DrawGizmosLine(
						pos.x, pos.y,
						operation.TargetGlobalX + targetRect.width / 2,
						operation.TargetGlobalY + targetRect.height / 2,
						8,
						operation.Motion == NavigationOperateMotion.Move ? Color32.RED : Color32.GREEN
					);
					pos.x = operation.TargetGlobalX + targetRect.width / 2;
					pos.y = operation.TargetGlobalY + targetRect.height / 2;
				}
			} else if (NavigationState == RigidbodyNavigationState.Fly) {
				Game.DrawGizmosLine(
					pos.x, pos.y,
					NavigationAim.x + targetRect.width / 2,
					NavigationAim.y + targetRect.height / 2,
					8,
					Color32.CYAN
				);
			}
		}

	}


	private void NavUpdate_State () {

		// Don't Refresh State when Jumping
		if (
			NavigationState == RigidbodyNavigationState.Operation &&
			CurrentNavOperationIndex >= 0 &&
			CurrentNavOperationIndex < CurrentNavOperationCount &&
			NavOperations[CurrentNavOperationIndex].Motion == NavigationOperateMotion.Jump
		) return;

		int START_MOVE_DISTANCE_SQ = StartMoveDistance * StartMoveDistance;
		int END_MOVE_DISTANCE_SQ = EndMoveDistance * EndMoveDistance;
		int START_FLY_DISTANCE_SQ = StartFlyDistance * StartFlyDistance;
		int END_FLY_DISTANCE_SQ = EndFlyDistance * EndFlyDistance;

		// Fly When No Grounded Aim Position
		if (!NavigationAimGrounded) {
			NavigationState = RigidbodyNavigationState.Fly;
			NavFlyStartFrame = Game.GlobalFrame;
			return;
		}

		int aimSqrtDis = Util.SquareDistance(NavigationAim.x, NavigationAim.y, X, Y);
		switch (NavigationState) {

			case RigidbodyNavigationState.Idle:
				if (aimSqrtDis > START_FLY_DISTANCE_SQ) {
					// Idle >> Fly
					NavigationState = RigidbodyNavigationState.Fly;
					NavFlyStartFrame = Game.GlobalFrame;
				} else if (aimSqrtDis > START_MOVE_DISTANCE_SQ) {
					// Idle >> Nav
					NavigationState = RigidbodyNavigationState.Operation;
				}
				break;

			case RigidbodyNavigationState.Operation:
				if (aimSqrtDis > START_FLY_DISTANCE_SQ) {
					// Nav >> Fly
					NavigationState = RigidbodyNavigationState.Fly;
					NavFlyStartFrame = Game.GlobalFrame;
				} else if (CurrentNavOperationIndex >= CurrentNavOperationCount && aimSqrtDis < END_MOVE_DISTANCE_SQ) {
					// Nav >> Idle
					NavigationState = RigidbodyNavigationState.Idle;
				}
				break;

			case RigidbodyNavigationState.Fly:
				if (
					Game.GlobalFrame > NavFlyStartFrame + MinimumFlyDuration &&
					aimSqrtDis < END_FLY_DISTANCE_SQ &&
					Y > NavigationAim.y - Const.HALF
				) {
					// Fly >> ??
					NavigationState = aimSqrtDis > START_MOVE_DISTANCE_SQ ?
						RigidbodyNavigationState.Operation :
						RigidbodyNavigationState.Idle;
					CurrentNavOperationIndex = 0;
					CurrentNavOperationCount = 0;
				}
				break;

		}

	}


	private void NavUpdate_Movement_Idle () {
		var mov = Movement;
		if (mov != null) {
			mov.Stop();
			mov.MovementState = Target.InWater ? CharacterMovementState.SwimIdle : CharacterMovementState.Idle;
		} else {
			VelocityX = 0;
		}
	}


	private void NavUpdate_Movement_Operating () {

		if (CurrentNavOperationIndex >= CurrentNavOperationCount) return;

		var mov = Movement;

		int insIndex = Target.InstanceOrder;
		var operation = NavOperations[CurrentNavOperationIndex];
		int targetX = operation.TargetGlobalX;
		int targetY = operation.TargetGlobalY;
		var motion = operation.Motion;
		if (InstanceShift != 0) {
			targetX += (
				(insIndex % 2 == 0 ? InstanceShift : -InstanceShift) * (insIndex / 2) + Const.HALF
			).UMod(Const.CEL) - Const.HALF;
		}

		switch (motion) {

			// Move
			case NavigationOperateMotion.Move:

				int speed =
					mov == null ? DefaultRunSpeed :
					Target.InWater ? mov.SwimSpeed * mov.InWaterSpeedRate / 1000 :
					mov.RunSpeed;

				// Slow Down
				if (CurrentNavOperationIndex == CurrentNavOperationCount - 1) {
					int endDis = (X - operation.TargetGlobalX).Abs() + (Y - operation.TargetGlobalY).Abs();
					int speedLimit = (endDis / EndMoveSlowDown.GreaterOrEquel(1)).Clamp(4, speed.Abs());
					speed = speed.Clamp(-speedLimit, speedLimit);
				} else if (CurrentNavOperationIndex == 0) {
					int endDis = (X - operation.TargetGlobalX).Abs() + (Y - operation.TargetGlobalY).Abs();
					speed = speed.MoveTowards(speed / 2, endDis / 2);
				}

				// Move
				if (targetX == X) NavMoveDoneX = true;
				if (targetY == Y) NavMoveDoneY = true;

				bool moveRight = targetX - X > 0;
				bool moveUp = targetY - Y > 0;

				VelocityX = NavMoveDoneX ? 0 : moveRight ? speed : -speed;
				VelocityY = NavMoveDoneY ? 0 : moveUp ? speed : -speed;

				X += VelocityX;
				Y += VelocityY;

				// Done Check
				NavMoveDoneX = NavMoveDoneX || (moveRight != targetX - X > 0);
				NavMoveDoneY = NavMoveDoneY || (moveUp != targetY - Y > 0);

				// Goto Next Operation
				if (NavMoveDoneX && NavMoveDoneY) {
					if (!Navigation.IsGround(targetX, targetY, out _)) {
						NavigationState = RigidbodyNavigationState.Fly;
						NavFlyStartFrame = Game.GlobalFrame;
						CurrentNavOperationIndex = 0;
						CurrentNavOperationCount = 0;
					} else {
						GotoNextOperation();
					}
				}

				break;


			// Jump
			case NavigationOperateMotion.Jump:

				if (NavJumpDuration == 0) {
					// Jump Start
					int dis = (X - targetX).Abs() + (Y - targetY).Abs() / 2;
					NavJumpFrame = 0;
					NavJumpDuration = (dis / JumpSpeed).Clamp(dis < Const.HALF ? 3 : 24, MaxJumpDuration);
					NavJumpFromPosition.x = X;
					NavJumpFromPosition.y = Y;
				}

				if (NavJumpDuration > 0 && NavJumpFrame <= NavJumpDuration) {
					// Jumping
					float lerp01 = NavJumpFrame / (float)NavJumpDuration;
					float jump01 = Ease.OutSine(lerp01);
					int newX = NavJumpFromPosition.x.LerpTo(targetX, jump01);
					int newY = NavJumpFromPosition.y.LerpTo(targetY, lerp01);
					if (NavJumpDuration > 3) {
						int deltaY = Util.Abs(NavJumpFrame - NavJumpDuration / 2);
						int arc = (Util.DistanceInt(
							NavJumpFromPosition.x,
							NavJumpFromPosition.y,
							targetX,
							targetY
						) / 2).Clamp(Const.HALF, Const.CEL * 3);
						newY += Util.Remap(
							NavJumpDuration * NavJumpDuration / 4, 0, 0, arc,
							deltaY * deltaY
						);
					}
					VelocityX = newX - X;
					VelocityY = newY - Y;
					X = newX;
					Y = newY;
					NavJumpFrame++;
				} else {
					// Jump End
					VelocityX = targetX - X;
					VelocityY = targetY - Y;
					X = targetX;
					Y = targetY;
					GotoNextOperation();
				}
				break;

			// None
			case NavigationOperateMotion.None:
				GotoNextOperation();
				break;
		}

		// Move State
		if (VelocityX != 0) {
			if (mov != null) {
				mov.Move(VelocityX > 0 ? Direction3.Right : Direction3.Left, 0);
				mov.FacingRight = VelocityX > 0;
			} else {
				Target.X += VelocityX;
			}
		}
		if (mov != null) {
			if (!Target.InWater) {
				// Move
				mov.MovementState =
					motion == NavigationOperateMotion.Move ? CharacterMovementState.Run :
					VelocityY > 0 ? CharacterMovementState.JumpUp : CharacterMovementState.JumpDown;
			} else {
				// Swim
				mov.MovementState = VelocityX == 0 ? CharacterMovementState.SwimIdle : CharacterMovementState.SwimMove;
			}
		}

	}


	private void NavUpdate_Movement_Flying () {
		var mov = Movement;
		int speed = mov == null ? DefaultFlySpeed : mov.FlyMoveSpeed;
		if (Target.InWater && mov != null) speed = speed * mov.InWaterSpeedRate / 1000;
		var flyAim = NavigationAim;
		if (mov == null || mov.FlyAvailable) {
			// Can Fly
			flyAim.x = X.LerpTo(flyAim.x, 100);
			flyAim.y = Y.LerpTo(flyAim.y, 100);
			VelocityX = (flyAim.x - X).Clamp(-speed, speed);
			VelocityY = (flyAim.y - Y).Clamp(-speed, speed);
			X += VelocityX;
			Y += VelocityY;
			// Move State
			if (mov != null) {
				mov.MovementState = CharacterMovementState.Fly;
				if ((X - NavigationAim.x).Abs() > 96) {
					mov.Move(X < NavigationAim.x ? Direction3.Right : Direction3.Left, 0);
				}
			}
		} else {
			// Can't Fly
			if (TeleportWhenCantFly) {
				X = flyAim.x;
				Y = flyAim.y;
			}
			NavigationState = RigidbodyNavigationState.Idle;
			if (mov != null) {
				mov.MovementState = CharacterMovementState.Idle;
			}
		}
	}


	#endregion




	#region --- API ---


	/// <summary>
	/// Set aim position in global space
	/// </summary>
	/// <param name="newAim"></param>
	/// <param name="grounded">True if the new aim position is grounded</param>
	public void SetNavigationAim (Int2 newAim, bool grounded) {
		NavigationAim = newAim;
		NavigationAimGrounded = grounded;
		CurrentNavOperationIndex = 0;
		CurrentNavOperationCount = 0;
		NavJumpDuration = 0;
	}


	public void ResetNavigation () {
		NavigationState = RigidbodyNavigationState.Idle;
		NavFlyStartFrame = int.MinValue;
		CurrentNavOperationIndex = 0;
		CurrentNavOperationCount = 0;
		NavigationAim = new Int2(X, Y);
		NavigationAimGrounded = Target.IsGrounded;
		NavJumpFrame = 0;
		NavJumpDuration = 0;
		NavMoveDoneX = false;
		NavMoveDoneY = false;
	}


	#endregion




	#region --- LGC ---


	private void GotoNextOperation () {
		CurrentNavOperationIndex++;
		NavJumpFrame = 0;
		NavJumpDuration = 0;
		NavMoveDoneX = false;
		NavMoveDoneY = false;
		// Skip Too Tiny Jump
		while (
			CurrentNavOperationIndex < CurrentNavOperationCount
		) {
			var motion = NavOperations[CurrentNavOperationIndex];
			if (
				motion.Motion == NavigationOperateMotion.Jump &&
				Util.SquareDistance(X, Y, motion.TargetGlobalX, motion.TargetGlobalY) <= Const.HALF * Const.HALF
			) {
				CurrentNavOperationIndex++;
			} else break;
		}
	}


	#endregion




}