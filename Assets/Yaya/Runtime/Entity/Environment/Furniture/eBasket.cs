using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;

namespace Yaya {
	public class eBasket : eFurniture {


		public static readonly int TYPE_ID = typeof(eBasket).AngeHash();
		protected override RectInt RenderingRect => base.RenderingRect.Expand(32, 32, 0, 0);



	}
}
