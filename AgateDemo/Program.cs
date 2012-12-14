
using System;
using System.Linq;
using System.Collections.Generic;
using AgateLib;
using AgateLib.DisplayLib;
using AgateLib.Geometry;
using AgateLib.InputLib;

namespace AgateDemo
{
    static class Program
    {
        private class Entity
        {
            public int tile;
            public int x, y;
        }
        static Surface tileset;
        static int[,] map, map2;
        static List<Entity> entities, fixtures;

        static int tileWidth = 48;
        static int tileHeight = 64;
        static int tileHIncrease = 16;
        static int tileVIncrease = 32;
        static int mapWidth;
        static int mapHeight;
        static int cursorX = 3;
        static int cursorY = 3;
        static FontSurface mandrillFont;
        static Random rnd = new Random();

        static DisplayWindow wind;
        static Entity Spawn(int tileNo, int width, int height)
        {
            int rx = 1, ry = 4;
            switch (rnd.Next(2))
            {
                case 0: rx = rnd.Next(width / 20) * 20 + 1;
                    ry = rnd.Next(height / 20) * 20;
                    ry += (rnd.Next(2) == 0) ? 5 : 15;
                    break;
                case 1: ry = rnd.Next(height / 20) * 20 + 1;
                    rx = rnd.Next(width / 20) * 20;
                    rx += (rnd.Next(2) == 0) ? 5 : 15;
                    break;
            }
            return new Entity(){tile = tileNo, x = rx, y = ry};
        }
        static void Init()
        {
            map = new[,]
			{
{ 1178, 1177, 1177, 1177, 1177, 1177, 1177, 1177, 1177, 1177, 1177, 1177, 1177, 1177, 1177, 1177, 1177, 1177, 1177, 1179}, //0
{ 1176, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1176},
{ 1176, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1176},
{ 1176, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1176},
{ 1176, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1176}, //4
{ 1176, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1176},
{ 1176, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1176},
{ 1176, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1176},
{ 1176, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1176},
{ 1176, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1176}, //9
{ 1176, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1176},
{ 1186, 1177, 1194, 1177, 1177, 1177, 1179, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1176},
{ 1176, 1194, 1194, 1194, 1194, 1194, 1176, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1176},
{ 1176, 1194, 1194, 1194, 1194, 1194, 1186, 1177, 1177, 1177, 1194, 1179, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1176},
{ 1176, 1194, 1194, 1194, 1194, 1194, 1176, 1194, 1194, 1194, 1194, 1176, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1176}, //14
{ 1176, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1176, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1176},
{ 1176, 1194, 1194, 1194, 1194, 1194, 1176, 1194, 1194, 1194, 1194, 1176, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1176},
{ 1176, 1194, 1194, 1194, 1194, 1194, 1176, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1176},
{ 1176, 1194, 1194, 1194, 1194, 1194, 1176, 1194, 1194, 1194, 1194, 1176, 1194, 1194, 1194, 1194, 1194, 1194, 1194, 1176},
{ 1180, 1177, 1177, 1177, 1177, 1177, 1183, 1177, 1177, 1177, 1177, 1183, 1177, 1177, 1177, 1177, 1177, 1177, 1177, 1181}  //19
/*
				{ 1178, 1177,1177,1177, 1177,1177, 1179, 1175,1175 },
				{ 1176, 1194,1194,1194, 1194,1194, 1176, 1175,1175 },
				{ 1186, 1177,1177,1194, 1194,1194, 1194, 1194,1194 },
 				{ 1176, 1194,1194,1194, 1194,1194, 1176, 1175,1175 },
				{ 1176, 1194,1194,1194, 1194,1194, 1176, 1175,1175 },
				{ 1180, 1177,1177,1194,1177,1177,1181, 1175,1175 },
				{ 1175,1175,1175,1194,1175,1175,1175,  1175,1175 },*/
			};
            /*
            map = DungeonMap.merge(DungeonMap.geomorphs[1], DungeonMap.geomorphs[2], false);
            map = DungeonMap.merge(map, DungeonMap.geomorphs[rand.Next(3)], false);
            map2 = DungeonMap.merge(DungeonMap.rotateCW(DungeonMap.geomorphs[2]), DungeonMap.geomorphs[2], false);
            map2 = DungeonMap.merge(map2, DungeonMap.geomorphs[rand.Next(3)], false);
            map3 = DungeonMap.merge(DungeonMap.rotateCW(DungeonMap.geomorphs[rand.Next(3)]), DungeonMap.rotateCW(DungeonMap.geomorphs[rand.Next(3)]), false);
            map3 = DungeonMap.merge(map3, DungeonMap.geomorphs[rand.Next(3)], false);
            map = DungeonMap.merge(map, map2, true);
            map = DungeonMap.merge(map, map3, true);
            */
            map = DungeonMap.merge(DungeonMap.geomorphs[1], DungeonMap.geomorphs[rnd.Next(4)], false);
            for (int eh = 0; eh < 5; eh++)
            {
                if (rnd.Next(2) == 0)
                    map = DungeonMap.merge(map, DungeonMap.geomorphs[rnd.Next(4)], false);
                else
                    map = DungeonMap.merge(map, DungeonMap.rotateCW(DungeonMap.geomorphs[rnd.Next(4)]), false);
            }
            for (int ah = 1; ah < 5; ah++)
            {
                map2 = DungeonMap.merge(DungeonMap.geomorphs[rnd.Next(4)], DungeonMap.geomorphs[rnd.Next(4)], false);
                for (int eh = 0; eh < 5; eh++)
                {
                    if (rnd.Next(2) == 0)
                        map2 = DungeonMap.merge(map2, DungeonMap.geomorphs[rnd.Next(4)], false);
                    else
                        map2 = DungeonMap.merge(map2, DungeonMap.rotateCW(DungeonMap.geomorphs[rnd.Next(4)]), false);
                }
                map = DungeonMap.merge(map, map2, true);
            }

                map = DungeonMap.cleanUp(map);
           // map = DungeonMap.geomorph;
            fixtures = new List<Entity>()
			{
				new Entity() { tile = 1203, x = 10, y = 10 },
				new Entity() { tile = 1206, x = 12, y = 11 },
				new Entity() { tile = 1197, x = 14, y = 10 } /*,
                new Entity() { tile = 1189, x = 2, y = 4},
                new Entity() { tile = 1189, x = 10, y = 13},
                new Entity() { tile = 1188, x = 6, y = 15},
                new Entity() { tile = 1188, x = 11, y = 17},*/
			};
            int mw = map.GetLength(1), mh = map.GetLength(0);
            entities = new List<Entity>()
            {
				new Entity() { tile = 541 , x = 6, y = 7 },
				new Entity() { tile = 1409, x = 4, y = 18 },
				new Entity() { tile = 1406, x = 4, y = 3 },
				new Entity() { tile = 414 , x = 11, y = 5 },
				new Entity() { tile = 469 , x = 2, y = 4 },
				new Entity() { tile = 17, x = 8, y = 8 },
				new Entity() { tile = 14, x = 13, y = 6 },
				new Entity() { tile = 14, x = 15, y = 6 },
				new Entity() { tile = 14, x = 14, y = 6 },
				new Entity() { tile = 14, x = 12, y = 6 },
				new Entity() { tile = 3, x = 12, y = 8 }
            };
            for (int i = 0; i < 222; i++)
            {
                entities.Add(Spawn(i, mw, mh));
            }
            for (int i = 226; i < 434; i++)
            {
                entities.Add(Spawn(i, mw, mh));
            }
            
            tileWidth = 48;
            tileHeight = 64;
            tileHIncrease = 16;
            tileVIncrease = 32;
            mapWidth = map.GetUpperBound(1);
            mapHeight = map.GetUpperBound(0);
            /*var alphaMatrix = new ColorMatrix();
            alphaMatrix.Matrix33 = 0.5f;
            alphaAttributes = new ImageAttributes();
            alphaAttributes.SetColorMatrix(alphaMatrix);*/
            cursorX = 4;
            cursorY = 5;

            //wind = DisplayWindow.CreateWindowed("Vicious Demo with AgateLib", ((mapWidth + 1) * 32) + (tileHIncrease * (1 + mapHeight)), (mapHeight * tileVIncrease) + tileHeight);
            wind = DisplayWindow.CreateWindowed("Vicious Demo with AgateLib", ((20 ) * 32) + (tileHIncrease * (20)), (19 * tileVIncrease) + tileHeight);

            tileset = new Surface("slashem-revised.png");
            
            mandrillFont = FontSurface.AgateMono10;
            
        }

