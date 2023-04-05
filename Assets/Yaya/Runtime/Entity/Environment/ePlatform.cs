using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {


	[EntityAttribute.Bounds(-Const.CEL * 5 / 2, 0, Const.CEL * 6, Const.CEL)]
	public class eWoodPlatformH : eWoodPlatform {
		protected override Vector2Int Distance => new(Const.CEL * 5, 0);
		protected override uint SpeedX => 8;
	}


	[EntityAttribute.Bounds(0, -Const.CEL * 5 / 2, Const.CEL, Const.CEL * 6)]
	public class eWoodPlatformV : eWoodPlatform {
		protected override Vector2Int Distance => new(0, Const.CEL * 5);
		protected override uint SpeedY => 8;
	}


	public abstract class eWoodPlatform : PingPongPlatform {

		private static readonly int ARTCODE_LEFT = "WoodPlatform Left".AngeHash();
		private static readonly int ARTCODE_MID = "WoodPlatform Mid".AngeHash();
		private static readonly int ARTCODE_RIGHT = "WoodPlatform Right".AngeHash();
		private static readonly int ARTCODE_SINGLE = "WoodPlatform Single".AngeHash();

		protected override int ArtworkCode_Left => ARTCODE_LEFT;
		protected override int ArtworkCode_Mid => ARTCODE_MID;
		protected override int ArtworkCode_Right => ARTCODE_RIGHT;
		protected override int ArtworkCode_Single => ARTCODE_SINGLE;

		public override bool OneWay => true;

	}


	public class eSnakePlatformSlow : SnakePlatform {
		public override int EndBreakDuration => 120;
		public override int Speed => 12;
		public override bool OneWay => true;
	}


	public class eSnakePlatformQuick : SnakePlatform {
		public override int EndBreakDuration => 120;
		public override int Speed => 24;
		public override bool OneWay => true;
	}


}