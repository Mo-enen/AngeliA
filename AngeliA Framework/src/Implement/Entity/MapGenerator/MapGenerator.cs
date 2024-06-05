using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;



namespace AngeliA;
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
			if (!_HasMapInDisk.HasValue) {
				_HasMapInDisk = Util.HasFileIn(ProcedureMapRoot, true, "*");
			}
			return _HasMapInDisk.Value;
		}
	}
	protected string ProcedureMapRoot => Util.CombinePaths(Universe.BuiltIn.ProcedureMapRoot, GetType().Name);
	protected string TempMapRoot => Util.CombinePaths(AngePath.ProcedureMapTempRoot, GetType().Name);
	protected WorldStream SampleReader { get; private set; } = null;
	protected WorldStream ResultWriter { get; private set; } = null;

	// Data
	private readonly CancellationTokenSource GenerateMapToken = new();
	private System.Threading.Tasks.Task GenerateMapTask = null;
	private bool? _HasMapInDisk = null;


	#endregion




	#region --- MSG ---


	[OnGameInitializeLater]
	public static void OnGameInitializeLater () {
		Util.DeleteFolder(Universe.BuiltIn.ProcedureMapRoot);
		Util.CreateFolder(Universe.BuiltIn.ProcedureMapRoot);
		Util.DeleteFolder(AngePath.ProcedureMapTempRoot);
		Util.CreateFolder(AngePath.ProcedureMapTempRoot);
		foreach (var type in typeof(MapGenerator).AllChildClass()) {
			Util.CreateFolder(Util.CombinePaths(Universe.BuiltIn.ProcedureMapRoot, type.Name));
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
		foreach (string path in Util.EnumerateFolders(Universe.BuiltIn.ProcedureMapRoot, true)) {
			Util.DeleteFolder(path);
			Util.CreateFolder(path);
		}
	}


	public void GenerateAsync () {
		CancelAsyncGeneration();
		GenerateMapTask = System.Threading.Tasks.Task.Factory.StartNew(Generate, GenerateMapToken.Token);
	}


	public void Generate () {
		try {
			string procedureMapRoot = ProcedureMapRoot;
			string tempMapRoot = TempMapRoot;

			SampleReader = WorldStream.GetOrCreateStream(Universe.BuiltIn.MapRoot);
			ResultWriter = new WorldStream();
			ResultWriter.Load(tempMapRoot);

			BeforeMapGenerate();

			Util.DeleteFolder(tempMapRoot);
			Util.CreateFolder(tempMapRoot);

			_HasMapInDisk = false;
			OnMapGenerate();
			_HasMapInDisk = null;

			ResultWriter.SaveAllDirty();

			Util.DeleteFolder(procedureMapRoot);
			Util.MoveFolder(tempMapRoot, procedureMapRoot);
			IUnique.SaveToDisk(procedureMapRoot);
			Stage.SetViewZ(Stage.ViewZ);

			AfterMapGenerate();
		} catch (System.Exception ex) {
			Debug.LogException(ex);
		}
		ResultWriter?.Clear();
		SampleReader = null;
		ResultWriter = null;
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