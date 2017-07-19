using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameEngine;
using SFML.Graphics;
using SFML.Audio;
using SFML.Window;
using SFML.System;
using System.IO; //to write in file


namespace Mario
{
    public class MainScene : Scene
    {
        public CharacterEntity player;
        private int coordinatedAnimFramePointer = 0;
        Clock coordinatedAnimClock = new Clock();
        Font arial = new Font(@"resources\arial.ttf");  //o @ desabilita caracteres de escape, \n etc. \ agora é só \
        public const int MAX_TIME = 400;
        private System.Windows.Forms.Timer gameTimer;
        private System.Windows.Forms.Timer gameTimerTracking;
        private int timeCounter, timeCounterTracking;
        private String actionCurrent = "";
        public int PlayerLives = 3;
        public int Score = 0;
        private string data = "";//data to save in .txt file
        private string file = "";//name the file
        public int currentLevel = 1;
        private int positionX = 0;
        


        public MainScene(GameObject gameObject): base(gameObject)   //chama construtor da classe base, no caso, Scene.
        {
            
        }

        public override void Initialize()   //override é necessario para sobrescrever metodos virtuais ou abstratos.
        {
            // This method is called when the scene is added to the scene manager

            // Initialize level data & viewport
            this.level.LoadMap(System.IO.Directory.GetCurrentDirectory() + @"\leveldata.xml", currentLevel);//linha que cria o mapa
            this.BackgroundColor = this.level.BackgroundColor;

            viewPort = new Viewport(this._gameObject, 64, 64, this.level);  //encapsula parametros de renderização e da janela
            this.SetTracking();

            base.Initialize();
        }

        public override void Reset()
        {
            // This method is called when the StartScene() method is called
            this.level.LoadMap(System.IO.Directory.GetCurrentDirectory() + @"\leveldata.xml", currentLevel);
            viewPort.Reset();

            this.Entities.Clear();

            // set up the player character entity
            Characters.Mario mario = new Characters.Mario(this._gameObject);
            player = mario;
            this.Entities.Add(player);
            positionX = player.X;

            ResourceManager.Instance.PlaySound("music-1");

            // Set up the game timer
            timeCounter = timeCounterTracking = level.time;
            gameTimerTracking = new System.Windows.Forms.Timer();
            gameTimer = new System.Windows.Forms.Timer();
           
            
            gameTimer.Tick += new EventHandler(gameTimer_Tick);
            gameTimerTracking.Tick += new EventHandler(gameTimerTracking_Tick);
            
            gameTimer.Interval = 1000; // 1 second
            gameTimer.Start();

            gameTimerTracking.Interval = 1000; // 1 second
            gameTimerTracking.Start();

            //data = "";
            PrintTracking("Begin Game");
            file = String.Format("{0: yyyy'-'MM'-'dd'T'HH'h'mm'm'ss }.txt", DateTime.Now);// file name pattern

            base.Reset();
        }

        public  void PrintTracking(String action)
        {
            actionCurrent = action;
            if (timeCounterTracking != timeCounter || timeCounterTracking == MAX_TIME)
            {
                string newData = String.Format("{0,8}{1,8}{2,12}{3,14}{4,8}{5,9}", MAX_TIME - timeCounter , PlayerLives, Score, action, positionX, player.Y);
                data += newData + Environment.NewLine;
                Console.WriteLine(newData);
            }
            timeCounterTracking = timeCounter;
        }

        public void SetTracking()
        {
            string headerData = String.Format("{0,10}{1,10}{2,10}{3,10}{4,10}{5,10}", "GameTime", "PlayLife", "Score", "Action", "PosX", "PosY");
            data += headerData + Environment.NewLine;
            Console.WriteLine(headerData); //Headings
        }

