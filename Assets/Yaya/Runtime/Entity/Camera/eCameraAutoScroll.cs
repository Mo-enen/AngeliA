using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {

	public class eCameraAutoScrollLeft : eCameraAutoScroll {
		protected override Direction3 DirectionX => Direction3.Left;
		protected override Direction3 DirectionY => Direction3.None;
	}
	public class eCameraAutoScrollRight : eCameraAutoScroll {
		protected override Direction3 DirectionX => Direction3.Right;
		protected override Direction3 DirectionY => Direction3.None;
	}
	public class eCameraAutoScrollBottom : eCameraAutoScroll {
		protected override Direction3 DirectionX => Direction3.None;
		protected override Direction3 DirectionY => Direction3.Down;
	}
	public class eCameraAutoScrollTop : eCameraAutoScroll {
		protected override Direction3 DirectionX => Direction3.None;
		protected override Direction3 DirectionY => Direction3.Up;
	}

	public class eCameraAutoScrollBottomLeft : eCameraAutoScroll {
		protected override Direction3 DirectionX => Direction3.Left;
		protected override Direction3 DirectionY => Direction3.Down;
	}
	public class eCameraAutoScrollBottomRight : eCameraAutoScroll {
		protected override Direction3 DirectionX => Direction3.Right;
		protected override Direction3 DirectionY => Direction3.Down;
	}
	public class eCameraAutoScrollTopLeft : eCameraAutoScroll {
		protected override Direction3 DirectionX => Direction3.Left;
		protected override Direction3 DirectionY => Direction3.Up;
	}
	public class eCameraAutoScrollTopRight : eCameraAutoScroll {
		protected override Direction3 DirectionX => Direction3.Right;
		protected override Direction3 DirectionY => Direction3.Up;
	}

	[EntityAttribute.ForceUpdate]
	[EntityAttribute.Capacity(16)]
	[EntityAttribute.MapEditorGroup("Camera")]
	public abstract class eCameraAutoScroll : Entity {




		#region --- VAR ---


		protected abstract Direction3 DirectionX { get; }
		protected abstract Direction3 DirectionY { get; }
		protected virtual int Speed => 24;


		#endregion




		#region --- MSG ---



		#endregion




		#region --- API ---



		#endregion




		#region --- LGC ---



		#endregion




	}
}
