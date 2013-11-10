using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using PastaGameLibrary;

namespace GbJamTotem
{

	/// <summary>
	/// This is the main type for your game
	/// </summary>
	public class Game1 : MyGame
	{
		public const int InitialTotemPosition = 0;
		public const int TotemSpacing = 90;

		public const int GameboyWidth = 160;
		public const int GameboyHeight = 144;
		public const int CameraOffset = 50;

        public const int screenZoom = 4;

		public static Random Random = new Random();
        public static ParticleSystem Souls;
        public static ParticleSystem Explosions;

		public static Camera2D GameCamera;
		public static KeyboardState kbs = new KeyboardState();
        public static KeyboardState old_kbs = new KeyboardState();
        public static Player player;

        public static PauseScreen pauseScreen;
        public static MenuScreen menuScreen;
        public static Countdown startingCountdown;
        public static ScoreBorder scoreBorder;
        public static MapBorder mapBorder;
        public static ComboCounter comboCounter;
        public static int normalTotemValue = 100;

		public static bool isInGameplay = false;

        public static SpriteFont menuText;
        public static SpriteFont debugText;
        bool debugMode = true;

        Sprite m_falaise;

		//Color m_bgColor = new Color(239, 255, 222);
        
        static Color blue = new Color(166, 202, 240);
        static Color white = new Color(255, 251, 240);
        Color m_bgColor = blue;

		GameboyDrawer m_drawer;
		static int currentIndex = 0;
		public static Totem[] totems = new Totem[3];
		public static DynamicMusic[] musics = new DynamicMusic[3];

		public static Totem CurrentTotem
		{
			get
			{
				if (currentIndex >= totems.Length || currentIndex < 0)
					return null;
				return totems[currentIndex];
			}
		}

		public static DynamicMusic CurrentMusic
		{
			get
			{
				if (currentIndex >= musics.Length || currentIndex < 0)
					return null;
				return musics[currentIndex];
			}
		}

		public static DrawingList Foreground = new DrawingList();

        PowerUp p;

        PTimer m_flashComboTimer;

        #region SOUND & MUSIC ATTRIBUTES

        public static SoundEffectInstance normalTotemCollisionSound_Channel1;
        public static SoundEffectInstance normalTotemCollisionSound_Channel2;

        public static SoundEffectInstance metalTotemCollisionSound_Channel1;
        public static SoundEffectInstance metalTotemCollisionSound_Channel2;

        public static SoundEffectInstance swordSlashSound;
        public static SoundEffectInstance moveLeftToRightSound;
        public static SoundEffectInstance moveRightToLeftSound;
		public static SoundEffectInstance spikeHitSound;
		public static SoundEffectInstance groundImpact;

        public static SoundEffectInstance feedback_combo3;
        public static SoundEffectInstance feedback_combo6;
        public static SoundEffectInstance feedback_combo9;
        public static SoundEffectInstance feedback_comboBreaker;
        public static bool feedbackLock; // Avoid looping sound if combo remains the same
        public static bool isComboBreakerSoundPossible; // Activate comboBreakerSound sound when comboCount >= comboMax;

        public static SoundEffectInstance musicT4P1L1;
        public static SoundEffectInstance musicT4P1L2;
        public static SoundEffectInstance musicT4P1L3;
        public static SoundEffectInstance musicT4P1L4;

        public static SoundEffectInstance musicT4P2L1;
        public static SoundEffectInstance musicT4P2L2;
        public static SoundEffectInstance musicT4P2L3;
        public static SoundEffectInstance musicT4P2L4;

        public static SoundEffectInstance musicT4P3L1;
        public static SoundEffectInstance musicT4P3L2;
        public static SoundEffectInstance musicT4P3L3;
        public static SoundEffectInstance musicT4P3L4;


        #endregion


