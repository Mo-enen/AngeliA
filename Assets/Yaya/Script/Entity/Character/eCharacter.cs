using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	[MapEditorGroup("Character")]
	public abstract class eCharacter : eRigidbody {




		#region --- VAR ---


		// Api
		public override int PhysicsLayer => YayaConst.CHARACTER;
		public override int CollisionMask => YayaConst.MASK_SOLID;
		public override bool CarryRigidbodyOnTop => false;
		public override bool IsInAir => base.IsInAir && !Movement.IsClimbing;
		public override int AirDragX => 0;
		public override int AirDragY => 0;
		public override bool IgnoreRiseGravityShift => true;
		public CharacterMovement Movement { get; private set; }
		public CharacterRenderer Renderer { get; private set; }


		#endregion




		#region --- MSG ---


		public override void OnInitialize (Game game) {

			base.OnInitialize(game);
			string typeName = GetType().Name;
			if (typeName.StartsWith("e")) typeName = typeName[1..];

			// Movement
			var movement = game.LoadMeta<CharacterMovement>($"{typeName}.Movement");
			Movement = movement ?? new CharacterMovement();
			Movement.Init(this);

			// Renderer
			var renderer = game.LoadMeta<CharacterRenderer>($"{typeName}.Renderer");
			Renderer = renderer ?? new CharacterRenderer();
			Renderer.Init(this);

		}


		public override void PhysicsUpdate () {
			if (Movement != null) Movement.PhysicsUpdate();
			base.PhysicsUpdate();
		}


		public override void FrameUpdate () {
			if (Renderer != null) Renderer.FrameUpdate();
			base.FrameUpdate();
		}


		#endregion




		#region --- OVR ---


		protected override bool InsideGroundCheck () => CellPhysics.Overlap(YayaConst.MASK_LEVEL, new(X, Y + Height / 4, 1, 1), this);


		#endregion




	}
}
