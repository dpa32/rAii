using System.Collections.Generic;
using UnityEngine;

public class WFCVisualizer
{
    private readonly WFCGrid grid;
    private readonly GameObject parent;
    private readonly GameObject defaultPrefab;
    private readonly Dictionary<Position, GameObject> spawnedTiles = new();

    public WFCVisualizer(WFCGrid grid, GameObject parent, GameObject defaultPrefab)
    {
        this.grid = grid;
        this.parent = parent;
        this.defaultPrefab = defaultPrefab;
    }

    public void UpdateDisplay()
    {
        foreach (var cell in grid.Cells)
        {
            if (spawnedTiles.ContainsKey(cell.Position))
                continue;

            if (!cell.IsCollapsed)
                continue;

            var tile = cell.PossibleTiles.Count > 0 ? cell.PossibleTiles[0] : null;

            if (tile == null || tile.Prefab == null)
            {
                // 빈 칸이거나 시각화 안 함 (또는 투명 prefab 사용 가능)
                GameObject empty = GameObject.CreatePrimitive(PrimitiveType.Cube);
                empty.transform.localScale = new Vector3(1f, 0.05f, 1f);
                empty.GetComponent<Renderer>().material.color = new Color(1f, 1f, 1f, 0.2f); // 반투명
                Object.Destroy(empty.GetComponent<Collider>());
                empty.transform.position = new Vector3(cell.Position.X, 0, cell.Position.Y);
                empty.transform.SetParent(parent.transform);
                spawnedTiles[cell.Position] = empty;
                continue;
            }

            GameObject obj = Object.Instantiate(tile.Prefab,
                new Vector3(cell.Position.X, 0, cell.Position.Y),
                Quaternion.identity,
                parent.transform);

            spawnedTiles[cell.Position] = obj;
        }
    }
}
