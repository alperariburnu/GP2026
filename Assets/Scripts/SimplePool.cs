using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Lightweight generic pool for MonoBehaviour prefabs.
/// </summary>
public class SimplePool<T> where T : MonoBehaviour
{
    private Stack<T> available = new Stack<T>();
    private T prefab;
    private Transform root;

    /// <summary>
    /// Create a pool. Optionally pre-warm with instances.
    /// </summary>
    public SimplePool(T prefab, int preWarm = 0)
    {
        this.prefab = prefab;
        root = new GameObject($"Pool<{typeof(T).Name}>").transform;

        for (int i = 0; i < preWarm; i++)
        {
            T item = Object.Instantiate(prefab, root);
            item.gameObject.SetActive(false);
            available.Push(item);
        }
    }

    /// <summary>
    /// Get an instance from the pool or null if none available (caller may Instantiate fallback).
    /// </summary>
    public T Get()
    {
        if (available.Count > 0)
        {
            T item = available.Pop();
            item.transform.SetParent(null);
            item.gameObject.SetActive(true);
            return item;
        }

        return null;
    }

    /// <summary>
    /// Releases an instance back into the pool.
    /// </summary>
    public void Release(T instance)
    {
        if (instance == null)
            return;

        instance.gameObject.SetActive(false);
        instance.transform.SetParent(root);
        available.Push(instance);
    }
}
