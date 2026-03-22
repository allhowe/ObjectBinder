using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

public class ObjectBinder : MonoBehaviour
{
    [Serializable]
    public class Item
    {
        public string name;
        public Object target;
    }

    public List<Item> items;
    private Dictionary<string, Object> data;

    public T Get<T>(string name) where T : Object
    {
        data ??= items.ToDictionary(p => p.name, p => p.target);

        if (!data.TryGetValue(name, out var target))
        {
            Debug.LogError($"ObjectBinder: Name not found. Name:{name}");
            return null;
        }

        if (target is not T t)
        {
            Debug.LogError($"ObjectBinder: Type mismatch. Key:{name}");
            return null;
        }

        return t;
    }


#if UNITY_EDITOR
    public TextAsset asset;
#endif
}
