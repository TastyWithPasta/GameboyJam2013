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
        int playerInitialPosition = -50;

        public static PauseScreen pauseScreen;
        public static Countdown startingCountdown;
        public static ScoreBorder scoreBorder;
        public static MapBorder mapBorder;
        public static ComboCounter comboCounter;
        public static int normalTotemValue = 100;

		public static bool isInGameplay = false;

        public static SpriteFont debugText;
        bool debugMode = true;

        Sprite floorBackground;
        Transform climbingAltitude;

		//Color m_bgColor = new Color(239, 255, 222);
        Color m_bgColor = new Color(166, 202, 240);
		GameboyDrawer m_drawer;
		public static Totem totem;

		public static DrawingList Foreground = new DrawingList();

        float scaleCombo = 0;

        #region SOUND ATTRIBUTES

        public static SoundEffectInstance normalTotemCollisionSound_Channel1;
        public static SoundEffectInstance normalTotemCollisionSound_Channel2;

        public static SoundEffectInstance metalTotemCollisionSound_Channel1;
        public static SoundEffectInstance metalTotemCollisionSound_Channel2;

        public static SoundEffectInstance swordSlashSound;
        public static SoundEffectInstance moveLeftToRightSound;
        public static SoundEffectInstance moveRightToLeftSound;

        public static SoundEffectInstance musicT1P1;
        public static SoundEffectInstance musicT1P2;

        public static SoundEffectInstance musicT1P3L1;
        public static SoundEffectInstance musicT1P3L2;
        public static SoundEffectInstance musicT1P3L3;
        public static SoundEffectInstance musicT1P3L4;

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

            SoundEffectLibrary.LoadContent(Content, "SoundEffects");

            m_drawer = new GameboyDrawer(this);
            GameCamera = new Camera2D(new Vector2(GameboyWidth * 0.5f, GameboyHeight * 0.5f));
            GameCamera.ScaleToZoom = true;

			// Totem
			//
			totem = new Totem();
			totem.AddSections(new SectionData(typeof(NormalSection), 0, 0, 30));
			totem.AddSections(new SectionData(typeof(MetalSection), 5, 5, 7));
			totem.AddSections(new SectionData(typeof(SpikeSection), 3, 3, 7));
			totem.Build();

            // Player initialisation
            //
            climbingAltitude = new Transform();
            climbingAltitude.PosY = totem.Top;
            player = new Player(new Vector2(playerInitialPosition, 0), climbingAltitude);
			player.Initialise(totem);

            // Pause screen & GUI initialisation
            //
            pauseScreen = new PauseScreen();
            startingCountdown = new Countdown();
            scoreBorder = new ScoreBorder();
            mapBorder = new MapBorder();
            comboCounter = new ComboCounter(player);

            // Background textures
            //
            floorBackground = new Sprite(this, TextureLibrary.GetSpriteSheet("floor_background"), new Transform());
            floorBackground.Transform.PosY = -20;

            // Sound & musics
            //
            normalTotemCollisionSound_Channel1 = SoundEffectLibrary.Get("normal_collision_sound").CreateInstance();
            normalTotemCollisionSound_Channel2 = SoundEffectLibrary.Get("normal_collision_sound").CreateInstance();
            metalTotemCollisionSound_Channel1 = SoundEffectLibrary.Get("metal_collision_sound").CreateInstance();
            metalTotemCollisionSound_Channel2 = SoundEffectLibrary.Get("metal_collision_sound").CreateInstance();
            swordSlashSound = SoundEffectLibrary.Get("sword_slash").CreateInstance();
            moveLeftToRightSound = SoundEffectLibrary.Get("move_left_to_right").CreateInstance();
            moveRightToLeftSound = SoundEffectLibrary.Get("move_right_to_left").CreateInstance();

            musicT1P1 = SoundEffectLibrary.Get("music_T1P1").CreateInstance();
            musicT1P2 = SoundEffectLibrary.Get("music_T1P2").CreateInstance();

            musicT1P3L1 = SoundEffectLibrary.Get("music_T1P3L1").CreateInstance();
            musicT1P3L2 = SoundEffectLibrary.Get("music_T1P3L2").CreateInstance();
            musicT1P3L3 = SoundEffectLibrary.Get("music_T1P3L3").CreateInstance();
            musicT1P3L4 = SoundEffectLibrary.Get("music_T1P3L4").CreateInstance();


            musicT1P3L1.IsLooped = true;
            musicT1P3L2.IsLooped = true;
            musicT1P3L3.IsLooped = true;
            musicT1P3L4.IsLooped = true;

			//Foule et joueur port�
			Cutscenes.Initalise();
			Cutscenes.StartMainMenu();
        }

		/// <summary>
		/// UnloadContent will be called once per game and is the place to unload
		/// all content.
		/// </summary>
		protected override void UnloadContent()
		{
			// TODO: Unload any non ContentManager content here
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
				if (kbs.IsKeyDown(Keys.Left))
					Cutscenes.crowd.Transform.PosX -= 1.0f;
                if (kbs.IsKeyDown(Keys.Right))
					Cutscenes.crowd.Transform.PosX += 1.0f;
                if (kbs.IsKeyDown(Keys.Up))
                    GameCamera.Transform.PosY -= 1f;
                if (kbs.IsKeyDown(Keys.Down))
                    GameCamera.Transform.PosY += 1;
                if (kbs.IsKeyDown(Keys.RightShift))
                    GameCamera.Transform.ScaleUniform = GameCamera.Transform.SclX * 1.01f;
                if (kbs.IsKeyDown(Keys.RightControl))
                    GameCamera.Transform.ScaleUniform = GameCamera.Transform.SclX * 0.99f;
            }
			if (kbs.IsKeyDown(Keys.S) && old_kbs.IsKeyUp(Keys.S))
			{
				Cutscenes.GoToTotem(totem);
			}

            startingCountdown.Update();

            pauseScreen.Update();
            if (pauseScreen.IsGamePaused) return;
           
            // Playing music
            //

            // Starting playing music
            //
            if (startingCountdown.CountdownHasFinished && musicT1P3L1.State != SoundState.Playing)
            {
                musicT1P3L1.Play();
                musicT1P3L2.Play();
                musicT1P3L3.Play();
                musicT1P3L4.Play();

                musicT1P3L2.Volume = 0f;
                musicT1P3L3.Volume = 0f;
                musicT1P3L4.Volume = 0f;
            }

            if (player.ComboCount == 3)
                musicT1P3L2.Volume = 1f;

            if(player.ComboCount == 6)
                musicT1P3L3.Volume = 1f;

            if (player.ComboCount == 9)
                musicT1P3L4.Volume = 1f;

            if (player.ComboCount == 0)
            {
                musicT1P3L2.Volume = 0f;
                musicT1P3L3.Volume = 0f;
                musicT1P3L4.Volume = 0f;
            }

            // Update game if unpaused
            //
            player.Update();
            totem.Update();
            scoreBorder.Update();
            mapBorder.Update();
            comboCounter.Update();

			if (isInGameplay)
			{
				GameCamera.Transform.PosX = player.Transform.PosX;//player.SpriteTransform.PosX;
				GameCamera.Transform.PosY = player.Transform.PosY + CameraOffset;
			}

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

            #region Combo Multiplier with SpriteFont
            // TODO
            // Mettre un vrai texte comme spritefont
            //
            /*
            if (player.ComboCount > 1 && player.IsFalling)
            {
                Vector2 posCombo;

                if (old_kbs.IsKeyDown(Keys.Space) && kbs.IsKeyDown(Keys.Space))
                {
                    scaleCombo = 0;
                }

                if (player.IsToLeft)
                {
                    posCombo = new Vector2(100, 72);
                }
                else
                {
                    posCombo = new Vector2(20, 72);
                }


                SpriteBatch.Begin();
                SpriteBatch.DrawString(debugText, "x " + player.ComboCount, posCombo, Color.White, 0, new Vector2(0, (float)(debugText.MeasureString("9").Y / 2)), scaleCombo, SpriteEffects.None, 0);
                //SpriteBatch.DrawString(debugText, "Combo : " + player.ComboCount, new Vector2(), Color.Red);
                SpriteBatch.End();

                if (scaleCombo < 1)
                    scaleCombo += 0.1f;
            }
             * */
            #endregion



            // Begin all drawing methods
            //
			SpriteBatch.Begin(SpriteSortMode.Immediate, null, SamplerState.PointClamp, null, null, null, GameCamera.CameraMatrix);
            floorBackground.Draw();
			Cutscenes.crowd.DrawBack();
			totem.Draw();
			Cutscenes.cutscenePlayer.Draw();
			Cutscenes.crowd.DrawFront();

            player.Draw();
			
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

			// End drawing
            //
            m_drawer.Draw();            
            
            // Drawing separately pause screen
            // (Has spriteBatch inside)
            //
            pauseScreen.Draw();
            

            if (debugMode)
            {
                SpriteBatch.Begin();
                // Debug text
                //
                SpriteBatch.DrawString(debugText, "Soul : " + scoreBorder.Score + "/"+ scoreBorder.ScoreBarMaxValue , new Vector2(0, 300), Color.Red);
                
                SpriteBatch.End();
            }

			base.Draw(gameTime);
		}
	}
}
