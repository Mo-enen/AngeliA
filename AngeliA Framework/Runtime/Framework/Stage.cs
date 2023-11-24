using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;


namespace AngeliaFramework {
	public static class Stage {




		#region --- SUB ---


		private class EntityComparer : IComparer<Entity> {
			public static readonly EntityComparer Instance = new();
			public int Compare (Entity a, Entity b) {
				int result = b.Active.CompareTo(a.Active);
				if (result != 0) return result;
				result = a.Order.CompareTo(b.Order);
				if (result != 0) return result;
				result = a.TypeID.CompareTo(b.TypeID);
				if (result != 0) return result;
				result = a.SpawnFrame.CompareTo(b.SpawnFrame);
				if (result != 0) return result;
				result = a.X.CompareTo(b.X);
				if (result != 0) return result;
				result = a.Y.CompareTo(b.Y);
				if (result != 0) return result;
				result = a.InstanceOrder.CompareTo(b.InstanceOrder);
				return result;
			}
		}


		private class EntityStack {

			// Api
			public int SpawnedCount => InstanceCount - Entities.Count;
			public Stack<Entity> Entities = null;
			public System.Type EntityType = null;
			public RectInt LocalBound = default;
			public bool DrawBehind = false;
			public bool DestroyOnSquadTransition = true;
			public bool DontSpawnFromWorld = false;
			public bool ForceSpawn = false;
			public int Capacity = 0;
			public bool DespawnOutOfRange = true;
			public bool UpdateOutOfRange = false;
			public int Order = 0;
			public int Layer = 0;
			private int InstanceCount = 0;

			// API
			public Entity Peek () => Entities.Count > 0 ? Entities.Peek() : null;

			public Entity Pop () {
				if (Entities.Count > 0) return Entities.Pop();
				if (InstanceCount < Capacity) return CreateInstance();
				return null;
			}

			public void Push (Entity e) => Entities.Push(e);

			public Entity CreateInstance () {
				if (System.Activator.CreateInstance(EntityType) is not Entity e) return null;
				e.Active = false;
				e.DestroyOnSquadTransition = DestroyOnSquadTransition;
				e.DespawnOutOfRange = DespawnOutOfRange;
				e.LocalBounds = LocalBound;
				e.UpdateOutOfRange = UpdateOutOfRange;
				e.Order = Order;
				InstanceCount++;
				return e;
			}

		}


		#endregion




		#region --- VAR ---


		// Const
		private static readonly int[] ENTITY_CAPACITY = new int[Const.ENTITY_LAYER_COUNT] { 4096, 512, 1024, 1024, 128, 64, };

		// Api
		public static int[] EntityCounts { get; private set; } = new int[Const.ENTITY_LAYER_COUNT];
		public static Entity[][] Entities { get; private set; } = null;
		public static RectInt SpawnRect { get; private set; } = default;
		public static RectInt AntiSpawnRect { get; private set; } = default;
		public static int LastSettleFrame { get; private set; } = 0;
		public static int ViewZ { get; private set; } = 0;
		public static RectInt ViewRect { get; private set; } = default;
		public static int? DelayingViewX => ViewDelayX.value;
		public static int? DelayingViewY => ViewDelayY.value;
		public static int? DelayingViewHeight => ViewDelayHeight.value;

		// Event
		public static event System.Action OnViewZChanged;

		// Data
		private static (int? value, int priority) ViewDelayX = (null, int.MinValue);
		private static (int? value, int priority) ViewDelayY = (null, int.MinValue);
		private static (int? value, int priority) ViewDelayHeight = (null, int.MinValue);
		private static int ViewLerpRate = 1000;
		private static int? RequireSetViewZ = null;
		private static int GlobalFrame = 0;
		private static readonly Dictionary<int, EntityStack> EntityPool = new();
		private static readonly HashSet<Vector3Int> StagedEntityHash = new();
		private static readonly HashSet<Vector3Int> GlobalAntiSpawnHash = new();
		private static readonly HashSet<Vector3Int> LocalAntiSpawnHash = new();


		#endregion




		#region --- MSG ---


