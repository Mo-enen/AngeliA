using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA.Platformer;


public enum CharacterNavigationState { Idle, Operation, Fly, }


public class CharacterNavigation (Character character) {




	#region --- VAR ---


	// Api
	public readonly Character TargetCharacter = character;
	public CharacterNavigationState NavigationState { get; set; } = CharacterNavigationState.Idle;
	public Int2 NavigationAim { get; protected set; } = default;
	public bool NavigationAimGrounded { get; protected set; } = default;
	public bool HasPerformableOperation => CurrentNavOperationIndex < CurrentNavOperationCount || CurrentNavOperationCount == 0;

	// Override
	public virtual bool NavigationEnable => false;
	public virtual bool ClampInSpawnRect => false;
	public virtual int NavigationStartMoveDistance => Const.CEL * 4;
	public virtual int NavigationEndMoveDistance => Const.CEL * 1;
	public virtual int NavigationStartFlyDistance => Const.CEL * 18;
	public virtual int NavigationEndFlyDistance => Const.CEL * 3;
	public virtual int NavigationMinimumFlyDuration => 60;
	public virtual int NavigationJumpSpeed => 32;
	public virtual int NavigationMaxJumpDuration => 80;

	// Short
	private IRect Rect => TargetCharacter.Rect;
	private int X { get => TargetCharacter.X; set => TargetCharacter.X = value; }
	private int Y { get => TargetCharacter.Y; set => TargetCharacter.Y = value; }
	private int VelocityX { get => TargetCharacter.VelocityX; set => TargetCharacter.VelocityX = value; }
	private int VelocityY { get => TargetCharacter.VelocityY; set => TargetCharacter.VelocityY = value; }
	private bool InWater => TargetCharacter.InWater;
	private bool IsGrounded => TargetCharacter.IsGrounded;
	private bool IsInsideGround => TargetCharacter.IsInsideGround;
	private CharacterMovement Movement => TargetCharacter.Movement;

	// Data
	private readonly Navigation.Operation[] NavOperations = new Navigation.Operation[64];
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


	public virtual void OnActivated () => ResetNavigation();


	public virtual void PhysicsUpdate () {

		if (!NavigationEnable) return;

		// Clamp In Range
		if (ClampInSpawnRect) {
			var range = Stage.SpawnRect;
			if (!range.Overlaps(Rect)) {
				X = X.Clamp(range.xMin, range.xMax);
				Y = Y.Clamp(range.yMin, range.yMax);
				NavigationState = CharacterNavigationState.Fly;
				NavFlyStartFrame = Game.GlobalFrame;
			}
		}

		// Refresh
		NavUpdate_State();

		// Perform Navigate
		if (CurrentNavOperationIndex >= CurrentNavOperationCount) {
			CurrentNavOperationCount = 0;
			CurrentNavOperationIndex = 0;
			if (NavigationState == CharacterNavigationState.Operation) {
				CurrentNavOperationCount = Navigation.NavigateTo(
					NavOperations, Game.GlobalFrame, Stage.ViewRect, X, Y, NavigationAim.x, NavigationAim.y
				);
				if (CurrentNavOperationCount == 0) {
					NavigationState = CharacterNavigationState.Idle;
				}
				NavMoveDoneX = false;
				NavMoveDoneY = false;
			}
		}

		// Move to Target
		switch (NavigationState) {
			case CharacterNavigationState.Idle:
				NavUpdate_Movement_Idle();
				break;
			case CharacterNavigationState.Operation:
				NavUpdate_Movement_Operating();
				break;
			case CharacterNavigationState.Fly:
				NavUpdate_Movement_Flying();
				break;
		}

	}


	private void NavUpdate_State () {

		// Don't Refresh State when Jumping
		if (
			NavigationState == CharacterNavigationState.Operation &&
			CurrentNavOperationIndex >= 0 &&
			CurrentNavOperationIndex < CurrentNavOperationCount &&
			NavOperations[CurrentNavOperationIndex].Motion == NavigationOperateMotion.Jump
		) return;

		int START_MOVE_DISTANCE_SQ = NavigationStartMoveDistance * NavigationStartMoveDistance;
		int END_MOVE_DISTANCE_SQ = NavigationEndMoveDistance * NavigationEndMoveDistance;
		int START_FLY_DISTANCE_SQ = NavigationStartFlyDistance * NavigationStartFlyDistance;
		int END_FLY_DISTANCE_SQ = NavigationEndFlyDistance * NavigationEndFlyDistance;

		// Fly When No Grounded Aim Position
		if (!NavigationAimGrounded) {
			NavigationState = CharacterNavigationState.Fly;
			NavFlyStartFrame = Game.GlobalFrame;
			return;
		}

		int aimSqrtDis = Util.SquareDistance(NavigationAim.x, NavigationAim.y, X, Y);
		switch (NavigationState) {

			case CharacterNavigationState.Idle:
				if (aimSqrtDis > START_FLY_DISTANCE_SQ) {
					// Idle >> Fly
					NavigationState = CharacterNavigationState.Fly;
					NavFlyStartFrame = Game.GlobalFrame;
				} else if (aimSqrtDis > START_MOVE_DISTANCE_SQ) {
					// Idle >> Nav
					NavigationState = CharacterNavigationState.Operation;
				}
				break;

			case CharacterNavigationState.Operation:
				if (aimSqrtDis > START_FLY_DISTANCE_SQ) {
					// Nav >> Fly
					NavigationState = CharacterNavigationState.Fly;
					NavFlyStartFrame = Game.GlobalFrame;
				} else if (CurrentNavOperationIndex >= CurrentNavOperationCount && aimSqrtDis < END_MOVE_DISTANCE_SQ) {
					// Nav >> Idle
					NavigationState = CharacterNavigationState.Idle;
				}
				break;

			case CharacterNavigationState.Fly:
				if (
					Game.GlobalFrame > NavFlyStartFrame + NavigationMinimumFlyDuration &&
					aimSqrtDis < END_FLY_DISTANCE_SQ &&
					Y > NavigationAim.y - Const.HALF
				) {
					// Fly >> ??
					NavigationState = aimSqrtDis > START_MOVE_DISTANCE_SQ ?
						CharacterNavigationState.Operation :
						CharacterNavigationState.Idle;
				}
				break;

		}

	}


