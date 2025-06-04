using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using NUnit.Framework;

public static class WFCTools
{
    // 회전 타일 생성
    public static void CreateRotatedTiles(TileRuleSet ruleSet)
    {
        if (ruleSet == null)
        {
            return;
        }

        List<string> existingNames = ruleSet.Tiles.Select(t => t.TileName).ToList();
        List<TileData> newTiles = new();

        foreach (var original in ruleSet.Tiles.Where(t => t.Rotation == Rotations.Rot_0).ToList())
        {
            for (int i = 1; i < 4; i++)
            {
                var rot = (Rotations)i;
                string newName = original.TileName + $"_{rot}";

                if (existingNames.Contains(newName))
                {
                    continue;
                }

                var clone = new TileData
                {
                    TileName = newName,
                    Prefab = original.Prefab,
                    Rotation = rot,
                    CompatibleTiles = new DirectionTileList()
                };

                foreach (var pair in original.CompatibleTiles)
                {
                    var newDir = DirectionHelper.RotateDirection(pair.Key, i);
                    StringListWrapper value = new();
                    value.List = pair.Value.List.ToList();
                    clone.CompatibleTiles[newDir] = value;
                }

                newTiles.Add(clone);
            }
        }

        ruleSet.Tiles.AddRange(newTiles);
        ruleSet.Tiles.Sort((a, b) => string.Compare(a.TileName, b.TileName, StringComparison.Ordinal));

        EditorUtility.SetDirty(ruleSet);
        AssetDatabase.SaveAssets();
    }

    // 생성했던 타일 삭제
    public static void DeleteRotatedTiles(TileRuleSet ruleSet)
    {
        if (ruleSet == null)
        {
            return;
        }

        foreach (var tile in ruleSet.Tiles.ToList())
        {
            if (tile.Rotation == Rotations.Rot_0)
            {
                foreach (var comList in tile.CompatibleTiles.Values.ToList())
                {
                    foreach (var comTile in comList.List.Where(t => t.Contains("0")).ToList()) // 회전 버전과의 관계
                    {
                        comList.List.Remove(comTile);
                    }
                }
            }
            else
            {
                tile.CompatibleTiles = new();
                ruleSet.Tiles.Remove(tile);
            }
        }

        EditorUtility.SetDirty(ruleSet);
        AssetDatabase.SaveAssets();
    }

    public static void ApplyReverseRules(TileRuleSet ruleSet)
    {
        var tileDict = ruleSet.Tiles.ToDictionary(t => t.TileName);
        var addedCount = 0;

        // 기존 규칙 
        List<(string from, Directions dir, string to)> snapshot = new();

        foreach (var tile in ruleSet.Tiles)
        {
            foreach (var kvp in tile.CompatibleTiles)
            {
                var dir = kvp.Key;
                foreach (var to in kvp.Value.List)
                {
                    snapshot.Add((tile.TileName, dir, to));
                }
            }
        }

        // 역방향 추가
        foreach (var (from, dir, to) in snapshot)
        {
            if (!tileDict.TryGetValue(to, out var toTile) || !tileDict.TryGetValue(from, out var fromTile))
                continue;

            var reverseDir = DirectionHelper.GetOppositeDirection(dir);

            if (!toTile.CompatibleTiles.TryGetValue(reverseDir, out var listWrapper))
            {
                listWrapper = new StringListWrapper();
                toTile.CompatibleTiles[reverseDir] = listWrapper;
            }

            if (!listWrapper.List.Contains(from))
            {
                listWrapper.List.Add(from);
                addedCount++;
                Debug.Log($"[Reverse Added] {to} -> {reverseDir} -> {from}");
            }
        }

        Debug.Log($"Reverse rule : {addedCount}개 추가됨");

        EditorUtility.SetDirty(ruleSet);
        AssetDatabase.SaveAssets();
    }








    public static void ApplyRotationRules(TileRuleSet ruleSet)
    {
        var nameToTile = ruleSet.Tiles.ToDictionary(t => t.TileName);

        var originalLinks = new List<(TileData fromTile, Directions dir, TileData toTile)>();

        // 기존 타일 간의 연결 정보를 수집
        foreach (var tile in ruleSet.Tiles)
        {
            foreach (var (dir, wrapper) in tile.CompatibleTiles)
            {
                foreach (var toName in wrapper.List)
                {
                    if (!nameToTile.TryGetValue(toName, out var toTile)) continue;
                    originalLinks.Add((tile, dir, toTile));
                }
            }
        }

        foreach (var (fromTile, dir, toTile) in originalLinks)
        {
            string baseFrom = GetBaseTileName(fromTile.TileName);
            string baseTo = GetBaseTileName(toTile.TileName);
            int rotFrom = (int)fromTile.Rotation;
            int rotTo = (int)toTile.Rotation;

            int deltaRot = (4 + rotTo - rotFrom) % 4; // 상대 회전

            Debug.Log($"{fromTile.TileName}->{dir}->{toTile.TileName}");
            string fromName;
            string toName;
            for (int i = 0; i < 4; i++)
            {
                var fromRot = (Rotations)((rotFrom + i) % 4);
                var toRot = (Rotations)((rotTo + i) % 4);
                if (fromRot == 0)
                {
                    fromName = baseFrom;
                }
                else
                {
                    fromName = $"{baseFrom}_{fromRot}";
                }

                if (toRot == 0)
                {
                    toName = baseTo;
                }
                else
                {
                    toName = $"{baseTo}_{toRot}";
                }

                if (!nameToTile.TryGetValue(fromName, out var fromRotTile))
                {
                    Debug.Log($"{fromName} is null");
                    continue;
                }
                if (!nameToTile.TryGetValue(toName, out var toRotTile))
                {
                    Debug.Log($"{toName} is null");
                    continue;
                }

                var rotatedDir = DirectionHelper.RotateDirection(dir, i);

                if (!fromRotTile.CompatibleTiles.TryGetValue(rotatedDir, out var list))
                {
                    list = new StringListWrapper();
                    fromRotTile.CompatibleTiles[rotatedDir] = list;
                }

                if (!list.List.Contains(toRotTile.TileName))
                {
                    list.List.Add(toRotTile.TileName);
                    Debug.Log($"{toTile.TileName} Added");
                }
            }
        }
    }
    private static string GetBaseTileName(string fullName)
    {
        int index = fullName.IndexOf("_Rot_");
        return index >= 0 ? fullName.Substring(0, index) : fullName;
    }
}