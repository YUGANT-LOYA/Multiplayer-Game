using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabManager : MonoBehaviour
{
    private static PrefabManager _instance = null;
    public Item[] _items = null;

    public static PrefabManager singleton
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<PrefabManager>();
            }

            return _instance;
        }
    }

    public Item GetItemPrefab(string id)
    {
        if (_items != null)
        {
            for (int i = 0; i < _items.Length; i++)
            {
                if (_items[i] != null && _items[i].id.Trim() == id.Trim())
                {
                    //Debug.Log($"Item Found - {_items[i].id}");
                    return _items[i];
                }
            }
        }
        
        //Debug.Log("Item Not Found !");
        return null;
    }
}