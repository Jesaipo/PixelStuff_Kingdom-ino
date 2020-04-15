using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    static Vector3Int UP = new Vector3Int(0, 0, 1);
    static Vector3Int DOWN = new Vector3Int(0, 0, -1);
    static Vector3Int LEFT = new Vector3Int(-1, 0, 0);
    static Vector3Int RIGHT = new Vector3Int(1, 0, 0);

    private Grid m_Grid;

    public List<Cell> m_CellList = new List<Cell>();
    public List<Cell> m_NewCellList = new List<Cell>();

    public GameObject m_StartCell;

    public List<GameObject> m_SquarePrefab = new List<GameObject>();

    private void Awake()
    {
        m_Grid = this.GetComponent<Grid>();
    }

    // Start is called before the first frame update
    void Start()
    {
        Vector3Int CellPosition = Vector3Int.zero;
        m_CellList.Add(new Cell(m_StartCell,m_Grid.CellToWorld(CellPosition), CellPosition));
    }

    // Update is called once per frame
    void Update()
    {
        if (m_NewCellList.Count > 1)
        {
            Vector3Int CellMoveVector = Vector3Int.zero;
            if (Input.GetKeyDown(KeyCode.Z))
            {
                CellMoveVector += UP;
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                CellMoveVector += DOWN;
            }
            if (Input.GetKeyDown(KeyCode.Q))
            {
                CellMoveVector += LEFT;
            }
            if (Input.GetKeyDown(KeyCode.D))
            {
                CellMoveVector += RIGHT;
            }

            if (CellMoveVector != Vector3Int.zero)
            {
                
                foreach (Cell targetCell in m_NewCellList)
                {
                    targetCell.m_GridPosition.y = 0;
                    Vector3Int newGridPosition = targetCell.m_GridPosition + CellMoveVector;
                    bool isValid = false;
                    bool isOnTop = false;
                    foreach (Cell existingCell in m_CellList)
                    {
                        if (existingCell.m_GridPosition == newGridPosition)
                        {
                            isValid = false;
                            isOnTop = true;
                            break;
                        }
                        if(existingCell.m_GridPosition == newGridPosition + UP || 
                            existingCell.m_GridPosition == newGridPosition + DOWN ||
                            existingCell.m_GridPosition == newGridPosition + RIGHT ||
                            existingCell.m_GridPosition == newGridPosition + LEFT)
                        {
                            if(targetCell.m_Object.CompareTag(existingCell.m_Object.tag) || existingCell.m_Object.CompareTag(m_StartCell.tag))
                            {
                                isValid = true;
                            }
                        }
                    }
                    if(isOnTop)
                    {
                        newGridPosition.y = 1;
                    }
                    else
                    {
                        newGridPosition.y = 0;
                    }

                    if (isValid)
                    {
                        targetCell.m_Object.transform.localScale = new Vector3(1f, 1f, 1f);
                    }
                    else
                    {
                        targetCell.m_Object.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                    }

                    targetCell.m_GridPosition = newGridPosition;
                    targetCell.m_Object.transform.position = m_Grid.CellToWorld(targetCell.m_GridPosition);
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.N))
        {
            Debug.Log("New");
            Vector3Int CellPosition = new Vector3Int(0, 0, 0);
            int rand = Random.Range(0, m_SquarePrefab.Count);
            m_NewCellList.Add(new Cell(m_SquarePrefab[rand], m_Grid.CellToWorld(CellPosition), CellPosition));
            rand = Random.Range(0, m_SquarePrefab.Count);
            CellPosition.x += 1;
            m_NewCellList.Add(new Cell(m_SquarePrefab[rand], m_Grid.CellToWorld(CellPosition), CellPosition));

        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            foreach (Cell targetCell in m_NewCellList)
            {
                m_CellList.Add(targetCell);
            }

            m_NewCellList.Clear();
        }

        if (Input.GetMouseButtonDown(0))
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

public class Cell
{
    public GameObject m_Object;
    public Vector3Int m_GridPosition;

    public Cell(GameObject prefab, Vector3 position, Vector3Int GridPosition)
    {
        m_Object = GameObject.Instantiate(prefab);
        m_Object.transform.position = position;
        m_GridPosition = GridPosition;
    }
}
