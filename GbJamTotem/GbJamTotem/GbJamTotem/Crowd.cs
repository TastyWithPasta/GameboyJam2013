using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PastaGameLibrary;
using Microsoft.Xna.Framework;

namespace GbJamTotem
{
	class BooleanInterpolation : IPInterpolation<float>
	{
		float[] m_ticks;

		public BooleanInterpolation(float[] ticks)
		{
 			m_ticks = ticks;
		}
		public BooleanInterpolation()
		{
			m_ticks = new float[] { 0.5f };
		}
		public float GetInterpolation(float from, float to, float ratio)
		{
			bool isDown = false;

			for (int i = 0; i < m_ticks.Length; ++i)
				if (ratio > m_ticks[i])
					isDown = !isDown;

			if (isDown)
				return from;
			return to;
		}
	}

	class Individual : GameObject
	{
		const float JumpMovementTime = 0.4f;
		const float TotalJumpTime = JumpMovementTime + 0.3f;

		const float WalkAnimationTime = 0.2f;

		const float WaitRatio = 0.9f;
		const float JumpHeight = 10.0f;

		Transform m_spriteTransform, m_jumpTransform;
		float m_moveFactor;
		Sequence m_saut;

		Sequence m_throw;
		ScaleToAction m_throwStretch;
		MoveToStaticAction m_walk;

		SingleActionManager m_actionManager;

		public float MoveFactor
		{
			get { return m_moveFactor; }
			set { m_moveFactor = value; }
		}

		public Individual(Crowd crowd, Vector2 positionInCrowd) : base()
		{
			m_transform.Position = positionInCrowd;
			m_transform.ParentTransform = crowd.Transform;

			m_spriteTransform = new Transform();
			m_jumpTransform = new PastaGameLibrary.Transform(m_spriteTransform, true);
			m_sprite = new PastaGameLibrary.Sprite(Program.TheGame, TextureLibrary.GetSpriteSheet("perso_foule"), m_jumpTransform);
			m_sprite.Origin = new Vector2(0.5f, 1.0f);
			
			m_saut = new Sequence(-1);

			///
			///Jump animation
			///
			Vector2 jumpTarget = new Vector2(0, (float)(Program.Random.NextDouble() * JumpHeight * 0.5 + JumpHeight * 0.5));
			float movementTime = (float)(Program.Random.NextDouble() * 0.5 * JumpMovementTime + JumpMovementTime * 0.5);
			MoveToStaticAction jumpMovement = new MoveToStaticAction(Program.TheGame, m_jumpTransform, jumpTarget, 1);
			jumpMovement.Timer.Interval = movementTime;
			jumpMovement.Interpolator = new PBounceInterpolation(0);
			m_saut.AddAction(jumpMovement);
			m_saut.AddAction(new DelayAction(Program.TheGame, TotalJumpTime - movementTime));

			///
			///Walk animation
			///
			m_walk = new MoveToStaticAction(Program.TheGame, m_jumpTransform, new Vector2(0, 1), -1);
			m_walk.Timer.Interval = (float)Program.Random.NextDouble() * WalkAnimationTime * 0.5f + WalkAnimationTime * 0.5f;
			m_walk.Interpolator = new BooleanInterpolation();

			///
			///Throw player animation
			///
			m_throw = new Sequence(1);
			float intensity = 1 - Math.Min(1, m_transform.Position.Length() * 0.02f);
			m_throwStretch = new ScaleToAction(Program.TheGame, m_jumpTransform, new Vector2(1.0f, 1 - intensity * 0.5f), 1);
			m_throwStretch.Interpolator = new PSmoothstepInterpolation();
			ScaleToAction throwDestretch = new ScaleToAction(Program.TheGame, m_jumpTransform, new Vector2(1.0f, 1.0f), 1);
			throwDestretch.Timer.Interval = 0.1f;

			m_throw.AddAction(m_throwStretch);

			MoveToStaticAction throwMovement = new MoveToStaticAction(Program.TheGame, m_jumpTransform, Vector2.Zero, 1);
			throwMovement.StartPosition = new Vector2(0, -20 - 30 * intensity);
			throwMovement.Timer.Interval = 0.4f;
			throwMovement.Interpolator = new PBounceInterpolation(0, 1);
			m_throw.AddAction(new Concurrent(new PastaGameLibrary.Action[] { throwMovement, throwDestretch }));

			m_actionManager = new SingleActionManager();
		}

		public void Jump()
		{
			m_actionManager.StartNew(m_saut);
		}
		public void StartWalk()
		{
			m_actionManager.StartNew(m_walk);
		}
		public void StopWalk()
		{
			m_walk.Stop();
		}
		public void LaunchPlayer(float stretchTime)
		{
			m_throwStretch.Timer.Interval = stretchTime;
			m_actionManager.StartNew(m_throw);
		}

		public override void Update()
		{
			m_actionManager.Update();
			m_spriteTransform.ScaleUniform = Math.Min(2, Math.Max(0, 1 + m_transform.PosY * 0.035f));
			m_spriteTransform.Position = m_spriteTransform.Position + (m_transform.PositionGlobal - m_spriteTransform.Position) * m_moveFactor;
		}

		public override void Draw()
		{
			m_sprite.Draw();
		}
	}


	public class Crowd : GameObject
	{
		public const float LaunchTensionTime = 1.0f;
		const float FrontToBackRatio = 0.5f; //Pourcentage de personnage sur la tranche avant de la foule
		const float MoveSpeed = 0.25f; //Value between 0 and 1
		List<Individual> m_people = new List<Individual>();
		List<Individual> m_frontPeople = new List<Individual>(); //Personnages à l'avant de la foule
		List<Individual> m_backPeople = new List<Individual>(); //Personnages à l'arrière de la foule
		Vector2 m_sizeRatio;
		Vector2 m_previousPosition;
		float m_radius;

