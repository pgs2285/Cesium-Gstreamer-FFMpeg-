using UnityEngine;

public class DrawRaysToPlaneCorners : MonoBehaviour
{
    public Transform point; // The point from which to cast the rays
    public Vector3 planeNormal = Vector3.up; // Normal of the plane
    public Vector3 planeCenter = Vector3.zero; // Center of the plane
    public float planeWidth = 1.0f; // Width of the rectangle on the plane
    public float planeHeight = 1.0f; // Height of the rectangle on the plane

    void OnDrawGizmos()
    {
        if (point == null)
        {
            Debug.LogError("Please assign a point Transform.");
            return;
        }

        // Calculate the two vectors on the plane
        Vector3 right = Vector3.Cross(planeNormal, Vector3.up).normalized;
        if (right == Vector3.zero) // If planeNormal is parallel to Vector3.up
            right = Vector3.Cross(planeNormal, Vector3.forward).normalized;
        Vector3 forward = Vector3.Cross(right, planeNormal).normalized;

        right *= planeWidth / 2;
        forward *= planeHeight / 2;

        // Calculate the four corners of the plane
        Vector3[] corners = new Vector3[4];
        corners[0] = planeCenter + right + forward;
        corners[1] = planeCenter + right - forward;
        corners[2] = planeCenter - right - forward;
        corners[3] = planeCenter - right + forward;

        Gizmos.color = Color.blue;

        // Draw lines from the point to each of the corners
        foreach (Vector3 corner in corners)
        {
            Gizmos.DrawLine(point.position, corner);
        }

        // Optionally, draw the plane edges for visualization
        Gizmos.color = Color.green;
        for (int i = 0; i < corners.Length; i++)
        {
            Gizmos.DrawLine(corners[i], corners[(i + 1) % corners.Length]);
        }
    }
}
