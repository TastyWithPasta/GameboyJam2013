﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PastaGameLibrary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GbJamTotem
{
    public class MenuScreen : GameObject
    {
        //SpriteFont menuText;

        Sprite arrow;

        public enum MenuState
        {
            START,
            CHALLENGES,
            CHALLENGE_CHOICE
        }

        public enum ChallengeState
        {
            CHALL_1,
            CHALL_2,
            CHALL_3,
            CHALL_4,
            CHALL_5,
        }

        MenuState choice;
        public ChallengeState challengeChoice;
		MoveToStaticAction moveTo, moveOut;
		SingleActionManager actionManager = new SingleActionManager();


        bool isActive, isHidden;
        bool canLauchChallenge;
        int deltaArrowBetweenChallenges = 19;

        public bool IsActive
        {
            get { return isActive; }
        }

        public MenuScreen()
        {
            isActive = false;
			isHidden = true;
            canLauchChallenge = false;

            m_sprite = new Sprite(Program.TheGame, TextureLibrary.GetSpriteSheet("menu_start_bg"), m_transform);
            m_sprite.Transform.Position = new Vector2(80, 120);
            arrow = new Sprite(Program.TheGame, TextureLibrary.GetSpriteSheet("arrow"), new Transform(m_transform, true));

			moveTo = new MoveToStaticAction(Program.TheGame, m_transform, new Vector2(80, 120), 1);
			moveTo.StartPosition = new Vector2(80, 200);
			moveTo.Interpolator = new PSmoothstepInterpolation();
			moveTo.Timer.Interval = 0.5f;

			moveOut = new MoveToStaticAction(Program.TheGame, m_transform, new Vector2(80, 200), 1);
			moveOut.StartPosition = new Vector2(80, 120);
			moveOut.Interpolator = new PSmoothstepInterpolation();
			moveOut.Timer.Interval = 0.5f;

            choice = MenuState.START;
            challengeChoice = ChallengeState.CHALL_1;
        }

        public override void Update()
        {
            if (isActive)
            {
                // Positionning arrow
                //
                if (choice == MenuState.START)
                    arrow.Transform.Position = new Vector2(-25, -7);

                if (choice == MenuState.CHALLENGES)
                    arrow.Transform.Position = new Vector2(-45, 7);

                // Actions with enter or space
                //
                if ( (Game1.kbs.IsKeyDown(Keys.Enter) && Game1.old_kbs.IsKeyUp(Keys.Enter)) 
                    || (Game1.kbs.IsKeyDown(Keys.Space) && Game1.old_kbs.IsKeyUp(Keys.Space)) )
                {
                    ExecuteStartAction();
                }

                // Up and down input between start & challenges
                //
                if (choice != MenuState.CHALLENGE_CHOICE)
                {
                    if (Game1.kbs.IsKeyDown(Keys.Up) && Game1.old_kbs.IsKeyUp(Keys.Up))
                    {
                        choice = MenuState.START;
                    }

                    if (Game1.kbs.IsKeyDown(Keys.Down) && Game1.old_kbs.IsKeyUp(Keys.Down))
                    {
                        choice = MenuState.CHALLENGES;
                    }
                }

                // Left and right input between challenges
                //
                if (choice == MenuState.CHALLENGE_CHOICE)
                {

                    if (Game1.kbs.IsKeyDown(Keys.LeftAlt) && Game1.old_kbs.IsKeyUp(Keys.LeftAlt))
                    {
                        choice = MenuState.START;
                        challengeChoice = ChallengeState.CHALL_1;
                        m_sprite.SpriteSheet = TextureLibrary.GetSpriteSheet("menu_start_bg");
                        canLauchChallenge = false;
                    }

                    if (Game1.kbs.IsKeyDown(Keys.Right) && Game1.old_kbs.IsKeyUp(Keys.Right))
                    {
                        if (challengeChoice < ChallengeState.CHALL_5)
                        {
                            challengeChoice++;
                            arrow.Transform.PosX += deltaArrowBetweenChallenges;
                        }
                    }

                    if (Game1.kbs.IsKeyDown(Keys.Left) && Game1.old_kbs.IsKeyUp(Keys.Left))
                    {
                        if (challengeChoice > ChallengeState.CHALL_1)
                        {
                            challengeChoice--;
                            arrow.Transform.PosX -= deltaArrowBetweenChallenges;
                        }
                    }
                }

            }
			actionManager.Update();

        }

        public void ExecuteStartAction()
        {
            // Start challenges
            //
            if (choice == MenuState.CHALLENGE_CHOICE && canLauchChallenge)
            {
                    HideMenu();
					Program.TheGame.LoadLevel((int)challengeChoice);
                    //Game1.totem.BuildFromFile("Level1_p1");
                    //Game1.scoreBorder.ScoreBarMaxValue = Game1.scoreBorder.calculateScoreMax();
                    //Game1.mapBorder.setTopTotem();
                    canLauchChallenge = false;
					Game1.SetupNextLevel();
					Cutscenes.GoToTotem(Game1.CurrentTotem, 1.0f, -40);
					Cutscenes.title.Dissappear();
					//Cutscenes.GoToTotem(Game1.CurrentTotem, 3.0f);
            }

            // Start random game
            //
            if (choice == MenuState.START)
            {
				//HideMenu();
				//Game1.CurrentTotem.AddSections(new SectionData(typeof(NormalSection), 0, 0, 30));
				//Game1.CurrentTotem.AddSections(new SectionData(typeof(MetalSection), 10, 10, 7));
				//Game1.CurrentTotemt.AddSections(new SectionData(typeof(SpikeSection), 4, 4, 7));
				//Game1.CurrentTotem.BuildRandom();

				//// Set informations for slides
				////
				//Game1.scoreBorder.ScoreBarMaxValue = Game1.scoreBorder.calculateScoreMax();
				//Game1.mapBorder.setTopTotem();

				//Cutscenes.GoToTotem(Game1.totem);
            }

            if (choice == MenuState.CHALLENGES)
            {
                choice = MenuState.CHALLENGE_CHOICE;
                m_sprite.SpriteSheet = TextureLibrary.GetSpriteSheet("menu_chall_bg");
                arrow.Transform.Position = new Vector2(-47, 7);
                canLauchChallenge = true;
            }

           

        }

        public void ShowMenu()
        {
            choice = MenuState.START;
			actionManager.StartNew(moveTo);
            isActive = true;
			isHidden = false;
        }

        public void HideMenu()
        {
            m_sprite.Transform.Scale = new Vector2(0);
			actionManager.StartNew(moveOut);
            isActive = false;
			isHidden = true;
        }

        public override void Draw()
        {
			if (isHidden)
			return;

			m_sprite.Draw();
			arrow.Draw();
			

            //Program.TheGame.SpriteBatch.Begin();

            //Program.TheGame.SpriteBatch.DrawString(menuText, "Test", new Vector2((int)50, (int)50), Color.Black);

            //Program.TheGame.SpriteBatch.End()

        }

    }
}
