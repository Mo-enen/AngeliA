using System.Collections;
using System.Collections.Generic;
using AngeliaFramework;
using UnityEngine;

namespace Yaya {
	public abstract class EntityBehaviour<E> where E : Entity {


		public E Source { get; private set; } = null;


		public virtual void Initialize (E source) {
			Source = source;
		}


		public virtual void Update () { }


	}
}
