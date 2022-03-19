namespace Yaya {
	public class eSpringH : eSpring {
		protected override bool Horizontal => true;
		public override int PushLevel => int.MaxValue - 1;
	}
}