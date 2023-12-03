using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;


namespace AngeliaFramework {
	[EntityAttribute.DontDrawBehind]
	[EntityAttribute.Capacity(1, 0)]
	[EntityAttribute.MapEditorGroup("MapGenerator")]
	[EntityAttribute.ForceSpawn]
	[EntityAttribute.Layer(EntityLayer.GAME)]
	public abstract class MapGenerator : Entity {




		#region --- SUB ---


		[EntityAttribute.MapEditorGroup("MapGenerator")]
		public class MapGenerator_Room : IMapEditorItem { }

		[EntityAttribute.MapEditorGroup("MapGenerator")]
		public class MapGenerator_Door : IMapEditorItem { }


		#endregion




		#region --- VAR ---


		// Api
		public bool IsGenerating => GenerateMapTask != null && GenerateMapTask.Status == TaskStatus.Running;
		public bool HasMapInDisk {
			get {
				if (!_HasMapInDisk.HasValue) _HasMapInDisk = Util.HasFileIn(MapRoot, true, "*");
				return _HasMapInDisk.Value;
			}
		}
		protected string MapRoot { get; init; } = "";
		protected string TempMapRoot { get; init; } = "";
		protected WorldStream SampleReader { get; private set; } = null;
		protected WorldStream ResultWriter { get; private set; } = null;

		// Data
		private readonly CancellationTokenSource GenerateMapToken = new();
		private Task GenerateMapTask = null;
		private bool? _HasMapInDisk = null;


		#endregion




		#region --- MSG ---


		[OnGameInitialize(-256)]
		public static void OnGameInitialize () {
			Util.DeleteFolder(AngePath.ProcedureMapRoot);
			Util.CreateFolder(AngePath.ProcedureMapRoot);
			Util.DeleteFolder(AngePath.ProcedureMapTempRoot);
			Util.CreateFolder(AngePath.ProcedureMapTempRoot);
			foreach (var type in typeof(MapGenerator).AllChildClass()) {
				Util.CreateFolder(Util.CombinePaths(AngePath.ProcedureMapRoot, type.Name));
				Util.CreateFolder(Util.CombinePaths(AngePath.ProcedureMapTempRoot, type.Name));
			}
		}


		public MapGenerator () {
			MapRoot = Util.CombinePaths(AngePath.ProcedureMapRoot, GetType().Name);
			TempMapRoot = Util.CombinePaths(AngePath.ProcedureMapTempRoot, GetType().Name);
			_HasMapInDisk = null;
		}


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
			foreach (string path in Util.EnumerateFolders(AngePath.ProcedureMapRoot, true)) {
				Util.DeleteFolder(path);
				Util.CreateFolder(path);
			}
		}


		public void GenerateAsync () {
			CancelAsyncGeneration();
			GenerateMapTask = Task.Factory.StartNew(Generate, GenerateMapToken.Token);
		}


		public void Generate () {
			try {
				BeforeMapGenerate();

				SampleReader = new WorldStream(WorldSquad.MapRoot, WorldSquad.Channel.GetLocation(), @readonly: true, isProcedure: true);
				ResultWriter = new WorldStream(TempMapRoot, MapLocation.Procedure, @readonly: false, isProcedure: true);

				Util.DeleteFolder(TempMapRoot);
				Util.CreateFolder(TempMapRoot);

				_HasMapInDisk = false;
				GenerateMap();
				_HasMapInDisk = null;

				SampleReader.Dispose();
				ResultWriter.Dispose();

				Util.DeleteFolder(MapRoot);
				Util.MoveFolder(TempMapRoot, MapRoot);
				WorldSquad.Front.ForceReloadDelay();
				WorldSquad.Behind.ForceReloadDelay();

				AfterMapGenerate();
			} catch (System.Exception ex) {
				SampleReader?.Clear();
				ResultWriter?.Clear();
				Debug.LogException(ex);
			}
		}


		protected abstract void GenerateMap ();
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