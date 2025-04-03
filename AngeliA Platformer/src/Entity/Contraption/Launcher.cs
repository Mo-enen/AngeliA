using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA.Platformer;

/// <summary>
/// Entity that spawn a given target entity repeately
/// </summary>
[EntityAttribute.Layer(EntityLayer.ENVIRONMENT)]
[EntityAttribute.MapEditorGroup("Contraption")]
public abstract class Launcher : Entity, IBlockEntity {




	#region --- VAR ---


	// API
	/// <summary>
	/// Starting position offset in global space for the launched entity
	/// </summary>
	public virtual Int2 LaunchOffset => new(Width / 2, 0);
	/// <summary>
	/// Starting velocity for the launched entity
	/// </summary>
	public virtual Int2 LaunchVelocity => default;
	/// <summary>
	/// Launching entity type ID
	/// </summary>
	public virtual int TargetEntityID => _TargetEntityId;
	/// <summary>
	/// Launching entity with this type ID when the "TargetEntityID" is invalid
	/// </summary>
	public virtual int FailbackEntityID => 0;
	/// <summary>
	/// How many entities can it launch every time after the launcher spawned on stage
	/// </summary>
	public virtual int MaxLaunchCount => 32;
	/// <summary>
	/// How many frames does it takes to launch another entity
	/// </summary>
	public virtual int LaunchFrequency => 120;
	/// <summary>
	/// How many entity does it launch at once
	/// </summary>
	public virtual int ItemCountPerLaunch => 1;
	/// <summary>
	/// True if the launcher perform launch every "LaunchFrequency" frames
	/// </summary>
	public virtual bool AllowingAutoLaunch => true;
	/// <summary>
	/// True if launcher search entity target ID from overlapping map element
	/// </summary>
	public virtual bool LaunchOverlappingElement => true;
	/// <summary>
	/// True if launcher can launch when the starting position is blocked by other entity
	/// </summary>
	public virtual bool LaunchWhenEntranceBlocked => false;
	/// <summary>
	/// True if the launched entities can reposition and save into the map
	/// </summary>
	public virtual bool KeepLaunchedEntityInMap => false;
	/// <summary>
	/// True if the launcher always launch towards player horizontal location
	/// </summary>
	public virtual bool LaunchTowardsPlayer => false;
	/// <summary>
	/// True if the entity is move by monentum instead of velocity
	/// </summary>
	public virtual bool UseMomentum => false;
	bool IBlockEntity.EmbedEntityAsElement => true;
	bool IBlockEntity.AllowBeingEmbedAsElement => false;
	protected int LastLaunchedFrame { get; set; }
	protected int CurrentLaunchedCount { get; private set; }

	// Data
	private int _TargetEntityId;


	#endregion




	#region --- MSG ---


	public override void OnActivated () {
		base.OnActivated();
		LastLaunchedFrame = int.MinValue;
		CurrentLaunchedCount = 0;
		OnEntityRefresh();
	}


	public void OnEntityRefresh () {
		if (LaunchOverlappingElement) {
			var pivotPos = PivotUnitPosition;
			int id = WorldSquad.Front.GetBlockAt(pivotPos.x, pivotPos.y, pivotPos.z, BlockType.Element);
			_TargetEntityId = Stage.IsValidEntityID(id) || ItemSystem.HasItem(id) ? id : FailbackEntityID;
		} else {
			_TargetEntityId = FailbackEntityID;
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
			Game.SettleFrame % LaunchFrequency == LaunchFrequency / 2 &&
			CurrentLaunchedCount < MaxLaunchCount
		) {
			for (int i = 0; i < ItemCountPerLaunch; i++) {
				LaunchEntity();
			}
		}
	}


