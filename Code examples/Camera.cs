using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Space_challengeMG
{
    // Final - just counts all types of ViewMove
    // Static - doesnt changes its value
    // Variable - changes its value according to its rules
    public class Camera
    {
        #region Camera
        public bool CameraLocked = false;

        #region ViewMove
        #region Basic viewMove
        public Vector2 ActualViewMove = new Vector2(400, 240); //Final viewMove
        public Vector2 OriginViewMove = new Vector2(400, 240); //Static viewMove
        #endregion

        public ShakeScreen ShakeScreen = new ShakeScreen();

        public FocusViewMove TargetViewMove; // for focusing the target (SCShip)

        public DragViewMove DragViewMove = new DragViewMove();

        public ToSeeViewMove ToSeeViewMove; 
        #endregion

        public Zoomer Zoomer;

        #region AllMapView
        bool allMapView = false;
        public bool AllMapView
        {
            get
            {
                return allMapView;
            }

            set
            {
                if (value != allMapView)
                {
                    allMapView = value;
                }
            }
        }

        private float getZoomToSeeAll()
        {
            float xZoom = (Table.Resolution.X / (SpaceSize.X + 32));
            float yZoom = (Table.Resolution.Y / (SpaceSize.Y + 32));

            if (xZoom < yZoom)
                return xZoom;
            else
                return yZoom;
        }
        #endregion
        #endregion

        public Vector2 ViewMoveToGo = Vector2.Zero;

        public Level Level;
        public Table Table;
        public Vector2 SpaceSize;

        public bool EaseViewMoveOnBorder = false;

        public Camera(Level level, Vector2 focusedPosition)
        {
            this.Level = level;
            this.Table = level.Table;
            this.SpaceSize = level.SpaceSize;

            this.TargetViewMove = new FocusViewMove(level);
            this.TargetViewMove.FocusedPointToGo = this.TargetViewMove.FocusedPoint = focusedPosition;

            this.ToSeeViewMove = new ToSeeViewMove(level.SCShip, level.SCShipControls);

            this.Zoomer = new Zoomer(level);
        }

        public void Update(GameTime gameTime, MyState myState)
        {
            #region Counting parts of ViewMove
            TargetViewMove.Update(gameTime);

            DragViewMove.Update(gameTime, myState);

            ShakeScreen.Update(gameTime);

            ToSeeViewMove.Update(gameTime);
            #endregion

            this.Zoomer.Update(gameTime, myState);

            #region Counting final ViewMove
            ActualViewMove = TargetViewMove.FocusedPoint * Zoomer.FinalZoom + OriginViewMove + DragViewMove.ViewMove + ShakeScreen.ShakeViewMove * Zoomer.FinalZoom
                    + ToSeeViewMove.ViewMove * Zoomer.FinalZoom;

            EaseViewMove(); 
            #endregion
        }

        public void ApplyShakeForce(Vector2 force)
        {
            ShakeScreen.ApplyShakeForce(force);
        }

        public void EaseViewMove()
        {
            #region View move on sides
            if (EaseViewMoveOnBorder)
            {
                float sideConst = 1.25f;
                #region Leva
                if (-ActualViewMove.X < (-SpaceSize.X / 2) * Zoomer.FinalZoom)//leva
                {
                    ActualViewMove.X += (-ActualViewMove.X + (SpaceSize.X / 2) * Zoomer.FinalZoom) / sideConst;
                }
                #endregion
                #region Prava
                if (-ActualViewMove.X + Table.Resolution.X > (SpaceSize.X / 2) * Zoomer.FinalZoom)//prava
                {
                    ActualViewMove.X += (-ActualViewMove.X + ((-SpaceSize.X / 2) * Zoomer.FinalZoom + Table.Resolution.X)) / sideConst;
                }
                #endregion
                #region Horni
                if (-ActualViewMove.Y < (-SpaceSize.Y / 2) * Zoomer.FinalZoom)//horni
                {
                    ActualViewMove.Y += (-ActualViewMove.Y + (SpaceSize.Y / 2) * Zoomer.FinalZoom) / sideConst;
                }
                #endregion
                #region Dolni
                if (-ActualViewMove.Y + Table.Resolution.Y > (SpaceSize.Y / 2) * Zoomer.FinalZoom)//prava
                {
                    ActualViewMove.Y += (-ActualViewMove.Y + ((-SpaceSize.Y / 2) * Zoomer.FinalZoom + Table.Resolution.Y)) / sideConst;
                }

                #region Hard postionning
                /*if (-ActualViewMove.Y + 480 > ((SpaceSize.Y / 2 + 30) * Zoom))//dolni
                        {
                            ActualViewMove.Y = (-SpaceSize.Y / 2 - 30) * Zoom + 480;
                        }*/
                #endregion
                #endregion
            }
            #endregion
        }
    }
}
