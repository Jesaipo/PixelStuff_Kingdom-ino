using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class VerletManager : MonoBehaviour
{
    
    public int XCount = 2;
    public int YCount = 2;

    public bool Generate = false;

    List<VerletPoint> Points;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Generate)
        {
            for(int i = Points.Count - 1; i > 0; i--)
            {
                VerletPoint point = Points[i];
                if (point.x > XCount || point.y > YCount)
                {
                    GameObject pointGameobject = point.gameObject;
                    Destroy(pointGameobject);
                    Points.RemoveAt(i);
                }
                else
                {
            //        GameObject go = new GameObject();
                 //   go.transform.position = this.transform.position;
                    //go.transform.position += new Vector3()
                }
            }
        }
    }
}
