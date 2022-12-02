using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	[EntityAttribute.ExcludeInMapEditor]
	[EntityAttribute.Capacity(1)]
	[EntityAttribute.ForceUpdate]
	[EntityAttribute.DontDestroyOnSquadTransition]
	[EntityAttribute.DontDestroyOutOfRange]
	public partial class eMapEditor : Entity {




		#region --- VAR ---


		// Api
		public static eMapEditor Current { get; private set; } = null;


		#endregion




		#region --- MSG ---


		public override void OnInitialize () {
			base.OnInitialize();
			Current = this;
		}


		public override void OnActived () {
			base.OnActived();

		}


		public override void PhysicsUpdate () {
			base.PhysicsUpdate();

		}


		public override void FrameUpdate () {
			base.FrameUpdate();

		}


		#endregion




		#region --- LGC ---



		#endregion




	}
}