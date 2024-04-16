using System.Collections.Generic;
using System.Collections;
using AngeliA;

namespace AngeliaEngine;

public partial class PixelEditor {



	#region --- SUB ---



	private struct PaintUndoItem : IUndoItem {
		public int Step { get; set; }
		public IRect LocalPixelRect;
		public int SpriteID;
	}


	private struct PixelUndoItem : IUndoItem {
		public int Step { get; set; }
		public Color32 From;
		public Color32 To;
	}


	private struct IndexedPixelUndoItem : IUndoItem {
		public int Step { get; set; }
		public Color32 From;
		public Color32 To;
		public int LocalPixelIndex;
	}


	private struct MoveSliceUndoItem : IUndoItem {
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
		public int From;
		public int To;
	}


	private struct SpriteTagUndoItem : IUndoItem {
		public int Step { get; set; }
		public int SpriteID;
		public int From;
		public int To;
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


	// Data
	private UndoRedo Undo { get; init; }
	private AngeSprite CurrentUndoSprite;
	private int LastGrowUndoFrame = -1;
	private int CurrentUndoPixelIndex;
	private IRect CurrentUndoPixelLocalRect;


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
				RegisterUndo(new PixelUndoItem() {
					From = oldPixels[(y - oldPixelRect.y) * oldPixelRect.width + (x - oldPixelRect.x)],
					To = Color32.CLEAR,
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


	private void OnUndoRedoPerformed (IUndoItem item, bool reverse) {

		switch (item) {

			case PaintUndoItem paint: {
				// Get Sprite
				if (!Sheet.SpritePool.TryGetValue(
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
				CurrentUndoPixelIndex = reverse ? 0 : paint.LocalPixelRect.width * paint.LocalPixelRect.height - 1;
				CurrentUndoPixelLocalRect = paint.LocalPixelRect;
			}
			break;

			case PixelUndoItem pixel: {
				var sprite = CurrentUndoSprite;
				if (sprite == null) break;
				int i = CurrentUndoPixelIndex;
				var pixRect = CurrentUndoSprite.PixelRect;
				var paintRect = CurrentUndoPixelLocalRect;
				int pixX = paintRect.x + i % paintRect.width;
				int pixY = paintRect.y + i / paintRect.width;
				int pixIndex = pixY * pixRect.width + pixX;
				sprite.Pixels[pixIndex] = reverse ? pixel.To : pixel.From;
				CurrentUndoPixelIndex += reverse ? 1 : -1;
				break;
			}

			case IndexedPixelUndoItem iPixel: {
				var sprite = CurrentUndoSprite;
				if (sprite == null) break;
				sprite.Pixels[iPixel.LocalPixelIndex] = reverse ? iPixel.To : iPixel.From;
				break;
			}

			case MoveSliceUndoItem move: {
				if (!Sheet.SpritePool.TryGetValue(
					move.SpriteID, out var sprite
				)) break;
				sprite.PixelRect.x = reverse ? move.To.x : move.From.x;
				sprite.PixelRect.y = reverse ? move.To.y : move.From.y;
				break;
			}

			case SpriteObjectUndoItem spriteObj: {
				bool create = spriteObj.Create == reverse;
				if (create) {
					var newSprite = spriteObj.Sprite.CreateCopy();
					Sheet.AddSprite(newSprite);
					StagedSprites.Add(new SpriteData() {
						Sprite = newSprite,
						PixelDirty = true,
						Selecting = false,
						DraggingStartRect = default,
					});
				} else {
					int id = spriteObj.Sprite.ID;
					int index = Sheet.IndexOfSprite(id);
					if (index < 0) break;
					Sheet.RemoveSprite(index);
					var staged = StagedSprites;
					for (int i = 0; i < staged.Count; i++) {
						if (staged[i].Sprite.ID == id) {
							staged.RemoveAt(i);
							break;
						}
					}
				}
				break;
			}

			case SpriteTriggerUndoItem trigger: {
				if (!Sheet.SpritePool.TryGetValue(
					trigger.SpriteID, out var sprite
				)) break;
				sprite.IsTrigger = reverse ? trigger.To : !trigger.To;
				break;
			}


			case SpriteBorderUndoItem border: {
				if (!Sheet.SpritePool.TryGetValue(
					border.SpriteID, out var sprite
				)) break;
				sprite.GlobalBorder = reverse ? border.To : border.From;
				break;
			}

			case SpriteRuleUndoItem rule: {
				if (!Sheet.SpritePool.TryGetValue(
					rule.SpriteID, out var sprite
				)) break;
				sprite.Rule = reverse ? rule.To : rule.From;
				break;
			}

			case SpriteTagUndoItem tag: {
				if (!Sheet.SpritePool.TryGetValue(
					tag.SpriteID, out var sprite
				)) break;
				sprite.Tag = reverse ? tag.To : tag.From;
				break;
			}

			case SpriteNameUndoItem name: {
				if (!Sheet.SpritePool.TryGetValue(
					name.SpriteID, out var sprite
				)) break;
				sprite.RealName = reverse ? name.To : name.From;
				break;
			}

			case SpritePivotUndoItem pivot: {
				if (!Sheet.SpritePool.TryGetValue(
					pivot.SpriteID, out var sprite
				)) break;
				if (pivot.X) {
					sprite.PivotX = reverse ? pivot.To : pivot.From;
				} else {
					sprite.PivotY = reverse ? pivot.To : pivot.From;
				}
				break;
			}

			case SpriteZUndoItem z: {
				if (!Sheet.SpritePool.TryGetValue(
					z.SpriteID, out var sprite
				)) break;
				sprite.LocalZ = reverse ? z.To : z.From;
				break;
			}

			case SpriteDurationUndoItem duration: {
				if (!Sheet.SpritePool.TryGetValue(
					duration.SpriteID, out var sprite
				)) break;
				sprite.Duration = reverse ? duration.To : duration.From;
				break;
			}

			case SpriteRectUndoItem spRect: {
				if (!Sheet.SpritePool.TryGetValue(
					spRect.SpriteID, out var sprite
				)) break;
				sprite.ResizePixelRect(reverse ? spRect.To : spRect.From, false, out _);
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
