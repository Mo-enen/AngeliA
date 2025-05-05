using System.Collections.Generic;
using System.Collections;
using AngeliA;

namespace AngeliA.PixelEditor;

public partial class PixelEditor {




	#region --- SUB ---



	private struct PaintUndoItem : IUndoItem {
		public int Step { get; set; }
		public IRect LocalPixelRect;
		public int SpriteID;
	}


	private struct IndexedPixelUndoItem : IUndoItem {
		public int Step { get; set; }
		public Color32 From;
		public Color32 To;
		public int LocalPixelIndex;
	}


	private struct MoveSpriteUndoItem : IUndoItem {
		public int Step { get; set; }
		public int SpriteID;
		public Int2 From;
		public Int2 To;
	}


	private struct SpriteObjectUndoItem : IUndoItem {
		public int Step { get; set; }
		public AngeSprite Sprite;
		public bool Create;
	}


	private struct SpriteTriggerUndoItem : IUndoItem {
		public int Step { get; set; }
		public int SpriteID;
		public bool To;
	}


	private struct SpriteBorderUndoItem : IUndoItem {
		public int Step { get; set; }
		public int SpriteID;
		public Int4 From;
		public Int4 To;
	}


	private struct SpriteRuleUndoItem : IUndoItem {
		public int Step { get; set; }
		public int SpriteID;
		public BlockRule From;
		public BlockRule To;
	}


	private struct SpriteTagUndoItem : IUndoItem {
		public int Step { get; set; }
		public int SpriteID;
		public Tag From;
		public Tag To;
	}


	private struct SpriteNameUndoItem : IUndoItem {
		public int Step { get; set; }
		public int SpriteID;
		public string From;
		public string To;
	}


	private struct SpritePivotUndoItem : IUndoItem {
		public int Step { get; set; }
		public int SpriteID;
		public int From;
		public int To;
		public bool X;
	}


	private struct SpriteZUndoItem : IUndoItem {
		public int Step { get; set; }
		public int SpriteID;
		public int From;
		public int To;
	}


	private struct SpriteDurationUndoItem : IUndoItem {
		public int Step { get; set; }
		public int SpriteID;
		public int From;
		public int To;
	}


	private struct SpriteRectUndoItem : IUndoItem {
		public int Step { get; set; }
		public int SpriteID;
		public IRect From;
		public IRect To;
		public bool Start;
	}


	#endregion




	#region --- VAR ---


	// Const
	private static readonly LanguageCode NOTI_UNDO_PAINT = ("Noti.UndoPaint", "Undo: Paint");
	private static readonly LanguageCode NOTI_REDO_PAINT = ("Noti.RedoPaint", "Redo: Paint");
	private static readonly LanguageCode NOTI_UNDO_MOVE_SPRITE = ("Noti.UndoMoveSprite", "Undo: Move sprite");
	private static readonly LanguageCode NOTI_REDO_MOVE_SPRITE = ("Noti.RedoMoveSprite", "Redo: Move sprite");
	private static readonly LanguageCode NOTI_UNDO_CREATE_SPRITE = ("Noti.UndoCreateSprite", "Undo: Create sprite");
	private static readonly LanguageCode NOTI_REDO_CREATE_SPRITE = ("Noti.RedoCreateSprite", "Redo: Create sprite");
	private static readonly LanguageCode NOTI_UNDO_DELETE_SPRITE = ("Noti.UndoDeleteSprite", "Undo: Delete sprite");
	private static readonly LanguageCode NOTI_REDO_DELETE_SPRITE = ("Noti.RedoDeleteSprite", "Redo: Delete sprite");
	private static readonly LanguageCode NOTI_UNDO_SPRITE_META = ("Noti.UndoSpriteMeta", "Undo: Sprite meta");
	private static readonly LanguageCode NOTI_REDO_SPRITE_META = ("Noti.RedoSpriteMeta", "Redo: Sprite meta");
	private static readonly LanguageCode NOTI_UNDO_SPRITE_SIZE = ("Noti.UndoSpriteSize", "Undo: Sprite size");
	private static readonly LanguageCode NOTI_REDO_SPRITE_SIZE = ("Noti.RedoSpriteSize", "Redo: Sprite size");

	// Data
	private UndoRedo Undo { get; init; }
	private AngeSprite CurrentUndoSprite;
	private int LastGrowUndoFrame = -1;