	private void NavUpdate_Movement_Idle () {
		VelocityX = 0;
		if (!InWater && !IsInsideGround) {
			if (Navigation.IsGround(Game.GlobalFrame, Stage.ViewRect, X, Y + Const.HALF / 2, out int groundY)) {
				// Move to Ground
				VelocityY = groundY - Y;
				TargetCharacter.MakeGrounded(1);
			} else {
				// Fall Down
				int gravity = Rigidbody.GlobalGravity * (VelocityY <= 0 ? TargetCharacter.FallingGravityScale / 1000 : TargetCharacter.RisingGravityScale / 1000);
				VelocityY = (VelocityY - gravity).Clamp(-TargetCharacter.MaxGravitySpeed, int.MaxValue);
			}
		} else {
			VelocityY = 0;
		}
		Y += VelocityY;
		Movement.MovementState = InWater ? CharacterMovementState.SwimIdle : CharacterMovementState.Idle;
	}


	private void NavUpdate_Movement_Operating () {

		if (CurrentNavOperationIndex >= CurrentNavOperationCount) return;

		var operation = NavOperations[CurrentNavOperationIndex];
		int targetX = operation.TargetGlobalX;
		int targetY = operation.TargetGlobalY;
		var motion = operation.Motion;

		switch (motion) {

			// Move
			case NavigationOperateMotion.Move:

				int speed = InWater ? Movement.SwimSpeed * Movement.InWaterSpeedRate / 1000 : Movement.RunSpeed;

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
					if (!Navigation.IsGround(Game.GlobalFrame, Stage.ViewRect, targetX, targetY, out _)) {
						NavigationState = CharacterNavigationState.Fly;
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
					int dis = Util.DistanceInt(X, Y, targetX, targetY);
					NavJumpFrame = 0;
					NavJumpDuration = (dis / NavigationJumpSpeed).Clamp(dis < Const.HALF ? 3 : 24, NavigationMaxJumpDuration);
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
			Movement.Move(VelocityX > 0 ? Direction3.Right : Direction3.Left, 0);
		}
		if (!InWater) {
			// Move
			Movement.MovementState =
				motion == NavigationOperateMotion.Move ? CharacterMovementState.Run :
				VelocityY > 0 ? CharacterMovementState.JumpUp : CharacterMovementState.JumpDown;
		} else {
			// Swim
			Movement.MovementState = VelocityX == 0 ? CharacterMovementState.SwimIdle : CharacterMovementState.SwimMove;
		}

		// Func
		void GotoNextOperation () {
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
	}


	private void NavUpdate_Movement_Flying () {
		int speed = Movement.FlyMoveSpeed;
		if (InWater) speed = speed * Movement.InWaterSpeedRate / 1000;
		var flyAim = NavigationAim;
		if (Movement.FlyAvailable) {
			// Can Fly
			flyAim.x = X.LerpTo(flyAim.x, 100);
			flyAim.y = Y.LerpTo(flyAim.y, 100);
			VelocityX = (flyAim.x - X).Clamp(-speed, speed);
			VelocityY = (flyAim.y - Y).Clamp(-speed, speed);
			X += VelocityX;
			Y += VelocityY;
			// Move State
			Movement.MovementState = CharacterMovementState.Fly;
			if ((X - NavigationAim.x).Abs() > 96) {
				Movement.Move(X < NavigationAim.x ? Direction3.Right : Direction3.Left, 0);
			}
		} else {
			// Can't Fly
			X = flyAim.x;
			Y = flyAim.y;
			NavigationState = CharacterNavigationState.Idle;
			Movement.MovementState = CharacterMovementState.Idle;
		}
	}


	#endregion




	#region --- API ---


	public void SetNavigationAim (Int2 newAim, bool grounded) {
		NavigationAim = newAim;
		NavigationAimGrounded = grounded;
		CurrentNavOperationIndex = 0;
		CurrentNavOperationCount = 0;
		NavJumpDuration = 0;
	}


	public void ResetNavigation () {
		NavigationState = CharacterNavigationState.Idle;
		NavFlyStartFrame = int.MinValue;
		CurrentNavOperationIndex = 0;
		CurrentNavOperationCount = 0;
		NavigationAim = new Int2(X, Y);
		NavigationAimGrounded = IsGrounded;
		NavJumpFrame = 0;
		NavJumpDuration = 0;
		NavMoveDoneX = false;
		NavMoveDoneY = false;
	}


	#endregion




}