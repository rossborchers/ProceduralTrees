using UnityEngine;
using System.Collections;

public class Translate : MonoBehaviour
{
    [SerializeField]
    Vector3 translation = Vector3.zero;

    protected void Update()
    {
        transform.Translate(translation * Time.deltaTime);
    }
}