        public override void HandleKeyPress(KeyEventArgs e)
        {
            //Console.WriteLine("o que aparece em code: -"+string(e.Code)+"-");
            if (e.Code == Keyboard.Key.P)
            {
                if (!this.IsPaused)
                {
                    PrintTracking("Press Pause");
                    ResourceManager.Instance.GetSound("pause").Play();
                    ResourceManager.Instance.GetSound("music-1").Pause();
                    this.Pause();
                }
                else
                {
                    ResourceManager.Instance.GetSound("pause").Stop();
                    ResourceManager.Instance.GetSound("music-1").Play();
                    this.Resume();
                }
            }

            if (e.Code == Keyboard.Key.Right)
            {

                actionCurrent = "MOVE RIGHT";
                player.Facing = Direction.RIGHT;
                player.IsMoving = true;
                positionX += 10;
               
            }

            if (e.Code == Keyboard.Key.Left)
            {

                actionCurrent = "MOVE LEFT";
                player.Facing = Direction.LEFT;
                player.IsMoving = true;
                positionX -= 10;
            }

            if (e.Code == Keyboard.Key.Space && player.IsJumping == false)
            {

                
                ResourceManager.Instance.PlaySound("smallJumpSound");
                player.IsJumping = true;
                player.Velocity = -55;
                PrintTracking("MOVE JUMP");
                
            }

            if (e.Code == Keyboard.Key.Escape)
            {
                this._gameObject.Close();
                
            }

            base.HandleKeyPress(e);
        }

        public override void HandleKeyReleased(KeyEventArgs e)
        {
            if(!player.IsJumping)
                player.IsMoving = false;
        }

        public override void Update()
        {
            for (int x = 0; x < Entities.Count; x++)
            {

                CharacterEntity e = (CharacterEntity)Entities[x];
                // Update animations of common entities at the same time
                if (coordinatedAnimClock.ElapsedTime.AsMilliseconds() > 100)//a cada 100 milisegundos
                {
                    coordinatedAnimClock.Restart();
                    int Count;

                    foreach (CharacterEntity cz in Entities.Where(zx => zx.GetType() == typeof(Mario.Characters.CoinBox)))
                        cz.sprite.TextureRect = cz.EntitySpriteSheet.GetSprite(Direction.NONE, coordinatedAnimFramePointer);

                    foreach (CharacterEntity cz in Entities.Where(zx => zx.GetType() == typeof(Mario.Characters.Coin)))
                    {
                        cz.sprite.TextureRect = cz.EntitySpriteSheet.GetSprite(Direction.NONE, coordinatedAnimFramePointer);
                        Count = cz.EntitySpriteSheet.SpriteFrames.Count;
                    }


                    coordinatedAnimFramePointer++;
                    if (coordinatedAnimFramePointer >= e.EntitySpriteSheet.SpriteFrames.Count)
                        coordinatedAnimFramePointer = 0;
                }

                // Update the entity
                e.Update();
                e.Draw();

                // Constrain speed of jump/fall based on tile height
                if (e.Velocity > viewPort.TileHeight) e.Velocity = viewPort.TileHeight;
                if (e.Velocity < -viewPort.TileHeight) e.Velocity = -viewPort.TileHeight;

                // Check for collisions
                if (e.Y > -1)
                    CheckCharacterCollisions(e);

                // Character is too far offscreen. Kill it.
                if (level.goLeft == true)
                {
                    if (e.X < -100 && !e.IsStatic)// || e.X > _gameObject.Window.Size.X + 100)
                        e.Delete = true;
                }
                else
                {
                    if (e.X < -100)// || e.X > _gameObject.Window.Size.X + 100)
                        e.Delete = true;
                }
            }

            // Scroll viewport if needed
            ViewportScrollHandler();

            // Update the score, time, etc
            UpdateTextData();
            

            // Remove and entities flagged for deletion
            // Occurs when an enemy falls in a pit, or scrolls off screen, destroyed by player, etc
            for (int i = Entities.Count - 1; i >= 0; i--)
                if (Entities[i].Delete)
                    Entities.RemoveAt(i);

        }

        public override void Exit()
        {
            ResourceManager.Instance.StopSound("music-1");

            gameTimer.Enabled = false;
            File.WriteAllText(file, data);
        }

