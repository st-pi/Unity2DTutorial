using System.Collections.Generic;
using UnityEngine;

public class TurnManager
{
	public event System.Action OnTick;

	private readonly ObjectMover m_ObjectMover;
	private readonly float m_StepDelay;

	private readonly List<ITurnActor> m_Actors = new();
	private readonly List<ITurnActor> m_PendingActors = new();

	private int m_TurnCount;
	private int m_NextActorIndex;
	private float m_WaitTimer;

	public bool IsResolving { get; private set; }

	public TurnManager(ObjectMover objectMover, float stepDelay)
	{
		m_ObjectMover = objectMover;
		m_StepDelay = stepDelay;
		m_TurnCount = 1;
	}

	public void Register(ITurnActor actor)
	{
		if (!m_Actors.Contains(actor))
		{
			m_Actors.Add(actor);
		}
	}

	public void Unregister(ITurnActor actor)
	{
		m_Actors.Remove(actor);
	}

	public void Tick()
	{
		m_TurnCount += 1;

		OnTick?.Invoke();

		m_PendingActors.Clear();
		m_PendingActors.AddRange(m_Actors);
		m_NextActorIndex = 0;
		m_WaitTimer = m_StepDelay;
		IsResolving = true;
	}

	public void Update(float deltaTime)
	{
		if (!IsResolving)
		{
			return;
		}

		if (m_WaitTimer > 0.0f)
		{
			m_WaitTimer -= deltaTime;
			return;
		}

		if (m_ObjectMover.IsMoving)
		{
			return;
		}

		while (m_NextActorIndex < m_PendingActors.Count)
		{
			ITurnActor actor = m_PendingActors[m_NextActorIndex];
			m_NextActorIndex += 1;

			if (actor is UnityEngine.Object unityObject && unityObject == null)
			{
				continue;
			}

			if (actor.TakeTurn())
			{
				m_WaitTimer = m_StepDelay;
				return;
			}
		}

		m_PendingActors.Clear();
		IsResolving = false;
	}

	public void Reset()
	{
		m_PendingActors.Clear();
		m_NextActorIndex = 0;
		m_WaitTimer = 0.0f;
		IsResolving = false;
	}
}
