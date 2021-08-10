using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserPointer : MonoBehaviour
{
    public float laserBeamLength;
    private LineRenderer lineRenderer;
    public LayerMask layerMask;
    // Start is called before the first frame update
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.red;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 endPosition = transform.position + (transform.right * laserBeamLength);
        RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.right, Mathf.Infinity, layerMask);
        if (hit.collider && hit.collider.tag == "Wall") {
            endPosition = hit.point;
            lineRenderer.SetPositions(new Vector3[] {transform.position, hit.point});
        } else {
            lineRenderer.SetPositions(new Vector3[] {transform.position, endPosition});
        }

    }
}
