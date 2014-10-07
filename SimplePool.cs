using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

// This is a super dumb pooler, use with caution

public static class SimplePool 
{
    public static Dictionary<string, GameObject[]>      pools;
    public static int                                   poolSize    = 50;
    public static int                                   poolMargin  = 10;
    public static Dictionary<string, int>               poolCounts;
    public static Dictionary<string, HashSet<int>>      activeObjs;

    public static GameObject Catch (GameObject go, Transform t)
    {
        if (!PoolExists(go.name)) { CreatePool(go);       }
        if (PoolIsFull(go.name))  { IncreasePoolSize(go); }

        int i = 0;
        for (; i < pools[go.name].Length; i++)
        {
            int hc  = pools[go.name][i].GetHashCode();   
            
            if (activeObjs[go.name].Contains(hc)) { continue; }

            activeObjs[go.name].Add(hc);
            Transform  pT  = pools[go.name][i].GetComponent<Transform>();

            pT.position = t.position;
            pT.rotation = t.rotation;

            pools[go.name][i].SetActive(true);

            break;
        }
        // i should never be out of range here because we increase size 
        // and we don't need to worry about being thread safe
        return pools[go.name][i];
    }

    public static void Release (GameObject go)
    {
        activeObjs[go.name].Remove(go.GetHashCode());
        go.SetActive(false);
    }

    public static bool PoolExists (string pName)
    {
        return pools != null && pools.ContainsKey(pName);
    }

    public static bool PoolIsFull (string pName)
    {
        return activeObjs[pName].Count > pools[pName].Length - poolMargin;
    }

    public static void CreatePool (GameObject go)
    {
        if (pools == null) 
        { 
            pools      = new Dictionary<string, GameObject[]>();
            activeObjs = new Dictionary<string, HashSet<int>>();
        }

        pools[go.name] = CreateObjs(go);
        activeObjs[go.name] = new HashSet<int>();
    }

    public static void IncreasePoolSize (GameObject go)
    {
        int          currLen = pools[go.name].Length;
        GameObject[] nPool   = new GameObject[currLen + poolSize];
        GameObject[] newObjs = CreateObjs(go);
        
        Array.Copy(pools[go.name], 0, nPool, 0, currLen);
        Array.Copy(newObjs, 0, nPool, currLen, newObjs.Length);

        pools[go.name] = nPool;
    }

    public static GameObject[] CreateObjs (GameObject go)
    {
        GameObject[] gos = new GameObject[poolSize];

        for (int i = 0; i < poolSize; i++)
        {
            GameObject goInst = (GameObject) GameObject.Instantiate(   
                go, 
                Vector3.zero, 
                Quaternion.identity);

            goInst.name = go.name;
            goInst.SetActive(false);
            gos[i] = goInst;
        }
        return gos;
    }
}