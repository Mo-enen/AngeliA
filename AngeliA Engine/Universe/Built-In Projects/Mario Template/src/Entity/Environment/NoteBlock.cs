using System.Collections;
using System.Collections.Generic;
using AngeliA;
using AngeliA.Platformer;

namespace MarioTemplate;

[NoItemCombination]
[EntityAttribute.Layer(EntityLayer.ENVIRONMENT)]
public class NoteBlock : Entity, IBumpable, IBlockEntity {

	// VAR
	bool IBumpable.FromAbove => true;
	bool IBumpable.FromBelow => true;
	public int LastBumpedFrame { get; set; } = int.MinValue;
	public Direction4 LastBumpFrom { get; set; }
	bool IBlockEntity.EmbedEntityAsElement => true;
	private int ItemInside;
	private int TargetY;

	// MSG
	public override void OnActivated () {
		base.OnActivated();
		ItemInside = MarioUtil.GetEmbedItemID(Rect);
		TargetY = Y;
	}

	public override void FirstUpdate () {
		base.FirstUpdate();
		Physics.FillEntity(PhysicsLayer.ENVIRONMENT, this);
	}

	public override void Update () {
		base.Update();
		// Bounce Animation
		Y = Y.LerpTo(TargetY, 0.5f);
	}

	public override void LateUpdate () {
		base.LateUpdate();
		Draw();
	}

	void IBumpable.OnBumped (Rigidbody rig) {

		// Bounce
		FrameworkUtil.PerformSpringBounce(this, LastBumpFrom, 64);
		if (LastBumpFrom == Direction4.Down) {
			Y += Const.CEL;
		} else {
			Y -= Const.CEL;
		}

		// Spawn Item
		if (ItemInside != 0) {
			if (LastBumpFrom == Direction4.Down) {
				// From Below
				// Spawn Item Inside
				if (MarioUtil.SpawnEmbedItem(ItemInside, Rect, Direction4.Up) is Rigidbody rItem) {
					rItem.VelocityY = 64;
				}
				ItemInside = 0;
			} else {
				// From Above
				// Spawn Item Inside
				if (MarioUtil.SpawnEmbedItem(ItemInside, Rect, Direction4.Down) is Rigidbody rItem) {
					rItem.VelocityY = -32;
				}
				ItemInside = 0;
			}
		}
	}

	bool IBumpable.AllowBump (Rigidbody rig) => true;

}
