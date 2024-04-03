using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AngeliA;

public class SpriteGroup {

	public const int MAX_COUNT = 256;
	public int Count => SpriteIDs.Count;
	public int this[int index] {
		get => SpriteIDs[index];
		set => SpriteIDs[index] = value;
	}

	public int ID;
	public string Name;
	public int LoopStart;
	public List<int> SpriteIDs;
	public bool Animated;
	public bool WithRule;
	public bool Random;
	
	public void Add (int spriteID) => SpriteIDs.Add(spriteID);
	public void RemoveAt (int index) => SpriteIDs.RemoveAt(index);

}
