using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

[EntityAttribute.MapEditorGroup("Buff")]
public abstract class Buff : IMapItem {




	#region --- VAR ---


	// Api
	public int TypeID { get; init; }
	public int NameID { get; init; }
	public int DescriptionID { get; init; }
	public string TypeName { get; init; }
	public abstract int DefaultDuration { get; }


	#endregion




	#region --- MSG ---


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


	#endregion




}
