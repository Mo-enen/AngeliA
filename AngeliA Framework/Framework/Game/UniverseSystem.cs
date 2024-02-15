using System.Collections;
using System.Collections.Generic;


namespace AngeliA.Framework; 



[System.AttributeUsage(System.AttributeTargets.Method)]
public class OnUniverseOpenAttribute : OrderedAttribute { public OnUniverseOpenAttribute (int order = 0) : base(order) { } }



public static class UniverseSystem {

	// Event
	private static event System.Action OnUniverseOpen;

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
		CurrentUniverse = BuiltInUniverse = new(
			Util.CombinePaths(AngePath.BuiltInUniverseRoot),
			Util.CombinePaths(AngePath.BuiltInSavingRoot),
			@readonly: !Game.IsEdittime
		);

		// Fill User Universes
		UserUniverses.Clear();
		Util.CreateFolder(AngePath.WorkspaceRoot);
		foreach (var folder in Util.EnumerateFolders(AngePath.WorkspaceRoot, true)) {
			UserUniverses.Add(new Universe(folder, @readonly: false));
		}
		SortUniverseList(UserUniverses);

		// Fill Downloaded Universes
		DownloadedUniverses.Clear();
		Util.CreateFolder(AngePath.DownloadRoot);
		foreach (var folder in Util.EnumerateFolders(AngePath.DownloadRoot, true)) {
			DownloadedUniverses.Add(new Universe(folder, @readonly: true));
		}

	}

	[OnGameInitialize(int.MaxValue)]
	public static void OnGameInitializeMax () => OpenUniverse(BuiltInUniverse);

	// API
	public static void OpenUniverse (Universe universe, bool ignoreCallback = false) {
		if (universe == null) return;
		// Open
		CurrentUniverse = universe;
		universe.CreateFolders();
		universe.Info.ModifyDate = System.DateTime.Now.ToFileTime();
		// Callback
		if (!ignoreCallback) OnUniverseOpen?.Invoke();
		// Sort
		if (universe != BuiltInUniverse && UserUniverses.Contains(universe)) {
			SortUniverseList(UserUniverses);
		}
	}

	private static void SortUniverseList (List<Universe> universes) => universes.Sort((a, b) => b.Info.ModifyDate.CompareTo(a.Info.ModifyDate));

}