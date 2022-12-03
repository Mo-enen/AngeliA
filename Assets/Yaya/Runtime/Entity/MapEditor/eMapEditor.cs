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


		public override void OnInactived () {
			base.OnInactived();

		}


		public override void FrameUpdate () {
			base.FrameUpdate();
			Update_View();
		}


		#endregion




		#region --- API ---


		public static void StartEdit () {
			if (Current.Active) return;
			Game.Current.AddEntity<eMapEditor>(0, 0);
			Game.Current.ReloadAllEntitiesFromWorld();
		}


		public static void StopEdit () {
			if (!Current.Active) return;
			Current.Active = false;
			Game.Current.ReloadAllEntitiesFromWorld();
		}


		#endregion




		#region --- LGC ---



		#endregion




	}
}