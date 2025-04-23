using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using AngeliA;

namespace AngeliA.Platformer;

/// <summary>
/// Core system for triggering specified function of entities on stage/map
/// </summary>
public static class CircuitSystem {




	#region --- VAR ---


	// Const
	private const int MAX_COUNT = 4096 * 2;

	// Data
	private static readonly Dictionary<int, (bool left, bool right, bool down, bool up)> WireIdPool = [];
	private static readonly Queue<(Int3 pos, bool left, bool right, bool down, bool up, int stamp)> TriggeringTask = [];
	private static readonly Dictionary<Int3, int> TriggeredTaskStamp = [];
	private static readonly Dictionary<int, MethodInfo> OperatorPool = [];
	private static readonly object[] OperateParamCache = [null, 0, Direction4.Down];
	private static readonly HashSet<Int4> LoadedBackgroundTrigger = [];
	[OnCircuitWireActived_Int3UnitPos] internal static System.Action<Int3> OnCircuitWireActived;


	#endregion




	#region --- MSG ---


	[OnGameInitialize]
	internal static void OnGameInitialize () {
		// Init Operator Pool
		OperatorPool.Clear();
		foreach (var (method, _) in Util.AllStaticMethodWithAttribute<CircuitOperate_Int3UnitPos_IntStamp_Direction5FromAttribute>()) {
			if (method.DeclaringType == null) continue;
			var type = method.DeclaringType;
			if (type.IsAbstract) {
				foreach (var _type in type.AllChildClass()) {
					OperatorPool.Add(_type.AngeHash(), method);
				}
			} else {
				OperatorPool.Add(type.AngeHash(), method);
			}
		}
		OperatorPool.TrimExcess();
	}


	[OnGameInitialize]
	internal static void ReloadAllBackgroundTrigger () {
		lock (LoadedBackgroundTrigger) {
			// Load Pusher from File
			LoadedBackgroundTrigger.Clear();
			string path = Util.CombinePaths(Universe.BuiltIn.SlotMetaRoot, "LoadedBackgroundTrigger");
			if (!Util.FileExists(path)) return;
			using var stream = File.Open(path, FileMode.Open);
			using var reader = new BinaryReader(stream);
			while (reader.NotEnd()) {
				if (LoadedBackgroundTrigger.Count >= MAX_COUNT) {
					break;
				}
				LoadedBackgroundTrigger.Add(new Int4(
					reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32()
				));
			}
		}
	}


	[OnGameInitialize(-64)]
	internal static void OnGameInitialize_Background () {
		// Start Background Thread
		System.Threading.Tasks.Task.Run(TriggerAllLoadedLoop);
		static void TriggerAllLoadedLoop () {
			while (true) {

				if (Game.GlobalFrame == 0) goto _SLEEP_;

				try {
					var squad = WorldSquad.Front;
					lock (LoadedBackgroundTrigger) {
						for (int i = 0; i < LoadedBackgroundTrigger.Count; i++) {
							foreach (var int4 in LoadedBackgroundTrigger) {
								var unitPos = new Int3(int4.x, int4.y, int4.z);
								int entityID = int4.w;
								if (squad.GetBlockAt(unitPos.x, unitPos.y, unitPos.z, BlockType.Entity) != entityID) {
									LoadedBackgroundTrigger.Remove(int4);
									continue;
								}
								OperateCircuit(unitPos);
							}
						}
					}
				} catch (System.Exception ex) { Debug.LogException(ex); }

				// Sleep
				_SLEEP_:;
				Thread.Sleep(1000);

			}
		}
	}


	[OnSavingSlotChanged]
	internal static void OnSavingSlotChanged () {
		if (Game.GlobalFrame != 0) {
			ReloadAllBackgroundTrigger();
		}
	}


	[OnMapEditorModeChange_Mode]
	internal static void OnMapEditorModeChange (OnMapEditorModeChange_ModeAttribute.Mode mode) {
		if (mode == OnMapEditorModeChange_ModeAttribute.Mode.EnterEditMode) {
			SaveAndClearAllBackgroundTrigger();
		} else if (mode == OnMapEditorModeChange_ModeAttribute.Mode.EnterPlayMode) {
			ReloadAllBackgroundTrigger();
		}
	}


