using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(TileRuleSet))]
public class TileRuleSetEditor : Editor
{
    private TileRuleSet _ruleSet;
    private int _selectedTileIndex = 0;

    private void OnEnable()
    {
        _ruleSet = (TileRuleSet)target;
    }

    public override void OnInspectorGUI()
    {
        if (_ruleSet.Tiles == null || _ruleSet.Tiles.Count == 0)
        {
            EditorGUILayout.HelpBox("Tiles list is enpty.", MessageType.Warning);
            if (GUILayout.Button("Reload"))
            {
                EditorUtility.SetDirty(_ruleSet);
            }
            return;
        }
        EditorGUILayout.ObjectField("This Asset", _ruleSet, typeof(TileRuleSet), false);
        // 타일 선택 드롭다운
        string[] tileNames = _ruleSet.Tiles.Select(t => t.TileName).ToArray();
        _selectedTileIndex = EditorGUILayout.Popup("Select Tile", _selectedTileIndex, tileNames);

        TileData selectedTile = _ruleSet.Tiles[_selectedTileIndex];

        EditorGUILayout.Space();

        EditorGUILayout.LabelField($"Tile: {selectedTile.TileName}", EditorStyles.boldLabel);

        // 각 방향별 호환 타일 목록 편집
        foreach (Directions dir in System.Enum.GetValues(typeof(Directions)))
        {
            EditorGUILayout.LabelField(dir.ToString());
            StringListWrapper compatibleList = selectedTile.CompatibleTiles.ContainsKey(dir) ? selectedTile.CompatibleTiles[dir] : new StringListWrapper();
            if (!selectedTile.CompatibleTiles.ContainsKey(dir))
            {
                selectedTile.CompatibleTiles[dir] = compatibleList;
            }

            // 호환 타일 체크박스 목록
            foreach (var tile in _ruleSet.Tiles)
            {
                bool isCompatible = compatibleList.List.Contains(tile.TileName);
                bool toggled = EditorGUILayout.ToggleLeft(tile.TileName, isCompatible);
                if (toggled && !isCompatible)
                    compatibleList.List.Add(tile.TileName);
                else if (!toggled && isCompatible)
                    compatibleList.List.Remove(tile.TileName);
            }
        }

        if (GUILayout.Button("Apply Reverse Rules"))
        {
            WFCTools.ApplyReverseRules(_ruleSet);
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(_ruleSet);
        }
    }
}
