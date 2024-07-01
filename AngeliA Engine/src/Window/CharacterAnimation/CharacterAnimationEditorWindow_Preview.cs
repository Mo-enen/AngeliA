using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliaEngine;

public partial class CharacterAnimationEditorWindow {




	#region --- SUB ---


	private sealed class PreviewCharacter : PoseCharacter {

		protected override void RenderBodyGadgets () {
			Instance.Preview_Wing.DrawGadget(this);
			Instance.Preview_Tail.DrawGadget(this);
			Instance.Preview_Face.DrawGadget(this);
			Instance.Preview_Hair.DrawGadget(this);
			Instance.Preview_Ear.DrawGadget(this);
			Instance.Preview_Horn.DrawGadget(this);
		}

		protected override void RenderCloths () {
			Instance.PreviewCloth_Head.DrawCloth(this);
			Instance.PreviewCloth_Body.DrawCloth(this);
			Instance.PreviewCloth_Hip.DrawCloth(this);
			Instance.PreviewCloth_Hand.DrawCloth(this);
			Instance.PreviewCloth_Foot.DrawCloth(this);
		}

		protected override void PerformPoseAnimation () {
			Instance.Animation?.Animate(Instance.Preview);
		}

		protected override void RenderEquipmentAndInventory () {
			var weapon = Instance.PreviewWeapon;
			if (weapon == null) {
				OverridePoseHandheldAnimation(EquippingWeaponHeld, 0, 2);
				return;
			}
			EquippingWeaponHeld = weapon.Handheld;
			EquippingWeaponType = weapon.WeaponType;
			OverridePoseHandheldAnimation(EquippingWeaponHeld, 0, 2);
			int startIndex = Renderer.GetUsedCellCount();
			weapon.PoseAnimationUpdate_FromEquipment(this);
			if (Renderer.GetCells(out var cells, out int count)) {
				for (int i = startIndex; i < count; i++) {
					cells[i].Z = 512;
				}
			}
		}

	}


	private sealed class PreviewAxe : Weapon {
		public override WeaponType WeaponType => WeaponType.Axe;
		public override WeaponHandheld Handheld => WeaponHandheld.SingleHanded;
		public PreviewAxe () : base(false) { }
	}
	private sealed class PreviewSword : Weapon {
		public override WeaponType WeaponType => WeaponType.Sword;
		public override WeaponHandheld Handheld => WeaponHandheld.DoubleHanded;
		public PreviewSword () : base(false) { }
	}


	#endregion




	#region --- VAR ---


	// Const
	private static readonly SpriteCode ICON_CHAR_PREVIEW = "Icon.PreviewCharacter";
	private static readonly SpriteCode ICON_ZOOM_M = "Icon.CharPreviewZoomMin";
	private static readonly SpriteCode ICON_ZOOM_P = "Icon.CharPreviewZoomPlus";
	private static readonly SpriteCode ICON_FLIP = "Icon.CharPreviewFlip";
	private static readonly SpriteCode ICON_WP = "Icon.CharAni.Weapon";
	private static readonly (Weapon weapon, string defaultName)[] PREVIEW_WEAPONS = {
		(null, ""),
		(new PreviewAxe(), nameof(PreviewAxe)),
		(new PreviewSword(), nameof(PreviewSword)),
	};
	private static readonly LanguageCode TIP_PREVIEW = ("Tip.PreviewChar", "Select a character for preview the animation");

	// Data
	private readonly Dictionary<int, CharacterRenderingConfig> ConfigPool = new();
	private readonly List<string> AllRigCharacterNames;
	private readonly PreviewCharacter Preview = new() { Active = true, };
	private readonly ModularFace Preview_Face = new();
	private readonly ModularHorn Preview_Horn = new();
	private readonly ModularWing Preview_Wing = new();
	private readonly ModularTail Preview_Tail = new();
	private readonly ModularEar Preview_Ear = new();
	private readonly ModularHair Preview_Hair = new();
	private readonly ModularHeadSuit PreviewCloth_Head = new();
	private readonly ModularBodySuit PreviewCloth_Body = new();
	private readonly ModularHipSuit PreviewCloth_Hip = new();
	private readonly ModularHandSuit PreviewCloth_Hand = new();
	private readonly ModularFootSuit PreviewCloth_Foot = new();
	private Weapon PreviewWeapon = null;
	private string PreviewCharacterName = "";
	private bool PreviewInitialized = false;
	private int PreviewZoom = 1000;


	#endregion




	#region --- MSG ---


	private void Update_Preview_Toolbar (IRect toolbarRect) {

		int padding = Unify(6);

		// BG
		GUI.DrawSlice(EngineSprite.UI_TOOLBAR, toolbarRect);
		toolbarRect = toolbarRect.Shrink(Unify(6));
		var rect = toolbarRect.EdgeLeft(toolbarRect.height);

		// Preview Char
		if (GUI.Button(rect, ICON_CHAR_PREVIEW, Skin.SmallDarkButton)) {
			ShowPreviewCharacterMenu(rect);
		}
		RequireTooltip(rect, TIP_PREVIEW);
		rect.SlideRight(padding);

		// Weapon
		if (GUI.Button(rect, ICON_WP, Skin.SmallDarkButton)) {
			ShowPreviewWeaponMenu(rect);
		}
		rect.SlideRight(padding);

		// Flip
		if (GUI.Button(rect, ICON_FLIP, Skin.SmallDarkButton)) {
			Preview.FacingRight = !Preview.FacingRight;
		}
		rect.SlideRight(padding);

		// Zoom Button -
		if (GUI.Button(rect, ICON_ZOOM_M, Skin.SmallDarkButton)) {
			PreviewZoom = (PreviewZoom - 100).Clamp(500, 1500);
			LastPreviewZoom.Value = PreviewZoom;
		}
		rect.SlideRight(padding);

		// Zoom Button +
		if (GUI.Button(rect, ICON_ZOOM_P, Skin.SmallDarkButton)) {
			PreviewZoom = (PreviewZoom + 100).Clamp(500, 1500);
			LastPreviewZoom.Value = PreviewZoom;
		}
		rect.SlideRight(padding);

	}


