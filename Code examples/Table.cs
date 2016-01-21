using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Space_challengeMG
{
    public class Table
    {
        #region Menus
        public MenuController ActiveMenu;

        public List<MenuList> AllMenus = new List<MenuList>();

        public MenuMain MenuMain;
        public MenuPause MenuPause;
        public MenuDeath MenuDeath;
        public MenuWin MenuWin; 
        #endregion
        
        public CoinView CoinView;

        #region Basics
        public Level Level = null;
        public ContentManager Content;
        public GraphicsDevice GraphicsDevice;
        public Game1 Game; 
        #endregion

        #region Display parameters
        public Vector2 Resolution;
        public Vector2 WindowSize;
        public Vector2 WindowRatio; // Resolution / WindowsSize --> ideal is one 
        #endregion

        public Table(Game1 game)
        {
            #region Basics
            this.Content = game.Content;
            this.GraphicsDevice = game.GraphicsDevice;
            this.Game = game; 
            #endregion

            #region Setting display parameters
            Resolution = new Vector2(GraphicsDevice.PresentationParameters.BackBufferWidth, GraphicsDevice.PresentationParameters.BackBufferHeight);
            WindowSize = new Vector2(Game.Window.ClientBounds.Width, Game.Window.ClientBounds.Height);
            WindowRatio = Resolution / WindowSize; 
            #endregion

            #region Static props
            bool firstStart = GameInfo.LoadLevelsProgress();
            GameInfo.GameWorlds = GameWorld.InitGameWorlds(Content, this);

            TextureShapes.InitTextures(Content);
            Fonts.InitFonts(Content);
            SoundEff.Init(Content);
            GameInfo.LoadFinance();
            SCShipSkins.InitSkins(Content);
            GameInfo.LoadShipSkins(Content);
            GameInfo.LoadShipUpgrades();
            GameInfo.LoadAllScore(); 
            #endregion

            #region Menus creation
            this.MenuMain = new MenuMain(Content, GraphicsDevice, this);
            this.MenuPause = new MenuPause(Content, GraphicsDevice, this);
            this.MenuDeath = new MenuDeath(Content, GraphicsDevice, this);
            this.MenuWin = new MenuWin(Content, GraphicsDevice, this);
            #endregion

            #region Start menu
            SoundEff.PlayMenuMusic();
            SetMenu(MenuType.Main);
            #endregion

            #region CoinView
            CoinView = new CoinView(this, new Vector2(Resolution.X - 50, 40));
            GameInfo.Finance.CoinView = CoinView; 
            #endregion
        }

        public void Update(GameTime gameTime, MyState myState)
        {
            #region Menu
            if (ActiveMenu != null)
            {
                ActiveMenu.Update(gameTime, myState);
            } 
            #endregion

            #region Level
            if (Level != null)
            {
                #region Level pause
                if (myState.BackPressed && !myState.GamePadStateHandelt)
                {
                        SetMenu(MenuType.Pause);
                        Level.Pause();
                        myState.GamePadStateHandelt = true;
                    
                }
                #endregion

                Level.Update(gameTime, myState, Content, GraphicsDevice);
            }
            #endregion

            SoundEff.Update(gameTime);

            CoinView.Update(gameTime);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            #region Level
            if (Level != null)
            {
                Level.Draw(spriteBatch);
            } 
            #endregion

            #region ActiveMenu
            if (ActiveMenu != null)
            {
                ActiveMenu.Draw(spriteBatch);
            } 
            #endregion

            CoinView.Draw(spriteBatch);
        }

        #region Menu select
        // Set menu is for changing different types of menu ( MainMenu, PauseMenu, WinMenu,... )
        public void SetMenu(MenuController menu)
        {
            if (ActiveMenu != menu)
                ActiveMenu = menu;

            ActiveMenu.EnableBasicML();
        }

        public void SetMenu(MenuType menuType)
        {
            switch (menuType)
            {
                #region Main
                case MenuType.Main:
                    {
                        setMainMenu();
                    }
                    break;
                #endregion
                #region Pause
                case MenuType.Pause:
                    {
                        setPause();
                    }
                    break;
                #endregion
                #region Death
                case MenuType.Death:
                    {
                        setMenuDeath();
                    }
                    break;
                #endregion
                #region Win
                case MenuType.Win:
                    {
                        setMenuWin();
                    }
                    break; 
                #endregion
            }
        }

        #region Specific menus
        private void setMainMenu()
        {
            if (Level != null)
            {
                Level = null;
            }

            SetMenu(MenuMain);

            if (CoinView != null)
                CoinView.Hide();

            SoundEff.PlayMenuMusic();
        }

        private void setPause()
        {
            Level.Pause();
            SetMenu(MenuPause);
        }

        private void setMenuDeath()
        {
            SetMenu(MenuDeath);
        }

        private void setMenuWin()
        {
            SetMenu(MenuWin);
        }
        #endregion
        #endregion

        #region GameControl
        public void StartGame(Level level)
        {
            if (ActiveMenu != null)
            {
                ActiveMenu = null;
            }

            Level = level;
            Level.LoadLevel();
        }

        public void RestartGame()
        {
            if (Level != null)
            {
                EndCurrentLevel();

                Level newLevel = Level.GameWorld.GetLevelByLevelNumber(Level.LevelNumber);

                StartGame(newLevel);
            }
            else
                throw new NullReferenceException("Current level is null -> cannot restart it");
        }

        private void EndCurrentLevel()
        {
            if (!Level.IsEnded)
            {
                Level.EndLevel();
            }
        }

        public bool NextLevel() //returns if was succesfull
        {
            for (int i = 0; i < Level.GameWorld.Levels.Count; i++)
            {
                Level level = Level.GameWorld.Levels[i];
                if (this.Level.Path == level.Path)
                {
                    if (i < Level.GameWorld.Levels.Count - 1)
                    {
                        StartGame(Level.GameWorld.Levels[i + 1]);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            throw new Exception("GameWorld hasnt been inited yet");
        } 
        #endregion
    }
}