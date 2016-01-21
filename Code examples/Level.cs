using System;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FarseerPhysics.Dynamics;
using Space_challengeMG.Player;

namespace Space_challengeMG
{
    /// <summary>
    /// Teams: 0 - always nature
    ///        1 - player
    ///        2... - others (enemies)
    /// </summary>
    
    
    /// GamePlayState
    ///  Created - the level is just created
    ///  Initialised - the level has loaded the most basic files
    ///  Loading - loading in progress
    ///  TTC - Waiting for Touch To Contionue
    ///  Playing - The game is being played
    ///  Paused - PauseMenu is commanding now
    ///  Won - Player has reached the PlaceFinish
    ///  Died - Player destroyed - no way to revive yet

    public enum GamePlayState {Created, Initialised , Loading, TTC, Playing, Paused, Won, Died}
    public class Level
    {
        #region Camera
        public Camera Camera;
        public Vector2 ActualViewMove
        {
            get
            {
                return Camera.ActualViewMove;
            }
        }
        public float Zoom
        {
            get
            {
                return Camera.Zoomer.FinalZoom;
            }
        } 
        #endregion

        #region SpaceSize
        private Vector2 spaceSize;
        public Vector2 SpaceSize
        {
            get
            {
                return spaceSize;
            }
            set
            {
                spaceSize = value;
                SpaceSizeDiagonal = value.Length();
            }
        }
        public float SpaceSizeDiagonal
        {
            get;
            private set;
        } 
        #endregion

        #region General basics
        public Table Table;
        public GameWorld GameWorld;
        public ContentManager Content;
        public int LevelNumber;
        public string Path;
        public GamePlayState GamePlayState = GamePlayState.Created;
        public LevelLoader Loader; 
        public bool TouchPanelAllowed = true;
        public bool IsEnded { get; private set; }
        #endregion

        public SCShipControls SCShipControls;

        #region GameSpeed
        public float GameSpeed = 1f;
        float gameSpeed = 1;
        public float GameSpeedToGo
        {
            get
            {
                return gameSpeed;
            }
            set
            {
                gameSpeed = value;
            }
        }
        public float GameSpeedSpeed = 0.001f; //game speed change speed...
        public void ChangeGameSpeedImmidietely(float value)
        {
            GameSpeed = GameSpeedToGo = value;
        } 
        #endregion

        #region Bars
        public BarView BarView;
        #endregion

        #region Darkness
        //Back darking when a menu is on
        public MyTexture2D DarkTexture;
        public float DarkOpacity = 0;
        public float DarkOpacityToGo = 0;
        public float DarkOpacitySpeed = 0.0004f;
        public float DarkPauseOpacity = 0.6f;
        #endregion

        #region Game elements
        public World world;
        public Map Map;
        public SCShip SCShip;
        public PlaceFinish PlaceFinish;
        public Borders Borders;
        #endregion

        public Level(Vector2 spaceSize, int levelNumber, string path, GameWorld gameWorld, Table table)
        {
            this.IsEnded = false;
            this.DarkOpacity = 1; 
            this.Content = table.Content;

            this.SpaceSize = spaceSize;
            this.LevelNumber = levelNumber;
            this.Path = path;
            this.GameWorld = gameWorld;

            this.Table = table;
        }

        #region Loading
        public bool IsLoaded()
        {
            if (GamePlayState == Space_challengeMG.GamePlayState.Won || GamePlayState == Space_challengeMG.GamePlayState.Died || 
                GamePlayState == Space_challengeMG.GamePlayState.Paused || GamePlayState == Space_challengeMG.GamePlayState.Playing)
            {
                return true;
            }

            return false;
        }

        #region Load process parts
        // Creates basic elements in level (such as world, joystics, map) - action before loading
        #region Initialising
        

        public void InitialiseLevel()
        {
            if (GamePlayState == GamePlayState.Created)
            {
                #region World
                world = new World(Vector2.Zero);

                #region BodyCounting
                //world.BodyAdded += (body) =>
                //{
                //    Game1.Bodies++;
                //};
                //world.BodyRemoved += (body) => { Game1.Bodies--; }; 
                #endregion
                #endregion

                Map = new Map(this);

                // Darkness texture (for pausing)
                DarkTexture = TextureShapes.Box32;

                Borders = new Borders(this);

                GamePlayState = GamePlayState.Loading;

                createLevelLoader();

                SoundEff.StopMusic();
                SoundEff.ResumeSounds();
            }
        }
        #endregion

