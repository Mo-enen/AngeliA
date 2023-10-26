using System.Collections;
using System.Collections.Generic;


namespace AngeliaFramework {
	public abstract partial class Character {




		#region --- Movement ---


		public readonly BuffInt MovementWidth = new(150);
		public readonly BuffInt MovementHeight = new(384);
		public readonly BuffInt RollingHeight = new(200);
		public readonly BuffInt KnockbackDeceleration = new(16);
		public readonly BuffInt PushSpeedLoseRate = new(500);

		// Walk
		public readonly BuffInt WalkSpeed = new(20);
		public readonly BuffInt WalkAcceleration = new(3);
		public readonly BuffInt WalkBrakeAcceleration = new(30);
		public readonly BuffInt WalkDeceleration = new(4);

		// Run
		public readonly BuffInt WalkToRunAccumulation = new(0);
		public readonly BuffInt RunSpeed = new(32);
		public readonly BuffInt RunAcceleration = new(3);
		public readonly BuffInt RunBrakeAcceleration = new(30);
		public readonly BuffInt RunDeceleration = new(4);

		// Jump
		public readonly BuffInt JumpSpeed = new(73);
		public readonly BuffInt JumpCount = new(2);
		public readonly BuffInt JumpReleaseLoseRate = new(700);
		public readonly BuffInt JumpRiseGravityRate = new(600);
		public readonly BuffBool GrowJumpCountWhenFallOffEdge = new(true);
		public readonly BuffBool FirstJumpWithRoll = new(false);
		public readonly BuffBool SubsequentJumpWithRoll = new(true);
		public readonly BuffBool JumpDownThoughOneway = new(false);

		// Dash
		public readonly BuffBool DashAvailable = new(true);
		public readonly BuffBool DashWithRoll = new(false);
		public readonly BuffInt DashHeight = new(200);
		public readonly BuffInt DashSpeed = new(42);
		public readonly BuffInt DashDuration = new(20);
		public readonly BuffInt DashCooldown = new(4);
		public readonly BuffInt DashAcceleration = new(24);
		public readonly BuffInt DashCancelLoseRate = new(300);

		// Rush
		public readonly BuffBool RushAvailable = new(true);
		public readonly BuffBool RushInAir = new(false);
		public readonly BuffBool RushInWater = new(true);
		public readonly BuffBool RushWhenClimb = new(false);
		public readonly BuffBool RushWhenSquat = new(false);
		public readonly BuffInt RushHeight = new(384);
		public readonly BuffInt RushSpeed = new(72);
		public readonly BuffInt RushStopSpeed = new(8);
		public readonly BuffInt RushDuration = new(8);
		public readonly BuffInt RushStiff = new(10);
		public readonly BuffInt RushCooldown = new(2);
		public readonly BuffInt RushAcceleration = new(12);
		public readonly BuffInt RushDeceleration = new(4);

		// Squat
		public readonly BuffBool SquatAvailable = new(true);
		public readonly BuffInt SquatHeight = new(200);
		public readonly BuffInt SquatSpeed = new(14);
		public readonly BuffInt SquatAcceleration = new(48);
		public readonly BuffInt SquatDeceleration = new(48);

		// Pound
		public readonly BuffBool PoundAvailable = new(true);
		public readonly BuffInt PoundSpeed = new(96);

		// Swim
		public readonly BuffInt SwimWidth = new(200);
		public readonly BuffInt SwimHeight = new(384);
		public readonly BuffInt InWaterSpeedLoseRate = new(500);
		public readonly BuffInt SwimSpeed = new(42);
		public readonly BuffInt SwimJumpSpeed = new(128);
		public readonly BuffInt SwimAcceleration = new(4);
		public readonly BuffInt SwimDeceleration = new(4);

		// Climb
		public readonly BuffBool ClimbAvailable = new(true);
		public readonly BuffBool JumpWhenClimbAvailable = new(true);
		public readonly BuffInt ClimbSpeedX = new(12);
		public readonly BuffInt ClimbSpeedY = new(18);

		// Fly
		public readonly BuffInt FlyHeight = new(200);
		public readonly BuffInt FlyCooldown = new(24);
		public readonly BuffInt FlyRiseSpeed = new(96);
		public readonly BuffInt FlyGravityRiseRate = new(800);
		public readonly BuffInt FlyGravityFallRate = new(200);
		public readonly BuffInt FlyFallSpeed = new(12);
		public readonly BuffInt FlyMoveSpeed = new(32);
		public readonly BuffInt FlyAcceleration = new(2);
		public readonly BuffInt FlyDeceleration = new(1);

		// Slide
		public readonly BuffBool SlideAvailable = new(false);
		public readonly BuffBool SlideOnAnyBlock = new(false);
		public readonly BuffBool ResetJumpCountWhenSlide = new(true);
		public readonly BuffInt SlideDropSpeed = new(4);

		// Grab
		public readonly BuffBool GrabTopAvailable = new(true);
		public readonly BuffBool GrabSideAvailable = new(true);
		public readonly BuffBool ResetJumpCountWhenGrab = new(true);
		public readonly BuffBool GrabFlipThroughDownAvailable = new(true);
		public readonly BuffBool GrabFlipThroughUpAvailable = new(true);
		public readonly BuffInt GrabFlipThroughDuration = new(18);
		public readonly BuffInt GrabTopHeight = new(364);
		public readonly BuffInt GrabSideHeight = new(364);
		public readonly BuffInt GrabMoveSpeedX = new(24);
		public readonly BuffInt GrabMoveSpeedY = new(24);


		#endregion




		#region --- Attack ---


		public readonly BuffInt AttackDuration = new(12);
		public readonly BuffInt AttackCooldown = new(2);
		public readonly BuffInt AttackComboGap = new(12);
		public readonly BuffInt HoldAttackPunish = new(4);
		public readonly BuffInt MinimalChargeAttackDuration = new(int.MaxValue);
		public readonly BuffInt MovementLoseRateOnAttack = new(0);
		public readonly BuffBool CancelAttackOnJump = new(false);
		public readonly BuffBool RepeatAttackWhenHolding = new(false);
		public readonly BuffBool LockFacingOnAttack = new(false);
		public readonly BuffBool AttackInAir = new(true);
		public readonly BuffBool AttackInWater = new(true);
		public readonly BuffBool AttackWhenMoving = new(true);
		public readonly BuffBool AttackWhenClimbing = new(false);
		public readonly BuffBool AttackWhenFlying = new(false);
		public readonly BuffBool AttackWhenRolling = new(false);
		public readonly BuffBool AttackWhenSquatting = new(false);
		public readonly BuffBool AttackWhenDashing = new(false);
		public readonly BuffBool AttackWhenSliding = new(false);
		public readonly BuffBool AttackWhenGrabbing = new(false);
		public readonly BuffBool AttackWhenRush = new(false);


		#endregion




		#region --- Health ---


		public readonly BuffInt MaxHP = new(1);
		public readonly BuffInt InvincibleDuration = new(120);
		public readonly BuffInt DamageStunDuration = new(24);
		public readonly BuffInt KnockBackSpeed = new(64);
		public readonly BuffBool InvincibleOnDash = new(false);
		public readonly BuffBool InvincibleOnRush = new(false);


		#endregion




	}
}