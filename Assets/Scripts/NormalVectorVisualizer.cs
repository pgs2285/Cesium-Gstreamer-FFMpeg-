using UnityEngine;

public class NormalVectorVisualizer : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        Debug.DrawLine(transform.position, transform.right * 100);
    }
}
