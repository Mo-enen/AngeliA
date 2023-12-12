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




		#region --- VAR ---


		// Api
		public bool IsGenerating => GenerateMapTask != null && GenerateMapTask.Status == TaskStatus.Running;
		public bool HasMapInDisk {
			get {
				if (!_HasMapInDisk.HasValue || _CheckedSlot != AngePath.CurrentSaveSlot) {
					_HasMapInDisk = Util.HasFileIn(MapRoot, true, "*");
					_CheckedSlot = AngePath.CurrentSaveSlot;
				}
				return _HasMapInDisk.Value;
			}
		}
		protected string MapRoot => Util.CombinePaths(AngePath.ProcedureMapRoot, GetType().Name);
		protected string TempMapRoot => Util.CombinePaths(AngePath.ProcedureMapTempRoot, GetType().Name);
		protected WorldStream SampleReader { get; private set; } = null;
		protected WorldStream ResultWriter { get; private set; } = null;

		// Data
		private readonly CancellationTokenSource GenerateMapToken = new();
		private Task GenerateMapTask = null;
		private bool? _HasMapInDisk = null;
		private int _CheckedSlot = -1;


		#endregion




		#region --- MSG ---


		[OnSlotChanged]
		public static void OnSlotChanged () {
			Util.DeleteFolder(AngePath.ProcedureMapRoot);
			Util.CreateFolder(AngePath.ProcedureMapRoot);
			Util.DeleteFolder(AngePath.ProcedureMapTempRoot);
			Util.CreateFolder(AngePath.ProcedureMapTempRoot);
			foreach (var type in typeof(MapGenerator).AllChildClass()) {
				Util.CreateFolder(Util.CombinePaths(AngePath.ProcedureMapRoot, type.Name));
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
				string mapRoot = MapRoot;
				string tempMapRoot = TempMapRoot;

				SampleReader = new WorldStream(WorldSquad.MapRoot, WorldSquad.Channel.GetLocation(), @readonly: true, isProcedure: false);
				ResultWriter = new WorldStream(tempMapRoot, MapLocation.Procedure, @readonly: false, isProcedure: true);

				BeforeMapGenerate();

				Util.DeleteFolder(tempMapRoot);
				Util.CreateFolder(tempMapRoot);

				_HasMapInDisk = false;
				GenerateMap();
				_HasMapInDisk = null;

				SampleReader.Dispose();
				ResultWriter.Dispose();

				Util.DeleteFolder(mapRoot);
				Util.MoveFolder(tempMapRoot, mapRoot);
				WorldSquad.Front.ForceReloadDelay();
				WorldSquad.Behind.ForceReloadDelay();
				Stage.SetViewZ(Stage.ViewZ);

				IGlobalPosition.CreateMetaFileFromMaps(mapRoot);

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