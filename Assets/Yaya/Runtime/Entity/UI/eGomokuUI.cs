using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	[EntityAttribute.Capacity(1, 0)]
	public class eGomokuUI : UIEntity {




		#region --- VAR ---


		private static readonly int CIRCLE_CODE = "Circle16".AngeHash();


		#endregion




		#region --- MSG ---


		public override void OnActived () {
			base.OnActived();



		}


		protected override void FrameUpdateUI () {




		}


		#endregion




		#region --- API ---



		#endregion




		#region --- LGC ---



		#endregion




	}
}