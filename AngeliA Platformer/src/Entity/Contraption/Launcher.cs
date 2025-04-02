using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA.Platformer;

[EntityAttribute.Layer(EntityLayer.ENVIRONMENT)]
[EntityAttribute.MapEditorGroup("Contraption")]
public abstract class Launcher : Entity, IBlockEntity {




	#region --- VAR ---


	// API
	public virtual Int2 LaunchOffset => new(Width / 2, 0);
	public virtual Int2 LaunchVelocity => default;
	public virtual int TargetEntityID => _TargetEntityId;
	public virtual int FailbackEntityID => 0;
	public virtual int MaxLaunchCount => 32;
	public virtual int LaunchFrequency => 120;
	public virtual int ItemCountPreLaunch => 1;
	public virtual bool AllowingAutoLaunch => true;
	public virtual bool LaunchOverlappingElement => true;
	public virtual bool LaunchWhenEntranceBlocked => false;
	public virtual bool KeepLaunchedEntityInMap => false;
	public virtual bool LaunchTowardsPlayer => false;
	public virtual bool UseMomentum => false;
	bool IBlockEntity.EmbedEntityAsElement => true;
	bool IBlockEntity.AllowBeingEmbedAsElement => false;
	public int LastLaunchedFrame { get; set; }
	public int CurrentLaunchedCount { get; private set; }

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
			for (int i = 0; i < ItemCountPreLaunch; i++) {
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


	protected virtual void OnEntityLaunched (Entity entity, int x, int y) { }


	#endregion




	#region --- API ---


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


	public bool LaunchToRightSide () => LaunchTowardsPlayer ?
		PlayerSystem.Selecting == null || PlayerSystem.Selecting.Rect.CenterX() >= Rect.CenterX() :
		LaunchVelocity.x >= 0;


	#endregion




}
