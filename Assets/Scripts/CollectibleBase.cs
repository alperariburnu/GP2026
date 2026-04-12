using UnityEngine;

/// <summary>
/// Base class demonstrating virtual methods. Inherit from this to override collection behavior.
/// </summary>
public class CollectibleBase : MonoBehaviour
{
    // Do not serialize this base field to avoid name collision with derived classes.
    protected int baseValue = 1;

    /// <summary>
    /// Virtual Collect method which derived classes can override to customize behavior.
    /// </summary>
    public virtual void Collect()
    {
        // Default behavior: log and destroy
        Debug.Log($"CollectibleBase: Collected value {baseValue}");
        Destroy(gameObject);
    }

    public virtual int GetValue()
    {
        return baseValue;
    }
}
