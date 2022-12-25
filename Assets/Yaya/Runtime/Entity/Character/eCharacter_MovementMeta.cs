using System.Collections;
using System.Collections.Generic;
using Moenen.Standard;


namespace Yaya {
	public abstract partial class eCharacter {


		public BuffInt MovementWidth { get; private set; } = new(150);
		public BuffInt MovementHeight { get; private set; } = new(384);
		public BuffInt AntiKnockbackSpeed { get; private set; } = new(16);

		// Walk
		public BuffInt WalkSpeed { get; private set; } = new(20);
		public BuffInt WalkAcceleration { get; private set; } = new(3);
		public BuffInt WalkDecceleration { get; private set; } = new(4);

		// Run
		public BuffInt RunAccumulation { get; private set; } = new(48);
		public BuffInt RunSpeed { get; private set; } = new(32);
		public BuffInt RunAcceleration { get; private set; } = new(3);
		public BuffInt RunDecceleration { get; private set; } = new(4);
		public BuffInt OppositeXAccelerationRate { get; private set; } = new(3000);

		// Jump
		public BuffInt JumpSpeed { get; private set; } = new(62);
		public BuffInt JumpCount { get; private set; } = new(2);
		public BuffInt JumpReleaseLoseRate { get; private set; } = new(700);
		public BuffInt JumpRiseGravityRate { get; private set; } = new(600);
		public BuffBool JumpWithRoll { get; private set; } = new(false);
		public BuffBool JumpSecondWithRoll { get; private set; } = new(true);
		public BuffBool JumpThoughOneway { get; private set; } = new(false);

		// Dash
		public BuffBool DashAvailable { get; private set; } = new(true);
		public BuffInt DashSpeed { get; private set; } = new(42);
		public BuffInt DashDuration { get; private set; } = new(12);
		public BuffInt DashCooldown { get; private set; } = new(4);
		public BuffInt DashAcceleration { get; private set; } = new(24);
		public BuffInt DashCancelLoseRate { get; private set; } = new(300);

		// Squat
		public BuffBool SquatAvailable { get; private set; } = new(true);
		public BuffInt SquatHeight { get; private set; } = new(200);
		public BuffInt SquatSpeed { get; private set; } = new(14);
		public BuffInt SquatAcceleration { get; private set; } = new(48);
		public BuffInt SquatDecceleration { get; private set; } = new(48);

		// Pound
		public BuffBool PoundAvailable { get; private set; } = new(true);
		public BuffInt PoundSpeed { get; private set; } = new(96);

		// Swim
		public BuffInt SwimWidth { get; private set; } = new(256);
		public BuffInt SwimHeight { get; private set; } = new(384);
		public BuffInt InWaterSpeedLoseRate { get; private set; } = new(500);
		public BuffInt SwimSpeed { get; private set; } = new(42);
		public BuffInt SwimAcceleration { get; private set; } = new(4);
		public BuffInt SwimDecceleration { get; private set; } = new(4);
		public BuffBool SwimInFreeStyle { get; private set; } = new(false);

		// Free Swim
		public BuffInt FreeSwimSpeed { get; private set; } = new(40);
		public BuffInt FreeSwimAcceleration { get; private set; } = new(4);
		public BuffInt FreeSwimDecceleration { get; private set; } = new(4);
		public BuffInt FreeSwimDashSpeed { get; private set; } = new(84);
		public BuffInt FreeSwimDashDuration { get; private set; } = new(12);
		public BuffInt FreeSwimDashCooldown { get; private set; } = new(4);
		public BuffInt FreeSwimDashAcceleration { get; private set; } = new(128);

		// Climb
		public BuffBool ClimbAvailable { get; private set; } = new(true);
		public BuffBool JumpWhenClimbAvailable { get; private set; } = new(true);
		public BuffInt ClimbSpeedX { get; private set; } = new(12);
		public BuffInt ClimbSpeedY { get; private set; } = new(18);

		// Fly
		public BuffBool FlyAvailable { get; private set; } = new(false);
		public BuffInt FlyHeight { get; private set; } = new(200);
		public BuffInt FlyCount { get; private set; } = new(1);
		public BuffInt FlyCooldown { get; private set; } = new(24);
		public BuffInt FlySpeed { get; private set; } = new(64);
		public BuffInt FlyGravityRiseRate { get; private set; } = new(800);
		public BuffInt FlyGravityFallRate { get; private set; } = new(200);
		public BuffInt FlyFallSpeed { get; private set; } = new(12);
		public BuffInt FlyGlideSpeed { get; private set; } = new(36);
		public BuffInt FlyGlideAcceleration { get; private set; } = new(2);
		public BuffInt FlyGlideDecceleration { get; private set; } = new(1);

		// Slide
		public BuffBool SlideAvailable { get; private set; } = new(false);
		public BuffBool SlideOnAnyBlock { get; private set; } = new(true);
		public BuffInt SlideDropSpeed { get; private set; } = new(4);
		public BuffInt SlideJumpCountRefill { get; private set; } = new(0);

		// Grab
		public BuffBool GrabTopAvailable { get; private set; } = new(false);
		public BuffBool GrabSideAvailable { get; private set; } = new(false);
		public BuffInt GrabFlipThroughDuration { get; private set; } = new(0);
		public BuffInt GrabMoveSpeedX { get; private set; } = new(24);
		public BuffInt GrabMoveSpeedY { get; private set; } = new(24);
		public BuffInt GrabJumpCountRefill { get; private set; } = new(0);



	}
}