using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    private Grid m_Grid;

    private void Awake()
    {
        m_Grid = this.GetComponent<Grid>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            RaycastHit hitInfo;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if(Physics.Raycast(ray, out hitInfo))
            {
                GameObject.CreatePrimitive(PrimitiveType.Cube).transform.position =  m_Grid.GetCellCenterWorld(m_Grid.WorldToCell(hitInfo.point));
            }
        }
    }
}
