using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public abstract class Buff {




	#region --- VAR ---


	// Api
	public int TypeID { get; init; }
	public int NameID { get; init; }
	public int DescriptionID { get; init; }
	public string TypeName { get; init; }


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


	#endregion




}
