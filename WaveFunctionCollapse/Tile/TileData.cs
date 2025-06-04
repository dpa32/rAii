using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DirectionTileList : SerializableDictionary<Directions, StringListWrapper> { }

[Serializable]
public class TileData
{
    public string TileName;
    public GameObject Prefab;
    public Rotations Rotation;
    public float Weight = 1f; 

    public DirectionTileList CompatibleTiles = new();
}