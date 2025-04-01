using System;
using System.Collections.Generic;

namespace AngeliA;

public static partial class FrameworkUtil {


	// VAR
	private static readonly int[] ITEM_TYPE_ICONS = [
		BuiltInSprite.ITEM_ICON_HAND_TOOL,
		BuiltInSprite.ITEM_ICON_ARMOR,
		BuiltInSprite.ITEM_ICON_HELMET,
		BuiltInSprite.ITEM_ICON_SHOES,
		BuiltInSprite.ITEM_ICON_GLOVES,
		BuiltInSprite.ITEM_ICON_JEWELRY,
		BuiltInSprite.ITEM_ICON_WEAPON,
		BuiltInSprite.ITEM_ICON_FOOD,
		BuiltInSprite.ITEM_ICON_ITEM,
	];
	private static readonly List<PhysicsCell[,,]> CellPhysicsCells = [];
	private static readonly Color32[] COLLIDER_TINTS = [
		Color32.RED_BETTER,
		Color32.ORANGE_BETTER,
		Color32.YELLOW,
		Color32.GREEN,
		Color32.CYAN,
		Color32.BLUE,
		Color32.GREY_128,
	];


	// API
	public static void ClampCells (Cell[] cells, IRect rect, int startIndex, int endIndex) => ClampCells(cells.GetSpan(), rect, startIndex, endIndex);

	public static void ClampCells (Span<Cell> cells, IRect rect, int startIndex, int endIndex) {
		if (cells == null) return;
		endIndex = endIndex.LessOrEquel(cells.Length);
		for (int i = startIndex; i < endIndex; i++) {
			cells[i].Clamp(rect);
		}
	}


	internal static void LimbRotate (
		ref int targetX, ref int targetY, ref int targetPivotX, ref int targetPivotY, ref int targetRotation, ref int targetWidth, ref int targetHeight,
		int rotation, bool useFlip, int grow
	) {
		targetPivotY = 1000;
		targetWidth = targetWidth.Abs();
		bool bigRot = rotation.Abs() > 90;

		// Rotate
		targetRotation = rotation;
		int newPivotX = rotation > 0 != bigRot ? 1000 : 0;
		if (newPivotX != targetPivotX) {
			targetPivotX = newPivotX;
			targetX += newPivotX == 1000 ? targetWidth : -targetWidth;
		}

		// Flip
		if (targetPivotX > 500) {
			targetPivotX = 1000 - targetPivotX;
			if (bigRot || useFlip) {
				targetWidth = -targetWidth;
			} else {
				targetX -= (int)(Util.Cos(targetRotation * Util.Deg2Rad) * targetWidth);
				targetY += (int)(Util.Sin(targetRotation * Util.Deg2Rad) * targetWidth);
			}
		}

		// Fix for Big Rot
		if (bigRot) {
			targetWidth = -targetWidth;
			targetHeight -= targetWidth.Abs();
		}

		// Grow
		if (rotation != 0 && grow != 0) {
			targetHeight += rotation.Abs() * grow * targetWidth.Abs() / 96000;
		}

	}

	internal static void LimbRotate (
		ref int targetX, ref int targetY, ref int targetPivotX, ref int targetPivotY, ref int targetRotation, ref int targetWidth, ref int targetHeight,
		int parentX, int parentY, int parentRotation, int parentWidth, int parentHeight,
		int rotation, bool useFlip, int grow
	) {
		targetPivotY = 1000;
		targetWidth = targetWidth.Abs();
		bool bigRot = rotation.Abs() > 90;

		// Rotate
		targetRotation = parentRotation + rotation;
		targetX = parentX - (int)(Util.Sin(parentRotation * Util.Deg2Rad) * parentHeight);
		targetY = parentY - (int)(Util.Cos(parentRotation * Util.Deg2Rad) * parentHeight);
		targetPivotX = rotation > 0 != bigRot ? 1000 : 0;
		if (parentWidth < 0 != targetPivotX > 500) {
			int pWidth = parentWidth.Abs();
			int sign = targetPivotX > 500 ? -1 : 1;
			targetX -= (int)(Util.Cos(parentRotation * Util.Deg2Rad) * pWidth) * sign;
			targetY += (int)(Util.Sin(parentRotation * Util.Deg2Rad) * pWidth) * sign;
		}

		// Flip
		if (targetPivotX > 500) {
			targetPivotX = 1000 - targetPivotX;
			if (bigRot || useFlip) {
				targetWidth = -targetWidth;
			} else {
				targetX -= (int)(Util.Cos(targetRotation * Util.Deg2Rad) * targetWidth);
				targetY += (int)(Util.Sin(targetRotation * Util.Deg2Rad) * targetWidth);
			}
		}

		// Fix for Big Rot
		if (bigRot) {
			targetWidth = -targetWidth;
			targetHeight -= parentWidth.Abs();
		}

		// Grow
		if (rotation != 0 && grow != 0) {
			targetHeight += rotation.Abs() * grow * targetWidth.Abs() / 96000;
		}

	}


