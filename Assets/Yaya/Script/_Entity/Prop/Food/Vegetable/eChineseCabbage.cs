using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AngeliaFramework;
namespace Yaya {
	public class eChineseCabbage : eItem {
		private static readonly int CODE = "Chinese Cabbage".AngeHash();
		protected override int ItemCode => CODE;



	}
}
