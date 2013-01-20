using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using AgateLib;
using AgateLib.DisplayLib;
using AgateLib.Geometry;
//using Newtonsoft.Json;
using System.Runtime.Serialization;
using ProtoBuf;
using System.ComponentModel;
using ProtoBuf.Meta;

namespace AgateDemo
{
        public class Level
        {
            public int floor;
            public static Random rnd = new Random();
            public static bool safeStart = true;

            //[JsonIgnore]
            public Color[,] mapColors;
            //public int minVisibleX = 0, minVisibleY = 0, maxVisibleX = 19, maxVisibleY = 19;
            
            //public enum Direction { North, East, South, West, None };
           // public enum InputMode { Menu, Map, None, Dialog };
           // public static InputMode mode = InputMode.None;
            //public static List<Direction> dirlist = new List<Direction>();
            //currentlyPerformingMenuEvent = false, 
             public static Point requestingMove = new Point() { X = -1, Y = -1 };
            //private static Demo.Mob currentActorInternal = null, hoverActorInternal = null;
            /*public static Mob currentActor
            {
                get { return currentActorInternal; }
                set
                {
                    if (value == null && hoverActor == null)
                    {
                        currentActorInternal = null;
                        UnitInfo.sw.Reset();
                        UnitInfo.sw.Pause();
                    }
                    else if (hoverActor == null)
                    {
                        currentActorInternal = value;
                        UnitInfo.sw.Reset();
                        UnitInfo.sw.ForceResume();
                    }
                    else
                    {
                        currentActorInternal = value;
                    }
                }
            }
            public static Mob hoverActor
            {
                get { return hoverActorInternal; }
                set
                {

                    if (value == null && currentActor == null)
                    {
                        hoverActorInternal = null;
                        UnitInfo.sw.Reset();
                        UnitInfo.sw.Pause();
                    }
                    else
                    {
                        hoverActorInternal = value;
                        UnitInfo.sw.Reset();
                        UnitInfo.sw.ForceResume();
                    }
                }
            }*/
            static Surface tileset;

           // public const int mapDisplayWidth = 960, mapDisplayHeight = 672;
            public int[,] map, map2;
            public int[] map1D;
            public Dictionary<Point, Demo.Mob> entities, o_entities, allies;
            //[JsonIgnore]
            public Dictionary<Point, int> highlightedCells = new Dictionary<Point, int>();
            //[JsonIgnore]
            public Dictionary<Point, int> highlightedTargetCells = new Dictionary<Point, int>();
            //[JsonIgnore]
            public Dictionary<Point, int> nonHighlightedFreeCells = new Dictionary<Point, int>();
            public Dictionary<Point, bool> doNotStopCells = new Dictionary<Point, bool>();

            //[JsonIgnore]
            public double[,] visibleCells;

            //[JsonIgnore]
            public double[,] seenCells;
            public Dictionary<Point, Demo.Entity> fixtures;
            //[JsonIgnore]
            public Dictionary<Point, int> displayDamage = new Dictionary<Point, int>();
            //[JsonIgnore]
            public Dictionary<Point, bool> displayKills = new Dictionary<Point, bool>();
            // public static SimpleUI basicUI;

            static int tileWidth = 48;
            static int tileHeight = 64;
            static int tileHIncrease = 16;
            static int tileVIncrease = 32;
            public int mapWidthBound;
            public int mapHeightBound;
            //public static int Demo.cursorX = 3;
            //public static int Demo.cursorY = 3;
            static FontSurface mandrillFont;

//            static DisplayWindow wind;

