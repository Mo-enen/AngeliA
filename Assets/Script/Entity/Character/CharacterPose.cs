using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Yaya {
	public abstract class CharacterPose {


		// Api-Pose
		public bool FacingFront { get; set; } = true;
		public bool FacingRight { get; set; } = true;

		// Api
		public int Frame { get; private set; } = 0;
		protected eCharacter Character { get; init; } = null;


		// API
		public CharacterPose (eCharacter character) => Character = character;


		public virtual void CalculatePose (int frame) {
			Frame = frame;
		}


	}
}
