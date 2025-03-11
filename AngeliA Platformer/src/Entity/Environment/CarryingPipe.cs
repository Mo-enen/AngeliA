using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA.Platformer;

[EntityAttribute.MapEditorGroup("Entity")]
[EntityAttribute.Layer(EntityLayer.ENVIRONMENT)]
public abstract class CarryingPipe : Entity, IBlockEntity {

	// VAR
	protected abstract SpriteCode EdgeSprite { get; }
	protected abstract SpriteCode MidSprite { get; }
	protected abstract SpriteCode InsertSprite { get; }
	protected abstract Direction4 Direction { get; }
	private Direction5? DirL = null;
	private Direction5? DirR = null;
	private Direction5? DirD = null;
	private Direction5? DirU = null;

	// MSG
	public override void OnActivated () {
		base.OnActivated();
		(this as IBlockEntity).OnEntityRefresh();
	}

	void IBlockEntity.OnEntityRefresh () {
		DirL = null;
		DirR = null;
		DirD = null;
		DirU = null;
	}

	public override void FirstUpdate () {
		base.FirstUpdate();
		Physics.FillEntity(PhysicsLayer.ENVIRONMENT, this);
	}

	public override void Update () {
		base.Update();
		TryRefreshAllDirectionCache();

	}

	public override void LateUpdate () {
		base.LateUpdate();

	}

	private void TryRefreshAllDirectionCache () {
		if (!DirL.HasValue) {
			var pipe = Physics.GetEntity<CarryingPipe>(IRect.Point(X - Width / 2, Y + Height / 2), PhysicsMask.ENVIRONMENT, this);
			DirL = pipe != null ? pipe.Direction.ToDirection5() : Direction5.Center;
		}
		if (!DirR.HasValue) {
			var pipe = Physics.GetEntity<CarryingPipe>(IRect.Point(X + Width + Width / 2, Y + Height / 2), PhysicsMask.ENVIRONMENT, this);
			DirR = pipe != null ? pipe.Direction.ToDirection5() : Direction5.Center;
		}
		if (!DirD.HasValue) {
			var pipe = Physics.GetEntity<CarryingPipe>(IRect.Point(X - Width / 2, Y - Height / 2), PhysicsMask.ENVIRONMENT, this);
			DirD = pipe != null ? pipe.Direction.ToDirection5() : Direction5.Center;
		}
		if (!DirU.HasValue) {
			var pipe = Physics.GetEntity<CarryingPipe>(IRect.Point(X - Width / 2, Y + Height + Height / 2), PhysicsMask.ENVIRONMENT, this);
			DirU = pipe != null ? pipe.Direction.ToDirection5() : Direction5.Center;
		}
	}

}