	/// <summary>
	/// Get average color of given pixels
	/// </summary>
	public static Color32 GetSummaryTint (Color32[] pixels) {
		if (pixels == null || pixels.Length == 0) return Color32.CLEAR;
		var sum = Float3.Zero;
		float len = 0;
		for (int i = 0; i < pixels.Length; i++) {
			var pixel = pixels[i];
			if (pixel.a != 0) {
				sum.x += pixel.r / 255f;
				sum.y += pixel.g / 255f;
				sum.z += pixel.b / 255f;
				len++;
			}
		}
		return new Color32(
			(byte)(sum.x * 255f / len),
			(byte)(sum.y * 255f / len),
			(byte)(sum.z * 255f / len),
			255
		);
	}


	// Draw
	/// <summary>
	/// Draw a shadow for given rendering cell
	/// </summary>
	/// <param name="source">Target rendering cell</param>
	/// <param name="offsetX">Position offset X in global space</param>
	/// <param name="offsetY">Position offset Y in global space</param>
	/// <param name="alpha"></param>
	/// <param name="z">Position Z for the shadow</param>
	/// <returns>Rendering cell of the shadow</returns>
	public static Cell DrawEnvironmentShadow (Cell source, int offsetX = -Const.HALF / 2, int offsetY = 0, byte alpha = 64, int z = -64 * 1024 + 16) {
		int oldLayer = Renderer.CurrentLayerIndex;
		Renderer.SetLayer(RenderLayer.SHADOW);
		var result = Renderer.DrawPixel(default);
		result.CopyFrom(source);
		result.X += offsetX;
		result.Y += offsetY;
		result.Z = z;
		result.Color = new Color32(0, 0, 0, alpha);
		Renderer.SetLayer(oldLayer);
		return result;
	}


	/// <summary>
	/// Display target pose-styled character as UI (like the character preview in player menu)
	/// </summary>
	/// <param name="rect">Rect position to display the UI in global space</param>
	/// <param name="renderer">Target character</param>
	/// <param name="animationFrame">Current frame for animation</param>
	/// <returns>True if the character is rendered</returns>
	public static bool DrawPoseCharacterAsUI (IRect rect, PoseCharacterRenderer renderer, int animationFrame) => DrawPoseCharacterAsUI(rect, renderer, animationFrame, out _, out _);
	/// <inheritdoc cref="DrawPoseCharacterAsUI(IRect, PoseCharacterRenderer, int)"/>
	public static bool DrawPoseCharacterAsUI (IRect rect, PoseCharacterRenderer renderer, int animationFrame, out IRect globalRect, out IRect uiRect) {

		globalRect = default;
		uiRect = default;
		if (renderer == null) return false;
		var target = renderer.TargetCharacter;

		// Draw Player
		int cellIndexStart;
		int cellIndexEnd;
		using (new UILayerScope(ignoreSorting: true)) {
			cellIndexStart = Renderer.GetUsedCellCount();
			int oldAniFrame = renderer.CurrentAnimationFrame;
			renderer.CurrentAnimationFrame = animationFrame;
			bool oldActive = target.Active;
			target.Active = true;
			renderer.LateUpdate();
			target.Active = oldActive;
			renderer.CurrentAnimationFrame = oldAniFrame;
			cellIndexEnd = Renderer.GetUsedCellCount();
		}
		if (cellIndexStart == cellIndexEnd) return false;

		if (!Renderer.GetCells(RenderLayer.UI, out var cells, out int count)) return false;

		// Get Min Max
		bool flying = target.AnimationType == CharacterAnimationType.Fly;
		int originalMinX = target.X - Const.HALF - 16;
		int originalMinY = target.Y - 16 + (flying ? renderer.PoseRootY / 2 : 0);
		int originalMaxX = target.X + Const.HALF + 16;
		int originalMaxY = target.Y + Const.CEL * 2 + 16;
		if (flying) {
			originalMinY -= Const.HALF;
			originalMaxY -= Const.HALF;
		}
		globalRect.SetMinMax(originalMinX, originalMaxX, originalMinY, originalMaxY);

		// Move Cells
		RemapCells(
			cells, cellIndexStart, Util.Min(count, cellIndexEnd),
			IRect.MinMaxRect(originalMinX, originalMinY, originalMaxX, originalMaxY),
			rect, out int minZ,
			500, 0
		);
		uiRect = rect.Fit(originalMaxX - originalMinX, originalMaxY - originalMinY, 500, 0);

		// Fix Z
		if (minZ != int.MaxValue) {
			for (int i = cellIndexStart; i < count && i < cellIndexEnd; i++) {
				var cell = cells[i];
				cell.Z -= minZ;
			}
		}

		// Sort
		Util.QuickSort(cells, cellIndexStart, Util.Min(cellIndexEnd - 1, count - 1), Renderer.CellComparer.Instance);

		return true;

	}


