using System.Collections;
using System.Collections.Generic;
using Moenen.Standard;


namespace Yaya {
	public abstract partial class eCharacter {


		public readonly BuffInt MovementWidth = new(150);
		public readonly BuffInt MovementHeight = new(384);
		public readonly BuffInt RollingHeight = new(200);
		public readonly BuffInt AntiKnockbackSpeed = new(16);

		// Walk
		public readonly BuffInt WalkSpeed = new(20);
		public readonly BuffInt WalkAcceleration = new(3);
		public readonly BuffInt WalkBrakeAcceleration = new(30);
		public readonly BuffInt WalkDecceleration = new(4);

		// Run
		public readonly BuffInt RunAccumulation = new(48);
		public readonly BuffInt RunSpeed = new(32);
		public readonly BuffInt RunAcceleration = new(3);
		public readonly BuffInt RunBrakeAcceleration = new(30);
		public readonly BuffInt RunDecceleration = new(4);

		// Jump
		public readonly BuffInt JumpSpeed = new(62);
		public readonly BuffInt JumpCount = new(2);
		public readonly BuffInt JumpReleaseLoseRate = new(700);
		public readonly BuffInt JumpRiseGravityRate = new(600);
		public readonly BuffBool GrowJumpCountWhenFallOffEdge = new(true);
		public readonly BuffBool JumpWithRoll = new(false);
		public readonly BuffBool SecondJumpWithRoll = new(true);
		public readonly BuffBool JumpThoughOneway = new(false);

		// Dash
		public readonly BuffBool DashAvailable = new(true);
		public readonly BuffInt DashHeight = new(200);
		public readonly BuffInt DashSpeed = new(42);
		public readonly BuffInt DashDuration = new(12);
		public readonly BuffInt DashCooldown = new(4);
		public readonly BuffInt DashAcceleration = new(24);
		public readonly BuffInt DashCancelLoseRate = new(300);

		// Squat
		public readonly BuffBool SquatAvailable = new(true);
		public readonly BuffInt SquatHeight = new(200);
		public readonly BuffInt SquatSpeed = new(14);
		public readonly BuffInt SquatAcceleration = new(48);
		public readonly BuffInt SquatDecceleration = new(48);

		// Pound
		public readonly BuffBool PoundAvailable = new(true);
		public readonly BuffInt PoundSpeed = new(96);

		// Swim
		public readonly BuffInt SwimWidth = new(256);
		public readonly BuffInt SwimHeight = new(384);
		public readonly BuffInt InWaterSpeedLoseRate = new(500);
		public readonly BuffInt SwimSpeed = new(42);
		public readonly BuffInt SwimJumpSpeed = new(128);
		public readonly BuffInt SwimAcceleration = new(4);
		public readonly BuffInt SwimDecceleration = new(4);
		public readonly BuffBool SwimInFreeStyle = new(false);

		// Free Swim
		public readonly BuffInt FreeSwimSpeed = new(40);
		public readonly BuffInt FreeSwimAcceleration = new(4);
		public readonly BuffInt FreeSwimDecceleration = new(4);
		public readonly BuffInt FreeSwimDashSpeed = new(84);
		public readonly BuffInt FreeSwimDashDuration = new(12);
		public readonly BuffInt FreeSwimDashCooldown = new(4);
		public readonly BuffInt FreeSwimDashAcceleration = new(128);

		// Climb
		public readonly BuffBool ClimbAvailable = new(true);
		public readonly BuffBool JumpWhenClimbAvailable = new(true);
		public readonly BuffInt ClimbSpeedX = new(12);
		public readonly BuffInt ClimbSpeedY = new(18);

		// Fly
		public readonly BuffBool FlyAvailable = new(false);
		public readonly BuffBool FlyGlideAvailable = new(false);
		public readonly BuffInt FlyHeight = new(200);
		public readonly BuffInt FlyCooldown = new(24);
		public readonly BuffInt FlyRiseSpeed = new(96);
		public readonly BuffInt FlyGravityRiseRate = new(800);
		public readonly BuffInt FlyGravityFallRate = new(200);
		public readonly BuffInt FlyFallSpeed = new(12);
		public readonly BuffInt FlyMoveSpeed = new(36);
		public readonly BuffInt FlyGlideAcceleration = new(2);
		public readonly BuffInt FlyGlideDecceleration = new(1);

		// Slide
		public readonly BuffBool SlideAvailable = new(false);
		public readonly BuffBool SlideOnAnyBlock = new(true);
		public readonly BuffBool ResetJumpCountWhenSlide = new(true);
		public readonly BuffInt SlideDropSpeed = new(4);

		// Grab
		public readonly BuffBool GrabTopAvailable = new(false);
		public readonly BuffBool GrabSideAvailable = new(false);
		public readonly BuffBool ResetJumpCountWhenGrab = new(true);
		public readonly BuffBool GrabFlipThroughDown = new(false);
		public readonly BuffBool GrabFlipThroughUp = new(false);
		public readonly BuffInt GrabFlipThroughDuration = new(18);
		public readonly BuffInt GrabTopHeight = new(364);
		public readonly BuffInt GrabSideHeight = new(364);
		public readonly BuffInt GrabMoveSpeedX = new(24);
		public readonly BuffInt GrabMoveSpeedY = new(24);



	}
}