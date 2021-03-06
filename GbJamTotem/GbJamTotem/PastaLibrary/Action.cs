﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace PastaGameLibrary
{
	public abstract class Action
	{
		bool m_isActive = false;
		bool m_isPaused = false;
		public bool IsActive
		{
			get { return m_isActive; }
		}

		public void Start()
		{
			if (m_isActive)
				return;
			m_isActive = true;
			m_isPaused = false;
			OnStart();
		}
		public void Restart()
		{
			Stop();
			Start();
		}

		public void Stop()
		{
			m_isActive = false;
			OnStop();
		}
		public void Pause()
		{
			m_isPaused = true;
			OnPause();
		}
		public void Update()
		{
			if(!m_isActive || m_isPaused)
				return;
			OnUpdate();
		}

		protected abstract void OnUpdate();
		protected virtual void OnStart() { }
		protected virtual void OnStop() { }
		protected virtual void OnPause() { }
		protected virtual void OnResume() { }
	}
	public abstract class DurationAction : Action, IDisposable
	{
		MyGame m_theGame = null;
		PTimer m_timer = null;
		IPInterpolation<float> m_interpolator = new PLerpInterpolation();
		int m_playsLeft = -1;
		int m_initialAmountOfPlays = -1;

		public DurationAction(MyGame theGame, int amountOfPlays)
			: base()
		{
			m_theGame = theGame;
			m_timer = new PTimer(m_theGame.TimerManager, Loop);
			m_playsLeft = m_initialAmountOfPlays = amountOfPlays;
		}

		private void Loop()
		{
			if (m_playsLeft > 0)
				m_playsLeft--;
			if (m_playsLeft < 0)
				Restart();
			if (m_playsLeft == 0)
			{
				Update();
				Stop();
			}
		}

		public IPInterpolation<float> Interpolator
		{
			get { return m_interpolator; }
			set { m_interpolator = value; }
		}
		public PTimer Timer
		{
			get { return m_timer; }
		}
		public MyGame TheGame
		{
			get { return m_theGame; }
		}
		public bool IsLooping
		{
			get { return m_playsLeft < 0; }
		}

		protected override void OnStart()
		{
			base.OnStart();
			m_timer.Start();
		}

		protected override void OnPause()
		{
			base.OnPause();
			m_timer.Pause();
		}

		protected override void OnStop()
		{
			base.OnStop();
			m_timer.Stop();
			m_playsLeft = m_initialAmountOfPlays;
		}

		public void Dispose()
		{
			m_timer.Dispose();
		}
	}
	public class MethodAction : Action
	{
		public delegate void ActionMethod();
		ActionMethod m_actionMethod;
		bool m_loop = false;

		public MethodAction(ActionMethod actionMethod)
			: base()
		{
			m_actionMethod = actionMethod;
		}
		public MethodAction(ActionMethod actionMethod, bool loop)
			: base()
		{
			m_loop = loop;
			m_actionMethod = actionMethod;
		}

		protected override void OnUpdate()
		{
			if (m_loop)
				m_actionMethod();
			//throw new NotImplementedException();
		}

		protected override void OnStop()
		{
			base.OnStop();
			m_loop = false;
		}

		protected override void OnStart()
		{
			base.OnStart();
			m_actionMethod();
			if(!m_loop)
				Stop();
		}
	}

	public class FloatPointerAnimation : DurationAction
	{
		public class FloatContainer
		{
			public float Value;
		}

		float m_startValue, m_endValue;
		FloatContainer m_valueToAnimate;

		public FloatPointerAnimation(MyGame theGame, FloatContainer valueToAnimate, float targetValue, int playAmount)
			: base(theGame, playAmount)
		{
			m_startValue = valueToAnimate.Value;
			m_endValue = targetValue;
			m_valueToAnimate = valueToAnimate;
		}

		protected override void OnUpdate()
		{
			m_valueToAnimate.Value = Interpolator.GetInterpolation(m_startValue, m_endValue, Timer.ProgressRatio);
		}
	}
	public class Vector2PointerAnimation : DurationAction
	{
		public class Vector2Container
		{
			public Vector2 Value;
		}

		Vector2 m_startValue, m_endValue;
		Vector2Container m_valueToAnimate;

		public Vector2PointerAnimation(MyGame theGame, Vector2Container valueToAnimate, Vector2 targetValue, int playAmount)
			: base(theGame, playAmount)
		{
			m_startValue = valueToAnimate.Value;
			m_endValue = targetValue;
			m_valueToAnimate = valueToAnimate;
		}

		protected override void OnUpdate()
		{
			m_valueToAnimate.Value.X = Interpolator.GetInterpolation(m_startValue.X, m_endValue.X, Timer.ProgressRatio);
			m_valueToAnimate.Value.Y = Interpolator.GetInterpolation(m_startValue.Y, m_endValue.Y, Timer.ProgressRatio);
		}
	}

	public class SpriteSheetAnimation : DurationAction, IDisposable
	{
		int m_startFrame, m_endFrame;
		Sprite m_sprite;

		public int StartFrame
		{
			get { return m_startFrame; }
		}
		public int EndFrame
		{
			get { return m_endFrame; }
		}

		public SpriteSheetAnimation(Sprite sprite, int startFrame, int endFrame, float intervalInSeconds, int playAmount)
			: base(sprite.TheGame, playAmount)
		{
			m_sprite = sprite;
			m_startFrame = startFrame;
			m_endFrame = endFrame;
			Timer.Interval = intervalInSeconds;
		}

		protected override void OnUpdate()
		{
			float interpolation = Interpolator.GetInterpolation(m_startFrame, m_endFrame + 1, Timer.ProgressRatio);
			int currentFrame = (int)interpolation;
			if (currentFrame <= m_endFrame)
				m_sprite.SetFrame(currentFrame);
		}
	}
	public class DelayAction : DurationAction
	{
		public DelayAction(MyGame theGame, float delayTime)
			: base(theGame, 1)
		{
			Timer.Interval = delayTime;
		}

		protected override void OnUpdate()
		{}
	}

	public class MoveToStaticAction : DurationAction, IDisposable
	{
		Transform m_transform = null;
		Vector2 m_target, m_startPos;

		public Vector2 Target
		{
			get { return m_target; }
			set { m_target = value; }
		}
		public Vector2 StartPosition
		{
			get { return m_startPos; }
			set { m_startPos = value; }
		}

		public MoveToStaticAction(MyGame theGame, Transform transform, Vector2 target, int playAmount)
			: base(theGame, playAmount)
		{
			m_transform = transform;
			m_startPos = m_transform.Position;
			m_target = target;
		}

		protected override void OnUpdate()
		{
			float progressRatio = Timer.ProgressRatio;

			m_transform.Position = new Vector2(Interpolator.GetInterpolation(m_startPos.X, m_target.X, progressRatio),
												Interpolator.GetInterpolation(m_startPos.Y, m_target.Y, progressRatio));
		}
	}
	public class MoveToTransform : DurationAction, IDisposable
	{
		Transform m_transform = null;
		Transform m_start, m_end;
		public bool RotationActive = true;
		public bool ScaleActive = true;
		public bool TranslationActive = true;

		public Transform End
		{
			get { return m_end; }
			set { m_end = value; }
		}

		public Transform Start
		{
			get { return m_start; }
			set { m_start = value; }
		}

		public MoveToTransform(MyGame theGame, Transform transform, Transform start, Transform end, int playAmount)
			: base(theGame, playAmount)
		{
			m_transform = transform;
			m_start = start;
			m_end = end;
		}
		protected override void OnUpdate()
		{
			float progressRatio = Math.Min(Timer.ProgressRatio, 1);

			if(TranslationActive)
			m_transform.Position = new Vector2(Interpolator.GetInterpolation(m_start.PosX, m_end.PosX, progressRatio),
												Interpolator.GetInterpolation(m_start.PosY, m_end.PosY, progressRatio));
			if(ScaleActive)
			m_transform.Scale = new Vector2(Interpolator.GetInterpolation(m_start.SclX, m_end.SclX, progressRatio),
												Interpolator.GetInterpolation(m_start.SclY, m_end.SclY, progressRatio));
			if(RotationActive)
			m_transform.Direction = Interpolator.GetInterpolation((float)m_start.Direction, (float)m_end.Direction, progressRatio);

		}
	}

	public class ScaleToAction : DurationAction, IDisposable
	{
		Transform m_transform = null;
		Vector2 m_target, m_startScale;

		public Vector2 StartScale
		{
			get { return m_startScale; }
			set { m_startScale = value; }
		}
		public Vector2 Target
		{
			get { return m_target; }
			set { m_target = value; }
		}

		public ScaleToAction(MyGame theGame, Transform transform, Vector2 target, int playAmount)
			: base(theGame, playAmount)
		{
			m_transform = transform;
			m_startScale = m_transform.Scale;
			m_target = target;
		}
		protected override void OnUpdate()
		{
			float progressRatio = Timer.ProgressRatio;

			m_transform.Scale = new Vector2(Interpolator.GetInterpolation(m_startScale.X, m_target.X, progressRatio),
												Interpolator.GetInterpolation(m_startScale.Y, m_target.Y, progressRatio));
		}
	}
	public class RotateToStaticAction : DurationAction, IDisposable
	{
		float m_rotationTarget, m_startRot;
		Transform m_transform;

		public RotateToStaticAction(MyGame theGame, Transform transform, float target, int playAmount)
			: base(theGame, playAmount)
		{
			m_transform = transform;
			m_startRot = (float)m_transform.Direction;
			m_rotationTarget = target;
		}
		protected override void OnUpdate()
		{
			m_transform.Direction = Interpolator.GetInterpolation(m_startRot, m_rotationTarget, Timer.ProgressRatio);
		}
	}

	public class Concurrent : Action
	{
		Action[] m_actions;


		public Concurrent(Action[] actions)
		{
			m_actions = actions;
		}

		protected override void OnUpdate()
		{
			int amountOfActive = 0;
			for (int i = 0; i < m_actions.Length; ++i)
			{
				m_actions[i].Update();
				if (m_actions[i].IsActive)
					amountOfActive++;
			}
			if (amountOfActive == 0)
				Stop();
		}
		protected override void OnStart()
		{
			for (int i = 0; i < m_actions.Length; ++i)
			{
				m_actions[i].Stop();
				m_actions[i].Start();
			}
		}
		protected override void OnPause()
		{
			for (int i = 0; i < m_actions.Length; ++i)
				m_actions[i].Pause();
		}
		protected override void OnStop()
		{
			for (int i = 0; i < m_actions.Length; ++i)
				m_actions[i].Stop();
		}
	}
	public class Sequence : Action
	{
		List<Action> m_actions;
		int m_currentActive;
		int m_playsLeft = 1;
		float m_totalTime = 0;

		public Sequence(int amountOfPlays)
			: base()
		{
			m_playsLeft = amountOfPlays;
			m_actions = new List<Action>();
		}

		public void UpdateTotalTime()
		{
			m_totalTime = 0;
			DurationAction da;
			for(int i = 0; i < m_actions.Count; ++i)
				if(m_actions[i] is DurationAction)
				{
					da = (DurationAction)(m_actions[i]);
					m_totalTime += da.Timer.Interval;
				}
		}

		public float TotalTime
		{
			get
			{
				return m_totalTime;
			}
		}

		public float ProgressRatio
		{
			get
			{
				DurationAction da;
				float time = 0;
				for (int i = 0; i < m_currentActive; ++i)
					if (m_actions[i] is DurationAction)
					{
						da = (DurationAction)(m_actions[i]);
						time += da.Timer.Interval;
					}

				if(m_currentActive is DurationAction)
				{
					da = (DurationAction)(m_actions[m_currentActive]);
					time += da.Timer.ProgressRatio * da.Timer.Interval;
				}
				return time / m_totalTime;
			}
		}

		private void Loop()
		{
			if (m_playsLeft > 0)
				m_playsLeft--;
			if (m_playsLeft < 0)
				Restart();
			if (m_playsLeft == 0)
				Stop();
		}

		public void AddAction(Action action)
		{
			m_actions.Add(action);
			UpdateTotalTime();
		}

		protected override void OnStart()
		{
			m_actions[0].Start();
			m_currentActive = 0;
		}
		protected override void OnStop()
		{
			for(int i = 0; i < m_actions.Count; ++i)
				m_actions[i].Stop();
		}
		protected override void OnUpdate()
		{
			m_actions[m_currentActive].Update();
			if (!m_actions[m_currentActive].IsActive)
			{
				m_currentActive++;
				if (m_currentActive >= m_actions.Count)
					Loop();
				else
					m_actions[m_currentActive].Start();
			}
		}
	}


	//A single action is active at any given time.
	public class SingleActionManager : IPUpdatable
	{
		Action m_currentAction;

		public void StartNew(Action currentAction)
		{
			if(m_currentAction != null)
				m_currentAction.Stop();
			m_currentAction = currentAction;
			m_currentAction.Start();
		}
		public bool IsActive
		{
			get {
				if (m_currentAction == null)
					return false;
				return m_currentAction.IsActive; }
		}
		public void Stop()
		{
			m_currentAction.Stop();
		}
		public void Start()
		{
			m_currentAction.Start();
		}
		public void Update()
		{
			if (m_currentAction != null)
				m_currentAction.Update();
		}
	}

	//Actions are destroyed when stopped. Normally, they will play only once.
	public class ExpendableActionManager : IPUpdatable
	{
		List<Action> m_expendableActions = new List<Action>();

		public void Add(Action action)
		{
			m_expendableActions.Add(action);
		}
		public void Remove(Action action)
		{
			m_expendableActions.Remove(action);
		}
		public void ClearAll()
		{
			m_expendableActions.Clear();
		}
		public void StartAll()
		{
			for (int i = 0; i < m_expendableActions.Count; ++i)
				m_expendableActions[i].Start();
		}
		public void Update()
		{
			for (int i = m_expendableActions.Count - 1; i > -1; --i)
			{
				if (m_expendableActions[i].IsActive)
					m_expendableActions[i].Update();
				else
					m_expendableActions.RemoveAt(i);
			}
		}
	}

	//Actions will not be destroyed when stopped.
	public class ConcurrentActionManager : IPUpdatable
	{
		List<Action> m_concurrentActions = new List<Action>();

		public void StartNew(Action action)
		{
			m_concurrentActions.Add(action);
			action.Start();
		}
		public void ClearAll()
		{
			m_concurrentActions.Clear();
		}
		public void StartAll()
		{
			for (int i = 0; i < m_concurrentActions.Count; ++i)
				m_concurrentActions[i].Start();
		}
		public void StopAll()
		{
			for (int i = 0; i < m_concurrentActions.Count; ++i)
				m_concurrentActions[i].Stop();
		}
		public void Update()
		{
			for (int i = 0; i < m_concurrentActions.Count; ++i)
				m_concurrentActions[i].Update();
		}
	}
}
