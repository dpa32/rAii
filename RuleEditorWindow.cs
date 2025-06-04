using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class RuleEditorWindow : EditorWindow
{
    private TileRuleSet _currentRuleSet;
    private Vector2 _optionListScroll;
    private Vector2 _tileListScroll;
    private Vector2 _neighborScroll;

    private int _selectedTileIndex = -1;

    [MenuItem("WFC/Rule Editor Window")]
    public static void OpenWindow()
    {
        GetWindow<RuleEditorWindow>("WFC Rule Editor");
    }

    private void OnGUI()
    {
        EditorGUILayout.Space();

        // 룰셋 할당 필드
        _currentRuleSet = (TileRuleSet)EditorGUILayout.ObjectField("Tile Rule Set", _currentRuleSet, typeof(TileRuleSet), false);

        if (_currentRuleSet == null)
        {
            EditorGUILayout.HelpBox("Select TileRuleSet", MessageType.Info);
            return;
        }

        _optionListScroll = EditorGUILayout.BeginScrollView(_optionListScroll);

        EditorGUILayout.Space();

        if (GUILayout.Button("Apply Rotation Rules"))
        {
            if (EditorUtility.DisplayDialog("Notice", "Apply Rotation Rules?", "Go", "Cancle"))
            {
                WFCTools.ApplyRotationRules(_currentRuleSet);
                EditorUtility.SetDirty(_currentRuleSet);
                AssetDatabase.SaveAssets();
                Debug.Log("Apply Rotation Rules Successed");
            }
        }

        if (GUILayout.Button("Apply Reverse Rules"))
        {
            if (EditorUtility.DisplayDialog("Notice", "Apply Reverse Rules?", "Go", "Cancle"))
            {
                WFCTools.ApplyReverseRules(_currentRuleSet);
                EditorUtility.SetDirty(_currentRuleSet);
                AssetDatabase.SaveAssets();
                Debug.Log("Apply Reverse Rules Successed");
            }
        }

        if (GUILayout.Button("Create Rotated Tiles"))
        {
            if (EditorUtility.DisplayDialog("Notice", "Create Rotated Tiles?", "Go", "Cancle"))
            {
                WFCTools.CreateRotatedTiles(_currentRuleSet);
                EditorUtility.SetDirty(_currentRuleSet);
                AssetDatabase.SaveAssets();
                Debug.Log("Create Rotated Tiles Successed");
            }
        }
        if (GUILayout.Button("Delete Rotated Tiles"))
        {
            if (EditorUtility.DisplayDialog("Notice", "Delete Rotated Tiles?", "Go", "Cancle"))
            {
                WFCTools.DeleteRotatedTiles(_currentRuleSet);
                EditorUtility.SetDirty(_currentRuleSet);
                AssetDatabase.SaveAssets();
                Debug.Log("Delete Rotated Tiles Successed");
            }
        }

        EditorGUILayout.Space(5);
        EditorGUILayout.BeginHorizontal();
        DrawTileList();
        DrawSelectedTileNeighbors();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndScrollView();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(_currentRuleSet);
        }
    }

    void DrawTileList()
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(200));
        EditorGUILayout.LabelField("Tiles", EditorStyles.boldLabel);

        _tileListScroll = EditorGUILayout.BeginScrollView(_tileListScroll);

        for (int i = 0; i < _currentRuleSet.Tiles.Count; i++)
        {
            var tile = _currentRuleSet.Tiles[i];
            if (GUILayout.Button(tile.TileName, (i == _selectedTileIndex) ? EditorStyles.whiteLabel : EditorStyles.label))
            {
                _selectedTileIndex = i;
            }
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    void DrawSelectedTileNeighbors()
    {
        EditorGUILayout.BeginVertical();

        if (_selectedTileIndex < 0 || _selectedTileIndex >= _currentRuleSet.Tiles.Count)
        {
            EditorGUILayout.LabelField("Select Tile");
            EditorGUILayout.EndVertical();
            return;
        }

        var tile = _currentRuleSet.Tiles[_selectedTileIndex];

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"Selected Tile: {tile.TileName}", EditorStyles.boldLabel);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        _neighborScroll = EditorGUILayout.BeginScrollView(_neighborScroll);

        Directions[] dirs = { Directions.North, Directions.West, Directions.East, Directions.South };

        foreach (Directions dir in dirs)
        {
            if (dir == Directions.North || dir == Directions.South)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.Space(100);
                EditorGUILayout.BeginVertical();
            }
            else
            {
                if (dir == Directions.West)
                {
                    EditorGUILayout.BeginHorizontal();
                }
                EditorGUILayout.BeginVertical();

            }
            EditorGUILayout.LabelField(dir.ToString(), EditorStyles.boldLabel);

            if (dir == Directions.North || dir == Directions.South)
            {
                DrawNeighhborsGroupByDir(dir, tile);
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                DrawNeighhborsGroupByDir(dir, tile);

                EditorGUILayout.EndVertical();
                if (dir == Directions.East)
                {
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.Space(5);
            }
        }

        EditorGUILayout.EndScrollView();

        EditorGUILayout.EndVertical();
    }

    private void DrawNeighhborsGroupByDir(Directions dir, TileData tile)
    {
        // 현재 방향에 호환 가능한 타일 리스트
        StringListWrapper neighbors = tile.CompatibleTiles.ContainsKey(dir) ? tile.CompatibleTiles[dir] : null;
        if (neighbors == null)
        {
            neighbors = new StringListWrapper();
            tile.CompatibleTiles[dir] = neighbors;
        }

        foreach (var otherTile in _currentRuleSet.Tiles)
        {
            bool hasNeighbor = neighbors.List.Contains(otherTile.TileName);
            bool toggled = EditorGUILayout.ToggleLeft(otherTile.TileName, hasNeighbor);
            if (toggled && !hasNeighbor)
            {
                neighbors.List.Add(otherTile.TileName);
            }
            else if (!toggled && hasNeighbor)
            {
                neighbors.List.Remove(otherTile.TileName);
            }
        }

        EditorGUILayout.Space();
    }
}