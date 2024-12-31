using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA.Platformer;

[EntityAttribute.Layer(EntityLayer.ENVIRONMENT)]
public abstract class Launcher : Entity, IBlockEntity, ICircuitOperator {




	#region --- VAR ---


	// API
	public virtual Int2 LaunchOffset => default;
	public virtual Int2 LaunchVelocity => default;
	public virtual bool LaunchWhenTriggeredByCircuit => true;
	public virtual int TargetEntityID => TargetEntityIdFromMap;
	public virtual int MaxLaunchCount => int.MaxValue;
	public virtual int LaunchFrequency => 120;
	public virtual bool AllowingAutoLaunch => true;
	public virtual bool LaunchOverlapingElement => true;
	public int LastLaunchedFrame { get; private set; }

	// Data
	private int TargetEntityIdFromMap;
	private int LaunchedCount;


	#endregion




	#region --- MSG ---


	public override void OnActivated () {
		base.OnActivated();
		if (LaunchOverlapingElement) {
			int id = WorldSquad.Front.GetBlockAt((X + 1).ToUnit(), (Y + 1).ToUnit(), BlockType.Element);
			TargetEntityIdFromMap = Stage.IsValidEntityID(id) ? id : 0;
		} else {
			TargetEntityIdFromMap = 0;
		}
		LaunchedCount = 0;
		LastLaunchedFrame = int.MinValue;
	}


	public override void Update () {
		base.Update();
		if (
			AllowingAutoLaunch &&
			LaunchFrequency > 0 &&
			Game.GlobalFrame % LaunchFrequency == LaunchFrequency - 1
		) {
			LaunchEntity();
		}
	}


	void ICircuitOperator.TriggerCircuit () {
		if (!LaunchWhenTriggeredByCircuit) return;
		LaunchEntity();
	}


	protected virtual void OnEntityLaunched (Entity entity) { }


	#endregion




	#region --- API ---


	public Entity LaunchEntity () {
		if (LaunchedCount >= MaxLaunchCount) return null;
		if (TargetEntityID == 0) return null;
		if (Stage.SpawnEntity(TargetEntityID, X + LaunchOffset.x, Y + LaunchOffset.y) is not Entity entity) return null;
		if (entity is Rigidbody rig) {
			rig.VelocityX = LaunchVelocity.x;
			rig.VelocityY = LaunchVelocity.y;
		}
		OnEntityLaunched(entity);
		LaunchedCount++;
		LastLaunchedFrame = Game.GlobalFrame;
		return entity;
	}


	#endregion




	#region --- LGC ---





	#endregion




}
