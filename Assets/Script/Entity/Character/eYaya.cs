using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {


	public class YayaRenderer : CharacterRenderer {
		public YayaRenderer (eCharacter ch) : base(ch) { }
		public override void FrameUpdate (CharacterPose pose) {
			base.FrameUpdate(pose);





		}
	}


	public class YayaPose : CharacterPose {
		public YayaPose (eCharacter character) : base(character) { }
		public override void CalculatePose (int frame) {
			base.CalculatePose(frame);




		}
	}


	public class eYaya : ePlayer {

		public override CharacterMovement Movement => _Movement ??= new(this) {

		};
		public override CharacterRenderer Renderer => _Renderer ??= new(this) {

		};
		public override CharacterPose Pose => _Pose ??= new(this) {

		};

		[AngeliaInspector] CharacterMovement _Movement = null;
		[AngeliaInspector] YayaRenderer _Renderer = null;
		[AngeliaInspector] YayaPose _Pose = null;

	}
}
