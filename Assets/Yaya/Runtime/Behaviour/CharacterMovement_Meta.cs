using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Moenen.Standard;


namespace Yaya {
	public partial class CharacterMovement : ISerializationCallbackReceiver {



		// Buff
		public BuffInt Width { get; private set; } = new();
		public BuffInt Height { get; private set; } = new();
		public BuffInt SquatHeight { get; private set; } = new();
		public BuffInt SwimWidth { get; private set; } = new();
		public BuffInt SwimHeight { get; private set; } = new();
		public BuffInt FlyHeight { get; private set; } = new();
		public BuffInt AntiKnockbackSpeed { get; private set; } = new();

		public BuffInt WalkSpeed { get; private set; } = new();
		public BuffInt WalkAcceleration { get; private set; } = new();
		public BuffInt WalkDecceleration { get; private set; } = new();

		public BuffInt RunTrigger { get; private set; } = new();
		public BuffInt RunSpeed { get; private set; } = new();
		public BuffInt RunAcceleration { get; private set; } = new();
		public BuffInt RunDecceleration { get; private set; } = new();
		public BuffInt OppositeXAccelerationRate { get; private set; } = new();

		public BuffInt JumpSpeed { get; private set; } = new();
		public BuffInt JumpCount { get; private set; } = new();
		public BuffInt JumpReleaseLoseRate { get; private set; } = new();
		public BuffInt JumpRiseGravityRate { get; private set; } = new();
		public BuffBool JumpRoll { get; private set; } = new();
		public BuffBool JumpSecondRoll { get; private set; } = new();

		public BuffBool DashAvailable { get; private set; } = new();
		public BuffInt DashSpeed { get; private set; } = new();
		public BuffInt DashDuration { get; private set; } = new();
		public BuffInt DashCooldown { get; private set; } = new();
		public BuffInt DashAcceleration { get; private set; } = new();
		public BuffInt DashCancelLoseRate { get; private set; } = new();

		public BuffBool SquatAvailable { get; private set; } = new();
		public BuffInt SquatSpeed { get; private set; } = new();
		public BuffInt SquatAcceleration { get; private set; } = new();
		public BuffInt SquatDecceleration { get; private set; } = new();

		public BuffBool PoundAvailable { get; private set; } = new();
		public BuffInt PoundSpeed { get; private set; } = new();

		public BuffInt InWaterSpeedLoseRate { get; private set; } = new();
		public BuffInt SwimSpeed { get; private set; } = new();
		public BuffInt SwimAcceleration { get; private set; } = new();
		public BuffInt SwimDecceleration { get; private set; } = new();
		public BuffBool SwimInFreeStyle { get; private set; } = new();

		public BuffInt FreeSwimSpeed { get; private set; } = new();
		public BuffInt FreeSwimAcceleration { get; private set; } = new();
		public BuffInt FreeSwimDecceleration { get; private set; } = new();
		public BuffInt FreeSwimDashSpeed { get; private set; } = new();
		public BuffInt FreeSwimDashDuration { get; private set; } = new();
		public BuffInt FreeSwimDashCooldown { get; private set; } = new();
		public BuffInt FreeSwimDashAcceleration { get; private set; } = new();

		public BuffBool ClimbAvailable { get; private set; } = new();
		public BuffBool JumpWhenClimbAvailable { get; private set; } = new();
		public BuffInt ClimbSpeedX { get; private set; } = new();
		public BuffInt ClimbSpeedY { get; private set; } = new();

		public BuffBool FlyAvailable { get; private set; } = new();
		public BuffInt FlyCount { get; private set; } = new();
		public BuffInt FlyCooldown { get; private set; } = new();
		public BuffInt FlySpeed { get; private set; } = new();
		public BuffInt FlyGravityRiseRate { get; private set; } = new();
		public BuffInt FlyGravityFallRate { get; private set; } = new();
		public BuffInt FlyFallSpeed { get; private set; } = new();
		public BuffInt FlyGlideSpeed { get; private set; } = new();
		public BuffInt FlyGlideAcceleration { get; private set; } = new();
		public BuffInt FlyGlideDecceleration { get; private set; } = new();

		// Ser
#pragma warning disable
		[SerializeField] int _Width = 150;
		[SerializeField] int _Height = 384;
		[SerializeField] int _SquatHeight = 200;
		[SerializeField] int _SwimWidth = 256;
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
		[SerializeField] int _FlyCooldown = 24;
		[SerializeField] int _FlySpeed = 64;
		[SerializeField] int _FlyGravityRiseRate = 800;
		[SerializeField] int _FlyGravityFallRate = 200;
		[SerializeField] int _FlyFallSpeed = 12;
		[SerializeField] int _FlyGlideSpeed = 36;
		[SerializeField] int _FlyGlideAcceleration = 2;
		[SerializeField] int _FlyGlideDecceleration = 1;

#pragma warning restore


		public void OnAfterDeserialize () => BuffValue.DeserializeBuffValues(this);
		public void OnBeforeSerialize () => BuffValue.SerializeBuffValues(this);


	}
}