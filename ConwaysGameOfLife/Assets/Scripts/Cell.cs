using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{
	public Vector2Int boardPos;
	
	private bool alive;

	private MeshRenderer mr;

	// --------------------------------------------------------------

	void Awake()
	{
		mr = GetComponent<MeshRenderer>();
	}

	// --------------------------------------------------------------

	public void SetCell(int _x, int _y, float cellSize, bool alive)
	{
		boardPos.x = _x;
		boardPos.y = _y;
		transform.localScale = new Vector3(cellSize, cellSize, 1f);
		this.alive = alive;
		SetStateColor();
	}

	public bool Alive
	{
		get { return alive; }
		set
		{
			if (value != alive)
			{
				alive = value;
				SetStateColor();
			}
		}
	}

	public override string ToString()
	{
		string ret = base.ToString() + ": \nWorld pos: " + transform.position + " Board pos: " + boardPos + " Alive: " + alive;
		return ret;
	}

	void SetStateColor()
	{
		mr.material.color = alive ? Color.black : Color.white;
	}
}
