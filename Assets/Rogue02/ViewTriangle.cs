using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewTriangle : MonoBehaviour
{
    Vector3[] points = new Vector3[3];

    public Triangle triangle;

    private void Start()
    {
        this.transform.position = triangle.CirclePoint;
        points[0] = triangle.pointA - triangle.CirclePoint;
        points[1] = triangle.pointB - triangle.CirclePoint;
        points[2] = triangle.pointC - triangle.CirclePoint;
    }

    private void OnDrawGizmosSelected()
    {
        if (triangle == null)
            return;
        // this.transform.SetSiblingIndex(this.transform.parent.childCount-1);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position + points[0], 5f);
        Gizmos.DrawSphere(transform.position + points[1], 5f);
        Gizmos.DrawSphere(transform.position + points[2], 5f);
        Gizmos.DrawLine(transform.position + points[0], transform.position + points[1]);
        Gizmos.DrawLine(transform.position + points[1], transform.position + points[2]);
        Gizmos.DrawLine(transform.position + points[0], transform.position + points[2]);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(triangle.CirclePoint, triangle.radius);
    }
    private void OnDrawGizmos()
    {
        if (triangle == null)
            return;
        Gizmos.color = Color.white;
        Gizmos.DrawLine(triangle.pointA, triangle.pointB);
        Gizmos.DrawLine(triangle.pointB, triangle.pointC);
        Gizmos.DrawLine(triangle.pointA, triangle.pointC);
    }
}