	/// <inheritdoc cref="RemapCells(Span{Cell}, int, int, IRect, IRect, out int, int, int, bool, bool)"/>
	public static void RemapCells (Cell[] cells, int cellIndexStart, int cellIndexEnd, IRect from, IRect to, int fitPivotX = 500, int fitPivotY = 500, bool fit = true) => RemapCells(cells.GetSpan(), cellIndexStart, cellIndexEnd, from, to, out _, fitPivotX, fitPivotY, fit);
	/// <inheritdoc cref="RemapCells(Span{Cell}, int, int, IRect, IRect, out int, int, int, bool, bool)"/>
	public static void RemapCells (Span<Cell> cells, int cellIndexStart, int cellIndexEnd, IRect from, IRect to, int fitPivotX = 500, int fitPivotY = 500, bool fit = true) => RemapCells(cells, cellIndexStart, cellIndexEnd, from, to, out _, fitPivotX, fitPivotY, fit);
	/// <inheritdoc cref="RemapCells(Span{Cell}, int, int, IRect, IRect, out int, int, int, bool, bool)"/>
	public static void RemapCells (Cell[] cells, int cellIndexStart, int cellIndexEnd, IRect from, IRect to, out int minZ, int fitPivotX = 500, int fitPivotY = 500, bool fit = true) => RemapCells(cells.GetSpan(), cellIndexStart, cellIndexEnd, from, to, out minZ, fitPivotX, fitPivotY, fit);
	/// <summary>
	/// Remap the position and size of given rendering cells
	/// </summary>
	/// <param name="cells"></param>
	/// <param name="cellIndexStart">Start index of remap logic</param>
	/// <param name="cellIndexEnd">End index of remap logic (exclude)</param>
	/// <param name="from">Remap from this range (global space)</param>
	/// <param name="to">Remap to this range (global space)</param>
	/// <param name="minZ">Minimal Z value for sort rendering cells</param>
	/// <param name="fitPivotX"></param>
	/// <param name="fitPivotY"></param>
	/// <param name="fit">True if keep the aspect ratio by resize the cells</param>
	public static void RemapCells (Span<Cell> cells, int cellIndexStart, int cellIndexEnd, IRect from, IRect to, out int minZ, int fitPivotX = 500, int fitPivotY = 500, bool fit = true) {
		int originalWidth = from.width;
		int originalHeight = from.height;
		var targetRect = fit ?
			to.Fit(originalWidth, originalHeight, fitPivotX, fitPivotY) :
			to.Envelope(originalWidth, originalHeight);
		minZ = int.MaxValue;
		for (int i = cellIndexStart; i < cellIndexEnd; i++) {
			var cell = cells[i];
			minZ = Util.Min(minZ, cell.Z);
			cell.X = (targetRect.x + (cell.X - from.x) * targetRect.width / (float)originalWidth).FloorToInt();
			cell.Y = (targetRect.y + (cell.Y - from.y) * targetRect.height / (float)originalHeight).FloorToInt();
			cell.Width = (cell.Width * targetRect.width / (float)originalWidth).CeilToInt();
			cell.Height = (cell.Height * targetRect.height / (float)originalHeight).CeilToInt();
			if (!cell.Shift.IsZero) {
				cell.Shift = new Int4(
					cell.Shift.left * targetRect.width / originalWidth,
					cell.Shift.right * targetRect.width / originalWidth,
					cell.Shift.down * targetRect.height / originalHeight,
					cell.Shift.up * targetRect.height / originalHeight
				);
			}
		}
	}


