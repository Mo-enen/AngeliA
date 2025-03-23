using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace AngeliA;

/// <summary>
/// Represent a type of buff
/// </summary>
[EntityAttribute.MapEditorGroup("Buff")]
public abstract class Buff : IMapItem {




	#region --- VAR ---


	// Api
	/// <summary>
	/// Unique angehash of this buff class
	/// </summary>
	public int TypeID { get; init; }
	/// <summary>
	/// Unique angehash for language system to get the display name of this buff
	/// </summary>
	public int NameID { get; init; }
	/// <summary>
	/// Unique angehash for language system to get the description of this buff
	/// </summary>
	public int DescriptionID { get; init; }
	/// <summary>
	/// AngeName of this type of buff
	/// </summary>
	public string TypeName { get; init; }
	/// <summary>
	/// How many frames should this buff apply by default
	/// </summary>
	public abstract int DefaultDuration { get; }


	#endregion




	#region --- MSG ---


	[OnGameInitialize]
	internal static void OnGameInitialize () {
		var cheatMethodInfo = typeof(Buff).GetMethod(nameof(CheatMethod), BindingFlags.Static | BindingFlags.NonPublic);
		foreach (var type in typeof(Buff).AllChildClass()) {
			CheatSystem.TryAddCheatAction($"Give{type.AngeName()}", cheatMethodInfo, type.AngeHash());
		}
	}


	
	public Buff () {
		string name = GetType().AngeName();
		TypeID = name.AngeHash();
		NameID = $"iName.{name}".AngeHash();
		DescriptionID = $"iDes.{name}".AngeHash();
		TypeName = name;
	}


	
	public virtual void BeforeUpdate (Character target) { }
	
	public virtual void LateUpdate (Character target) { }
	
	public virtual void OnCharacterAttack (Character target, Bullet bullet) { }
	
	public virtual void OnCharacterRenderered (CharacterRenderer renderer) { }


	private static void CheatMethod () {
		if (CheatSystem.CurrentParam is not int buffID) return;
		if (PlayerSystem.Selecting == null) return;
		PlayerSystem.Selecting.Buff.GiveBuff(buffID);
	}


	#endregion




}
