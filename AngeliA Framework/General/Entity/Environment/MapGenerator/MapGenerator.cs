using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;


namespace AngeliaFramework {
	public abstract class ActionMapGenerator : MapGenerator, IActionTarget {

		private static readonly int HINT_GENERATE = "CtrlHint.MapGenerator.Generate".AngeHash();
		private static readonly int HINT_GENERATING = "CtrlHint.MapGenerator.Generating".AngeHash();
		private readonly CellContent HintContent = new() {
			Alignment = Alignment.MidMid,
			BackgroundTint = Const.BLACK,
			TightBackground = true,
			Wrap = false,
			Clip = false,
		};
		protected virtual bool UseGeneratingHint => true;

		public override void OnActivated () {
			base.OnActivated();

		}

		public override void FillPhysics () {
			base.FillPhysics();
			CellPhysics.FillEntity(PhysicsLayer.ENVIRONMENT, this, true);
		}

		public override void FrameUpdate () {
			base.FrameUpdate();
			var cell = DrawArtwork();
			if (!IsGenerating) {
				// Normal
				if ((this as IActionTarget).IsHighlighted) {
					IActionTarget.HighlightBlink(cell, Direction3.None, FittingPose.Single);
					// Hint
					ControlHintUI.DrawGlobalHint(X, Y + Const.CEL * 2 + Const.HALF, Gamekey.Action, Language.Get(HINT_GENERATE, "Generate Map"), true);
				}
			} else {
				// Generating
				if (UseGeneratingHint) {
					CellRendererGUI.Label(
						HintContent.SetText(Language.Get(HINT_GENERATING, "Generating")),
						new RectInt(X - Const.CEL, Y + Const.CEL * 2, Const.CEL * 3, Const.CEL)
					);
				}
			}
		}

		protected virtual Cell DrawArtwork () => CellRenderer.Draw(TypeID, Rect);

		void IActionTarget.Invoke () => GenerateAsync();

		bool IActionTarget.AllowInvoke () => !IsGenerating && !HasMapInDisk;

	}


	[EntityAttribute.DontDrawBehind]
	[EntityAttribute.Capacity(1, 0)]
	[EntityAttribute.MapEditorGroup("MapGenerator")]
	[EntityAttribute.ForceSpawn]
	[EntityAttribute.Layer(EntityLayer.GAME)]
	public abstract class MapGenerator : Entity {

		public bool IsGenerating => GenerateMapTask != null && GenerateMapTask.Status == TaskStatus.Running;
		public bool HasMapInDisk {
			get {
				if (!_HasMapInDisk.HasValue) _HasMapInDisk = Util.HasFileIn(MapRoot, true, "*");
				return _HasMapInDisk.Value;
			}
		}

		private readonly CancellationTokenSource GenerateMapToken = new();
		private Task GenerateMapTask = null;
		private bool? _HasMapInDisk = null;
		protected readonly string MapRoot = "";
		protected readonly string TempMapRoot = "";

		public MapGenerator () {
			MapRoot = Util.CombinePaths(AngePath.ProcedureMapRoot, GetType().Name);
			TempMapRoot = Util.CombinePaths(AngePath.ProcedureMapTempRoot, GetType().Name);
			_HasMapInDisk = null;
		}

		public void GenerateAsync () {
			if (GenerateMapTask != null && !GenerateMapTask.IsCompleted && GenerateMapToken.Token.CanBeCanceled) {
				GenerateMapToken.Cancel();
			}
			GenerateMapTask = Task.Factory.StartNew(Generate, GenerateMapToken.Token);
		}

		public void Generate () {
			Util.DeleteFolder(TempMapRoot);
			Util.CreateFolder(TempMapRoot);
			Util.DeleteFolder(MapRoot);
			Util.CreateFolder(MapRoot);
			_HasMapInDisk = null;
			GenerateMap();
		}

		protected abstract void GenerateMap ();

	}
}