	/// <summary>
	/// Draw usage bar UI
	/// </summary>
	/// <param name="rect">Rect position in global space</param>
	/// <param name="usage"></param>
	/// <param name="maxUsage"></param>
	public static void DrawItemUsageBar (IRect rect, int usage, int maxUsage) {
		rect = rect.Shrink(rect.height / 6);
		int border = rect.height / 10;
		// BG
		Renderer.DrawPixel(rect, Color32.BLACK);
		// Frame
		Renderer.DrawSlice(BuiltInSprite.FRAME_16, rect, border, border, border, border, Color32.GREY_128);
		// Bar
		Renderer.DrawPixel(
			new IRect(rect.x, rect.y, rect.width * usage / maxUsage, rect.height).Shrink(border),
			Color32.Lerp(Color32.RED, Color32.GREEN, (float)usage / maxUsage)
		);
	}


	/// <summary>
	/// Display current physics colliders with game gizmos functions
	/// </summary>
	/// <param name="physicsMask">Which physics layers are included</param>
	/// <param name="offset">Position offset for all gizmos</param>
	/// <param name="brightness">0 means dark, 1 means normal color</param>
	/// <param name="ignoreNonOnewayTrigger">True if triggers that is not oneway gate are excluded</param>
	/// <param name="ignoreOnewayTrigger">True if oneway gates are excluded</param>
	/// <param name="useTechEffect">True if the gizmos glitchs</param>
	/// <param name="layerTints">Color for specified layers. Set to null to use default.</param>
	public static void DrawAllCollidersAsGizmos (
		int physicsMask = PhysicsMask.ALL,
		Int2 offset = default,
		float brightness = 1f,
		bool ignoreNonOnewayTrigger = false,
		bool ignoreOnewayTrigger = false,
		bool useTechEffect = false,
		Color32[] layerTints = null
	) {

		layerTints ??= COLLIDER_TINTS;

		// Init Cells
		if (CellPhysicsCells.Count == 0) {
			try {
				var layers = Util.GetStaticFieldValue(typeof(Physics), Physics.LayersName) as System.Array;
				for (int layerIndex = 0; layerIndex < PhysicsLayer.COUNT; layerIndex++) {
					var layerObj = layers.GetValue(layerIndex);
					CellPhysicsCells.Add(Util.GetFieldValue(layerObj, Physics.CellsName) as PhysicsCell[,,]);
				}
			} catch (Exception ex) { Debug.LogException(ex); }
			if (CellPhysicsCells.Count == 0) CellPhysicsCells.Add(null);
		}

		// Draw Cells
		if (CellPhysicsCells.Count > 0 && CellPhysicsCells[0] != null) {
			int thick = GUI.Unify(1);
			var cameraRect = Renderer.CameraRect;
			float framePingPong01 = Ease.InOutQuad(Game.GlobalFrame.PingPong(120) / 120f);
			framePingPong01 /= 4f;
			for (int layer = 0; layer < CellPhysicsCells.Count; layer++) {
				try {
					if ((physicsMask & (1 << layer)) == 0) continue;
					var tint = layerTints[layer.Clamp(0, layerTints.Length - 1)];
					tint = Color32.LerpUnclamped(Color32.BLACK, tint, brightness);
					var cells = CellPhysicsCells[layer];
					int cellWidth = cells.GetLength(0);
					int cellHeight = cells.GetLength(1);
					int celDepth = cells.GetLength(2);
					for (int y = 0; y < cellHeight; y++) {
						for (int x = 0; x < cellWidth; x++) {
							for (int d = 0; d < celDepth; d++) {
								var cell = cells[x, y, d];
								if (cell.Frame != Physics.CurrentFrame) break;
								if (cell.IsTrigger) {
									bool isNonOnewayTrigger = !HasOnewayTag(cell.Tag) && !cell.Tag.HasAny(Tag.Climb | Tag.ClimbStable);
									if (ignoreNonOnewayTrigger && isNonOnewayTrigger) continue;
									if (ignoreOnewayTrigger && !isNonOnewayTrigger) continue;
								}
								if (!cell.Rect.Overlaps(cameraRect)) continue;
								// Effect
								var rect = cell.Rect.Shift(offset);
								var _tint = tint;
								if (useTechEffect) {
									const int RANDOM_SHAKE = 5;
									_tint.a = (byte)(_tint.a - framePingPong01 * 255).Clamp(0, 255);
									rect.x += Util.QuickRandom(-RANDOM_SHAKE, RANDOM_SHAKE + 1);
									rect.y += Util.QuickRandom(-RANDOM_SHAKE, RANDOM_SHAKE + 1);
								}
								// Frame
								Game.DrawGizmosRect(rect.EdgeInside(Direction4.Down, thick), _tint);
								Game.DrawGizmosRect(rect.EdgeInside(Direction4.Up, thick), _tint);
								Game.DrawGizmosRect(rect.EdgeInside(Direction4.Left, thick), _tint);
								Game.DrawGizmosRect(rect.EdgeInside(Direction4.Right, thick), _tint);
								// Cross
								if (!cell.IsTrigger) {
									Game.DrawGizmosLine(rect.x, rect.y, rect.xMax, rect.yMax, thick, _tint);
									Game.DrawGizmosLine(rect.xMax, rect.y, rect.x, rect.yMax, thick, _tint);
								}
							}
						}
					}
				} catch (Exception ex) { Debug.LogException(ex); }
			}
		}
	}


