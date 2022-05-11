using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	[MapEditorGroup("Character")]
	public abstract class eCharacter : eRigidbody {




		#region --- VAR ---


		// Api
		public override int CollisionLayer => YayaConst.CHARACTER;
		public override bool CarryRigidbodyOnTop => false;
		public override bool IsInAir => base.IsInAir && !Movement.IsClimbing;
		public override int AirDragX => 0;
		public override int AirDragY => 0;
		public CharacterMovement Movement { get; private set; }
		public CharacterRenderer Renderer { get; private set; }
		protected virtual System.Type MovementType => typeof(CharacterMovement);
		protected virtual System.Type RendererType => typeof(CharacterRenderer);


		#endregion




		#region --- MSG ---


		public override void OnInitialize (Game game) {

			base.OnInitialize(game);
			string typeName = GetType().Name;
			if (typeName.StartsWith("e")) typeName = typeName[1..];

			// Movement
			var movement = game.LoadJsonConfig($"{typeName}.Movement", MovementType);
			Movement = movement != null ? movement as CharacterMovement : new CharacterMovement();
			Movement.Init(this);

			// Renderer
			var renderer = game.LoadJsonConfig($"{typeName}.Renderer", RendererType);
			Renderer = renderer != null ? renderer as CharacterRenderer : new CharacterRenderer();
			Renderer.Init(this);

		}


		public override void PhysicsUpdate (int frame) {
			if (Movement != null) Movement.PhysicsUpdate(frame);
			base.PhysicsUpdate(frame);
		}


		public override void FrameUpdate (int frame) {
			if (Renderer != null) Renderer.FrameUpdate(frame);
			base.FrameUpdate(frame);
		}


		#endregion




		#region --- OVR ---


		protected override bool InsideGroundCheck () => CellPhysics.Overlap((int)PhysicsMask.Level, new(X, Y + Height / 4, 1, 1), this);


		#endregion




	}
}