	[BeforeSavingSlotChanged]
	[OnGameQuitting]
	internal static void SaveAndClearAllBackgroundTrigger () {
		// Save Pusher to File 
		string path = Util.CombinePaths(Universe.BuiltIn.SlotMetaRoot, "LoadedBackgroundTrigger");
		using var stream = File.Open(path, FileMode.Create);
		using var writer = new BinaryWriter(stream);
		lock (LoadedBackgroundTrigger) {
			foreach (var int4 in LoadedBackgroundTrigger) {
				writer.Write(int4.x);
				writer.Write(int4.y);
				writer.Write(int4.z);
				writer.Write(int4.w);
			}
			LoadedBackgroundTrigger.Clear();
		}
	}


	[OnGameUpdateLater]
	internal static void OnGameUpdateLater () {
		int targetCount = TriggeringTask.Count;
		for (int i = 0; i < targetCount; i++) {
			var (pos, left, right, down, up, stamp) = TriggeringTask.Dequeue();
			if (left) Interate(pos.Shift(-1, 0), stamp, Direction5.Right);
			if (right) Interate(pos.Shift(1, 0), stamp, Direction5.Left);
			if (down) Interate(pos.Shift(0, -1), stamp, Direction5.Up);
			if (up) Interate(pos.Shift(0, 1), stamp, Direction5.Down);
		}
	}


	#endregion




	#region --- API ---


	/// <summary>
	/// Register target IMapItem as wire for operating circuit
	/// </summary>
	/// <param name="id">ID of the target entity/element</param>
	/// <param name="connectL">True if this wire connect to the wire/operator on left</param>
	/// <param name="connectR">True if this wire connect to the wire/operator on right</param>
	/// <param name="connectD">True if this wire connect to the wire/operator on bottom</param>
	/// <param name="connectU">True if this wire connect to the wire/operator on top</param>
	public static void RegisterWire (int id, bool connectL, bool connectR, bool connectD, bool connectU) => WireIdPool[id] = (connectL, connectR, connectD, connectU);


	/// <summary>
	/// True if the given ID is registed as wire
	/// </summary>
	public static bool IsWire (int typeID) => WireIdPool.ContainsKey(typeID);


	/// <summary>
	/// Trigger the system at given position
	/// </summary>
	/// <param name="unitX">Position to start circuit trigger in unit space</param>
	/// <param name="unitY">Position to start circuit trigger in unit space</param>
	/// <param name="unitZ">Position Z</param>
	/// <param name="stamp">unique ID for this operation</param>
	/// <param name="startDirection">Only start at this direction (Set to center to start all direction)</param>
	public static void TriggerCircuit (int unitX, int unitY, int unitZ, int stamp = int.MinValue, Direction5 startDirection = Direction5.Center) {
		stamp = stamp == int.MinValue ? Game.PauselessFrame : stamp;
		var startPos = new Int3(unitX, unitY, unitZ);
		Interate(startPos, stamp, startDirection.Opposite());
	}


	/// <summary>
	/// Perform the function from the operator at given position
	/// </summary>
	/// <param name="unitPos">Target position in unit space</param>
	/// <param name="stamp">unique ID for this operation</param>
	/// <param name="circuitFrom">Which direction does the circuit came from. Set to center to make current as original.</param>
	/// <returns></returns>
	public static bool OperateCircuit (Int3 unitPos, int stamp = int.MinValue, Direction5 circuitFrom = Direction5.Center) {

		var squad = WorldSquad.Front;
		bool triggered = false;

		// Check Block Operator
		int entityId = squad.GetBlockAt(unitPos.x, unitPos.y, unitPos.z, BlockType.Entity);
		if (entityId != 0 && OperatorPool.TryGetValue(entityId, out var method)) {
			TriggeredTaskStamp[unitPos] = stamp;
			OperateParamCache[0] = unitPos;
			OperateParamCache[1] = stamp;
			OperateParamCache[2] = circuitFrom;
			method?.Invoke(null, OperateParamCache);
			triggered = true;
		}

		// Check Staged Operator
		if (unitPos.z == Stage.ViewZ && Physics.GetEntity<ICircuitOperator>(
				IRect.Point(unitPos.x.ToGlobal() + Const.HALF, unitPos.y.ToGlobal() + Const.HALF),
				PhysicsMask.ENTITY, null, OperationMode.ColliderAndTrigger
			) is ICircuitOperator _operator
		) {
			_operator.OnTriggeredByCircuit();
			triggered = true;
		}

		return triggered;
	}


