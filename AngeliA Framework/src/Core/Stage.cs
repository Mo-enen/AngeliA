using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace AngeliA;

public static class Stage {




	#region --- SUB ---


	private class EntityComparer : IComparer<Entity> {
		public static readonly EntityComparer Instance = new();
		public int Compare (Entity a, Entity b) =>
			a.Active != b.Active ? b.Active.CompareTo(a.Active) :
			a.Order != b.Order ? a.Order.CompareTo(b.Order) :
			a.TypeID != b.TypeID ? a.TypeID.CompareTo(b.TypeID) :
			a.SpawnFrame != b.SpawnFrame ? a.SpawnFrame.CompareTo(b.SpawnFrame) :
			a.X != b.X ? a.X.CompareTo(b.X) :
			a.Y != b.Y ? a.Y.CompareTo(b.Y) :
			a.InstanceOrder.CompareTo(b.InstanceOrder);
	}
	private class ReversedEntityComparer : IComparer<Entity> {
		public static readonly ReversedEntityComparer Instance = new();
		public int Compare (Entity b, Entity a) =>
			a.Active != b.Active ? a.Active.CompareTo(b.Active) :
			a.Order != b.Order ? a.Order.CompareTo(b.Order) :
			a.TypeID != b.TypeID ? a.TypeID.CompareTo(b.TypeID) :
			a.SpawnFrame != b.SpawnFrame ? a.SpawnFrame.CompareTo(b.SpawnFrame) :
			a.X != b.X ? a.X.CompareTo(b.X) :
			a.Y != b.Y ? a.Y.CompareTo(b.Y) :
			a.InstanceOrder.CompareTo(b.InstanceOrder);
	}


	private class EntityStack {

		// Api
		public int SpawnedCount => InstanceCount - Entities.Count;
		public Stack<Entity> Entities = null;
		public Type EntityType = null;
		public IRect LocalBound = default;
		public bool DrawBehind = false;
		public bool DestroyOnZChanged = true;
		public bool DontSpawnFromWorld = false;
		public int Capacity = 0;
		public bool DespawnOutOfRange = true;
		public bool UpdateOutOfRange = false;
		public bool RequireReposition = false;
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
			e.DestroyOnZChanged = DestroyOnZChanged;
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
	private static readonly int[] DEFAULT_ENTITY_CAPACITY = [
		64,		//UI
		4096,	//GAME
		512,	//CHARACTER
		1024,	//ENVIRONMENT 
		1024,	//WATER 
		1024,	//BULLET 
		1024,	//ITEM
		128,	//DECORATE
	];

	// Api
	public static int[] EntityCounts { get; private set; } = new int[EntityLayer.COUNT];
	public static Entity[][] Entities { get; private set; } = null;
	public static IRect SpawnRect { get; private set; } = default;
	public static IRect AntiSpawnRect { get; private set; } = default;
	public static IRect ViewRect { get; private set; } = default;
	public static int LastSettleFrame { get; private set; } = 0;
	public static int ViewZ { get; private set; } = 0;
	public static int? DelayingViewX => ViewDelayX.value;
	public static int? DelayingViewY => ViewDelayY.value;
	public static int? DelayingViewHeight => ViewDelayHeight.value;
	public static bool Enable { get; private set; } = true;
	public static bool IsReady { get; private set; } = false;

	// Data
	private static (int? value, int priority) ViewDelayX = (null, int.MinValue);
	private static (int? value, int priority) ViewDelayY = (null, int.MinValue);
	private static (int? value, int priority, int centralizedFrame) ViewDelayHeight = (null, int.MinValue, -1);
	private static event Action BeforeFirstUpdate;
	private static event Action BeforeBeforeUpdate;
	private static event Action BeforeUpdateUpdate;
	private static event Action BeforeLateUpdate;
	private static event Action AfterLateUpdate;
	private static event Action OnViewZChanged;
	private static event Action<int> BeforeLayerFrameUpdate;
	private static event Action<int> AfterLayerFrameUpdate;
	private static event Action<Entity, Int3?, Int3> AfterEntityReposition;
	private static readonly Dictionary<int, EntityStack> EntityPool = [];
	private static readonly HashSet<Int3> StagedEntityHash = [];
	private static int ViewLerpRate = 1000;
	private static int? RequireSetViewZ = null;


