using System.Collections;
using System.Collections.Generic;


using AngeliA;
namespace AngeliA.Platformer;


[EntityAttribute.MapEditorGroup("Vegetation")]
[EntityAttribute.Capacity(1024)]
[EntityAttribute.Layer(EntityLayer.ENVIRONMENT)]
public abstract class Leaf : Entity, IBlockEntity, ICombustible, IDamageReceiver {




	#region --- VAR ---


	// Const
	private static readonly int[] LEAF_OFFSET_SEEDS = [0, 6, 2, 8, 3, 7, 2, 3, 5, 2, 2, 6, 9, 3, 6, 1, 9, 0, 1, 7, 4, 2, 8, 4, 6, 5, 2, 4, 8, 7,];
	private const byte LEAF_HIDE_ALPHA = 42;


	// Virtual
	protected virtual int LeafCount => 3;
	protected virtual int LeafExpand => Const.CEL / 3;
	protected int LeafArtworkCode { get; private set; } = 0;
	int ICombustible.BurnStartFrame { get; set; }
	int IDamageReceiver.Team => Const.TEAM_ENVIRONMENT;

	// Data
	private Color32 LeafTint = new(255, 255, 255, 255);
	private bool CharacterNearby = false;


	#endregion




	#region --- MSG ---


	public override void OnActivated () {
		base.OnActivated();
		Width = Const.CEL;
		Height = Const.CEL;
		// Leaf
		LeafArtworkCode = Renderer.TryGetSpriteFromGroup(
			TypeID, (X * 5 + Y * 7) / Const.CEL, out var lSprite
		) ? lSprite.ID : TypeID;
	}


	public override void FirstUpdate () => Physics.FillEntity(PhysicsLayer.ENVIRONMENT, this, true);


	public override void Update () {
		base.Update();
		CharacterNearby = Physics.HasEntity<Character>(Rect.Expand(Const.CEL), PhysicsMask.CHARACTER, null);
	}


	public override void LateUpdate () {
		base.LateUpdate();
		// Leaf
		LeafTint.a = (byte)Util.Lerp(LeafTint.a, CharacterNearby ? LEAF_HIDE_ALPHA : 255, 0.1f);
		int sLen = LEAF_OFFSET_SEEDS.Length;
		for (int i = 0; i < LeafCount; i++) {
			int seedX = LEAF_OFFSET_SEEDS[(i + X / Const.CEL).UMod(sLen)];
			int seedY = LEAF_OFFSET_SEEDS[(i + Y / Const.CEL).UMod(sLen)];
			var offset = new Int2(
				((X * 137 * seedX + Y * 327 * seedY) / Const.CEL).UMod(Const.CEL) - Const.HALF,
				((X * 149 * seedX + Y * 177 * seedY) / Const.CEL).UMod(Const.CEL) - Const.HALF
			);
			DrawLeaf(offset, 12 * i, LeafExpand, offset.x % 2 == 0);
		}
		// Func
		void DrawLeaf (Int2 offset, int frameOffset, int expand, bool flipX = false) {
			var rect = Rect.Shift(offset.x, GetLeafShiftY(frameOffset) + offset.y).Expand(expand);
			if (flipX) {
				rect.x += rect.width;
				rect.width = -rect.width;
			}
			Renderer.Draw(LeafArtworkCode, rect, LeafTint);
		}
	}


	protected virtual void OnLeafBreak () {

		// Item
		ItemSystem.DropItemFor(this);

		// Block
		if (Universe.BuiltInInfo.AllowPlayerModifyMap) {
			FrameworkUtil.PickEntityBlock(this, false);
		} else {
			FrameworkUtil.RemoveFromWorldMemory(this);
		}
	}


	#endregion




	#region --- API ---


	protected int GetLeafShiftY (int frameOffset = 0, int duration = 60, int amount = 12) {
		return ((Game.GlobalFrame + (X / Const.CEL) + frameOffset).PingPong(duration) * amount / duration) - (amount / 2);
	}


	void IDamageReceiver.TakeDamage (Damage damage) {
		if (damage.Amount <= 0) return;
		Active = false;
		OnLeafBreak();
	}


	#endregion




}