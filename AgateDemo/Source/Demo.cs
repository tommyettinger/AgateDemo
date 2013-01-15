
using System;
using System.Linq;
using System.Collections.Generic;
using AgateLib;
using AgateLib.DisplayLib;
using AgateLib.Geometry;
using AgateLib.InputLib;
using System.Runtime.Serialization;
/*using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;*/

using ProtoBuf;
using System.ComponentModel;
using ProtoBuf.Meta;

namespace AgateDemo
{
    public enum Direction { North, East, South, West, None };
    public enum InputMode { Menu, Map, None, Dialog };
    public static class Demo
    {
        // public static Color[,] mapColors;
        public static Random rnd = new Random();
        //public static int minVisibleX = 0, minVisibleY = 0, maxVisibleX = 19, maxVisibleY = 19;

        public static InputMode mode = InputMode.None;
        //public static List<Direction> dirlist = new List<Direction>();
        public class Entity
        {
            public int tile;
            public Point pos;
            public int x
            {
                get { return pos.X; }
                set { pos.X = value; }
            }
            public int y
            {
                get { return pos.Y; }
                set { pos.Y = value; }
            }
            public Entity(int tile, int x, int y)
            {
                this.tile = tile;
                this.pos = new Point(x, y);
                this.x = x;
                this.y = y;
            }
            public Entity()
            {
                pos = new Point();
            }
        }
        public class Mob : Entity
        {
            public int hp_internal;
            public int maxHP = 10;
            public int health
            {
                get { return hp_internal; }
                set
                {
                    hp_internal = value;
                    if (hp_internal <= 0)
                        this.Kill();
                    else if (hp_internal > maxHP)
                        hp_internal = maxHP;
                }
            }
            public bool friendly;
            public Point o_pos;
            public string name;
            public List<Direction> moveList = new List<Direction>();
            public int moveSpeed = 3, actionCount = 0;
            public List<Skill> skillList;
            public Skill currentSkill;
            //[JsonIgnore]
            public SimpleUI ui = null;
            //[JsonIgnore]
            public Vision fov = null;
            //[JsonIgnore]
            public Level dlevel = null;
            public int dlevelIndex = 0;
            public Intellect intel = null;
            public Mob(int tileNumber, int xPos, int yPos, bool isFriendly, int floor)
            {
                base.tile = tileNumber;
                base.pos = new Point(xPos, yPos);
                tile = tileNumber;
                name = TileData.tilesToNames[tileNumber];
                x = xPos;
                y = yPos;
                o_pos = new Point() { X = this.x, Y = this.y };
                friendly = isFriendly;
                dlevelIndex = floor;
                dlevel = fullDungeon[floor];
                fov = new Vision(this);
                fov.calculateSight();

                if (friendly)
                {
                    //allies.Add(o_pos, this);
                    //recalculateVision();
                    moveSpeed = 6;
                }
                else
                {
                    intel = new Intellect(this);
                }
                ui = SimpleUI.InitUI();
                //hasActed = false;
                health = 10;
                skillList = new List<Skill>() { };
                currentSkill = null;
                //initiative = rnd.Next(1000);
            }

            public Mob Clone()
            {
                Mob mb = new Mob(this.tile, this.x, this.y, this.friendly, this.dlevelIndex);
                //mb.initiative = this.initiative;
                mb.hp_internal = this.hp_internal;
                mb.o_pos = this.o_pos;
                mb.actionCount = 2;
                return mb;
            }
            public void Kill()
            {
                dlevel.entities.Remove(this.pos);
                dlevel.o_entities.Remove(this.o_pos);
                if (this.friendly)
                {
                    dlevel.allies.Remove(this.o_pos);
                    currentLevel.recalculateVision();
                }
                try
                {
                    initiative.Remove(initiative.First(e => e.Value == this.o_pos).Key);
                }
                catch (Exception)
                { }
            }

            [OnDeserialized]
            internal void OnDeserializedMethod(StreamingContext context)
            {
                dlevel = fullDungeon[dlevelIndex];
                fov = new Vision(this);
                fov.calculateSight();

                ui = SimpleUI.InitUI();
            }
        }
        public static SortedDictionary<int, Point> initiative = new SortedDictionary<int, Point>();
        public static int currentInitiative;
        public static bool lockState = false, lockForAnimation = false, showHealth = false, highlightingOn = false, hjkl = true;
        //currentlyPerformingMenuEvent = false, 
        public static Point requestingMove = new Point() { X = -1, Y = -1 };
        private static Mob currentActorInternal = null, hoverActorInternal = null;
        public static Mob currentActor
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
        }
        static Surface tileset;

        public const int mapDisplayWidth = 960, mapDisplayHeight = 672;
        public static Level currentLevel;
        public static List<Level> fullDungeon;
        public static int levelIndex = 0;

        public class GameState
        {
            public SortedDictionary<int, Point> initiative;
            public int currentInitiative;
//            public Level currentLevel;
            public List<Level> fullDungeon;
            public int levelIndex;
            public int cursorX;
            public int cursorY;
        }
        public static SortedDictionary<DateTime, GameState> allSavedStates = null;
        public static SortedDictionary<DateTime, GameState> getState()
        {
            SortedDictionary<DateTime, GameState> ds;

            if (System.IO.File.Exists("save.mobsav"))
            {
                System.IO.FileStream fl = new System.IO.FileStream("save.mobsav", System.IO.FileMode.Open);
                ds = (SortedDictionary<DateTime, GameState>)Serializer.model.Deserialize(fl, null, typeof(SortedDictionary<DateTime, GameState>));

                fl.Close();
            }
            else
            {
                ds = new SortedDictionary<DateTime, GameState>();
                System.IO.File.Create("save.mobsav").Close();
            }
            ds.Add(DateTime.Now, new GameState()
            {
                initiative = initiative,
                currentInitiative = currentInitiative,
                fullDungeon = fullDungeon,
                levelIndex = levelIndex,
                cursorX = cursorX,
                cursorY = cursorY
            });
            return ds;
        }
        public static void LoadStates()
        {/*
            var jsonSerializerSettings = new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.All
            };*/
            /*
            RuntimeTypeModel rttm = RuntimeTypeModel.Create();
            rttm.AutoAddMissingTypes = true;
            rttm.Add(typeof(GameState), true);*/
            //GameState s = null;
            allSavedStates = (SortedDictionary<DateTime, GameState>)Serializer.model.Deserialize(new System.IO.FileStream("save.mobsav", System.IO.FileMode.Open), null, typeof(SortedDictionary<DateTime, GameState>));
           // GameState s = //JsonConvert.DeserializeObject<GameState>(System.IO.File.ReadAllText("save.mobsav"), jsonSerializerSettings);

            mode = InputMode.Dialog;
            DialogBrowser.currentUI = DialogUI.CreateLoadGameDialog(allSavedStates);
            DialogBrowser.UnHide();
        }
        public static void LoadGame()
        {
            GameState s = allSavedStates.ToList()[DialogBrowser.currentUI.currentDialog.currentOption].Value;
            initiative = s.initiative;
            currentInitiative = s.currentInitiative;

            fullDungeon = s.fullDungeon;
            foreach (Level l in fullDungeon)
            {
                l.map = new int[l.mapHeightBound + 1, l.mapWidthBound + 1];
                for (int i = 0; i <= l.mapHeightBound; i++)
                {
                    for (int j = 0; j <= l.mapWidthBound; j++)
                    {
                        l.map[i, j] = l.map1D[i * (1 + l.mapWidthBound) + j];
                    }
                }

                int mw = l.map.GetLength(1), mh = l.map.GetLength(0);

                l.visibleCells = new double[mh, mw];
                l.seenCells = new double[mh, mw];
                l.mapColors = DungeonMap.recolor(l.map);
            }
            foreach(Level l in fullDungeon)
            {
                l.allies = new Dictionary<Point, Mob>();
                foreach (Mob mb in l.entities.Values)
                {
                    mb.dlevel = l;
                    mb.moveList = new List<Direction>();
                    mb.currentSkill = mb.skillList[0];
                    mb.fov = new Vision(mb);
                    mb.fov.calculateSight();

                    mb.ui = SimpleUI.InitUI();
                    mb.ui.addSkills(mb);
                    
                    l.o_entities[mb.o_pos] = mb;
                    if (mb.friendly)
                    {
                        l.allies.Add(mb.pos, mb);
                    }
                    else
                    {
                        mb.intel = new Intellect(mb);
                    }
                }
            }
            foreach (Level l in fullDungeon)
            {
                l.recalculateVision();
            }
            levelIndex = s.levelIndex;
            currentLevel = fullDungeon[levelIndex];
            cursorX = s.cursorX;
            cursorY = s.cursorY;
            
        }
        //public static int[,] map, map2;
        //public static Dictionary<Point, Mob> entities, currentLevel.o_entities, currentLevel.allies;
        //        public static Dictionary<Point, int> highlightedCells = new Dictionary<Point, int>(), highlightedTargetCells = new Dictionary<Point, int>(), nonHighlightedFreeCells = new Dictionary<Point, int>();
        //public static Dictionary<Point, bool> doNotStopCells = new Dictionary<Point, bool>();
        //public static double[,] visibleCells, seenCells;
        // public static Dictionary<Point, Entity> fixtures;

        public static Dictionary<Point, int> displayDamage = new Dictionary<Point, int>();
        public static Dictionary<Point, bool> displayKills = new Dictionary<Point, bool>();
        // public static SimpleUI basicUI;
        /*
        static int tileWidth = 48;
        static int tileHeight = 64;
        static int tileHIncrease = 16;
        static int tileVIncrease = 32;*/
//        public static int mapWidth;
  //      public static int mapHeight;
        public static int cursorX = 3;
        public static int cursorY = 3;
        static FontSurface mandrillFont;

        static DisplayWindow wind;

        public static void Shuffle<T>(T[] array)
        {
            Random random = rnd;
            for (int i = array.Length; i > 1; i--)
            {
                // Pick random element to swap.
                int j = random.Next(i); // 0 <= j <= i-1
                // Swap.
                T tmp = array[j];
                array[j] = array[i - 1];
                array[i - 1] = tmp;
            }
        }
        /*
        public static void recalculateVision()
        {
            currentLevel.visibleCells.Fill(indices => 0.0);
            foreach (Mob mb in currentLevel.allies.Values)
            {
                mb.fov.calculateSight();
                currentLevel.visibleCells.Fill(indices => (((double)mb.fov.sight.GetValue(indices) > (double)currentLevel.visibleCells.GetValue(indices)) ? (double)mb.fov.sight.GetValue(indices) : (double)currentLevel.visibleCells.GetValue(indices)));
            }
            currentLevel.seenCells.Fill(indices => (((double)currentLevel.visibleCells.GetValue(indices) > 0.0 || (double)currentLevel.seenCells.GetValue(indices) > 0.0) ? 1.0 : 0.0));//UnionWith(visibleCells);
        }*/
        public static Mob checkPos(int checkX, int checkY)
        {
            Mob ret = null;
            currentLevel.entities.TryGetValue(new Point() { X = checkX, Y = checkY }, out ret);
            return ret;
        }
        public static Mob checkPos(int checkX, int checkY, int checkTile)
        {
            Mob ret = null;
            currentLevel.entities.TryGetValue(new Point() { X = checkX, Y = checkY }, out ret);
            if (ret != null && ret.tile != checkTile)
                ret = null;
            return ret;
        }
        public static Entity checkFixture(int checkX, int checkY)
        {
            Entity ret = null;
            currentLevel.fixtures.TryGetValue(new Point() { X = checkX, Y = checkY }, out ret);
            return ret;
        }
        public static Entity checkFixture(int checkX, int checkY, int checkTile)
        {
            Entity ret = null;
            currentLevel.fixtures.TryGetValue(new Point() { X = checkX, Y = checkY }, out ret);
            if (ret != null && ret.tile != checkTile)
                ret = null;
            return ret;
        }

        public static void MoveCursor(int x, int y)
        {
            List<Point> cursorLine = Line.LineHook(new Point(cursorX, cursorY), new Point(x, y));
            //currentActor = null;
            lockForAnimation = true;
            for (int i = 0; i < cursorLine.Count; i++)
            {
                Point pt = cursorLine[i];
                double startingTime = Timing.TotalMilliseconds;
                //for (int row = (cursorY < 20) ? 0 : (cursorY > mapHeight - 10) ? mapHeight - 20 : cursorY - 20; row <= mapHeight && row <= cursorY + 20; row++)
                //for (int col = (cursorX <= 10) ? 0 : (cursorX > mapWidth - 10) ? mapWidth - 19 : cursorX - 10; col <= mapWidth && (col < cursorX + 10 || col < 20); col++)
                int waitAmount = 50;//(int)Math.Floor(50.0 - ((cursorLine.Count / 2 > i) ? i / (cursorLine.Count / 50.0) : (25) - i / (cursorLine.Count / 10.0)));
                //if ((pt.Y < 20 || pt.Y > mapHeight - 10 || pt.X <= 10 || pt.X > mapWidth - 10))
                //  waitAmount = 50;
                while (Timing.TotalMilliseconds - startingTime < waitAmount)
                {
                    System.Threading.Thread.Sleep(25);
                    if (Display.CurrentWindow.IsClosed)
                        return;
                    Display.BeginFrame();
                    Show();
                    Display.EndFrame();
                    try
                    {
                        Core.KeepAlive();
                    }
                    catch (Exception) { };
                }

                cursorX = pt.X;
                cursorY = pt.Y;
            }
            double startTime = Timing.TotalMilliseconds;
            while (Timing.TotalMilliseconds - startTime < 110)
            {
                System.Threading.Thread.Sleep(25);
                if (Display.CurrentWindow.IsClosed)
                    return;
                Display.BeginFrame();
                Show();
                Display.EndFrame();
                try
                {
                    Core.KeepAlive();
                }
                catch (Exception) { }
            }
            cursorX = x;
            cursorY = y;
            lockForAnimation = false;
        }

