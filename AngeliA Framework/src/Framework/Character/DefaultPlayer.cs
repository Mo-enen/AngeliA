namespace AngeliA;

public sealed class DefaultPlayer : PoseCharacter {
	public static readonly int TYPE_ID = typeof(DefaultPlayer).AngeHash();
	public override bool AllowInvoke () => !Health.IsInvincible;
}