		[OnGameInitialize(-64)]
		public static void BeforeGameInitializeLater () {
			ViewRect = new(
				0, 0,
				Const.VIEW_RATIO * Mathf.Clamp(Const.DEFAULT_HEIGHT, Const.MIN_HEIGHT, Const.MAX_HEIGHT) / 1000,
				Mathf.Clamp(Const.DEFAULT_HEIGHT, Const.MIN_HEIGHT, Const.MAX_HEIGHT)
			);
			Entities = new Entity[Const.ENTITY_LAYER_COUNT][];
			for (int i = 0; i < Const.ENTITY_LAYER_COUNT; i++) {
				Entities[i] = new Entity[ENTITY_CAPACITY[i]];
			}
			EntityPool.Clear();
			var allEntityTypes = new List<System.Type>(typeof(Entity).AllChildClass());
			for (int tIndex = 0; tIndex < allEntityTypes.Count; tIndex++) {

				var eType = allEntityTypes[tIndex];

				int id = eType.AngeHash();
				int preSpawn = 0;
				int capacity = 64;

				var att_Layer = eType.GetCustomAttribute<EntityAttribute.LayerAttribute>(true);
				var att_Capacity = eType.GetCustomAttribute<EntityAttribute.CapacityAttribute>(true);
				var att_Bound = eType.GetCustomAttribute<EntityAttribute.BoundsAttribute>(true);
				var att_DontDespawn = eType.GetCustomAttribute<EntityAttribute.DontDestroyOutOfRangeAttribute>(true);
				var att_ForceUpdate = eType.GetCustomAttribute<EntityAttribute.UpdateOutOfRangeAttribute>(true);
				var att_DontDrawBehind = eType.GetCustomAttribute<EntityAttribute.DontDrawBehindAttribute>(true);
				var att_DontDestroyOnTran = eType.GetCustomAttribute<EntityAttribute.DontDestroyOnSquadTransitionAttribute>(true);
				var att_DontSpawnFromWorld = eType.GetCustomAttribute<EntityAttribute.DontSpawnFromWorld>(true);
				var att_ForceSpawn = eType.GetCustomAttribute<EntityAttribute.ForceSpawnAttribute>(true);
				var att_Order = eType.GetCustomAttribute<EntityAttribute.StageOrderAttribute>(true);
				int layer = att_Layer != null ? att_Layer.Layer.Clamp(0, Const.ENTITY_LAYER_COUNT - 1) : 0;
				if (att_Capacity != null) {
					capacity = att_Capacity.Value.Clamp(1, Entities[layer].Length);
					preSpawn = att_Capacity.PreSpawn.Clamp(0, Entities[layer].Length);
				}

				var stack = new EntityStack() {
					Entities = new Stack<Entity>(capacity),
					LocalBound = att_Bound != null ? att_Bound.Value : new(0, 0, Const.CEL, Const.CEL),
					DrawBehind = att_DontDrawBehind == null,
					DestroyOnSquadTransition = att_DontDestroyOnTran == null,
					ForceSpawn = att_ForceSpawn != null,
					Capacity = capacity,
					EntityType = eType,
					DespawnOutOfRange = att_DontDespawn == null,
					UpdateOutOfRange = att_ForceUpdate != null,
					DontSpawnFromWorld = att_DontSpawnFromWorld != null,
					Order = att_Order != null ? att_Order.Order : 0,
					Layer = layer,
				};
				for (int i = 0; i < preSpawn; i++) {
					try {
						var e = stack.CreateInstance();
						if (e == null) break;
						stack.Entities.Push(e);
					} catch (System.Exception ex) { Debug.LogException(ex); }
				}
				EntityPool.TryAdd(id, stack);
			}
			// Event
			Util.LinkEventWithAttribute<OnViewZChangedAttribute>(typeof(Stage), nameof(OnViewZChanged));
		}


