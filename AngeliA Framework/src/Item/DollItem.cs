namespace AngeliA;

public abstract class DollItem : Item {
	public override int MaxStackCount => 1;
	public abstract int DollSize { get; }
	public override void OnPoseAnimationUpdate_FromInventory (PoseCharacterRenderer rendering, int inventoryID, int itemIndex) {
		base.OnPoseAnimationUpdate_FromInventory(rendering, inventoryID, itemIndex);
		if (!Renderer.TryGetSprite(TypeID, out var sprite)) return;
		int dollCount = Inventory.ItemTotalCount<DollItem>(inventoryID, itemIndex, out int itemOrder, ignoreStack: true);
		if (dollCount == 0) return;
		dollCount += 2;
		var hip = rendering.Hip;
		float pos01 = (itemOrder + 1f) / dollCount;
		float gapX = DollSize / 2f / hip.Width.Abs();
		pos01 = Util.LerpUnclamped(0f - gapX, 1f + gapX, pos01);
		var character = rendering.TargetCharacter;
		var pos = hip.GlobalLerp(pos01, 0.5f, true);
		int rot = character.DeltaPositionX.Clamp(-30, 30) / 2;
		Renderer.Draw(
			sprite,
			pos.x + (character.Movement.FacingRight ? hip.Width.Abs() / 6 : 0), pos.y,
			500, 618, rot,
			DollSize, hip.Height.Sign() * DollSize,
			z: hip.FrontSide ? hip.Z + 8 : hip.Z - 8
		);
	}
}
