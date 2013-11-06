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

        public const int screenZoom = 2;

		public static Random Random = new Random();
		public static ParticleSystem Souls;
		public static Camera2D GameCamera;
		public static KeyboardState kbs = new KeyboardState();
        public static KeyboardState old_kbs = new KeyboardState();
        public static Player player;
        int playerInitialPosition = -50;

        public static PauseScreen pauseScreen;
        public static ScoreBorder scoreBorder;
        public static MapBorder mapBorder;
        public static int normalTotemValue = 100;

        public static SpriteFont debugText;
        bool debugMode = true;

        Sprite floorBackground;
        Transform climbingAltitude;
        int deltaAboveClimbingAltitude;

		Color m_bgColor = new Color(239, 255, 222);
		GameboyDrawer m_drawer;
		public static Totem m_totem;

		public static DrawingList Foreground = new DrawingList();


		public Game1()
            : base(160 * screenZoom, 144 * screenZoom)
		{
			Souls = new ParticleSystem(this, 500);
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

            //SoundEffectLibrary.LoadContent(Content, "SoundEffects");
            m_drawer = new GameboyDrawer(this);
            GameCamera = new Camera2D(new Vector2(GameboyWidth * 0.5f, GameboyHeight * 0.5f));
            GameCamera.ScaleToZoom = true;

			// Totem
			//
			m_totem = new Totem();
			m_totem.AddSections(new SectionData(typeof(NormalSection), 0, 0, 20));
			m_totem.AddSections(new SectionData(typeof(MetalSection), 5, 5, 5));
			m_totem.AddSections(new SectionData(typeof(SpikeSection), 3, 3, 3));
			m_totem.Build();

            // Player initialisation
            //
            deltaAboveClimbingAltitude = -100;
            climbingAltitude = new Transform();
            climbingAltitude.PosY = m_totem.Top + deltaAboveClimbingAltitude;
            player = new Player(new Vector2(playerInitialPosition, 0), climbingAltitude);
			player.Initialise(m_totem);

            // Pause screen & GUI initialisation
            //
            pauseScreen = new PauseScreen();
            scoreBorder = new ScoreBorder(ScreenHeight);
            mapBorder = new MapBorder();

            // Background textures
            //
            floorBackground = new Sprite(this, TextureLibrary.GetSpriteSheet("floor_background"), new Transform());
            floorBackground.Transform.PosY = -20;

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
                    GameCamera.Transform.PosX -= 1f;
                if (kbs.IsKeyDown(Keys.Right))
                    GameCamera.Transform.PosX += 1;
                if (kbs.IsKeyDown(Keys.Up))
                    GameCamera.Transform.PosY -= 1f;
                if (kbs.IsKeyDown(Keys.Down))
                    GameCamera.Transform.PosY += 1;
                if (kbs.IsKeyDown(Keys.RightShift))
                    GameCamera.Transform.ScaleUniform = GameCamera.Transform.SclX * 1.01f;
                if (kbs.IsKeyDown(Keys.RightControl))
                    GameCamera.Transform.ScaleUniform = GameCamera.Transform.SclX * 0.99f;
            }

            pauseScreen.Update();
            if (pauseScreen.IsGamePaused) return;
           
            // Update game if unpaused
            //
            player.Update();
            m_totem.Update();
            scoreBorder.Update();
            mapBorder.Update();

            GameCamera.Update();
            GameCamera.Transform.PosX = player.Transform.PosX;
            GameCamera.Transform.PosY = player.Transform.PosY + CameraOffset;

            if (GameCamera.Transform.PosY > -CameraOffset)
                GameCamera.Transform.PosY = -CameraOffset;

			Souls.Update();

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

            // Begin all drawing methods
            //
			SpriteBatch.Begin(SpriteSortMode.Immediate, null, SamplerState.PointClamp, null, null, null, GameCamera.CameraMatrix);
            floorBackground.Draw();
			
			m_totem.Draw();
            player.Draw();
			Souls.Draw();
			SpriteBatch.End();

            // Drawing GUI
            //
            SpriteBatch.Begin();
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

            

            // TODO
            // Mettre un vrai texte comme spritefont
            //
            if (player.ComboCount > 0)
            {
                SpriteBatch.Begin();
                SpriteBatch.DrawString(debugText, "Combo : " + player.ComboCount, new Vector2(), Color.Red);
                SpriteBatch.End();
            }

            if (debugMode)
            {
                SpriteBatch.Begin();
                // Debug text
                //
                //SpriteBatch.DrawString(debugText, "Score : " + scoreBorder.Score, new Vector2(0, 300), Color.Red);
                //SpriteBatch.DrawString(debugText, "Max : " + m_totem.TotalAmountOfSections*normalTotemValue, new Vector2(0, 320), Color.Red);
                //SpriteBatch.DrawString(debugText, "sclY : " + scoreBorder.m_graphicScore.Transform.SclY, new Vector2(0,340), Color.Red);
                
                SpriteBatch.End();
            }

			base.Draw(gameTime);
		}
	}
}