		internal static void Update_View () {

			// Move View Rect
			if (ViewDelayX.value.HasValue || ViewDelayY.value.HasValue || ViewDelayHeight.value.HasValue) {
				int targetHeight = (ViewDelayHeight.value ?? ViewRect.height).Clamp(Const.MIN_HEIGHT, Const.MAX_HEIGHT);
				var viewRectDelay = new RectInt(
					ViewDelayX.value ?? ViewRect.x,
					ViewDelayY.value ?? ViewRect.y,
					Const.VIEW_RATIO * targetHeight / 1000,
					targetHeight
				);
				if (ViewLerpRate >= 1000) {
					ViewRect = viewRectDelay;
					ViewDelayX.value = null;
					ViewDelayY.value = null;
					ViewDelayHeight.value = null;
				} else {
					ViewRect = new(
						ViewRect.x.LerpTo(viewRectDelay.x, ViewLerpRate),
						ViewRect.y.LerpTo(viewRectDelay.y, ViewLerpRate),
						ViewRect.width.LerpTo(viewRectDelay.width, ViewLerpRate),
						ViewRect.height.LerpTo(viewRectDelay.height, ViewLerpRate)
					);
					if (ViewRect.IsSame(viewRectDelay)) {
						ViewDelayX.value = null;
						ViewDelayY.value = null;
						ViewDelayHeight.value = null;
					}
				}
			}
			ViewDelayX.priority = int.MinValue;
			ViewDelayY.priority = int.MinValue;
			ViewDelayHeight.priority = int.MinValue;

			// Rects
			SpawnRect = ViewRect.Expand(Const.SPAWN_PADDING);

		}


		internal static void FrameUpdate (int globalFrame, int targetLayer = -1) {

			GlobalFrame = globalFrame;

			int startLayer = 0;
			int endLayer = Const.ENTITY_LAYER_COUNT;
			if (targetLayer >= 0) {
				startLayer = targetLayer;
				endLayer = targetLayer + 1;
			}

			// Remove Inactive and Outside SpawnRect
			for (int layer = startLayer; layer < endLayer; layer++) {
				RefreshStagedEntities(layer);
			}

			// Fill Physics
			for (int layer = startLayer; layer < endLayer; layer++) {
				var entities = Entities[layer];
				int count = EntityCounts[layer];
				count = count.Clamp(0, entities.Length);
				for (int index = 0; index < count; index++) {
					try {
						entities[index].FillPhysics();
					} catch (System.Exception ex) { Debug.LogException(ex); }
				}
			}

			// Before Physics Update
			for (int layer = startLayer; layer < endLayer; layer++) {
				var entities = Entities[layer];
				int count = EntityCounts[layer];
				count = count.Clamp(0, entities.Length);
				for (int index = 0; index < count; index++) {
					var e = entities[index];
					if (e.UpdateOutOfRange || e.FrameUpdated || ViewRect.Overlaps(e.GlobalBounds)) {
						try {
							e.BeforePhysicsUpdate();
						} catch (System.Exception ex) { Debug.LogException(ex); }
					}
				}
			}

			// Physics Update
			for (int layer = startLayer; layer < endLayer; layer++) {
				var entities = Entities[layer];
				int count = EntityCounts[layer];
				count = count.Clamp(0, entities.Length);
				for (int index = 0; index < count; index++) {
					var e = entities[index];
					if (e.UpdateOutOfRange || e.FrameUpdated || ViewRect.Overlaps(e.GlobalBounds)) {
						try {
							e.PhysicsUpdate();
						} catch (System.Exception ex) { Debug.LogException(ex); }
					}
				}
			}

			// FrameUpdate
			var cullCameraRect = CellRenderer.CameraRect.Expand(
				CellRenderer.CameraShaking || FrameTask.IsTasking<TeleportTask>() ?
				new Vector4Int(Const.CEL * 4, Const.CEL * 4, Const.CEL * 4, Const.CEL * 4) : Vector4Int.zero
			);
			for (int layer = startLayer; layer < endLayer; layer++) {
				var entities = Entities[layer];
				int count = EntityCounts[layer];
				count = count.Clamp(0, entities.Length);
				for (int index = 0; index < count; index++) {
					var e = entities[index];
					if (e.UpdateOutOfRange || cullCameraRect.Overlaps(e.GlobalBounds)) {
						try {
							CellRenderer.SetLayerToDefault();
							CellRenderer.SetTextLayer(0);
							e.FrameUpdate();
							e.FrameUpdated = true;
						} catch (System.Exception ex) { Debug.LogException(ex); }
					}
				}
			}

			// Final
			AntiSpawnRect = ViewRect.Expand(Const.ANTI_SPAWN_PADDING);
		}