	private void Update_Preview (IRect panelRect) {

		if (Game.PauselessFrame < 2) return;

		// Init
		if (!PreviewInitialized) {
			// Init Character
			PreviewInitialized = true;
			SetPreviewCharacter(LastPreviewCharacter.Value);
		}

		// Preview Character
		int padding = Unify(6);
		var previewRect = panelRect.Shrink(padding, padding, padding, padding);
		using (new SheetIndexScope(SheetIndex)) {
			Preview.AnimationType = CharacterAnimationType.Idle;
			FrameworkUtil.DrawPoseCharacterAsUI(previewRect.ScaleFrom(PreviewZoom, previewRect.CenterX(), previewRect.y), Preview, AnimationFrame);
		}

		// Zoom with Wheel
		if (previewRect.MouseInside() && Input.MouseWheelDelta != 0) {
			int delta = Input.MouseWheelDelta;
			PreviewZoom = (PreviewZoom + delta * 100).Clamp(500, 1500);
			LastPreviewZoom.Value = PreviewZoom;
		}


	}


	#endregion




	#region --- LGC ---


	private void SetPreviewCharacter (string characterName) {
		using var _ = new SheetIndexScope(SheetIndex);
		PreviewCharacterName = characterName;
		LastPreviewCharacter.Value = characterName;
		Preview.OnActivated();
		int charID = characterName.AngeHash();
		if (!ConfigPool.TryGetValue(charID, out var config)) {
			config = PoseCharacter.CreateCharacterRenderingConfigFromSheet(characterName);
			ConfigPool[charID] = config;
		}
		if (config != null) {
			config.LoadToCharacter(Preview);
			// Body Gadget
			Preview_Face.FillFromSheet(characterName);
			Preview_Horn.FillFromSheet(characterName);
			Preview_Wing.FillFromSheet(characterName);
			Preview_Tail.FillFromSheet(characterName);
			Preview_Ear.FillFromSheet(characterName);
			Preview_Hair.FillFromSheet(characterName);
			// Cloth
			PreviewCloth_Head.FillFromSheet(characterName);
			PreviewCloth_Body.FillFromSheet(characterName);
			PreviewCloth_Hip.FillFromSheet(characterName);
			PreviewCloth_Hand.FillFromSheet(characterName);
			PreviewCloth_Foot.FillFromSheet(characterName);
			// Failback
			using (new SheetIndexScope(-1)) {
				if (!Preview_Face.SpriteLoaded) {
					Preview_Face.FillFromSheet(nameof(DefaultFace));
				}
				if (!Preview_Hair.SpriteLoaded) {
					Preview_Hair.FillFromSheet(nameof(DefaultHair));
				}
				if (!PreviewCloth_Body.SpriteLoaded) {
					PreviewCloth_Body.FillFromSheet(nameof(DefaultBodySuit));
				}
				if (!PreviewCloth_Hip.SpriteLoaded) {
					PreviewCloth_Hip.FillFromSheet(nameof(DefaultHipSuit));
				}
				if (!PreviewCloth_Foot.SpriteLoaded) {
					PreviewCloth_Foot.FillFromSheet(nameof(DefaultFootSuit));
				}
			}
		}
	}


	private void ShowPreviewCharacterMenu (IRect rect) {
		if (CurrentProject == null) return;
		if (AllRigCharacterNames.Count == 0) return;
		rect.x += Unify(4);
		GenericPopupUI.BeginPopup(rect.BottomLeft());
		for (int i = 0; i < AllRigCharacterNames.Count; i++) {
			string name = AllRigCharacterNames[i];
			GenericPopupUI.AddItem(
				name, Click,
				@checked: PreviewCharacterName == name,
				data: i
			);
		}
		// Func
		static void Click () {
			if (GenericPopupUI.InvokingItemData is not int index) return;
			if (index < 0 || index >= Instance.AllRigCharacterNames.Count) return;
			Instance.SetPreviewCharacter(Instance.AllRigCharacterNames[index]);
		}
	}


	private void ShowPreviewWeaponMenu (IRect rect) {
		if (CurrentProject == null) return;
		rect.x += Unify(4);
		GenericPopupUI.BeginPopup(rect.BottomLeft());
		for (int i = 0; i < PREVIEW_WEAPONS.Length; i++) {
			var (weapon, defaultName) = PREVIEW_WEAPONS[i];
			if (weapon == null) {
				GenericPopupUI.AddItem(
					BuiltInText.UI_NONE,
					Click,
					@checked: PreviewWeapon == null,
					data: null
				);
			} else {
				GenericPopupUI.AddItem(
					Language.Get(weapon.TypeID, defaultName),
					Click,
					@checked: PreviewWeapon == weapon,
					data: weapon
				);
			}
		}
		// Func
		static void Click () {
			if (GenericPopupUI.InvokingItemData == null) {
				Instance.PreviewWeapon = null;
				PreviewWeaponIndex.Value = 0;
			} else if (GenericPopupUI.InvokingItemData is Weapon weapon) {
				Instance.PreviewWeapon = weapon;
				PreviewWeaponIndex.Value = 0;
				for (int i = 0; i < PREVIEW_WEAPONS.Length; i++) {
					if (PREVIEW_WEAPONS[i].weapon == weapon) {
						PreviewWeaponIndex.Value = i;
						break;
					}
				}
			}
		}
	}


	#endregion




}