        private void CheckCharacterCollisions(CharacterEntity e)
        {
            // Character collisions
            for (int x = 0; x < Entities.Count; x++)
            {
                CharacterEntity c = (CharacterEntity)Entities[x];

                if (c.IsStatic || c.IgnoreAllCollisions)
                    continue;

                if (c.ID != e.ID)
                {
                    FloatRect entityRect = e.sprite.GetGlobalBounds();
                    FloatRect area = new FloatRect();

                    FloatRect boundBox = c.sprite.GetGlobalBounds();
                    boundBox.Top = boundBox.Top + boundBox.Height + 1;
                    boundBox.Height = 1;

                    if (boundBox.Intersects(entityRect))
                    {
                        // Down
                        e.OnCharacterCollision(c, Direction.DOWN);
                        continue;
                    }

                    boundBox = c.sprite.GetGlobalBounds();
                    boundBox.Top = boundBox.Top - 1;
                    boundBox.Height = 1;

                    if (boundBox.Intersects(entityRect, out area))
				    {
	                    // Up
                        e.OnCharacterCollision(c, Direction.UP);
                        continue;
				    }

                    boundBox = c.sprite.GetGlobalBounds();
                    boundBox.Left = boundBox.Left + boundBox.Width;
                    boundBox.Width = 1;

                    if (boundBox.Intersects(entityRect))
                    {
                        // Right
                        e.OnCharacterCollision(c, Direction.LEFT);
                        c.OnCharacterCollision(e, Direction.RIGHT);
                        continue;
                    }

                    boundBox = c.sprite.GetGlobalBounds();
                    boundBox.Width = 1;

                    if (boundBox.Intersects(entityRect))
                    {
                        // Left
                        e.OnCharacterCollision(c, Direction.RIGHT);
                        c.OnCharacterCollision(e, Direction.LEFT);
                        continue;
                    }
                       
                }
            }
            
        }

        protected void ViewportScrollHandler()
        {
            // Check for scroll           
            int midScreen = (int)_gameObject.Window.Size.Y / 2;

            // Scroll viewport until end of level
            bool condition = true;
            if (level.goLeft == false) condition = player.Facing == Direction.RIGHT && player.X >= midScreen;
            //if (player.IsMoving && player.Facing == Direction.RIGHT && player.X >= midScreen && !viewPort.IsEndOfLevel)
            if (player.IsMoving && !viewPort.IsEndOfLevel && condition)
            {
                player.X = midScreen;

                foreach (CharacterEntity character in _gameObject.SceneManager.CurrentScene.Entities)
                {
                    if (!character.IsPlayer && character.Facing == Direction.RIGHT)
                    {
                        character.Acceleration = 0;
                    }
                    if (!character.IsPlayer && character.Facing == Direction.LEFT)
                    {
                        character.Acceleration = -20; // Math.Sign(character.Acceleration) * (Math.Abs(character.Acceleration) * 2);
                    }
                    
                    
                }
                
                viewPort.Scroll(Direction.RIGHT, player.Acceleration);
            }
            else
            #region Scroll Backwards
            // Uncomment the lines below to allow player to move backwards
            /*
            if (player.IsMoving && player.Facing == Direction.LEFT && player.X <= midScreen && !viewPort.IsStartOfLevel)
            {
                player.X = midScreen;
                viewPort.Scroll(Direction.LEFT, player.Acceleration);

                foreach (CharacterEntity character in _gameObject.SceneManager.CurrentScene.Entities)
                {
                    if (!character.IsPlayer && character.Facing == Direction.LEFT)
                    {
                        character.Acceleration = 0;
                    }
                    if (!character.IsPlayer && character.Facing == Direction.RIGHT)
                    {
                        character.Acceleration = -20; // Math.Sign(character.Acceleration) * (Math.Abs(player.Acceleration) * 2);
                    }

                }
            }
            else*/
            #endregion
            {
                // If player is not moving, reset the acceleration of visibile entities to normal values
                foreach (CharacterEntity character in _gameObject.SceneManager.CurrentScene.Entities)
                {
                    if (!character.IsPlayer && !character.IsStatic)
                    {
                        if (character.Acceleration == 0) character.Acceleration = 1;
                        character.Acceleration = Math.Sign(character.Acceleration) * 10;
                    }
                }

            }

        }