		[OnGameUpdate]
		public static void OnGameUpdate () {
			// Z Change
			if (RequireSetViewZ.HasValue) {
				int newZ = RequireSetViewZ.Value;
				RequireSetViewZ = null;
				LastSettleFrame = GlobalFrame;
				ClearStagedEntities();
				if (newZ != ViewZ) {
					ViewZ = newZ;
					LocalAntiSpawnHash.Clear();
				}
				OnViewZChanged?.Invoke();
			}
		}


		#endregion




		#region --- API ---


		public static void SetViewZ (int newZ) => RequireSetViewZ = newZ;


		// View Position
		public static void SetViewPositionDelay (int x, int y, int lerp = 1000, int priority = int.MinValue) {
			if (priority >= ViewDelayX.priority) ViewDelayX = (x, priority);
			if (priority >= ViewDelayY.priority) ViewDelayY = (y, priority);
			ViewLerpRate = lerp;
		}


		public static void SetViewXDelay (int x, int lerp = 1000, int priority = int.MinValue) {
			if (priority >= ViewDelayX.priority) ViewDelayX = (x, priority);
			ViewLerpRate = lerp;
		}


		public static void SetViewYDelay (int y, int lerp = 1000, int priority = int.MinValue) {
			if (priority >= ViewDelayY.priority) ViewDelayY = (y, priority);
			ViewLerpRate = lerp;
		}


		// View Size
		public static void SetViewSizeDelay (int height, int lerp = 1000, int priority = int.MinValue) {
			if (priority >= ViewDelayHeight.priority) ViewDelayHeight = (height, priority);
			ViewLerpRate = lerp;
		}


		// Spawn
		public static Entity SpawnEntity (int typeID, int x, int y) => SpawnEntityLogic(typeID, x, y, new Vector3Int(int.MinValue, 0, 0));
		public static T SpawnEntity<T> (int x, int y) where T : Entity => SpawnEntityLogic(
			typeof(T).AngeHash(), x, y, new(int.MinValue, 0)
		) as T;
		public static bool TrySpawnEntity (int typeID, int x, int y, out Entity entity) {
			entity = SpawnEntityLogic(typeID, x, y, new(int.MinValue, 0));
			return entity != null;
		}
		public static bool TrySpawnEntity<T> (int x, int y, out T entity) where T : Entity {
			entity = SpawnEntityLogic(
				typeof(T).AngeHash(), x, y, new(int.MinValue, 0)
			) as T;
			return entity != null;
		}


		public static Entity SpawnEntityFromWorld (int typeID, int unitX, int unitY, int unitZ) {
			var uPos = new Vector3Int(unitX, unitY, unitZ);
			if (StagedEntityHash.Contains(uPos)) return null;
			if (GlobalAntiSpawnHash.Contains(uPos)) return null;
			if (LocalAntiSpawnHash.Contains(uPos)) return null;
			if (!EntityPool.TryGetValue(typeID, out var stack)) return null;
			if (stack.DontSpawnFromWorld) return null;
			int x = unitX * Const.CEL;
			int y = unitY * Const.CEL;
			if (AntiSpawnRect.Overlaps(stack.LocalBound.Shift(x, y))) return null;
			return SpawnEntityLogic(typeID, x, y, uPos);
		}


		// Anti Spawn
		public static void MarkAsGlobalAntiSpawn (Entity entity) {
			if (!entity.FromWorld) return;
			GlobalAntiSpawnHash.TryAdd(entity.InstanceID);
		}
		public static void ClearGlobalAntiSpawn () {
			if (GlobalAntiSpawnHash.Count > 0) {
				GlobalAntiSpawnHash.Clear();
			}
		}
		public static void MarkAsLocalAntiSpawn (Entity entity) {
			if (!entity.FromWorld) return;
			LocalAntiSpawnHash.TryAdd(entity.InstanceID);
		}
		public static void ClearLocalAntiSpawn () {
			if (LocalAntiSpawnHash.Count > 0) {
				LocalAntiSpawnHash.Clear();
			}
		}


