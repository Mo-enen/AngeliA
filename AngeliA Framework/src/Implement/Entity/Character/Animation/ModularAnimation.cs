using System.Collections;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AngeliA;

[System.Serializable]
public sealed class ModularAnimation : PoseAnimation {




	#region --- SUB ---


	public enum CharacterOverrideType { None, Pose, Handheld, Attack, }


	#endregion




	#region --- VAR ---


	[JsonIgnore] public int ID;
	[JsonIgnore] public string Name = string.Empty;
	public string CharacterName = string.Empty;
	public CharacterOverrideType Override = CharacterOverrideType.None;
	public CharacterAnimationType PoseType = CharacterAnimationType.Idle;
	public WeaponHandheld Handheld = WeaponHandheld.SingleHanded;
	public WeaponType AttackType = WeaponType.Hand;


	#endregion




	#region --- MSG ---




	#endregion




	#region --- API ---



	#endregion




	#region --- LGC ---



	#endregion




}
