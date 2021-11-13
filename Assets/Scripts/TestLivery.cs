using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestLivery : MonoBehaviour
{
    public CatColors colors;
    public ShipLivery livery;

    public int setCertainLivery = -1;

    int combo = 0;
    int sail = 0;
    int hull = 0;

    private void Start()
    {
        SetLivery();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            int combo = Random.Range(0, colors.liveryColorCombinations.Length);
            int sail = Random.Range(0, colors.sailLiveries.Length);
            int hull = Random.Range(0, colors.hullLiveryTextures.Length);

            SetLivery();
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            combo = (combo + 1) % colors.liveryColorCombinations.Length;
            SetLivery();
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            sail = (sail + 1) % colors.sailLiveries.Length;
            SetLivery();
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            hull = (hull + 1) % colors.hullLiveryTextures.Length;
            SetLivery();
        }
    }

    void SetLivery()
    {
        Debug.Log(string.Format("Appplying livery: combo {0}, sail {1}, hull {2}", combo, sail, hull));
        livery.textToWrite = "001";
        livery.ApplyLivery(combo, sail, hull);
    }
}