        public Game1()
            : base(160 * screenZoom, 144 * screenZoom)
		{
			Souls = new ParticleSystem(this, 500);
            Explosions = new ParticleSystem(this, 100);
		}
		protected override void Initialize()
		{
			// TODO: Add your initialization logic here
            m_flashComboTimer = new PTimer(this.TimerManager, delegate() { m_bgColor = blue; });
            m_flashComboTimer.Interval = 0.1f;
			base.Initialize();
		}

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
        protected override void LoadContent()
        {
            TextureLibrary.LoadContent(Content, "Textures");
            TextureLibrary.Initialise(GraphicsDevice);

            debugText = Content.Load<SpriteFont>("Text/Debug");
            menuText = Content.Load<SpriteFont>("Text/Menu");

            SoundEffectLibrary.LoadContent(Content, "SoundEffects");

            m_drawer = new GameboyDrawer(this);
            GameCamera = new Camera2D(new Vector2(GameboyWidth * 0.5f, GameboyHeight * 0.5f));
            GameCamera.ScaleToZoom = true;

			// Totem
			//

			LoadLevel(1);
			//totems[0] = new Totem();
			////testTotem.BuildFromFile("Level1_p1");
			//totems[0].AddSections(new SectionData(typeof(NormalSection), 0, 0, 30));
			//totems[0].AddSections(new SectionData(typeof(MetalSection), 10, 10, 7));
			//totems[0].AddSections(new SectionData(typeof(SpikeSection), 4, 4, 7));
			//totems[0].BuildRandom();
			//totems[0].Transform.PosX = 100;


            // Player initialisation	
            //
            player = new Player();

            // Pause screen & GUI initialisation
            //
            menuScreen = new MenuScreen();
            pauseScreen = new PauseScreen();
            startingCountdown = new Countdown();
            scoreBorder = new ScoreBorder();
            mapBorder = new MapBorder();
            comboCounter = new ComboCounter(player);

            p = new PowerUp(true);
            p.Transform.Position = new Vector2(40, -1800);

            // Background textures
            //
			m_falaise = new Sprite(this, TextureLibrary.GetSpriteSheet("decors_sol_falaise"), new Transform());
			m_falaise.Origin = new Vector2(0, 0.32f);
            m_falaise.Transform.PosX = -60;
			m_falaise.Transform.PosY = 10;

            // Sound & musics
            //
            normalTotemCollisionSound_Channel1 = SoundEffectLibrary.Get("normal_collision_sound").CreateInstance();
            normalTotemCollisionSound_Channel2 = SoundEffectLibrary.Get("normal_collision_sound").CreateInstance();
            metalTotemCollisionSound_Channel1 = SoundEffectLibrary.Get("metal_collision_sound").CreateInstance();
            metalTotemCollisionSound_Channel2 = SoundEffectLibrary.Get("metal_collision_sound").CreateInstance();

            swordSlashSound = SoundEffectLibrary.Get("sword_slash").CreateInstance();
            moveLeftToRightSound = SoundEffectLibrary.Get("move_left_to_right").CreateInstance();
            moveRightToLeftSound = SoundEffectLibrary.Get("move_right_to_left").CreateInstance();
			spikeHitSound = SoundEffectLibrary.Get("spike_hit").CreateInstance();
			groundImpact = SoundEffectLibrary.Get("ground_slam").CreateInstance();

            feedback_combo3 = SoundEffectLibrary.Get("feedback_combo3").CreateInstance();
            feedback_combo6 = SoundEffectLibrary.Get("feedback_combo6").CreateInstance();
            feedback_combo9 = SoundEffectLibrary.Get("feedback_combo9").CreateInstance();
            feedback_comboBreaker = SoundEffectLibrary.Get("feedback_comboBreaker").CreateInstance();
            feedbackLock = true;
            isComboBreakerSoundPossible = false;

            musicT4P1L1 = SoundEffectLibrary.Get("music_T4P1L1").CreateInstance();
            musicT4P1L2 = SoundEffectLibrary.Get("music_T4P1L2").CreateInstance();
            musicT4P1L3 = SoundEffectLibrary.Get("music_T4P1L3").CreateInstance();
            musicT4P1L4 = SoundEffectLibrary.Get("music_T4P1L4").CreateInstance();

            musicT4P2L1 = SoundEffectLibrary.Get("music_T4P2L1").CreateInstance();
            musicT4P2L2 = SoundEffectLibrary.Get("music_T4P2L2").CreateInstance();
            musicT4P2L3 = SoundEffectLibrary.Get("music_T4P2L3").CreateInstance();
            musicT4P2L4 = SoundEffectLibrary.Get("music_T4P2L4").CreateInstance();

            musicT4P3L1 = SoundEffectLibrary.Get("music_T4P3L1").CreateInstance();
            musicT4P3L2 = SoundEffectLibrary.Get("music_T4P3L2").CreateInstance();
            musicT4P3L3 = SoundEffectLibrary.Get("music_T4P3L3").CreateInstance();
            musicT4P3L4 = SoundEffectLibrary.Get("music_T4P3L4").CreateInstance();

            //dynamicMusic = new DynamicMusic(musicT1P1L1, musicT1P1L2, musicT1P1L3, musicT1P1L4);
            //dynamicMusic = new DynamicMusic(musicT1P2L1, musicT1P2L2, musicT1P2L3, musicT1P2L4);
            dynamicMusic = new DynamicMusic(musicT4P2L1, musicT4P2L2, musicT4P2L3, musicT4P2L4);
            //dynamicMusic = new DynamicMusic(musicT4P3L1, musicT4P3L2, musicT4P3L3, musicT4P3L4);
			//Foule et joueur porté
			Cutscenes.Initalise();
			Cutscenes.StartMainMenu();
			SetupNextLevel();
        }

