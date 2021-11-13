using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestLivery : MonoBehaviour
{
    public CatColors colors;
    public ShipLivery livery;

    public int setCertainLivery = -1;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            SetLivery();
        }
    }

    void SetLivery()
    {
        int combo = Random.Range(0, colors.liveryColorCombinations.Length);
        int sail = Random.Range(0, colors.sailLiveries.Length);
        int hull = Random.Range(0, colors.hullLiveryTextures.Length);

        if (setCertainLivery >= 0)
            sail = setCertainLivery;
        livery.textToWrite = "001";
        livery.ApplyLivery(combo, sail, hull);
    }
}
