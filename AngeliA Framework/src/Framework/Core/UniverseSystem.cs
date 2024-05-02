using System.Collections;
using System.Collections.Generic;


namespace AngeliA;


[System.AttributeUsage(System.AttributeTargets.Method)]
public class OnUniverseOpenAttribute : OrderedAttribute { public OnUniverseOpenAttribute (int order = 0) : base(order) { } }

[System.AttributeUsage(System.AttributeTargets.Method)]
public class BeforeUniverseOpenAttribute : OrderedAttribute { public BeforeUniverseOpenAttribute (int order = 0) : base(order) { } }


public static class UniverseSystem {


	// Event
	private static event System.Action OnUniverseOpen;
	private static event System.Action BeforeUniverseOpen;

	// Api
	public static Universe CurrentUniverse { get; private set; } = null;
	public static Universe BuiltInUniverse { get; private set; } = null;
	public static List<Universe> UserUniverses { get; } = new();
	public static List<Universe> DownloadedUniverses { get; } = new();


	// MSG
	[OnGameInitialize(int.MinValue)]
	public static void OnGameInitializeMin () {

		Util.LinkEventWithAttribute<OnUniverseOpenAttribute>(typeof(UniverseSystem), nameof(OnUniverseOpen));

		// Load BuiltIn Universe
#if DEBUG
		bool _readonly = false;
#else
		bool _readonly = true;
#endif

		CurrentUniverse = BuiltInUniverse = Universe.LoadUniverse(
			AngePath.BuiltInUniverseRoot,
			@readonly: _readonly,
			useBuiltInSavingRoot: true
		);

		OpenUniverse(BuiltInUniverse, ignoreCallback: true);

		// Fill User Universes
		UserUniverses.Clear();
		Util.CreateFolder(AngePath.WorkspaceRoot);
		foreach (var folder in Util.EnumerateFolders(AngePath.WorkspaceRoot, true)) {
			UserUniverses.Add(Universe.LoadUniverse(
				AngePath.GetUniverseRoot(folder),
				@readonly: false
			));
		}
		SortUniverseList(UserUniverses);

		// Fill Downloaded Universes
		DownloadedUniverses.Clear();
		Util.CreateFolder(AngePath.DownloadRoot);
		foreach (var folder in Util.EnumerateFolders(AngePath.DownloadRoot, true)) {
			DownloadedUniverses.Add(Universe.LoadUniverse(
				AngePath.GetUniverseRoot(folder),
				@readonly: true
			));
		}

	}


	[OnGameInitialize(int.MaxValue)]
	public static void OnGameInitializeMax () {
		BeforeUniverseOpen?.Invoke();
		OnUniverseOpen?.Invoke();
	}


	// API
	public static void OpenUniverse (Universe universe, bool ignoreCallback = false) {

		if (universe == null) return;

		if (!ignoreCallback) BeforeUniverseOpen?.Invoke();

		CurrentUniverse = universe;
		universe.CreateFolders();
		universe.Info.ModifyDate = System.DateTime.Now.ToFileTime();
		if (universe != BuiltInUniverse && UserUniverses.Contains(universe)) {
			SortUniverseList(UserUniverses);
		}
		AngePath.SetCurrentUserPath(universe.Info.DeveloperName, universe.Info.ProductName);

		if (!ignoreCallback) OnUniverseOpen?.Invoke();

	}


	private static void SortUniverseList (List<Universe> universes) => universes.Sort((a, b) => b.Info.ModifyDate.CompareTo(a.Info.ModifyDate));


}