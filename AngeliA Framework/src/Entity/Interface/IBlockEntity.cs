using System.Collections.Generic;
using System.Collections;

namespace AngeliA;

public interface IBlockEntity {

	// VAR
	private static readonly PhysicsCell[] BlockOperationCache = new PhysicsCell[32];
	public int MaxStackCount => 256;
	public bool EmbedEntityAsElement => false;
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
	public void OnEntityPicked () { }
	public void OnEntityPut () { }
	public void OnEntityRefresh () { }

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
	public static bool IsIgnoreEmbedAsElement (int blockEntityID) => IgnoreEmbedAsElement.Contains(blockEntityID);

}
