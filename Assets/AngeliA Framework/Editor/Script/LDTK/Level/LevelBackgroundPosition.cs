

namespace LdtkToAngeliA {
	[System.Serializable]
	public partial class LevelBackgroundPosition {
		/// <summary>
		/// An array of 4 float values describing the cropped sub-rectangle of the displayed
		/// background image. This cropping happens when original is larger than the level bounds.
		/// Array format: `[ cropX, cropY, cropWidth, cropHeight ]`
		/// </summary>
		public double[] cropRect;

		/// <summary>
		/// An array containing the `[scaleX,scaleY]` values of the **cropped** background image,
		/// depending on `bgPos` option.
		/// </summary>
		public double[] scale;

		/// <summary>
		/// An array containing the `[x,y]` pixel coordinates of the top-left corner of the
		/// **cropped** background image, depending on `bgPos` option.
		/// </summary>
		public long[] topLeftPx;
	}
}
