using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapCreator : MonoBehaviour
{
    [SerializeField] private Transform panel;
    [SerializeField] private GameObject tilePrefab;

    
    private Tile[,] tiles = new Tile[10,10];

    void Start()
    {
        Generate();
    }

    void Generate()
    {
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                GameObject t = Instantiate(tilePrefab, Vector3.zero, Quaternion.identity, panel);
                RectTransform rt = t.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(i * 0.1f, j * 0.1f);
                rt.anchorMax = new Vector2((i + 1) * 0.1f, (j + 1) * 0.1f);
                rt.offsetMax = Vector2.zero;
                rt.offsetMin = Vector2.zero;

                Tile tile = t.GetComponent<Tile>();
                tile.x = i;
                tile.y = j;
                tiles[i, j] = tile;
            }
        }
    }
}