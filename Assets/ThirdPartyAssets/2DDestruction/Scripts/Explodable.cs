using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using System;

[RequireComponent(typeof(Rigidbody2D))]
public class Explodable : MonoBehaviour
{
    public System.Action<List<GameObject>> OnFragmentsGenerated;
    public bool allowRuntimeFragmentation = false;
    public int extraPoints = 0;
    public PhysicsMaterial2D physicsMaterial;
    public int subshatterSteps = 0;
    [Serializable]
    public class ExplodeEvent : UnityEvent {}
    public ExplodeEvent explodeEvent = new ExplodeEvent();
    public string fragmentLayer = "Default";
    public string sortingLayerName = "Default";
    public int orderInLayer = 0;

    private Transform sortingAnchor;

    public enum ShatterType
    {
        Triangle,
        Voronoi
    };
    public ShatterType shatterType;
    public List<GameObject> fragments = new List<GameObject>();
    private List<List<Vector2>> polygons = new List<List<Vector2>>();

    private void Start()
    {
        sortingAnchor = transform.Find("SortingAnchor");    
        if (fragments.Count == 0 && allowRuntimeFragmentation)
        {
            generateFragments();
            foreach (GameObject frag in fragments)
            {
                frag.GetComponent<Rigidbody2D>().sharedMaterial = physicsMaterial;
                frag.SetActive(false);
            }
        }
    }
   
    /// <summary>
    /// Creates fragments if necessary and destroys original gameobject
    /// </summary>
    public void explode()
    {
        explodeEvent.Invoke();
            foreach (GameObject frag in fragments)
            {
                frag.SetActive(true);
                frag.transform.position = new Vector3(frag.transform.position.x, sortingAnchor.position.y, frag.transform.position.z);
                frag.tag = "Pickup";
            }

        //if fragments exist destroy the original
        if (fragments.Count > 0)
        {
            gameObject.GetComponent<SpriteRenderer>().sprite = null;
            Collider2D[] colList = gameObject.GetComponents<Collider2D>();

            foreach (Collider2D col in colList)
            {
                col.enabled = false;
            }
        }
    }
    /// <summary>
    /// Creates fragments and then disables them
    /// </summary>
    public void fragmentInEditor()
    {
        if (fragments.Count > 0)
        {
            deleteFragments();
        }
        generateFragments();
        setPolygonsForDrawing();
        foreach (GameObject frag in fragments)
        {
            frag.transform.parent = transform;
            frag.SetActive(false);
            frag.GetComponent<Rigidbody2D>().sharedMaterial = physicsMaterial;;
        }
    }
    public void deleteFragments()
    {
        foreach (GameObject frag in fragments)
        {
            if (Application.isEditor)
            {
                DestroyImmediate(frag);
            }
            else
            {
                Destroy(frag);
            }
        }
        fragments.Clear();
        polygons.Clear();
    }
    /// <summary>
    /// Turns Gameobject into multiple fragments
    /// </summary>
    private void generateFragments()
    {
        fragments = new List<GameObject>();
        switch (shatterType)
        {
            case ShatterType.Triangle:
                fragments = SpriteExploder.GenerateTriangularPieces(gameObject, extraPoints, subshatterSteps);
                break;
            case ShatterType.Voronoi:
                fragments = SpriteExploder.GenerateVoronoiPieces(gameObject, extraPoints, subshatterSteps);
                break;
            default:
                Debug.Log("invalid choice");
                break;
        }
        //sets additional aspects of the fragments
        foreach (GameObject p in fragments)
        {
            if (p != null)
            {
                p.layer = LayerMask.NameToLayer(fragmentLayer);
                p.GetComponent<MeshRenderer>().sortingLayerName = sortingLayerName;
                p.GetComponent<MeshRenderer>().sortingOrder = -32760;
                p.transform.parent = transform;
            }
        }

        foreach (ExplodableAddon addon in GetComponents<ExplodableAddon>())
        {
            if (addon.enabled)
            {
                addon.OnFragmentsGenerated(fragments);
            }
        }
    }
    private void setPolygonsForDrawing()
    {
        polygons.Clear();
        List<Vector2> polygon;

        foreach (GameObject frag in fragments)
        {
            polygon = new List<Vector2>();
            foreach (Vector2 point in frag.GetComponent<PolygonCollider2D>().points)
            {
                Vector2 offset = rotateAroundPivot((Vector2)frag.transform.position, (Vector2)transform.position, Quaternion.Inverse(transform.rotation)) - (Vector2)transform.position;
                offset.x /= transform.localScale.x;
                offset.y /= transform.localScale.y;
                polygon.Add(point + offset);
            }
            polygons.Add(polygon);
        }
    }
    private Vector2 rotateAroundPivot(Vector2 point, Vector2 pivot, Quaternion angle)
    {
        Vector2 dir = point - pivot;
        dir = angle * dir;
        point = dir + pivot;
        return point;
    }

    void OnDrawGizmos()
    {
        if (Application.isEditor)
        {
            if (polygons.Count == 0 && fragments.Count != 0)
            {
                setPolygonsForDrawing();
            }

            Gizmos.color = Color.blue;
            Gizmos.matrix = transform.localToWorldMatrix;
            Vector2 offset = (Vector2)transform.position * 0;
            foreach (List<Vector2> polygon in polygons)
            {
                for (int i = 0; i < polygon.Count; i++)
                {
                    if (i + 1 == polygon.Count)
                    {
                        Gizmos.DrawLine(polygon[i] + offset, polygon[0] + offset);
                    }
                    else
                    {
                        Gizmos.DrawLine(polygon[i] + offset, polygon[i + 1] + offset);
                    }
                }
            }
        }
    }
}
