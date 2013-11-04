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

		public static Random Random = new Random();
		public static ParticleSystem Particles;
		public static Camera2D GameCamera;
		public static KeyboardState kbs = new KeyboardState();
        public static KeyboardState old_kbs = new KeyboardState();
        public static Player player;

        SpriteFont debugText;
        bool debugMode = true;

        Sprite floorBackground;
        Transform climbingAltitude;
        int deltaAboveClimbingAltitude;

		Color m_bgColor = new Color(239, 255, 222);
		GameboyDrawer m_drawer;
		Totem m_totem;

		public static DrawingList Foreground = new DrawingList();


		public Game1()
			: base(160, 144)
		{
			Particles = new ParticleSystem(this, 100);
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
			m_totem.Build();

            // Player initialisation
            //
            deltaAboveClimbingAltitude = -100;
            climbingAltitude = new Transform();
            climbingAltitude.PosY = m_totem.Top + deltaAboveClimbingAltitude;
            player = new Player(new Vector2(-50, 0), climbingAltitude);

			player.Initialise(m_totem);

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
			// Allows the game to exit
			kbs = Keyboard.GetState();
			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
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

            if (kbs.IsKeyDown(Keys.Escape))
                this.Exit();

            player.Update();
			m_totem.Update();

			GameCamera.Update();
            GameCamera.Transform.PosX = player.Transform.PosX;
			GameCamera.Transform.PosY = player.Transform.PosY + CameraOffset;

			if (GameCamera.Transform.PosY > -CameraOffset)
				GameCamera.Transform.PosY = -CameraOffset;


			Particles.Update();
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

            // Begin Drawing
            //
			SpriteBatch.Begin(SpriteSortMode.Immediate, null, SamplerState.PointClamp, null, null, null, GameCamera.CameraMatrix);
            floorBackground.Draw();
			Particles.Draw();
			m_totem.Draw();
            player.Draw();

			SpriteBatch.End();

			// End drawing
            //
			m_drawer.Draw();

            if (debugMode)
            {
                SpriteBatch.Begin();
                // Debug text
                //SpriteBatch.DrawString(debugText, "LR Status : " + player.m_slashLR.IsActive, new Vector2(), Color.Black);
                //SpriteBatch.DrawString(debugText, "RL Status : " + player.m_slashRL.IsActive, new Vector2(0,20), Color.Black);
                SpriteBatch.End();
            }

			base.Draw(gameTime);
		}
	}
}
