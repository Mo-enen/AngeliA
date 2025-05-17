using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AngeliA;

/// <summary>
/// A chain of AngeSprites
/// </summary>
public class SpriteGroup {

	public const int MAX_COUNT = 256;
	/// <summary>
	/// How many sprites does this group have
	/// </summary>
	public int Count => Sprites.Count;

	public AngeSprite this[int index] => Sprites[index];

	/// <summary>
	/// Unique ID of this group
	/// </summary>
	public int ID;
	/// <summary>
	/// Unique name of this group
	/// </summary>
	public string Name;
	/// <summary>
	/// Sprite index this animation start to play after it reach the end. Set to -1 for not loop.
	/// </summary>
	public int LoopStart;
	/// <summary>
	/// Instance of sprite it holds
	/// </summary>
	public List<AngeSprite> Sprites = [];
	/// <summary>
	/// True if this group is animation group
	/// </summary>
	public bool Animated;
	/// <summary>
	/// True if this group contains sprite with auto tiling rule
	/// </summary>
	public bool WithRule;
	/// <summary>
	/// True if this group should apply random paiting brush in map editor
	/// </summary>
	public bool Random;

}
