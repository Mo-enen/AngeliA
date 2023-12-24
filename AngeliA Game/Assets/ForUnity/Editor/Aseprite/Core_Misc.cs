using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaForUnity.Editor {




	public enum UserPivotMode {
		SlicePivotFirst = 0,
		IgnoreSlicePivot = 1,

	}



	public enum TextureExportMode {
		OneTexturePerAse = 0,
		OneTexturePerFrame = 1,

	}



	public enum PaddingMode {
		ClearColor = 0,
		NearbyColor = 1,
		NoPadding = 2,

	}



	public enum PaddingTarget {
		FrameEdge = 0,
		SliceEdge = 1,
		FrameAndSliceEdge = 2,

	}



	public class TaskResult {

		public class FrameResult {

			public static FrameResult GetEmpty (int frameIndex = 0) {
				return new FrameResult() {
					Height = 0,
					Width = 0,
					Pixels = new Byte4[0],
					Sprites = new SpriteMetaData[0],
					FrameIndex = frameIndex,
				};
			}

			public int FrameIndex;
			public int Width;
			public int Height;
			public Byte4[] Pixels;
			public SpriteMetaData[] Sprites;
			public int[] SpriteFrames = null;
		}



		public FrameResult[] Frames;
		public float[] Durations;
		public int TaskLayerCount;
		public int TaskLayerIndex;
		public string LayerName;

	}



	public class Pair2<T, U> {
		public T A;
		public U B;
		public Pair2 () { }
		public Pair2 (T a, U b) {
			A = a;
			B = b;
		}
	}



	public class Pair3<T, U, V> {
		public T A;
		public U B;
		public V C;
		public Pair3 () { }
		public Pair3 (T a, U b, V c) {
			A = a;
			B = b;
			C = c;
		}
	}



	public class Pair4<T, U, V, W> {
		public T A;
		public U B;
		public V C;
		public W D;
		public Pair4 () { }
		public Pair4 (T a, U b, V c, W d) {
			A = a;
			B = b;
			C = c;
			D = d;
		}
	}



}