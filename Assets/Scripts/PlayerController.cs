using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
	private BoardManager m_Board;
	private Vector2Int m_CellPosition;
	private Animator m_Animator;
	private bool m_IsGameOver;

	private void Awake()
	{
		m_Animator = GetComponent<Animator>();
	}

	void Update()
	{
		if (m_IsGameOver)
		{
			if (Keyboard.current.enterKey.wasPressedThisFrame)
			{
				GameManager.Instance.StartNewGame();
			}

			return;
		}

		if (GameManager.Instance.TurnManager.IsResolving || GameManager.Instance.ObjectMover.IsMoving)
		{
			return;
		}

		Vector2Int newCellTarget = m_CellPosition;

		bool hasMoved = false;

		if (Keyboard.current.upArrowKey.wasPressedThisFrame)
		{
			newCellTarget.y += 1;
			hasMoved = true;
		}
		else if (Keyboard.current.downArrowKey.wasPressedThisFrame)
		{
			newCellTarget.y -= 1;
			hasMoved = true;
		}
		else if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
		{
			newCellTarget.x += 1;
			hasMoved = true;
		}
		else if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
		{
			newCellTarget.x -= 1;
			hasMoved = true;
		}

		if (hasMoved)
		{
			BoardManager.CellData cellData = m_Board.GetCellData(newCellTarget);

			if (cellData != null && cellData.Passable)
			{
				GameManager.Instance.TurnManager.Tick();

				if (cellData.ContainedObject == null)
				{
					MoveTo(newCellTarget);
				}
				else if (cellData.ContainedObject.PlayerWantsToEnter())
				{
					MoveTo(newCellTarget);
				}
				else
				{
					Attack();
				}
			}
		}
	}

	public void Spawn(BoardManager boardManager, Vector2Int cell)
	{
		m_Board = boardManager;

		MoveTo(cell, true);
	}

	public void MoveTo(Vector2Int cell, bool immediate = false)
	{
		m_CellPosition = cell;
		Vector3 targetPosition = m_Board.CellToWorld(m_CellPosition);

		if (immediate)
		{
			transform.position = targetPosition;
		}
		else
		{
			GameManager.Instance.ObjectMover.AddMovement(transform, targetPosition, OnMoveFinished);
		}
	}

	private void OnMoveFinished()
	{
		BoardManager.CellData cellData = m_Board.GetCellData(m_CellPosition);
		if (cellData != null && cellData.ContainedObject != null)
		{
			cellData.ContainedObject.PlayerEntered();
		}
	}

	public void GameOver()
	{
		m_IsGameOver = true;
	}

	public void Init()
	{
		m_IsGameOver = false;
	}

	public void Attack()
	{
		m_Animator.SetTrigger("Attack");
	}

	public void TakeDamage()
	{
		m_Animator.SetTrigger("DamageTaken");
	}

	public Vector2Int GetCurrentCell()
	{
		return m_CellPosition;
	}
}