        public static void Update()
        {
            
            foreach (Entity ent in entities)
            {
                var randVal = rnd.Next(4);
                switch (randVal)
                {
                    case 0: if (ent.x > 0 && (map[ent.y, ent.x - 1] == 1194) && entities.FirstOrDefault(e => e.x == ent.x - 1 && e.y == ent.y) == null) ent.x--;
                        break;
                    case 1: if (ent.y > 0 && (map[ent.y - 1, ent.x] == 1194) && entities.FirstOrDefault(e => e.x == ent.x && e.y == ent.y - 1) == null) ent.y--;
                        break;
                    case 2: if (ent.x + 1 < mapWidth && (map[ent.y, ent.x + 1] == 1194) && entities.FirstOrDefault(e => e.x == ent.x + 1 && e.y == ent.y) == null) ent.x++;
                        break;
                    case 3: if (ent.y + 1 < mapHeight && (map[ent.y + 1, ent.x] == 1194) && entities.FirstOrDefault(e => e.x == ent.x && e.y == ent.y + 1) == null) ent.y++;
                        break;
                }
            }
        }
        static void Show()
        {
            Display.Clear(Color.FromArgb(32, 32, 32));

            for (int row = (cursorY < 20) ? 0 : (cursorY > mapHeight - 10) ? mapHeight - 20 : cursorY - 20; row <= mapHeight && row <= cursorY + 20; row++)
            {
                var pY = tileVIncrease * ((cursorY <= 10) ? row : (cursorY > mapHeight - 10) ? row - ( mapHeight - 19 ): row - (cursorY - 10)); //   //(cursorY > mapHeight - 10) ? mapHeight - (cursorY - 10) : cursorY - 10)
                var pX = tileHIncrease * (20 - 1 - ((cursorY <= 10) ? row : (cursorY > mapHeight - 10) ? row  - ( mapHeight - 19 ): row - (cursorY - 10)));// +tileHIncrease; //row - (cursorY - 10)
                for (var col = (cursorX <= 10) ? 0 : (cursorX > mapWidth - 10) ? mapWidth - 19: cursorX - 10 ; col <= mapWidth && (col < cursorX + 10 || col < 20); col++)
                {
                    var dest = new Rectangle(pX, pY, tileWidth, tileHeight);
                    var tile = map[row, col];
                    var src = new Rectangle((tile % 38) * tileWidth, (tile / 38) * tileHeight, tileWidth, tileHeight);
                    tileset.Draw(src, dest);

#if !CURSORONFLOOR
                    if (cursorX == col && cursorY == row)
                        tileset.Draw(new Rectangle((1442 % 38) * tileWidth, (1442 / 38) * tileHeight, tileWidth, tileHeight), dest);
#endif

                    var entity = entities.FirstOrDefault(e => e.x == col && e.y == row);
                    var fixture = fixtures.FirstOrDefault(e => e.x == col && e.y == row);
                    if (entity != null)
                    {
                        tile = entity.tile;
                        src = new Rectangle((tile % 38) * tileWidth, (tile / 38) * tileHeight, tileWidth, tileHeight);
                        tileset.Draw(src, dest);
                    }
                    else if (fixture != null)
                    {
                        tile = fixture.tile;
                        src = new Rectangle((tile % 38) * tileWidth, (tile / 38) * tileHeight, tileWidth, tileHeight);
                        tileset.Draw(src, dest);
                    }
                    pX += tileVIncrease;
                }
                //pY += tileHIncrease;
            }
            mandrillFont.DrawText(32.0, 32.0, "FPS: " + (int)Display.FramesPerSecond);
        }




//        private static bool hasPressedSpace = false;
        [STAThread]
        static void Main(string[] args)
        {
            //AgateLib.AgateFileProvider.chosenLocation = (args.Length > 0) ? args[0] : null;
            using (AgateSetup setup = new AgateSetup("Vicious Agate Demo", args))
            {
                setup.ApplicationName = "Vicious Agate Demo";
                setup.CompanyName = "The RGRD Agenda";
                setup.InitializeAll();

                if (setup.WasCanceled)
                    return;
                Init();

                Keyboard.KeyDown += new InputEventHandler(OnKeyDown);

                while (!Display.CurrentWindow.IsClosed)
                {
                    Display.BeginFrame();
                    Show();
                    Display.EndFrame();

                    Core.KeepAlive();
                }
            }
        }

        static void OnKeyDown(InputEventArgs e)
        {
            if (e.KeyCode == KeyCode.Space)
                Update();
            else if (e.KeyCode == KeyCode.Left && cursorX > 0 && map[cursorY, cursorX - 1] == 1194)
                cursorX--;
            else if (e.KeyCode == KeyCode.Right && cursorX < mapWidth && map[cursorY, cursorX + 1] == 1194)
                cursorX++;
            else if (e.KeyCode == KeyCode.Up && cursorY > 0 && map[cursorY - 1, cursorX] == 1194)
                cursorY--;
            else if (e.KeyCode == KeyCode.Down && cursorY < mapHeight && map[cursorY + 1, cursorX] == 1194)
                cursorY++;
            else if (e.KeyCode == KeyCode.Q)
                Display.CurrentWindow.Dispose();
        }


    }
}
