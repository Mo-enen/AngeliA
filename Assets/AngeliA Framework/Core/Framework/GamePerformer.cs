using UnityEngine;
namespace AngeliaFramework {
	public abstract class GamePerformer : MonoBehaviour {
		protected Game Game => m_Game;
		[SerializeField] Game m_Game = null;
		private bool Initialized = false;
		private void FixedUpdate () {
			if (!Initialized) {
				Initialized = true;
				m_Game.Initialize();
			}
			m_Game.FrameUpdate();
		}
	}
}