	/// <summary>
	/// Get type icon artwork sprite ID for given item
	/// </summary>
	public static int GetItemTypeIcon (int itemID) {
		int typeIcon;
		var item = ItemSystem.GetItem(itemID);
		if (item is Equipment eq) {
			// Equipment
			if (item is Weapon) {
				typeIcon = ITEM_TYPE_ICONS[^3];
			} else {
				typeIcon = ITEM_TYPE_ICONS[(int)eq.EquipmentType];
			}
		} else if (item is Food) {
			// Food
			typeIcon = ITEM_TYPE_ICONS[^2];
		} else {
			// General
			typeIcon = ITEM_TYPE_ICONS[^1];
		}
		return typeIcon;
	}


	/// <summary>
	/// Draw clock hands from given time
	/// </summary>
	/// <param name="rect">Rect position for clock face in global space</param>
	/// <param name="handCode">Artwork sprite ID for the hand</param>
	/// <param name="thickness"></param>
	/// <param name="thicknessSecond"></param>
	/// <param name="tint">Color tint</param>
	/// <param name="z">Z value for sort rendering cells</param>
	public static void DrawClockHands (IRect rect, int handCode, int thickness, int thicknessSecond, Color32 tint, int z = int.MinValue) => DrawClockHands(rect.CenterX(), rect.CenterY(), rect.height, handCode, thickness, thicknessSecond, tint, z);


	/// <summary>
	/// Draw clock hands from given time
	/// </summary>
	/// <param name="centerX">Center position in global space</param>
	/// <param name="centerY">Center position in global space</param>
	/// <param name="radius">Radius of the clock face</param>
	/// <param name="handCode">Artwork sprite ID for the hand</param>
	/// <param name="thickness"></param>
	/// <param name="thicknessSecond"></param>
	/// <param name="tint">Color tint</param>
	/// <param name="z">Z value for sort rendering cells</param>
	public static void DrawClockHands (int centerX, int centerY, int radius, int handCode, int thickness, int thicknessSecond, Color32 tint, int z = int.MinValue) {

		Time01_to_TimeDigit(Sky.InGameDaytime01, out int hour, out int min, out int second);

		// Sec
		Renderer.Draw(
			handCode, centerX, centerY,
			500, 0, second * 360 / 60,
			thicknessSecond, radius * 900 / 2000, tint, z
		);
		// Min
		Renderer.Draw(
			handCode, centerX, centerY,
			500, 0, min * 360 / 60,
			thickness, radius * 800 / 2000, tint, z
		);
		// Hour
		Renderer.Draw(
			handCode, centerX, centerY,
			500, 0, (hour * 360 / 12) + (min * 360 / 12 / 60),
			thickness, radius * 400 / 2000, tint, z
		);
	}


