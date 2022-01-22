using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework.Entities {
	public partial class CharacterMovement {


		public int Width { get; init; } = 166;
		public int Height { get; init; } = 188;
		public int Gravity { get; init; } = 6;
		public int MaxGravitySpeed { get; init; } = 90;

		// Move
		public int MoveSpeed { get; init; } = 17;
		public int MoveAcceleration { get; init; } = 6;
		public int MoveDecceleration { get; init; } = 4;
		public bool MoveWalkOnWater { get; init; } = false;

		// Jump
		public int JumpSpeed { get; init; } = 60;
		public int JumpCount { get; init; } = 2;
		public int JumpReleaseLoseRate { get; init; } = 700;
		public int JumpRaiseGravity { get; init; } = 3;

		// Dash
		public bool DashAvailable { get; init; } = true;
		public int DashSpeed { get; init; } = 42;
		public int DashDuration { get; init; } = 12;
		public int DashCooldown { get; init; } = 4;
		public int DashAcceleration { get; init; } = 24;

		// Squat
		public bool SquatAvailable { get; init; } = true;
		public int SquatSpeed { get; init; } = 8;
		public int SquatAcceleration { get; init; } = 48;
		public int SquatDecceleration { get; init; } = 48;
		public int SquatHeightRate { get; init; } = 618;

		// Pound
		public bool PoundAvailable { get; init; } = true;
		public int PoundSpeed { get; init; } = 100;


	}
}