        public static void AnimateResults(SkillResult skr)
        {/*
            minVisibleY = (cursorY < 20) ? 0 : (cursorY > mapHeight - 10) ? mapHeight - 20 : cursorY - 20;
            maxVisibleY = minVisibleY;
            for (int row = (cursorY < 20) ? 0 : (cursorY > mapHeight - 10) ? mapHeight - 20 : cursorY - 20; row <= mapHeight && row <= cursorY + 20; row++)
            {
                maxVisibleY++;

            }
            minVisibleX = (cursorX <= 10) ? 0 : (cursorX > mapWidth - 10) ? mapWidth - 19 : cursorX - 10;
            maxVisibleX = minVisibleX;
            for (var col = (cursorX <= 10) ? 0 : (cursorX > mapWidth - 10) ? mapWidth - 19 : cursorX - 10; col <= mapWidth && (col < cursorX + 10 || col < 20); col++)
            {
                maxVisibleX++;
            }*/
            foreach (Point c in skr.damages.Keys)
            {
                //while (Timing.TotalMilliseconds - startingTime < 200 && (ent.x >= minVisibleX && ent.x <= maxVisibleX && ent.y >= minVisibleY && ent.y <= maxVisibleY)) ;
                if (currentLevel.visibleCells[c.Y, c.X] > 0)
                //if (c.X >= minVisibleX && c.X <= maxVisibleX && c.Y >= minVisibleY && c.Y <= maxVisibleY)
                {
                    //currentActor = null;
                    lockForAnimation = true;
                    try
                    {
                        currentLevel.displayDamage.Add(c, skr.damages[c]);
                    }
                    catch (Exception) { }
                }
            }
            double startingTime = Timing.TotalMilliseconds;
            if (currentLevel.displayDamage.Count > 0)
            {
                while (Timing.TotalMilliseconds - startingTime < 500)
                {
                    System.Threading.Thread.Sleep(25);
                    if (Display.CurrentWindow.IsClosed)
                        return;
                    Display.BeginFrame();
                    Show();
                    Display.EndFrame();
                    Core.KeepAlive();
                }
                currentLevel.displayDamage.Clear();
            }
            startingTime = Timing.TotalMilliseconds;
            foreach (Point c in skr.kills.Keys)
            {
                //while (Timing.TotalMilliseconds - startingTime < 200 && (ent.x >= minVisibleX && ent.x <= maxVisibleX && ent.y >= minVisibleY && ent.y <= maxVisibleY)) ;
                if (currentLevel.visibleCells[c.Y, c.X] > 0)
                {
                    //currentActor = null;
                    lockForAnimation = true;
                    currentLevel.displayKills.Add(c, skr.kills[c]);
                }
            }
            if (currentLevel.displayKills.Count > 0)
            {
                while (Timing.TotalMilliseconds - startingTime < 1000)
                {
                    System.Threading.Thread.Sleep(25);
                    if (Display.CurrentWindow.IsClosed)
                        return;
                    Display.BeginFrame();
                    Show();
                    Display.EndFrame();
                    Core.KeepAlive();
                }
                currentLevel.displayKills.Clear();
            }
            if (Display.CurrentWindow.IsClosed)
                return;
            Display.BeginFrame();
            Show();
            Display.EndFrame();

            //cursorX = ent.x;
            //cursorY = ent.y;

            lockForAnimation = false;
        }
        public static void MoveMob(Mob ent, IEnumerable<Direction> movepath)
        {
            Dictionary<Point, Mob> displaced = new Dictionary<Point, Mob>();
            if (currentLevel.visibleCells[ent.y, ent.x] > 0)
            {
                MoveCursor(ent.x, ent.y);
            }
            double startingTime = Timing.TotalMilliseconds;
            if (currentLevel.visibleCells[ent.y, ent.x] > 0)
            {
                currentActor = ent;
                lockForAnimation = true;
                while (Timing.TotalMilliseconds - startingTime < 300)
                {
                    System.Threading.Thread.Sleep(25);
                    if (Display.CurrentWindow.IsClosed)
                        return;
                    Display.BeginFrame();
                    Show();
                    Display.EndFrame();
                    Core.KeepAlive();
                }
            }
            startingTime = Timing.TotalMilliseconds;
            foreach (Direction currMove in movepath)
            {

                //while (Timing.TotalMilliseconds - startingTime < 200 && (ent.x >= minVisibleX && ent.x <= maxVisibleX && ent.y >= minVisibleY && ent.y <= maxVisibleY)) ;
                //if (ent.x >= minVisibleX && ent.x <= maxVisibleX && ent.y >= minVisibleY && ent.y <= maxVisibleY)
                if (currentLevel.visibleCells[ent.y, ent.x] > 0)
                {
                    currentActor = ent;
                    lockForAnimation = true;
                    while (Timing.TotalMilliseconds - startingTime < 200)
                    {
                        System.Threading.Thread.Sleep(25);
                        if (Display.CurrentWindow.IsClosed)
                            return;
                        Display.BeginFrame();
                        Show();
                        Display.EndFrame();
                        try
                        {
                            Core.KeepAlive();
                        }
                        catch (Exception) { };
                    }
                }
                startingTime = Timing.TotalMilliseconds;
                currentLevel.entities.Remove(ent.pos);

                switch (currMove)
                {
                    case Direction.West: if (ent.x > 0 && (currentLevel.map[ent.y, ent.x - 1] == 1194) && checkPos(ent.x - 1, ent.y) == null) //entities.FirstOrDefault(e => e.x == ent.x - 1 && e.y == ent.y)
                        {
                            if (displaced.ContainsKey(ent.pos))
                                currentLevel.entities.Add(ent.pos, displaced[ent.pos]);
                            ent.x--;
                        }
                        else if (ent.x > 0 && (currentLevel.map[ent.y, ent.x - 1] == 1187) && checkFixture(ent.x - 1, ent.y, 1190) != null)
                        {
                            currentLevel.map[ent.y, ent.x - 1] = 1194;
                            currentLevel.fixtures[new Point() { X = ent.x - 1, Y = ent.y }].tile = 1188;
                        }
                        else if (ent.x > 0 && (currentLevel.map[ent.y, ent.x - 1] == 1194) && checkPos(ent.x - 1, ent.y) != null && checkPos(ent.x - 1, ent.y).friendly == ent.friendly)
                        {
                            if (displaced.ContainsKey(ent.pos))
                                currentLevel.entities.Add(ent.pos, displaced[ent.pos]);
                            displaced.Add(checkPos(ent.x - 1, ent.y).pos, checkPos(ent.x - 1, ent.y));
                            ent.x--;
                        }
                        break;
                    case Direction.North: if (ent.y > 0 && (currentLevel.map[ent.y - 1, ent.x] == 1194) && checkPos(ent.x, ent.y - 1) == null)
                        {
                            if (displaced.ContainsKey(ent.pos))
                                currentLevel.entities.Add(ent.pos, displaced[ent.pos]);
                            ent.y--;
                        }
                        else if (ent.y > 0 && (currentLevel.map[ent.y - 1, ent.x] == 1187) && checkFixture(ent.x, ent.y - 1, 1191) != null)
                        {
                            currentLevel.map[ent.y - 1, ent.x] = 1194;
                            currentLevel.fixtures[new Point() { X = ent.x, Y = ent.y - 1 }].tile = 1189;
                            //                        fixtures.FirstOrDefault(e => e.x == ent.x && e.y == ent.y - 1 && e.tile == 1191).tile = 1189;
                        }
                        else if (ent.y > 0 && (currentLevel.map[ent.y - 1, ent.x] == 1194) && checkPos(ent.x, ent.y - 1) != null && checkPos(ent.x, ent.y - 1).friendly == ent.friendly)
                        {
                            if (displaced.ContainsKey(ent.pos))
                                currentLevel.entities.Add(ent.pos, displaced[ent.pos]);
                            displaced.Add(checkPos(ent.x, ent.y - 1).pos, checkPos(ent.x, ent.y - 1));
                            ent.y--;
                        }
                        break;
                    case Direction.East: if (ent.x < currentLevel.mapWidthBound && (currentLevel.map[ent.y, ent.x + 1] == 1194) && checkPos(ent.x + 1, ent.y) == null) //entities.FirstOrDefault(e => e.x == ent.x - 1 && e.y == ent.y)
                        {
                            if (displaced.ContainsKey(ent.pos))
                                currentLevel.entities.Add(ent.pos, displaced[ent.pos]);
                            ent.x++;
                        }
                        else if (ent.x < currentLevel.mapWidthBound && (currentLevel.map[ent.y, ent.x + 1] == 1187) && checkFixture(ent.x + 1, ent.y, 1190) != null)
                        {
                            currentLevel.map[ent.y, ent.x + 1] = 1194;
                            currentLevel.fixtures[new Point() { X = ent.x + 1, Y = ent.y }].tile = 1188;
                        }
                        else if (ent.x < currentLevel.mapWidthBound && (currentLevel.map[ent.y, ent.x + 1] == 1194) && checkPos(ent.x + 1, ent.y) != null && checkPos(ent.x + 1, ent.y).friendly == ent.friendly)
                        {
                            if (displaced.ContainsKey(ent.pos))
                                currentLevel.entities.Add(ent.pos, displaced[ent.pos]);
                            displaced.Add(checkPos(ent.x + 1, ent.y).pos, checkPos(ent.x + 1, ent.y));
                            ent.x++;
                        }
                        break;
                    case Direction.South: if (ent.y < currentLevel.mapHeightBound && (currentLevel.map[ent.y + 1, ent.x] == 1194) && checkPos(ent.x, ent.y + 1) == null)
                        {
                            if (displaced.ContainsKey(ent.pos))
                                currentLevel.entities.Add(ent.pos, displaced[ent.pos]);
                            ent.y++;
                        }
                        else if (ent.y < currentLevel.mapHeightBound && (currentLevel.map[ent.y + 1, ent.x] == 1187) && checkFixture(ent.x, ent.y + 1, 1191) != null)
                        {
                            currentLevel.map[ent.y + 1, ent.x] = 1194;
                            currentLevel.fixtures[new Point() { X = ent.x, Y = ent.y + 1 }].tile = 1189;
                        }
                        else if (ent.y < currentLevel.mapHeightBound && (currentLevel.map[ent.y + 1, ent.x] == 1194) && checkPos(ent.x, ent.y + 1) != null && checkPos(ent.x, ent.y + 1).friendly == ent.friendly)
                        {
                            if (displaced.ContainsKey(ent.pos))
                                currentLevel.entities.Add(ent.pos, displaced[ent.pos]);
                            displaced.Add(checkPos(ent.x, ent.y + 1).pos, checkPos(ent.x, ent.y + 1));
                            ent.y++;
                        }
                        break;
                }
                currentLevel.entities[ent.pos] = ent;
                if (currentLevel.visibleCells[ent.y, ent.x] > 0)
                {
                    cursorX = ent.x;
                    cursorY = ent.y;
                }
                if (ent.friendly)
                {
                    ent.fov.calculateSight();
                    currentLevel.recalculateVision();
                }
                requestingMove.X = ent.o_pos.X;
                requestingMove.Y = ent.o_pos.Y;
                if (Display.CurrentWindow.IsClosed)
                    return;
                Display.BeginFrame();
                Show();
                Display.EndFrame();
            }
            //cursorX = ent.x;
            //cursorY = ent.y;

            lockForAnimation = false;

            if (ent.friendly && currentLevel.checkFixture(ent.x, ent.y, 1198) != null)
            {
                mode = InputMode.Dialog;
                DialogBrowser.currentUI = DialogUI.CreateYesNoDialog("The Narrator", new List<string>() { "Descend deeper into the dungeon?" }, Descend, DialogBrowser.Hide);
                DialogBrowser.UnHide();
            }
            else if (ent.friendly && currentLevel.checkFixture(ent.x, ent.y, 1197) != null && levelIndex == 0)
            {
                mode = InputMode.Dialog;
                DialogBrowser.currentUI = DialogUI.CreateYesNoDialog("The Narrator", new List<string>() { "Leave the dungeon?", "This will end your dungeon adventure." }, Quit, DialogBrowser.Hide);
                DialogBrowser.UnHide();
            }
            else if (ent.friendly && currentLevel.checkFixture(ent.x, ent.y, 1197) != null && levelIndex != 0)
            {
                mode = InputMode.Dialog;
                DialogBrowser.currentUI = DialogUI.CreateYesNoDialog("The Narrator", new List<string>() { "Ascend closer to the surface?" }, Ascend, DialogBrowser.Hide);
                DialogBrowser.UnHide();
            }
        }
        public static void Descend()
        {
            if (levelIndex + 1 >= fullDungeon.Count)
            {
                return;
            }
            List<Mob> currAllies = currentLevel.allies.Values.ToList();
            for (int i = 0; i < currAllies.Count && i < fullDungeon[levelIndex + 1].safeUpCells.Count; i++)
            {
                fullDungeon[levelIndex + 1].entities.Add(fullDungeon[levelIndex + 1].safeUpCells[i], currAllies[i]);
                currentLevel.entities.Remove(currAllies[i].pos);
                fullDungeon[levelIndex + 1].o_entities.Add(fullDungeon[levelIndex + 1].safeUpCells[i], currAllies[i]);
                currentLevel.o_entities.Remove(currAllies[i].o_pos);
            }
            levelIndex++;

            foreach (Point c in fullDungeon[levelIndex].o_entities.Keys)
            {
                fullDungeon[levelIndex].o_entities[c].o_pos = c;
                fullDungeon[levelIndex].o_entities[c].dlevel = fullDungeon[levelIndex];
            }

            foreach (Point c in fullDungeon[levelIndex].entities.Keys)
            {
                fullDungeon[levelIndex].entities[c].x = c.X;
                fullDungeon[levelIndex].entities[c].y = c.Y;
            }
            currentLevel = fullDungeon[levelIndex];

            initiative.Clear();
            foreach (Point cl in currentLevel.o_entities.Keys)
            {
                currentLevel.o_entities[cl].actionCount = 0;
                int curr = rnd.Next(10000);
                while (initiative.ContainsKey(curr))
                {
                    curr = rnd.Next(10000);
                }
                initiative[curr] = cl;

            }
            currentInitiative = initiative.Keys.Max();

            cursorX = currentLevel.safeUpCells[0].X;
            cursorY = currentLevel.safeUpCells[0].Y;
            currentLevel.allies = currentLevel.o_entities.Where(a => a.Value.friendly == true).ToDictionary(a => a.Key, a => a.Value);
            currentLevel.recalculateVision();
            mode = InputMode.None;
        }
        public static void Ascend()
        {
            if (levelIndex == 0)
            {
                return;
            }
            List<Mob> currAllies = currentLevel.allies.Values.ToList();
            for (int i = 0; i < currAllies.Count && i < fullDungeon[levelIndex - 1].safeDownCells.Count; i++)
            {
                fullDungeon[levelIndex - 1].entities.Add(fullDungeon[levelIndex - 1].safeDownCells[i], currAllies[i]);
                currentLevel.entities.Remove(currAllies[i].pos);
                fullDungeon[levelIndex - 1].o_entities.Add(fullDungeon[levelIndex - 1].safeDownCells[i], currAllies[i]);
                currentLevel.o_entities.Remove(currAllies[i].o_pos);
            }

            levelIndex -= 1;

            foreach (Point c in fullDungeon[levelIndex].o_entities.Keys)
            {
                fullDungeon[levelIndex].o_entities[c].o_pos = c;
                fullDungeon[levelIndex].o_entities[c].dlevel = fullDungeon[levelIndex];
            }

            foreach (Point c in fullDungeon[levelIndex].entities.Keys)
            {
                fullDungeon[levelIndex].entities[c].x = c.X;
                fullDungeon[levelIndex].entities[c].y = c.Y;
            }
            currentLevel = fullDungeon[levelIndex];

            initiative.Clear();
            foreach (Point cl in currentLevel.o_entities.Keys)
            {
                currentLevel.o_entities[cl].actionCount = 0;
                int curr = rnd.Next(10000);
                while (initiative.ContainsKey(curr))
                {
                    curr = rnd.Next(10000);
                }
                initiative[curr] = cl;

            }
            currentInitiative = initiative.Keys.Max();
            cursorX = currentLevel.safeDownCells[0].X;
            cursorY = currentLevel.safeDownCells[0].Y;
            currentLevel.allies = currentLevel.o_entities.Where(a => a.Value.friendly == true).ToDictionary(a => a.Key, a => a.Value);
            currentLevel.recalculateVision();
            mode = InputMode.None;
        }
        public static void MoveDirect(Mob ent, Point targetSquare, Dictionary<Point, int> validMoves, Dictionary<Point, bool> invalidMoves)
        {
            //entities.Remove(ent.pos);
            //Dictionary<Cell, int> validMoves = new Dictionary<Cell, int>() { };
            //Dictionary<Cell, bool> invalidMoves = new Dictionary<Cell, bool>();
            calculateAllMoves(currentLevel, ent.x, ent.y, ent.moveSpeed, true, validMoves, invalidMoves, ent.friendly);

            // Cell targetSquare = validMoves.Keys.Except(invalidMoves.Keys).ToList()[randomPosition]
            Point currentSquare = targetSquare;
            if (!validMoves.ContainsKey(targetSquare))
                return;
            if (invalidMoves.ContainsKey(targetSquare))
                return;
            int currentDist = validMoves[targetSquare];

            Direction[] randir = { Direction.East, Direction.North, Direction.West, Direction.South };
            //Shuffle(randir);
            while (currentDist < ent.moveSpeed)
            {
                foreach (Direction d in randir)
                {
                    switch (d)
                    {
                        case Direction.East:
                            {
                                if (validMoves.ContainsKey(new Point(currentSquare.X + 1, currentSquare.Y)) && validMoves[new Point(currentSquare.X + 1, currentSquare.Y)] > currentDist)
                                {
                                    currentSquare = new Point(currentSquare.X + 1, currentSquare.Y);
                                    currentDist = validMoves[currentSquare];
                                    ent.moveList.Insert(0, Direction.West);
                                }
                                break;
                            }
                        case Direction.West:
                            {
                                if (validMoves.ContainsKey(new Point(currentSquare.X - 1, currentSquare.Y)) && validMoves[new Point(currentSquare.X - 1, currentSquare.Y)] > currentDist)
                                {
                                    currentSquare = new Point(currentSquare.X - 1, currentSquare.Y);
                                    currentDist = validMoves[currentSquare];
                                    ent.moveList.Insert(0, Direction.East);
                                }
                                break;
                            }
                        case Direction.North:
                            {
                                if (validMoves.ContainsKey(new Point(currentSquare.X, currentSquare.Y - 1)) && validMoves[new Point(currentSquare.X, currentSquare.Y - 1)] > currentDist)
                                {
                                    currentSquare = new Point(currentSquare.X, currentSquare.Y - 1);
                                    currentDist = validMoves[currentSquare];
                                    ent.moveList.Insert(0, Direction.South);
                                }
                                break;
                            }
                        case Direction.South:
                            {
                                if (validMoves.ContainsKey(new Point(currentSquare.X, currentSquare.Y + 1)) && validMoves[new Point(currentSquare.X, currentSquare.Y + 1)] > currentDist)
                                {
                                    currentSquare = new Point(currentSquare.X, currentSquare.Y + 1);
                                    currentDist = validMoves[currentSquare];
                                    ent.moveList.Insert(0, Direction.North);
                                }
                                break;
                            }
                    }
                }
            }

            MoveMob(ent, ent.moveList);
            ent.moveList.Clear();

        }

