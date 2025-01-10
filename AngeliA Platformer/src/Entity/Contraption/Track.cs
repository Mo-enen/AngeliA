using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA.Platformer;

[EntityAttribute.MapEditorGroup("Contraption")]
public sealed class Track : Entity, IBlockEntity {




	#region --- VAR ---


	public static readonly int TYPE_ID = typeof(Track).AngeHash();


	#endregion




	#region --- MSG ---


	public override void OnActivated () {
		base.OnActivated();
		OnEntityRefresh();
	}


	public void OnEntityRefresh () {

	}


	public override void BeforeUpdate () {
		base.BeforeUpdate();
		
	}
	

	public override void LateUpdate () {
		base.LateUpdate();


	}


	#endregion




	#region --- API ---



	#endregion




	#region --- LGC ---



	#endregion




}
