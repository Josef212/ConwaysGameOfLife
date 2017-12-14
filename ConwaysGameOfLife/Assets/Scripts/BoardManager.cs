using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using Random = UnityEngine.Random;

public class BoardManager : MonoBehaviour
{
	public GameObject cellPrefab;
	public Transform boardParent;

	public Vector2 boardSize;
	[Range(1, 100)]
	public float cellsSize;

	[SerializeField]
	private Vector3Int cellsAmmount;

	[Range(1, 5)]
	public int aliveCheckRadius = 1;

	[Range(0f, 5f)]
	public float timeToWaitBetweenFrames = 1f;

	[Range(0f, 0.1f)]
	public float cellRandomSpawnProbability = 0.01f;

	[Range(0, 10)]
	public int paintRadius = 4;

	public bool startPaused = false;
	public bool useSeed = true;

	private Cell[,] board;
	private bool[,] calcCells;

	private bool pause = false;
	public bool IsPause { get { return pause; } }
	
	public Vector2Int logCell;

	public int randomSeed = 0;
	private System.Random rnd;


	// -------------------------------------

	void Awake ()
	{
		rnd = new System.Random((useSeed) ? randomSeed : Random.Range(int.MinValue, int.MaxValue));
		pause = startPaused;
		CalcBoardSize();
		CalcBoard();
		StartCoroutine(UpdateBoard());
	}

	void Update()
	{
		if (Input.GetMouseButton(0) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.LeftCommand)))
		{
			Paint(false, false);
		}
		else if (Input.GetMouseButton(0))
		{
			Paint(true, false);
		}

		if (Input.GetMouseButton(1) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.LeftCommand)))
		{
			Paint(false, true);
		}
		else if(Input.GetMouseButton(1))
		{
			Paint(true, true);
		}
	}

	IEnumerator UpdateBoard()
	{
		while (true)
		{
			yield return (timeToWaitBetweenFrames != 0) ? new WaitForSeconds(timeToWaitBetweenFrames) : null;

			if (!pause)
			{
				Step();
			}
		}
	}

	// -------------------------------------

	int CountAliveNeighbours(int x, int y)
	{
		int ret = 0;

		for (int i = -aliveCheckRadius; i <= aliveCheckRadius; ++i)
		{
			for (int j = -aliveCheckRadius; j <= aliveCheckRadius; ++j)
			{
				int _x = (int)(i + x + cellsAmmount.x) % cellsAmmount.x;
				int _y = (int)(j + y + cellsAmmount.y) % cellsAmmount.y;

				if (board[_x, _y].Alive) ret++;
			}
		}

		if (board[x, y].Alive) ret--;

		return ret;
	}

	Cell[] GetNeighbours(int x, int y, int radius)
	{
		Cell[] ret = new Cell[(2 * radius) * (2 * radius)];

		int a = 0;
		for (int i = -radius; i < radius; ++i)
		{
			for (int j = -radius; j < radius; ++j)
			{
				int _x = (int)(i + x + cellsAmmount.x) % cellsAmmount.x;
				int _y = (int)(j + y + cellsAmmount.y) % cellsAmmount.y;

				ret[a] = board[_x, _y];
				++a;
			}
		}

		return ret;
	}

	void Paint(bool alive, bool line)
	{
		Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;

		if (Physics.Raycast(r, out hit, 20f))
		{
			Cell c = hit.transform.GetComponent<Cell>();
			if (line)
			{
				c.Alive = alive;
			}
			else
			{
				if (c != null)
				{
					foreach (Cell cell in GetNeighbours(c.boardPos.x, c.boardPos.y, paintRadius))
					{
						cell.Alive = alive;
					}
				}
			}
		}
	}

	void Step()
	{
		for (int x = 0; x < cellsAmmount.x; ++x)
		{
			for (int y = 0; y < cellsAmmount.y; ++y)
			{
				int aliveNeighbours = CountAliveNeighbours(x, y);

				Cell c = board[x, y];
				if (!c.Alive && aliveNeighbours == 3)
				{
					calcCells[x, y] = true;
				}
				else if (c.Alive && (aliveNeighbours < 2 || aliveNeighbours > 3))
				{
					calcCells[x, y] = false;
				}
				else
				{
					calcCells[x, y] =
						(c.Alive)
							? c.Alive
							: ((rnd.NextDouble() < cellRandomSpawnProbability)
								? true
								: false);
				}
			}
		}

		for (int x = 0; x < cellsAmmount.x; ++x)
		{
			for (int y = 0; y < cellsAmmount.y; ++y)
			{
				board[x, y].Alive = calcCells[x, y];
			}
		}
	}

	// -------------------------------------

#if UNITY_EDITOR

	void OnValidate()
	{
		CalcBoardSize();
	}

	void OnDrawGizmos()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireCube(transform.position, new Vector3(boardSize.x, boardSize.y, 1f));

		//if (board != null)
		//{
		//	for (int x = 0; x < cellsAmmount.x; ++x)
		//	{
		//		for (int y = 0; y < cellsAmmount.y; ++y)
		//		{
		//			Gizmos.color = board[x, y].Alive ? Color.black : Color.white;
		//			Gizmos.DrawCube(board[x, y].transform.position, new Vector3(cellsSize, cellsSize, 1f));
		//		}
		//	}
		//}
	}

	void OnGUI()
	{
		if (GUILayout.Button("Restart"))
		{
			RecalcAll();
		}
	}

	public void RecalcAll()
	{
		ClearBoard();
		CalcBoardSize();
		CalcBoard();
		StartCoroutine(UpdateBoard());
	}

	public void LogCell()
	{
		if(board != null)
			Debug.Log(board[logCell.x, logCell.y].ToString());
		else
			Debug.Log("Board not created.");
	}

	public void Pause()
	{
		pause = true;
	}

	public void Continue()
	{
		pause = false;
	}

	public void DoStep()
	{
		Step();
	}

#endif

	// -------------------------------------

	private void CalcBoardSize()
	{
		cellsAmmount.x = (int) (boardSize.x / cellsSize);
		cellsAmmount.y = (int)(boardSize.y / cellsSize);
		cellsAmmount.z = 1;
	}

	private void CalcBoard()
	{
		board = new Cell[cellsAmmount.x, cellsAmmount.y];
		calcCells = new bool[cellsAmmount.x, cellsAmmount.y];

		for (int x = 0; x < cellsAmmount.x; ++x)
		{
			for (int y = 0; y < cellsAmmount.y; ++y)
			{
				Vector3 pos = new Vector3((x * cellsSize) - (boardSize.x / 2) + (cellsSize / 2),
										-(y * cellsSize) + (boardSize.y / 2) - (cellsSize / 2),
										0f);
				bool alive = rnd.Next(0, 2) == 1;

				GameObject cell = Instantiate(cellPrefab, pos, Quaternion.identity, boardParent);
				Cell c = cell.GetComponent<Cell>();
				board[x, y] = c;
				c.SetCell(x, y, cellsSize, alive);
			}
		}
	}

	void ClearBoard()
	{
		StopAllCoroutines();

		var childs = new List<Transform>();
		foreach (Transform t in boardParent)
		{
			childs.Add(t);
		}
		childs.ForEach(child => Destroy(child.gameObject));
	}
}
