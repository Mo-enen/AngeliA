using UnityEngine;
using UnityEditor;
namespace Moenen.Editor {
	[CustomEditor(typeof(MonoBehaviour), true)]
	public class MoenenMonoInspector : UnityEditor.Editor {
		public override void OnInspectorGUI () {
			serializedObject.Update();
			DrawPropertiesExcluding(serializedObject, "m_Script");
			serializedObject.ApplyModifiedProperties();
		}
	}
	[CustomEditor(typeof(ScriptableObject), true)]
	public class MoenenScriptableInspector : UnityEditor.Editor {
		public override void OnInspectorGUI () {
			serializedObject.Update();
			DrawPropertiesExcluding(serializedObject, "m_Script");
			serializedObject.ApplyModifiedProperties();
		}
	}
}
