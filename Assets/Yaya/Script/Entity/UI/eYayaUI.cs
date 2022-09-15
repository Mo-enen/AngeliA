using AngeliaFramework;
namespace Yaya {
	public abstract class eYayaUI : UiEntity {
		protected override int LayerID => YayaConst.SHADER_UI;
	}
	public abstract class eYayaScreenUI : ScreenUI {
		protected override int LayerID => YayaConst.SHADER_UI;
	}
}