		// Get
		public static T GetEntity<T> () where T : Entity => TryGetEntity<T>(out var result) ? result : null;
		public static Entity GetEntity (int typeID) => TryGetEntity(typeID, out var result) ? result : null;


		public static bool TryGetEntityLayer (int entityID, out int layer) {
			if (EntityPool.TryGetValue(entityID, out var stack)) {
				layer = stack.Layer;
				return true;
			}
			layer = -1;
			return false;
		}


		public static bool TryGetEntity<E> (out E result) where E : Entity {
			result = null;
			for (int layer = 0; layer < Const.ENTITY_LAYER_COUNT; layer++) {
				int count = EntityCounts[layer];
				var entities = Entities[layer];
				for (int i = 0; i < count; i++) {
					var e = entities[i];
					if (e is E) {
						result = e as E;
						return true;
					}
				}
			}
			return false;
		}
		public static bool TryGetEntity (int typeID, out Entity result) {
			result = null;
			if (!TryGetEntityLayer(typeID, out int layer)) return false;
			int count = EntityCounts[layer];
			var entities = Entities[layer];
			for (int i = 0; i < count; i++) {
				var e = entities[i];
				if (e.TypeID == typeID) {
					result = e;
					return true;
				}
			}
			result = null;
			return false;
		}
		public static bool TryGetEntityNearby<E> (Vector2Int pos, out E finalTarget) where E : Entity {
			finalTarget = null;
			int finalDistance = int.MaxValue;
			for (int layer = 0; layer < Const.ENTITY_LAYER_COUNT; layer++) {
				int count = EntityCounts[layer];
				var entities = Entities[layer];
				for (int i = 0; i < count; i++) {
					var e = entities[i];
					if (e is not E target) continue;
					if (finalTarget == null) {
						finalTarget = target;
						finalDistance = Util.SquareDistance(target.Rect.position, pos);
					} else {
						int dis = Util.SquareDistance(target.Rect.position, pos);
						if (dis < finalDistance) {
							finalDistance = dis;
							finalTarget = target;
						}
					}
				}
			}
			return finalTarget != null;
		}


		public static bool TryGetEntities (int layer, out Entity[] entities, out int count) {
			if (layer >= 0 && layer < Const.ENTITY_LAYER_COUNT) {
				count = EntityCounts[layer];
				entities = Entities[layer];
				return true;
			} else {
				count = 0;
				entities = null;
				return false;
			}
		}


		// Composite
		public static Entity PeekOrGetEntity (int typeID) => PeekEntity(typeID) ?? GetEntity(typeID);
		public static T PeekOrGetEntity<T> () where T : Entity => PeekEntity<T>() ?? GetEntity<T>();
		public static Entity GetOrAddEntity (int typeID, int x, int y) {
			if (TryGetEntity(typeID, out var entity)) {
				entity.X = x;
				entity.Y = y;
				return entity;
			} else {
				return SpawnEntity(typeID, x, y);
			}
		}
		public static T GetOrAddEntity<T> (int x, int y) where T : Entity {
			if (TryGetEntity<T>(out var entity)) {
				entity.X = x;
				entity.Y = y;
				return entity;
			} else {
				return SpawnEntity<T>(x, y);
			}
		}


		// Misc
		public static bool RequireDrawEntityBehind (int id, int unitX, int unitY, int unitZ) =>
			EntityPool.TryGetValue(id, out var stack) &&
			stack.DrawBehind &&
			!ItemSystem.HasItem(id) &&
			!GlobalAntiSpawnHash.Contains(new(unitX, unitY, unitZ));


		public static int GetSpawnedEntityCount (int id) => EntityPool.TryGetValue(id, out var meta) ? meta.SpawnedCount : 0;


