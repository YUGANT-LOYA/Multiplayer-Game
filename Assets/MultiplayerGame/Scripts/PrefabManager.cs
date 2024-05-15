using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabManager : MonoBehaviour
{
    private static PrefabManager _instance = null;
    [SerializeField] private Item[] _items = null;
    [SerializeField] private Character[] _characters = null;
    
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
    
    public Character GetCharacterPrefab(string id)
    {
        if (_characters != null)
        {
            for (int i = 0; i < _characters.Length; i++)
            {
                if (_characters[i] != null && _characters[i].id.Trim() == id.Trim())
                {
                    //Debug.Log($"Character Found - {_items[i].id}");
                    return _characters[i];
                }
            }
        }
        
        //Debug.Log("Character Not Found !");
        return null;
    }
}