	#endregion




	#region --- LGC ---


	// Register
	private void RegisterUndo (IUndoItem item, bool ignoreStep = false) {
		if (!ignoreStep && LastGrowUndoFrame != Game.PauselessFrame) {
			LastGrowUndoFrame = Game.PauselessFrame;
			Undo.GrowStep();
		}
		Undo.Register(item);
	}


	private void RegisterUndoForPixelChangesWhenResize (AngeSprite sprite, IRect oldPixelRect, Color32[] oldPixels) {
		if (oldPixelRect == sprite.PixelRect) return;
		var newPixelRect = sprite.PixelRect;
		IRect inter = default;
		if (newPixelRect.xMin > oldPixelRect.xMin) {
			// L
			inter = oldPixelRect.EdgeInside(Direction4.Left, newPixelRect.xMin - oldPixelRect.xMin);
		} else if (newPixelRect.xMax < oldPixelRect.xMax) {
			// R
			inter = oldPixelRect.EdgeInside(Direction4.Right, oldPixelRect.xMax - newPixelRect.xMax);
		} else if (newPixelRect.yMin > oldPixelRect.yMin) {
			// D
			inter = oldPixelRect.EdgeInside(Direction4.Down, newPixelRect.yMin - oldPixelRect.yMin);
		} else if (newPixelRect.yMax < oldPixelRect.yMax) {
			// U
			inter = oldPixelRect.EdgeInside(Direction4.Up, oldPixelRect.yMax - newPixelRect.yMax);
		}
		if (inter.width == 0) return;
		RegisterUndo(new PaintUndoItem() {
			SpriteID = 0,
			LocalPixelRect = default,
		});
		for (int j = 0; j < inter.height; j++) {
			int y = j + inter.y;
			for (int i = 0; i < inter.width; i++) {
				int x = i + inter.x;
				int index = (y - oldPixelRect.y) * oldPixelRect.width + (x - oldPixelRect.x);
				RegisterUndo(new IndexedPixelUndoItem() {
					From = oldPixels[index],
					To = Color32.CLEAR,
					LocalPixelIndex = index,
				});
			}
		}
		RegisterUndo(new PaintUndoItem() {
			SpriteID = sprite.ID,
			LocalPixelRect = inter.Shift(-oldPixelRect.x, -oldPixelRect.y),
		});
	}


	// Perform
	private void OnUndoPerformed (IUndoItem item) => OnUndoRedoPerformed(item, false);


	private void OnRedoPerformed (IUndoItem item) => OnUndoRedoPerformed(item, true);


