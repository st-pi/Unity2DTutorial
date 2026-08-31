using UnityEngine;

public class Enemy : CellObject, ITurnActor
{
	public int Health = 3;

	private int m_CurrentHealth;
	private Animator m_Animator;

	private void Awake()
	{
		m_Animator = GetComponent<Animator>();
		GameManager.Instance.TurnManager.Register(this);
	}

	private void OnDestroy()
	{
		GameManager.Instance.TurnManager.Unregister(this);
	}

	public override void Init(Vector2Int coord)
	{
		base.Init(coord);
		m_CurrentHealth = Health;
	}

	public override bool PlayerWantsToEnter()
	{
		m_CurrentHealth -= 1;

		if (m_CurrentHealth <= 0)
		{
			Destroy(gameObject);
		}

		return false;
	}

	bool MoveTo(Vector2Int coord)
	{
		var board = GameManager.Instance.BoardManager;
		var targetCell = board.GetCellData(coord);

		if (targetCell == null
			|| !targetCell.Passable
			|| targetCell.ContainedObject != null)
		{
			return false;
		}

		var currentCell = board.GetCellData(m_Cell);
		currentCell.ContainedObject = null;

		targetCell.ContainedObject = this;
		m_Cell = coord;
		GameManager.Instance.ObjectMover.AddMovement(transform, board.CellToWorld(coord));

		return true;
	}

	public bool TakeTurn()
	{
		var playerCell = GameManager.Instance.PlayerController.GetCurrentCell();

		int xDist = playerCell.x - m_Cell.x;
		int yDist = playerCell.y - m_Cell.y;

		int absXDist = Mathf.Abs(xDist);
		int absYDist = Mathf.Abs(yDist);

		if ((xDist == 0 && absYDist == 1) || (yDist == 0 && absXDist == 1))
		{
			m_Animator.SetTrigger("Attack");
			GameManager.Instance.PlayerController.TakeDamage();
			GameManager.Instance.ChangeFood(-3);
			return true;
		}

		if (absXDist > absYDist)
		{
			return TryMoveInX(xDist) || TryMoveInY(yDist);
		}

		return TryMoveInY(yDist) || TryMoveInX(xDist);
	}

	bool TryMoveInX(int xDist)
	{
		if (xDist > 0)
		{
			return MoveTo(m_Cell + Vector2Int.right);
		}

		return MoveTo(m_Cell + Vector2Int.left);
	}

	bool TryMoveInY(int yDist)
	{
		if (yDist > 0)
		{
			return MoveTo(m_Cell + Vector2Int.up);
		}

		return MoveTo(m_Cell + Vector2Int.down);
	}
}