	/// <summary>
	/// Draw the pendulum for clocks
	/// </summary>
	/// <param name="artCodeLeg">Artwork sprite ID for the long handle part</param>
	/// <param name="artCodeHead">Artwork sprite ID for the head on the edge</param>
	/// <param name="x">Center position in global space</param>
	/// <param name="y">Center position in global space</param>
	/// <param name="length">Length of the pendulum in global space</param>
	/// <param name="thickness">Thichness of the leg in global space</param>
	/// <param name="headSize">Size of the head in global space</param>
	/// <param name="maxRot">Rotation amount</param>
	/// <param name="deltaX">Extra position shift amount</param>
	public static void DrawClockPendulum (int artCodeLeg, int artCodeHead, int x, int y, int length, int thickness, int headSize, int maxRot, int deltaX = 0) {
		float t11 = Util.Sin(Game.GlobalFrame * 6 * Util.Deg2Rad);
		int rot = (t11 * maxRot).RoundToInt();
		int dX = -(t11 * deltaX).RoundToInt();
		// Leg
		Renderer.Draw(artCodeLeg, x + dX, y, 500, 1000, rot, thickness, length);
		// Head
		Renderer.Draw(
			artCodeHead, x + dX, y, 500,
			500 * (headSize / 2 + length) / (headSize / 2),
			rot, headSize, headSize
		);
	}


	/// <summary>
	/// Draw spinning effect for teleporting with portal
	/// </summary>
	/// <param name="localFrame"></param>
	/// <param name="pointX">Center position in global space</param>
	/// <param name="pointY">Center position in global space</param>
	/// <param name="duration">Total duration of the animation</param>
	/// <param name="cellIndexStart">Start index of target rendering cells in current rendering layer</param>
	/// <param name="reverseSpin"></param>
	public static void SpiralSpinningCellEffect (int localFrame, int pointX, int pointY, int duration, int cellIndexStart, bool reverseSpin = false) {
		if (!Renderer.GetCells(out var cells, out int count)) return;
		for (int i = cellIndexStart; i < count; i++) {
			float lerp01 = localFrame.PingPong(duration / 2) / (duration / 2f);
			int offsetX = (int)((1f - lerp01) * Const.CEL * Util.Sin(lerp01 * 720f * Util.Deg2Rad));
			int offsetY = (int)((1f - lerp01) * Const.CEL * Util.Cos(lerp01 * 720f * Util.Deg2Rad));
			var cell = cells[i];
			cell.X += offsetX;
			cell.Y += offsetY;
			int spinSpeed = reverseSpin ? -720 : 720;
			cell.RotateAround(localFrame * spinSpeed / duration, pointX + offsetX, pointY + offsetY);
			cell.ScaleFrom(
				Util.RemapUnclamped(0, duration / 2, 1000, 0, localFrame.PingPong(duration / 2)),
				pointX + offsetX, pointY + offsetY
			);
		}
	}


	/// <summary>
	/// Draw a highlight effect
	/// </summary>
	/// <param name="targetRect">Rect position in global space</param>
	/// <param name="tint">Color tint</param>
	/// <param name="lineCount"></param>
	/// <param name="duration">Duration in frame for a single loop</param>
	public static void DrawLoopingActivatedHighlight (IRect targetRect, Color32 tint, int lineCount = 4, int duration = 22) {
		int localFrame = Game.GlobalFrame % duration;
		var rect = targetRect;
		Renderer.SetLayerToAdditive();
		for (int i = 0; i < lineCount; i++) {
			tint.a = (byte)(i == lineCount - 1 ? Util.RemapUnclamped(0, duration, 64, 0, localFrame) : 64);
			rect.y = targetRect.y;
			rect.height = i * targetRect.height / lineCount;
			rect.height += Util.RemapUnclamped(0, duration, 0, targetRect.height / lineCount, localFrame);
			Renderer.Draw(BuiltInSprite.SOFT_LINE_H, rect, tint);
		}
		Renderer.SetLayerToDefault();
	}


