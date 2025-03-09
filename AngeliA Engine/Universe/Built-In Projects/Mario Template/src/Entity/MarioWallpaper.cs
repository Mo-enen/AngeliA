using System.Collections;
using System.Collections.Generic;
using AngeliA;
using AngeliA.Platformer;

namespace MarioTemplate;

[EntityAttribute.ExcludeInMapEditor]
[EntityAttribute.Capacity(1, 1)]
public class MarioWallpaper : Wallpaper {


	// VAR
	private static readonly int TYPE_ID = typeof(MarioWallpaper).AngeHash();
	private static readonly SpriteCode CLOUD_SP = "Wallpaper.Cloud";
	private static readonly SpriteCode MOUNTAIN_SP = "Wallpaper.Mountain";


	// MSG
	[OnGameInitializeLater]
	internal static void OnGameInitializeLater () => Stage.SpawnEntity(TYPE_ID, 0, 0);


	[OnMapEditorModeChange_Mode]
	internal static void OnMapEditorModeChange (OnMapEditorModeChange_ModeAttribute.Mode mode) {
		if (mode == OnMapEditorModeChange_ModeAttribute.Mode.EnterPlayMode) {
			Stage.SpawnEntity(TYPE_ID, 0, 0);
		}
	}


	[OnGameUpdatePauseless]
	internal static void OnGameUpdatePauseless () {
		if (MapEditor.IsEditing) {
			Sky.ForceSkyboxTint(new Color32(63, 70, 162), new Color32(83, 105, 201));
		} else {
			Sky.ForceSkyboxTint(new Color32(118, 133, 253));
		}
	}


	protected override void DrawBackground (IRect backgroundRect) {

		// Cloud
		if (Renderer.TryGetSpriteGroup(CLOUD_SP, out var cloudGroup)) {
			const int CLOUD_COUNT = 12;
			for (int i = 0; i < CLOUD_COUNT; i++) {
				var sp = cloudGroup.Sprites[i % cloudGroup.Count];

				var cameraRect = Renderer.CameraRect.Expand(sp.GlobalWidth, 0, sp.GlobalHeight, 0);

				int basicX = Util.QuickRandomWithSeed(
					348956 + i * 18, 0, 1000
				) * cameraRect.width / 1000;
				int basicY = Util.QuickRandomWithSeed(
					8921367 + i * 621, 0, 1000
				) * cameraRect.height / 1000;

				int paraX = (cameraRect.x / 32);
				int paraY = (cameraRect.y / 32);

				int aniX = Game.GlobalFrame * Util.QuickRandomWithSeed(5612356 + i * 5, 100, 200) / 100;

				Renderer.Draw(
					sp,
					cameraRect.x + (basicX + paraX - aniX).UMod(cameraRect.width),
					cameraRect.y + (basicY + paraY).UMod(cameraRect.height),
					0, 0, 0,
					sp.GlobalWidth, sp.GlobalHeight
				);
			}
		}

		// Mountain
		if (Renderer.TryGetSprite(MOUNTAIN_SP, out var mountainSp)) {
			const int MOUNTAIN_COUNT = 3;
			var cameraRect = Renderer.CameraRect.Expand(mountainSp.GlobalWidth / 2, mountainSp.GlobalWidth / 2, 0, 0);
			for (int i = 0; i < MOUNTAIN_COUNT; i++) {

				int basicX = Util.QuickRandomWithSeed(
					782345 + i * 62345, 0, 1000
				) * cameraRect.width / 1000;

				int paraX = cameraRect.x / 16;

				int scale = Util.QuickRandomWithSeed(675434 + i * 345, 1300, 1800);

				Renderer.Draw(
					mountainSp,
					cameraRect.x + (basicX + paraX).UMod(cameraRect.width),
					cameraRect.y,
					500, 0, 0,
					mountainSp.GlobalWidth * scale / 1000,
					mountainSp.GlobalHeight * scale / 1000
				);
			}
		}

	}


}
