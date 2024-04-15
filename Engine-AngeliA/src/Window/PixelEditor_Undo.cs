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
		public PaintUndoItem (int spriteID, IRect rect) {
			LocalPixelRect = rect;
			SpriteID = spriteID;
		}
	}


	private struct PixelUndoItem : IUndoItem {
		public int Step { get; set; }
		public Color32 From;
		public Color32 To;
		public PixelUndoItem (Color32 from, Color32 to) {
			From = from;
			To = to;
		}
	}


	private struct IndexedPixelUndoItem : IUndoItem {
		public int Step { get; set; }
		public Color32 From;
		public Color32 To;
		public int LocalPixelIndex;
		public IndexedPixelUndoItem (Color32 from, Color32 to, int localPixelIndex) {
			From = from;
			To = to;
			LocalPixelIndex = localPixelIndex;
		}
	}


	private struct MoveSliceUndoItem : IUndoItem {
		public int Step { get; set; }
		public int SpriteID;
		public Int2 From;
		public Int2 To;
		public MoveSliceUndoItem (int spriteID, Int2 from, Int2 to) {
			SpriteID = spriteID;
			From = from;
			To = to;
		}
	}


	private struct SpriteObjectUndoItem : IUndoItem {
		public int Step { get; set; }
		public AngeSprite Sprite;
		public bool Create;
		public SpriteObjectUndoItem (AngeSprite sprite, bool create) {
			Sprite = sprite;
			Create = create;
		}
	}


	private struct SpriteTriggerUndoItem : IUndoItem {
		public int Step { get; set; }
		public int SpriteID;
		public bool To;
		public SpriteTriggerUndoItem (int spriteId, bool to) {
			SpriteID = spriteId;
			To = to;
		}
	}


	private struct SpriteBorderUndoItem : IUndoItem {
		public int Step { get; set; }
		public int SpriteID;
		public Int4 From;
		public Int4 To;
		public SpriteBorderUndoItem (int spriteID, Int4 from, Int4 to) {
			SpriteID = spriteID;
			From = from;
			To = to;
		}
	}


	private struct SpriteRuleUndoItem : IUndoItem {
		public int Step { get; set; }
		public int SpriteID;
		public int From;
		public int To;
		public SpriteRuleUndoItem (int spriteID, int from, int to) {
			SpriteID = spriteID;
			From = from;
			To = to;
		}
	}


	private struct SpriteTagUndoItem : IUndoItem {
		public int Step { get; set; }
		public int SpriteID;
		public int From;
		public int To;
		public SpriteTagUndoItem (int spriteID, int from, int to) {
			SpriteID = spriteID;
			From = from;
			To = to;
		}
	}


	private struct SpriteNameUndoItem : IUndoItem {
		public int Step { get; set; }
		public int SpriteID;
		public string From;
		public string To;
		public SpriteNameUndoItem (int spriteID, string from, string to) {
			SpriteID = spriteID;
			From = from;
			To = to;
		}
	}


	private struct SpritePivotUndoItem : IUndoItem {
		public int Step { get; set; }
		public int SpriteID;
		public int From;
		public int To;
		public bool X;
		public SpritePivotUndoItem (int spriteID, int from, int to, bool x) {
			SpriteID = spriteID;
			From = from;
			To = to;
			X = x;
		}
	}


	private struct SpriteZUndoItem : IUndoItem {
		public int Step { get; set; }
		public int SpriteID;
		public int From;
		public int To;
		public SpriteZUndoItem (int spriteID, int from, int to) {
			SpriteID = spriteID;
			From = from;
			To = to;
		}
	}


	private struct SpriteDurationUndoItem : IUndoItem {
		public int Step { get; set; }
		public int SpriteID;
		public int From;
		public int To;
		public SpriteDurationUndoItem (int spriteID, int from, int to) {
			SpriteID = spriteID;
			From = from;
			To = to;
		}
	}


	#endregion




	#region --- VAR ---


	// Data
	private readonly UndoRedo Undo = new(512 * 1024, OnUndoPerformed, OnRedoPerformed);
	private int LastGrowUndoFrame = -1;
	private AngeSprite CurrentUndoSprite;
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


	// Perform
	private static void OnUndoPerformed (IUndoItem item) => OnUndoRedoPerformed(item, false);


	private static void OnRedoPerformed (IUndoItem item) => OnUndoRedoPerformed(item, true);


	private static void OnUndoRedoPerformed (IUndoItem item, bool reverse) {

		switch (item) {

			case PaintUndoItem paint: {
				// Get Sprite
				if (!Instance.Sheet.SpritePool.TryGetValue(
					paint.SpriteID, out var sprite
				)) break;
				// Pixel Dirty
				foreach (var spData in Instance.StagedSprites) {
					if (spData.Sprite.ID == paint.SpriteID) {
						spData.PixelDirty = true;
						break;
					}
				}
				// Cache
				Instance.CurrentUndoSprite = sprite;
				Instance.CurrentUndoPixelIndex = reverse ? 0 : paint.LocalPixelRect.width * paint.LocalPixelRect.height - 1;
				Instance.CurrentUndoPixelLocalRect = paint.LocalPixelRect;
			}
			break;

			case PixelUndoItem pixel: {
				var sprite = Instance.CurrentUndoSprite;
				if (sprite == null) break;
				int i = Instance.CurrentUndoPixelIndex;
				var pixRect = Instance.CurrentUndoSprite.PixelRect;
				var paintRect = Instance.CurrentUndoPixelLocalRect;
				int pixX = paintRect.x + i % paintRect.width;
				int pixY = paintRect.y + i / paintRect.width;
				int pixIndex = pixY * pixRect.width + pixX;
				sprite.Pixels[pixIndex] = reverse ? pixel.To : pixel.From;
				Instance.CurrentUndoPixelIndex += reverse ? 1 : -1;
				break;
			}

			case IndexedPixelUndoItem iPixel: {
				var sprite = Instance.CurrentUndoSprite;
				if (sprite == null) break;
				sprite.Pixels[iPixel.LocalPixelIndex] = reverse ? iPixel.To : iPixel.From;
				break;
			}

			case MoveSliceUndoItem move: {
				if (!Instance.Sheet.SpritePool.TryGetValue(
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
					Instance.Sheet.AddSprite(newSprite);
					Instance.StagedSprites.Add(new SpriteData() {
						Sprite = newSprite,
						PixelDirty = true,
						Selecting = false,
						DraggingStartRect = default,
					});
				} else {
					int id = spriteObj.Sprite.ID;
					int index = Instance.Sheet.IndexOfSprite(id);
					if (index < 0) break;
					Instance.Sheet.RemoveSprite(index);
					var staged = Instance.StagedSprites;
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
				if (!Instance.Sheet.SpritePool.TryGetValue(
					trigger.SpriteID, out var sprite
				)) break;
				sprite.IsTrigger = reverse ? trigger.To : !trigger.To;
				break;
			}


			case SpriteBorderUndoItem border: {
				if (!Instance.Sheet.SpritePool.TryGetValue(
					border.SpriteID, out var sprite
				)) break;
				sprite.GlobalBorder = reverse ? border.To : border.From;
				break;
			}

			case SpriteRuleUndoItem rule: {
				if (!Instance.Sheet.SpritePool.TryGetValue(
					rule.SpriteID, out var sprite
				)) break;
				sprite.Rule = reverse ? rule.To : rule.From;
				break;
			}

			case SpriteTagUndoItem tag: {
				if (!Instance.Sheet.SpritePool.TryGetValue(
					tag.SpriteID, out var sprite
				)) break;
				sprite.Tag = reverse ? tag.To : tag.From;
				break;
			}

			case SpriteNameUndoItem name: {
				if (!Instance.Sheet.SpritePool.TryGetValue(
					name.SpriteID, out var sprite
				)) break;
				sprite.RealName = reverse ? name.To : name.From;
				break;
			}

			case SpritePivotUndoItem pivot: {
				if (!Instance.Sheet.SpritePool.TryGetValue(
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
				if (!Instance.Sheet.SpritePool.TryGetValue(
					z.SpriteID, out var sprite
				)) break;
				sprite.LocalZ = reverse ? z.To : z.From;
				break;
			}

			case SpriteDurationUndoItem duration: {
				if (!Instance.Sheet.SpritePool.TryGetValue(
					duration.SpriteID, out var sprite
				)) break;
				sprite.Duration = reverse ? duration.To : duration.From;
				break;
			}

		}

		Instance.SetDirty();// done

	}


	#endregion




}
