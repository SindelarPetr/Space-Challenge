using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Space_challengeMG
{
    public class MenuMain : MenuController
    {
        public MLMain MLMain;

        public MyGridController MyGridController;

        public MenuMain(ContentManager content, GraphicsDevice graphics, Table table)
            : base(content, graphics, MenuType.Main, table)
        {
            #region MenuLists creation
            this.MLMain = new MLMain(content, this, null);
            this.BasicML = this.MLMain;

            this.MLMain.Enable();
            #endregion

            MyGridController = new MyGridController(Table);
        }

        public override void Update(Microsoft.Xna.Framework.GameTime gameTime, MyState myState)
        {
            base.Update(gameTime, myState);

            MyGridController.Update(gameTime, myState);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            MyGridController.Draw(spriteBatch);

            base.Draw(spriteBatch);
        }
    }
}
