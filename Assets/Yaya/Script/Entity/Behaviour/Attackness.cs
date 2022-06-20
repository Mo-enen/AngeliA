using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class Attackness : EntityBehaviour<Entity>, ITxtMeta {




		#region --- VAR ---


		// Ser



		#endregion




		#region --- MSG ---


		public override void Initialize (Entity source) {
			base.Initialize(source);


		}


		public override void Update () {
			base.Update();


		}


		#endregion




		#region --- API ---


		// Meta
		public void LoadFromText (string text) => BuffValue.LoadBuffMetaFromText(this, text);


		public string SaveToText () => BuffValue.SaveBuffMetaToText(this);


		// Attackness
		public bool Attack () {
			// IAttackReceiver




			return true;
		}


		#endregion




		#region --- LGC ---




		#endregion



	}
}
