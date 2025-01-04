using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA.Platformer;

[EntityAttribute.Layer(EntityLayer.ENVIRONMENT)]
[EntityAttribute.MapEditorGroup("Launcher")]
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
	public virtual bool KeepLaunchedEntityInMap => false;
	public virtual bool LaunchWhenTriggeredByCircuit => false;
	public virtual bool LaunchTowardsPlayer => true;
	bool IBlockEntity.ContainEntityAsElement => true;
	public int LastLaunchedFrame { get; private set; }

	// Data
	private AutoValidList<(Entity e, int frame)> LaunchedEntities = null;
	private int TargetEntityIdFromMap;


	#endregion




	#region --- MSG ---


	public override void OnActivated () {
		base.OnActivated();
		LaunchedEntities ??= new(MaxLaunchCount.LessOrEquel(256), ValidFunc);
		LastLaunchedFrame = int.MinValue;
		OnEntityRefresh();
		static bool ValidFunc ((Entity e, int frame) pair) => pair.e.Active && pair.e.SpawnFrame == pair.frame;
	}


	public void OnEntityRefresh () {
		if (LaunchOverlapingElement) {
			int id = WorldSquad.Front.GetBlockAt((X + 1).ToUnit(), (Y + 1).ToUnit(), BlockType.Element);
			TargetEntityIdFromMap = Stage.IsValidEntityID(id) || ItemSystem.HasItem(id) ? id : 0;
		} else {
			TargetEntityIdFromMap = 0;
		}
	}


	public override void FirstUpdate () {
		base.FirstUpdate();
		Physics.FillEntity(PhysicsLayer.ENVIRONMENT, this, true);
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


	public override void LateUpdate () {
		base.LateUpdate();
		// Entity Icon
		if (
			TargetEntityID != 0 &&
			Renderer.TryGetSpriteForGizmos(TargetEntityID, out var iconSp)
		) {
			var tool = FrameworkUtil.GetPlayerHoldingHandTool();
			if (tool is BlockBuilder || tool is PickTool) {
				using var _ = new UILayerScope();
				Renderer.Draw(
					iconSp,
					X + Width / 2, Y + Height / 2,
					500, 500, Util.QuickRandom(-6, 7),
					Width * 3 / 4, Height * 3 / 4
				);
			}
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
			PhysicsMask.ENTITY, Rect, this, OperationMode.ColliderAndTrigger
		)) return null;
		Entity entity = null;
		if (Stage.IsValidEntityID(TargetEntityID)) {
			entity = Stage.SpawnEntity(TargetEntityID, X + LaunchOffset.x, Y + LaunchOffset.y);
		} else if (ItemSystem.HasItem(TargetEntityID)) {
			entity = ItemSystem.SpawnItem(TargetEntityID, X + LaunchOffset.x, Y + LaunchOffset.y);
		}
		if (entity == null) return null;
		if (!KeepLaunchedEntityInMap) entity.IgnoreReposition = true;
		if (entity is Rigidbody rig) {
			var vel = LaunchVelocity;
			if (
				vel.x != 0 &&
				LaunchTowardsPlayer &&
				PlayerSystem.Selecting != null &&
				PlayerSystem.Selecting.Rect.CenterX() < Rect.CenterX()
			) {
				vel.x = -vel.x;
			}
			rig.VelocityX = vel.x;
			rig.VelocityY = vel.y;
		}
		OnEntityLaunched(entity, X + LaunchOffset.x, Y + LaunchOffset.y);
		LastLaunchedFrame = Game.GlobalFrame;
		LaunchedEntities.Add((entity, entity.SpawnFrame));
		return entity;
	}


	#endregion




}
