using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PastaGameLibrary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace GbJamTotem
{
	public class Prop : GameObject
	{
		public PhysicsComponent Physics;
		public bool IsVisible = false;

		public Prop(string textureName)
			: base()
		{
			m_sprite = new Sprite(Program.TheGame, TextureLibrary.GetSpriteSheet(textureName), new Transform(m_transform, true));
			m_sprite.Transform.Direction = Math.PI * 10;
			Physics = new PhysicsComponent(Program.TheGame, m_sprite.Transform);
			Physics.OnBounce = delegate() { IsVisible = false; };
		}

		public override void Update()
		{
			Physics.Update();
		}

		public override void Draw()
		{
			if(IsVisible)
				m_sprite.Draw();
		}
	}

	public class GlitterParticle : GameObject, IParticle
	{
		const float MaxTTL = 0.8f;
		float m_ttl = MaxTTL;
		float m_velX, m_velY;

		public GlitterParticle()
		{
			m_sprite = new Sprite(Program.TheGame, Cutscenes.auraTexture, m_transform);

			float speed = 20;
			float direction = (float)(Program.Random.NextDouble() * Math.PI * 2);
			m_velX = (float)(Math.Cos(direction) * speed);
			m_velY = (float)(Math.Sin(direction) * speed);
			m_transform.Position = Cutscenes.cutscenePlayer.Transform.Position;
		}

		public override void Update()
		{
			float dt =(float)Program.TheGame.ElapsedTime;
			m_ttl -= dt;
			m_transform.PosX += m_velX * dt;
			m_transform.PosY += m_velY * dt;
		}

		public bool RemoveMe()
		{
			return m_ttl <= 0;
		}

		public override void Draw()
		{
			m_sprite.Draw();
		}
	}

	public class SoulParticle : IParticle
	{
		Transform m_transform;
		Sprite m_sprite;
		PhysicsComponent m_physics;

		public SoulParticle()
		{
			m_transform = new Transform();
			m_transform.Position = Cutscenes.cutscenePlayer.Transform.Position;
			m_physics = new PhysicsComponent(Program.TheGame, m_transform);
			m_sprite = new Sprite(Program.TheGame, Cutscenes.soulTexture, m_transform);
			m_physics.Throw((float)Program.Random.NextDouble() * 0.5f + 0.6f, (float)Program.Random.NextDouble() * -2.0f, 0);
			m_physics.GroundLevel = 55;
		}

		public bool RemoveMe()
		{
			return !m_physics.IsProjected;
		}

		public void Update()
		{
			m_physics.Update();
		}

		public void Draw()
		{
			m_sprite.Draw();
		}
	}

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

		int m_soulsToGive;

		MoveToTransform m_moveToCrowd;
		MoveToTransform m_ascendMovement;
		Sequence m_carryAnimation;
		Sequence m_ascend;
		
		Concurrent m_shling;

		SingleActionManager m_actionManager;

		DelayAction m_swordDelay;
		MoveToTransform m_swordMovement;
		MoveToStaticAction m_cloudMovement;

		Sequence m_jumpFromTotem;
		MoveToStaticAction m_decelerate;

		Sequence m_hitSpikes;
		MoveToTransform m_moveToCrashingPlayer;

		//MoveToStaticAction m_walk;

		Prop m_sword, m_cloud;
		Sprite m_shlingSprite;

		SoundEffectInstance m_ascendSound;

		PhysicsComponent m_physics;
		Totem m_totemInstance;
		bool isVisible = true;

		Sequence m_levitate;
		MoveToStaticAction m_moveToCliffTip;
		ParticleGenerator<GlitterParticle> m_particleGenerator;
		ParticleGenerator<SoulParticle> m_soulParticleGenerator;
		ParticleSystem m_auraParticles;
		ParticleSystem m_soulParticles;
		bool m_generateAura, m_generateSouls;

		Sequence m_jumpInMouth;

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
			m_cloud = new Prop("cloud");
			m_sword = new Prop("sword");

			m_shlingSprite = new Sprite(Program.TheGame, TextureLibrary.GetSpriteSheet("shling"), new Transform(m_transform, true));

			m_sprite = new PastaGameLibrary.Sprite(Program.TheGame, TextureLibrary.GetSpriteSheet("player_cutscene", 1, 4), new Transform(m_transform, true));
			m_sprite.Origin = new Vector2(0.5f, 0.5f);
			m_sprite.PixelCorrection = false;

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
			m_swordMovement = new MoveToTransform(Program.TheGame, m_sword.Transform, start, end, 1);
			end.PosX = SwordOffsetToPlayerX;
			end.PosY = SwordOffsetToPlayerY;
			m_swordDelay = new DelayAction(Program.TheGame, AscendDuration * SwordStartTimeRatio);

			//Cloud movement
			m_cloudMovement = new MoveToStaticAction(Program.TheGame, m_cloud.Transform, new Vector2(CloudOffsetToPlayerX, CloudOffsetToPlayerY), 1);
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
				Game1.player.ShowPlayer();
				isVisible = false;
				m_cloud.IsVisible = false;
				m_sword.IsVisible = false;
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
			readyAnim.AddAction(new DelayAction(Program.TheGame, 0.5f));
			readyAnim.AddAction(new MethodAction(delegate() {
					Cutscenes.GetReady();
					SoundEffectLibrary.Get("sword_slash").Play();
				}));

			Concurrent shlingReady = new Concurrent(new PastaGameLibrary.Action[] { 
				m_shling,
				readyAnim,
			});

			m_ascend = new Sequence(1);
			m_ascend.AddAction(new DelayAction(Program.TheGame, Crowd.LaunchTensionTime));
			m_ascend.AddAction(new MethodAction(delegate() { m_ascendSound.Play(); }));
			Concurrent ascendAndSword = new Concurrent(new PastaGameLibrary.Action[] { m_ascendMovement, swordAndCloudMovement });
			m_ascend.AddAction(ascendAndSword);
			m_ascend.AddAction(showPlayer);
			m_ascend.AddAction(shlingReady);

			m_physics = new PhysicsComponent(Program.TheGame, m_transform);
			m_physics.Mass = 3.0f;

			m_jumpFromTotem = new Sequence(1);
			m_decelerate = new MoveToStaticAction(Program.TheGame, m_transform, Vector2.Zero, 1);
			m_decelerate.Interpolator = new PSquareInterpolation(0.5f);
			m_jumpFromTotem.AddAction(m_decelerate);
			m_jumpFromTotem.AddAction(new DelayAction(Program.TheGame, 0.2f));
			m_jumpFromTotem.AddAction(new MethodAction(delegate() 
				{
					m_physics.OnBounce = null;
					m_physics.Throw(1.0f, -2.0f, 0);
					Game1.CurrentMusic.StopDynamicMusic();
					SoundEffectLibrary.Get("sword_slash").Play();
				}));
			m_jumpFromTotem.AddAction(new DelayAction(Program.TheGame, 0.75f));
			m_jumpFromTotem.AddAction(new MethodAction(delegate()
			{
				Game1.SetupNextRound();
				if (Game1.CurrentTotem == null)
					Cutscenes.GoToCliff();
				else
					Cutscenes.GoToTotem(Game1.CurrentTotem, 1.0f, 0);
			}));
			m_actionManager = new SingleActionManager();

			m_hitSpikes = new Sequence(1);

			m_moveToCrashingPlayer = new MoveToTransform(Program.TheGame, Game1.GameCamera.Transform, new Transform(), new Transform(), 1);
			m_moveToCrashingPlayer.Timer.Interval = 0.2f;

			m_hitSpikes.AddAction(new DelayAction(Program.TheGame, 1.0f));
			m_hitSpikes.AddAction(m_moveToCrashingPlayer);
			m_hitSpikes.AddAction(new DelayAction(Program.TheGame, 0.5f));
			m_hitSpikes.AddAction(new MethodAction(delegate() { 
				if(Game1.CurrentTotem == null)
					Cutscenes.GoToCliff();
				else
					Cutscenes.GoToTotem(Game1.CurrentTotem, 1.0f, 0); 
				m_sprite.SetFrame(0);
				m_physics.OnBounce = null;
				m_physics.Throw(0, -3, 0);
				m_transform.PosY = m_physics.GroundLevel;
			}));

			m_auraParticles = new ParticleSystem(Program.TheGame, 100);
			m_soulParticles = new ParticleSystem(Program.TheGame, 500);

			m_levitate = new Sequence(1);
			m_moveToCliffTip = new MoveToStaticAction(Program.TheGame, m_transform, new Vector2(Cutscenes.InitialCharacterPosition + 10, -8), 1);
			m_moveToCliffTip.Interpolator = new PSmoothstepInterpolation();
			m_moveToCliffTip.Timer.Interval = 1.0f;
			m_levitate.AddAction(m_moveToCliffTip);
			m_levitate.AddAction(new MethodAction(delegate() { m_generateSouls = true; }));

			m_particleGenerator = new ParticleGenerator<GlitterParticle>(Program.TheGame, m_auraParticles);
			m_particleGenerator.Automatic = true;
			m_particleGenerator.GenerationInterval = 0.01f;
			m_soulParticleGenerator = new ParticleGenerator<SoulParticle>(Program.TheGame, m_soulParticles);

			m_jumpInMouth = new Sequence(1);
			m_jumpInMouth.AddAction(new DelayAction(Program.TheGame, 0.5f));
			m_jumpInMouth.AddAction(new MethodAction(delegate() { m_generateSouls = false; }));
			m_jumpInMouth.AddAction(new DelayAction(Program.TheGame, 0.5f));
			m_jumpInMouth.AddAction(new MethodAction(delegate() { JumpInMonsterMouth(); }));
			m_jumpInMouth.AddAction(new DelayAction(Program.TheGame, 0.25f));
			m_jumpInMouth.AddAction(new MethodAction(delegate() { Cutscenes.monster.CloseMouth(); }));
			m_jumpInMouth.AddAction(new DelayAction(Program.TheGame, 2.0f));
			m_jumpInMouth.AddAction(new MethodAction(delegate() { Cutscenes.crowd.PushNewGuy(); }));
			m_jumpInMouth.AddAction(new DelayAction(Program.TheGame, 1.0f));
			m_jumpInMouth.AddAction(new MethodAction(delegate() { Cutscenes.StartMainMenu(); }));
		}

		public void SetOriginMiddle()
		{
			m_sprite.Origin = new Vector2(0.5f, 0.5f);
		}
		public void SetOriginBottom()
		{
			m_sprite.Origin = new Vector2(0.5f, 1.0f);
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
			m_sword.Transform.ParentTransform = m_transform;
			m_swordDelay.Timer.Interval = AscendDuration * SwordStartTimeRatio;
			m_swordMovement.Start.Position = new Vector2(SwordStartX, SwordStartY);
			m_swordMovement.Start.Direction = SwordStartAngle;
			m_swordMovement.Timer.Interval = AscendDuration * (1 - SwordStartTimeRatio);
			m_sword.IsVisible = true;
			m_sword.Transform.Position = m_swordMovement.Start.Position;
			m_actionManager.StartNew(m_ascend);

			//Cloud
			m_cloud.Transform.ParentTransform = m_transform;
			m_cloudMovement.Timer.Interval = AscendDuration * (1 - SwordStartTimeRatio);
			m_cloud.IsVisible = true;
			m_cloud.Transform.Position = m_cloudMovement.StartPosition;

			//Shling
			m_shlingSprite.Transform.ScaleUniform = 0.0f;
			m_sprite.SetFrame(0);
			SetOriginMiddle();
		}
		public void JumpOnCrowd()
		{
			m_moveToCrowd.End = Cutscenes.crowd.PlayerCharacterTransform;
			m_moveToCrowd.Start = new Transform();
			m_moveToCrowd.Start.ParentTransform = Cutscenes.crowd.Transform;
			m_transform.Position -= Cutscenes.crowd.Transform.Position;
			m_transform.ParentTransform = Cutscenes.crowd.Transform;
			m_actionManager.StartNew(m_carryAnimation);
			Game1.swordSlashSound.Play();
		}
		public void JumpFromTotem()
		{
			m_transform.ParentTransform = null;
			m_transform.Position = Game1.player.SpriteTransform.PositionGlobal;
			m_decelerate.StartPosition = m_transform.Position;
			m_decelerate.Target = m_totemInstance.StopHotspot + m_totemInstance.Transform.Position;
			m_decelerate.Timer.Interval = 0.5f / Game1.player.SpeedMultiplier;
			m_actionManager.StartNew(m_jumpFromTotem);
			isVisible = true;
			Game1.scoreBorder.Slide(false);
			Game1.mapBorder.Slide(false);
			Game1.successJingle.Play();
			SetOriginBottom();
		}
		public void HitSpikes()
		{
			float horizontalHitForce = 1.5f;
			Game1.CurrentMusic.StopDynamicMusic();

			m_transform.ParentTransform = null;
			m_transform.Position = Game1.player.SpriteTransform.PositionGlobal + new Vector2(CharacterOffsetToPlayerX, CharacterOffsetToPlayerY);

			m_sword.IsVisible = true;
			m_sword.Transform.Position += m_sword.Transform.ParentTransform.PositionGlobal;
			m_sword.Transform.ParentTransform = null;
			m_cloud.IsVisible = true;
			m_cloud.Transform.Position += m_cloud.Transform.ParentTransform.PositionGlobal;
			m_cloud.Transform.ParentTransform = null;

			if (Game1.player.SpriteTransform.Position.X < 0)
			{
				m_physics.Throw(-horizontalHitForce, -3, 1.0f);
				m_sword.Physics.Throw(-horizontalHitForce, -2, 1.0f);
				m_cloud.Physics.Throw(-horizontalHitForce, -1, 1.0f);
			}
			else
			{
				m_physics.Throw(horizontalHitForce, -3, 1.0f);
				m_sword.Physics.Throw(horizontalHitForce, -2, -1.0f);
				m_cloud.Physics.Throw(-horizontalHitForce, -1, 1.0f);
			}
			m_physics.OnBounce = HitGround;
			isVisible = true;
		}
		private void HitGround()
		{
			m_physics.Stop();
			Game1.GameCamera.Transform.Position += Game1.GameCamera.Transform.ParentTransform.PositionGlobal;
			Game1.GameCamera.Transform.ParentTransform = null;
			m_transform.PosY = 0;
			m_transform.Direction = 0;
			Game1.groundImpact.Play();
			Game1.GameCamera.Shake(2, 0.3f);
			m_moveToCrashingPlayer.Start.Position = Game1.GameCamera.Transform.Position;
			m_moveToCrashingPlayer.Start.ScaleUniform = Game1.GameCamera.Transform.SclX;
			m_moveToCrashingPlayer.End.Position = m_transform.PositionGlobal + new Vector2(0, Cutscenes.CameraHeightOnGround);
			m_moveToCrashingPlayer.End.ScaleUniform = 1.0f;
			m_sprite.SetFrame(1);
			m_actionManager.StartNew(m_hitSpikes);
			Game1.scoreBorder.Slide(false);
			Game1.mapBorder.Slide(false);
			Game1.SetupNextRound();
			SetOriginBottom();
		}
		public void EjectFromCrowd()
		{
			SetOriginBottom();
			m_physics.Throw(2.5f, -3, 0);
			m_physics.GroundLevel = 0;
			m_transform.Direction = 0;
			m_sprite.SetFrame(3);
		}
		public void GiveSouls(int soulsToGive)
		{
			SetOriginMiddle();
			if (m_transform.ParentTransform != null)
				m_transform.Position += m_transform.ParentTransform.PositionGlobal;
			m_transform.ParentTransform = null;
			m_moveToCliffTip.StartPosition = m_transform.Position;
			m_sprite.SetFrame(0);
			m_actionManager.StartNew(m_levitate);
			m_generateAura = true;
			m_soulsToGive = soulsToGive;
			Game1.scoreDisplay.SlideIn();
		}
		public void DropFromCrowd(float fx, float fy)
		{
			m_physics.Throw(fx, fy, 0);
		}
		public void JumpInMonsterMouth()
		{
			m_physics.Throw(1.5f, -2.0f, 0.5f);
			m_physics.GroundLevel = 80;
			m_generateAura = false;
			Game1.swordSlashSound.Play();
		}

		public override void Update()
		{
			if (m_generateAura)
				m_particleGenerator.Update();
			if (m_generateSouls)
			{
				m_soulParticleGenerator.Generate();
				m_soulsToGive--;
				Game1.scoreDisplay.Value += 1;
				if (m_soulsToGive <= 0)
				{
					Game1.scoreDisplay.Value = Game1.TotalScore;
					m_generateSouls = false;
					m_actionManager.StartNew(m_jumpInMouth);
				}
			}

			m_soulParticles.Update();
			m_auraParticles.Update();

			m_physics.Update();
			m_sword.Update();
			m_cloud.Update();
			m_actionManager.Update();
			//throw new NotImplementedException();
		}

		public override void Draw()
		{
			if (isVisible)
			{
				m_soulParticles.Draw();
				m_auraParticles.Draw();

				m_cloud.Draw();
				m_sprite.Draw();
				m_sword.Draw();
			}
			if (m_shling.IsActive)
				m_shlingSprite.Draw();
		}
	}


	public static class Cutscenes
	{
		public static Title title;
		public static Crowd crowd;
		public static Monster monster;
		public static CutscenePlayer cutscenePlayer;


		public static SpriteSheet auraTexture;
		public static SpriteSheet soulTexture;

		//General
		public const float CameraHeightOnGround = -50;
		const float TotemCrowdOffset = -20.0f;
		const float TimeToTotem = 1.0f;

		//Intro
		const float MoveInTime = 5.0f;

		//Main Menu
		const float CameraMenuX = 360;
		const float CameraMenuY = 30;
		public const float InitialCharacterPosition = 325;
		const float InitialCrowdPosition = 265;

		//MoveToTotem
		const float CameraDelay = 1.5f;
		const float TimeToFirstTotem = 2.0f;
		static MoveToTransform moveToAscendingPlayer;
		static MoveToStaticAction moveToTotem;
		static Sequence gotoFirstTotem;
		static Sequence playerLaunch;

		//Ready
		static MoveToTransform moveToFallingPlayer;
		static Sequence readySequence; 

		//Totem positions
		const float Totem1Position = 0.0f;
		static float currentTotemPosition = 0.0f;
		static SingleActionManager actionManager = new SingleActionManager();

		//Gameplay Zoom
		static float[] TargetZooms = new float[] { 1.0f, 0.92f, 0.84f, 0.76f };
		static ScaleToAction cameraZoom;

		static MoveToTransform goToPlayerOnGroundMovement;
		static Sequence goToPlayerOnGround;

		static Sequence intro;
		static Sequence goToCliff;

		public static bool IsReady
		{
			get { return actionManager.IsActive; }
		}

		public static void Initalise()
		{
			cutscenePlayer = new CutscenePlayer();
			cutscenePlayer.Transform.PosX = -100;
			monster = new Monster();
			crowd = new Crowd(40, 18, new Vector2(2.5f, 0.5f));

			title = new Title();

			DelayAction cameraDelay = new DelayAction(Program.TheGame, CameraDelay);
			moveToTotem = new MoveToStaticAction(Program.TheGame, Game1.GameCamera.Transform, Vector2.Zero, 1);
			moveToTotem.StartPosition = new Vector2(CameraMenuX, CameraMenuY);
			moveToTotem.Interpolator = new PSmoothstepInterpolation();
			moveToTotem.Timer.Interval = TimeToFirstTotem;
			MethodAction moveCrowd = new MethodAction(delegate() { 
				crowd.MoveTo(currentTotemPosition + TotemCrowdOffset, TimeToFirstTotem); });
			gotoFirstTotem = new Sequence(1);
			gotoFirstTotem.AddAction(cameraDelay);
			gotoFirstTotem.AddAction(moveCrowd);
			gotoFirstTotem.AddAction(moveToTotem);
			gotoFirstTotem.AddAction(new MethodAction(delegate() { Cutscenes.ThrowPlayer(Game1.CurrentTotem); }));

			playerLaunch = new Sequence(1);
			playerLaunch.AddAction(new DelayAction(Program.TheGame, Crowd.LaunchTensionTime));
			moveToAscendingPlayer = new MoveToTransform(Program.TheGame, Game1.GameCamera.Transform, new Transform(), cutscenePlayer.Transform, 1);
			moveToAscendingPlayer.Interpolator = new PSquareInterpolation(0.1f);
			moveToAscendingPlayer.RotationActive = false;
			playerLaunch.AddAction(moveToAscendingPlayer);

			readySequence = new Sequence(1);
			Transform end = new Transform(Game1.player.Transform, true);
			end.PosY = Game1.CameraOffset;
			moveToFallingPlayer = new MoveToTransform(Program.TheGame, Game1.GameCamera.Transform, new Transform(), end, 1);
			moveToFallingPlayer.Interpolator = new PSmoothstepInterpolation();
			moveToFallingPlayer.RotationActive = false;

			readySequence.AddAction(new MethodAction(delegate() { Game1.player.GetReady(); }));
			readySequence.AddAction(moveToFallingPlayer);
			readySequence.AddAction(new MethodAction(delegate() { Game1.player.StartCountDown(); })) ;

			cameraZoom = new ScaleToAction(Program.TheGame, Game1.GameCamera.Transform, Vector2.Zero, 1);
			cameraZoom.Interpolator = new PSmoothstepInterpolation();
			cameraZoom.Timer.Interval = 0.3f;

			goToPlayerOnGround = new Sequence(1);
			goToPlayerOnGroundMovement = new MoveToTransform(Program.TheGame, Game1.GameCamera.Transform, new Transform(), cutscenePlayer.Transform, 1);
			goToPlayerOnGroundMovement.Timer.Interval = 1.0f;
			goToPlayerOnGroundMovement.Interpolator = new PSmoothstepInterpolation();
			goToPlayerOnGround.AddAction(new DelayAction(Program.TheGame, 0.5f));
			goToPlayerOnGround.AddAction(goToPlayerOnGroundMovement);

			intro = new Sequence(1);
			MoveToStaticAction moveToMenu = new MoveToStaticAction(Program.TheGame, Game1.GameCamera.Transform, new Vector2(CameraMenuX, CameraMenuY), 1);
			moveToMenu.Timer.Interval = MoveInTime + 1.0f;

			intro.AddAction(moveToMenu);
			intro.AddAction(new MethodAction(delegate() { crowd.PushNewGuy(); }));
			intro.AddAction(new DelayAction(Program.TheGame, 1.5f));
			intro.AddAction(new MethodAction(delegate() { StartMainMenu(); }));

			goToCliff = new Sequence(1);
			goToCliff.AddAction(moveToTotem);
			goToCliff.AddAction(new MethodAction(delegate() { cutscenePlayer.GiveSouls(Game1.TotalScore); monster.OpenMouth(); }));

			auraTexture = TextureLibrary.GetSpriteSheet("soul_temp");
			soulTexture = TextureLibrary.GetSpriteSheet("soul");
			//moveToAscendingPlayer = new MoveToTransform(Program.TheGame, Game1.GameCamera.Transform, new Transform(), cutscenePlayer.Transform, 1);
		}

		public static void ZoomToStage(int stageNumber)
		{
			float targetZoom = TargetZooms[Math.Max(0, Math.Min(3, stageNumber))];
			cameraZoom.StartScale = Game1.GameCamera.Transform.Scale;
			cameraZoom.Target = new Vector2(targetZoom, targetZoom);
			actionManager.StartNew(cameraZoom);
		}

		public static void Intro()
		{
			crowd.Transform.Position = new Vector2(-100, 0);
			crowd.MoveTo(InitialCrowdPosition, MoveInTime);
			Game1.GameCamera.Transform.Position = crowd.Transform.Position;
			actionManager.StartNew(intro);
		}

		public static void StartMainMenu()
		{
			Game1.scoreDisplay.SlideOut();
			crowd.Transform.Position = new Vector2(InitialCrowdPosition, 0);
			//cutscenePlayer.Transform.Position = new Vector2(InitialCharacterPosition, 0);
			Game1.GameCamera.Transform.Position = new Vector2(CameraMenuX, CameraMenuY);
			title.Appear();
			Game1.menuScreen.ShowMenu();
			monster.Idle();
		}

		public static void ThrowPlayer(Totem totem)
		{
			crowd.LaunchPlayer();
			Game1.player.Initialise(totem);
			moveToAscendingPlayer.Start.Position = Game1.GameCamera.Transform.Position;
			cutscenePlayer.Launch(totem);
			moveToAscendingPlayer.Timer.Interval = cutscenePlayer.AscendDuration; //Total time of animation = crowd stretch + throwing time
			actionManager.StartNew(playerLaunch);
		}

		public static void DropPlayer(float forceX, float forceY)
		{
			crowd.LaunchPlayer();
		}

		public static void GetReady()
		{
			Game1.player.GetReady();
			Game1.GameCamera.Transform.ParentTransform = Game1.player.Transform;
			Game1.GameCamera.Transform.Position -= Game1.player.Transform.PositionGlobal;
			moveToFallingPlayer.Start.Position = Game1.GameCamera.Transform.Position;
			actionManager.StartNew(readySequence);
		}

		public static void GoToTotem(Totem totem, float time, int pickupOffset)
		{
			moveToTotem.StartPosition = Game1.GameCamera.Transform.Position;
			moveToTotem.Target = new Vector2(totem.Transform.PosX, CameraHeightOnGround);
			currentTotemPosition = totem.Transform.PosX;
			crowd.PickupPlayer(time, pickupOffset);
			actionManager.StartNew(gotoFirstTotem);
		}
		public static void GoToCliff()
		{
			moveToTotem.StartPosition = Game1.GameCamera.Transform.Position;
			moveToTotem.Target = new Vector2(CameraMenuX, CameraMenuY);
			crowd.MoveTo(InitialCrowdPosition, 2.0f);
			actionManager.StartNew(goToCliff);
		}

		public static void FinishTotem()
		{
			Game1.player.FinishTotem();
			cutscenePlayer.JumpFromTotem();
			Game1.GameCamera.Transform.Position += Game1.GameCamera.Transform.ParentTransform.PositionGlobal;
			Game1.GameCamera.Transform.ParentTransform = null;
			goToPlayerOnGroundMovement.Start.Position = Game1.GameCamera.Transform.Position;
			actionManager.StartNew(goToPlayerOnGround);
		}

		public static void Update()
		{
			actionManager.Update();
			crowd.Update();
			monster.Update();
			cutscenePlayer.Update();
			title.Update();
		}
	}
}
