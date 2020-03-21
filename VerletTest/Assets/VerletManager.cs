using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VerletManager : MonoBehaviour
{

    private LineRenderer lineRenderer;
    public Mesh msh;
    Vector3[] vertices;
    int[] triangles;
    public List<List<VerletPoint>> verletPointMatrix = new List<List<VerletPoint>>();
    public List<VerletLink> verletLink = new List<VerletLink>();
    public float LinkLength = 0.25f;
    public int verletLength = 10;
    public int verletHeight = 10;
    private float lineWidth = 0.1f;
    public int iterationCount = 50;
    public Vector3 mousePosition;

    // Start is called before the first frame update
    void Start()
    {
        this.lineRenderer = this.GetComponent<LineRenderer>();
        Vector3 verletStartPoint = this.transform.position;

        for (int i = 0; i < verletHeight; i++)
        {
            List<VerletPoint> verletLengthList = new List<VerletPoint>();
            for (int j = 0; j < verletLength; j++)
            {
                verletLengthList.Add(new VerletPoint(verletStartPoint));
                verletStartPoint.y -= LinkLength;
            }
            verletStartPoint.x += LinkLength;
            verletStartPoint.y = this.transform.position.y;
            verletPointMatrix.Add(verletLengthList);
        }
        //creation du maillage

        for (int i = 0; i < verletHeight; i++)
        {
            for (int j = 0; j < verletLength; j++)
            {
                //linkDown
                if (j < verletLength - 1)
                {
                    verletLink.Add(new VerletLink(new Vector2(i, j), new Vector2(i, j + 1)));
                }

                //linkRight

                if (i < verletHeight - 1)
                {
                    verletLink.Add(new VerletLink(new Vector2(i, j), new Vector2(i + 1, j)));
                }
            }
        }

        // Create the mesh
        msh = new Mesh();

        // Set up game object with mesh;
        MeshFilter filter = gameObject.AddComponent(typeof(MeshFilter)) as MeshFilter;
        filter.mesh = msh;

        //CreateShape();

        //UpdateMesh();
    }

    void CreateShape()
    {
        /*vertices = new Vector3[]
        {
            new Vector3 (0,0,0),
            new Vector3 (0,0,1),
            new Vector3 (1,0,0),
            new Vector3 (1,0,1)
        };

        triangles = new int[]
        {
            0,1,2, 1,3,2
        };*/
        vertices = new Vector3[(verletHeight) * (verletLength)];
        for(int i =0, z = 0; z < verletLength; z++)
        {
            for (int x = 0; x < verletHeight; x++)
            {
                vertices[i] = new Vector3(verletPointMatrix[z][x].posNow.x, verletPointMatrix[z][x].posNow.y, 0); //verletPointMatrix[z][x].posNow.x,0, verletPointMatrix[z][x].posNow.y);
                i++;
            }
        }

        
        triangles = new int[(verletHeight-1) *(verletLength -1 )*6];
        int vert = 0;
        int tris = 0;

        for (int z = 0; z < verletHeight -1; z++)
        {
            for (int x = 0; x < verletLength-1; x++)
            {

                triangles[tris] = vert;
                triangles[tris + 1] = vert + verletLength + 1;
                triangles[tris + 2] = vert + 1;

                triangles[tris + 3] = vert;
                triangles[tris + 4] = vert + verletLength;
                triangles[tris + 5] = vert + verletLength + 1;

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
            this.DrawRope(verletPointMatrix[0]);
        CreateShape();

        UpdateMesh();
    }

    private void FixedUpdate()
    {
            this.Simulate(verletPointMatrix, verletLink);
    }

    private void Simulate(List<List<VerletPoint>> verletPointMatrix, List<VerletLink> links)
    {
        // SIMULATION
        Vector2 forceGravity = new Vector2(0f, -1.5f);

        for (int i = 0; i < verletHeight; i++)
        {
            for (int j = 0; j < verletLength; j++)
            {
                VerletPoint firstSegment = verletPointMatrix[i][j];
                Vector2 velocity = firstSegment.posNow - firstSegment.posOld;
                firstSegment.posOld = firstSegment.posNow;
                firstSegment.posNow += velocity;
                firstSegment.posNow += forceGravity * Time.fixedDeltaTime;
                verletPointMatrix[i][j] = firstSegment;
            }
        }

        for (int i = 0; i < verletHeight; i++)
        {
            verletPointMatrix[i][0] = new VerletPoint( new Vector2(this.transform.position.x + i * LinkLength, this.transform.position.y));
        }

            //CONSTRAINTS
            for (int i = 0; i < iterationCount; i++)
        {
            this.ApplyConstraint(links);
        }
    }

    private void ApplyConstraint(List<VerletLink> links)
    {
        //Constrant to Mouse
       // VerletPoint firstSegment = ropeSegments[0];
        //firstSegment.posNow = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //ropeSegments[0] = firstSegment;

        for (int i = 0; i < links.Count; i++)
        {
            VerletPoint firstSeg = verletPointMatrix[(int)verletLink[i].pointPosition.x][(int)verletLink[i].pointPosition.y];
            VerletPoint secondSeg = verletPointMatrix[(int)verletLink[i].point2Position.x][(int)verletLink[i].point2Position.y];

            float dist = (firstSeg.posNow - secondSeg.posNow).magnitude;
            float error = Mathf.Abs(dist - this.LinkLength);
            Vector2 changeDir = Vector2.zero;

            if (dist > LinkLength)
            {
                changeDir = (firstSeg.posNow - secondSeg.posNow).normalized;
            }
            else if (dist < LinkLength)
            {
                changeDir = (secondSeg.posNow - firstSeg.posNow).normalized;
            }

            Vector2 changeAmount = changeDir * error;
            if (i != 0)
            {
                firstSeg.posNow -= changeAmount * 0.5f;
                verletPointMatrix[(int)verletLink[i].pointPosition.x][(int)verletLink[i].pointPosition.y] = firstSeg;
                secondSeg.posNow += changeAmount * 0.5f;
                verletPointMatrix[(int)verletLink[i].point2Position.x][(int)verletLink[i].point2Position.y] = secondSeg;
            }
            else
            {
                secondSeg.posNow += changeAmount;
                verletPointMatrix[(int)verletLink[i].point2Position.x][(int)verletLink[i].point2Position.y] = secondSeg;
            }
        }
    }

    private void DrawRope(List<VerletPoint> ropeSegments)
    {
       /* float lineWidth = this.lineWidth;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;

        Vector3[] ropePositions = new Vector3[this.verletLength];
        for (int i = 0; i < this.verletLength; i++)
        {
            ropePositions[i] = ropeSegments[i].posNow;
            Gizmos.color = Color.yellow;
            
        }

        lineRenderer.positionCount = ropePositions.Length;
        lineRenderer.SetPositions(ropePositions);*/
    }

    void OnDrawGizmosSelected()
    {
        if (verletPointMatrix.Count != 0)
        {
            for (int i = 0; i < verletHeight; i++)
            {
                for (int j = 0; j < this.verletLength; j++)
                {
                    // Draw a yellow sphere at the transform's position
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(verletPointMatrix[i][j].posNow, 0.1f);
                }
            }

            for (int i = 0; i < verletLink.Count; i++)
            {
                Debug.DrawLine(verletPointMatrix[(int)verletLink[i].pointPosition.x][(int)verletLink[i].pointPosition.y].posNow, verletPointMatrix[(int)verletLink[i].point2Position.x][(int)verletLink[i].point2Position.y].posNow, Color.white);
            }
        }
    }

    public struct VerletPoint
    {
        public Vector2 posNow;
        public Vector2 posOld;

        public VerletPoint(Vector2 pos)
        {
            this.posNow = pos;
            this.posOld = pos;
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
