using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(Vector3d))]
public class Vector3dDrawer : PropertyDrawer {

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
		return Screen.width < 333 ? ( 16f + 18f ) : 16f;
	}

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		label = EditorGUI.BeginProperty(position, label, property);
		Rect contentRect = EditorGUI.PrefixLabel(position, label);
		if (position.height > 16) {
			position.height = 16f;
			EditorGUI.indentLevel++;
			contentRect = EditorGUI.IndentedRect(position);
			contentRect.y += 18f;
		}
		contentRect.width *= 1f / 3f;
		EditorGUI.indentLevel = 0;
		EditorGUIUtility.labelWidth = 14f;
		EditorGUI.PropertyField(contentRect, property.FindPropertyRelative("x"), new GUIContent("x"));
		contentRect.x += contentRect.width;
		EditorGUI.PropertyField(contentRect, property.FindPropertyRelative("y"), new GUIContent("y"));
		contentRect.x += contentRect.width;
		EditorGUI.PropertyField(contentRect, property.FindPropertyRelative("z"), new GUIContent("z"));
		EditorGUI.EndProperty();
	}
}