            [OnDeserialized]
            internal void OnDeserializedMethod(StreamingContext context)
            {
                int mw = map.GetLength(1), mh = map.GetLength(0);

                visibleCells = new double[mh, mw];
                seenCells = new double[mh, mw];
                mapColors = DungeonMap.recolor(map);
            }
            public void recalculateVision()
            {
                visibleCells.Fill(indices => 0.0);
                foreach (Demo.Mob mb in allies.Values)
                {
                    mb.fov.calculateSight();
                    visibleCells.Fill(indices => (((double)mb.fov.sight.GetValue(indices) > (double)visibleCells.GetValue(indices)) ? (double)mb.fov.sight.GetValue(indices) : (double)visibleCells.GetValue(indices)));
                }
                seenCells.Fill(indices => (((double)visibleCells.GetValue(indices) > 0.0 || (double)seenCells.GetValue(indices) > 0.0) ? 1.0 : 0.0));//UnionWith(visibleCells);
            }
            public Demo.Mob checkPos(int checkX, int checkY)
            {
                Demo.Mob ret = null;
                entities.TryGetValue(new Point() { X = checkX, Y = checkY }, out ret);
                return ret;
            }
            public Demo.Mob checkPos(int checkX, int checkY, int checkTile)
            {
                Demo.Mob ret = null;
                entities.TryGetValue(new Point() { X = checkX, Y = checkY }, out ret);
                if (ret != null && ret.tile != checkTile)
                    ret = null;
                return ret;
            }
            public Demo.Entity checkFixture(int checkX, int checkY)
            {
                Demo.Entity ret = null;
                fixtures.TryGetValue(new Point() { X = checkX, Y = checkY }, out ret);
                return ret;
            }
            public Demo.Entity checkFixture(int checkX, int checkY, int checkTile)
            {
                Demo.Entity ret = null;
                fixtures.TryGetValue(new Point() { X = checkX, Y = checkY }, out ret);
                if (ret != null && ret.tile != checkTile)
                    ret = null;
                return ret;
            }
            //[JsonIgnore]
            private Dictionary<Point, int> grStore = new Dictionary<Point, int>();
            public List<Point> safeUpCells = new List<Point>(), safeDownCells = new List<Point>();
            //[JsonIgnore]
            private Dictionary<Point, bool> ivStore = new Dictionary<Point, bool>();
            public HashSet<Point> enemyBlockedCells = new HashSet<Point>();
            public void addStairs(bool isBottom)
            {
                int j = rnd.Next(map.GetLength(0));
                int i = rnd.Next(map.GetLength(1));
                while (map[j, i] != DungeonMap.gr || checkFixture(i, j) != null)
                {
                    j = rnd.Next(map.GetLength(0));
                    i = rnd.Next(map.GetLength(1));
                }
                grStore = new Dictionary<Point, int>();
                if (map[j, i] == DungeonMap.gr && checkFixture(i, j) == null)
                    Demo.calculateAllMoves(this, i, j, 7, false, grStore, ivStore, true);
                if (grStore.Count > 8)
                {
                    fixtures.Add(new Point(i, j), new Demo.Entity(1197,i,j, Color.Black));
                    safeUpCells = grStore.Keys.Where((k, e) => grStore[k] == 6).ToList();
                    safeUpCells.AddRange(grStore.Keys.Where((k, e) => grStore[k] == 5));
                    safeUpCells.AddRange(grStore.Keys.Where((k, e) => grStore[k] == 4));
                    safeUpCells.AddRange(grStore.Keys.Where((k, e) => grStore[k] == 3));
                    safeUpCells.AddRange(grStore.Keys.Where((k, e) => grStore[k] == 2));
                    safeUpCells.AddRange(grStore.Keys.Where((k, e) => grStore[k] == 1));
                    safeUpCells.AddRange(grStore.Keys.Where((k, e) => grStore[k] == 0));
                    enemyBlockedCells.Add(new Point(i, j));
                    enemyBlockedCells.UnionWith(safeUpCells); //Add(new Point(i, j))
                }
                else
                {
//                    grStore.Clear();
                    addStairs(isBottom);
                }
                if (isBottom)
                    return;
                j = rnd.Next(map.GetLength(0));
                i = rnd.Next(map.GetLength(1));
                grStore = new Dictionary<Point, int>();
                while (map[j, i] != DungeonMap.gr || checkFixture(i, j) != null)
                {
                    j = rnd.Next(map.GetLength(0));
                    i = rnd.Next(map.GetLength(1));
                }
                if (map[j, i] == DungeonMap.gr)
                {
                    Demo.calculateAllMoves(this, i, j, 7, false, grStore, ivStore, true);
                }
                if (grStore.Count > 8)
                {
                    fixtures.Add(new Point(i, j), new Demo.Entity(1198, i, j, Color.Black));
                    safeDownCells = grStore.Keys.Where((k, e) => grStore[k] == 6).ToList();
                    safeDownCells.AddRange(grStore.Keys.Where((k, e) => grStore[k] == 5));
                    safeDownCells.AddRange(grStore.Keys.Where((k, e) => grStore[k] == 4));
                    safeDownCells.AddRange(grStore.Keys.Where((k, e) => grStore[k] == 3));
                    safeDownCells.AddRange(grStore.Keys.Where((k, e) => grStore[k] == 2));
                    safeDownCells.AddRange(grStore.Keys.Where((k, e) => grStore[k] == 1));
                    safeDownCells.AddRange(grStore.Keys.Where((k, e) => grStore[k] == 0));
                    enemyBlockedCells.Add(new Point(i, j));
                    enemyBlockedCells.UnionWith(safeDownCells);//Add(new Point(i, j));
                }
                else
                {
                    grStore.Clear();
                    safeUpCells.Clear();
                    safeDownCells.Clear();
                    addStairs(isBottom);
                }
                
            }