        // action after loading
        public void FinishLoad(GraphicsDevice GraphicsDevice)
        {
            world.Step(0.1f);

            GC.Collect();
            GC.WaitForPendingFinalizers();

            BonusHeal bonusHeal = new BonusHeal(this, Vector2.Zero, 100);
            Map.AllBonuses.Add(bonusHeal);

            GamePlayState = GamePlayState.TTC;
        }

        public void LoadLevel()
        {
            if (GamePlayState == GamePlayState.Created)
            {
                this.InitialiseLevel();
            }

            if (Loader.FinishLoaded == false)
            {
                Loader.LoadStep();

                if (Loader.CoreLoaded && !Loader.FinishLoaded)
                {
                    FinishLoad(this.Table.GraphicsDevice);
                }
            }
        }

        private void createLevelLoader()
        {
            if (Loader == null)
            {
                Loader = new LevelLoader(this, Path);
            }
        }
        #endregion

        #region Element loading
        public void LoadingSCShip(SCShip sCShip)
        {
            SCShip = sCShip;
            SCShipControls = getControls();
            BarView = new BarView(Table, new Vector2(240, 50), sCShip);
            Camera = new Camera(this, SCShip.Position);
        }

        protected SCShipControls getControls()
        {
            switch (Options.ControlsType)
            {
                case ControlsType.KeyboardAndMouse:
                    {
                        return new TouchAndKeyboardControls(this);
                    }
                case ControlsType.StaticJoysticAndJoystic:
                    {
                        return new JoysticsControls(this);
                    }
                default:
                    {
                        throw new NotImplementedException("This ControlsType is not implemented");
                    }
            }
        }

        public void LoadingPlaceFinish(PlaceFinish placeFinish)
        {
            this.PlaceFinish = placeFinish;

            Color colorWorld;
            if (this.GameWorld.Levels.IndexOf(this) == this.GameWorld.Levels.Count - 1) //toto je poslední level
            {
                int worldIndex = GameInfo.GameWorlds.IndexOf(this.GameWorld);
                if (worldIndex != GameInfo.GameWorlds.Count - 1) // toto není poslední world
                {
                    colorWorld = GameInfo.GameWorlds[worldIndex + 1].Color;
                }
                else
                {
                    colorWorld = Color.Gold;
                }
            }
            else
            {
                colorWorld = GameWorld.Color;
            }

            placeFinish.BotLightColor = colorWorld;
        } 
        #endregion
        #endregion