		public static void ClearStagedEntities () {
			for (int layer = 0; layer < Const.ENTITY_LAYER_COUNT; layer++) {
				var entities = Entities[layer];
				int count = EntityCounts[layer];
				for (int i = 0; i < count; i++) {
					var e = entities[i];
					if (e.DespawnOutOfRange && e.DestroyOnSquadTransition) {
						e.Active = false;
					}
				}
				RefreshStagedEntities(layer);
			}
			AntiSpawnRect = default;
		}


		#endregion




		#region --- LGC ---


		private static void RefreshStagedEntities (int layer) {

			var entities = Entities[layer];
			int count = EntityCounts[layer];

			// Inactive Out of Range Entities
			for (int i = 0; i < count; i++) {
				var entity = entities[i];
				if (entity.Active && entity.DespawnOutOfRange && !SpawnRect.Overlaps(entity.GlobalBounds)) {
					entity.Active = false;
				}
			}

			// Sort
			Util.QuickSort(entities, 0, count - 1, EntityComparer.Instance);

			// Remove Inactive
			while (count > 0) {
				var e = entities[count - 1];
				if (e.Active) break;
				PushEntityBack(e);
				count--;
			}

			EntityCounts[layer] = count;
		}


		private static Entity SpawnEntityLogic (int typeID, int x, int y, Vector3Int globalUnitPos) {
#if UNITY_EDITOR
			if (!UnityEditor.EditorApplication.isPlaying) return null;
#endif
			try {
				if (
					globalUnitPos.x != int.MinValue &&
					StagedEntityHash.Contains(globalUnitPos)
				) return null;
				if (!EntityPool.TryGetValue(typeID, out var eMeta)) {
#if UNITY_EDITOR
					if (typeID != 0 && !EntityPool.ContainsKey(typeID)) Debug.LogWarning($"Invalid Entity Type ID {typeID}");
#endif
					return null;
				}
				if (!EntityPool.TryGetValue(typeID, out var stack)) return null;
				int layer = stack.Layer;
				int count = EntityCounts[layer];
				var entities = Entities[layer];
				Entity entity = null;
				if (count >= entities.Length) {
					if (!eMeta.ForceSpawn) return null;
					// Force Spawn
					for (int i = 0; i < count; i++) {
						var e = entities[i];
						if (e.TypeID != typeID && (!EntityPool.TryGetValue(e.TypeID, out var _meta) || _meta.ForceSpawn)) continue;
						// Pop
						entity = eMeta.Pop();
						if (entity == null) break;
						entities[i] = entity;
						// Inactive First NoneForceSpawn Entity
						PushEntityBack(e);
						goto KeepOn;
					}
					return null;
					KeepOn:;
				}
				// Normal Spawn
				if (entity == null) {
					entity = eMeta.Pop();
					if (entity == null) return null;
					entities[count] = entity;
					count++;
					EntityCounts[layer] = count;
				}
				// Init Entity
				if (entity != null) {
					if (globalUnitPos.x != int.MinValue) {
						StagedEntityHash.Add(globalUnitPos);
					} else {
						globalUnitPos.y = stack.SpawnedCount;
					}
					entity.InstanceID = globalUnitPos;
					entity.X = x;
					entity.Y = y;
					entity.Width = Const.CEL;
					entity.Height = Const.CEL;
					entity.Active = true;
					entity.OnActivated();
					entity.FrameUpdated = false;
					entity.SpawnFrame = GlobalFrame;
					return entity;
				}
			} catch (System.Exception ex) { Debug.LogException(ex); }
			return null;
		}


		private static void PushEntityBack (Entity entity, EntityStack meta = null) {
			entity.Active = false;
			try {
				entity.OnInactivated();
			} catch (System.Exception ex) { Debug.LogException(ex); }
			entity.FrameUpdated = false;
			StagedEntityHash.Remove(entity.InstanceID);
			if (meta == null && EntityPool.TryGetValue(entity.TypeID, out meta)) {
				meta.Push(entity);
			}
		}


		private static E PeekEntity<E> () where E : Entity => PeekEntity(typeof(E).AngeHash()) as E;
		private static Entity PeekEntity (int typeID) => EntityPool.TryGetValue(typeID, out var meta) ? meta.Peek() : null;


		#endregion




	}
}
