using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA.Platformer;

[EntityAttribute.Layer(EntityLayer.ENVIRONMENT)]
[EntityAttribute.MapEditorGroup("Contraption")]
public abstract class Launcher : Entity, IBlockEntity {




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
	public virtual bool LaunchTowardsPlayer => true;
	bool IBlockEntity.ContainEntityAsElement => true;
	public int LastLaunchedFrame { get; set; }

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


	protected virtual void OnEntityLaunched (Entity entity, int x, int y) { }


	#endregion




	#region --- API ---


	public Entity LaunchEntity () {
		LaunchedEntities.Update();

		if (TargetEntityID == 0) return null;
		if (LaunchedEntities.Count >= LaunchedEntities.Capacity) return null;
		bool rightSide = LaunchToRightSide();
		var offset = LaunchOffset;
		if (!rightSide) offset.x = -offset.x;

		// Blocked Check
		if (!LaunchWhenEntranceBlocked && Physics.Overlap(
			PhysicsMask.ENTITY, Rect.Shift(offset).Shrink(1), this, OperationMode.ColliderAndTrigger
		)) return null;

		// Spawn Entity
		Entity entity = null;
		if (Stage.IsValidEntityID(TargetEntityID)) {
			entity = Stage.SpawnEntity(TargetEntityID, X + offset.x, Y + offset.y);
		} else if (ItemSystem.HasItem(TargetEntityID)) {
			entity = ItemSystem.SpawnItem(TargetEntityID, X + offset.x, Y + offset.y);
		}
		if (entity == null) return null;

		if (!KeepLaunchedEntityInMap) entity.IgnoreReposition = true;

		// Rigidbody Movement
		if (entity is Rigidbody rig) {
			var vel = LaunchVelocity;
			if (
				vel.x != 0 &&
				LaunchTowardsPlayer &&
				!rightSide
			) {
				vel.x = -vel.x;
			}
			rig.VelocityX = vel.x;
			rig.VelocityY = vel.y;
		}

		// Finish
		OnEntityLaunched(entity, X + offset.x, Y + offset.y);
		LastLaunchedFrame = Game.GlobalFrame;
		LaunchedEntities.Add((entity, entity.SpawnFrame));
		return entity;
	}


	public bool LaunchToRightSide () => PlayerSystem.Selecting == null || PlayerSystem.Selecting.Rect.CenterX() >= Rect.CenterX();


	#endregion




}
