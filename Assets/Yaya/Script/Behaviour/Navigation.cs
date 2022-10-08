using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;
using Moenen.Standard;


namespace Yaya {
	public class Navigation : ISerializationCallbackReceiver {




		#region --- VAR ---


		// Api
		public eCharacter Source { get; private set; } = null;

		// Buff


#pragma warning disable
		// Ser

#pragma warning restore

		// Data
		private int TargetX = 0;
		private int TargetY = 0;


		#endregion




		#region --- MSG ---


		public void OnBeforeSerialize () => BuffValue.SerializeBuffValues(this);
		public void OnAfterDeserialize () => BuffValue.DeserializeBuffValues(this);


		#endregion




		#region --- API ---


		public void OnActived (eCharacter source) {
			Source = source;
			TargetX = 0;
			TargetY = 0;
		}


		public void SetTargetPosition (int x, int y) {
			TargetX = x;
			TargetY = y;


		}


		public void Navigate () {






		}


		#endregion




		#region --- LGC ---



		#endregion




	}
}
