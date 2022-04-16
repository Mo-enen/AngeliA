using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class YayaWorldSquad : WorldSquad {
		public YayaWorldSquad (string mapRoot) : base(mapRoot) { }
		protected override void DrawLevelBlock (World.Block block, int unitX, int unitY) {
			base.DrawLevelBlock(block, unitX, unitY);
			while (block != null) {
				if (CellRenderer.GetSprite(block.TypeID, out var sp) && sp.HasCollider) {
					var rect = new RectInt(unitX * Const.CELL_SIZE, unitY * Const.CELL_SIZE, Const.CELL_SIZE, Const.CELL_SIZE);
					CellPhysics.FillBlock(
						(int)PhysicsLayer.Level,
						rect.Shrink(sp.GlobalBorder.Left, sp.GlobalBorder.Right, sp.GlobalBorder.Down, sp.GlobalBorder.Up),
						sp.IsTrigger,
						sp.Tag
					);
				}
				block = block.Next;
			}
		}
	}


	public class Yaya : Game {


		// Const
		private static readonly int[] ENTITY_CAPACITY = new int[] { 512, 512, 256, 512, 1024 };

		// Api
		//protected override int EntityLayerCount => YayaConst.ENTITY_LAYER_COUNT;
		protected override int PhysicsLayerCount => YayaConst.PHYSICS_LAYER_COUNT;


		// MSG
		protected override void Initialize () {
			base.Initialize();
			Initialize_Misc();
			Initialize_Quit();
		}


		private void Initialize_Misc () {
			YayaConst.GetLanguage = (key) => CurrentLanguage ? CurrentLanguage[key] : "";
		}


		private void Initialize_Quit () {
			bool willQuit = false;
			Application.wantsToQuit += () => {
#if UNITY_EDITOR
				if (UnityEditor.EditorApplication.isPlaying) { return true; }
#endif
				if (willQuit) {
					return true;
				} else {
					// Show Quit Dialog
					var dialog = AddEntity(typeof(eDialog).AngeHash(), 0, 0) as eDialog;
					dialog.Setup(
						2048,
						YayaConst.QuitConfirmContent, YayaConst.LabelQuit, YayaConst.LabelCancel, "",
						() => {
							willQuit = true;
							PlayerData.SaveToDisk();
							Application.Quit();
						},
						() => { },
						null
					);
					return false;
				}
			};
		}


		// Override
		//protected override int GetEntityCapacity (int layer) => ENTITY_CAPACITY[layer.Clamp(0, EntityLayerCount - 1)];
		protected override WorldSquad CreateWorldSquad () => new YayaWorldSquad(MapRoot);


	}
}