        public void Update(GameTime constGameTime, MyState myState, 
            ContentManager Content, GraphicsDevice graphics)
        {
            TimeSpan localtime = TimeSpan.FromMilliseconds(constGameTime.ElapsedGameTime.TotalMilliseconds * GameSpeed);
            GameTime gameTime= new GameTime(constGameTime.TotalGameTime, localtime);

            if (!TouchPanelAllowed)
            {
                myState = new MyState(Table);
            }

            #region Loaded
            if (IsLoaded()) //Update hry
            {
                DarkOpacity = Vypocty.UpravitLinearneHodnotu(DarkOpacity, DarkOpacitySpeed/200 + Math.Abs(1.1f - DarkOpacity)/800, DarkOpacityToGo, constGameTime);
                GameSpeed = Vypocty.UpravitLinearneHodnotu(GameSpeed, GameSpeedSpeed / 10 + Math.Abs(GameSpeed - GameSpeedToGo) / 700, GameSpeedToGo, constGameTime);

                world.Step((float)gameTime.ElapsedGameTime.TotalSeconds);

                #region SCShipControls
                SCShipControls.Update(gameTime, myState);

                #region SCShip controlling
                if (SCShip != null)
                {
                    #region Joy movement
                    if (SCShipControls.IsMovementActive())
                    {
                        SCShip.Drive.SpeedAngleToGo = SCShipControls.GetMovementAngle();
                        SCShip.Drive.ActualPartSpeedMax = SCShipControls.GetMovementRatio();
                        SCShip.Drive.EditSpeedAngle = true;
                    }
                    else
                    {
                        SCShip.Drive.SpeedAngleToGo = SCShip.Body.Rotation;
                        SCShip.Drive.ActualPartSpeedMax = 0;
                        SCShip.Drive.EditSpeedAngle = false;
                    }
                    #endregion

                    #region Heading
                    if (SCShipControls.IsHeadingActive())
                    {
                        SCShip.Cannon.Angle = SCShipControls.GetHeadingAngle();
                        SCShip.Shooting = true;
                    }
                    else
                    {
                        SCShip.Cannon.Angle = SCShip.Rotation;
                        SCShip.Shooting = false;
                    }
                    #endregion
                }  
                #endregion
                #endregion

                #region Game elements
                Map.Update(gameTime);
                #endregion

                #region Bar
                if(BarView != null)
                BarView.Update(gameTime);
                #endregion

                #region AutoZoom and view edditation
                if (SCShip != null)
                {
                    Camera.TargetViewMove.FocusedPointToGo = -SCShip.Position;
                    Camera.Update(gameTime, myState);
                }
                #endregion

                #region Death menu
                if (GamePlayState == Space_challengeMG.GamePlayState.Died)
                {
                    if (Table.ActiveMenu == null || Table.ActiveMenu.Type != MenuType.Death)
                    {
                        DarkOpacityToGo = DarkPauseOpacity;
                        TouchPanelAllowed = false;

                        Table.SetMenu(MenuType.Death);
                    }
                }
                #endregion

                //Game1.TextToShow.Content = "";
            }
            #endregion

            #region Loading
            else
            {
                if (!Loader.FinishLoaded)
                {
                    LoadLevel();
                }
                else
                {
                    Loader.Update(gameTime, myState);

                    if (!Loader.TTCScreen.WaitingForTouch)
                    {
                        GamePlayState = Space_challengeMG.GamePlayState.Playing;
                    }
                }
            } 
            #endregion
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            #region Game
            if (this.IsLoaded())
            {
                #region Game elements
                Map.Draw(spriteBatch);

                SpriteFont font = Content.Load<SpriteFont>("fnt");

                //spriteBatch.DrawString(font, "Drawed boxes: " + DrawedBoxes.ToString(), new Vector2(20, 230), Color.Red);
                #endregion

                Borders.Draw(spriteBatch);

                #region Body show
                /*foreach (Body body in world.BodyList)
                {
                    Vector2 pos = ConvertUnits.ToDisplayUnits(body.Position) * Zoom + ActualViewMove;
                    if (Vypocty.KolizeCtverecVObrzovce(pos, new Vector2(5), Table.Resolution))
                    {
                        Color color = Color.Red; //nemá userData
                        if (body.UserData is LiveShip)
                        {
                            color = Color.Blue;
                        }
                        else
                        {
                            if (body.UserData != null)
                            {
                                color = Color.Yellow; //cokoliv dalšího
                            }
                        }
                        spriteBatch.Draw(TextureShapes.Box2.Texture, pos, null, Color.DarkBlue * 0.5f, 1, TextureShapes.Box2.Origin,
                            new Vector2(5) * Zoom, SpriteEffects.None, 0f);
                    }
                }*/
                #endregion
                
                if(BarView != null)
                    BarView.Draw(spriteBatch);

                SCShipControls.Draw(spriteBatch);

                #region Darkness
                if (DarkOpacity != 0)
                    spriteBatch.Draw(DarkTexture.Texture, Table.Resolution / 2, null, Color.Black * DarkOpacity,
                0, DarkTexture.Size / 2, Table.Resolution / DarkTexture.Size, SpriteEffects.None, 0);
                #endregion
            }
            #endregion

            #region Loading
            else
            {
                if(Loader.FinishLoaded)
                    Loader.TTCScreen.Draw(spriteBatch);

                Loader.LoadingBar.Draw(spriteBatch);
            }
            #endregion
        }

        #region Game orders
        #region Pause
        public void Pause()
        {
            stopGameStream(false);
            pauseAudio();
            playPauseSound();

            GamePlayState = Space_challengeMG.GamePlayState.Paused;
        }

