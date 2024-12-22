using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA.Platformer;


[EntityAttribute.Layer(EntityLayer.ENVIRONMENT)]
public abstract class Button : Entity, IBlockEntity {


	

	#region --- VAR ---


	// Data
	private static readonly Dictionary<int, ButtonOperator> OperatorPool = [];


	#endregion




	#region --- MSG ---



	[OnGameInitialize]
	internal static void OnGameInitialize () {
		OperatorPool.Clear();
		foreach (var type in typeof(ButtonOperator).AllChildClass()) {
			if (System.Activator.CreateInstance(type) is not ButtonOperator oper) continue;
			OperatorPool.Add(type.AngeHash(), oper);
		}
	}


	public override void FirstUpdate () {
		base.FirstUpdate();
		Physics.FillEntity(PhysicsLayer.ENVIRONMENT, this, true, Tag.OnewayUp);
	}


	#endregion




	#region --- API ---


	public virtual void PressButton () {



	}


	#endregion




	#region --- LGC ---



	#endregion




}
