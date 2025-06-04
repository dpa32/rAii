using System.Collections;
using UnityEngine;

public class WFCRunner : MonoBehaviour
{
    public TileRuleSet ruleSet;
    public GameObject defaultTilePrefab;
    public Transform tileParent;
    public int width = 10;
    public int height = 10;

    private WFCGrid grid;
    private WFCVisualizer visualizer;

    void Start()
    {
        StartWFC();
    }

    public void StartWFC()
    {
        grid = new WFCGrid(width, height, ruleSet);
        visualizer = new WFCVisualizer(grid, tileParent.gameObject, defaultTilePrefab);
        StartCoroutine(RunWFC());
    }

    public IEnumerator RunWFC()
    {
        while (true)
        {
            var collapsedPos = grid.CollapseStep();
            if (collapsedPos == null)
                break;

            grid.PropagateConstraints(collapsedPos.Value);
            visualizer.UpdateDisplay();

            yield return new WaitForSeconds(0.05f);
        }

        Debug.Log("WFC ¿Ï·á!");
    }
}
