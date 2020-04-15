using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VerletManager : MonoBehaviour
{
    public Mesh msh;
    Vector3[] vertices;
    Vector3[] verticesOld;
    int[] triangles;
    Vector2[] uvs;
    public List<int> pinnedVerticesIndex = new List<int>();
    public List<VerletLink> verletLink = new List<VerletLink>();
    //public float LinkLength = 0.25f;
    public float VerletHeight = 1;
    public int HeightPointCount = 20;
    public float VerletLength = 1;
    public int LengthPointCount = 20;
    public int iterationCount = 50;

    public enum InteractionMode
    {
        Wind,
        Pin,
        Needle,
        None
    };

    public InteractionMode currentInteractionMode = InteractionMode.None;

    public void PinIteractionMode()
    {
        currentInteractionMode = InteractionMode.Pin;
    }
    public void WindIteractionMode()
    {
        currentInteractionMode = InteractionMode.Wind;
    }

    public GameObject[] ventilateurs;
    public float ventilateurStrenght = 2.0f;
    public float ventilateurDistanceMax = 1.0f;
    public float WindSinusPeriod = 1.0f;

    public void NeedleIteractionMode()
    {
        currentInteractionMode = InteractionMode.Needle;
    }

    public int StartNeedleIndex = -1;
    public float stringLength = 0.0f;
    public Vector3 linkCreatedPosition = Vector3.zero;





    // Use this for initialization
    void Start()
    {
        ventilateurs = GameObject.FindGameObjectsWithTag("Ventilateur");

        InitVerticeAndLink();
    }

    void InitVerticeAndLink()
    {
        float heightLinkLength = VerletHeight / HeightPointCount;
        float lengthLinkLength = VerletLength / LengthPointCount;

        Vector3 verletStartPoint = this.transform.position - new Vector3(VerletLength/2,0,0);

        vertices = new Vector3[(HeightPointCount) * (LengthPointCount)];
        verticesOld = new Vector3[(HeightPointCount) * (LengthPointCount)];
        int idx = 0;
        for (int i = 0; i < LengthPointCount; i++)
        {
            for (int j = 0; j < HeightPointCount; j++)
            {
                vertices[idx] = verletStartPoint;
                verticesOld[idx] = verletStartPoint;
                idx++;
                verletStartPoint.y -= heightLinkLength;
            }
            verletStartPoint.x += lengthLinkLength;
            verletStartPoint.y = this.transform.position.y;
        }

        //creation du maillage

        for (int i = 0; i < LengthPointCount; i++)
        {
            for (int j = 0; j < HeightPointCount; j++)
            {
                //linkDown
                if (j < HeightPointCount - 1)
                {
                    verletLink.Add(new VerletLink(i* HeightPointCount + j, i * HeightPointCount + j + 1, heightLinkLength));
                }

                //linkRight

                if (i < LengthPointCount - 1)
                {
                    verletLink.Add(new VerletLink(i * HeightPointCount + j, (i + 1) * HeightPointCount + j, lengthLinkLength));
                }
            }
        }

        pinnedVerticesIndex.Add(0);
        pinnedVerticesIndex.Add((LengthPointCount - 1) * HeightPointCount);


        // Create the mesh
        //GameObject meshGo = new GameObject();
        msh = new Mesh();

        // Set up game object with mesh;
        //meshGo.AddComponent(typeof(MeshRenderer));
        MeshFilter filter = gameObject.AddComponent(typeof(MeshFilter)) as MeshFilter;
        filter.mesh = msh;

        RecomputeTriangle();

        msh.Clear();

        msh.vertices = vertices;
        msh.triangles = triangles;
        msh.uv = uvs;

        msh.RecalculateNormals();
    }

    void RecomputeTriangle()
    {
        triangles = new int[(LengthPointCount - 1) * (HeightPointCount - 1) * 6];
        int vert = 0;
        int tris = 0;

        for (int z = 0; z < LengthPointCount - 1; z++)
        {
            for (int x = 0; x < HeightPointCount - 1; x++)
            {

                triangles[tris] = vert;
                triangles[tris + 1] = vert + HeightPointCount + 1;
                triangles[tris + 2] = vert + 1;

                triangles[tris + 3] = vert;
                triangles[tris + 4] = vert + HeightPointCount;
                triangles[tris + 5] = vert + HeightPointCount + 1;

                vert++;
                tris += 6;

            }
            vert++;
        }

        int uvIndex = 0;
        uvs = new Vector2[(HeightPointCount) * (LengthPointCount)];
        for (int z = 0; z < LengthPointCount; z++)
        {
            for (int x = 0; x < HeightPointCount; x++)
            {
                uvs[uvIndex] = new Vector2((float)z / (float)(LengthPointCount -1), (float)x / (float)(HeightPointCount - 1));
                uvIndex++;
            }
        }
    }

    void UpdateMesh()
    {
        //msh.Clear();

        msh.vertices = vertices;
        //msh.triangles = triangles;
        //msh.uv = uvs;

        //msh.RecalculateNormals();
        //msh.RecalculateTangents();
        //msh.RecalculateBounds();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateMesh();

        switch(currentInteractionMode)
            {
            case InteractionMode.Pin:
                if (Input.GetMouseButtonDown(0))
                {
                    Vector3 pz = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    float distanceToVertice;
                    int closesIndex = GetVerticeIndexNear(pz, out distanceToVertice);

                    if (distanceToVertice < (VerletHeight / HeightPointCount) * (VerletHeight / HeightPointCount))
                    {
                        pinnedVerticesIndex.Add(closesIndex);
                    }

                } else if(Input.GetMouseButtonDown(1))
                {
                    Vector3 pz = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    float distanceToVertice;
                    int closesIndex = GetVerticeIndexNear(pz, out distanceToVertice);

                    if (distanceToVertice < (VerletHeight / HeightPointCount) * (VerletHeight / HeightPointCount))
                    {
                        pinnedVerticesIndex.Remove(closesIndex);
                    }
                }
                break;

            case InteractionMode.Wind:
                if (Input.GetMouseButtonDown(0))
                {
                    Vector3 pz = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    pz.z = 0;
                    GameObject newVentilo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    newVentilo.transform.position = pz;
                    newVentilo.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                    newVentilo.tag = "Ventilateur";
                    ventilateurs = GameObject.FindGameObjectsWithTag("Ventilateur");

                }
                else if (Input.GetMouseButtonDown(1))
                {
                    if (ventilateurs.Length > 0)
                    {
                        Destroy(ventilateurs[ventilateurs.Length - 1]);
                        ventilateurs = GameObject.FindGameObjectsWithTag("Ventilateur");
                    }
                }
                break;
            case InteractionMode.Needle:
                if (Input.GetMouseButton(0))
                {

                    Vector3 pz = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    if (StartNeedleIndex < 0)
                    {
                        //needle init
                        float distanceToVertice;
                        int closesIndex = GetVerticeIndexNear(pz, out distanceToVertice);

                        if (distanceToVertice < (VerletHeight / HeightPointCount) * (VerletHeight / HeightPointCount))
                        {
                            Debug.Log("Click INIT");
                            StartNeedleIndex = closesIndex;
                            return;
                        }
                    }

                    if (Input.GetMouseButtonDown(1))
                    {

                        float distanceToVertice;
                        int closesIndex = GetVerticeIndexNear(pz, out distanceToVertice);

                        if (distanceToVertice < (VerletHeight / HeightPointCount) * (VerletHeight / HeightPointCount))
                        {
                            if (StartNeedleIndex != closesIndex)
                            {
                                Debug.Log("RightClick create link");
                                verletLink.Add(new VerletLink(StartNeedleIndex, closesIndex, (vertices[StartNeedleIndex] - vertices[closesIndex]).magnitude));
                                linkCreatedPosition = pz;
                            }
                        }
                    }

                    if (Input.GetMouseButton(1))
                    {
                        Debug.Log("Click & rightClick " + verletLink[verletLink.Count - 1].linkSize + " - " + (linkCreatedPosition - pz).magnitude);
                        float linkSize = verletLink[verletLink.Count - 1].linkSize - (linkCreatedPosition - pz).magnitude;
                        if (linkSize < 0)
                        {
                            linkSize = 0;
                        }
                        verletLink[verletLink.Count - 1].linkSize = linkSize;
                        linkCreatedPosition = pz;
                    }
                }
                else
                {
                    StartNeedleIndex = -1;
                    if (Input.GetMouseButtonDown(1))
                    {
                        verletLink.RemoveAt(verletLink.Count - 1);
                    }
                }
                break;


            default:
                break;
        }
    }


    public int GetVerticeIndexNear(Vector3 position,out float minDistance, bool is2D = true)
    {
        if(is2D)
        {
            position.z = 0;
        }
        minDistance = float.MaxValue;
        int minDistanceIndex = -1;
        for (int index = 0; index < vertices.Length; index++)
        {
            Vector3 indexPos = vertices[index];
            if (is2D)
            {
                indexPos.z = 0;
            }
            float distance = (indexPos - position).sqrMagnitude;
            if(distance < minDistance)
            {
                minDistanceIndex = index;
                minDistance = distance;
            }
        }

        return minDistanceIndex;
    }


    private void FixedUpdate()
    {
        this.Simulate(verletLink);
    }

    private void Simulate( List<VerletLink> links)
    {
        // SIMULATION

        Vector3 forceGravity = new Vector3(0f, -1.5f,0f);


        for (int index = 0; index < vertices.Length; index++)
        {
                if (pinnedVerticesIndex.Contains(index))
                {
                    continue;
                }

                Vector3 velocity = vertices[index] - verticesOld[index];
                verticesOld[index] = vertices[index];
                vertices[index] += velocity;
                vertices[index] += forceGravity * Time.fixedDeltaTime;


            for (int v = 0; v < ventilateurs.Length; v++)
            {
                if (ventilateurs[v] != null)
                {
                    float dist = (vertices[index] - ventilateurs[v].transform.position).magnitude;
                    if (dist < ventilateurDistanceMax)
                    {
                        float factor = (ventilateurDistanceMax - dist) / ventilateurDistanceMax;
                        Vector3 dir = (vertices[index] - ventilateurs[v].transform.position).normalized;
                        vertices[index] += factor * ventilateurStrenght * dir * Time.fixedDeltaTime * Mathf.Sin(6.0f * 3.141592654f * Time.fixedDeltaTime * WindSinusPeriod);
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
            int firstPointIndex = verletLink[i].pointIndex;
            int secondPointIndex = verletLink[i].point2Index;
            float linkLength = verletLink[i].linkSize;

            Vector3 firstSeg = vertices[firstPointIndex];
            Vector3 secondSeg = vertices[secondPointIndex];

            float dist = (firstSeg - secondSeg).magnitude;
            float error = Mathf.Abs(dist - linkLength);
            Vector3 changeDir = Vector2.zero;

            if (dist > linkLength)
            {
                changeDir = (firstSeg - secondSeg).normalized;
            }
            else if (dist < linkLength)
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
        if (vertices != null && vertices.Length != 0)
        {
            for (int index = 0; index < vertices.Length; index++)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(vertices[index], 0.01f);
            }

            /*Gizmos.color = Color.blue;
            Vector3 verletStartPoint = this.transform.position;
            for (int i = 0; i < VerletLength; i++)
            {
                for (int j = 0; j < VerletHeight; j++)
                {
                    Gizmos.DrawSphere(verletStartPoint,0.05f);
                    verletStartPoint.y -= LinkLength;
                }
                verletStartPoint.x += LinkLength;
                verletStartPoint.y = this.transform.position.y;
            }*/


            Gizmos.color = Color.white;
            for (int i = 0; i < verletLink.Count; i++)
            {
                Debug.DrawLine(vertices[verletLink[i].pointIndex], vertices[verletLink[i].point2Index], Color.white);
            }
        }
    }

    public class VerletLink
    {
        public int pointIndex;
        public int point2Index;
        public float linkSize;

        public VerletLink(int pointIndex, int point2Index, float linkSize)
        {
            this.pointIndex = pointIndex;
            this.point2Index = point2Index;
            this.linkSize = linkSize;
        }
    }

}