		/// <summary>
		/// UnloadContent will be called once per game and is the place to unload
		/// all content.
		/// </summary>
		protected override void UnloadContent()
		{
			// TODO: Unload any non ContentManager content here
		}

		public static void SetupNextLevel()
		{
			currentIndex = -1;
			SetupNextRound();
		}
		public static void SetupNextRound()
		{
			if (CurrentMusic != null)
				CurrentMusic.StopDynamicMusic();

			currentIndex++;
			player.Initialise(CurrentTotem);
			startingCountdown.resetCountdown();
		}
		public void LoadLevel(int index)
		{
			string path = "Level_" + index;

			totems[0] = new Totem();
			totems[0].BuildFromFile(path + "/Level" + index + "_1");
			totems[1] = new Totem();
			totems[1].BuildFromFile(path + "/Level" + index + "_2");
			totems[2] = new Totem();
			totems[2].BuildFromFile(path + "/Level" + index + "_3");

			for (int i = 0; i < 3; ++i)
			{
				SoundEffectInstance musicL1 = SoundEffectLibrary.Get("music_T" + index + "P" + (i + 1) + "L1").CreateInstance();
				SoundEffectInstance musicL2 = SoundEffectLibrary.Get("music_T" + index + "P" + (i + 1) + "L2").CreateInstance();
				SoundEffectInstance musicL3 = SoundEffectLibrary.Get("music_T" + index + "P" + (i + 1) + "L3").CreateInstance();
				SoundEffectInstance musicL4 = SoundEffectLibrary.Get("music_T" + index + "P" + (i + 1) + "L4").CreateInstance();
				musics[i] = new DynamicMusic(musicL1, musicL2, musicL3, musicL4);
			}
			
			PlaceTotems();
		}

		private void PlaceTotems()
		{
			for (int i = 0; i < totems.Length; ++i)
				totems[i].Transform.PosX = InitialTotemPosition + i * TotemSpacing;
		}

		public void Flash(float time)
        {
            m_bgColor = white;
			m_flashComboTimer.Interval = time;
            m_flashComboTimer.Stop();
            m_flashComboTimer.Start();
        }

		public void UpdateComboEffects()
		{
			if (CurrentMusic.State == DynamicMusic.dynamicMusicState.PLAYING)
			{
				switch (player.ComboCount)
				{
					case 0:
						if (isComboBreakerSoundPossible)
						{
							feedback_comboBreaker.Play();
							feedbackLock = true;
							isComboBreakerSoundPossible = false;
						}
						CurrentMusic.ResetSecondaryLayers();
						Cutscenes.ZoomToStage(0);
						break;
					case 3:
						if (!feedbackLock)
						{
							feedback_combo3.Play();
							Flash(0.5f);
							feedbackLock = true;
						}
						CurrentMusic.EnableLayer(2);
						Cutscenes.ZoomToStage(1);
						break;
					case 6:
						if (!feedbackLock)
						{
							feedback_combo6.Play();
							Flash(0.5f);
							feedbackLock = true;
						}
						CurrentMusic.EnableLayer(3);
						Cutscenes.ZoomToStage(2);
						break;
					case 9:
						if (!feedbackLock)
						{
							feedback_combo9.Play();
							Flash(0.5f);
							feedbackLock = true;
						}
						CurrentMusic.EnableLayer(4);
						isComboBreakerSoundPossible = true;
						Cutscenes.ZoomToStage(3);
						break;
					default:
						feedbackLock = false;
						break;
				}
			}
		}

		protected override void Update(GameTime gameTime)
		{
            old_kbs = kbs;
			kbs = Keyboard.GetState();

            // Allows the game to exit
			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||kbs.IsKeyDown(Keys.Escape))
				this.Exit();

