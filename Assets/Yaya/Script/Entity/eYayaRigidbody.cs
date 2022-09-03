namespace Yaya {
	public class eYayaRigidbody : Rigidbody {
		public override int CollisionMask => YayaConst.MASK_SOLID;
		public override int Mask_Level => YayaConst.MASK_LEVEL;
		public override int Water_Tag => YayaConst.WATER_TAG;
		public override int QuickSand_Tag => YayaConst.QUICKSAND_TAG;
	}
}