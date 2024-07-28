using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AngeliA;

public class SpriteGroup {

	public const int MAX_COUNT = 256;
	public int Count => Sprites.Count;
	public int ID;
	public string Name;
	public int LoopStart;
	public List<AngeSprite> Sprites = new();
	public bool Animated;
	public bool WithRule;
	public bool Random;

}
