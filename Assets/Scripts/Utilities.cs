﻿using System.Collections.Generic;
using UnityEngine;

public class Utilities
{

    public delegate int RandomFunction(int min, int excludingMax);

    public static void InitializeArray<T>(ref T[] array, T value)
    {
        for (int i = 0; i < array.Length; ++i)
        {
            array[i] = value;
        }
    }

    public static void InitializeIteratorArray(ref int[] array)
    {
        for (int i = 0; i < array.Length; ++i)
        {
            array[i] = i;
        }
    }

    public static int[] IteratorArray(int length)
    {
        int[] a = new int[length];
        InitializeIteratorArray(ref a);
        return a;
    }

    public static void RandomizeArray<T>(ref T[] array, RandomFunction rand)
    {
        for (int i = array.Length - 1; i > 0; --i)
        {
            int n = rand(0, i + 1);
            T aux = array[i];
            array[i] = array[n];
            array[n] = aux;
        }
    }

    public static void RandomizeList<T>(ref List<T> array, RandomFunction rand)
    {
        for (int i = array.Count - 1; i > 0; --i)
        {
            int n = rand(0, i + 1);
            T aux = array[i];
            array[i] = array[n];
            array[n] = aux;
        }
    }

    public static void DebugLogArray<T>(T[] array)
    {
        for (int i = 0; i < array.Length; ++i)
        {
            Debug.Log("[" + i + "]" + array[i]);
        }
    }

    public static void DebugLogList<T>(List<T> array)
    {
        for (int i = 0; i < array.Count; ++i)
        {
            Debug.Log("[" + i + "]" + array[i]);
        }
    }

    public static Vector3 NormalFromAngle(float a)
    {
        return new Vector3(-Mathf.Sin(a), Mathf.Cos(a), 0f);
    }

    public static T RandomValue<T>(T[] a)
    {
        return a[Random.Range(0, a.Length)];
    }

    public static T RandomValue<T>(List<T> a)
    {
        return a[Random.Range(0, a.Count)];
    }

    // Gives a random value between a series of arrays
    public static T RandomValue<T>(params List<T>[] a)
    {
        int count = 0;
        for (int i = 0; i < a.Length; ++i)
        {
            count += a[i].Count;
        }

        int random = Random.Range(0, count);

        int which = 0;
        while (which < a.Length && random >= a[which].Count)
        {
            random -= a[which].Count;
            which++;
        }

        return a[which][random];
    }

    public delegate int RandomGenerator(int max, int min);

    public static T RandomValue<T>(RandomGenerator rand, params List<T>[] a)
    {
        int count = 0;
        for (int i = 0; i < a.Length; ++i)
        {
            count += a[i].Count;
        }

        int random = rand(0, count);

        int which = 0;
        while (which < a.Length && random >= a[which].Count)
        {
            random -= a[which].Count;
            which++;
        }

        return a[which][random];
    }
}
