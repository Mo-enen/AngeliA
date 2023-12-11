using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;


namespace AngeliaFramework {


	[EntityAttribute.MapEditorGroup("MapGenerator")]
	public abstract class MapGeneratorElement : IMapEditorItem { }
	public class RoomWall : MapGeneratorElement { }
	public class RoomTunnel : MapGeneratorElement { }
	public class RoomConnector : MapGeneratorElement { }


	public class RoomNode {
		public Room Room { get; init; }
		public RoomNode Base { get; init; }
		public List<RoomNode> Children { get; init; }
		public Dictionary<int, Cubicle> Meta { get; init; }
		public RoomNode (Room room, RoomNode baseNode) {
			Room = room;
			Base = baseNode;
			Children = new();
			Meta = new();
		}
		public string PrintTree () {

			var builder = new StringBuilder();
			AppendTree(this, builder, 0);
			return builder.ToString();

			static void AppendTree (RoomNode node, StringBuilder builder, int indent) {

				// Room ID
				if (node.Room != null) {
					builder.Append($" {node.Room}");
				} else {
					builder.Append('?');
				}

				// Meta
				builder.Append(' ');
				builder.Append(new string('m', node.Meta.Count));

				// Line
				builder.AppendLine();

				// Children
				for (int i = 0; i < node.Children.Count; i++) {
					builder.Append(new string('\t', indent));
					AppendTree(node.Children[i], builder, indent + 1);
				}
			}
		}
	}


	public class Cubicle {
		public int ID;
		public int ContentMinX;
		public int ContentMinY;
		public int ContentWidth;
		public int ContentHeight;
		public int[] Entities;
		public int[] Levels;
		public int[] Backgrounds;
		public int ContentMaxX => ContentMinX + ContentWidth - 1;
		public int ContentMaxY => ContentMinY + ContentHeight - 1;
	}


	public class Room : Cubicle {

		public struct Tunnel {
			public int Index;
			public int Size;
			public Direction4 Direction;
		}

		public struct Door {
			public int X;
			public int Y;
			public bool Front;
		}

		public int EdgeMinX => ContentMinX - 1;
		public int EdgeMinY => ContentMinY - 1;
		public int EdgeMaxX => ContentMaxX + 1;
		public int EdgeMaxY => ContentMaxY + 1;
		public int EdgeWidth => ContentWidth + 2;
		public int EdgeHeight => ContentHeight + 2;

		public int[] EdgeLeft;
		public int[] EdgeRight;
		public int[] EdgeDown;
		public int[] EdgeUp;
		public Tunnel[] Tunnels;
		public Door[] Doors;

		public int[] GetEdge (Direction4 direction) => direction switch {
			Direction4.Up => EdgeUp,
			Direction4.Down => EdgeDown,
			Direction4.Left => EdgeLeft,
			Direction4.Right => EdgeRight,
			_ => EdgeDown,
		};

		public override string ToString () => $"<color=#FFCC33>{ID}</color> {ContentWidth}×{ContentHeight}{(Tunnels.Length > 0 ? " " + new string('t', Tunnels.Length) : "")}{(Doors.Length > 0 ? " " + new string('d', Doors.Length) : "")}";

	}
}