	/// <summary>
	/// Get connection directions from given wire ID
	/// </summary>
	/// <returns>True if the wire ID is valid</returns>
	public static bool WireEntityID_to_WireConnection (int id, out bool connectL, out bool connectR, out bool connectD, out bool connectU) {
		if (WireIdPool.TryGetValue(id, out var connect)) {
			connectL = connect.left;
			connectR = connect.right;
			connectD = connect.down;
			connectU = connect.up;
			return true;
		} else {
			connectL = false;
			connectR = false;
			connectD = false;
			connectU = false;
			return false;
		}
	}


	/// <summary>
	/// True if the given ID is a valid circuit operator entity
	/// </summary>
	public static bool IsCircuitOperator (int typeID) => OperatorPool.ContainsKey(typeID) || ICircuitOperator.IsOperator(typeID);


	/// <summary>
	/// Convert wire connection info into an Intager
	/// </summary>
	public static int WireConnection_to_BitInt (bool connectL, bool connectR, bool connectD, bool connectU) => (connectL ? 0b1000 : 0b0000) | (connectR ? 0b0100 : 0b0000) | (connectD ? 0b0010 : 0b0000) | (connectU ? 0b0001 : 0b0000);


	/// <summary>
	/// Convert an intager into wire connection info
	/// </summary>
	public static void BitInt_to_WireConnection (int bitInt, out bool connectL, out bool connectR, out bool connectD, out bool connectU) {
		connectL = (bitInt & 0b1000) != 0;
		connectR = (bitInt & 0b0100) != 0;
		connectD = (bitInt & 0b0010) != 0;
		connectU = (bitInt & 0b0001) != 0;
	}


	/// <summary>
	/// Get unique ID for circuit system at given position in unit space
	/// </summary>
	public static int GetStamp (Int3 unitPos) => TriggeredTaskStamp.TryGetValue(unitPos, out var pos) ? pos : -1;


	// Background
	/// <summary>
	/// Add a trigger in background thread
	/// </summary>
	/// <param name="entityID">Source ID of this trigger</param>
	/// <param name="unitPos">Position in unit space</param>
	/// <returns>True if the capacity is not reached</returns>
	public static bool TryAddBackgroundTrigger (int entityID, Int3 unitPos) {
		if (LoadedBackgroundTrigger.Count >= MAX_COUNT) return false;
		LoadedBackgroundTrigger.Add(new Int4(unitPos.x, unitPos.y, unitPos.z, entityID));
		return true;
	}


	/// <summary>
	/// Remove the given entity from background thread triggers
	/// </summary>
	/// <param name="entityID">Source ID of this trigger</param>
	/// <param name="unitPos">Position in unit space</param>
	/// <returns>True if the trigger is removed</returns>
	public static bool TryRemoveBackgroundTrigger (int entityID, Int3 unitPos) => LoadedBackgroundTrigger.Remove(new Int4(unitPos.x, unitPos.y, unitPos.z, entityID));


	#endregion




	#region --- LGC ---


	private static void Interate (Int3 pos, int stamp, Direction5 circuitFrom) {

		if (TriggeredTaskStamp.TryGetValue(pos, out int triggeredStamp) && stamp <= triggeredStamp) return;
		var squad = WorldSquad.Front;

		// Check for Wire Expand
		int _id = squad.GetBlockAt(pos.x, pos.y, pos.z, BlockType.Element);
		if (
			_id != 0 &&
			WireIdPool.TryGetValue(_id, out var connectDirections) &&
			(circuitFrom == Direction5.Center || ConnectionValid(circuitFrom, connectDirections))
		) {
			TriggeredTaskStamp[pos] = stamp;
			TriggeringTask.Enqueue((
				pos,
				connectDirections.left, connectDirections.right,
				connectDirections.down, connectDirections.up,
				stamp
			));
			OnCircuitWireActived?.Invoke(pos);
		}

		OperateCircuit(pos, stamp, circuitFrom);


	}


	private static bool ConnectionValid (Direction5 requireCon, (bool, bool, bool, bool) wireCons) => requireCon switch {
		Direction5.Left => wireCons.Item1,
		Direction5.Right => wireCons.Item2,
		Direction5.Down => wireCons.Item3,
		Direction5.Up => wireCons.Item4,
		_ => false,
	};


	#endregion




}
