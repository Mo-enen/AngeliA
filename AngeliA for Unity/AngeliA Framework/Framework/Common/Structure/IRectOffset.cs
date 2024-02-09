namespace AngeliaFramework {
	public class IRectOffset {

		public int left;
		public int right;
		public int bottom;
		public int top;
		public int horizontal => left + right;
		public int vertical => bottom + top;

		public IRectOffset (int left, int right, int bottom, int top) {
			this.left = left;
			this.right = right;
			this.bottom = bottom;
			this.top = top;
		}

	}
}
