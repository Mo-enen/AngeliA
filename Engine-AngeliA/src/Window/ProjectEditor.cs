using System.Collections.Generic;
using System.Collections;
using AngeliA;

namespace AngeliaEngine;

public class ProjectEditor : WindowUI {




	#region --- VAR ---


	protected override bool BlockEvent => true;


	#endregion




	#region --- MSG ---


	public override void BeforeUpdate () {
		base.BeforeUpdate();
		Cursor.RequireCursor();
	}


	public override void UpdateWindowUI () {




	}


	#endregion




	#region --- API ---



	#endregion




	#region --- LGC ---



	#endregion




}