namespace AngeliA;

public sealed class DefaultPlayer : Character {
	public static readonly int TYPE_ID = typeof(DefaultPlayer).AngeHash();
	protected override CharacterRenderer CreateNativeRenderer () => new PoseCharacterRenderer(this);
	protected override CharacterMovement CreateNativeMovement () => new PoseCharacterMovement(this);
	public override bool AllowInvoke () => !Health.IsInvincible;
}
