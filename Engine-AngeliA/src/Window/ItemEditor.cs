using System.Collections.Generic;
using System.Collections;
using AngeliA;

namespace AngeliaEngine;

public class ItemEditor : WindowUI {




	#region --- VAR ---


	public override string DefaultName => "Item";
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