        public override void DrawBackground()
        {
            List<Entity> NewEntities = viewPort.Render();

            foreach (Entity e in NewEntities)
            {
                CharacterEntity c = null;

                switch (e.Name)
                {
                    case "goomba": c = new Characters.Goomba(this._gameObject); break;
                    case "koopatroopa": c = new Characters.KoopaTroopa(this._gameObject); break;
                    case "coinbox": c = new Characters.CoinBox(this._gameObject); break;
                    case "emptycoinbox": c = new Characters.EmptyCoinBox(this._gameObject); break;
                    case "rock": c = new Characters.Rock(this._gameObject); break;
                    case "brick": c = new Characters.Brick(this._gameObject); break;
                    case "block": c = new Characters.Block(this._gameObject); break;
                    case "goal": c = new Characters.Goal(this._gameObject); break;
                    case "pipetopleft": c = new Characters.PipeTopLeft(this._gameObject); break;
                    case "pipetopright": c = new Characters.PipeTopRight(this._gameObject); break;
                    case "pipeleft": c = new Characters.PipeLeft(this._gameObject); break;
                    case "piperight": c = new Characters.PipeRight(this._gameObject); break;
                    case "flagpole": c = new Characters.FlagPole(this._gameObject); break;
                    case "flag": c = new Characters.Flag(this._gameObject); break;
                    case "poletop": c = new Characters.PoleTop(this._gameObject); break;
                    case "smallcastle": c = new Characters.SmallCastle(this._gameObject); break;
                    case "coin": c = new Characters.Coin(this._gameObject); break;
                    case "coinbounce": c = new Characters.CoinBounce(this._gameObject); break;
                    case "piranhaplant": c = new Characters.PiranhaPlant(this._gameObject);break;
                }

                c.X = e.X;
                c.Y = e.Y;
                c.OriginTileCol = e.OriginTileCol;
                c.OriginTileRow = e.OriginTileRow;

                var xx = Entities.Where(x => x.OriginTileRow == c.OriginTileRow && x.OriginTileCol == c.OriginTileCol);
                if (xx.Count() == 0)
                    Entities.Add(c);
            }

            NewEntities.Clear();

        }

        private void UpdateTextData()
        {
            Text text = new Text("", arial);
            string t = "MARIO";
            text.DisplayedString = t;
            text.Position = new Vector2f(100, 50);
            text.Draw(this._gameObject.Window, RenderStates.Default);

            t = Score.ToString("000000"); 
            text.DisplayedString = t;
            text.Position = new Vector2f(100, 80);
            text.Draw(this._gameObject.Window, RenderStates.Default);

            t = "x " + this.PlayerLives.ToString("00");
            text.DisplayedString = t;
            text.Position = new Vector2f(300, 80);
            text.Draw(this._gameObject.Window, RenderStates.Default);

            t = "WORLD";
            text.DisplayedString = t;
            text.Position = new Vector2f(530, 50);
            text.Draw(this._gameObject.Window, RenderStates.Default);

            t = "1-"+currentLevel;
            text.DisplayedString = t;
            text.Position = new Vector2f(560, 80);
            text.Draw(this._gameObject.Window, RenderStates.Default);

            t = "TIME";
            text.DisplayedString = t;
            text.Position = new Vector2f(830, 50);
            text.Draw(this._gameObject.Window, RenderStates.Default);

            t = timeCounter.ToString();
            text.DisplayedString = t;
            text.Position = new Vector2f(850, 80);
            text.Draw(this._gameObject.Window, RenderStates.Default);
        }

        private void gameTimerTracking_Tick(object sender, EventArgs e) 
        {
            PrintTracking(actionCurrent);
        }

        private void gameTimer_Tick(object sender, EventArgs e)
        {
            timeCounter--;
            
            if (timeCounter == 100)
            {
                ResourceManager.Instance.GetSound("warning").Play();
                ResourceManager.Instance.GetSound("music-1").Pause();
            }

            if(ResourceManager.Instance.GetSound("warning").Status == SoundStatus.Stopped && ResourceManager.Instance.GetSound("music-1").Status == SoundStatus.Paused)
                ResourceManager.Instance.GetSound("music-1").Play();

            if (timeCounter == 0)
            {
                gameTimer.Stop();
                Characters.Mario m = (Characters.Mario) Entities.Find(x => x.GetType() == typeof(Characters.Mario));
                m.Die();
            }
        }

        public void IncreaseScore(int points)
        {
            Score += points;
        }
    }
}