	/// <summary>
	/// Draw a direction mark with looping moving triangles
	/// </summary>
	/// <param name="range">Rect position in global space</param>
	/// <param name="frame">Current animation frame</param>
	/// <param name="tint">Color tint</param>
	/// <param name="direction"></param>
	/// <param name="count">Triangle count</param>
	/// <param name="size">Triangle size</param>
	/// <param name="z">Z value for sort rendering cells</param>
	/// <param name="speed">Moving speed of the triangle</param>
	public static void DrawLoopingTriangleMark (IRect range, int frame, Color32 tint, Direction4 direction, int count, int size, int z, int speed) {
		using var _ = new ClampCellsScope(range);
		int centerX = range.CenterX();
		int centerY = range.CenterY();
		bool vertical = direction.IsVertical();
		int aniSign = direction == Direction4.Right || direction == Direction4.Up ? speed : -speed;
		int loopFix = aniSign < 0 ? -size / 2 : 0;
		for (int i = 0; i < count; i++) {
			Renderer.Draw(
				direction switch {
					Direction4.Left => BuiltInSprite.ICON_TRIANGLE_LEFT,
					Direction4.Right => BuiltInSprite.ICON_TRIANGLE_RIGHT,
					Direction4.Down => BuiltInSprite.ICON_TRIANGLE_DOWN,
					Direction4.Up => BuiltInSprite.ICON_TRIANGLE_UP,
					_ => 0,
				},
				vertical ? centerX : range.x + i * size + loopFix + (aniSign * frame).UMod(size),
				vertical ? range.y + i * size + loopFix + (aniSign * frame).UMod(size) : centerY,
				500, 500, 0,
				size, size,
				tint, z
			);
		}
	}


	// Animate Effect
	/// <summary>
	/// Draw effect for object on fire
	/// </summary>
	/// <param name="spriteID">Artwork sprite ID</param>
	/// <param name="rect">Rect range in global space</param>
	/// <param name="count">Rendering sprite count at same time</param>
	/// <param name="loop">Duration for a single loop</param>
	/// <param name="size">Size of a single sprite</param>
	/// <param name="seed">Seed to generate random value</param>
	/// <param name="z">Z value for sort rendering cells</param>
	public static void DrawOnFireEffect (int spriteID, IRect rect, int count = 2, int loop = 40, int size = 200, int seed = 0, int z = int.MaxValue) {
		if (!Renderer.TryGetSprite(spriteID, out var sprite, ignoreAnimation: false)) return;
		int left = rect.x;
		int down = rect.y;
		int width = rect.width;
		int height = rect.height;
		int frame = Game.GlobalFrame;
		float frame01 = frame / (float)loop;
		float fixedFrame01 = frame01 * Const.CEL / height;
		int hHeight = height / 2;
		seed = seed == 0 ? 1276344 + rect.x.ToUnit() * 217634 + rect.y.ToUnit() * 1235 : seed;
		for (int i = 0; i < count; i++) {
			float lerp01 = i / (float)count;
			int localSeed = seed + ((int)(fixedFrame01 + lerp01)) * 8254;
			int x = left + Util.QuickRandomWithSeed(localSeed + i * 21632, 0, width).UMod(width);
			int y = down + ((fixedFrame01 + lerp01) * height).RoundToInt().UMod(height);
			int _size = Util.QuickRandomWithSeed(localSeed + i * 2512, size / 2, size);
			int alpha = ((y - rect.y).PingPong(hHeight) * 512 / hHeight).Clamp(0, 255);
			Renderer.Draw(sprite, x, y, 500, 500, 0, _size, _size, Color32.WHITE.WithNewA(alpha), z: z);
		}
	}


