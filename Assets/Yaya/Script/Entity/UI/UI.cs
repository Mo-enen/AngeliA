using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using AngeliaFramework;


namespace Yaya {
	[EntityAttribute.DontDespawnWhenOutOfRange]
	[EntityAttribute.DontDestroyOnSquadTransition]
	public abstract class ScreenUI : UiEntity { }



	[EntityAttribute.ExcludeInMapEditor]
	[EntityAttribute.ForceUpdate]
	public abstract class UiEntity : Entity, IInitialize {


		public int LocalFrame => Game.GlobalFrame - ActiveFrame;
		private int ActiveFrame = 0;
		private static readonly Dictionary<Key, int> KeyIdMap = new();


		public static void Initialize () {
			KeyIdMap.Clear();
			foreach (var key in System.Enum.GetValues(typeof(Key))) {
				KeyIdMap.TryAdd((Key)key, $"k_{key}".AngeHash());
			}
		}


		public override void OnActived () {
			base.OnActived();
			ActiveFrame = Game.GlobalFrame;
		}


		public override void FrameUpdate () {
			base.FrameUpdate();
			CellRenderer.SetLayer(YayaConst.SHADER_UI);
			UpdateForUI();
			CellRenderer.SetLayerToDefault();
		}


		protected abstract void UpdateForUI ();


		protected int GetKeyID (Key key) => KeyIdMap.TryGetValue(key, out int id) ? id : 0;


	}
}