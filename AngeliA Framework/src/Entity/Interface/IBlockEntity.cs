using System.Collections.Generic;
using System.Collections;

namespace AngeliA;

/// <summary>
/// Interface that makes the entity behave like a block from map. This will make the entity become a block item for ItemSystem.
/// </summary>
public interface IBlockEntity {

	// VAR
	private static readonly PhysicsCell[] BlockOperationCache = new PhysicsCell[32];
	/// <summary>
	/// Max item stack count as a block item.
	/// </summary>
	public int MaxStackCount => 256;
	/// <summary>
	/// True if this entity can embed other entity as a element in map (like putting a coin into a launcher in SMM2 and this entity would be the launcher)
	/// </summary>
	public bool EmbedEntityAsElement => false;
	/// <summary>
	/// True if this entity can be embed as a element in map (like putting a coin into a launcher in SMM2 and this entity would be the coin)
	/// </summary>
	public bool AllowBeingEmbedAsElement => true;
	private static readonly HashSet<int> IgnoreEmbedAsElement = [];

	// MSG
	[OnGameInitialize]
	internal static void OnGameInitialize () {
		IgnoreEmbedAsElement.Clear();
		foreach (var type in typeof(IBlockEntity).AllClassImplemented()) {
			if (System.Activator.CreateInstance(type) is not Entity e || e is not IBlockEntity bEntity) continue;
			if (!bEntity.AllowBeingEmbedAsElement) {
				IgnoreEmbedAsElement.Add(e.TypeID);
			}
		}
	}
	/// <summary>
	/// This function is called when this entity being picked as a block by a pick-tool
	/// </summary>
	public void OnEntityPicked () { }
	/// <summary>
	/// This function is called when this entity being put into the map as a block
	/// </summary>
	public void OnEntityPut () { }
	/// <summary>
	/// This function is called when there are map changes happens nearby and the entity needs to refresh itself
	/// </summary>
	public void OnEntityRefresh () { }

	/// <summary>
	/// Refresh all block entity instances near the given position.
	/// </summary>
	/// <param name="centerUnitPos">This position is in unit space</param>
	/// <param name="ignore">Do not refresh this entity</param>
	public static void RefreshBlockEntitiesNearby (Int2 centerUnitPos, Entity ignore = null) {
		int nearByCount = Physics.OverlapAll(
			BlockOperationCache, PhysicsMask.MAP,
			new IRect(centerUnitPos.x.ToGlobal(), centerUnitPos.y.ToGlobal(), Const.CEL, Const.CEL).Expand(Const.CEL - 1),
			ignore, OperationMode.ColliderAndTrigger
		);
		for (int j = 0; j < nearByCount; j++) {
			var nearByHit = BlockOperationCache[j];
			if (nearByHit.Entity is not IBlockEntity nearByEntity) continue;
			nearByEntity.OnEntityRefresh();
		}
	}
	/// <summary>
	/// True if the given type of entity do not take other entity as embed element
	/// </summary>
	public static bool IsIgnoreEmbedAsElement (int blockEntityID) => IgnoreEmbedAsElement.Contains(blockEntityID);

}