	#endregion




	#region --- MSG ---


	[OnGameInitialize(-64)]
	internal static void OnGameInitialize () {

		IsReady = true;
		Enable = !Game.IsToolApplication;
		int defaultViewHegiht = Universe.BuiltInInfo.DefaultViewHeight;
		int minViewHeight = Universe.BuiltInInfo.MinViewHeight;
		int maxViewHeight = Universe.BuiltInInfo.MaxViewHeight;
		ViewRect = new IRect(
			0, 0,
			Universe.BuiltInInfo.ViewRatio * defaultViewHegiht.Clamp(minViewHeight, maxViewHeight) / 1000,
			defaultViewHegiht.Clamp(minViewHeight, maxViewHeight)
		);
		SpawnRect = ViewRect.Expand(Const.SPAWN_PADDING);

		if (!Enable) return;

		var capacities = new int[EntityLayer.COUNT];
		DEFAULT_ENTITY_CAPACITY.CopyTo(capacities, 0);
		foreach (var (_, att) in Util.ForAllAssemblyWithAttribute<EntityLayerCapacityAttribute>()) {
			if (att.Layer < 0 || att.Layer >= EntityLayer.COUNT) continue;
			capacities[att.Layer] = att.Capacity;
		}
		Entities = new Entity[EntityLayer.COUNT][];
		for (int i = 0; i < EntityLayer.COUNT; i++) {
			Entities[i] = new Entity[capacities[i]];
		}
		EntityPool.Clear();

		foreach (var eType in typeof(Entity).AllChildClass()) {

			int id = eType.AngeHash();
			int preSpawn = 0;
			int capacity = 64;
			var att_Layer = eType.GetCustomAttribute<EntityAttribute.LayerAttribute>(true);
			var att_Capacity = eType.GetCustomAttribute<EntityAttribute.CapacityAttribute>(true);
			var att_Bound = eType.GetCustomAttribute<EntityAttribute.BoundsAttribute>(true);
			var att_DontDespawn = eType.GetCustomAttribute<EntityAttribute.DontDestroyOutOfRangeAttribute>(true);
			var att_ForceUpdate = eType.GetCustomAttribute<EntityAttribute.UpdateOutOfRangeAttribute>(true);
			var att_DontDrawBehind = eType.GetCustomAttribute<EntityAttribute.DontDrawBehindAttribute>(true);
			var att_DontDestroyOnTran = eType.GetCustomAttribute<EntityAttribute.DontDestroyOnZChangedAttribute>(true);
			var att_DontSpawnFromWorld = eType.GetCustomAttribute<EntityAttribute.DontSpawnFromWorld>(true);
			var att_Order = eType.GetCustomAttribute<EntityAttribute.StageOrderAttribute>(true);
			var att_Repos = eType.GetCustomAttribute<EntityAttribute.RepositionWhenInactiveAttribute>(true);
			int layer = att_Layer != null ? att_Layer.Layer.Clamp(0, EntityLayer.COUNT - 1) : 0;
			if (att_Capacity != null) {
				capacity = att_Capacity.Value.Clamp(1, Entities[layer].Length);
				preSpawn = att_Capacity.PreSpawn.Clamp(0, Entities[layer].Length);
			}
			var stack = new EntityStack() {
				Entities = new Stack<Entity>(preSpawn),
				LocalBound = att_Bound != null ? att_Bound.Value : new(0, 0, Const.CEL, Const.CEL),
				DrawBehind = att_DontDrawBehind == null,
				DestroyOnZChanged = att_DontDestroyOnTran == null,
				Capacity = capacity,
				EntityType = eType,
				DespawnOutOfRange = att_DontDespawn == null,
				UpdateOutOfRange = att_ForceUpdate != null,
				DontSpawnFromWorld = att_DontSpawnFromWorld != null,
				Order = att_Order != null ? att_Order.Order : 0,
				RequireReposition = att_Repos != null,
				Layer = layer,
			};
			for (int i = 0; i < preSpawn; i++) {
				try {
					var e = stack.CreateInstance();
					if (e == null) break;
					stack.Entities.Push(e);
				} catch (Exception ex) { Debug.LogException(ex); }
			}
			EntityPool.TryAdd(id, stack);
		}

		// Event
		Util.LinkEventWithAttribute<OnViewZChangedAttribute>(typeof(Stage), nameof(OnViewZChanged));
		Util.LinkEventWithAttribute<BeforeLayerFrameUpdateAttribute>(typeof(Stage), nameof(BeforeLayerFrameUpdate));
		Util.LinkEventWithAttribute<AfterLayerFrameUpdateAttribute>(typeof(Stage), nameof(AfterLayerFrameUpdate));

		Util.LinkEventWithAttribute<BeforeFirstUpdateAttribute>(typeof(Stage), nameof(BeforeFirstUpdate));
		Util.LinkEventWithAttribute<BeforeBeforeUpdateAttribute>(typeof(Stage), nameof(BeforeBeforeUpdate));
		Util.LinkEventWithAttribute<BeforeUpdateUpdateAttribute>(typeof(Stage), nameof(BeforeUpdateUpdate));
		Util.LinkEventWithAttribute<BeforeLateUpdateAttribute>(typeof(Stage), nameof(BeforeLateUpdate));
		Util.LinkEventWithAttribute<AfterLateUpdateAttribute>(typeof(Stage), nameof(AfterLateUpdate));

		Util.LinkEventWithAttribute<AfterEntityRepositionAttribute>(typeof(Stage), nameof(AfterEntityReposition));

	}


