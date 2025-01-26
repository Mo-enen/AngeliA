using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public sealed class CharacterBuff {




	#region --- SUB ---


	private struct State () {
		public readonly bool IsActived => Game.GlobalFrame <= EndFrame;
		public int EndFrame = -1;
		public Buff Buff;
		public object Data;
	}


	#endregion




	#region --- VAR ---


	// Api
	public int BuffCount => BuffStates.Count;

	// Data
	private static readonly Dictionary<int, Buff> Pool = [];
	private readonly Dictionary<int, State> BuffStates = [];
	private readonly Dictionary<int, int> BuffPrevents = [];
	private readonly Character Character;


	#endregion




	#region --- MSG ---


	[OnGameInitialize(-128)]
	internal static void OnGameInitialize () {
		Pool.Clear();
		foreach (var type in typeof(Buff).AllChildClass()) {
			if (System.Activator.CreateInstance(type) is not Buff buff) continue;
			Pool.TryAdd(buff.TypeID, buff);
		}
		Pool.TrimExcess();
	}


	[OnGameRestart]
	internal static void OnGameRestart () => PlayerSystem.Selecting?.Buff.ClearAllBuffs();


	internal CharacterBuff (Character target) => Character = target;


	internal void ApplyOnBeforeUpdate () {
		// Refresh Prevents
		foreach (var (id, endFrame) in BuffPrevents) {
			if (Game.GlobalFrame >= endFrame) {
				BuffPrevents.Remove(id);
			}
		}
		// Update States
		foreach (var (id, state) in BuffStates) {
			if (!state.IsActived) {
				BuffStates.Remove(id);
				continue;
			}
			state.Buff.BeforeUpdate(Character);
		}
	}


	internal void ApplyOnLateUpdate () {
		foreach (var (_, state) in BuffStates) {
			state.Buff.LateUpdate(Character);
		}
	}


	internal void ApplyOnCharacterRenderered () {
		foreach (var (_, state) in BuffStates) {
			state.Buff.OnCharacterRenderered(Character.Rendering);
		}
	}


	internal void ApplyOnAttack (Bullet bullet) {
		foreach (var (_, state) in BuffStates) {
			state.Buff.OnCharacterAttack(Character, bullet);
		}
	}


	#endregion




	#region --- API ---


	public bool HasBuff (int id) => BuffStates.ContainsKey(id);


	public void GiveBuff (int id, int duration = 1) {
		if (!Pool.TryGetValue(id, out var buff)) return;
		if (BuffPrevents.ContainsKey(id)) return;
		if (!BuffStates.TryGetValue(id, out var state)) {
			state = new State() {
				Buff = buff,
				Data = default,
			};
		}
		state.EndFrame = Util.Max(state.EndFrame, Game.GlobalFrame + duration);
		BuffStates[id] = state;
	}


	public void ClearBuff (int id) => BuffStates.Remove(id);


	public void ClearAllBuffs () {
		BuffStates.Clear();
		BuffPrevents.Clear();
	}


	public void PreventBuff (int id, int duration = 1) {
		BuffStates.Remove(id);
		BuffPrevents[id] = Game.GlobalFrame + duration;
	}


	public bool IsBuffPrevented (int id) => BuffPrevents.ContainsKey(id);


	public object GetBuffData (int id) => BuffStates.TryGetValue(id, out var state) ? state.Data : null;


	public void SetBuffData (int id, object data) {
		if (!BuffStates.TryGetValue(id, out var state)) return;
		state.Data = data;
		BuffStates[id] = state;
	}


	public int GetBuffEndFrame (int id) => BuffStates.TryGetValue(id, out var state) ? state.EndFrame : -1;


	public IEnumerable<Buff> ForAllBuffs () {
		foreach (var (_, state) in BuffStates) {
			yield return state.Buff;
		}
	}


	public static string GetBuffDisplayName (int id) {
		if (!Pool.TryGetValue(id, out var buff)) return "";
		return Language.Get(buff.NameID, buff.TypeName);
	}


	public static string GetBuffDescription (int id) {
		if (!Pool.TryGetValue(id, out var buff)) return "";
		return Language.Get(buff.DescriptionID);
	}


	#endregion




}