        public static void MoveRandom(Mob ent)
        {
            /*
            if (visibleCells.Contains(ent.pos))
            {
                MoveCursor(ent.x, ent.y);
            }*/
            //entities.Remove(ent.pos);
            Dictionary<Point, int> validMoves = new Dictionary<Point, int>() { };
            Dictionary<Point, bool> invalidMoves = new Dictionary<Point, bool>();

            calculateAllMoves(currentLevel, ent.x, ent.y, ent.moveSpeed, true, validMoves, invalidMoves, ent.friendly);
            foreach (Point nogo in currentLevel.enemyBlockedCells)
            {
                invalidMoves.Add(nogo, true);
            }
            if (validMoves.Keys.Except(invalidMoves.Keys).ToList().Count <= 0)
            {
                return;
            }
            int randomPosition = rnd.Next(validMoves.Keys.Except(invalidMoves.Keys).ToList().Count);
            Point targetSquare = validMoves.Keys.Except(invalidMoves.Keys).ToList()[randomPosition], currentSquare = targetSquare;
            int currentDist = validMoves[targetSquare];

            Direction[] randir = { Direction.East, Direction.North, Direction.West, Direction.South };
            Shuffle(randir);
            while (currentDist < ent.moveSpeed)
            {
                foreach (Direction d in randir)
                {
                    switch (d)
                    {
                        case Direction.East:
                            {
                                if (validMoves.ContainsKey(new Point(currentSquare.X + 1, currentSquare.Y)) && validMoves[new Point(currentSquare.X + 1, currentSquare.Y)] > currentDist)
                                {
                                    currentSquare = new Point(currentSquare.X + 1, currentSquare.Y);
                                    currentDist = validMoves[currentSquare];
                                    ent.moveList.Insert(0, Direction.West);
                                }
                                break;
                            }
                        case Direction.West:
                            {
                                if (validMoves.ContainsKey(new Point(currentSquare.X - 1, currentSquare.Y)) && validMoves[new Point(currentSquare.X - 1, currentSquare.Y)] > currentDist)
                                {
                                    currentSquare = new Point(currentSquare.X - 1, currentSquare.Y);
                                    currentDist = validMoves[currentSquare];
                                    ent.moveList.Insert(0, Direction.East);
                                }
                                break;
                            }
                        case Direction.North:
                            {
                                if (validMoves.ContainsKey(new Point(currentSquare.X, currentSquare.Y - 1)) && validMoves[new Point(currentSquare.X, currentSquare.Y - 1)] > currentDist)
                                {
                                    currentSquare = new Point(currentSquare.X, currentSquare.Y - 1);
                                    currentDist = validMoves[currentSquare];
                                    ent.moveList.Insert(0, Direction.South);
                                }
                                break;
                            }
                        case Direction.South:
                            {
                                if (validMoves.ContainsKey(new Point(currentSquare.X, currentSquare.Y + 1)) && validMoves[new Point(currentSquare.X, currentSquare.Y + 1)] > currentDist)
                                {
                                    currentSquare = new Point(currentSquare.X, currentSquare.Y + 1);
                                    currentDist = validMoves[currentSquare];
                                    ent.moveList.Insert(0, Direction.North);
                                }
                                break;
                            }
                    }
                }
            }

            /*
            int randVal;
            foreach (int any in new int[] { 0, 0, 0 })
            {
                randVal = rnd.Next(4);
                switch (randVal)
                {
                case 0: if (ent.x > 0 && (map[ent.y, ent.x - 1] == 1194) && checkPos(ent.x - 1, ent.y) == null) //entities.FirstOrDefault(e => e.x == ent.x - 1 && e.y == ent.y)
                    {
                        ent.x--;
                    }
                    else if (alterGlobals && ent.x > 0 && (map[ent.y, ent.x - 1] == 1187) && checkFixture(ent.x - 1, ent.y, 1190) != null)
                    {
                        map[ent.y, ent.x - 1] = 1194;
                        fixtures[new Cell() { x = ent.x - 1, y = ent.y }].tile = 1188;
                    }
                    break;
                case 1: if (ent.y > 0 && (map[ent.y - 1, ent.x] == 1194) && checkPos(ent.x, ent.y - 1) == null)
                    {
                        ent.y--;
                    }
                    else if (alterGlobals && ent.y > 0 && (map[ent.y - 1, ent.x] == 1187) && checkFixture(ent.x, ent.y - 1, 1191) != null)
                    {
                        map[ent.y - 1, ent.x] = 1194;
                        fixtures[new Cell() { x = ent.x, y = ent.y - 1 }].tile = 1189;
                        //                        fixtures.FirstOrDefault(e => e.x == ent.x && e.y == ent.y - 1 && e.tile == 1191).tile = 1189;
                    }
                    break;
                case 2: if (ent.x > 0 && (map[ent.y, ent.x + 1] == 1194) && checkPos(ent.x + 1, ent.y) == null) //entities.FirstOrDefault(e => e.x == ent.x - 1 && e.y == ent.y)
                    {
                        ent.x++;
                    }
                    else if (alterGlobals && ent.x > 0 && (map[ent.y, ent.x + 1] == 1187) && checkFixture(ent.x + 1, ent.y, 1190) != null)
                    {
                        map[ent.y, ent.x + 1] = 1194;
                        fixtures[new Cell() { x = ent.x + 1, y = ent.y }].tile = 1188;
                    }
                    break;
                case 3: if (ent.y > 0 && (map[ent.y + 1, ent.x] == 1194) && checkPos(ent.x, ent.y + 1) == null)
                    {
                        ent.y++;
                    }
                    else if (alterGlobals && ent.y > 0 && (map[ent.y + 1, ent.x] == 1187) && checkFixture(ent.x, ent.y + 1, 1191) != null)
                    {
                        map[ent.y + 1, ent.x] = 1194;
                        fixtures[new Cell() { x = ent.x, y = ent.y + 1 }].tile = 1189;
                    }
                    break;
              
                    case 0: ent.moveList.Add(Direction.West);
                        break;
                    case 1: ent.moveList.Add(Direction.North);
                        break;
                    case 2: ent.moveList.Add(Direction.East);
                        break;
                    case 3: ent.moveList.Add(Direction.South);
                        break;
                }
            }*/
            MoveMob(ent, ent.moveList);
            Point c = ent.pos;
            Point[] dCells = { new Point(c.X + 1, c.Y), new Point(c.X - 1, c.Y), new Point(c.X, c.Y - 1), new Point(c.X, c.Y + 1) };
            Shuffle(dCells);
            if (currentLevel.entities.ContainsKey(dCells[0]))
            {
                ent.currentSkill.targetSquare = dCells[0];
                AnimateResults(ent.currentSkill.ApplySkill(ent));
            }
            else if (currentLevel.entities.ContainsKey(dCells[1]))
            {
                ent.currentSkill.targetSquare = dCells[1];
                AnimateResults(ent.currentSkill.ApplySkill(ent));
            }
            else if (currentLevel.entities.ContainsKey(dCells[2]))
            {
                ent.currentSkill.targetSquare = dCells[2];
                AnimateResults(ent.currentSkill.ApplySkill(ent));
            }
            else if (currentLevel.entities.ContainsKey(dCells[3]))
            {
                ent.currentSkill.targetSquare = dCells[3];
                AnimateResults(ent.currentSkill.ApplySkill(ent));
            }
            ent.moveList.Clear();
            /*                entities[ent.pos] = ent;
                            requestingMove.x = ent.o_pos.x;
                            requestingMove.y = ent.o_pos.y;
             */

        }
        public static void MoveMobAndAttack(Mob ent, IEnumerable<Direction> movepath)
        {
            Dictionary<Point, Mob> displaced = new Dictionary<Point, Mob>();
            if (currentLevel.visibleCells[ent.y, ent.x] > 0)
            {
                MoveCursor(ent.x, ent.y);
            }
            double startingTime = Timing.TotalMilliseconds;
            if (currentLevel.visibleCells[ent.y, ent.x] > 0)
            {
                currentActor = ent;
                lockForAnimation = true;
                while (Timing.TotalMilliseconds - startingTime < 300)
                {
                    System.Threading.Thread.Sleep(25);
                    if (Display.CurrentWindow.IsClosed)
                        return;
                    Display.BeginFrame();
                    Show();
                    Display.EndFrame();
                    try
                    {
                        Core.KeepAlive();
                    }
                    catch (Exception) { };
                }
            }
            startingTime = Timing.TotalMilliseconds;
            foreach (Direction currMove in movepath)
            {

                //while (Timing.TotalMilliseconds - startingTime < 200 && (ent.x >= minVisibleX && ent.x <= maxVisibleX && ent.y >= minVisibleY && ent.y <= maxVisibleY)) ;
                //if (ent.x >= minVisibleX && ent.x <= maxVisibleX && ent.y >= minVisibleY && ent.y <= maxVisibleY)
                if (ent.actionCount >= 2)
                    break;
                if (currentLevel.visibleCells[ent.y, ent.x] > 0)
                {
                    currentActor = ent;
                    lockForAnimation = true;
                    while (Timing.TotalMilliseconds - startingTime < 200)
                    {
                        System.Threading.Thread.Sleep(25);
                        if (Display.CurrentWindow.IsClosed)
                            return;
                        Display.BeginFrame();
                        Show();
                        Display.EndFrame();
                        Core.KeepAlive();
                    }
                }
                startingTime = Timing.TotalMilliseconds;
                currentLevel.entities.Remove(ent.pos);

                switch (currMove)
                {
                    case Direction.West: if (ent.x > 0 && (currentLevel.map[ent.y, ent.x - 1] == 1194) && checkPos(ent.x - 1, ent.y) == null) //entities.FirstOrDefault(e => e.x == ent.x - 1 && e.y == ent.y)
                        {
                            if (displaced.ContainsKey(ent.pos))
                                currentLevel.entities.Add(ent.pos, displaced[ent.pos]);
                            ent.x--;
                            currentLevel.entities[ent.pos] = ent;
                        }
                        else if (ent.x > 0 && (currentLevel.map[ent.y, ent.x - 1] == 1187) && checkFixture(ent.x - 1, ent.y, 1190) != null)
                        {
                            currentLevel.map[ent.y, ent.x - 1] = 1194;
                            currentLevel.fixtures[new Point(ent.x - 1, ent.y)].tile = 1188;
                            currentLevel.entities[ent.pos] = ent;
                        }
                        else if (ent.x > 0 && (currentLevel.map[ent.y, ent.x - 1] == 1194) && checkPos(ent.x - 1, ent.y) != null && checkPos(ent.x - 1, ent.y).friendly == ent.friendly)
                        {
                            if (displaced.ContainsKey(ent.pos))
                                currentLevel.entities.Add(ent.pos, displaced[ent.pos]);
                            displaced.Add(checkPos(ent.x - 1, ent.y).pos, checkPos(ent.x - 1, ent.y));
                            ent.x--;
                            currentLevel.entities[ent.pos] = ent;
                        }
                        //else if (ent.x > 0 && (currentLevel.map[ent.y, ent.x - 1] == 1194) && checkPos(ent.x - 1, ent.y) != null && checkPos(ent.x - 1, ent.y).friendly != ent.friendly)
                        //{
                        //    currentLevel.entities[ent.pos] = ent;
                        //    ent.currentSkill.targetSquare = new Point(ent.x - 1, ent.y);
                        //    AnimateResults(ent.currentSkill.ApplySkill(ent));
                        //    ent.actionCount = 2;
                        //}
                        break;
                    case Direction.North: if (ent.y > 0 && (currentLevel.map[ent.y - 1, ent.x] == 1194) && checkPos(ent.x, ent.y - 1) == null)
                        {
                            if (displaced.ContainsKey(ent.pos))
                                currentLevel.entities.Add(ent.pos, displaced[ent.pos]);
                            ent.y--;
                            currentLevel.entities[ent.pos] = ent;
                        }
                        else if (ent.y > 0 && (currentLevel.map[ent.y - 1, ent.x] == 1187) && checkFixture(ent.x, ent.y - 1, 1191) != null)
                        {
                            currentLevel.map[ent.y - 1, ent.x] = 1194;
                            currentLevel.fixtures[new Point() { X = ent.x, Y = ent.y - 1 }].tile = 1189;
                            currentLevel.entities[ent.pos] = ent;
                            //                        fixtures.FirstOrDefault(e => e.x == ent.x && e.y == ent.y - 1 && e.tile == 1191).tile = 1189;
                        }
                        else if (ent.y > 0 && (currentLevel.map[ent.y - 1, ent.x] == 1194) && checkPos(ent.x, ent.y - 1) != null && checkPos(ent.x, ent.y - 1).friendly == ent.friendly)
                        {
                            if (displaced.ContainsKey(ent.pos))
                                currentLevel.entities.Add(ent.pos, displaced[ent.pos]);
                            displaced.Add(checkPos(ent.x, ent.y - 1).pos, checkPos(ent.x, ent.y - 1));
                            ent.y--;
                            currentLevel.entities[ent.pos] = ent;
                        }
                        //else if (ent.y > 0 && (currentLevel.map[ent.y - 1, ent.x] == 1194) && checkPos(ent.x, ent.y - 1) != null && checkPos(ent.x, ent.y - 1).friendly != ent.friendly)
                        //{
                        //    currentLevel.entities[ent.pos] = ent;
                        //    ent.currentSkill.targetSquare = new Point(ent.x, ent.y - 1);
                        //    AnimateResults(ent.currentSkill.ApplySkill(ent));
                        //    ent.actionCount = 2;
                        //}
                        break;
                    case Direction.East: if (ent.x < currentLevel.mapWidthBound && (currentLevel.map[ent.y, ent.x + 1] == 1194) && checkPos(ent.x + 1, ent.y) == null) //entities.FirstOrDefault(e => e.x == ent.x - 1 && e.y == ent.y)
                        {
                            if (displaced.ContainsKey(ent.pos))
                                currentLevel.entities.Add(ent.pos, displaced[ent.pos]);
                            ent.x++;
                            currentLevel.entities[ent.pos] = ent;
                        }
                        else if (ent.x < currentLevel.mapWidthBound && (currentLevel.map[ent.y, ent.x + 1] == 1187) && checkFixture(ent.x + 1, ent.y, 1190) != null)
                        {
                            currentLevel.map[ent.y, ent.x + 1] = 1194;
                            currentLevel.fixtures[new Point() { X = ent.x + 1, Y = ent.y }].tile = 1188;
                            currentLevel.entities[ent.pos] = ent;
                        }
                        else if (ent.x < currentLevel.mapWidthBound && (currentLevel.map[ent.y, ent.x + 1] == 1194) && checkPos(ent.x + 1, ent.y) != null && checkPos(ent.x + 1, ent.y).friendly == ent.friendly)
                        {
                            if (displaced.ContainsKey(ent.pos))
                                currentLevel.entities.Add(ent.pos, displaced[ent.pos]);
                            displaced.Add(checkPos(ent.x + 1, ent.y).pos, checkPos(ent.x + 1, ent.y));
                            ent.x++;
                            currentLevel.entities[ent.pos] = ent;
                        }
                        //else if (ent.x < currentLevel.mapWidthBound && (currentLevel.map[ent.y, ent.x + 1] == 1194) && checkPos(ent.x + 1, ent.y) != null && checkPos(ent.x + 1, ent.y).friendly != ent.friendly)
                        //{
                        //    currentLevel.entities[ent.pos] = ent;
                        //    ent.currentSkill.targetSquare = new Point(ent.x + 1, ent.y);
                        //    AnimateResults(ent.currentSkill.ApplySkill(ent));
                        //    ent.actionCount = 2;
                        //}
                        break;
                    case Direction.South: if (ent.y < currentLevel.mapHeightBound && (currentLevel.map[ent.y + 1, ent.x] == 1194) && checkPos(ent.x, ent.y + 1) == null)
                        {
                            if (displaced.ContainsKey(ent.pos))
                                currentLevel.entities.Add(ent.pos, displaced[ent.pos]);
                            ent.y++;
                            currentLevel.entities[ent.pos] = ent;
                        }
                        else if (ent.y < currentLevel.mapHeightBound && (currentLevel.map[ent.y + 1, ent.x] == 1187) && checkFixture(ent.x, ent.y + 1, 1191) != null)
                        {
                            currentLevel.map[ent.y + 1, ent.x] = 1194;
                            currentLevel.fixtures[new Point() { X = ent.x, Y = ent.y + 1 }].tile = 1189;
                            currentLevel.entities[ent.pos] = ent;
                        }
                        else if (ent.y < currentLevel.mapHeightBound && (currentLevel.map[ent.y + 1, ent.x] == 1194) && checkPos(ent.x, ent.y + 1) != null && checkPos(ent.x, ent.y + 1).friendly == ent.friendly)
                        {
                            if (displaced.ContainsKey(ent.pos))
                                currentLevel.entities.Add(ent.pos, displaced[ent.pos]);
                            displaced.Add(checkPos(ent.x, ent.y + 1).pos, checkPos(ent.x, ent.y + 1));
                            ent.y++;
                            currentLevel.entities[ent.pos] = ent;
                        }
                        //else if (ent.y < currentLevel.mapHeightBound && (currentLevel.map[ent.y + 1, ent.x] == 1194) && checkPos(ent.x, ent.y + 1) != null && checkPos(ent.x, ent.y + 1).friendly != ent.friendly)
                        //{
                        //    currentLevel.entities[ent.pos] = ent;
                        //    ent.currentSkill.targetSquare = new Point(ent.x, ent.y + 1);
                        //    AnimateResults(ent.currentSkill.ApplySkill(ent));
                        //    ent.actionCount = 2;
                        //}
                        break;
                    default:
                        {
                            currentLevel.entities[ent.pos] = ent;
                            break;
                        }
                }

                ent.fov.calculateSight();
                Point c = ent.pos;
                Point[] dCells = { new Point(c.X + 1, c.Y), new Point(c.X - 1, c.Y), new Point(c.X, c.Y - 1), new Point(c.X, c.Y + 1) };
                Shuffle(dCells);
                if (currentLevel.entities.ContainsKey(dCells[0]) && currentLevel.entities[dCells[0]].friendly)
                {
                    ent.currentSkill.targetSquare = dCells[0];
                    AnimateResults(ent.currentSkill.ApplySkill(ent));
                    ent.actionCount = 2;
                }
                else if (currentLevel.entities.ContainsKey(dCells[1]) && currentLevel.entities[dCells[1]].friendly)
                {
                    ent.currentSkill.targetSquare = dCells[1];
                    AnimateResults(ent.currentSkill.ApplySkill(ent));
                    ent.actionCount = 2;
                }
                else if (currentLevel.entities.ContainsKey(dCells[2]) && currentLevel.entities[dCells[2]].friendly)
                {
                    ent.currentSkill.targetSquare = dCells[2];
                    AnimateResults(ent.currentSkill.ApplySkill(ent));
                    ent.actionCount = 2;
                }
                else if (currentLevel.entities.ContainsKey(dCells[3]) && currentLevel.entities[dCells[3]].friendly)
                {
                    ent.currentSkill.targetSquare = dCells[3];
                    AnimateResults(ent.currentSkill.ApplySkill(ent));
                    ent.actionCount = 2;
                }
                if (currentLevel.visibleCells[ent.y, ent.x] > 0)
                {
                    MoveCursor(ent.x, ent.y);
                }
                if (ent.friendly)
                {
                    currentLevel.recalculateVision();
                }
                requestingMove.X = ent.o_pos.X;
                requestingMove.Y = ent.o_pos.Y;
                if (Display.CurrentWindow.IsClosed)
                    return;
                Display.BeginFrame();
                Show();
                Display.EndFrame();
            }
            //cursorX = ent.x;
            //cursorY = ent.y;

            lockForAnimation = false;

        }

