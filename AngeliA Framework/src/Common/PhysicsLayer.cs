namespace AngeliA;


/// <summary>
/// A single physics layer
/// </summary>
public static class PhysicsLayer {
	public const int LEVEL = 0;
	public const int ENVIRONMENT = 1;
	public const int ITEM = 2;
	public const int CHARACTER = 3;
	public const int DAMAGE = 4;
	public const int COUNT = 5;
}


/// <summary>
/// A group of physics layera
/// </summary>
public static class PhysicsMask {

	public const int NONE = 0;
	public const int ALL = 0b11111;

	public const int LEVEL = 0b1;
	public const int ENVIRONMENT = 0b10;
	public const int ITEM = 0b100;
	public const int CHARACTER = 0b1000;
	/// <summary>
	/// Colliders inside damage layer will deal damage when overlape with IDamageReceiver
	/// </summary>
	public const int DAMAGE = 0b10000;

	/// <summary>
	/// ENVIRONMENT | CHARACTER
	/// </summary>
	public const int ENTITY = ENVIRONMENT | CHARACTER;
	/// <summary>
	/// ENVIRONMENT | ITEM | CHARACTER
	/// </summary>
	public const int DYNAMIC = ENVIRONMENT | ITEM | CHARACTER;
	/// <summary>
	/// LEVEL | ENVIRONMENT | CHARACTER
	/// </summary>
	public const int SOLID = LEVEL | ENVIRONMENT | CHARACTER;
	/// <summary>
	/// LEVEL | ENVIRONMENT
	/// </summary>
	public const int MAP = LEVEL | ENVIRONMENT;
}