	public override void LateUpdate () {
		base.LateUpdate();
		// Entity Icon
		if (
			!TaskSystem.HasTask() &&
			Renderer.TryGetSpriteForGizmos(
				TargetEntityID != 0 ? TargetEntityID : BuiltInSprite.FRAME_HOLLOW_16,
				out var iconSp
			)
		) {
			var tool = FrameworkUtil.GetPlayerHoldingHandTool();
			if (tool is BlockBuilder || tool is BlockPicker) {
				using var _ = new UILayerScope();
				Renderer.Draw(
					iconSp,
					X + Width / 2, Y + Height / 2,
					500, 500, 0,
					Const.CEL, Const.CEL,
					Color32.WHITE.WithNewA(Game.GlobalFrame.PingPong(0, 60) * 4)
				);
			}
		}
	}


	/// <summary>
	/// This function is called when an entity is launched
	/// </summary>
	protected virtual void OnEntityLaunched (Entity entity, int x, int y) { }


	#endregion




	#region --- API ---


	/// <summary>
	/// True if the launcher is currently able to launch
	/// </summary>
	public bool ValidForLaunch () {

		if (TargetEntityID == 0) return false;
		bool rightSide = LaunchToRightSide();
		var offset = LaunchOffset;
		if (!rightSide) offset.x = -offset.x;

		// Blocked Check
		if (!LaunchWhenEntranceBlocked && (Physics.Overlap(
			PhysicsMask.SOLID, Rect.Shift(offset), this
		) || Physics.GetEntity(
			TargetEntityID, Rect.Shift(offset), PhysicsMask.ALL, this, OperationMode.ColliderAndTrigger
		) != null)) return false;

		return Stage.IsValidEntityID(TargetEntityID) || ItemSystem.HasItem(TargetEntityID);
	}


	/// <summary>
	/// Perform launch for once
	/// </summary>
	public Entity LaunchEntity () {

		if (!ValidForLaunch()) return null;

		bool rightSide = LaunchToRightSide();
		var offset = LaunchOffset;
		int launchX = rightSide ? Rect.CenterX() + offset.x : Rect.CenterX() - offset.x;
		int launchY = Rect.CenterY() + offset.y;

		// Spawn Entity
		Entity entity = null;
		if (Stage.IsValidEntityID(TargetEntityID)) {
			entity = Stage.SpawnEntity(TargetEntityID, launchX, launchY);
		} else if (ItemSystem.HasItem(TargetEntityID)) {
			entity = ItemSystem.SpawnItem(TargetEntityID, launchX, launchY);
		}

		if (entity == null) return null;

		// Fix Launched Entity Position
		entity.X += launchX - (rightSide ? entity.Rect.xMin : entity.Rect.xMax);
		entity.Y += launchY - entity.Rect.CenterY();

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
			if (UseMomentum) {
				rig.MomentumX = (vel.x, 1);
				rig.MomentumY = (vel.y, 1);
			} else {
				rig.VelocityX = vel.x;
				rig.VelocityY = vel.y;
			}
			rig.X -= rig.OffsetX;
		}

		if (!KeepLaunchedEntityInMap) {
			entity.IgnoreReposition = true;
		}

		// Pingpong Walker
		if (entity is IPingPongWalker walker) {
			walker.WalkingRight = rightSide;
		}

		// Block Entity
		if (entity is IBlockEntity) {
			IBlockEntity.RefreshBlockEntitiesNearby(entity.Center.ToUnit(), entity);
		}

		// Finish
		OnEntityLaunched(entity, launchX, launchY);
		if (LastLaunchedFrame != Game.GlobalFrame) {
			LastLaunchedFrame = Game.GlobalFrame;
			CurrentLaunchedCount++;
		}
		return entity;
	}


	/// <summary>
	/// True if the launcher should launch to right currently
	/// </summary>
	public bool LaunchToRightSide () => LaunchTowardsPlayer ?
		PlayerSystem.Selecting == null || PlayerSystem.Selecting.Rect.CenterX() >= Rect.CenterX() :
		LaunchVelocity.x >= 0;


	#endregion




}
