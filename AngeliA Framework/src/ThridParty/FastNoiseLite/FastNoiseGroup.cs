using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using JordanPeck;

namespace AngeliA;


public class FastNoiseGroup (int length) {




	#region --- VAR ---


	// Api
	public readonly int Length = length;
	public FastNoiseLite this[int index] => Noises[index];
	public float SolidMin { get; set; } = 0f;
	public float SolidMax { get; set; } = 0.5f;

	// Data
	private readonly FastNoiseLite[] Noises = new FastNoiseLite[length].FillWithNewValue();


	#endregion




	#region --- API ---


	public void SetSeed (int seed) {
		for (int i = 0; i < Length; i++) {
			Noises[i].Seed += seed;
		}
	}


	public float GetNoise (float x, float y, float z) {
		float finalValue = Noises[0].GetNoise(x, y, z);
		for (int i = 1; i < Length; i++) {
			finalValue += Noises[i].GetNoise(x, y, z);
		}
		return finalValue;
	}


	public bool IsSolid (float value) => SolidMin < SolidMax ? value > SolidMin && value < SolidMax :
		value < SolidMax || value > SolidMin;


	// File
	public static FastNoiseGroup LoadFromFile (string path) {
		using var stream = File.OpenRead(path);
		using var reader = new BinaryReader(stream);

		try {

			float sMin = reader.ReadSingle();
			float sMax = reader.ReadSingle();
			int length = reader.ReadByte();
			var noiseGroup = new FastNoiseGroup(length);
			for (int i = 0; i < length; i++) {
				var noise = noiseGroup[i];

				noise.Seed = reader.ReadInt32();
				noise.Min = reader.ReadSingle();
				noise.Max = reader.ReadSingle();
				noise.Frequency = reader.ReadSingle();
				noise.NoiseType = (NoiseType)reader.ReadByte();
				noise.RotationType3D = (RotationType3D)reader.ReadByte();
				noise.FractalType = (FractalType)reader.ReadByte();
				noise.Octaves = reader.ReadInt32();
				noise.Lacunarity = reader.ReadSingle();
				noise.Gain = reader.ReadSingle();
				noise.WeightedStrength = reader.ReadSingle();
				noise.PingPongStrength = reader.ReadSingle();
				noise.CellularDistanceFunction = (CellularDistanceFunction)reader.ReadByte();
				noise.CellularReturnType = (CellularReturnType)reader.ReadByte();
				noise.CellularJitterModifier = reader.ReadSingle();
				noise.DomainWarpAmp = reader.ReadInt32();
				noise.DomainWarpType = (DomainWarpType)reader.ReadByte();

			}

			return noiseGroup;
		} catch (System.Exception ex) { Debug.LogException(ex); }

		return null;
	}


	public static void SaveToFile (FastNoiseGroup noiseGroup, string path) {
		using var stream = File.OpenWrite(path);
		using var writer = new BinaryWriter(stream);

		try {

			writer.Write((float)noiseGroup.SolidMin);
			writer.Write((float)noiseGroup.SolidMax);

			writer.Write((byte)noiseGroup.Length);
			for (int i = 0; i < noiseGroup.Length; i++) {
				var noise = noiseGroup.Noises[i];

				writer.Write((int)noise.Seed);
				writer.Write((float)noise.Min);
				writer.Write((float)noise.Max);
				writer.Write((float)noise.Frequency);
				writer.Write((byte)noise.NoiseType);
				writer.Write((byte)noise.RotationType3D);
				writer.Write((byte)noise.FractalType);
				writer.Write((int)noise.Octaves);
				writer.Write((float)noise.Lacunarity);
				writer.Write((float)noise.Gain);
				writer.Write((float)noise.WeightedStrength);
				writer.Write((float)noise.PingPongStrength);
				writer.Write((byte)noise.CellularDistanceFunction);
				writer.Write((byte)noise.CellularReturnType);
				writer.Write((float)noise.CellularJitterModifier);
				writer.Write((int)noise.DomainWarpAmp);
				writer.Write((byte)noise.DomainWarpType);

			}
		} catch (System.Exception ex) { Debug.LogException(ex); }


	}


	#endregion




}