        public static void MoveSmart(Mob ent)
        {
            ent.fov.calculateSight();
            ent.intel.Evaluate();
            MoveMobAndAttack(ent, ent.moveList);
            //Point c = ent.pos;
            //Point[] dCells = { new Point(c.X + 1, c.Y), new Point(c.X - 1, c.Y), new Point(c.X, c.Y - 1), new Point(c.X, c.Y + 1) };
            //Shuffle(dCells);
            //if (currentLevel.entities.ContainsKey(dCells[0]))
            //{
            //    ent.currentSkill.targetSquare = dCells[0];
            //    AnimateResults(ent.currentSkill.ApplySkill(ent));
            //}
            //else if (currentLevel.entities.ContainsKey(dCells[1]))
            //{
            //    ent.currentSkill.targetSquare = dCells[1];
            //    AnimateResults(ent.currentSkill.ApplySkill(ent));
            //}
            //else if (currentLevel.entities.ContainsKey(dCells[2]))
            //{
            //    ent.currentSkill.targetSquare = dCells[2];
            //    AnimateResults(ent.currentSkill.ApplySkill(ent));
            //}
            //else if (currentLevel.entities.ContainsKey(dCells[3]))
            //{
            //    ent.currentSkill.targetSquare = dCells[3];
            //    AnimateResults(ent.currentSkill.ApplySkill(ent));
            //}
            ent.moveList.Clear();
            /*                entities[ent.pos] = ent;
                            requestingMove.x = ent.o_pos.x;
                            requestingMove.y = ent.o_pos.y;
             */

        }
        /*
        static Mob Spawn(int tileNo, int width, int height)
        {
            int rx = rnd.Next(width);
            int ry = rnd.Next(height);
            if (currentLevel.map[ry, rx] == DungeonMap.gr)
            {
                Mob nt = new Mob(tileNo, rx, ry, false);

                if (checkPos(nt.x, nt.y) != null)
                    return Spawn(tileNo, width, height);
                nt.skillList.Add(new Skill("Basic Attack", rnd.Next(3, 7), 1, 1, SkillAreaKind.SingleTarget, 0, true));
                nt.currentSkill = nt.skillList[0];
                //nt.ui.addSkills(nt);
                currentLevel.entities[nt.pos] = nt;
                currentLevel.o_entities[nt.o_pos] = nt;
                return nt;
            }
            return Spawn(tileNo, width, height);
        }*/
        public static void Init()
        {

            fullDungeon = new List<Level>();
            for (int i = 0; i < 5; i++)
            {
                Level.safeStart = (i == 2);
                fullDungeon.Add(new Level() { floor = i});
                fullDungeon[i].Init(false);
            }
            fullDungeon.Add(new Level() { floor = 5});
            fullDungeon[5].Init(true);
            levelIndex = 2;
            currentLevel = fullDungeon[2];
            /*,
           new Entity() { tile = 1189, x = 2, y = 4},
           new Entity() { tile = 1189, x = 10, y = 13},
           new Entity() { tile = 1188, x = 6, y = 15},
           new Entity() { tile = 1188, x = 11, y = 17},*/
            /*
            currentLevel.map = DungeonMap.cleanUp(currentLevel.map);
            //            map = DungeonMap.theme(map);
            currentLevel.mapColors = DungeonMap.recolor(currentLevel.map);*/

            // map = DungeonMap.geomorph;
            int mw = currentLevel.map.GetLength(1), mh = currentLevel.map.GetLength(0);

            currentLevel.visibleCells = new double[mh, mw];
            currentLevel.seenCells = new double[mh, mw];

            //currentLevel.allies = new Dictionary<Point, Mob>();
            /*currentLevel.entities = new Dictionary<Point, Mob>()
            {
            };
            currentLevel.o_entities = new Dictionary<Point, Mob>()
            {
            };
            */
            Mob nt;
            nt = new Mob(541, 6, 7, true, levelIndex); //beholder
            nt.skillList.Add(new Skill("Eye Beam", 3, 8));
            nt.skillList.Add(new Skill("Disintegrate", 10, 2));
            nt.skillList.Add(new Skill("Horrid Glare", 2, 0, 1, SkillAreaKind.Spray, 5, false));
            nt.skillList.Add(new Skill("Dark Magic", 2, 3, 6, SkillAreaKind.Burst, 1, true));
            nt.ui.addSkills(nt);
            currentLevel.entities[nt.pos] = nt;
            currentLevel.o_entities[nt.o_pos] = nt;
            nt = new Mob(541, 6, 8, true, levelIndex); //beholder
            nt.skillList.Add(new Skill("Eye Beam", 3, 8));
            nt.skillList.Add(new Skill("Disintegrate", 10, 2));
            nt.skillList.Add(new Skill("Horrid Glare", 2, 0, 1, SkillAreaKind.Spray, 5, false));
            nt.skillList.Add(new Skill("Dark Magic", 2, 3, 6, SkillAreaKind.Burst, 1, true));
            nt.ui.addSkills(nt);
            currentLevel.entities[nt.pos] = nt;
            currentLevel.o_entities[nt.o_pos] = nt;
            nt = new Mob(541, 6, 9, true, levelIndex); //beholder
            nt.skillList.Add(new Skill("Eye Beam", 3, 8));
            nt.skillList.Add(new Skill("Disintegrate", 10, 2));
            nt.skillList.Add(new Skill("Horrid Glare", 2, 0, 1, SkillAreaKind.Spray, 5, false));
            nt.skillList.Add(new Skill("Dark Magic", 2, 3, 6, SkillAreaKind.Burst, 1, true));
            nt.ui.addSkills(nt);
            currentLevel.entities[nt.pos] = nt;
            currentLevel.o_entities[nt.o_pos] = nt;
            nt = new Mob(503, 15, 4, true, levelIndex); //demogorgon
            nt.skillList.Add(new Skill("Tentacle Flail", 3, 0, 0, SkillAreaKind.Ring, 2, true));
            nt.skillList.Add(new Skill("Vicious Bites", 10, 1));
            nt.skillList.Add(new Skill("Dark Magic", 2, 3, 6, SkillAreaKind.Burst, 1, true));
            nt.skillList.Add(new Skill("Fire Magic", 3, 0, 1, SkillAreaKind.Spray, 3, true));
            nt.skillList.Add(new Skill("Storm Magic", 3, 0, 0, SkillAreaKind.Ring, 2, true));
            nt.skillList.Add(new Skill("Ice Magic", 5, 2, 5, SkillAreaKind.SingleTarget, 0, true));
            nt.ui.addSkills(nt);
            currentLevel.entities[nt.pos] = nt;
            currentLevel.o_entities[nt.o_pos] = nt;
            nt = new Mob(1409, 4, 18, true, levelIndex); //drow
            nt.skillList.Add(new Skill("Sword Slash", 5, 0, 0, SkillAreaKind.Ring, 1, false));
            nt.skillList.Add(new Skill("Crossbow", 3, 6));
            nt.skillList.Add(new Skill("Choking Bomb", 3, 2, 4, SkillAreaKind.Burst, 2, true));
            nt.ui.addSkills(nt);
            currentLevel.entities[nt.pos] = nt;
            currentLevel.o_entities[nt.o_pos] = nt;
            nt = new Mob(1406, 4, 3, true, levelIndex); //baku
            nt.skillList.Add(new Skill("Tusk Attack", 6, 1));
            nt.skillList.Add(new Skill("Trunk Slap", 2, 0, 0, SkillAreaKind.Ring, 3, true));
            Skill heal = new Skill("Rapid Healing", -5, 0);
            heal.minSkillDistance = 0;
            nt.skillList.Add(heal);
            nt.ui.addSkills(nt);
            currentLevel.entities[nt.pos] = nt;
            currentLevel.o_entities[nt.o_pos] = nt;
            nt = new Mob(102, 15, 15, true, levelIndex); //Tengu
            nt.skillList.Add(new Skill("Leaf Chop", 6, 1));
            nt.skillList.Add(new Skill("Fan Blast", 3, 0, 1, SkillAreaKind.Spray, 4, true));
            nt.skillList.Add(new Skill("Tornado", 5, 3, 6, SkillAreaKind.Burst, 1, true));
            nt.ui.addSkills(nt);
            currentLevel.entities[nt.pos] = nt;
            currentLevel.o_entities[nt.o_pos] = nt;

            /*
            nt = new Mob(414, 11, 5, true);
            entities[nt.pos] = nt;
            o_entities[nt.pos] = nt;
            nt = new Mob(469, 2, 4, true);
            entities[nt.pos] = nt;
            o_entities[nt.pos] = nt;
            nt = new Mob(17, 8, 8, true);
            entities[nt.pos] = nt;
            o_entities[nt.pos] = nt;
            nt = new Mob(14, 13, 6, true);
            entities[nt.pos] = nt;
            o_entities[nt.pos] = nt;
            nt = new Mob(14, 15, 6, true);
            entities[nt.pos] = nt;
            o_entities[nt.pos] = nt;
            nt = new Mob(14, 14, 6, true);
            entities[nt.pos] = nt;
            o_entities[nt.pos] = nt;
            nt = new Mob(14, 12, 6, true);
            entities[nt.pos] = nt;
            o_entities[nt.pos] = nt;
            nt = new Mob(3, 12, 8, true);
            entities[nt.pos] = nt;
            o_entities[nt.pos] = nt;
            */
            /*            for (int i = 0, c = 0; i < 222; c++, i++)//= rnd.Next(2, 7)) //check for c to limit number of entities
                        {
                            Spawn(i, mw, mh);
                        }
                        for (int i = 226; i < 434; i++)
                        {
                            Spawn(i, mw, mh);
                        }
            for (int i = 473; i < 542; i++)
            {
                Spawn(i, mw, mh);
            }*/

            foreach (Point cl in currentLevel.entities.Keys)
            {
                int curr = rnd.Next(10000);
                while (initiative.ContainsKey(curr))
                {
                    curr = rnd.Next(10000);
                }
                initiative[curr] = cl;
            }
            currentInitiative = initiative.Keys.Max();

            currentLevel.allies = currentLevel.o_entities.Where(a => a.Value.friendly == true).ToDictionary(a => a.Key, a => a.Value);
            Ascend();

            //currentLevel.allies = currentLevel.o_entities.Where(a => a.Value.friendly == true).ToDictionary(a => a.Key, a => a.Value);
            currentLevel.recalculateVision();
            var numGroundTiles = 0;
            foreach (int eh in currentLevel.map)
            {
                if (eh == DungeonMap.gr || eh == 1187)
                    numGroundTiles++;
            }/*
            tileWidth = 48;
            tileHeight = 64;
            tileHIncrease = 16;
            tileVIncrease = 32;*/
            currentLevel.mapWidthBound = currentLevel.map.GetUpperBound(1);
            currentLevel.mapHeightBound = currentLevel.map.GetUpperBound(0);
            /*var alphaMatrix = new ColorMatrix();
            alphaMatrix.Matrix33 = 0.5f;
            alphaAttributes = new ImageAttributes();
            alphaAttributes.SetColorMatrix(alphaMatrix);*/
            //cursorX = 6;
            //cursorY = 7;

            //wind = DisplayWindow.CreateWindowed("Vicious Demo with AgateLib", ((mapWidth + 1) * 32) + (tileHIncrease * (1 + mapHeight)), (mapHeight * tileVIncrease) + tileHeight);

            //Display.RenderState.WaitForVerticalBlank = true;
            //wind = DisplayWindow.CreateWindowed("Vicious Demo with AgateLib", mapDisplayWidth, mapDisplayHeight + 32, false);      //(19 * tileVIncrease) + tileHeight); //((20) * 32) + (tileHIncrease * (20))

            //tileset = new Surface("Resources" + "/" + "slashem-revised.png"); //System.IO.Path.DirectorySeparatorChar


            /*
            DialogBrowser.currentUI = new DialogUI(new Dialog("The Narrator", new List<string>() {"Welcome to the Unpleasant Dungeon!", "Navigate through menus with the arrow keys.",
                "Confirm a selection with the Z key.", "Do you understand?"}, new List<DialogItem>() { new DialogItem("Yes", null, null, DialogBrowser.Hide) }),
                FontSurface.BitmapMonospace("Resources" + "/" + "monkey.png", new Size(6, 14)));
            */
            //DialogBrowser.currentUI = DialogUI.InitUI();
        }

