using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VerletManager : MonoBehaviour
{
    public Mesh msh;
    Vector3[] vertices;
    Vector3[] verticesOld;
    int[] triangles;
    public List<int> pinnedVerticesIndex = new List<int>();
    public List<VerletLink> verletLink = new List<VerletLink>();
    public float LinkLength = 0.25f;
    public int verletHeight = 10;
    public int verletLength = 10;
    public int iterationCount = 50;

    public GameObject[] ventilateurs;
    public float ventilateurStrenght = 2.0f;
    public float ventilateurDistanceMax = 1.0f;


    // Use this for initialization
    void Start()
    {
        ventilateurs = GameObject.FindGameObjectsWithTag("Ventilateur");

        InitVerticeAndLink();
    }

    void InitVerticeAndLink()
    {
        Vector3 verletStartPoint = this.transform.position;

        vertices = new Vector3[(verletLength) * (verletHeight)];
        verticesOld = new Vector3[(verletLength) * (verletHeight)];
        int idx = 0;
        for (int i = 0; i < verletLength; i++)
        {
            for (int j = 0; j < verletHeight; j++)
            {
                vertices[idx] = verletStartPoint;
                verticesOld[idx] = verletStartPoint;
                idx++;
                verletStartPoint.y -= LinkLength;
            }
            verletStartPoint.x += LinkLength;
            verletStartPoint.y = this.transform.position.y;
        }

        //creation du maillage

        for (int i = 0; i < verletLength; i++)
        {
            for (int j = 0; j < verletHeight; j++)
            {
                //linkDown
                if (j < verletHeight - 1)
                {
                    verletLink.Add(new VerletLink(new Vector2(i, j), new Vector2(i, j + 1)));
                }

                //linkRight

                if (i < verletLength - 1)
                {
                    verletLink.Add(new VerletLink(new Vector2(i, j), new Vector2(i + 1, j)));
                }
            }
        }

        //
        /*for(int i = 0; i< verletLength;i++)
        {
            pinnedVerticesIndex.Add(i * verletHeight);
        }*/
        pinnedVerticesIndex.Add(0);
        pinnedVerticesIndex.Add((verletLength - 1) * verletHeight);


        // Create the mesh
        //GameObject meshGo = new GameObject();
        msh = new Mesh();

        // Set up game object with mesh;
        //meshGo.AddComponent(typeof(MeshRenderer));
        MeshFilter filter = gameObject.AddComponent(typeof(MeshFilter)) as MeshFilter;
        filter.mesh = msh;

        RecomputeTriangle();
    }

    void RecomputeTriangle()
    {
        triangles = new int[(verletLength - 1) * (verletHeight - 1) * 6];
        int vert = 0;
        int tris = 0;

        for (int z = 0; z < verletLength - 1; z++)
        {
            for (int x = 0; x < verletHeight - 1; x++)
            {

                triangles[tris] = vert;
                triangles[tris + 1] = vert + verletHeight + 1;
                triangles[tris + 2] = vert + 1;

                triangles[tris + 3] = vert;
                triangles[tris + 4] = vert + verletHeight;
                triangles[tris + 5] = vert + verletHeight + 1;

                vert++;
                tris += 6;

            }
            vert++;
        }
    }

    void UpdateMesh()
    {
        msh.Clear();

        msh.vertices = vertices;
        msh.triangles = triangles;

        msh.RecalculateNormals();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateMesh();
       /* if (Input.GetMouseButtonDown(0))
        {
            GameObject newVentilo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            newVentilo.transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            newVentilo.tag = "Ventilateur";
            ventilateurs = GameObject.FindGameObjectsWithTag("Ventilateur");
        }*/
    }

    private void FixedUpdate()
    {
        this.Simulate(verletLink);
    }

    private void Simulate( List<VerletLink> links)
    {
        // SIMULATION

        Vector3 forceGravity = new Vector3(0f, -1.5f,0f);


        for (int i = 0; i < verletLength; i++)
        {
            for (int j = 0; j < verletHeight; j++)
            {
                int index = i * verletHeight + j;
                if (pinnedVerticesIndex.Contains(index))
                {
                    continue;
                }

                Vector3 velocity = vertices[index] - verticesOld[index];
                verticesOld[index] = vertices[i * verletHeight + j];
                vertices[index] += velocity;
                vertices[index] += forceGravity * Time.fixedDeltaTime;


                for(int v = 0; v < ventilateurs.Length; v++)
                {
                    float dist = (vertices[index] - ventilateurs[v].transform.position).magnitude;
                    if(dist < ventilateurDistanceMax)
                    {
                        float factor = (ventilateurDistanceMax - dist) / ventilateurDistanceMax;
                        Vector3 dir = (vertices[index] - ventilateurs[v].transform.position).normalized;
                        vertices[index] += factor * ventilateurStrenght * dir * Time.fixedDeltaTime * Mathf.Sin(6.0f*3.141592654f * Time.fixedDeltaTime);
                    }
                }
            }
        }

        //CONSTRAINTS
        for (int i = 0; i < iterationCount; i++)
        {
            this.ApplyConstraint(links);
        }
    }

    private void ApplyConstraint(List<VerletLink> links)
    {

        for (int i = 0; i < links.Count; i++)
        {
            int firstPointIndex = (int)verletLink[i].pointPosition.x * verletHeight + (int)verletLink[i].pointPosition.y;
            int secondPointIndex = (int)verletLink[i].point2Position.x * verletHeight + (int)verletLink[i].point2Position.y;

            Vector3 firstSeg = vertices[firstPointIndex];
            Vector3 secondSeg = vertices[secondPointIndex];

            float dist = (firstSeg - secondSeg).magnitude;
            float error = Mathf.Abs(dist - this.LinkLength);
            Vector3 changeDir = Vector2.zero;

            if (dist > LinkLength)
            {
                changeDir = (firstSeg - secondSeg).normalized;
            }
            else if (dist < LinkLength)
            {
                changeDir = (secondSeg - firstSeg).normalized;
            }

            Vector3 changeAmount = changeDir * error;

            if (pinnedVerticesIndex.Contains(secondPointIndex))
            {
                firstSeg -= changeAmount;
                vertices[firstPointIndex] = firstSeg;
            }
            else if (pinnedVerticesIndex.Contains(firstPointIndex))
            {
                secondSeg += changeAmount;
                vertices[secondPointIndex] = secondSeg;
            }
            else
            {
                firstSeg -= changeAmount * 0.5f;
                vertices[firstPointIndex] = firstSeg;
                secondSeg += changeAmount * 0.5f;
                vertices[secondPointIndex] = secondSeg;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (vertices.Length != 0)
        {
            for (int i = 0; i < verletLength; i++)
            {
                for (int j = 0; j < this.verletHeight; j++)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(vertices[i * verletHeight + j], 0.1f);
                }
            }

            Gizmos.color = Color.blue;
            Vector3 verletStartPoint = this.transform.position;
            for (int i = 0; i < verletLength; i++)
            {
                for (int j = 0; j < verletHeight; j++)
                {
                    Gizmos.DrawSphere(verletStartPoint,0.05f);
                    verletStartPoint.y -= LinkLength;
                }
                verletStartPoint.x += LinkLength;
                verletStartPoint.y = this.transform.position.y;
            }


            Gizmos.color = Color.white;
            for (int i = 0; i < verletLink.Count; i++)
            {
                Debug.DrawLine(vertices[(int)verletLink[i].pointPosition.x * verletHeight + (int)verletLink[i].pointPosition.y], vertices[(int)verletLink[i].point2Position.x * verletHeight + (int)verletLink[i].point2Position.y], Color.white);
            }
        }
    }

    public struct VerletLink
    {
        public Vector2 pointPosition;
        public Vector2 point2Position;

        public VerletLink(Vector2 pointPosition, Vector2 point2Position)
        {
            this.pointPosition = pointPosition;
            this.point2Position = point2Position;
        }
    }

}