	[OnGameRestart]
	internal static void OnGameRestart () {
		if (!Enable) return;
		SetViewSizeDelay(Universe.BuiltInInfo.DefaultViewHeight, 1000, int.MaxValue);
	}


	[OnGameQuitting]
	internal static void OnGameQuitting () => DespawnAllNonUiEntities(refreshImmediately: true);


	[OnGameUpdate(-4096)]
	internal static void UpdateView () {

		if (!Enable) return;

		// Move View Rect
		if (ViewDelayX.value.HasValue || ViewDelayY.value.HasValue || ViewDelayHeight.value.HasValue) {
			// Get New View Rect
			int targetHeight = (ViewDelayHeight.value ?? ViewRect.height).Clamp(
				Universe.BuiltInInfo.MinViewHeight,
				Universe.BuiltInInfo.MaxViewHeight
			);
			var viewRectDelay = new IRect(
				ViewDelayX.value ?? ViewRect.x,
				ViewDelayY.value ?? ViewRect.y,
				Universe.BuiltInInfo.ViewRatio * targetHeight / 1000,
				targetHeight
			);
			// Centralize
			if (viewRectDelay.width != ViewRect.width && Game.GlobalFrame <= ViewDelayHeight.centralizedFrame + 1) {
				viewRectDelay.x -= (viewRectDelay.width - ViewRect.width) / 2;
			}
			// Set to View Rect
			if (ViewLerpRate >= 1000) {
				ViewRect = viewRectDelay;
				ViewDelayX.value = null;
				ViewDelayY.value = null;
				ViewDelayHeight.value = null;
			} else {
				ViewRect = new IRect(
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


	[OnGameUpdateLater(-4096)]
	internal static void UpdateAllEntities () {

		if (!Enable) return;

		// Update All Layers
		UpdateEntitiesForLayer(-1);

		// Z Change
		if (RequireSetViewZ.HasValue) {
			int newZ = RequireSetViewZ.Value;
			RequireSetViewZ = null;
			Settle();
			// Despawn Entities
			for (int layer = 0; layer < EntityLayer.COUNT; layer++) {
				var entities = Entities[layer];
				int count = EntityCounts[layer];
				for (int i = 0; i < count; i++) {
					var e = entities[i];
					if (e.DespawnOutOfRange || e.DestroyOnZChanged) {
						e.Active = false;
					}
				}
				RefreshStagedEntities(layer);
			}
			AntiSpawnRect = new IRect(int.MinValue + 1, 0, 0, 0);
			ViewZ = newZ;
			OnViewZChanged?.Invoke();
		}

	}


	[OnGameUpdatePauseless]
	internal static void UpdateUiEntitiesOnPause () {
		if (!Enable || Game.IsPlaying) return;
		UpdateEntitiesForLayer(EntityLayer.UI);
	}


	private static void UpdateEntitiesForLayer (int targetLayer) {

		int startLayer = 0;
		int endLayer = EntityLayer.COUNT;
		if (targetLayer >= 0) {
			startLayer = targetLayer;
			endLayer = targetLayer + 1;
		}

		// Remove Inactive and Outside SpawnRect
		for (int layer = startLayer; layer < endLayer; layer++) {
			RefreshStagedEntities(layer);
		}

		// First (Fill Physics)
		BeforeFirstUpdate?.Invoke();
		for (int layer = startLayer; layer < endLayer; layer++) {
			var span = new ReadOnlySpan<Entity>(Entities[layer]);
			int count = EntityCounts[layer].Clamp(0, span.Length);
			for (int index = 0; index < count; index++) {
				try {
					span[index].UpdateToFirst();
				} catch (Exception ex) { Debug.LogException(ex); }
			}
		}

		// Before
		BeforeBeforeUpdate?.Invoke();
		for (int layer = startLayer; layer < endLayer; layer++) {
			var span = new ReadOnlySpan<Entity>(Entities[layer]);
			int count = EntityCounts[layer].Clamp(0, span.Length);
			for (int index = 0; index < count; index++) {
				var e = span[index];
				if (e.UpdateOutOfRange || e.UpdateStep >= 4 || ViewRect.Overlaps(e.GlobalBounds)) {
					try {
						e.UpdateToBefore();
					} catch (Exception ex) { Debug.LogException(ex); }
				}
			}
		}

		// Update
		BeforeUpdateUpdate?.Invoke();
		for (int layer = startLayer; layer < endLayer; layer++) {
			var span = new ReadOnlySpan<Entity>(Entities[layer]);
			int count = EntityCounts[layer].Clamp(0, span.Length);
			for (int index = 0; index < count; index++) {
				var e = span[index];
				if (e.UpdateOutOfRange || e.UpdateStep >= 4 || ViewRect.Overlaps(e.GlobalBounds)) {
					try {
						e.UpdateToUpdate();
					} catch (Exception ex) { Debug.LogException(ex); }
				}
			}
		}

		// Late
		BeforeLateUpdate?.Invoke();
		var cullCameraRect = Renderer.CameraRect.Expand(GetCameraCullingPadding());
		for (int layer = startLayer; layer < endLayer; layer++) {
			var span = new ReadOnlySpan<Entity>(Entities[layer]);
			int count = EntityCounts[layer].Clamp(0, span.Length);
			BeforeLayerFrameUpdate?.Invoke(layer);
			for (int index = 0; index < count; index++) {
				var e = span[index];
				if (e.UpdateOutOfRange || cullCameraRect.Overlaps(e.GlobalBounds)) {
					try {
						Renderer.SetLayerToDefault();
						e.UpdateToLate();
					} catch (Exception ex) { Debug.LogException(ex); }
				}
			}
			AfterLayerFrameUpdate?.Invoke(layer);
		}
		AfterLateUpdate?.Invoke();

		// Final
		AntiSpawnRect = ViewRect.Expand(Const.ANTI_SPAWN_PADDING);
	}


	#endregion




	#region --- API ---


	public static void SetViewZ (int newZ, bool immediately = false) {
		if (immediately) ViewZ = newZ;
		RequireSetViewZ = newZ;
	}


	public static void SetViewRectImmediately (IRect newRect, bool remapAllRenderingCells = false) {

		newRect.width = newRect.width.GreaterOrEquel(1);
		newRect.height = newRect.height.GreaterOrEquel(1);

		// Stop Delay
		ViewDelayX.value = null;
		ViewDelayY.value = null;
		ViewDelayHeight.value = null;
		ViewDelayX.priority = int.MinValue;
		ViewDelayY.priority = int.MinValue;
		ViewDelayHeight.priority = int.MinValue;

		// Remap Rendering Cells
		if (remapAllRenderingCells) {
			int oldL = ViewRect.xMin;
			int oldR = ViewRect.xMax;
			int oldD = ViewRect.yMin;
			int oldU = ViewRect.yMax;
			int oldW = ViewRect.width;
			int oldH = ViewRect.height;
			int newL = newRect.xMin;
			int newR = newRect.xMax;
			int newD = newRect.yMin;
			int newU = newRect.yMax;
			int newW = newRect.width;
			int newH = newRect.height;
			for (int layer = 0; layer < RenderLayer.COUNT; layer++) {
				if (!Renderer.GetCells(layer, out var cells, out int count)) continue;
				for (int i = 0; i < count; i++) {
					var cell = cells[i];
					cell.X = Util.RemapUnclamped(oldL, oldR, newL, newR, (float)cell.X).RoundToInt();
					cell.Y = Util.RemapUnclamped(oldD, oldU, newD, newU, (float)cell.Y).RoundToInt();
					cell.Width = ((float)cell.Width * newW / oldW).RoundToInt();
					cell.Height = ((float)cell.Height * newH / oldH).RoundToInt();
				}
			}
		}

		// Apply Changes
		ViewRect = newRect;
		Renderer.UpdateCameraRect();
	}


	public static void Settle () => LastSettleFrame = Game.GlobalFrame;


	// Set View
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


	public static void SetViewSizeDelay (int height, int lerp = 1000, int priority = int.MinValue, bool centralized = false) {
		if (priority >= ViewDelayHeight.priority) {
			ViewDelayHeight = (height, priority, centralized ? Game.GlobalFrame : -1);
		}
		ViewLerpRate = lerp;
	}


	// Spawn
	public static Entity SpawnEntity (int typeID, int x, int y) => SpawnEntityLogic(typeID, x, y, new Int3(int.MinValue, 0, 0));
	public static T SpawnEntity<T> (int x, int y) where T : Entity => SpawnEntityLogic(typeof(T).AngeHash(), x, y, new(int.MinValue, 0)) as T;
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


	public static Entity SpawnEntityFromWorld (int typeID, int unitX, int unitY, int unitZ, bool forceSpawn = false) {
		var uPos = new Int3(unitX, unitY, unitZ);
		if (!forceSpawn && StagedEntityHash.Contains(uPos)) return null;
		if (!EntityPool.TryGetValue(typeID, out var stack)) return null;
		if (stack.DontSpawnFromWorld) return null;
		int x = unitX * Const.CEL;
		int y = unitY * Const.CEL;
		if (!forceSpawn && AntiSpawnRect.Overlaps(stack.LocalBound.Shift(x, y))) return null;
		return SpawnEntityLogic(typeID, x, y, uPos);
	}


	// Get Entity
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
		if (!Enable) return false;
		for (int layer = 0; layer < EntityLayer.COUNT; layer++) {
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
		if (!Enable) return false;
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
	public static bool TryGetEntityNearby<E> (Int2 pos, out E finalTarget) where E : Entity {
		finalTarget = null;
		if (!Enable) return false;
		int finalDistance = int.MaxValue;
		for (int layer = 0; layer < EntityLayer.COUNT; layer++) {
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
		if (Enable && layer >= 0 && layer < EntityLayer.COUNT) {
			count = EntityCounts[layer];
			entities = Entities[layer];
			return true;
		} else {
			count = 0;
			entities = null;
			return false;
		}
	}


	public static bool IsValidEntityID (int id) => EntityPool.ContainsKey(id);


	public static IEnumerable<E> ForAllActiveEntities<E> (int entityLayer = -1) where E : Entity {
		if (!Enable) yield break;
		int startLayer = entityLayer < 0 ? 0 : entityLayer;
		int endLayer = entityLayer < 0 ? EntityLayer.COUNT - 1 : entityLayer;
		for (int layer = startLayer; layer <= endLayer; layer++) {
			var entities = Entities[layer];
			int count = EntityCounts[layer]; ;
			for (int i = 0; i < count; i++) {
				var e = entities[i];
				if (e is E ee && e.Active) yield return ee;
			}
		}
	}


	public static int GetSpawnedEntityCount (int id) => EntityPool.TryGetValue(id, out var meta) ? meta.SpawnedCount : 0;


	// Despawn
	public static void DespawnAllEntitiesOfType<E> (int targetLayer = -1) where E : Entity {
		if (!Enable) return;
		int start = targetLayer < 0 ? 0 : targetLayer;
		int end = targetLayer < 0 ? EntityLayer.COUNT : targetLayer + 1;
		for (int layer = start; layer < end; layer++) {
			var entities = Entities[layer];
			int count = EntityCounts[layer];
			for (int i = 0; i < count; i++) {
				var e = entities[i];
				if (e.Active && e is E) {
					e.Active = false;
				}
			}
		}
	}


	// Stage Workflow
	public static void DespawnAllNonUiEntities (bool refreshImmediately = false) {
		if (!Enable) return;
		for (int layer = 0; layer < EntityLayer.COUNT; layer++) {
			if (layer == EntityLayer.UI) continue;
			var entities = Entities[layer];
			int count = EntityCounts[layer];
			for (int i = 0; i < count; i++) {
				entities[i].Active = false;
			}
			if (refreshImmediately) {
				RefreshStagedEntities(layer);
			}
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
	public static bool RequireDrawEntityBehind (int id) => EntityPool.TryGetValue(id, out var stack) && stack.DrawBehind;


	public static Int4 GetCameraCullingPadding () {
		int expand = Renderer.CameraRect.width * (Universe.BuiltInInfo.WorldBehindParallax - 1000) / 2000;
		return TaskSystem.HasTask() ? new Int4(expand, expand, expand, expand) : Int4.zero;
	}


	public static Type GetEntityType (int id) {
		if (EntityPool.TryGetValue(id, out var stack)) {
			return stack.EntityType;
		}
		return null;
	}


	public static int GetEntityCapacity (int typeID) => EntityPool.TryGetValue(typeID, out var stack) ? stack.Capacity : 0;


	#endregion




	#region --- LGC ---


	private static void RefreshStagedEntities (int layer) {

		if (!Enable) return;

		var entities = new Span<Entity>(Entities[layer]);
		int count = EntityCounts[layer].Clamp(0, entities.Length);

		// Inactive Out of Range Entities
		for (int i = 0; i < count; i++) {
			var entity = entities[i];
			if (entity.Active && entity.DespawnOutOfRange && !SpawnRect.Overlaps(entity.GlobalBounds)) {
				entity.Active = false;
			}
		}

		// Sort
		Util.QuickSort(
			entities, 0, count - 1,
			layer == EntityLayer.UI ? ReversedEntityComparer.Instance : EntityComparer.Instance
		);

		// Remove Inactive
		while (count > 0) {
			var e = entities[count - 1];
			if (e.Active) break;

			try {
				e.Stamp = int.MaxValue;
				e.OnInactivated();
			} catch (Exception ex) { Debug.LogException(ex); }

			StagedEntityHash.Remove(e.InstanceID);

			// Push Back
			if (EntityPool.TryGetValue(e.TypeID, out var stack)) {
				stack.Push(e);
				if (stack.RequireReposition && !e.DontRepositionOnce) {
					TryRepositionEntity(e);
				}
			}

			// Next
			count--;
		}

		EntityCounts[layer] = count;
	}


	private static void TryRepositionEntity (Entity entity) {

		bool requireRepos = false;
		bool requireClearOriginal = false;

		// Get Position
		int currentUnitX;
		int currentUnitY;
		if (entity is Rigidbody rig) {
			currentUnitX = (rig.X + rig.OffsetX + Const.HALF).ToUnit();
			currentUnitY = (rig.Y + rig.OffsetY + Const.HALF).ToUnit();
		} else {
			currentUnitX = (entity.X + Const.HALF).ToUnit();
			currentUnitY = (entity.Y + Const.HALF).ToUnit();
		}

		// Get Info for Original Block
		if (entity.MapUnitPos.HasValue) {
			var mapPos = entity.MapUnitPos.Value;
			int blockIdAtMapPos = WorldSquad.Front.GetBlockAt(mapPos.x, mapPos.y, mapPos.z, BlockType.Entity);
			// Get Require Mode
			if (blockIdAtMapPos != entity.TypeID) {
				// Overlaped by Other Entity
				requireRepos = true;
			} else if (currentUnitX != mapPos.x || currentUnitY != mapPos.y) {
				// Position Moved
				requireRepos = true;
				requireClearOriginal = true;
			}
		} else {
			// Entity Not from Map
			requireRepos = true;
		}

		// Perform Reposition
		if (
			requireRepos &&
			FrameworkUtil.TryGetEmptyPlaceNearby(
				currentUnitX, currentUnitY, ViewZ,
				out int resultUnitX, out int resultUnitY
			)
		) {
			// Set Block
			WorldSquad.Front.SetBlockAt(resultUnitX, resultUnitY, ViewZ, BlockType.Entity, entity.TypeID);
			// Clear Original
			if (requireClearOriginal) {
				var oPos = entity.MapUnitPos.Value;
				WorldSquad.Front.SetBlockAt(oPos.x, oPos.y, oPos.z, BlockType.Entity, 0);
			}
			// Fix Inventory
			if (entity != null) {
				AfterEntityReposition?.Invoke(entity, entity.MapUnitPos, new Int3(resultUnitX, resultUnitY, ViewZ));
			}

		}
	}


	private static Entity SpawnEntityLogic (int typeID, int x, int y, Int3 globalUnitPos) {
		if (!Enable) return null;
		try {
			if (
				globalUnitPos.x != int.MinValue &&
				StagedEntityHash.Contains(globalUnitPos)
			) return null;
			if (!EntityPool.TryGetValue(typeID, out var eMeta)) {
#if DEBUG
				if (typeID != 0 && !EntityPool.ContainsKey(typeID)) {
					Debug.LogWarning($"Invalid Entity Type ID {typeID}");
				}
#endif
				return null;
			}
			if (!EntityPool.TryGetValue(typeID, out var stack)) return null;
			int layer = stack.Layer;
			int count = EntityCounts[layer];
			var entities = Entities[layer];
			Entity entity = null;
			if (count >= entities.Length) return null;
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
				entity.UpdateStep = 0;
				entity.SpawnFrame = Game.GlobalFrame;
				return entity;
			}
		} catch (Exception ex) { Debug.LogException(ex); }
		return null;
	}


	private static E PeekEntity<E> () where E : Entity => PeekEntity(typeof(E).AngeHash()) as E;
	private static Entity PeekEntity (int typeID) => EntityPool.TryGetValue(typeID, out var meta) ? meta.Peek() : null;


	#endregion




}
