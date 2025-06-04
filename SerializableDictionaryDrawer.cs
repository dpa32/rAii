using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(SerializableDictionary<,>), true)]
public class SerializableDictionaryDrawer : PropertyDrawer
{
    private static float _lineHeight => EditorGUIUtility.singleLineHeight;
    private static float _verticalSpacing = 4f;

    // Foldout 상태 저장
    private Dictionary<string, bool> _foldoutStates = new();

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        string foldoutKey = property.propertyPath;

        if (!_foldoutStates.ContainsKey(foldoutKey))
            _foldoutStates[foldoutKey] = true;

        EditorGUI.BeginProperty(position, label, property);

        // Foldout UI
        _foldoutStates[foldoutKey] = EditorGUI.Foldout(
            new Rect(position.x, position.y, position.width, _lineHeight),
            _foldoutStates[foldoutKey],
            label,
            true
        );

        if (_foldoutStates[foldoutKey])
        {
            SerializedProperty keysProp = property.FindPropertyRelative("keys");
            SerializedProperty valuesProp = property.FindPropertyRelative("values");

            if (keysProp == null || valuesProp == null)
            {
                EditorGUI.LabelField(new Rect(position.x, position.y + _lineHeight, position.width, _lineHeight),
                    "Cannot find keys or values property.");
                EditorGUI.EndProperty();
                return;
            }

            float y = position.y + _lineHeight + _verticalSpacing;

            // + Add 버튼
            Rect addButtonRect = new Rect(position.x, y, 60, _lineHeight);
            if (GUI.Button(addButtonRect, "+ Add"))
            {
                keysProp.arraySize++;
                valuesProp.arraySize++;

                keysProp.GetArrayElementAtIndex(keysProp.arraySize - 1).Reset();
                valuesProp.GetArrayElementAtIndex(valuesProp.arraySize - 1).Reset();
                property.serializedObject.ApplyModifiedProperties();
            }

            y += _lineHeight + _verticalSpacing;

            // 각 요소 렌더링
            for (int i = 0; i < keysProp.arraySize; i++)
            {
                SerializedProperty keyProp = keysProp.GetArrayElementAtIndex(i);
                SerializedProperty valueProp = valuesProp.GetArrayElementAtIndex(i);

                float rowHeight = Mathf.Max(EditorGUI.GetPropertyHeight(keyProp), EditorGUI.GetPropertyHeight(valueProp));

                Rect keyRect = new Rect(position.x, y, position.width * 0.4f - 5, rowHeight);
                Rect valueRect = new Rect(position.x + position.width * 0.4f, y, position.width * 0.55f - 30, rowHeight);
                Rect removeRect = new Rect(position.x + position.width - 25, y, 20, _lineHeight);

                // 키 중복 검사
                bool duplicate = false;
                for (int j = 0; j < keysProp.arraySize; j++)
                {
                    if (j == i) continue;
                    if (SerializedProperty.DataEquals(keyProp, keysProp.GetArrayElementAtIndex(j)))
                    {
                        duplicate = true;
                        break;
                    }
                }

                // 키 색상 처리
                Color oldColor = GUI.color;
                GUI.color = duplicate ? Color.red : oldColor;
                EditorGUI.PropertyField(keyRect, keyProp, GUIContent.none);
                GUI.color = oldColor;

                EditorGUI.PropertyField(valueRect, valueProp, GUIContent.none);

                // 삭제 버튼
                if (GUI.Button(removeRect, "X"))
                {
                    keysProp.DeleteArrayElementAtIndex(i);
                    valuesProp.DeleteArrayElementAtIndex(i);
                    property.serializedObject.ApplyModifiedProperties();
                    break;
                }

                y += rowHeight + _verticalSpacing;
            }
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        string foldoutKey = property.propertyPath;
        if (!_foldoutStates.TryGetValue(foldoutKey, out bool foldout) || !foldout)
            return _lineHeight + _verticalSpacing;

        SerializedProperty keysProp = property.FindPropertyRelative("keys");
        SerializedProperty valuesProp = property.FindPropertyRelative("values");

        if (keysProp == null || valuesProp == null)
            return _lineHeight * 2;

        float height = _lineHeight + _verticalSpacing + _lineHeight + _verticalSpacing;

        for (int i = 0; i < keysProp.arraySize; i++)
        {
            float keyHeight = EditorGUI.GetPropertyHeight(keysProp.GetArrayElementAtIndex(i));
            float valueHeight = EditorGUI.GetPropertyHeight(valuesProp.GetArrayElementAtIndex(i));
            height += Mathf.Max(keyHeight, valueHeight) + _verticalSpacing;
        }

        return height;
    }
}
