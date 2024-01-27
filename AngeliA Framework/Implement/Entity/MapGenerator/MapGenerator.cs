using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;


namespace AngeliaFramework {
	[EntityAttribute.DontDrawBehind]
	[EntityAttribute.Capacity(1, 0)]
	[EntityAttribute.MapEditorGroup("MapGenerator")]
	[EntityAttribute.ForceSpawn]
	[EntityAttribute.Layer(EntityLayer.GAME)]
	[RequireSprite("{0}")]
	public abstract class MapGenerator : Entity {




		#region --- VAR ---


		// Api
		public bool IsGenerating => GenerateMapTask != null && GenerateMapTask.Status == TaskStatus.Running;
		public bool HasMapInDisk {
			get {
				if (!_HasMapInDisk.HasValue) {
					_HasMapInDisk = Util.HasFileIn(MapRoot, true, "*");
				}
				return _HasMapInDisk.Value;
			}
		}
		protected string MapRoot => Util.CombinePaths(ProjectSystem.CurrentProject.ProcedureMapRoot, GetType().Name);
		protected string TempMapRoot => Util.CombinePaths(AngePath.ProcedureMapTempRoot, GetType().Name);
		protected WorldStream SampleReader { get; private set; } = null;
		protected WorldStream ResultWriter { get; private set; } = null;

		// Data
		private readonly CancellationTokenSource GenerateMapToken = new();
		private Task GenerateMapTask = null;
		private bool? _HasMapInDisk = null;


		#endregion




		#region --- MSG ---


		[OnProjectOpen]
		public static void OnGameInitialize () {
			Util.DeleteFolder(ProjectSystem.CurrentProject.ProcedureMapRoot);
			Util.CreateFolder(ProjectSystem.CurrentProject.ProcedureMapRoot);
			Util.DeleteFolder(AngePath.ProcedureMapTempRoot);
			Util.CreateFolder(AngePath.ProcedureMapTempRoot);
			foreach (var type in typeof(MapGenerator).AllChildClass()) {
				Util.CreateFolder(Util.CombinePaths(ProjectSystem.CurrentProject.ProcedureMapRoot, type.Name));
				Util.CreateFolder(Util.CombinePaths(AngePath.ProcedureMapTempRoot, type.Name));
			}
		}


		public MapGenerator () => _HasMapInDisk = null;


		public override void OnActivated () {
			base.OnActivated();
			_HasMapInDisk = null;
		}


		#endregion




		#region --- API ---


		public static void DeleteAllGeneratedMapFiles () {
			// Cancel Async
			foreach (var generator in Stage.ForAllActiveEntities<MapGenerator>()) {
				generator.CancelAsyncGeneration();
			}
			// Delete Files
			foreach (string path in Util.EnumerateFolders(ProjectSystem.CurrentProject.ProcedureMapRoot, true)) {
				Util.DeleteFolder(path);
				Util.CreateFolder(path);
			}
		}


		public void StartGenerateAsync () {
			CancelAsyncGeneration();
			GenerateMapTask = Task.Factory.StartNew(StartGenerate, GenerateMapToken.Token);
		}


		public void StartGenerate () {
			try {
				string mapRoot = MapRoot;
				string tempMapRoot = TempMapRoot;

				SampleReader = new WorldStream(WorldSquad.MapRoot, @readonly: true);
				ResultWriter = new WorldStream(tempMapRoot, @readonly: false);

				BeforeMapGenerate();

				Util.DeleteFolder(tempMapRoot);
				Util.CreateFolder(tempMapRoot);

				_HasMapInDisk = false;
				OnMapGenerate();
				_HasMapInDisk = null;

				SampleReader.Dispose();
				ResultWriter.Dispose();

				Util.DeleteFolder(mapRoot);
				Util.MoveFolder(tempMapRoot, mapRoot);
				IGlobalPosition.SaveToDisk(mapRoot);
				WorldSquad.Front.ForceReloadDelay();
				WorldSquad.Behind.ForceReloadDelay();
				Stage.SetViewZ(Stage.ViewZ);

				AfterMapGenerate();
			} catch (System.Exception ex) {
				SampleReader?.Clear();
				ResultWriter?.Clear();
				Game.LogException(ex);
			}
		}


		protected abstract void OnMapGenerate ();
		protected virtual void BeforeMapGenerate () { }
		protected virtual void AfterMapGenerate () { }


		#endregion




		#region --- LGC ---


		private void CancelAsyncGeneration () {
			if (GenerateMapTask != null && !GenerateMapTask.IsCompleted && GenerateMapToken.Token.CanBeCanceled) {
				GenerateMapToken.Cancel();
			}
		}


		#endregion




	}
}