using System.Collections;
using System.Collections.Generic;
using System.IO;


namespace AngeliaFramework {


	public enum GroupType {
		General = 0,
		Rule = 1,
		Random = 2,
		Animated = 3,
	}


	public class SpriteGroup {

		public int Count => SpriteIDs.Count;

		public int ID;
		public int LoopStart;
		public GroupType Type;
		public List<int> SpriteIDs;
		public List<int> Timings;

		public int GetSpriteIdFromAnimationFrame (int localFrame, int loopStart = -1) {

			int len = Timings.Count;

			// Fix Loopstart
			loopStart = loopStart < 0 ? LoopStart : loopStart;
			int loopStartTiming = loopStart == 0 ? 0 : Timings[(loopStart - 1).Clamp(0, len - 1)];
			int totalFrame = localFrame < loopStartTiming ? Timings[^1] : Timings[^1] - loopStartTiming;
			int frameOffset = localFrame < loopStartTiming ? 0 : loopStartTiming;
			localFrame = ((localFrame - frameOffset) % totalFrame) + frameOffset;

			// Get Target Index
			int targetIndex = 0;
			for (int i = 0; i < len; i++) {
				if (localFrame < Timings[i]) {
					targetIndex = i;
					break;
				}
			}
			return SpriteIDs[targetIndex];
		}

		public void LoadFromBinary_v0 (BinaryReader reader) {
			uint byteLen = reader.ReadUInt32();
			long endPos = reader.BaseStream.Position + byteLen;
			try {
				// ID
				ID = reader.ReadInt32();

				// Group Type
				Type = (GroupType)reader.ReadByte();

				if (Type == GroupType.Animated) {
					// Loop Start
					LoopStart = reader.ReadInt16();
				}

				// Sprite Indexes
				int spriteLen = reader.ReadUInt16();
				SpriteIDs = new List<int>(spriteLen);
				for (int i = 0; i < spriteLen; i++) {
					SpriteIDs.Add(reader.ReadInt32());
				}

				// Timing
				if (Type == GroupType.Animated) {
					Timings = new();
					for (int i = 0; i < spriteLen; i++) {
						Timings.Add(reader.ReadInt32().GreaterOrEquel(1));
					}
				}

			} catch (System.Exception ex) { Game.LogException(ex); }
			reader.BaseStream.Position = endPos;
		}

		public void SaveToBinary_v0 (BinaryWriter writer) {
			long markPos = writer.BaseStream.Position;
			writer.Write((uint)0);
			long startPos = writer.BaseStream.Position;
			try {

				// ID
				writer.Write((int)ID);

				// Group Type
				writer.Write((byte)Type);

				if (Type == GroupType.Animated) {
					// Loop Start
					writer.Write((short)LoopStart);
				}

				// Sprite Indexes
				int spriteCount = SpriteIDs.Count;
				writer.Write((ushort)spriteCount);
				for (int i = 0; i < spriteCount; i++) {
					writer.Write((int)SpriteIDs[i]);
				}

				// Timing
				if (Type == GroupType.Animated) {
					if (Timings != null && Timings.Count >= spriteCount) {
						for (int i = 0; i < spriteCount; i++) {
							writer.Write((int)Timings[i]);
						}
					} else {
						for (int i = 0; i < spriteCount; i++) {
							writer.Write(i < Timings.Count ? (int)Timings[i] : 1);
						}
					}
				}

			} catch (System.Exception ex) { Game.LogException(ex); }

			long endPos = writer.BaseStream.Position;
			writer.BaseStream.Position = markPos;
			writer.Write((uint)(endPos - startPos));
			writer.BaseStream.Position = endPos;
		}

	}
}
