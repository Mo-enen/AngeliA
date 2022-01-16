using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework.Entities.Editor {
	public class eDebugPlayer : ePlayer {


		public override CharacterMovement Movement {
			get {
				if (_Movement == null) {
					_Movement = GetAsset("Debug Movement".ACode()) as CharacterMovement;
					if (_Movement == null) {
						_Movement = new();
					}
				}
				return _Movement;
			}
		}

		[SerializeField] private CharacterMovement _Movement = null;





	}
}
