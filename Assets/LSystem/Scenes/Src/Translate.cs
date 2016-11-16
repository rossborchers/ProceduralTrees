using UnityEngine;

/// <summary>
/// Moves GameObject a certain distance over time.
/// </summary>
public class Translate : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Global translation in units per second.")]
    Vector3 translation = Vector3.zero;

    protected void Update()
    {
        transform.Translate(translation * Time.deltaTime);
    }
}
