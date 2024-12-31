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
	public virtual int TargetEntityID => TargetEntityIdFromMap;
	public virtual int MaxLaunchCount => 32;
	public virtual int LaunchFrequency => 120;
	public virtual bool AllowingAutoLaunch => true;
	public virtual bool LaunchOverlapingElement => true;
	public virtual bool LaunchWhenEntranceBlocked => false;
	public virtual bool KeepLaunchedEntityInMap => true;
	public virtual bool LaunchWhenTriggeredByCircuit => false;
	public int LastLaunchedFrame { get; private set; }

	// Data
	private AutoValidList<(Entity e, int frame)> LaunchedEntities = null;
	private int TargetEntityIdFromMap;


	#endregion




	#region --- MSG ---


	public override void OnActivated () {
		base.OnActivated();
		LaunchedEntities ??= new(MaxLaunchCount.LessOrEquel(256), ValidFunc);
		if (LaunchOverlapingElement) {
			int id = WorldSquad.Front.GetBlockAt((X + 1).ToUnit(), (Y + 1).ToUnit(), BlockType.Element);
			TargetEntityIdFromMap = Stage.IsValidEntityID(id) ? id : 0;
		} else {
			TargetEntityIdFromMap = 0;
		}
		LastLaunchedFrame = int.MinValue;
		static bool ValidFunc ((Entity e, int frame) pair) => pair.e.Active && pair.e.SpawnFrame == pair.frame;
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


	protected virtual void OnEntityLaunched (Entity entity, int x, int y) { }


	#endregion




	#region --- API ---


	public Entity LaunchEntity () {
		LaunchedEntities.Update();
		if (LaunchedEntities.Count >= LaunchedEntities.Capacity) return null;
		if (TargetEntityID == 0) return null;
		if (!LaunchWhenEntranceBlocked && Physics.Overlap(
			PhysicsMask.ENTITY, Rect, this, OperationMode.ColliderOnly
		)) return null;
		if (Stage.SpawnEntity(TargetEntityID, X + LaunchOffset.x, Y + LaunchOffset.y) is not Entity entity) return null;
		if (!KeepLaunchedEntityInMap) entity.IgnoreReposition = true;
		if (entity is Rigidbody rig) {
			rig.VelocityX = LaunchVelocity.x;
			rig.VelocityY = LaunchVelocity.y;
		}
		OnEntityLaunched(entity, X + LaunchOffset.x, Y + LaunchOffset.y);
		LastLaunchedFrame = Game.GlobalFrame;
		LaunchedEntities.Add((entity, entity.SpawnFrame));
		return entity;
	}


	#endregion




}
