using System.Collections;
using System.Collections.Generic;
using System.Text;


namespace AngeliaFramework {


	[EntityAttribute.MapEditorGroup("MapGenerator")]
	public abstract class MapGeneratorElement : IMapItem { }
	public class RoomWall : MapGeneratorElement { }
	public class RoomTunnel : MapGeneratorElement { }
	public class RoomConnector : MapGeneratorElement { }


	public class RoomNode {
		private class RoomNodeComparer : IComparer<RoomNode> {
			public static readonly RoomNodeComparer Instance = new();
			public int Compare (RoomNode a, RoomNode b) {
				int result = a.Room.TypeID.CompareTo(b.Room.TypeID);
				if (result == 0) result = a.Room.ID.CompareTo(b.Room.ID);
				return result;
			}
		}
		public Room Room { get; init; }
		public RoomNode Base { get; init; }
		public List<RoomNode> Children { get; init; }
		public Dictionary<int, Cubicle> Meta { get; init; }
		public RoomNode (Room room, RoomNode baseNode) {
			Room = room ?? Room.EMPTY;
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
				builder.Append(" <color=#888888FF>");
				builder.Append(new string('m', node.Meta.Count));
				builder.Append("</color>");

				// Line
				builder.AppendLine();

				// Children
				for (int i = 0; i < node.Children.Count; i++) {
					builder.Append(new string('\t', indent));
					AppendTree(node.Children[i], builder, indent + 1);
				}
			}
		}
		public void SortAllChildren () {
			Children.Sort(RoomNodeComparer.Instance);
			foreach (var child in Children) {
				child.SortAllChildren();
			}
		}
	}


	public class Cubicle {
		public int ID;
		public int TypeID;
		public int ContentMinX;
		public int ContentMinY;
		public int ContentWidth;
		public int ContentHeight;
		public int[] Entities;
		public int[] Levels;
		public int[] Backgrounds;
		public int[] Elements;
		public int ContentMaxX => ContentMinX + ContentWidth - 1;
		public int ContentMaxY => ContentMinY + ContentHeight - 1;
	}


	public class Room : Cubicle {

		public static readonly Room EMPTY = new();

		public struct Tunnel {
			public int Index;
			public int Size;
			public Direction4 Direction;
		}

		public struct Teleporter {
			public int X;
			public int Y;
			public bool Front;
			public bool IsPortal;
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
		public Teleporter[] Teleporters;

		public int[] GetEdge (Direction4 direction) => direction switch {
			Direction4.Up => EdgeUp,
			Direction4.Down => EdgeDown,
			Direction4.Left => EdgeLeft,
			Direction4.Right => EdgeRight,
			_ => EdgeDown,
		};

		public override string ToString () => $"<color=#FFCC33>{ID}</color> {(TypeID != 0 ? "<size=75%>Ⓣ</size> " : "")}{ContentWidth}×{ContentHeight}<color=#888888FF>{(Tunnels.Length > 0 ? " " + new string('t', Tunnels.Length) : "")}{(Teleporters.Length > 0 ? " " + new string('d', Teleporters.Length) : "")}</color>";

	}
}