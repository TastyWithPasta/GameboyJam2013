using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PastaGameLibrary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace GbJamTotem
{
	public class CutscenePlayer : GameObject
	{
		const float CloudStartX = -200;
		const float SwordStartX = 200;
		const float SwordStartY = -250;
		const double SwordStartAngle = Math.PI * 7;
		const float CarryHeight = -20.0f;
		const float SwordStartTimeRatio = 0.65f; //Ratio du temps de départ de l'épée par rapport à AscendDuration

		const float SwordOffsetToPlayerX = 10;
		const float SwordOffsetToPlayerY = -2;
		const float CharacterOffsetToPlayerX = -7;
		const float CharacterOffsetToPlayerY = -1;
		const float CloudOffsetToPlayerX = -1;
		const float CloudOffsetToPlayerY = 12;

		const float ShlingScale = 4.0f;
		const float ShlingTime = 0.7f;
		const float ShlingSpin = 4.0f;

		MoveToTransform m_moveToCrowd;
		MoveToTransform m_ascendMovement;
		Sequence m_carryAnimation;
		Sequence m_ascend;
		Concurrent m_shling;

		SingleActionManager m_actionManager;

		DelayAction m_swordDelay;
		MoveToTransform m_swordMovement;
		MoveToStaticAction m_cloudMovement;

		Sprite m_swordSprite;
		Sprite m_cloudSprite; //Cloud Strife lol
		Sprite m_shlingSprite;

		SoundEffectInstance m_ascendSound;

		Totem m_totemInstance;
		bool isVisible = true;

		public bool IsVisible
		{
			get { return isVisible; }
			set { isVisible = value; }
		}

		public float AscendDuration
		{
			get {
				return 4.5f;
				return Math.Abs(m_totemInstance.Top * 0.002f);
			}
		}

		public CutscenePlayer()
			: base()
		{
			m_ascendSound = SoundEffectLibrary.Get("ascend").CreateInstance();

			m_swordSprite = new Sprite(Program.TheGame, TextureLibrary.GetSpriteSheet("sword"), new Transform(m_transform, true));
			m_swordSprite.Transform.Direction = Math.PI * 10;

			m_cloudSprite = new Sprite(Program.TheGame, TextureLibrary.GetSpriteSheet("cloud"), new Transform(m_transform, true));

			m_shlingSprite = new Sprite(Program.TheGame, TextureLibrary.GetSpriteSheet("shling"), new Transform(m_transform, true));

			m_sprite = new PastaGameLibrary.Sprite(Program.TheGame, TextureLibrary.GetSpriteSheet("player_nosword"), m_transform);
			m_sprite.Origin = new Vector2(0.5f, 0.5f);

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


			//Sword movement
			Transform start = new Transform(m_transform, true);
			Transform end = new Transform(m_transform, true);
			m_swordMovement = new MoveToTransform(Program.TheGame, m_swordSprite.Transform, start, end, 1);
			end.PosX = SwordOffsetToPlayerX;
			end.PosY = SwordOffsetToPlayerY;
			m_swordDelay = new DelayAction(Program.TheGame, AscendDuration * SwordStartTimeRatio);

			//Cloud movement
			m_cloudMovement = new MoveToStaticAction(Program.TheGame, m_cloudSprite.Transform, new Vector2(CloudOffsetToPlayerX, CloudOffsetToPlayerY), 1);
			m_cloudMovement.StartPosition = new Vector2(CloudStartX, 0);
			m_cloudMovement.Interpolator = new PSquareInterpolation(0.25f);

			//Delay of the ascend, then sword/cloud movement
			Sequence swordAndCloudMovement = new Sequence(1);
			swordAndCloudMovement.AddAction(m_swordDelay);
			swordAndCloudMovement.AddAction(new Concurrent(new PastaGameLibrary.Action[] { m_swordMovement, m_cloudMovement }));
			
			m_ascendMovement = new MoveToTransform(Program.TheGame, m_transform, new Transform(), new Transform(), 1);
			m_ascendMovement.Interpolator = new PSquareInterpolation(0.5f);

			MethodAction showPlayer = new MethodAction(delegate()
			{
				m_sprite.Transform.PosY -= 1;
				Game1.player.IsVisible = true;
				isVisible = false;
			});

			//Shling!
			ScaleToAction shlingScale = new ScaleToAction(Program.TheGame, m_shlingSprite.Transform, new Vector2(ShlingScale, ShlingScale), 1);
			shlingScale.Timer.Interval = ShlingTime;
			shlingScale.StartScale = Vector2.Zero;
			shlingScale.Interpolator = new PSquareInterpolation(2);

			RotateToStaticAction shlingRotate = new RotateToStaticAction(Program.TheGame, m_shlingSprite.Transform, ShlingSpin, 1);
			shlingRotate.Timer.Interval = ShlingTime;
			m_shling = new Concurrent(new PastaGameLibrary.Action[] { shlingScale, shlingRotate });


			Sequence readyAnim = new Sequence(1);
			readyAnim.AddAction(new DelayAction(Program.TheGame, 1.0f));
			readyAnim.AddAction(new MethodAction(delegate() {
					Cutscenes.GetReady();
				}));

			Concurrent shlingReady = new Concurrent(new PastaGameLibrary.Action[] { 
				m_shling,
				readyAnim
			});

			m_ascend = new Sequence(1);
			m_ascend.AddAction(new DelayAction(Program.TheGame, Crowd.LaunchTensionTime));
			m_ascend.AddAction(new MethodAction(delegate() { m_ascendSound.Play(); }));
			Concurrent ascendAndSword = new Concurrent(new PastaGameLibrary.Action[] { m_ascendMovement, swordAndCloudMovement });
			m_ascend.AddAction(ascendAndSword);
			m_ascend.AddAction(showPlayer);
			m_ascend.AddAction(shlingReady);
			

			
			

			m_actionManager = new SingleActionManager();
		}

		public void Launch(Totem totem)
		{
			m_totemInstance = totem;
			if(m_transform.ParentTransform != null)
				m_transform.Position += m_transform.ParentTransform.PositionGlobal;
			m_transform.ParentTransform = null;

			//Ascend movement
			m_ascendMovement.Start.Position = m_transform.Position;
			m_ascendMovement.Start.Direction = m_transform.Direction + Math.PI * 4;
			m_ascendMovement.End.Position = Game1.player.SpriteTransform.PositionGlobal + new Vector2(CharacterOffsetToPlayerX, CharacterOffsetToPlayerY);
			m_ascendMovement.Timer.Interval = AscendDuration;

			//Sword
			m_swordDelay.Timer.Interval = AscendDuration * SwordStartTimeRatio;
			m_swordMovement.Start.Position = new Vector2(SwordStartX, SwordStartY);
			m_swordMovement.Start.Direction = SwordStartAngle;
			m_swordMovement.Timer.Interval = AscendDuration * (1 - SwordStartTimeRatio);
			m_actionManager.StartNew(m_ascend);

			//Cloud
			m_cloudMovement.Timer.Interval = AscendDuration * (1 - SwordStartTimeRatio);

			//Shling
			m_shlingSprite.Transform.ScaleUniform = 0.0f;
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
			if (isVisible)
			{
				if (m_cloudMovement.IsActive)
					m_cloudSprite.Draw();
					m_sprite.Draw();
				if (m_swordMovement.IsActive)
					m_swordSprite.Draw();
			}
			if (m_shling.IsActive)
				m_shlingSprite.Draw();
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
		const float CameraMenuX = 200;
		const float CameraMenuY = 30;
		const float InitialCharacterPosition = 165;
		const float InitialCrowdPosition = 110;

		//Intro
		const float CameraDelay = 1.5f;
		const float TimeToFirstTotem = 2.0f;
		static MoveToStaticAction moveTo;
		static MoveToTransform m_moveToPlayer;
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
			crowd = new Crowd(25, 25, new Vector2(2.5f, 0.5f));

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

			m_moveToPlayer = new MoveToTransform(Program.TheGame, Game1.GameCamera.Transform, new Transform(), cutscenePlayer.Transform, 1);
			m_moveToPlayer.Interpolator = new PSquareInterpolation(0.1f);
			m_moveToPlayer.RotationActive = false;
		}

		public static void StartMainMenu()
		{
			crowd.Transform.Position = new Vector2(InitialCrowdPosition, 0);
			cutscenePlayer.Transform.Position = new Vector2(InitialCharacterPosition, 0);
			Game1.GameCamera.Transform.Position = new Vector2(CameraMenuX, CameraMenuY);
		}

		public static void ThrowPlayer(Totem totem)
		{
			crowd.LaunchPlayer();
			Game1.player.Initialise(totem);
			m_moveToPlayer.Start.Position = Game1.GameCamera.Transform.Position;
			cutscenePlayer.Launch(totem);
			m_moveToPlayer.Timer.Interval = cutscenePlayer.AscendDuration + Crowd.LaunchTensionTime; //Total time of animation = crowd stretch + throwing time
			actionManager.StartNew(m_moveToPlayer);
		}

		public static void GetReady()
		{
			Game1.player.GetReady();
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
