using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaFramework {


	[EntityAttribute.Bounds(-Const.CEL * 5 / 2, 0, Const.CEL * 6, Const.CEL)]
	public class WoodPlatformH : WoodPlatform {
		protected override Vector2Int Distance => new(Const.CEL * 5, 0);
		protected override uint SpeedX => 8;
	}


	[EntityAttribute.Bounds(0, -Const.CEL * 5 / 2, Const.CEL, Const.CEL * 6)]
	public class WoodPlatformV : WoodPlatform {
		protected override Vector2Int Distance => new(0, Const.CEL * 5);
		protected override uint SpeedY => 8;
	}


	public abstract class WoodPlatform : PingPongPlatform, ICombustible {
		public override bool OneWay => true;
		int ICombustible.BurnStartFrame { get; set; }

	}


	public class SnakePlatformWood : SnakePlatform, ICombustible {
		public override int EndBreakDuration => 120;
		public override int Speed => 12;
		public override bool OneWay => true;
		int ICombustible.BurnStartFrame { get; set; }
		public int BurnedDuration => 320;
	}


	public class SnakePlatformIron : SnakePlatform {
		public override int EndBreakDuration => 120;
		public override int Speed => 24;
		public override bool OneWay => true;
	}


}