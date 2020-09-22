using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    static Vector3Int UP = new Vector3Int(0, 0, 1);
    static Vector3Int DOWN = new Vector3Int(0, 0, -1);
    static Vector3Int LEFT = new Vector3Int(-1, 0, 0);
    static Vector3Int RIGHT = new Vector3Int(1, 0, 0);

    enum ERotationVector
    {
        Left,
        Right,
        None
    };

    private Grid m_Grid;

    public List<Cell> m_CellList = new List<Cell>();
    public List<Cell> m_NewCellList = new List<Cell>();

    public GameObject m_StartCell;

    public List<GameObject> m_SquarePrefab = new List<GameObject>();

    private bool m_IsNewCellValid = false;

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
            ERotationVector rotation = ERotationVector.None;
            if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                CellMoveVector += UP;
            }
            if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                CellMoveVector += DOWN;
            }
            if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                CellMoveVector += LEFT;
            }
            if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                CellMoveVector += RIGHT;
            }

            if(Input.GetKeyDown(KeyCode.A))
            {
                rotation = ERotationVector.Left;
            }
            if(Input.GetKeyDown(KeyCode.E))
            {
                rotation = ERotationVector.Right;
            }

            if (CellMoveVector != Vector3Int.zero || rotation != ERotationVector.None)
            {
                m_IsNewCellValid = false;
                bool isOnTop = false;
                foreach (Cell targetCell in m_NewCellList)
                {
                    targetCell.m_GridPosition.y = 0;
                    Vector3Int newGridPosition = targetCell.m_GridPosition + CellMoveVector;

                    if(rotation != ERotationVector.None)
                    {
                        Cell pivotCell = m_NewCellList[0];
                        Vector3Int pivotPosition = newGridPosition - pivotCell.m_GridPosition;
                        Vector3Int rotatedPivotPosition = Vector3Int.zero;
                        int rotationMultiplier = 0;
                        if (rotation == ERotationVector.Right)
                        {
                            //rotate
                            rotatedPivotPosition = new Vector3Int(pivotPosition.z, pivotPosition.y, -pivotPosition.x);
                            rotationMultiplier = 1;
                        }
                        else if(rotation == ERotationVector.Left)
                        {
                            //rotate
                            rotatedPivotPosition = new Vector3Int(-pivotPosition.z, pivotPosition.y, pivotPosition.x);
                            rotationMultiplier = -1;
                        }

                        newGridPosition = pivotCell.m_GridPosition + rotatedPivotPosition;
                        targetCell.m_Object.transform.Rotate(Vector3.up, rotationMultiplier * 90);
                    }

                    foreach (Cell existingCell in m_CellList)
                    {
                        if (existingCell.m_GridPosition == newGridPosition)
                        {
                            m_IsNewCellValid = false;
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
                                m_IsNewCellValid = true;
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

                    targetCell.m_GridPosition = newGridPosition;
                    targetCell.m_Object.transform.position = m_Grid.CellToWorld(targetCell.m_GridPosition);
                }

                foreach (Cell targetCell in m_NewCellList)
                {
                    if (isOnTop || !m_IsNewCellValid)
                    {
                        targetCell.m_Object.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
                    }
                    else
                    {
                        targetCell.m_Object.transform.localScale = new Vector3(1f, 1f, 1f);
                    }
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.N))
        {
            if (m_NewCellList.Count == 0)
            {
                Vector3Int CellPosition = new Vector3Int(0, 0, 0);
                int rand = Random.Range(0, m_SquarePrefab.Count);
                m_NewCellList.Add(new Cell(m_SquarePrefab[rand], m_Grid.CellToWorld(CellPosition), CellPosition));
                rand = Random.Range(0, m_SquarePrefab.Count);
                CellPosition.x += 1;
                m_NewCellList.Add(new Cell(m_SquarePrefab[rand], m_Grid.CellToWorld(CellPosition), CellPosition));
            }

        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            if (m_IsNewCellValid)
            {
                foreach (Cell targetCell in m_NewCellList)
                {
                    m_CellList.Add(targetCell);
                }

                m_NewCellList.Clear();
            }
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
