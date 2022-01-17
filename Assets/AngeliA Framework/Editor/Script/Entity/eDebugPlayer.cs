using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework.Entities.Editor {
	public class eDebugPlayer : ePlayer {

		
		public override CharacterMovement Movement {
			get {
				if (_Movement == null) {
					_Movement = GetAsset("Debug Character Movement".ACode()) as CharacterMovement;
					if (_Movement == null) {
						_Movement = new();
					}
				}
				return _Movement;
			}
		}
		public override CharacterRenderer Renderer {
			get {
				if (_Renderer == null) {
					_Renderer = GetAsset("Debug Character Renderer".ACode()) as CharacterRenderer;
					if (_Renderer == null) {
						_Renderer = new();
					}
				}
				return _Renderer;
			}
		}

		[SerializeField] private CharacterMovement _Movement = null;
		[SerializeField] private CharacterRenderer _Renderer = null;


		public override void FrameUpdate (int frame) {



			base.FrameUpdate(frame);
		}


	}
}
