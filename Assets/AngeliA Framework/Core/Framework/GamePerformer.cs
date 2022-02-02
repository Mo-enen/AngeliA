using UnityEngine;
namespace AngeliaFramework {
	public class GamePerformer : MonoBehaviour {


		public Game Game => m_Game;

		[SerializeField] Game m_Game = null;

		private bool Initialized = false;


		[RuntimeInitializeOnLoadMethod]
		private static void Init () {
			var games = Resources.FindObjectsOfTypeAll<Game>();
			if (games != null && games.Length > 0) {
				var game = games[0];
				if (game != null) {
					var tf = new GameObject("Game", typeof(GamePerformer)).transform;
					tf.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
					tf.localScale = Vector3.one;
					tf.GetComponent<GamePerformer>().m_Game = game;
				}
			}
		}


		private void FixedUpdate () {
			if (!Initialized) {
				Initialized = true;
				m_Game.Initialize();
			}
			m_Game.FrameUpdate();
		}


	}
}