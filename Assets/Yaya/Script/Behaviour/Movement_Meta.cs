using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Moenen.Standard;


namespace Yaya {
	public partial class Movement : ISerializationCallbackReceiver {



		// Buff
		public BuffInt Width { get; private set; } = new(150);
		public BuffInt Height { get; private set; } = new(384);
		public BuffInt SquatHeight { get; private set; } = new(200);
		public BuffInt SwimHeight { get; private set; } = new(384);
		public BuffInt FlyHeight { get; private set; } = new(200);
		public BuffInt AntiKnockbackSpeed { get; private set; } = new(16);

		public BuffInt WalkSpeed { get; private set; } = new(20);
		public BuffInt WalkAcceleration { get; private set; } = new(3);
		public BuffInt WalkDecceleration { get; private set; } = new(4);

		public BuffInt RunTrigger { get; private set; } = new(60);
		public BuffInt RunSpeed { get; private set; } = new(32);
		public BuffInt RunAcceleration { get; private set; } = new(3);
		public BuffInt RunDecceleration { get; private set; } = new(4);
		public BuffInt OppositeXAccelerationRate { get; private set; } = new(3000);

		public BuffInt JumpSpeed { get; private set; } = new(62);
		public BuffInt JumpCount { get; private set; } = new(2);
		public BuffInt JumpReleaseLoseRate { get; private set; } = new(700);
		public BuffInt JumpRiseGravityRate { get; private set; } = new(600);
		public BuffBool JumpRoll { get; private set; } = new(false);
		public BuffBool JumpSecondRoll { get; private set; } = new(true);

		public BuffBool DashAvailable { get; private set; } = new(true);
		public BuffInt DashSpeed { get; private set; } = new(42);
		public BuffInt DashDuration { get; private set; } = new(12);
		public BuffInt DashCooldown { get; private set; } = new(4);
		public BuffInt DashAcceleration { get; private set; } = new(24);
		public BuffInt DashCancelLoseRate { get; private set; } = new(300);

		public BuffBool SquatAvailable { get; private set; } = new(true);
		public BuffInt SquatSpeed { get; private set; } = new(14);
		public BuffInt SquatAcceleration { get; private set; } = new(48);
		public BuffInt SquatDecceleration { get; private set; } = new(48);

		public BuffBool PoundAvailable { get; private set; } = new(true);
		public BuffInt PoundSpeed { get; private set; } = new(96);

		public BuffInt InWaterSpeedLoseRate { get; private set; } = new(500);
		public BuffInt SwimSpeed { get; private set; } = new(42);
		public BuffInt SwimAcceleration { get; private set; } = new(4);
		public BuffInt SwimDecceleration { get; private set; } = new(4);
		public BuffBool SwimInFreeStyle { get; private set; } = new(false);

		public BuffInt FreeSwimSpeed { get; private set; } = new(40);
		public BuffInt FreeSwimAcceleration { get; private set; } = new(4);
		public BuffInt FreeSwimDecceleration { get; private set; } = new(4);
		public BuffInt FreeSwimDashSpeed { get; private set; } = new(84);
		public BuffInt FreeSwimDashDuration { get; private set; } = new(12);
		public BuffInt FreeSwimDashCooldown { get; private set; } = new(4);
		public BuffInt FreeSwimDashAcceleration { get; private set; } = new(128);

		public BuffBool ClimbAvailable { get; private set; } = new(true);
		public BuffBool JumpWhenClimbAvailable { get; private set; } = new(true);
		public BuffInt ClimbSpeedX { get; private set; } = new(12);
		public BuffInt ClimbSpeedY { get; private set; } = new(18);

		public BuffBool FlyAvailable { get; private set; } = new(false);
		public BuffInt FlyCount { get; private set; } = new(1);
		public BuffInt FlyCooldown { get; private set; } = new(32);
		public BuffInt FlySpeed { get; private set; } = new(64);
		public BuffInt FlyGravityRiseRate { get; private set; } = new(800);
		public BuffInt FlyGravityFallRate { get; private set; } = new(200);
		public BuffInt FlyFallSpeed { get; private set; } = new(12);

		// Ser
#pragma warning disable
		[SerializeField] int _Width = 150;
		[SerializeField] int _Height = 384;
		[SerializeField] int _SquatHeight = 200;
		[SerializeField] int _SwimHeight = 384;
		[SerializeField] int _FlyHeight = 200;
		[SerializeField] int _AntiKnockbackSpeed = 16;

		[SerializeField] int _WalkSpeed = 20;
		[SerializeField] int _WalkAcceleration = 3;
		[SerializeField] int _WalkDecceleration = 4;

		[SerializeField] int _RunTrigger = 60;
		[SerializeField] int _RunSpeed = 32;
		[SerializeField] int _RunAcceleration = 3;
		[SerializeField] int _RunDecceleration = 4;
		[SerializeField] int _OppositeXAccelerationRate = 3000;

		[SerializeField] int _JumpSpeed = 62;
		[SerializeField] int _JumpCount = 2;
		[SerializeField] int _JumpReleaseLoseRate = 700;
		[SerializeField] int _JumpRiseGravityRate = 600;
		[SerializeField] bool _JumpRoll = false;
		[SerializeField] bool _JumpSecondRoll = true;

		[SerializeField] bool _DashAvailable = true;
		[SerializeField] int _DashSpeed = 42;
		[SerializeField] int _DashDuration = 12;
		[SerializeField] int _DashCooldown = 4;
		[SerializeField] int _DashAcceleration = 24;
		[SerializeField] int _DashCancelLoseRate = 300;

		[SerializeField] bool _SquatAvailable = true;
		[SerializeField] int _SquatSpeed = 14;
		[SerializeField] int _SquatAcceleration = 48;
		[SerializeField] int _SquatDecceleration = 48;

		[SerializeField] bool _PoundAvailable = true;
		[SerializeField] int _PoundSpeed = 96;

		[SerializeField] int _InWaterSpeedLoseRate = 500;
		[SerializeField] int _SwimSpeed = 42;
		[SerializeField] int _SwimAcceleration = 4;
		[SerializeField] int _SwimDecceleration = 4;
		[SerializeField] bool _SwimInFreeStyle = false;

		[SerializeField] int _FreeSwimSpeed = 40;
		[SerializeField] int _FreeSwimAcceleration = 4;
		[SerializeField] int _FreeSwimDecceleration = 4;
		[SerializeField] int _FreeSwimDashSpeed = 84;
		[SerializeField] int _FreeSwimDashDuration = 12;
		[SerializeField] int _FreeSwimDashCooldown = 4;
		[SerializeField] int _FreeSwimDashAcceleration = 128;

		[SerializeField] bool _ClimbAvailable = true;
		[SerializeField] bool _JumpWhenClimbAvailable = true;
		[SerializeField] int _ClimbSpeedX = 12;
		[SerializeField] int _ClimbSpeedY = 18;

		[SerializeField] bool _FlyAvailable = false;
		[SerializeField] int _FlyCount = 1;
		[SerializeField] int _FlyCooldown = 32;
		[SerializeField] int _FlySpeed = 64;
		[SerializeField] int _FlyGravityRiseRate = 800;
		[SerializeField] int _FlyGravityFallRate = 200;
		[SerializeField] int _FlyFallSpeed = 12;
#pragma warning restore


		public void OnAfterDeserialize () => BuffValue.DeserializeBuffValues(this);
		public void OnBeforeSerialize () => BuffValue.SerializeBuffValues(this);


	}
}