	/// <summary>
	/// Draw effect for frozen zone
	/// </summary>
	/// <param name="rect">Position range in global space</param>
	/// <param name="alpha"></param>
	/// <param name="count">Particle count at same time</param>
	/// <param name="offset">Position offset</param>
	/// <param name="seed">Seed to generate random value</param>
	/// <param name="size">Size of a single particle</param>
	/// <param name="z">Z value to sort rendering cells</param>
	public static void DrawFrozenEffect (IRect rect, byte alpha, int count = 32, Int2 offset = default, int seed = 0, int size = 142, int z = 0) {
		if (!Renderer.TryGetSprite(Const.PIXEL, out var sprite, true)) return;
		var tint = new Color32(200, 225, 255, alpha);
		int left = rect.x;
		int down = rect.y;
		int width = rect.width;
		int height = rect.height;
		seed = seed == 0 ? rect.x.ToUnit() * 1651243 + rect.y.ToUnit() * 128 : seed;
		int frame = Game.GlobalFrame;
		float frame01 = frame / 120f;
		float fixedFrame01 = frame01 * Const.CEL / height;
		for (int i = 0; i < count; i++) {
			float lerp01 = i / (float)count;
			if (lerp01 > frame01) break;
			if (Util.QuickRandom(0, 100) < 30) continue;
			int basicX = Util.QuickRandomWithSeed(seed + i * 21632);
			int _offsetX = offset.x == 0 ? 0 : offset.x * Util.QuickRandomWithSeed(seed + i * 891256, -2000, 2000) / 1000;
			int x = left + (basicX - _offsetX).UMod(width);
			int y = down + (((fixedFrame01 + lerp01) * height).RoundToInt() - offset.y).UMod(height);
			int _size = Util.QuickRandomWithSeed(seed + i * 1673 + 891237623, (size / 8).GreaterOrEquel(1), size);
			int rot = Util.QuickRandom(0, 360);
			Renderer.Draw(sprite, x, y, 500, 500, rot, _size, _size / 7, tint, z);
		}
	}


	/// <summary>
	/// Draw effect for electric light coming out of an object
	/// </summary>
	/// <param name="spriteID">Artwork sprite ID for a single lighten</param>
	/// <param name="rect">Position range in global space</param>
	/// <param name="count">Count of sprite at same time</param>
	/// <param name="size">Size of a single sprite</param>
	public static void DrawLightenEffect (int spriteID, IRect rect, int count = 2, int size = 196) {
		if (!Renderer.TryGetSprite(spriteID, out var sprite, ignoreAnimation: false)) return;
		for (int i = 0; i < count; i++) {
			size = Util.QuickRandom(size / 2, size);
			int x = rect.CenterX();
			int y = rect.CenterY();
			int rangeX = rect.width / 2;
			int rangeY = rect.height / 2;
			Renderer.Draw(
				sprite,
				Util.QuickRandom(x - rangeX, x + rangeX),
				Util.QuickRandom(y - rangeY, y + rangeY),
				500, 500,
				Util.QuickRandom(-150, 150),
				Util.QuickRandomSign() * size, size,
				z: int.MaxValue
			);
		}
	}


	/// <summary>
	/// Draw effect for object being poisoned
	/// </summary>
	/// <param name="spriteID">Artwork sprite ID</param>
	/// <param name="rect">Rect range in global space</param>
	/// <param name="loop">Duration of a single animation loop</param>
	/// <param name="count">Count of sprites at same time</param>
	/// <param name="seed">Seed to generate random value</param>
	/// <param name="size">Size of a single particle in global space</param>
	/// <param name="z">Z value for sort rendering cells</param>
	public static void DrawPoisonEffect (int spriteID, IRect rect, int loop = 120, int count = 4, int seed = 0, int size = 132, int z = int.MaxValue) {
		if (!Renderer.TryGetSprite(spriteID, out var sprite, ignoreAnimation: false)) return;
		int left = rect.x;
		int down = rect.y;
		int width = rect.width;
		int height = rect.height;
		seed = seed == 0 ? 1276344 + rect.x.ToUnit() * 217634 + rect.y.ToUnit() * 1235 : seed;
		int frame = Game.GlobalFrame;
		var tint = new Color32(200, 225, 255, (byte)(frame * 10).Clamp(0, 255));
		float frame01 = frame / (float)loop;
		float fixedFrame01 = frame01 * Const.CEL / height;
		int hHeight = height / 2;
		for (int i = 0; i < count; i++) {
			float lerp01 = i / (float)count;
			int localSeed = seed + ((int)(fixedFrame01 + lerp01)) * 8254;
			int x = left + (Util.QuickRandomWithSeed(localSeed + i * 21632, 0, width)).UMod(width);
			int y = down + ((fixedFrame01 + lerp01) * height).RoundToInt().UMod(height);
			int _size = Util.QuickRandomWithSeed(localSeed + i * 2512, size / 2, size);
			tint.a = (byte)((y - rect.y).PingPong(hHeight) * 512 / hHeight).Clamp(0, 255);
			Renderer.Draw(sprite, x, y, 500, 500, 0, _size, _size, tint, z);
		}
	}


}