            public Demo.Mob Spawn(int tileNo, int width, int height)
            {
                int rx = rnd.Next(width);
                int ry = rnd.Next(height);
                if (map[ry, rx] == DungeonMap.gr && enemyBlockedCells.Contains(new Point(rx, ry)) == false)
                {
                    Demo.Mob nt = new Demo.Mob(tileNo, rx, ry, false, floor);

                    if (checkPos(nt.x, nt.y) != null)
                        return Spawn(tileNo, width, height);
                    nt.skillList.Add(new Skill("Basic Attack", rnd.Next(3, 7), 1, 1, SkillAreaKind.SingleTarget, 0, true));
                    nt.currentSkill = nt.skillList[0];
                    //nt.ui.addSkills(nt);
                    entities[nt.pos] = nt;
                    o_entities[nt.o_pos] = nt;
                    return nt;
                }
                return Spawn(tileNo, width, height);
            }
            public void Init(bool isBottom)
            {
                
                //mode = InputMode.Dialog;
                int numMorphs = DungeonMap.geomorphs.Count;
                if (safeStart)
                    map = DungeonMap.merge(DungeonMap.geomorphs[0], DungeonMap.geomorphs[rnd.Next(numMorphs)], false);
                else
                    map = DungeonMap.merge(DungeonMap.geomorphs[rnd.Next(numMorphs)], DungeonMap.geomorphs[rnd.Next(numMorphs)], false);
                for (int eh = 2; eh < 2; eh++)
                {
                    if (rnd.Next(2) == 0)
                        map = DungeonMap.merge(map, DungeonMap.geomorphs[rnd.Next(numMorphs)], false);
                    else
                        map = DungeonMap.merge(map, DungeonMap.rotateCW(DungeonMap.geomorphs[rnd.Next(numMorphs)]), false);
                }
                for (int ah = 1; ah < 2; ah++)
                {
                    map2 = DungeonMap.merge(DungeonMap.geomorphs[rnd.Next(numMorphs)], DungeonMap.geomorphs[rnd.Next(numMorphs)], false);
                    for (int eh = 2; eh < 2; eh++)
                    {
                        if (rnd.Next(2) == 0)
                            map2 = DungeonMap.merge(map2, DungeonMap.geomorphs[rnd.Next(numMorphs)], false);
                        else
                            map2 = DungeonMap.merge(map2, DungeonMap.rotateCW(DungeonMap.geomorphs[rnd.Next(numMorphs)]), false);
                    }
                    map = DungeonMap.merge(map, map2, true);
                }

                fixtures = new Dictionary<Point, Demo.Entity>()
                {
                };/*
                fixtures.Add(new Point() { X = 10, Y = 10 }, new Demo.Entity() { tile = 1203, x = 10, y = 10 });
                fixtures.Add(new Point() { X = 12, Y = 11 }, new Demo.Entity() { tile = 1206, x = 12, y = 11 });
                */

                /*,
               new Entity() { tile = 1189, x = 2, y = 4},
               new Entity() { tile = 1189, x = 10, y = 13},
               new Entity() { tile = 1188, x = 6, y = 15},
               new Entity() { tile = 1188, x = 11, y = 17},*/

                map = DungeonMap.cleanUp(map, this);
                addStairs(isBottom);
                map = DungeonMap.theme(map);
                mapColors = DungeonMap.recolor(map);

                // map = DungeonMap.geomorph;
                int mw = map.GetLength(1), mh = map.GetLength(0);

                visibleCells = new double[mh, mw];
                seenCells = new double[mh, mw];

                allies = new Dictionary<Point, Demo.Mob>();
                entities = new Dictionary<Point, Demo.Mob>()
                {
                };
                o_entities = new Dictionary<Point, Demo.Mob>()
                {
                };
                
                for (int i = 0, c = 0; i < 222 && c < 50; c++, i+= rnd.Next(2, 7))//= rnd.Next(2, 7)) //check for c to limit number of entities
                {
                    Spawn(i, mw, mh);
                }
                for (int i = 226, c = 0; i < 434 && c < 50; i += rnd.Next(2, 7), c++)
                {
                    Spawn(i, mw, mh);
                }
                for (int i = 473, c = 0; i < 542 && c < 20; i += rnd.Next(2, 7), c++)
                {
                    Spawn(i, mw, mh);
                }
                tileWidth = 48;
                tileHeight = 64;
                tileHIncrease = 16;
                tileVIncrease = 32;
                mapWidthBound = map.GetUpperBound(1);
                mapHeightBound = map.GetUpperBound(0);
                /*var alphaMatrix = new ColorMatrix();
                alphaMatrix.Matrix33 = 0.5f;
                alphaAttributes = new ImageAttributes();
                alphaAttributes.SetColorMatrix(alphaMatrix);*/
                Demo.cursorX = 6;
                Demo.cursorY = 7;

                //wind = DisplayWindow.CreateWindowed("Vicious Demo with AgateLib", ((mapWidth + 1) * 32) + (tileHIncrease * (1 + mapHeight)), (mapHeight * tileVIncrease) + tileHeight);

               // Display.RenderState.WaitForVerticalBlank = true;
               // wind = DisplayWindow.CreateWindowed("Vicious Demo with AgateLib", Demo.mapDisplayWidth, Demo.mapDisplayHeight + 32, false);      //(19 * tileVIncrease) + tileHeight); //((20) * 32) + (tileHIncrease * (20))
            }
            static Level()
            {
                mandrillFont = FontSurface.BitmapMonospace("monkey_x2.png", new Size(12, 28)); //"Resources" + "/" + 
                tileset = new Surface("cleandungeon3.png"); //System.IO.Path.DirectorySeparatorChar //"Resources" + "/" + 
            }
            public void Show()
            {
                //Display.Clear(Color.FromArgb(32, 32, 32));
                // (cursorY <= 10) (cursorY > mapHeight - 10)    if cursorY <= 10, (vals < 20); 
                //minVisibleY = (cursorY < 20) ? 0 : (cursorY > mapHeight - 10) ? mapHeight - 20 : cursorY - 20;
                //maxVisibleY = minVisibleY;
                for (int row = (Demo.cursorY < 20) ? 0 : (Demo.cursorY > mapHeightBound - 10) ? mapHeightBound - 20 : Demo.cursorY - 20; row <= mapHeightBound && row <= Demo.cursorY + 20; row++)
                {
                    // maxVisibleY++;
                    var pY = tileVIncrease * ((Demo.cursorY <= 10) ? row : (Demo.cursorY > mapHeightBound - 10) ? row - (mapHeightBound - 19) : row - (Demo.cursorY - 10));
                    var pX = tileHIncrease * (20 - 1 - ((Demo.cursorY <= 10) ? row : (Demo.cursorY > mapHeightBound - 10) ? row - (mapHeightBound - 19) : row - (Demo.cursorY - 10)));
                    //   //(cursorY > mapHeight - 10) ? mapHeight - (cursorY - 10) : cursorY - 10)
                    // +tileHIncrease; //row - (cursorY - 10)

                    //minVisibleX = (cursorX <= 10) ? 0 : (cursorX > mapWidth - 10) ? mapWidth - 19 : cursorX - 10;
                    //maxVisibleX = minVisibleX;
                    for (int col = (Demo.cursorX <= 10) ? 0 : (Demo.cursorX > mapWidthBound - 10) ? mapWidthBound - 19 : Demo.cursorX - 10; col <= mapWidthBound && (col < Demo.cursorX + 10 || col < 20); col++)
                    {
                        // maxVisibleX++;
                        /*
                        if (visibleCells[col, row] <= 0 && seenCells[col, row] <= 0)
                        {
                            pX += tileVIncrease;
                            continue;
                        }
                        else if (visibleCells[col, row] <= 0 && seenCells[col, row] > 0)
                        {
                            var tile = map[row, col];
                            Rectangle src;

                            var dest = new Rectangle(pX, pY, tileWidth, tileHeight);
                            tileset.Color = Chroma.Blend(Color.Gray, mapColors[row, col], 0.2);
                            src = new Rectangle((tile % 38) * tileWidth, (tile / 38) * tileHeight, tileWidth, tileHeight);
                            tileset.Draw(src, dest);
                            tileset.Color = Color.White;
                            pX += tileVIncrease;
                            continue;

                        }
                        */
                        if (map[row, col] == DungeonMap.gr)
                        {

                            //tileset.Color = mapColors[row, col];
                            tileset.Color = Chroma.Blend(mapColors[row, col], Color.Gray, 1.0 - visibleCells[row, col]);
                            if (Demo.highlightingOn)
                            {
                                if (highlightedTargetCells.ContainsKey(new Point(col, row)))
                                {
                                    tileset.Color = Color.FromHsv(10.0, 0.7, 0.60 + (((Timing.TotalMilliseconds % 2000) < 1000) ? (Timing.TotalMilliseconds % 2000) / 3000 : (2000 - (Timing.TotalMilliseconds % 2000)) / 3000));
                                }
                                else if (highlightedCells.ContainsKey(new Point(col, row)) && !doNotStopCells.ContainsKey(new Point(col, row)))
                                {
                                    tileset.Color = Color.FromHsv(230.0, 0.7, 0.80 + (((Timing.TotalMilliseconds % 2000) < 1000) ? (Timing.TotalMilliseconds % 2000) / 6000 : (2000 - (Timing.TotalMilliseconds % 2000)) / 6000));
                                }
                                else if (o_entities.ContainsKey(requestingMove))// && o_entities[requestingMove].fov.sight[col, row] == 0)
                                {
                                    tileset.Color = Chroma.Blend(mapColors[row, col], Color.Gray, 1.0 - o_entities[requestingMove].fov.sight[row, col]);
                                }
                            }
                            var dest = new Rectangle(pX, pY, tileWidth, tileHeight);
                            var tile = map[row, col];
                            var src = new Rectangle((tile % 38) * tileWidth, (tile / 38) * tileHeight, tileWidth, tileHeight);
                            tileset.Draw(src, dest);
                            tileset.Color = Color.White;
                        }
                        pX += tileVIncrease;
                    }
                    pX = tileHIncrease * (20 - 1 - ((Demo.cursorY <= 10) ? row : (Demo.cursorY > mapHeightBound - 10) ? row - (mapHeightBound - 19) : row - (Demo.cursorY - 10)));
                    for (var col = (Demo.cursorX <= 10) ? 0 : (Demo.cursorX > mapWidthBound - 10) ? mapWidthBound - 19 : Demo.cursorX - 10; col <= mapWidthBound && (col < Demo.cursorX + 10 || col < 20); col++)
                    {
                    /*if (visibleCells[col, row] <= 0)
                    {
                        pX += tileVIncrease;
                        continue;
                    }*/
                        var dest = new Rectangle(pX, pY, tileWidth, tileHeight);
                        var entity = checkPos(col, row);
                        var fixture = checkFixture(col, row);
                        if (Demo.cursorX == col && Demo.cursorY == row && Demo.lockState && !Demo.lockForAnimation)
                        {
                            if (Demo.lockState)
                                tileset.Color = Color.FromHsv((Timing.TotalMilliseconds % 1800) / 5.0, 1.0, 1.0);
                            int offset = 0;
                            if (entity != null && entity.x == Demo.cursorX && entity.y == Demo.cursorY)
                                offset = (int)(((Timing.TotalMilliseconds % 2000) < 1000) ? (Timing.TotalMilliseconds % 2000) / 40 : (2000 - (Timing.TotalMilliseconds % 2000)) / 40);
                            int cursorTile = 1442;
                            if (entity != null)
                                cursorTile = 1441;
                            tileset.Draw(new Rectangle((cursorTile % 38) * tileWidth, (cursorTile / 38) * tileHeight, tileWidth, tileHeight), new Rectangle(pX, pY - offset, tileWidth, tileHeight));
                            tileset.Color = Color.White;
                        }
                        var tile = map[row, col];
                        Rectangle src;
                        if (tile != DungeonMap.gr)
                        {
                            //tileset.Color = mapColors[row, col];
                            tileset.Color = Chroma.Blend(mapColors[row, col], Color.Gray, 1.0 - visibleCells[row, col]);
                            if (Demo.highlightingOn && o_entities.ContainsKey(requestingMove))// && !o_entities[requestingMove].fov.sightSet.Contains(new Point(col, row)))
                            {
                                tileset.Color = Chroma.Blend(mapColors[row, col], Color.Gray, 1.0 - o_entities[requestingMove].fov.sight[row, col]);
                                //tileset.Color = Chroma.Blend(Color.Gray, mapColors[row, col], 0.2);
                            }
                            src = new Rectangle((tile % 38) * tileWidth, (tile / 38) * tileHeight, tileWidth, tileHeight);
                            tileset.Draw(src, dest);
                            tileset.Color = Color.White;

                        }
                        if (entity != null && visibleCells[row, col] > 0)
                        {
                            tile = entity.tile;
                            src = new Rectangle((tile % 38) * tileWidth, (tile / 38) * tileHeight, tileWidth, tileHeight);
                            Color tsc;// = Color.FromHsv((Timing.TotalMilliseconds % 1800) / 5.0, 0.5, 1.0);
                            //if (Demo.lockState && entity.friendly && requestingMove.X == entity.o_pos.X && requestingMove.Y == entity.o_pos.Y)
                           // {
                                tileset.Color = entity.color;
                                tileset.Draw(src, dest);
                                tileset.Color = Color.White;


                            if (Demo.cursorX == col && Demo.cursorY == row && Demo.lockState && !Demo.lockForAnimation)
                            {
                                if (Demo.lockState)
                                    tileset.Color = Color.FromHsv((Timing.TotalMilliseconds % 1800) / 5.0, 1.0, 1.0);
                                int offset = 0;
                                if (entity.x == Demo.cursorX && entity.y == Demo.cursorY)
                                    offset = (int)(((Timing.TotalMilliseconds % 2000) < 1000) ? (Timing.TotalMilliseconds % 2000) / 40 : (2000 - (Timing.TotalMilliseconds % 2000)) / 40);
                                tileset.Draw(new Rectangle((1440 % 38) * tileWidth, (1440 / 38) * tileHeight, tileWidth, tileHeight), new Rectangle(pX, pY - offset, tileWidth, tileHeight));
                                tileset.Color = Color.White;
                            }
                            if (entity.friendly)
                            {
                                src = new Rectangle((1443 % 38) * tileWidth, (1443 / 38) * tileHeight, tileWidth, tileHeight);
                                tileset.Draw(src, dest);
                            }
                            if (Demo.showHealth)
                            {
                                if (entity.health < 10)
                                {
                                    if (entity.friendly)
                                        tsc = Color.DarkBlue;
                                    else
                                        tsc = Color.Black;//Color.FromHsv((Timing.TotalMilliseconds % 3600) / 5.0, 0.8, 1.0);
                                    mandrillFont.Color = tsc;
                                    //     mandrillFont.Alpha = ((Timing.TotalMilliseconds % 2000) < 1000) ? (Timing.TotalMilliseconds % 2000) / 1000.0 : (2000 - (Timing.TotalMilliseconds % 2000)) / 1000;
                                    mandrillFont.DrawText(pX + 18, pY + 16, "" + entity.health);
                                }
                                if (entity.health >= 10 && entity.health < 100)
                                {
                                    if (entity.friendly)
                                        tsc = Color.DarkBlue;
                                    else
                                        tsc = Color.Black;//Color.FromHsv((Timing.TotalMilliseconds % 3600) / 5.0, 0.8, 1.0);
                                    mandrillFont.Color = tsc;
                                    //       mandrillFont.Alpha = ((Timing.TotalMilliseconds % 2000) < 1000) ? (Timing.TotalMilliseconds % 2000) / 1700.0 : (2000 - (Timing.TotalMilliseconds % 2000)) / 1700;
                                    mandrillFont.DrawText(pX + 12, pY + 16, "" + entity.health);
                                }
                            }
                        }
                        else if (fixture != null)
                        {

                            int frontTile = 1437;
                            int backTile = 1436;

                            if (Demo.cursorX == col && Demo.cursorY == row && Demo.lockState)
                            {
                                if (Demo.lockState)
                                    tileset.Color = Color.FromHsv((Timing.TotalMilliseconds % 1800) / 5.0, 1.0, 1.0);
                                int offset = 0;
                                tileset.Draw(new Rectangle((backTile % 38) * tileWidth, (backTile / 38) * tileHeight, tileWidth, tileHeight), new Rectangle(pX, pY - offset, tileWidth, tileHeight));
                                tileset.Color = Color.White;
                            }
                            tileset.Color = fixture.color;// Chroma.Blend(mapColors[row, col], Color.Black, 1.0 - visibleCells[row, col]);
                            tile = fixture.tile;
                            src = new Rectangle((tile % 38) * tileWidth, (tile / 38) * tileHeight, tileWidth, tileHeight);
                            tileset.Draw(src, dest);
                            tileset.Color = Color.White;
                            if (Demo.cursorX == col && Demo.cursorY == row && Demo.lockState)
                            {
                                if (Demo.lockState)
                                    tileset.Color = Color.FromHsv((Timing.TotalMilliseconds % 1800) / 5.0, 1.0, 1.0);
                                int offset = 0;
                                tileset.Draw(new Rectangle((frontTile % 38) * tileWidth, (frontTile / 38) * tileHeight, tileWidth, tileHeight), new Rectangle(pX, pY - offset, tileWidth, tileHeight));
                                tileset.Color = Color.White;
                            }

                        }
                        //     mandrillFont.Alpha = ((Timing.TotalMilliseconds % 2000) < 1000) ? (Timing.TotalMilliseconds % 2000) / 1000.0 : (2000 - (Timing.TotalMilliseconds % 2000)) / 1000;
                        //       mandrillFont.Alpha = ((Timing.TotalMilliseconds % 2000) < 1000) ? (Timing.TotalMilliseconds % 2000) / 1700.0 : (2000 - (Timing.TotalMilliseconds % 2000)) / 1700;
                        foreach (Point cl in displayDamage.Keys)
                        {
                            if (cl.Y == row && cl.X == col)
                            {
                                if (displayDamage[cl] < 10)
                                {
                                    int offset = (int)(((Timing.TotalMilliseconds % 2000) < 1000) ? (Timing.TotalMilliseconds % 2000) / 50 : (2000 - (Timing.TotalMilliseconds % 2000)) / 50);
                                    mandrillFont.Color = (Timing.TotalMilliseconds % 100 < 50) ? Color.Black : Color.White;
                                    mandrillFont.DrawText(pX + 18, pY + 16 - offset, "" + displayDamage[cl]);
                                    mandrillFont.Color = Color.White;
                                }
                                else if (displayDamage[cl] >= 10 && displayDamage[cl] < 100)
                                {
                                    int offset = (int)(((Timing.TotalMilliseconds % 2000) < 1000) ? (Timing.TotalMilliseconds % 2000) / 40 : (2000 - (Timing.TotalMilliseconds % 2000)) / 40);
                                    mandrillFont.Color = (Timing.TotalMilliseconds % 100 < 50) ? Color.Black : Color.White; //Color.DarkRed;
                                    mandrillFont.DrawText(pX + 12, pY + 16 - offset, "" + displayDamage[cl]);
                                    mandrillFont.Color = Color.White;
                                }
                            }
                        }
                        foreach (Point cl in displayKills.Keys)
                        {
                            if (cl.Y == row && cl.X == col && displayKills[cl])
                            {
                                int offset = (int)(((Timing.TotalMilliseconds % 4000) < 2000) ? (Timing.TotalMilliseconds % 4000) / 100 : (4000 - (Timing.TotalMilliseconds % 4000)) / 100);
                                mandrillFont.Color = (Timing.TotalMilliseconds % 100 < 50) ? Color.Black : Color.White;
                                mandrillFont.DrawText(pX, pY + 16 - offset, "DEAD");
                                mandrillFont.Color = Color.White;

                            }
                        }

                        pX += tileVIncrease;
                    }
                    //pY += tileHIncrease;
                }
                /*ScreenBrowser.Show();
                if (Demo.hoverActor != null)
                    UnitInfo.ShowMobInfo(Demo.hoverActor);
                else if (Demo.currentActor != null)
                    UnitInfo.ShowMobInfo(Demo.currentActor);
                Display.FillRect(new Rectangle(0, Demo.mapDisplayHeight, Demo.mapDisplayWidth, 32), (Color.Black));
                DialogBrowser.Show();
                MessageBrowser.Show();*/
                //mandrillFont.DrawText(32.0, 32.0, "FPS: " + (int)Display.FramesPerSecond);
            }




            //        private static bool hasPressedSpace = false;
            
            
        
    }
}