	private void OnUndoRedoPerformed (IUndoItem item, bool redo) {

		switch (item) {

			case PaintUndoItem paint: {
				// Get Sprite
				if (!EditingSheet.SpritePool.TryGetValue(
					paint.SpriteID, out var sprite
				)) break;
				// Pixel Dirty
				foreach (var spData in StagedSprites) {
					if (spData.Sprite.ID == paint.SpriteID) {
						spData.PixelDirty = true;
						break;
					}
				}
				// Cache
				CurrentUndoSprite = sprite;
				RequireNotification(redo ? NOTI_REDO_PAINT : NOTI_UNDO_PAINT);
			}
			break;

			case IndexedPixelUndoItem iPixel: {
				var sprite = CurrentUndoSprite;
				if (sprite == null) break;
				sprite.Pixels[iPixel.LocalPixelIndex] = redo ? iPixel.To : iPixel.From;
				break;
			}

			case MoveSpriteUndoItem move: {
				if (!EditingSheet.SpritePool.TryGetValue(
					move.SpriteID, out var sprite
				)) break;
				sprite.PixelRect.x = redo ? move.To.x : move.From.x;
				sprite.PixelRect.y = redo ? move.To.y : move.From.y;
				RequireNotification(redo ? NOTI_REDO_MOVE_SPRITE : NOTI_UNDO_MOVE_SPRITE);
				break;
			}

			case SpriteObjectUndoItem spriteObj: {
				bool create = spriteObj.Create == redo;
				if (create) {
					var newSprite = spriteObj.Sprite.CreateCopy();
					EditingSheet.AddSprite(newSprite);
					StagedSprites.Add(new SpriteData(newSprite));
				} else {
					int id = spriteObj.Sprite.ID;
					int index = EditingSheet.IndexOfSprite(id);
					if (index < 0) break;
					EditingSheet.RemoveSprite(index);
					var staged = StagedSprites;
					for (int i = 0; i < staged.Count; i++) {
						if (staged[i].Sprite.ID == id) {
							staged.RemoveAt(i);
							break;
						}
					}
				}
				RequireNotification(spriteObj.Create ?
					(redo ? NOTI_REDO_CREATE_SPRITE : NOTI_UNDO_CREATE_SPRITE) :
					(redo ? NOTI_REDO_DELETE_SPRITE : NOTI_UNDO_DELETE_SPRITE)
				);
				break;
			}

			case SpriteTriggerUndoItem trigger: {
				if (!EditingSheet.SpritePool.TryGetValue(
					trigger.SpriteID, out var sprite
				)) break;
				sprite.IsTrigger = redo ? trigger.To : !trigger.To;
				RequireNotification(redo ? NOTI_REDO_SPRITE_META : NOTI_UNDO_SPRITE_META);
				break;
			}


			case SpriteBorderUndoItem border: {
				if (!EditingSheet.SpritePool.TryGetValue(
					border.SpriteID, out var sprite
				)) break;
				sprite.GlobalBorder = redo ? border.To : border.From;
				RequireNotification(redo ? NOTI_REDO_SPRITE_META : NOTI_UNDO_SPRITE_META);
				break;
			}

			case SpriteRuleUndoItem rule: {
				if (!EditingSheet.SpritePool.TryGetValue(
					rule.SpriteID, out var sprite
				)) break;
				sprite.Rule = redo ? rule.To : rule.From;
				RequireNotification(redo ? NOTI_REDO_SPRITE_META : NOTI_UNDO_SPRITE_META);
				break;
			}

			case SpriteTagUndoItem tag: {
				if (!EditingSheet.SpritePool.TryGetValue(
					tag.SpriteID, out var sprite
				)) break;
				sprite.Tag = redo ? tag.To : tag.From;
				RequireNotification(redo ? NOTI_REDO_SPRITE_META : NOTI_UNDO_SPRITE_META);
				break;
			}

			case SpriteNameUndoItem name: {
				EditingSheet.RenameSprite(name.SpriteID, redo ? name.To : name.From);
				RequireNotification(redo ? NOTI_REDO_SPRITE_META : NOTI_UNDO_SPRITE_META);
				break;
			}

			case SpritePivotUndoItem pivot: {
				if (!EditingSheet.SpritePool.TryGetValue(
					pivot.SpriteID, out var sprite
				)) break;
				if (pivot.X) {
					sprite.PivotX = redo ? pivot.To : pivot.From;
				} else {
					sprite.PivotY = redo ? pivot.To : pivot.From;
				}
				RequireNotification(redo ? NOTI_REDO_SPRITE_META : NOTI_UNDO_SPRITE_META);
				break;
			}

			case SpriteZUndoItem z: {
				if (!EditingSheet.SpritePool.TryGetValue(
					z.SpriteID, out var sprite
				)) break;
				sprite.LocalZ = redo ? z.To : z.From;
				RequireNotification(redo ? NOTI_REDO_SPRITE_META : NOTI_UNDO_SPRITE_META);
				break;
			}

			case SpriteDurationUndoItem duration: {
				if (!EditingSheet.SpritePool.TryGetValue(
					duration.SpriteID, out var sprite
				)) break;
				sprite.Duration = redo ? duration.To : duration.From;
				RequireNotification(redo ? NOTI_REDO_SPRITE_META : NOTI_UNDO_SPRITE_META);
				break;
			}

			case SpriteRectUndoItem spRect: {
				if (!EditingSheet.SpritePool.TryGetValue(
					spRect.SpriteID, out var sprite
				)) break;
				sprite.ResizePixelRect(redo ? spRect.To : spRect.From, false, out _);
				RequireNotification(redo ? NOTI_REDO_SPRITE_SIZE : NOTI_UNDO_SPRITE_SIZE);
				// Pixel Dirty
				foreach (var spData in StagedSprites) {
					if (spData.Sprite.ID == sprite.ID) {
						spData.PixelDirty = true;
						break;
					}
				}
				break;
			}

		}

		SetDirty();

	}


	#endregion




}