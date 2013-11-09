using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PastaGameLibrary;
using Microsoft.Xna.Framework;

namespace GbJamTotem
{
	public class CutscenePlayer : GameObject
	{
		const float CarryHeight = -20.0f;
		MoveToTransform m_moveToCrowd;
		Sequence m_carryAnimation;
		SingleActionManager m_actionManager;

		public CutscenePlayer()
			: base()
		{
			m_sprite = new PastaGameLibrary.Sprite(Program.TheGame, TextureLibrary.GetSpriteSheet("perso_foule"), m_transform);
			m_sprite.Origin = new Vector2(0.5f, 1.0f);

			m_moveToCrowd = new MoveToTransform(Program.TheGame, m_transform, null, null, 1);
			m_moveToCrowd.Timer.Interval = 0.1f;
			MoveToStaticAction carryMovement = new MoveToStaticAction(Program.TheGame, m_transform, new Vector2(0, CarryHeight - 5), 1);
			carryMovement.Interpolator = new PBounceInterpolation(1.0f);
			carryMovement.StartPosition = new Vector2(0, CarryHeight);
			carryMovement.Timer.Interval = 0.2f;
			MethodAction action = new MethodAction(delegate() { carryMovement.StartPosition = new Vector2(0, CarryHeight); });

			Sequence bounceAnimation = new Sequence(-1);
			bounceAnimation.AddAction(carryMovement);
			bounceAnimation.AddAction(action);

			m_carryAnimation = new Sequence(1);
			m_carryAnimation.AddAction(m_moveToCrowd);
			m_carryAnimation.AddAction(bounceAnimation);

			m_actionManager = new SingleActionManager();
		}

		public void JumpOnCrowd()
		{
			m_moveToCrowd.End = Cutscenes.crowd.PlayerCharacterTransform;
			m_moveToCrowd.Start = new Transform();
			m_moveToCrowd.Start.ParentTransform = Cutscenes.crowd.Transform;
			m_transform.ParentTransform = Cutscenes.crowd.Transform;
			m_actionManager.StartNew(m_carryAnimation);
		}

		public override void Update()
		{
			m_actionManager.Update();
			//throw new NotImplementedException();
		}

		public override void Draw()
		{
			m_sprite.Draw();
		}
	}


	public static class Cutscenes
	{
		public static Crowd crowd;
		public static CutscenePlayer cutscenePlayer;

		//General
		const float CameraHeightOnGround = -50;
		const float TotemCrowdOffset = -20.0f;
		const float TimeToTotem = 1.0f;

		//Main Menu
		const float CameraMenuX = 350;
		const float CameraMenuY = 30;
		const float InitialCharacterPosition = 315;
		const float InitialCrowdPosition = 260;

		//Intro
		const float CameraDelay = 1.5f;
		const float TimeToFirstTotem = 2.0f;
		static MoveToStaticAction moveTo;
		static Sequence goToTotem;
		static Sequence cameraIntro;

		//Totem positions
		const float Totem1Position = 0.0f;
		static float currentTotemPosition = 0.0f;
		static SingleActionManager actionManager = new SingleActionManager();

		public static bool IsReady
		{
			get { return actionManager.IsActive; }
		}

		public static void Initalise()
		{
			cutscenePlayer = new CutscenePlayer();
			cutscenePlayer.Transform.PosX = -100;
			crowd = new Crowd(28, 20, new Vector2(2.5f, 0.5f));

			DelayAction cameraDelay = new DelayAction(Program.TheGame, CameraDelay);
			MoveToStaticAction moveToTotem = new MoveToStaticAction(Program.TheGame, Game1.GameCamera.Transform, new Vector2(Totem1Position, CameraHeightOnGround), 1);
			moveToTotem.StartPosition = new Vector2(CameraMenuX, CameraMenuY);
			moveToTotem.Interpolator = new PSmoothstepInterpolation();
			moveToTotem.Timer.Interval = TimeToFirstTotem;
			MethodAction moveCrowd = new MethodAction(delegate() { crowd.MoveTo(currentTotemPosition + TotemCrowdOffset, TimeToFirstTotem); });
			cameraIntro = new Sequence(1);
			cameraIntro.AddAction(cameraDelay);
			cameraIntro.AddAction(moveCrowd);
			cameraIntro.AddAction(moveToTotem);
		}

		public static void StartMainMenu()
		{
			crowd.Transform.Position = new Vector2(InitialCrowdPosition, 0);
			cutscenePlayer.Transform.Position = new Vector2(InitialCharacterPosition, 0);
			Game1.GameCamera.Transform.Position = new Vector2(CameraMenuX, CameraMenuY);
		}

		public static void GoToTotem(Totem totem)
		{
			currentTotemPosition = totem.Transform.PosX;
			if (actionManager.IsActive)
				return;
			crowd.PickupPlayer();
			actionManager.StartNew(cameraIntro);
		}

		public static void Update()
		{
			actionManager.Update();
			crowd.Update();
			cutscenePlayer.Update();
		}
	}
}
