using UnityEngine;
namespace AngeliaFramework {
	public abstract class GamePerformer : MonoBehaviour {
		public Game Game { get; private set; } = null;
		protected GameData GameData => m_GameData;
		[SerializeField] GameData m_GameData = null;
		private bool Initialized = false;
		private void FixedUpdate () {
			if (!Initialized) {
				Initialized = true;
				Game = new Game(m_GameData);
			}
			Game.FrameUpdate();
		}
	}
}