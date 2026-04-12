using UnityEngine;
using TMPro;

/// <summary>
/// Simple demo that shows different ways to find objects in the scene and use them.
/// Attach to an empty GameObject in the scene and optionally assign a GameObject via Inspector.
/// </summary>
public class SceneFinderDemo : MonoBehaviour
{
    [Header("Drag-n-drop (assign in Inspector)")]
    [SerializeField]
    private GameObject dragAssigned;

    [Header("Demo Settings")]
    [SerializeField]
    private string nameToFind = "TimerText";

    [SerializeField]
    private string tagToFind = "MainCamera";

    private void Start()
    {
        // Option 1: Drag-n-drop
        if (dragAssigned != null)
        {
            dragAssigned.SetActive(true);
            Debug.Log($"SceneFinderDemo: Activated dragAssigned '{dragAssigned.name}'.");
        }
        else
        {
            Debug.Log("SceneFinderDemo: dragAssigned not set via Inspector.");
        }

        // Option 2: With Name
        GameObject byName = GameObject.Find(nameToFind);
        if (byName != null)
        {
            var tmp = byName.GetComponent<TextMeshProUGUI>();
            if (tmp != null)
                tmp.text = "Found by Name";
            Debug.Log($"SceneFinderDemo: Found GameObject by name '{nameToFind}'.");
        }
        else
        {
            Debug.Log($"SceneFinderDemo: No GameObject named '{nameToFind}' found.");
        }

        // Option 3: With Tag
        try
        {
            GameObject byTag = GameObject.FindWithTag(tagToFind);
            if (byTag != null)
            {
                Debug.Log($"SceneFinderDemo: Found GameObject by tag '{tagToFind}': {byTag.name}");
                var cam = byTag.GetComponent<Camera>();
                if (cam != null)
                {
                    cam.backgroundColor = Color.gray; // example of using the found object
                }
            }
        }
        catch (UnityException)
        {
            Debug.Log($"SceneFinderDemo: Tag '{tagToFind}' does not exist in project.");
        }

        // Option 4: By Type
        var canvas = FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            Debug.Log($"SceneFinderDemo: Found Canvas by type: {canvas.name}");
        }

        // Option 5: By Component Type (GetComponent) and Option 6: GetComponentInChildren
        if (byName != null)
        {
            var sr = byName.GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.color = Color.green;

            var childTmp = byName.GetComponentInChildren<TextMeshProUGUI>();
            if (childTmp != null)
                childTmp.text = "Found in children";
        }

        // Use found thing: modify GameManager via its public API
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetScore(0);
            Debug.Log("SceneFinderDemo: Reset score to 0 via GameManager.SetScore().");
        }
    }
}