        public static void Update()
        {

            if (lockState || lockForAnimation || mode == InputMode.Dialog) //requestingMove.x >= 0 || 
            {
                return;
            }
            for (int i = currentInitiative; i >= -1; i--, currentInitiative--)    //each (Mob ent in entities.Values.OrderByDescending(n => (n.hasActed) ? -1 : n.initiative))
            {
                if (initiative.ContainsKey(i))
                {
                    Mob ent = currentLevel.o_entities[initiative[i]];
                    if (ent.actionCount > 1)
                        continue;
                    if (ent.friendly == false)
                    {
                        MoveSmart (ent);
                        ent.actionCount = 2;
                        //requestingMove.x = -1;
                        lockState = false;
                    }
                    else
                    {
                        MoveCursor(ent.x, ent.y);
                        mode = InputMode.Menu;
                        currentActor = ent;
                        //                      cursorX = ent.x;
                        //                        cursorY = ent.y;
                        requestingMove.X = ent.o_pos.X;
                        requestingMove.Y = ent.o_pos.Y;
                        lockState = true;
                        ScreenBrowser.currentUI = ent.ui;
                        ScreenBrowser.UnHide();
                        //                        MessageBrowser.AddMessage(ent.name + ": actionCount is " + ent.actionCount, 8000, true);
                        if (ent.actionCount < 1)
                        {
                            ScreenBrowser.Refresh();
                        }
                        //                        cursorX = o_entities[initiative[currentInitiative]].x;
                        //                        cursorY = o_entities[initiative[currentInitiative]].y;
                        break;
                    }
                }
            }
            if (currentInitiative < initiative.Keys.Min())
            {
                initiative.Clear();
                foreach (Point cl in currentLevel.o_entities.Keys)
                {
                    currentLevel.o_entities[cl].actionCount = 0;
                    int curr = rnd.Next(10000);
                    while (initiative.ContainsKey(curr))
                    {
                        curr = rnd.Next(10000);
                    }
                    initiative[curr] = cl;

                }
                currentInitiative = initiative.Keys.Max();
                // requestingMove = initiative[currentInitiative];
                //Update();
                //requestingMove.x = initiative[currentInitiative].x;
                //requestingMove.y = initiative[currentInitiative].y;
                //lockState = true;
            }
        }
        static void Show()
        {
            Display.Clear(Color.FromArgb(32, 32, 32));
            // (cursorY <= 10) (cursorY > mapHeight - 10)    if cursorY <= 10, (vals < 20); 
            //minVisibleY = (cursorY < 20) ? 0 : (cursorY > mapHeight - 10) ? mapHeight - 20 : cursorY - 20;
            //maxVisibleY = minVisibleY;
            if (mode != InputMode.Dialog)
                currentLevel.Show();
            ScreenBrowser.Show();
            if (hoverActor != null)
                UnitInfo.ShowMobInfo(hoverActor);
            else if (currentActor != null)
                UnitInfo.ShowMobInfo(currentActor);
            Display.FillRect(new Rectangle(0, mapDisplayHeight, mapDisplayWidth, 32), (Color.Black));
            DialogBrowser.Show();
            MessageBrowser.Show();
            //mandrillFont.DrawText(32.0, 32.0, "FPS: " + (int)Display.FramesPerSecond);
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

                mode = InputMode.Dialog;
                DialogBrowser.currentUI = DialogUI.InitLoadUI();
                currentLevel = new Level() { floor = 0};
                currentLevel.map = new int[40, 40];
                currentLevel.map.Fill(indices => DungeonMap.da);


                Display.RenderState.WaitForVerticalBlank = true;
                wind = DisplayWindow.CreateWindowed("Vicious Demo with AgateLib", mapDisplayWidth, mapDisplayHeight + 32, false);      //(19 * tileVIncrease) + tileHeight); //((20) * 32) + (tileHIncrease * (20))
                wind.Closed += new EventHandler((obj, e) => Quit());
                tileset = new Surface("Resources" + "/" + "slashem-revised.png"); //System.IO.Path.DirectorySeparatorChar


                mandrillFont = FontSurface.BitmapMonospace("Resources" + "/" + "monkey_x2.png", new Size(12, 28));
                mandrillFont.Color = Color.LightSkyBlue;
                ScreenBrowser.Init();
                //ScreenBrowser.currentUI.currentScreen.title = "Mobs with Jobs!";
                ScreenBrowser.isHidden = true;
                //ScreenBrowser.Show();
                //            basicUI = new SimpleUI(new Screen("Mobs with Jobs!", new List<MenuItem>() { }), mandrillFont);
                Keyboard.KeyUp += new InputEventHandler(onKeyUp);

                MessageBrowser.font = FontSurface.BitmapMonospace("Resources" + "/" + "monkey.png", new Size(6, 14));
                MessageBrowser.x = 100;
                MessageBrowser.y = mapDisplayHeight + 4;

                Keyboard.KeyDown += OnKeyDown_ActionMenu;

                Keyboard.KeyDown += new InputEventHandler(OnKeyDown);
                Keyboard.KeyDown += new InputEventHandler(DialogBrowser.OnKeyDown_Dialog);
                // Update();
                while (!Display.CurrentWindow.IsClosed)
                {
                    Display.BeginFrame();
                    Show();
                    Display.EndFrame();
                    Update();
                    try
                    {
                        Core.KeepAlive();
                    }
                    catch(Exception)
                    {
                    }
                }
            }
        }
        public static void Quit()
        {
            /*
            JsonSerializerSettings sett = new JsonSerializerSettings();
            sett.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;*/
            /*var jsonSerializerSettings = new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.All
            };*/
            /*
            RuntimeTypeModel rttm = RuntimeTypeModel.Create();
            rttm.Add(typeof(GameState), true);
            System.IO.File.WriteAllText("debuggery.txt", rttm.GetTypes().ToString());
            
            rttm.AutoAddMissingTypes = true;*/
            foreach (Level l in fullDungeon)
            {
                l.map1D = new int[(l.mapWidthBound + 1) * (1 + l.mapHeightBound)];
                for (int i = 0; i <= l.mapHeightBound; i++)
                {
                    for (int j = 0; j <= l.mapWidthBound; j++)
                    {
                        l.map1D[i * (l.mapWidthBound + 1) + j] = l.map[i, j];
                    }
                }
            }
            SortedDictionary<DateTime, GameState> gs = getState();
            Serializer.model.Serialize(new System.IO.FileStream("save.mobsav", System.IO.FileMode.Truncate), gs);
//            System.IO.File.WriteAllText("save.mobsav", JsonConvert.SerializeObject(getState(), jsonSerializerSettings));
        }
        static void OnKeyDown(InputEventArgs e)
        {
            if (e.KeyCode == KeyCode.Q)
            {
                Display.CurrentWindow.Dispose();
                while (!Display.CurrentWindow.IsClosed) ;
//                Quit();
            }
            if (e.KeyCode == KeyCode.S)
            {
                showHealth = true;
            }
        }
        static void onKeyUp(InputEventArgs e)
        {
            if (e.KeyCode == KeyCode.S && showHealth)
            {
                showHealth = false;
            }
        }
        public static void OnKeyDown_ActionMenu(InputEventArgs e)
        {
            if (lockState && !lockForAnimation && mode == InputMode.Menu && initiative[currentInitiative] == requestingMove && currentLevel.o_entities.ContainsKey(requestingMove) && currentLevel.o_entities[requestingMove].friendly)
            {
                //currentlyPerformingMenuEvent = true;
                //currentlyPerformingMenuEvent = 
                ScreenBrowser.OnKeyDown_Menu(e);
            }
        }
        public static void OnKeyDown_CorrectEvents(InputEventArgs e)
        {
            if ( //e.KeyCode == KeyCode.Plus && 
                lockState && !lockForAnimation &&
                //                ScreenBrowser.isHidden == false && 
                initiative[currentInitiative] == requestingMove &&
                currentLevel.o_entities.ContainsKey(requestingMove) && currentLevel.o_entities[requestingMove].friendly)
            {
                Keyboard.KeyDown -= new InputEventHandler(OnKeyDown_ActionMenu);
                Keyboard.KeyDown += new InputEventHandler(OnKeyDown_ActionMenu);
                //                OnKeyDown_SelectMove(e);
            }
        }

        public static void calculateAllMoves(int startX, int startY, int numMoves, bool performEntCheck)
        {
            calculateAllMoves(currentLevel, startX, startY, numMoves, performEntCheck, currentLevel.highlightedCells, currentLevel.doNotStopCells, true);
            /*
            if (numMoves == -1)
                return;
            else
            {
                Cell c = new Cell(startX, startY);

                if (map[startY, startX] != DungeonMap.gr)
                    return;
                if (highlightedCells.Count != 0 && performEntCheck && checkPos(startX, startY) != null)
                    return;
                if (!highlightedCells.ContainsKey(c))
                    highlightedCells.Add(c, numMoves);
                calculateAllMoves(startX - 1, startY, numMoves - 1, performEntCheck);
                calculateAllMoves(startX + 1, startY, numMoves - 1, performEntCheck);
                calculateAllMoves(startX, startY - 1, numMoves - 1, performEntCheck);
                calculateAllMoves(startX, startY + 1, numMoves - 1, performEntCheck);

            }*/
        }
        public static void calculateAllMovesOld(Level lvl, int startX, int startY, int numMoves, bool performEntCheck, Dictionary<Point, int> cellStore, Dictionary<Point, bool> invalidCells, bool moverIsFriendly)
        {
            if (numMoves == -1)
                return;
            else
            {
                Point c = Geometry.normalizeCell(new Point(startX, startY), lvl);
                startX = c.X;
                startY = c.Y;
                if (lvl.map[startY, startX] != DungeonMap.gr)
                    return;
                if (cellStore.Count != 0 && performEntCheck && checkPos(startX, startY) != null)
                {
                    if (checkPos(startX, startY).friendly == moverIsFriendly)
                    {

                        if (!cellStore.ContainsKey(c))
                        {
                            cellStore.Add(c, numMoves);
                            invalidCells.Add(c, true);
                        }
                        calculateAllMoves(lvl, startX - 1, startY, numMoves - 1, performEntCheck, cellStore, invalidCells, moverIsFriendly);
                        calculateAllMoves(lvl, startX + 1, startY, numMoves - 1, performEntCheck, cellStore, invalidCells, moverIsFriendly);
                        calculateAllMoves(lvl, startX, startY - 1, numMoves - 1, performEntCheck, cellStore, invalidCells, moverIsFriendly);
                        calculateAllMoves(lvl, startX, startY + 1, numMoves - 1, performEntCheck, cellStore, invalidCells, moverIsFriendly);
                    }
                    else return;
                }
                if (cellStore.ContainsKey(c) == true && cellStore[c] < numMoves)
                {
                    cellStore.Remove(c);
                    cellStore.Add(c, numMoves);
                }
                else if (cellStore.ContainsKey(c) == false)
                    cellStore.Add(c, numMoves);
                calculateAllMoves(lvl, startX - 1, startY, numMoves - 1, performEntCheck, cellStore, invalidCells, moverIsFriendly);
                calculateAllMoves(lvl, startX + 1, startY, numMoves - 1, performEntCheck, cellStore, invalidCells, moverIsFriendly);
                calculateAllMoves(lvl, startX, startY - 1, numMoves - 1, performEntCheck, cellStore, invalidCells, moverIsFriendly);
                calculateAllMoves(lvl, startX, startY + 1, numMoves - 1, performEntCheck, cellStore, invalidCells, moverIsFriendly);

            }
        }

        public static void calculateAllMoves(Level lvl, int startX, int startY, int numMoves, bool performEntCheck, Dictionary<Point, int> cellStore, Dictionary<Point, bool> invalidCells, bool moverIsFriendly)
        {
            if (numMoves == 0)
                return;
            else
            {
                Point c = Geometry.normalizeCell(new Point(startX, startY), lvl);
                startX = c.X;
                startY = c.Y;
                if (lvl.map[startY, startX] != DungeonMap.gr)
                    return;
                if (cellStore.Count == 0)
                    cellStore.Add(c, numMoves);
                foreach (Point cl in cellStore.Keys.ToList())
                {
                    if (cellStore[cl] != numMoves)
                        continue;
                    Point[] pts = { new Point(cl.X + 1, cl.Y), new Point(cl.X - 1, cl.Y), new Point(cl.X, cl.Y + 1), new Point(cl.X, cl.Y - 1) };
                    foreach (Point p in pts)
                    {
                        if (cellStore.ContainsKey(p))
                            continue;
                        if (p.X < 0 || p.Y < 0 || p.X > lvl.map.GetUpperBound(1) || p.Y > lvl.map.GetUpperBound(0))
                            continue;
                        if (performEntCheck && checkPos(p.X, p.Y) != null)
                        {
                            if (checkPos(p.X, p.Y).friendly == moverIsFriendly)
                            {
                                cellStore.Add(p, numMoves - 1);
                                invalidCells.Add(p, true);
                            }
                        }
                        else if (lvl.map[p.Y, p.X] == DungeonMap.gr)
                            cellStore.Add(p, numMoves - 1);
                    }
                }
                calculateAllMoves(lvl, startX, startY, numMoves - 1, performEntCheck, cellStore, invalidCells, moverIsFriendly);
                /*
                        if (cellStore.ContainsKey(c) == true && cellStore[c] < numMoves)
                        {
                            cellStore.Remove(c);
                            cellStore.Add(c, numMoves);
                        }
                        else if (cellStore.ContainsKey(c) == false)*/
                /*
                calculateAllMoves(lvl, startX + 1, startY, numMoves - 1, performEntCheck, cellStore, invalidCells, moverIsFriendly);
                calculateAllMoves(lvl, startX, startY - 1, numMoves - 1, performEntCheck, cellStore, invalidCells, moverIsFriendly);
                calculateAllMoves(lvl, startX, startY + 1, numMoves - 1, performEntCheck, cellStore, invalidCells, moverIsFriendly);

                calculateAllMoves(lvl, startX - 1, startY, numMoves - 1, performEntCheck, cellStore, invalidCells, moverIsFriendly);
                calculateAllMoves(lvl, startX + 1, startY, numMoves - 1, performEntCheck, cellStore, invalidCells, moverIsFriendly);
                calculateAllMoves(lvl, startX, startY - 1, numMoves - 1, performEntCheck, cellStore, invalidCells, moverIsFriendly);
                calculateAllMoves(lvl, startX, startY + 1, numMoves - 1, performEntCheck, cellStore, invalidCells, moverIsFriendly);
                 */
            }
        }
        public static void calculateAllMoves(Level lvl, int startX, int startY, int numMoves, int overreach, bool performEntCheck, Dictionary<Point, int> cellStore, Dictionary<Point, bool> invalidCells, List<Point> specialTargetCells, bool moverIsFriendly)
        {
            if (numMoves == -1 * overreach)
                return;
            else
            {
                Point c = Geometry.normalizeCell(new Point(startX, startY), lvl);
                startX = c.X;
                startY = c.Y;
                if (lvl.map[startY, startX] != DungeonMap.gr)
                    return;
                if (cellStore.Count == 0)
                    cellStore.Add(c, numMoves);
                foreach (Point cl in cellStore.Keys.ToList())
                {
                    if (cellStore[cl] != numMoves)
                        continue;
                    Point[] pts = { new Point(cl.X + 1, cl.Y), new Point(cl.X - 1, cl.Y), new Point(cl.X, cl.Y + 1), new Point(cl.X, cl.Y - 1) };
                    foreach (Point p in pts)
                    {
                        if (cellStore.ContainsKey(p))
                            continue;
                        if (p.X < 0 || p.Y < 0 || p.X > lvl.map.GetUpperBound(1) || p.Y > lvl.map.GetUpperBound(0))
                            continue;
                        if (performEntCheck && checkPos(p.X, p.Y) != null)
                        {
                            if (checkPos(p.X, p.Y).friendly == moverIsFriendly || specialTargetCells.Contains(p))
                            {
                                cellStore.Add(p, numMoves - 1);
                                invalidCells.Add(p, true);
                            }
                            
                        }
                        else if (lvl.map[p.Y, p.X] == DungeonMap.gr)
                            cellStore.Add(p, numMoves - 1);
                    }
                }
                calculateAllMoves(lvl, startX, startY, numMoves - 1, overreach, performEntCheck, cellStore, invalidCells, specialTargetCells, moverIsFriendly);
                /*
                        if (cellStore.ContainsKey(c) == true && cellStore[c] < numMoves)
                        {
                            cellStore.Remove(c);
                            cellStore.Add(c, numMoves);
                        }
                        else if (cellStore.ContainsKey(c) == false)*/
                /*
                calculateAllMoves(lvl, startX + 1, startY, numMoves - 1, performEntCheck, cellStore, invalidCells, moverIsFriendly);
                calculateAllMoves(lvl, startX, startY - 1, numMoves - 1, performEntCheck, cellStore, invalidCells, moverIsFriendly);
                calculateAllMoves(lvl, startX, startY + 1, numMoves - 1, performEntCheck, cellStore, invalidCells, moverIsFriendly);

                calculateAllMoves(lvl, startX - 1, startY, numMoves - 1, performEntCheck, cellStore, invalidCells, moverIsFriendly);
                calculateAllMoves(lvl, startX + 1, startY, numMoves - 1, performEntCheck, cellStore, invalidCells, moverIsFriendly);
                calculateAllMoves(lvl, startX, startY - 1, numMoves - 1, performEntCheck, cellStore, invalidCells, moverIsFriendly);
                calculateAllMoves(lvl, startX, startY + 1, numMoves - 1, performEntCheck, cellStore, invalidCells, moverIsFriendly);
                 */
            }
        }
        
        public static void removeMovesUnderMinimum(Level lvl, int startX, int startY, int numMoves, bool performEntCheck)
        {
            if (numMoves == 0)
                return;
            else
            {
                Point c = Geometry.normalizeCell(new Point(startX, startY), lvl);

                if (currentLevel.map[startY, startX] != DungeonMap.gr)
                    return;
                //if (nonHighlightedFreeCells.Count == 0)// && performEntCheck && checkPos(startX, startY) != null)
                //    nonHighlightedFreeCells.Add(c, 1);
                //return;
                if (currentLevel.highlightedCells.ContainsKey(c) && !currentLevel.nonHighlightedFreeCells.ContainsKey(c))
                {
                    currentLevel.highlightedCells.Remove(c);
                    currentLevel.nonHighlightedFreeCells.Add(c, 1);
                }
                removeMovesUnderMinimum(lvl, startX - 1, startY, numMoves - 1, performEntCheck);
                removeMovesUnderMinimum(lvl, startX + 1, startY, numMoves - 1, performEntCheck);
                removeMovesUnderMinimum(lvl, startX, startY - 1, numMoves - 1, performEntCheck);
                removeMovesUnderMinimum(lvl, startX, startY + 1, numMoves - 1, performEntCheck);

            }
        }
        public static void calculateAllTargets(Mob user, int startX, int startY, Skill sk)
        {
            SkillAreaKind sak = sk.areaKind;
            int radius = sk.radius;
            switch (sak)
            {
                case SkillAreaKind.Ring:
                    {
                        //assuming targetSquare of 10, 11 and radius 1:
                        int minX = startX - radius; // 9
                        if (minX < 0)
                            minX = 0;
                        int minY = startY - radius; // 10
                        if (minY < 0)
                            minY = 0;
                        int maxX = startX + radius; // 11
                        if (maxX > Demo.currentLevel.map.GetUpperBound(1))
                            maxX = Demo.currentLevel.map.GetUpperBound(1);
                        int maxY = startY + radius; // 12
                        if (maxY > Demo.currentLevel.map.GetUpperBound(0))
                            maxY = Demo.currentLevel.map.GetUpperBound(0);

                        for (int i = minX; i <= maxX; i++)
                        {
                            for (int j = minY; j <= maxY; j++)
                            {
                                if (j - startY > i - startX + radius ||        // 10 - 10 > 10 - 11 + 1
                                    j - startY > -1 * (i - startX) + radius || // 10 - 10 > -10 + 11 + 1
                                    j - startY < i - startX - radius ||        // 10 - 10 < 10 - 11 - 1
                                    j - startY < -1 * (i - startX) - radius || // 10 - 10 < -10 + 11 - 1
                                    (i == startX && j == startY))
                                    continue;
                                if (sk.hitsAllies == false && checkPos(i, j) != null && checkPos(i, j).friendly == user.friendly)
                                    continue;
                                if (currentLevel.map[j, i] == DungeonMap.gr && user.fov.sight[j, i] > 0)
                                    currentLevel.highlightedTargetCells.Add(new Point(i, j), 1);
                            }
                        }
                        break;
                    }
                case SkillAreaKind.Burst:
                    {
                        Dictionary<Point, int> damages = new Dictionary<Point, int>();
                        Dictionary<Point, bool> kills = new Dictionary<Point, bool>();
                        //assuming targetSquare of 10, 11 and radius 1:
                        int minX = startX - radius; // 9
                        if (minX < 0)
                            minX = 0;
                        int minY = startY - radius; // 10
                        if (minY < 0)
                            minY = 0;
                        int maxX = startX + radius; // 11
                        if (maxX > Demo.currentLevel.map.GetUpperBound(1))
                            maxX = Demo.currentLevel.map.GetUpperBound(1);
                        int maxY = startY + radius; // 12
                        if (maxY > Demo.currentLevel.map.GetUpperBound(0))
                            maxY = Demo.currentLevel.map.GetUpperBound(0);

                        for (int i = minX; i <= maxX; i++)
                        {
                            for (int j = minY; j <= maxY; j++)
                            {
                                if (j - startY > i - startX + radius ||        // 10 - 10 > 10 - 11 + 1
                                    j - startY > -1 * (i - startX) + radius || // 10 - 10 > -10 + 11 + 1
                                    j - startY < i - startX - radius ||        // 10 - 10 < 10 - 11 - 1
                                    j - startY < -1 * (i - startX) - radius)   // 10 - 10 < -10 + 11 - 1
                                    continue;

                                if (sk.hitsAllies == false && checkPos(i, j) != null && checkPos(i, j).friendly == user.friendly)
                                    continue;
                                if (currentLevel.map[j, i] == DungeonMap.gr && user.fov.sight[j, i] > 0)
                                    currentLevel.highlightedTargetCells.Add(new Point(i, j), 1);
                            }
                        }
                        break;
                    }
                case SkillAreaKind.Spray:
                    {

                        Dictionary<Point, int> damages = new Dictionary<Point, int>();
                        Dictionary<Point, bool> kills = new Dictionary<Point, bool>();
                        //assuming targetSquare of 10, 11 and radius 2:
                        int minX = startX - (radius - 1); // 8
                        if (minX < 0)
                            minX = 0;
                        int minY = startY - (radius - 1); // 9
                        if (minY < 0)
                            minY = 0;
                        int maxX = startX + (radius); // 12
                        if (maxX > Demo.currentLevel.map.GetUpperBound(1))
                            maxX = Demo.currentLevel.map.GetUpperBound(1);
                        int maxY = startY + (radius); // 13
                        if (maxY > Demo.currentLevel.map.GetUpperBound(0))
                            maxY = Demo.currentLevel.map.GetUpperBound(0);

                        if (startY == user.y && startX == user.x)
                        {
                            minY++;
                            maxY++;
                        }
                        for (int i = minX; i < maxX; i++)
                        {
                            for (int j = minY; j < maxY; j++)
                            {
                                if (startY == user.y && startX == user.x)
                                {
                                    if (j - startY > radius + 1 ||
                                        j - startY < i - startX + 1 ||
                                        j - startY < -1 * (i - startX) + 1 ||
                                        (i == startX && j == startY))
                                        continue;
                                }
                                else if (startY - user.y >= user.x - startX && startY - user.y >= startX - user.x)
                                {
                                    if (j - startY > radius ||
                                        j - startY < i - startX ||
                                        j - startY < -1 * (i - startX))// ||  //+1
                                        //(i == startX && j == startY))
                                        continue;
                                }
                                else if (user.y - startY >= user.x - startX && user.y - startY >= startX - user.x)
                                {
                                    if (j - startY < -1 * radius ||
                                        j - startY > i - startX ||
                                        j - startY > -1 * (i - startX))// || //-1
                                        // (i == startX && j == startY))
                                        continue;
                                }
                                else if (startX - user.x > user.y - startY && startX - user.x > startY - user.y)
                                {
                                    if (i - startX > radius ||
                                        i - startX < j - startY ||
                                        i - startX < -1 * (j - startY))// || // + 1 
                                        //(i == startX && j == startY))
                                        continue;
                                }
                                else if (user.x - startX > user.y - startY && user.x - startX > startY - user.y)
                                {
                                    if (i - startX < -1 * radius ||
                                        i - startX > j - startY ||
                                        i - startX > -1 * (j - startY))// || //- 1
                                        //(i == startX && j == startY))
                                        continue;
                                }

                                if (sk.hitsAllies == false && checkPos(i, j) != null && checkPos(i, j).friendly == user.friendly)
                                    continue;
                                if (currentLevel.map[j, i] == DungeonMap.gr && user.fov.sight[j, i] > 0)
                                    currentLevel.highlightedTargetCells.Add(new Point(i, j), 1);
                            }
                        }
                    }

                    break;
                case SkillAreaKind.SingleTarget:
                    {
                        if (sk.hitsAllies == false && checkPos(startX, startY) != null && checkPos(startX, startY).friendly == user.friendly)
                            break;
                        if (user.fov.sight[startY, startX] <= 0)
                            break;
                        currentLevel.highlightedTargetCells.Add(new Point(startX, startY), 1);
                        break;
                    }
            }
        }
        public static void HighlightMove()
        {
            MessageBrowser.AddHint("Move the cursor (which surrounds the corners of a floor tile, changing color) with the arrow keys.", 10000.0);
            MessageBrowser.AddHint("After moving the cursor to a place within the green area, press Z to move your creature.", 10000.0);
            if (currentLevel.highlightedCells.Count == 0 && currentLevel.o_entities.ContainsKey(requestingMove))
            {
                highlightingOn = true;
                int highX = currentLevel.o_entities[requestingMove].x;
                int highY = currentLevel.o_entities[requestingMove].y;
                currentLevel.highlightedCells.Clear();
                currentLevel.doNotStopCells.Clear();
                currentLevel.doNotStopCells.Add(currentLevel.o_entities[requestingMove].pos, true);
                calculateAllMoves(highX, highY, currentLevel.o_entities[requestingMove].moveSpeed, true);
                /*
                highlightedCells = highlightedCells.Where(kv => o_entities[requestingMove].fov.sight.Contains(kv.Key)).ToDictionary(kv => kv.Key, kv => kv.Value);
                doNotStopCells = doNotStopCells.Where(kv => o_entities[requestingMove].fov.sight.Contains(kv.Key)).ToDictionary(kv => kv.Key, kv => kv.Value);
                 */
            }
        }
        public static void HighlightSkill()
        {
            MessageBrowser.AddHint("Move the cursor (which surrounds the corners of a floor tile, changing color) with the arrow keys. ", 10000.0);
            MessageBrowser.AddHint("After moving the red area with the cursor, press Z to attack everything with a red tile under it.", 10000.0);
            currentLevel.o_entities[requestingMove].currentSkill = currentLevel.o_entities[requestingMove].skillList[currentLevel.o_entities[requestingMove].ui.currentScreen.currentMenuItem];
            if (currentLevel.highlightedCells.Count == 0 && currentLevel.o_entities.ContainsKey(requestingMove))
            {
                highlightingOn = true;
                int highX = currentLevel.o_entities[requestingMove].x;
                int highY = currentLevel.o_entities[requestingMove].y;
                calculateAllMoves(highX, highY, currentLevel.o_entities[requestingMove].currentSkill.maxSkillDistance, false);
                removeMovesUnderMinimum(currentLevel, highX, highY, currentLevel.o_entities[requestingMove].currentSkill.minSkillDistance, false);
                HighlightSkillArea();

                currentLevel.highlightedCells = currentLevel.highlightedCells.Where(kv => currentLevel.o_entities[requestingMove].fov.sight[kv.Key.Y, kv.Key.X] > 0).ToDictionary(kv => kv.Key, kv => kv.Value);
                currentLevel.nonHighlightedFreeCells = currentLevel.nonHighlightedFreeCells.Where(kv => currentLevel.o_entities[requestingMove].fov.sight[kv.Key.Y, kv.Key.X] > 0).ToDictionary(kv => kv.Key, kv => kv.Value);

            }
        }
        public static void HighlightSkillArea()
        {
            currentLevel.highlightedTargetCells.Clear();
            if (currentLevel.o_entities.ContainsKey(requestingMove))
            {
                highlightingOn = true;
                currentLevel.o_entities[requestingMove].currentSkill = currentLevel.o_entities[requestingMove].skillList[currentLevel.o_entities[requestingMove].ui.currentScreen.currentMenuItem];
                if (!currentLevel.nonHighlightedFreeCells.ContainsKey(new Point(cursorX, cursorY)))
                    calculateAllTargets(currentLevel.o_entities[requestingMove], cursorX, cursorY, currentLevel.o_entities[requestingMove].currentSkill);
            }
        }
        public static void OnKeyDown_SelectMove(InputEventArgs e)
        {
            if (lockState && !lockForAnimation && mode == InputMode.Map
                && initiative[currentInitiative] == requestingMove &&
                             currentLevel.o_entities.ContainsKey(requestingMove) && currentLevel.o_entities[requestingMove].friendly)
            //                 o_entities[requestingMove].moveList.Count <= o_entities[requestingMove].maxMoveDistance)
            {

                if (e.KeyCode == KeyCode.Space)
                {
                    // o_entities[requestingMove].moveList.Add(Direction.None);
                }
                else if ((e.KeyCode == KeyCode.Left || (hjkl && e.KeyCode == KeyCode.H)) && cursorX > 0 && (currentLevel.map[cursorY, cursorX - 1] == 1194) &&
                    (checkPos(cursorX - 1, cursorY) == null || currentLevel.doNotStopCells.ContainsKey(new Point(cursorX - 1, cursorY))))
                {
                    if (currentLevel.highlightedCells.ContainsKey(new Point(cursorX - 1, cursorY)))
                    {
                        cursorX--;
                        currentLevel.o_entities[requestingMove].moveList.Add(Direction.West);
                    }
                }
                else if ((e.KeyCode == KeyCode.Right || (hjkl && e.KeyCode == KeyCode.L)) && cursorX < currentLevel.mapWidthBound && (currentLevel.map[cursorY, cursorX + 1] == 1194) &&
                    (checkPos(cursorX + 1, cursorY) == null || currentLevel.doNotStopCells.ContainsKey(new Point(cursorX + 1, cursorY))))
                {
                    if (currentLevel.highlightedCells.ContainsKey(new Point(cursorX + 1, cursorY)))
                    {
                        cursorX++;
                        currentLevel.o_entities[requestingMove].moveList.Add(Direction.East);
                    }
                }
                else if ((e.KeyCode == KeyCode.Up || (hjkl && e.KeyCode == KeyCode.K)) && cursorY > 0 && (currentLevel.map[cursorY - 1, cursorX] == 1194) &&
                    (checkPos(cursorX, cursorY - 1) == null || currentLevel.doNotStopCells.ContainsKey(new Point(cursorX, cursorY - 1))))
                {
                    if (currentLevel.highlightedCells.ContainsKey(new Point(cursorX, cursorY - 1)))
                    {
                        cursorY--;
                        currentLevel.o_entities[requestingMove].moveList.Add(Direction.North);
                    }
                }
                else if ((e.KeyCode == KeyCode.Down || (hjkl && e.KeyCode == KeyCode.J)) && cursorY < currentLevel.mapHeightBound && (currentLevel.map[cursorY + 1, cursorX] == 1194) &&
                    (checkPos(cursorX, cursorY + 1) == null || currentLevel.doNotStopCells.ContainsKey(new Point(cursorX, cursorY + 1))))
                {
                    if (currentLevel.highlightedCells.ContainsKey(new Point(cursorX, cursorY + 1)))
                    {
                        cursorY++;
                        currentLevel.o_entities[requestingMove].moveList.Add(Direction.South);
                    }
                }
                else if ((e.KeyCode == KeyCode.Left || (hjkl && e.KeyCode == KeyCode.H)) && cursorX > 0 && (currentLevel.map[cursorY, cursorX - 1] == 1187) && checkPos(cursorX - 1, cursorY) == null && checkFixture(cursorX - 1, cursorY, 1190) != null)
                {
                    currentLevel.map[cursorY, cursorX - 1] = 1194;
                    currentLevel.fixtures[new Point() { X = cursorX - 1, Y = cursorY }].tile = 1188;
                    currentLevel.o_entities[requestingMove].moveList.Add(Direction.None);
                    currentLevel.recalculateVision();
                    HighlightMove();
                }
                else if ((e.KeyCode == KeyCode.Right || (hjkl && e.KeyCode == KeyCode.L)) && cursorX < currentLevel.mapWidthBound && (currentLevel.map[cursorY, cursorX + 1] == 1187) && checkPos(cursorX + 1, cursorY) == null && checkFixture(cursorX + 1, cursorY, 1190) != null)
                {
                    currentLevel.map[cursorY, cursorX + 1] = 1194;
                    currentLevel.fixtures[new Point() { X = cursorX + 1, Y = cursorY }].tile = 1188;
                    currentLevel.o_entities[requestingMove].moveList.Add(Direction.None);
                    currentLevel.recalculateVision();
                    HighlightMove();
                }
                else if ((e.KeyCode == KeyCode.Up || (hjkl && e.KeyCode == KeyCode.K)) && cursorY > 0 && (currentLevel.map[cursorY - 1, cursorX] == 1187) && checkPos(cursorX, cursorY - 1) == null && checkFixture(cursorX, cursorY - 1, 1191) != null)
                {
                    currentLevel.map[cursorY - 1, cursorX] = 1194;
                    currentLevel.fixtures[new Point() { X = cursorX, Y = cursorY - 1 }].tile = 1189;
                    currentLevel.o_entities[requestingMove].moveList.Add(Direction.None);
                    currentLevel.recalculateVision();
                    HighlightMove();
                }
                else if ((e.KeyCode == KeyCode.Down || (hjkl && e.KeyCode == KeyCode.J)) && cursorY < currentLevel.mapHeightBound && (currentLevel.map[cursorY + 1, cursorX] == 1187) && checkPos(cursorX, cursorY + 1) == null && checkFixture(cursorX, cursorY + 1, 1191) != null)
                {
                    currentLevel.map[cursorY + 1, cursorX] = 1194;
                    currentLevel.fixtures[new Point() { X = cursorX, Y = cursorY + 1 }].tile = 1189;
                    currentLevel.o_entities[requestingMove].moveList.Add(Direction.None);
                    currentLevel.recalculateVision();
                    HighlightMove();
                }
                else if (e.KeyCode == ScreenBrowser.backKey)
                {
                    cursorX = currentLevel.o_entities[requestingMove].x;
                    cursorY = currentLevel.o_entities[requestingMove].y;
                    currentLevel.o_entities[requestingMove].moveList.Clear();
                    currentLevel.highlightedCells.Clear();
                    highlightingOn = false;
                    currentLevel.doNotStopCells.Clear();
                    ScreenBrowser.HandleRecall();
                    lockState = true;
                    ScreenBrowser.UnHide();
                    mode = InputMode.Menu;
                }
                else if (e.KeyCode == ScreenBrowser.confirmKey)
                {
                    //                    o_entities[requestingMove].moveList.AddRange(Enumerable.Repeat(Direction.None, o_entities[requestingMove].maxMoveDistance - o_entities[requestingMove].moveList.Count));
                    //}
                    //if (o_entities[requestingMove].moveList.Count == o_entities[requestingMove].maxMoveDistance)
                    // {

                    lockState = false;
                    highlightingOn = false;
                    currentLevel.o_entities[requestingMove].moveList.Clear();
                    MoveDirect(currentLevel.o_entities[requestingMove], new Point(cursorX, cursorY), currentLevel.highlightedCells, currentLevel.doNotStopCells);

                    //                    MoveMob(o_entities[requestingMove], o_entities[requestingMove].moveList);
                    currentLevel.highlightedCells.Clear();
                    currentLevel.doNotStopCells.Clear();
                    ScreenBrowser.HandleFinish();
                    // requestingMove.x = -1;
                    currentLevel.o_entities[requestingMove].moveList.Clear();

                    currentLevel.o_entities[requestingMove].actionCount++;
                    if (currentLevel.o_entities[requestingMove].actionCount > 1 && mode != InputMode.Dialog)
                    {
                        mode = InputMode.None;
                        //currentInitiative--;
                    }
                    else if (mode != InputMode.Dialog)
                    {
                        lockState = true;
                        ScreenBrowser.UnHide();
                        mode = InputMode.Menu;
                    }
                    else
                    {

                        DialogBrowser.UnHide();
                    }

                    MoveCursor(currentLevel.o_entities[requestingMove].x, currentLevel.o_entities[requestingMove].y);
                    //Update();

                    //                    singleEventLock = false;
                }
            }
        }

        public static void OnKeyDown_LookAround(InputEventArgs e)
        {
            if (lockState && !lockForAnimation && mode == InputMode.Map && initiative[currentInitiative] == requestingMove &&
                             currentLevel.o_entities.ContainsKey(requestingMove) && currentLevel.o_entities[requestingMove].friendly)
            //                             o_entities[requestingMove].moveList.Count <= o_entities[requestingMove].maxMoveDistance)
            {


                currentActor = null;
                if (e.KeyCode == KeyCode.Space)
                {
                    //o_entities[requestingMove].moveList.Add(Direction.None);
                }
                else if ((e.KeyCode == KeyCode.Left || (hjkl && e.KeyCode == KeyCode.H)) && cursorX > 0 && (currentLevel.map[cursorY, cursorX - 1] == 1194 || currentLevel.map[cursorY, cursorX - 1] == 1187)
                    )//&& visibleCells.Contains(new Point(cursorX - 1, cursorY)))   // && checkPos(cursorX - 1, cursorY) == null)
                {
                    cursorX--;
                    hoverActor = checkPos(cursorX, cursorY);
                }
                else if ((e.KeyCode == KeyCode.Right || (hjkl && e.KeyCode == KeyCode.L)) && cursorX < currentLevel.mapWidthBound && (currentLevel.map[cursorY, cursorX + 1] == 1194 || currentLevel.map[cursorY, cursorX + 1] == 1187)
                     )//&& visibleCells.Contains(new Point(cursorX + 1, cursorY)))
                {
                    cursorX++;
                    hoverActor = checkPos(cursorX, cursorY);
                }
                else if ((e.KeyCode == KeyCode.Up || (hjkl && e.KeyCode == KeyCode.K)) && cursorY > 0 && (currentLevel.map[cursorY - 1, cursorX] == 1194 || currentLevel.map[cursorY - 1, cursorX] == 1187)
                     )//&& visibleCells.Contains(new Point(cursorX, cursorY - 1)))
                {

                    cursorY--;
                    hoverActor = checkPos(cursorX, cursorY);
                }
                else if ((e.KeyCode == KeyCode.Down || (hjkl && e.KeyCode == KeyCode.J)) && cursorY < currentLevel.mapHeightBound && (currentLevel.map[cursorY + 1, cursorX] == 1194 || currentLevel.map[cursorY + 1, cursorX] == 1187)
                     )//&& visibleCells.Contains(new Point(cursorX, cursorY + 1)))
                {
                    cursorY++;
                    hoverActor = checkPos(cursorX, cursorY);
                }
                else if (e.KeyCode == ScreenBrowser.backKey || e.KeyCode == ScreenBrowser.confirmKey)
                {
                    cursorX = currentLevel.o_entities[requestingMove].x;
                    cursorY = currentLevel.o_entities[requestingMove].y;
                    currentActor = currentLevel.o_entities[requestingMove];
                    hoverActor = null;
                    //                    o_entities[requestingMove].moveList.Clear();
                    ScreenBrowser.HandleRecall();
                    lockState = true;
                    ScreenBrowser.UnHide();

                    mode = InputMode.Menu;
                }
                /*                else if (e.KeyCode == ScreenBrowser.confirmKey)
                                {
                                    //                    o_entities[requestingMove].moveList.AddRange(Enumerable.Repeat(Direction.None, o_entities[requestingMove].maxMoveDistance - o_entities[requestingMove].moveList.Count));
                                    //}
                                    //if (o_entities[requestingMove].moveList.Count == o_entities[requestingMove].maxMoveDistance)
                                    // {

                                    ScreenBrowser.HandleFinish();
                                    lockState = false;
                                    MoveMob(o_entities[requestingMove], o_entities[requestingMove].moveList.Take(o_entities[requestingMove].maxMoveDistance));
                                    // requestingMove.x = -1;
                                    o_entities[requestingMove].moveList.Clear();

                                    o_entities[requestingMove].actionCount++;
                                    if (o_entities[requestingMove].actionCount > 1)
                                    {
                                        Keyboard.KeyDown -= OnKeyDown_ActionMenu;
                                        currentInitiative--;
                                    }
                                    else
                                    {
                                        lockState = true;
                                        ScreenBrowser.UnHide();
                                    }

                                    cursorX = o_entities[requestingMove].x;
                                    cursorY = o_entities[requestingMove].y;
                                    //Update();
                                }*/

            }
        }
        public static void OnKeyDown_SelectSkill(InputEventArgs e)
        {
            if (lockState && !lockForAnimation && mode == InputMode.Map && initiative[currentInitiative] == requestingMove &&
                             currentLevel.o_entities.ContainsKey(requestingMove) && currentLevel.o_entities[requestingMove].friendly)
            // o_entities[requestingMove].moveList.Count <= o_entities[requestingMove].currentSkill.maxSkillDistance)
            {

                currentLevel.o_entities[requestingMove].currentSkill = currentLevel.o_entities[requestingMove].skillList[currentLevel.o_entities[requestingMove].ui.currentScreen.currentMenuItem];
                /*
                if (e.KeyCode == KeyCode.Space)
                {
                    o_entities[requestingMove].moveList.Add(Direction.None);
                }*/

                if (e.KeyCode == ScreenBrowser.backKey)
                {
                    cursorX = currentLevel.o_entities[requestingMove].x;
                    cursorY = currentLevel.o_entities[requestingMove].y;
                    currentLevel.o_entities[requestingMove].moveList.Clear();
                    currentLevel.nonHighlightedFreeCells.Clear();
                    currentLevel.highlightedCells.Clear();
                    currentLevel.highlightedTargetCells.Clear();
                    highlightingOn = false;
                    ScreenBrowser.HandleRecall();
                    lockState = true;
                    ScreenBrowser.UnHide();
                    mode = InputMode.Menu;

                }
                else if ((e.KeyCode == KeyCode.Left || (hjkl && e.KeyCode == KeyCode.H)) && cursorX > 0 && (currentLevel.map[cursorY, cursorX - 1] == 1194))
                {
                    if (currentLevel.highlightedCells.ContainsKey(new Point(cursorX - 1, cursorY)) || currentLevel.nonHighlightedFreeCells.ContainsKey(new Point(cursorX - 1, cursorY)))
                    {
                        cursorX--;
                        currentLevel.o_entities[requestingMove].moveList.Add(Direction.West);
                        HighlightSkillArea();
                    }
                    hoverActor = checkPos(cursorX, cursorY);
                }
                else if ((e.KeyCode == KeyCode.Right || (hjkl && e.KeyCode == KeyCode.L)) && cursorX < currentLevel.mapWidthBound && (currentLevel.map[cursorY, cursorX + 1] == 1194))
                {
                    if (currentLevel.highlightedCells.ContainsKey(new Point(cursorX + 1, cursorY)) || currentLevel.nonHighlightedFreeCells.ContainsKey(new Point(cursorX + 1, cursorY)))
                    {
                        cursorX++;
                        currentLevel.o_entities[requestingMove].moveList.Add(Direction.East);
                        HighlightSkillArea();
                    }
                    hoverActor = checkPos(cursorX, cursorY);
                }
                else if ((e.KeyCode == KeyCode.Up || (hjkl && e.KeyCode == KeyCode.K)) && cursorY > 0 && (currentLevel.map[cursorY - 1, cursorX] == 1194))
                {
                    if (currentLevel.highlightedCells.ContainsKey(new Point(cursorX, cursorY - 1)) || currentLevel.nonHighlightedFreeCells.ContainsKey(new Point(cursorX, cursorY - 1)))
                    {
                        cursorY--;
                        currentLevel.o_entities[requestingMove].moveList.Add(Direction.North);
                        HighlightSkillArea();
                    }
                    hoverActor = checkPos(cursorX, cursorY);
                }
                else if ((e.KeyCode == KeyCode.Down || (hjkl && e.KeyCode == KeyCode.J)) && cursorY < currentLevel.mapHeightBound && (currentLevel.map[cursorY + 1, cursorX] == 1194))
                {
                    if (currentLevel.highlightedCells.ContainsKey(new Point(cursorX, cursorY + 1)) || currentLevel.nonHighlightedFreeCells.ContainsKey(new Point(cursorX, cursorY + 1)))
                    {
                        cursorY++;
                        currentLevel.o_entities[requestingMove].moveList.Add(Direction.South);
                        HighlightSkillArea();
                    }
                    hoverActor = checkPos(cursorX, cursorY);
                }



                /*

                if ((e.KeyCode == KeyCode.Left || (hjkl && e.KeyCode == KeyCode.H)) && cursorX > 0 && (map[cursorY, cursorX - 1] == 1194))// || map[cursorY, cursorX - 1] == 1187))// && checkPos(cursorX - 1, cursorY) == null)
                {
                    cursorX--;
                    if (o_entities[requestingMove].moveList.Count > 0 && o_entities[requestingMove].moveList[o_entities[requestingMove].moveList.Count - 1] == Direction.East)
                        o_entities[requestingMove].moveList.RemoveAt(o_entities[requestingMove].moveList.Count - 1);
                    else
                        o_entities[requestingMove].moveList.Add(Direction.West);
                }
                else if ((e.KeyCode == KeyCode.Right || (hjkl && e.KeyCode == KeyCode.L)) && cursorX < mapWidth && (map[cursorY, cursorX + 1] == 1194))// || map[cursorY, cursorX + 1] == 1187))// && checkPos(cursorX + 1, cursorY) == null)
                {
                    cursorX++;
                    if (o_entities[requestingMove].moveList.Count > 0 && o_entities[requestingMove].moveList[o_entities[requestingMove].moveList.Count - 1] == Direction.West)
                        o_entities[requestingMove].moveList.RemoveAt(o_entities[requestingMove].moveList.Count - 1);
                    else
                        o_entities[requestingMove].moveList.Add(Direction.East);
                }
                else if ((e.KeyCode == KeyCode.Up || (hjkl && e.KeyCode == KeyCode.K)) && cursorY > 0 && (map[cursorY - 1, cursorX] == 1194))// || map[cursorY - 1, cursorX] == 1187))// && checkPos(cursorX, cursorY - 1) == null)
                {
                    cursorY--;
                    if (o_entities[requestingMove].moveList.Count > 0 && o_entities[requestingMove].moveList[o_entities[requestingMove].moveList.Count - 1] == Direction.South)
                        o_entities[requestingMove].moveList.RemoveAt(o_entities[requestingMove].moveList.Count - 1);
                    else
                        o_entities[requestingMove].moveList.Add(Direction.North);
                }
                else if ((e.KeyCode == KeyCode.Down || (hjkl && e.KeyCode == KeyCode.J)) && cursorY < mapHeight && (map[cursorY + 1, cursorX] == 1194)) // || map[cursorY + 1, cursorX] == 1187// && checkPos(cursorX, cursorY + 1) == null)
                {
                    cursorY++;
                    if (o_entities[requestingMove].moveList.Count > 0 && o_entities[requestingMove].moveList[o_entities[requestingMove].moveList.Count - 1] == Direction.North)
                        o_entities[requestingMove].moveList.RemoveAt(o_entities[requestingMove].moveList.Count - 1);
                    else
                        o_entities[requestingMove].moveList.Add(Direction.South);
                }*/
                else if (e.KeyCode == ScreenBrowser.confirmKey)// && o_entities[requestingMove].moveList.Count >= o_entities[requestingMove].currentSkill.minSkillDistance)
                {
                    Point cursorPos = new Point() { X = cursorX, Y = cursorY };
                    if (currentLevel.nonHighlightedFreeCells.ContainsKey(cursorPos))
                    {
                        MessageBrowser.AddMessage("Minimum range not met, move the cursor into a highlighted area.", 5000.0, true);
                        return;
                    }
                    currentLevel.o_entities[requestingMove].currentSkill.targetSquare = cursorPos;

                    hoverActor = null;
                    //                    o_entities[requestingMove].hasActed = true;

                    SkillResult sa = currentLevel.o_entities[requestingMove].currentSkill.ApplySkill(currentLevel.o_entities[requestingMove]);
                    highlightingOn = false;
                    AnimateResults(sa);
                    currentLevel.highlightedCells.Clear();
                    currentLevel.highlightedTargetCells.Clear();
                    currentLevel.nonHighlightedFreeCells.Clear();
                    ScreenBrowser.HandleFinish();
                    if (currentLevel.o_entities.ContainsKey(requestingMove) == false)
                    {
                        mode = InputMode.None;
                        //currentInitiative--;
                        lockState = false;
                        return;
                    }
                    currentLevel.o_entities[requestingMove].actionCount++;
                    if (currentLevel.o_entities[requestingMove].actionCount > 1)
                    {
                        mode = InputMode.None;
                        //currentInitiative--;
                    }
                    else
                    {
                        lockState = true;
                        ScreenBrowser.UnHide();
                        mode = InputMode.Menu;
                    }


                    // requestingMove.x = -1;
                    currentLevel.o_entities[requestingMove].moveList.Clear();
                    //Update();
                    MoveCursor(currentLevel.o_entities[requestingMove].x, currentLevel.o_entities[requestingMove].y);
                    currentActor = currentLevel.o_entities[requestingMove];
                    lockState = false;

                }
            }

        }
        public static void waitAction()
        {
            lockState = false;
            currentLevel.o_entities[requestingMove].actionCount = 2;
            mode = InputMode.None;
            //currentInitiative--;
            currentLevel.o_entities[requestingMove].moveList.Clear();

            ScreenBrowser.HandleFinish();

            cursorX = currentLevel.o_entities[requestingMove].x;
            cursorY = currentLevel.o_entities[requestingMove].y;
        }



    }
}