            if (debugMode)
            {
				/*if (kbs.IsKeyDown(Keys.Left))
					GameCamera.Transform.PosX -= 1f;
					//Cutscenes.crowd.Transform.PosX -= 1.0f;
                if (kbs.IsKeyDown(Keys.Right))
					//Cutscenes.crowd.Transform.PosX += 1.0f;
					GameCamera.Transform.PosX += 1f;
                if (kbs.IsKeyDown(Keys.Up))
                    GameCamera.Transform.PosY -= 1f;
                if (kbs.IsKeyDown(Keys.Down))
                    GameCamera.Transform.PosY += 1;*/
                if (kbs.IsKeyDown(Keys.RightShift))
                    GameCamera.Transform.ScaleUniform = GameCamera.Transform.SclX * 1.01f;
                if (kbs.IsKeyDown(Keys.RightControl))
                    GameCamera.Transform.ScaleUniform = GameCamera.Transform.SclX * 0.99f;
            }

            startingCountdown.Update();

            if (menuScreen.IsActive)
                menuScreen.Update();

            pauseScreen.Update();
            if (pauseScreen.IsGamePaused) return;
           
            // Update game if unpaused
            //
            player.Update();
            CurrentTotem.Update();
            scoreBorder.Update();
            mapBorder.Update();
            comboCounter.Update();

            p.Update();

            GameCamera.Update();
			//if (GameCamera.Transform.PosY > -CameraOffset)
			//    GameCamera.Transform.PosY = -CameraOffset;

			Souls.Update();
			Cutscenes.Update();
            Explosions.Update();

            base.Update(gameTime);
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{

			m_drawer.SetRenderTarget();
			GraphicsDevice.Clear(m_bgColor);

            // Drawing combo score behind all sprites
            //

            SpriteBatch.Begin(SpriteSortMode.Immediate, null, SamplerState.PointClamp, null, null);
            comboCounter.Draw();
            SpriteBatch.End();

            // Begin all drawing methods
            //
			SpriteBatch.Begin(SpriteSortMode.Immediate, null, SamplerState.PointClamp, null, null, null, GameCamera.CameraMatrix);
            m_falaise.Draw();
			Cutscenes.crowd.DrawBack();
			for (int i = 0; i < totems.Length; ++i)
				totems[i].Draw();
			Cutscenes.cutscenePlayer.Draw();
			Cutscenes.crowd.DrawFront();

            player.Draw();

            p.Draw();
			
			Souls.Draw();
            Explosions.Draw();
			SpriteBatch.End();

            // Drawing GUI
            //
            SpriteBatch.Begin();
            startingCountdown.Draw();
            scoreBorder.Draw();
            mapBorder.Draw();
            SpriteBatch.End();

            // Drawing Menu
            //
            SpriteBatch.Begin(SpriteSortMode.Immediate, null, SamplerState.PointClamp, null, null);
            menuScreen.Draw();
            SpriteBatch.End();

            SpriteBatch.Begin(SpriteSortMode.Immediate, null, SamplerState.PointClamp, null, null);
            pauseScreen.Draw();
            SpriteBatch.End();

			// End drawing
            //
            m_drawer.Draw();

            if (startingCountdown.CountdownHasFinished)
            {
                SpriteBatch.Begin();
                SpriteBatch.DrawString(menuText, "Souls : " + scoreBorder.Score, new Vector2(70, 0), Color.Black);
                SpriteBatch.End();
            }

            if (debugMode)
            {
                SpriteBatch.Begin();
                // Debug text
                //
                //SpriteBatch.DrawString(debugText, "Souls : " + scoreBorder.Score + "/" + scoreBorder.ScoreBarMaxValue, new Vector2(0, 300), Color.Red);
                //SpriteBatch.DrawString(debugText, "Chall : " + menuScreen.challengeChoice, new Vector2(0, 320), Color.Red);
                //SpriteBatch.DrawString(menuText, "Test", new Vector2(20, 350), Color.Black);
                SpriteBatch.DrawString(debugText, "player posY : " +player.Transform.PosY , new Vector2(0, 300), Color.Red);
                //SpriteBatch.DrawString(debugText, "isCBSP : " + isComboBreakerSoundPossible, new Vector2(0, 320), Color.Red);
                SpriteBatch.End();
            }

			base.Draw(gameTime);
		}
	}
}
