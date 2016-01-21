using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Space_challengeMG.Menu.Languages;

namespace Space_challengeMG
{
    public class MLMain : MenuList
    {
        public MLPlay MLPlay;
        public MLOptions MLOptions;
        public MLAbout MLAbout;

        public MLMain(ContentManager content, MenuController menu, MenuList parent)
            : base(content, menu, menu.Table.Resolution / 2f, parent)
        {
            #region MenuLists
            #region MenuLists creation
            this.MLPlay = new MLPlay(content, menu, this);
            this.MLOptions = new MLOptions(content, menu, this);
            this.MLAbout = new MLAbout(content, menu, this);
            #endregion

            #region MenuLists addition
            this.Kids.Add(MLPlay);
            this.Kids.Add(MLOptions);
            this.Kids.Add(MLAbout);
            #endregion 
            #endregion

            #region Items creation
            //3 buttons -> only moving menu forward
            this.CreateButton(AppResource.Play, MLPlay); //Opens MenuList Play (MLPlay)
            this.CreateButton(AppResource.Options, MLOptions); //Opens MenuList Options (MLOptions)
            this.CreateButton(AppResource.About, MLAbout); //Opens MenuList About (MLAbout) 
            #endregion

        }

        public override void Update(Microsoft.Xna.Framework.GameTime gameTime, MyState myState)
        {
            base.Update(gameTime, myState);
        }

        public override void Enable()
        {
            base.Enable();
        }

        public override void Back()
        {
            Menu.Table.Game.Exit();
        }
    }
}
