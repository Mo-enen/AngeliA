using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

/// <summary>
/// Behavior class that handles buff logic for characters
/// </summary>
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
	/// <summary>
	/// Total count for all buff the character is having
	/// </summary>
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


	/// <summary>
	/// True if the character is having the given buff
	/// </summary>
	public bool HasBuff (int id) => BuffStates.ContainsKey(id);


	/// <summary>
	/// Give the character given buff for specified frames long
	/// </summary>
	public void GiveBuff (int id, int duration = -1) {
		if (!Pool.TryGetValue(id, out var buff)) return;
		if (BuffPrevents.ContainsKey(id)) return;
		if (!BuffStates.TryGetValue(id, out var state)) {
			state = new State() {
				Buff = buff,
				Data = default,
			};
		}
		duration = duration < 0 ? buff.DefaultDuration : duration;
		state.EndFrame = Util.Max(state.EndFrame, Game.GlobalFrame + duration);
		BuffStates[id] = state;
	}


	/// <summary>
	/// Remove the given buff from character
	/// </summary>
	public void ClearBuff (int id) => BuffStates.Remove(id);


	/// <summary>
	/// Remove all buff from character
	/// </summary>
	public void ClearAllBuffs () {
		BuffStates.Clear();
		BuffPrevents.Clear();
	}


	/// <summary>
	/// Do not allow the character have given buff for specified frames
	/// </summary>
	public void PreventBuff (int id, int duration = 1) {
		BuffStates.Remove(id);
		BuffPrevents[id] = duration == int.MaxValue ? int.MaxValue : Game.GlobalFrame + duration;
	}


	/// <summary>
	/// True if the given buff is being prevented
	/// </summary>
	public bool IsBuffPrevented (int id) => BuffPrevents.ContainsKey(id);


	/// <summary>
	/// Get the user data of the given buff from the character
	/// </summary>
	public object GetBuffData (int id) => BuffStates.TryGetValue(id, out var state) ? state.Data : null;


	/// <summary>
	/// Set user data of the given buff to the character
	/// </summary>
	public void SetBuffData (int id, object data) {
		if (!BuffStates.TryGetValue(id, out var state)) return;
		state.Data = data;
		BuffStates[id] = state;
	}


	/// <summary>
	/// Get which frame does the given buff ends
	/// </summary>
	public int GetBuffEndFrame (int id) => BuffStates.TryGetValue(id, out var state) ? state.EndFrame : -1;


	/// <summary>
	/// Iterate thought all buffs this character currently have
	/// </summary>
	public IEnumerable<Buff> ForAllBuffs () {
		foreach (var (_, state) in BuffStates) {
			yield return state.Buff;
		}
	}


	/// <summary>
	/// Get display name of the given buff from the language system
	/// </summary>
	public static string GetBuffDisplayName (int id) {
		if (!Pool.TryGetValue(id, out var buff)) return "";
		return Language.Get(buff.NameID, buff.TypeName);
	}


	/// <summary>
	/// Get description of the given buff from the language system
	/// </summary>
	public static string GetBuffDescription (int id) {
		if (!Pool.TryGetValue(id, out var buff)) return "";
		return Language.Get(buff.DescriptionID);
	}


	#endregion




}
