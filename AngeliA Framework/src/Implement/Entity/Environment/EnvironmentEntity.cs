using System.Collections;
using System.Collections.Generic;


namespace AngeliA; 

public enum FittingPose {
	Unknown = 0,
	Left = 1,
	Down = 1,
	Mid = 2,
	Right = 3,
	Up = 3,
	Single = 4,
}


[EntityAttribute.Layer(EntityLayer.ENVIRONMENT)]
[RequireSprite("{0}")]
public abstract class EnvironmentRigidbody : Rigidbody { }


[EntityAttribute.Layer(EntityLayer.ENVIRONMENT)]
[RequireSprite("{0}")]
public abstract class EnvironmentEntity : Entity {



	private static int CellStartIndex = int.MaxValue;
	private static int CellUpdateFrame = -1;


	[BeforeLevelRendered]
	public static void BeforeLevelRendered () => BeforeLayerFrameUpdate(EntityLayer.ENVIRONMENT);


	[AfterLevelRendered]
	public static void AfterLevelRendered () => AfterLayerFrameUpdate(EntityLayer.ENVIRONMENT);


	[BeforeLayerFrameUpdate]
	public static void BeforeLayerFrameUpdate (int layerIndex) {
		if (layerIndex != EntityLayer.ENVIRONMENT) return;
		CellStartIndex = Renderer.GetUsedCellCount(RenderLayer.DEFAULT);
		CellUpdateFrame = Game.GlobalFrame;
	}


	[AfterLayerFrameUpdate]
	public static void AfterLayerFrameUpdate (int layerIndex) {
		if (layerIndex != EntityLayer.ENVIRONMENT || CellUpdateFrame != Game.GlobalFrame) return;
		if (MapEditor.IsEditing) return;
		if (Renderer.GetCells(RenderLayer.DEFAULT, out var cells, out int count)) {
			Renderer.SetLayerToShadow();
			for (int i = CellStartIndex; i < count; i++) {
				var cell = cells[i];
				FrameworkUtil.DrawEnvironmentShadow(cell, z: cell.Z - 1);
			}
			Renderer.SetLayerToDefault();
		}
	}


	protected static FittingPose GetEntityPose (int typeID, int unitX, int unitY, bool horizontal) {
		bool n = WorldSquad.Front.GetBlockAt(horizontal ? unitX - 1 : unitX, horizontal ? unitY : unitY - 1, BlockType.Entity) == typeID;
		bool p = WorldSquad.Front.GetBlockAt(horizontal ? unitX + 1 : unitX, horizontal ? unitY : unitY + 1, BlockType.Entity) == typeID;
		return
			n && p ? FittingPose.Mid :
			!n && p ? FittingPose.Left :
			n && !p ? FittingPose.Right :
			FittingPose.Single;
	}


	protected static FittingPose GetEntityPose (Entity entity, bool horizontal, int mask, out Entity left_down, out Entity right_up, OperationMode mode = OperationMode.ColliderOnly, int tag = 0) {
		left_down = null;
		right_up = null;
		int unitX = entity.X.ToUnit();
		int unitY = entity.Y.ToUnit();
		int typeID = entity.TypeID;
		bool n = WorldSquad.Front.GetBlockAt(horizontal ? unitX - 1 : unitX, horizontal ? unitY : unitY - 1, BlockType.Entity) == typeID;
		bool p = WorldSquad.Front.GetBlockAt(horizontal ? unitX + 1 : unitX, horizontal ? unitY : unitY + 1, BlockType.Entity) == typeID;
		if (n) {
			left_down = Physics.GetEntity(typeID, entity.Rect.EdgeOutside(horizontal ? Direction4.Left : Direction4.Down), mask, entity, mode, tag);
		}
		if (p) {
			right_up = Physics.GetEntity(typeID, entity.Rect.EdgeOutside(horizontal ? Direction4.Right : Direction4.Up), mask, entity, mode, tag);
		}
		return
			n && p ? FittingPose.Mid :
			!n && p ? FittingPose.Left :
			n && !p ? FittingPose.Right :
			FittingPose.Single;
	}


}
