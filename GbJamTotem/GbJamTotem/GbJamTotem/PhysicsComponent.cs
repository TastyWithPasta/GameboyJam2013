 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PastaGameLibrary;

namespace GbJamTotem
{
	public class PhysicsComponent : IPUpdatable
	{
		MyGame m_myGame;
		Transform m_transform;

		private float m_stillnessTimer;
		private float m_previousY;
		public float StillnessTime = 0.3f;
		public int GroundLevel = 0;
		public float Mass, Friction, Restitution, AirFriction;
		private float m_angleIncrement;
		private bool m_isProjected;
		private Vector2 m_velocity;

		public bool IsProjected
		{
			get { return m_isProjected; }
		}

		public PhysicsComponent(MyGame theGame, Transform transform)
		{
			m_myGame = theGame;
			m_transform = transform;
			Mass = 1;
			Friction = 1;
			Restitution = 1;
			AirFriction = 1;
		}

		public void Update()
		{
			if (m_isProjected)
				UpdateProjectionPhysics();
		}
		public void Throw(float fx, float fy, float angleIncrementRange)
		{
			m_stillnessTimer = StillnessTime;
			m_isProjected = true;
			m_velocity.X = fx;
			m_velocity.Y = fy;

			if (angleIncrementRange != 0)
			{
				m_transform.Direction += (float)(Program.Random.NextDouble() * 0.5 - 0.25);
				m_angleIncrement = (float)(Program.Random.NextDouble() * angleIncrementRange - angleIncrementRange * 0.5);
			}
			else
				m_angleIncrement = 0;
		}
		public void Stop()
		{
			m_isProjected = false;
			m_velocity = Vector2.Zero;
			m_angleIncrement = 0;
		}
		private void UpdateProjectionPhysics()
		{
			m_previousY = m_transform.PosY;

			m_transform.Direction += m_angleIncrement;
			m_velocity.Y += 0.1f * Mass;
			m_velocity *= AirFriction;
			m_transform.PosX += m_velocity.X;
			m_transform.PosY += m_velocity.Y;

			if (m_transform.PosY >= GroundLevel)
			{
				m_velocity.X *= 0.5f * Friction;
				m_angleIncrement *= 0.5f;
			}
			if (m_transform.PosY > GroundLevel)
			{
				m_transform.PosY = GroundLevel;
				m_velocity.Y *= -0.4f * Restitution;
			}

			if (m_previousY == m_transform.PosY)
			{
				m_stillnessTimer -= (float)m_myGame.ElapsedTime;
				if (m_stillnessTimer < 0)
					m_isProjected = false;
			}

		}
	}
}