        public void Continue()
        {
            playGameStream();
            resumeAudio();
            playResumeSound();

            GamePlayState = Space_challengeMG.GamePlayState.Playing;
        } 
        #endregion

        #region Victory
        private void setWinMenu()
        {
            Table.SetMenu(MenuType.Win);
        }

        #region Victory sound
        private void playNormalVictorySound()
        {
            SoundEff.PlayMenuSound(SoundEff.Victory);
        }

        private void playFinalVictorySound()
        {
            SoundEff.PlayMenuSound(SoundEff.FinalVictory);
        }
        #endregion

        public void Won()
        {
            GamePlayState = Space_challengeMG.GamePlayState.Won;

            SCShip.Live.Immortal = true;

            stopAudio();
            stopGameStream();

            setWinMenu();

            #region GameProgress
            bool isGameJustFinished = false;
            int byThisLevelsDone = GameWorld.Levels.IndexOf(this) + 1;
            if (this.GameWorld.LevelsDone <= byThisLevelsDone)
            {
                #region First time finished level
                #region Game finish
                this.GameWorld.LevelsDone = byThisLevelsDone;
                if (!GameInfo.IsGameFinished)
                {
                    #region Game is not finished yet
                    if (this.GameWorld == GameInfo.GameWorlds[GameInfo.GameWorlds.Count - 1] && byThisLevelsDone == this.GameWorld.Levels.Count)
                    {
                        #region JUST FINISHED THE GAME!
                        isGameJustFinished = true;
                        GameInfo.IsGameFinished = true;

                        #endregion
                    }
                    #endregion
                }

                if (isGameJustFinished)
                {
                    Table.MenuWin.ShowMLWonJustTheGame();

                    playFinalVictorySound();
                }
                else
                {
                    playNormalVictorySound();
                }
                #endregion

                GameInfo.SaveLevelsProgress(); 
                #endregion
            }
            else
            {
                #region Level has been already finished
                playNormalVictorySound(); 
                #endregion
            }
            #endregion

            EndLevel();
        } 
        #endregion

        public void Died()
        {
            GamePlayState = GamePlayState.Died;

            stopGameStream();

            EndLevel();

            SoundEff.PlayMenuSound(SoundEff.HugeExplosion);
            SoundEff.PlayMenuSound(SoundEff.Lost);
        }

        public void EndLevel()
        {
            if (!IsEnded)
            {
                saveUsedValues();

                stopAudio();

                GameWorld.RefreshLevel(this);

                IsEnded = true;
            }
        }

        private void saveUsedValues()
        {
            GameInfo.SaveFinance();
            GameInfo.AllScore += SCShip.ScoreInfo;
            GameInfo.SaveAllScore();
        }

        #region Audio
        private void stopAudio()
        {
            SoundEff.StopSounds();
            SoundEff.StopMusic();
        }

        private void pauseAudio()
        {
            SoundEff.PauseSounds();
            SoundEff.PauseMusic();
        }

        private void resumeAudio()
        {
            SoundEff.ContinueMusic();
            SoundEff.ResumeSounds();
        }

        private void playAudio()
        {
            SoundEff.GameSoundInstances.Clear();
            SoundEff.PlayGameMusic();
            SoundEff.ResumeSounds();
        }

        #region PauseSounds
        private void playPauseSound()
        {
            SoundEff.PlayMenuSound(SoundEff.OpenPause);
        }

        private void playResumeSound()
        {
            SoundEff.PlayMenuSound(SoundEff.Resume);
        } 
        #endregion
        #endregion

        #region GameStream
        private void stopGameStream(bool stopSmoothly = true)
        {
            DarkOpacityToGo = DarkPauseOpacity;

            #region GameSpeed
            GameSpeedToGo = 0;

            if (!stopSmoothly)
            {
                GameSpeed = 0;
            }
            #endregion

            TouchPanelAllowed = false;
        }

        private void playGameStream(bool smoothly = true)
        {
            DarkOpacityToGo = 0;

            #region GameSpeed
            GameSpeedToGo = 1;

            if (!smoothly)
            {
                GameSpeed = 1;
            }
            #endregion

            TouchPanelAllowed = true;
        } 
        #endregion
        #endregion
    }
}