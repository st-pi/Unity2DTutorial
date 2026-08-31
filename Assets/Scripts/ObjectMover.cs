using System.Collections.Generic;
using UnityEngine;

public class ObjectMover
{
	private class MovingObject
	{
		public Transform Transform;
		public Vector3 Target;
		public System.Action OnArrived;
	}

	public float MoveSpeed { get; set; }

	private readonly List<MovingObject> m_MovingObjects = new();
	private readonly List<MovingObject> m_ArrivedThisFrame = new();

	public bool IsMoving => m_MovingObjects.Count > 0;

	public ObjectMover(float moveSpeed)
	{
		MoveSpeed = moveSpeed;
	}

	public void AddMovement(Transform objectTransform, Vector3 target, System.Action onArrived = null)
	{
		for (int i = 0; i < m_MovingObjects.Count; ++i)
		{
			if (m_MovingObjects[i].Transform == objectTransform)
			{
				m_MovingObjects[i].Target = target;
				m_MovingObjects[i].OnArrived = onArrived;
				return;
			}
		}

		m_MovingObjects.Add(new MovingObject
		{
			Transform = objectTransform,
			Target = target,
			OnArrived = onArrived
		});
	}

	public void Tick(float deltaTime)
	{
		if (m_MovingObjects.Count == 0)
		{
			return;
		}

		float step = MoveSpeed * deltaTime;
		m_ArrivedThisFrame.Clear();

		for (int i = m_MovingObjects.Count - 1; i >= 0; --i)
		{
			MovingObject moving = m_MovingObjects[i];

			if (moving.Transform == null)
			{
				m_MovingObjects.RemoveAt(i);
				continue;
			}

			moving.Transform.position = Vector3.MoveTowards(moving.Transform.position, moving.Target, step);

			if (moving.Transform.position == moving.Target)
			{
				m_MovingObjects.RemoveAt(i);
				m_ArrivedThisFrame.Add(moving);
			}
		}

		for (int i = 0; i < m_ArrivedThisFrame.Count; ++i)
		{
			m_ArrivedThisFrame[i].OnArrived?.Invoke();
		}

		m_ArrivedThisFrame.Clear();
	}

	public void Clear()
	{
		m_MovingObjects.Clear();
		m_ArrivedThisFrame.Clear();
	}
}