		Transform m_playerCharacterTransform;
		MoveToStaticAction m_moveToMovement;
		Sequence m_moveTo;
		Sequence m_pickupPlayer;
		SingleActionManager m_animationManager;

		public Transform PlayerCharacterTransform
		{
			get { return m_playerCharacterTransform; }
		}

		public Crowd(int amountOfPopulation, float radius, Vector2 sizeRatio)
		{
			m_radius = radius;
			m_sizeRatio = sizeRatio;

			m_playerCharacterTransform = new Transform(m_transform, true);
			m_playerCharacterTransform.Direction = 1.56f;
			m_playerCharacterTransform.PosY = -25;

			int maxIndex;
			float currentRadius, currentAngle;

			//Générer le devant
			maxIndex = (int)(amountOfPopulation * FrontToBackRatio);
			for (int i = 0; i < amountOfPopulation; ++i)
			{
				currentRadius = (float)(Program.Random.NextDouble() * m_radius);
				currentAngle = (float)(Program.Random.NextDouble() * 6.28);
				Vector2 position = new Vector2((float)Math.Cos(currentAngle) * currentRadius * m_sizeRatio.X, (float)Math.Sin(currentAngle) * currentRadius * m_sizeRatio.Y);
				Individual individu = new Individual(this, position);
				m_people.Add(individu);
				if (individu.Transform.PosY > 0)
					m_frontPeople.Add(individu);
				else
					m_backPeople.Add(individu);
			}

			m_frontPeople = m_frontPeople.OrderBy(o=>o.Transform.PosY).ToList();

			///
			/// MoveTo animation
			///
			m_moveToMovement = new MoveToStaticAction(Program.TheGame, m_transform, Vector2.Zero, 1);
			MethodAction stopWalk = new MethodAction(delegate() { for (int i = 0; i < m_people.Count; ++i) m_people[i].StopWalk(); });
			m_moveTo = new Sequence(1);
			m_moveTo.AddAction(m_moveToMovement);
			m_moveTo.AddAction(stopWalk);

			///
			/// Pickup player animation
			///
			m_pickupPlayer = new Sequence(1);
			m_pickupPlayer.AddAction(m_moveToMovement);
			//m_pickupPlayer.AddAction(new DelayAction(Program.TheGame, 0.1f));
			m_pickupPlayer.AddAction(new MethodAction(delegate() { Cutscenes.cutscenePlayer.JumpOnCrowd();}));
			m_animationManager = new SingleActionManager();
		}

		public void PickupPlayer(float time)
		{
			for (int i = 0; i < m_people.Count; ++i)
				m_people[i].StartWalk();
			m_moveToMovement.StartPosition = m_transform.Position;
			m_moveToMovement.Target = Cutscenes.cutscenePlayer.Transform.Position;
			m_moveToMovement.Timer.Interval = time;
			m_animationManager.StartNew(m_pickupPlayer);
		}
		public void Jump()
		{
			for (int i = 0; i < m_people.Count; ++i)
				m_people[i].Jump();
		}
		public void LaunchPlayer()
		{
			for (int i = 0; i < m_people.Count; ++i)
				m_people[i].LaunchPlayer(LaunchTensionTime);
		}
		public void MoveTo(float position, float time)
		{
			for (int i = 0; i < m_people.Count; ++i)
				m_people[i].StartWalk();
			m_moveToMovement.StartPosition = m_transform.Position;
			m_moveToMovement.Timer.Interval = time;
			m_moveToMovement.Target = new Vector2(position, 0);
			m_animationManager.StartNew(m_moveTo);
		}

		public override void Update()
		{
			for (int i = 0; i < m_people.Count; ++i)
				m_people[i].Update();
			m_animationManager.Update();

			Vector2 direction = (m_transform.Position - m_previousPosition);
			if (direction.X == 0 && direction.X == 0)
				return;
			direction.Normalize();
			direction *= m_sizeRatio * m_radius;

			Individual leader = m_people[0];
			Vector2 target = m_transform.Position + direction;
			float minDistance, maxDistance, currentDistance;
			minDistance = maxDistance = Vector2.Distance(m_people[0].Transform.Position, target);
			for (int i = 1; i < m_people.Count; ++i)
			{
				currentDistance = Vector2.Distance(m_people[i].Transform.PositionGlobal, target);
				if (currentDistance < minDistance)
				{
					leader = m_people[i];
					minDistance = currentDistance;
				}
				else if (currentDistance > maxDistance)
				{
					maxDistance = currentDistance;
				}
			}

			for (int i = 0; i < m_people.Count; ++i)
			{
				float dist = Vector2.Distance(m_people[i].Transform.PositionGlobal, target);
				float ratio = 1 - (dist - minDistance) / (maxDistance - minDistance);
				ratio = Math.Max(0.1f, Math.Min(0.5f, ratio));
				m_people[i].MoveFactor = ratio;
			}
			m_previousPosition = m_transform.Position;

			
		}

		public void DrawFront()
		{
			for (int i = 0; i < m_frontPeople.Count; ++i)
				m_frontPeople[i].Draw();
		}
		public void DrawBack()
		{
			for (int i = 0; i < m_backPeople.Count; ++i)
				m_backPeople[i].Draw();
		}
		public override void Draw()
		{
			throw new NotImplementedException();
		}
	}
}
