using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomPropertyDrawer(typeof(StringListWrapper))]
public class StringListWrapperDrawer : PropertyDrawer
{
    private ReorderableList _list;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty listProp = property.FindPropertyRelative("list");

        if (_list == null || _list.serializedProperty != property)
        {
            _list = new ReorderableList(property.serializedObject, listProp, true, false, true, true);
            _list.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                var element = listProp.GetArrayElementAtIndex(index);
                EditorGUI.PropertyField(rect, element, GUIContent.none);
            };
            _list.elementHeight = EditorGUIUtility.singleLineHeight + 2;
        }

        _list.DoList(position);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        SerializedProperty listProp = property.FindPropertyRelative("list");
        if (_list == null || _list.serializedProperty != property)
        {
            _list = new ReorderableList(property.serializedObject, listProp, true, false, true, true);
        }

        return _list.GetHeight();
    }
}
