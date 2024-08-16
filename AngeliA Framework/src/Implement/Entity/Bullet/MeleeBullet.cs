using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public class MeleeBullet : Bullet {

	// Const
	public static readonly int TYPE_ID = typeof(MeleeBullet).AngeHash();

	// Api
	protected override int Duration => 10;
	protected override int Damage => 1;
	protected virtual bool RenderBullet => false;
	protected sealed override int SpawnWidth => _SpawnWidth;
	protected sealed override int SpawnHeight => _SpawnHeight;
	protected sealed override bool DestroyOnHitEnvironment => false;
	protected sealed override bool DestroyOnHitReceiver => false;
	public virtual int SmokeParticleID => 0;

	// Data
	private int _SpawnWidth = 0;
	private int _SpawnHeight = 0;
	private bool FacingRight = true;

	// MSG
	public override void OnActivated () {
		_SpawnWidth = 0;
		_SpawnHeight = 0;
		Width = 0;
		Height = 0;
		base.OnActivated();
	}

	public override void Update () {
		FollowSender();
		base.Update();
	}

	public override void LateUpdate () {
		base.LateUpdate();
		if (RenderBullet) {
			DrawBullet(this, TypeID, FacingRight, 0, 1000);
		}
	}

	// API
	public void FollowSender () {
		if (Sender is not Character character) return;
		var characterRect = character.Rect;
		X = character.Movement.FacingRight ? characterRect.xMax : characterRect.xMin - Width;
		Y = character.Y - 1;
		FacingRight = character.Movement.FacingRight;
	}

	public void SetSpawnSize (int width, int height) {
		Width = _SpawnWidth = width;
		Height = _SpawnHeight = height;
	}

}