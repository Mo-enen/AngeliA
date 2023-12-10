using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;


namespace AngeliaFramework {


	[EntityAttribute.MapEditorGroup("MapGenerator")]
	public abstract class MapGeneratorElement : IMapEditorItem { }
	[EntityAttribute.AlsoKnownAs("MapGenerator_Solid")] public class RoomWall : MapGeneratorElement { }
	[EntityAttribute.AlsoKnownAs("MapGenerator_Tunnel")] public class RoomTunnel : MapGeneratorElement { }
	[EntityAttribute.AlsoKnownAs("MapGenerator_Connector")] public class RoomConnector : MapGeneratorElement { }


	public class RoomNode {
		public Room Room { get; init; }
		public RoomNode Base { get; init; }
		public List<RoomNode> Children { get; init; }
		public Dictionary<int, RoomMeta> Meta { get; init; }
		public RoomNode (Room room, RoomNode baseNode) {
			Room = room;
			Base = baseNode;
			Children = new();
			Meta = new();
		}
		public string PrintTree () {

			int indent = 0;
			var builder = new StringBuilder();
			AppendTree(this, builder, indent, 0);
			return builder.ToString();

			static void AppendTree (RoomNode node, StringBuilder builder, int indent, int indentAlt) {

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
					builder.Append(new string(' ', indentAlt));
					builder.Append(new string('│', indent - indentAlt));
					bool lastChild = i == node.Children.Count - 1;
					builder.Append(lastChild ? '└' : '├');
					AppendTree(node.Children[i], builder, indent + 1, lastChild ? indentAlt + 1 : indentAlt);
				}
			}
		}
	}


	public class RoomMeta {
		public int ID;
		public int EntityID;
		public int LevelID;
		public int BackgroundID;
		public RoomMeta (int id, int entityID, int levelID, int backgroundID) {
			ID = id;
			EntityID = entityID;
			LevelID = levelID;
			BackgroundID = backgroundID;
		}
	}


	public class Room {

		public struct Tunnel {
			public int LocalIndex;
			public int Size;
			public Direction4 Direction;
		}

		public struct Door {
			public int X;
			public int Y;
			public bool FrontDoor;
		}

		public int EdgeMinX => ContentMinX - 1;
		public int EdgeMinY => ContentMinY - 1;
		public int EdgeMaxX => ContentMaxX + 1;
		public int EdgeMaxY => ContentMaxY + 1;
		public int EdgeWidth => ContentWidth + 2;
		public int EdgeHeight => ContentHeight + 2;

		public int ID;
		public int ContentMinX;
		public int ContentMinY;
		public int ContentMaxX;
		public int ContentMaxY;
		public int ContentWidth;
		public int ContentHeight;
		public int[] EdgeLeft;
		public int[] EdgeRight;
		public int[] EdgeDown;
		public int[] EdgeUp;
		public int[] Entities;
		public int[] Levels;
		public int[] Backgrounds;
		public Tunnel[] Tunnels;
		public Door[] Doors;

		public override string ToString () => $"<color=#FFCC33>{ID}</color> {ContentWidth}×{ContentHeight}{(Tunnels.Length > 0 ? " " + new string('t', Tunnels.Length) : "")}{(Doors.Length > 0 ? " " + new string('d', Doors.Length) : "")}";

	}
}