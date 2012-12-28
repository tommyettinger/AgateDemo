
using System;
using System.Linq;
using System.Collections.Generic;
using AgateLib;
using AgateLib.DisplayLib;
using AgateLib.Geometry;
using AgateLib.InputLib;

namespace AgateDemo
{
    public struct Cell
    {
        public int x, y;
        public Cell(int x, int y)
        {
            this.x = x; this.y = y;
        }
        public override bool Equals(Object obj)
        {
            return obj is Cell && this == (Cell)obj;
        }
        public override int GetHashCode()
        {
            return x.GetHashCode() ^ y.GetHashCode();
        }
        public static bool operator ==(Cell a, Cell b)
        {
            return a.x == b.x && a.y == b.y;
        }
        public static bool operator !=(Cell a, Cell b)
        {
            return !(a == b);
        }
    }
    public static class Demo
    {
        static Random rnd = new Random();
        public static int minVisibleX = 0, minVisibleY = 0, maxVisibleX = 19, maxVisibleY = 19;

        public enum Direction { North, East, South, West, None };
        //public static List<Direction> dirlist = new List<Direction>();
        public class Entity
        {
            public int tile;
            public Cell pos;
            public int x
            {
                get { return pos.x; }
                set { pos.x = value; }
            }
            public int y
            {
                get { return pos.y; }
                set { pos.y = value; }
            }
        }
        public class Mob : Entity
        {
            private int hp;
            public int maxHP = 10;
            public int health
            {
                get { return hp; }
                set
                {
                    hp = value;
                    if (hp <= 0)
                        this.Kill();
                    else if (hp > maxHP)
                        hp = maxHP;
                }
            }
            public bool friendly;
            public Cell o_pos;
            public string name;
            public List<Direction> moveList = new List<Direction>();
            public int maxMoveDistance = 3, actionCount = 0;
            public List<Skill> skillList;
            public Skill currentSkill;
            public SimpleUI ui = null;

            public Mob(int tileNumber, int xPos, int yPos, bool isFriendly)
            {
                tile = tileNumber;
                name = TileData.tilesToNames[tileNumber];
                x = xPos;
                y = yPos;
                o_pos = new Cell() { x = this.x, y = this.y };
                friendly = isFriendly;
                if (friendly)
                {
                    maxMoveDistance = 6;
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
                Mob mb = new Mob(this.tile, this.x, this.y, this.friendly);
                //mb.initiative = this.initiative;
                mb.hp = this.hp;
                mb.o_pos = this.o_pos;
                mb.actionCount = 2;
                return mb;
            }
            public void Kill()
            {
                entities.Remove(this.pos);
                o_entities.Remove(this.o_pos);
                try
                {
                    initiative.Remove(initiative.First(e => e.Value == this.o_pos).Key);
                }
                catch (Exception)
                { }
            }
        }
        public static SortedDictionary<int, Cell> initiative = new SortedDictionary<int, Cell>();
        public static int currentInitiative;
        public static bool lockState = false, lockForAnimation = false, showHealth = false;
        static Cell requestingMove = new Cell() { x = -1, y = -1 };
        public static Mob currentActor = null, hoverActor = null;
        static Surface tileset;
        public static int[,] map, map2;
        public static Dictionary<Cell, Mob> entities, o_entities;
        public static Dictionary<Cell, int> highlightedCells = new Dictionary<Cell, int>(), highlightedTargetCells = new Dictionary<Cell, int>();
        public static Dictionary<Cell, Entity> fixtures;

        public static Dictionary<Cell, int> displayDamage = new Dictionary<Cell, int>();
        public static Dictionary<Cell, bool> displayKills = new Dictionary<Cell, bool>();
        // public static SimpleUI basicUI;

        static int tileWidth = 48;
        static int tileHeight = 64;
        static int tileHIncrease = 16;
        static int tileVIncrease = 32;
        static int mapWidth;
        static int mapHeight;
        static int cursorX = 3;
        static int cursorY = 3;
        static FontSurface mandrillFont;

        static DisplayWindow wind;
        public static Mob checkPos(int checkX, int checkY)
        {
            Mob ret = null;
            entities.TryGetValue(new Cell() { x = checkX, y = checkY }, out ret);
            return ret;
        }
        public static Mob checkPos(int checkX, int checkY, int checkTile)
        {
            Mob ret = null;
            entities.TryGetValue(new Cell() { x = checkX, y = checkY }, out ret);
            if (ret != null && ret.tile != checkTile)
                ret = null;
            return ret;
        }
        public static Entity checkFixture(int checkX, int checkY)
        {
            Entity ret = null;
            fixtures.TryGetValue(new Cell() { x = checkX, y = checkY }, out ret);
            return ret;
        }
        public static Entity checkFixture(int checkX, int checkY, int checkTile)
        {
            Entity ret = null;
            fixtures.TryGetValue(new Cell() { x = checkX, y = checkY }, out ret);
            if (ret != null && ret.tile != checkTile)
                ret = null;
            return ret;
        }
        public static void AnimateResults(SkillResult skr)
        {

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
            }
            foreach (Cell c in skr.damages.Keys)
            {
                //while (Timing.TotalMilliseconds - startingTime < 200 && (ent.x >= minVisibleX && ent.x <= maxVisibleX && ent.y >= minVisibleY && ent.y <= maxVisibleY)) ;
                if (c.x >= minVisibleX && c.x <= maxVisibleX && c.y >= minVisibleY && c.y <= maxVisibleY)
                {
                    currentActor = null;
                    lockForAnimation = true;
                    displayDamage.Add(c, skr.damages[c]);
                }
            }
            double startingTime = Timing.TotalMilliseconds;
            if (displayDamage.Count > 0)
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
                displayDamage.Clear();
            }
            startingTime = Timing.TotalMilliseconds;
            foreach (Cell c in skr.kills.Keys)
            {
                //while (Timing.TotalMilliseconds - startingTime < 200 && (ent.x >= minVisibleX && ent.x <= maxVisibleX && ent.y >= minVisibleY && ent.y <= maxVisibleY)) ;
                if (c.x >= minVisibleX && c.x <= maxVisibleX && c.y >= minVisibleY && c.y <= maxVisibleY)
                {
                    currentActor = null;
                    lockForAnimation = true;
                    displayKills.Add(c, skr.kills[c]);
                }
            }
            if (displayKills.Count > 0)
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
                displayKills.Clear();
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
            }
            double startingTime = Timing.TotalMilliseconds;
            foreach (Direction currMove in movepath)
            {

                //while (Timing.TotalMilliseconds - startingTime < 200 && (ent.x >= minVisibleX && ent.x <= maxVisibleX && ent.y >= minVisibleY && ent.y <= maxVisibleY)) ;
                if (ent.x >= minVisibleX && ent.x <= maxVisibleX && ent.y >= minVisibleY && ent.y <= maxVisibleY)
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
                entities.Remove(ent.pos);

                switch (currMove)
                {
                    case Direction.West: if (ent.x > 0 && (map[ent.y, ent.x - 1] == 1194) && checkPos(ent.x - 1, ent.y) == null) //entities.FirstOrDefault(e => e.x == ent.x - 1 && e.y == ent.y)
                        {
                            ent.x--;
                        }
                        else if (ent.x > 0 && (map[ent.y, ent.x - 1] == 1187) && checkFixture(ent.x - 1, ent.y, 1190) != null)
                        {
                            map[ent.y, ent.x - 1] = 1194;
                            fixtures[new Cell() { x = ent.x - 1, y = ent.y }].tile = 1188;
                        }
                        break;
                    case Direction.North: if (ent.y > 0 && (map[ent.y - 1, ent.x] == 1194) && checkPos(ent.x, ent.y - 1) == null)
                        {
                            ent.y--;
                        }
                        else if (ent.y > 0 && (map[ent.y - 1, ent.x] == 1187) && checkFixture(ent.x, ent.y - 1, 1191) != null)
                        {
                            map[ent.y - 1, ent.x] = 1194;
                            fixtures[new Cell() { x = ent.x, y = ent.y - 1 }].tile = 1189;
                            //                        fixtures.FirstOrDefault(e => e.x == ent.x && e.y == ent.y - 1 && e.tile == 1191).tile = 1189;
                        }
                        break;
                    case Direction.East: if (ent.x > 0 && (map[ent.y, ent.x + 1] == 1194) && checkPos(ent.x + 1, ent.y) == null) //entities.FirstOrDefault(e => e.x == ent.x - 1 && e.y == ent.y)
                        {
                            ent.x++;
                        }
                        else if (ent.x > 0 && (map[ent.y, ent.x + 1] == 1187) && checkFixture(ent.x + 1, ent.y, 1190) != null)
                        {
                            map[ent.y, ent.x + 1] = 1194;
                            fixtures[new Cell() { x = ent.x + 1, y = ent.y }].tile = 1188;
                        }
                        break;
                    case Direction.South: if (ent.y > 0 && (map[ent.y + 1, ent.x] == 1194) && checkPos(ent.x, ent.y + 1) == null)
                        {
                            ent.y++;
                        }
                        else if (ent.y > 0 && (map[ent.y + 1, ent.x] == 1187) && checkFixture(ent.x, ent.y + 1, 1191) != null)
                        {
                            map[ent.y + 1, ent.x] = 1194;
                            fixtures[new Cell() { x = ent.x, y = ent.y + 1 }].tile = 1189;
                        }
                        break;
                }
                entities[ent.pos] = ent;
                requestingMove.x = ent.o_pos.x;
                requestingMove.y = ent.o_pos.y;
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

        public static void MoveRandom(Mob ent)
        {
            //entities.Remove(ent.pos);

            int randVal;
            foreach (int any in new int[] { 0, 0, 0 })
            {
                randVal = rnd.Next(4);
                switch (randVal)
                {/*
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
              */
                    case 0: ent.moveList.Add(Direction.West);
                        break;
                    case 1: ent.moveList.Add(Direction.North);
                        break;
                    case 2: ent.moveList.Add(Direction.East);
                        break;
                    case 3: ent.moveList.Add(Direction.South);
                        break;
                }
            }
            MoveMob(ent, ent.moveList);
            Cell c = ent.pos;
            Cell cEast = new Cell(c.x + 1, c.y);
            Cell cWest = new Cell(c.x - 1, c.y);
            Cell cNorth = new Cell(c.x, c.y - 1);
            Cell cSouth = new Cell(c.x, c.y + 1);
            if (entities.ContainsKey(cEast))
            {
                ent.currentSkill.targetSquare = cEast;
                AnimateResults(ent.currentSkill.ApplySkill(ent));
            }
            else if (entities.ContainsKey(cWest))
            {
                ent.currentSkill.targetSquare = cWest;
                AnimateResults(ent.currentSkill.ApplySkill(ent));
            }
            else if (entities.ContainsKey(cNorth))
            {
                ent.currentSkill.targetSquare = cNorth;
                AnimateResults(ent.currentSkill.ApplySkill(ent));
            }
            else if (entities.ContainsKey(cSouth))
            {
                ent.currentSkill.targetSquare = cSouth;
                AnimateResults(ent.currentSkill.ApplySkill(ent));
            }
            ent.moveList.Clear();
            /*                entities[ent.pos] = ent;
                            requestingMove.x = ent.o_pos.x;
                            requestingMove.y = ent.o_pos.y;
             */

        }

        static Mob Spawn(int tileNo, int width, int height)
        {
            int rx = rnd.Next(width);
            int ry = rnd.Next(height);
            if (map[ry, rx] == DungeonMap.gr)
            {
                Mob nt = new Mob(tileNo, rx, ry, false);

                if (checkPos(nt.x, nt.y) != null)
                    return Spawn(tileNo, width, height);
                nt.skillList.Add(new Skill("Basic Attack", rnd.Next(3, 7), 1));
                nt.currentSkill = nt.skillList[0];
                entities[nt.pos] = nt;
                o_entities[nt.o_pos] = nt;
                return nt;
            }
            return Spawn(tileNo, width, height);
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
};
            int numMorphs = DungeonMap.geomorphs.Count;
            map = DungeonMap.merge(DungeonMap.geomorphs[0], DungeonMap.geomorphs[rnd.Next(numMorphs)], false);
            for (int eh = 2; eh < 4; eh++)
            {
                if (rnd.Next(2) == 0)
                    map = DungeonMap.merge(map, DungeonMap.geomorphs[rnd.Next(numMorphs)], false);
                else
                    map = DungeonMap.merge(map, DungeonMap.rotateCW(DungeonMap.geomorphs[rnd.Next(numMorphs)]), false);
            }
            for (int ah = 1; ah < 4; ah++)
            {
                map2 = DungeonMap.merge(DungeonMap.geomorphs[rnd.Next(numMorphs)], DungeonMap.geomorphs[rnd.Next(numMorphs)], false);
                for (int eh = 2; eh < 4; eh++)
                {
                    if (rnd.Next(2) == 0)
                        map2 = DungeonMap.merge(map2, DungeonMap.geomorphs[rnd.Next(numMorphs)], false);
                    else
                        map2 = DungeonMap.merge(map2, DungeonMap.rotateCW(DungeonMap.geomorphs[rnd.Next(numMorphs)]), false);
                }
                map = DungeonMap.merge(map, map2, true);
            }

            fixtures = new Dictionary<Cell, Entity>()
            {
            };
            fixtures.Add(new Cell() { x = 10, y = 10 }, new Entity() { tile = 1203, x = 10, y = 10 });
            fixtures.Add(new Cell() { x = 12, y = 11 }, new Entity() { tile = 1206, x = 12, y = 11 });
            fixtures.Add(new Cell() { x = 14, y = 10 }, new Entity() { tile = 1197, x = 14, y = 10 });

            /*,
           new Entity() { tile = 1189, x = 2, y = 4},
           new Entity() { tile = 1189, x = 10, y = 13},
           new Entity() { tile = 1188, x = 6, y = 15},
           new Entity() { tile = 1188, x = 11, y = 17},*/

            map = DungeonMap.cleanUp(map);
            map = DungeonMap.theme(map);

            // map = DungeonMap.geomorph;
            int mw = map.GetLength(1), mh = map.GetLength(0);
            entities = new Dictionary<Cell, Mob>()
            {
            };
            o_entities = new Dictionary<Cell, Mob>()
            {
            };

            Mob nt;
            nt = new Mob(541, 6, 7, true); //beholder
            nt.skillList.Add(new Skill("Eye Beam", 3, 8));
            nt.skillList.Add(new Skill("Disintegrate", 10, 2));
            nt.skillList.Add(new Skill("Horrid Glare", 2, 0, 1, SkillAreaKind.Spray, 5, false));
            nt.ui.addSkills(nt);
            entities[nt.pos] = nt;
            o_entities[nt.o_pos] = nt;
            nt = new Mob(541, 6, 8, true); //beholder
            nt.skillList.Add(new Skill("Eye Beam", 3, 8));
            nt.skillList.Add(new Skill("Disintegrate", 10, 2));
            nt.skillList.Add(new Skill("Horrid Glare", 2, 0, 1, SkillAreaKind.Spray, 5, false));
            nt.ui.addSkills(nt);
            entities[nt.pos] = nt;
            o_entities[nt.o_pos] = nt;
            nt = new Mob(541, 6, 9, true); //beholder
            nt.skillList.Add(new Skill("Eye Beam", 3, 8));
            nt.skillList.Add(new Skill("Disintegrate", 10, 2));
            nt.skillList.Add(new Skill("Horrid Glare", 2, 0, 1, SkillAreaKind.Spray, 5, false));
            nt.ui.addSkills(nt);
            entities[nt.pos] = nt;
            o_entities[nt.o_pos] = nt;
            nt = new Mob(503, 15, 4, true); //demogorgon
            nt.skillList.Add(new Skill("Tentacle Flail", 3, 0, 0, SkillAreaKind.Ring, 2, true));
            nt.skillList.Add(new Skill("Vicious Bites", 10, 1));
            nt.ui.addSkills(nt);
            entities[nt.pos] = nt;
            o_entities[nt.o_pos] = nt;
            nt = new Mob(1409, 4, 18, true); //drow
            nt.skillList.Add(new Skill("Sword Slash", 5, 0, 0, SkillAreaKind.Ring, 1, false));
            nt.skillList.Add(new Skill("Crossbow", 3, 6));
            nt.ui.addSkills(nt);
            entities[nt.pos] = nt;
            o_entities[nt.o_pos] = nt;
            nt = new Mob(1406, 4, 3, true); //baku
            nt.skillList.Add(new Skill("Tusk Attack", 6, 1));
            nt.skillList.Add(new Skill("Trunk Slap", 2, 0, 0, SkillAreaKind.Ring, 3, true));
            Skill heal = new Skill("Rapid Healing", -5, 0);
            heal.minSkillDistance = 0;
            nt.skillList.Add(heal);
            nt.ui.addSkills(nt);
            entities[nt.pos] = nt;
            o_entities[nt.o_pos] = nt;
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
            /*for (int i = 0, c = 0; i < 222; c++, i++)//= rnd.Next(2, 7)) //check for c to limit number of entities
            {
                Spawn(i, mw, mh);
            }*/
            for (int i = 226; i < 434; i++)
            {
                Spawn(i, mw, mh);
            }
            for (int i = 473; i < 542; i++)
            {
                Spawn(i, mw, mh);
            }

            foreach (Cell cl in entities.Keys)
            {
                int curr = rnd.Next(10000);
                while (initiative.ContainsKey(curr))
                {
                    curr = rnd.Next(10000);
                }
                initiative[curr] = cl;
            }
            currentInitiative = initiative.Keys.Max();

            var numGroundTiles = 0;
            foreach (int eh in map)
            {
                if (eh == DungeonMap.gr || eh == 1187)
                    numGroundTiles++;
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
            cursorX = 6;
            cursorY = 7;

            //wind = DisplayWindow.CreateWindowed("Vicious Demo with AgateLib", ((mapWidth + 1) * 32) + (tileHIncrease * (1 + mapHeight)), (mapHeight * tileVIncrease) + tileHeight);

            Display.RenderState.WaitForVerticalBlank = true;
            wind = DisplayWindow.CreateWindowed("Vicious Demo with AgateLib", 960, 672);//(19 * tileVIncrease) + tileHeight); //((20) * 32) + (tileHIncrease * (20))

            tileset = new Surface("Resources" + "/" + "slashem-revised.png"); //System.IO.Path.DirectorySeparatorChar

            mandrillFont = FontSurface.BitmapMonospace("Resources" + "/" + "monkey_x2.png", new Size(12, 28));
            mandrillFont.Color = Color.LightSkyBlue;
            ScreenBrowser.Init();
            //ScreenBrowser.currentUI.currentScreen.title = "Mobs with Jobs!";
            ScreenBrowser.isHidden = true;
            //ScreenBrowser.Show();
            //            basicUI = new SimpleUI(new Screen("Mobs with Jobs!", new List<MenuItem>() { }), mandrillFont);
            Keyboard.KeyUp += new InputEventHandler(onKeyUp);
        }

        public static void Update()
        {

            if (lockState || lockForAnimation) //requestingMove.x >= 0 || 
            {
                return;
            }
            for (int i = currentInitiative; i >= 0; i--, currentInitiative--)    //each (Mob ent in entities.Values.OrderByDescending(n => (n.hasActed) ? -1 : n.initiative))
            {
                if (initiative.ContainsKey(i))
                {
                    Mob ent = o_entities[initiative[i]];
                    if (ent.actionCount > 1)
                        continue;
                    if (ent.friendly == false)
                    {
                        MoveRandom(ent);
                        ent.actionCount = 2;
                        //requestingMove.x = -1;
                        lockState = false;
                    }
                    else
                    {
                        currentActor = ent;
                        cursorX = ent.x;
                        cursorY = ent.y;
                        requestingMove.x = ent.o_pos.x;
                        requestingMove.y = ent.o_pos.y;
                        lockState = true;
                        ScreenBrowser.currentUI = ent.ui;
                        ScreenBrowser.UnHide();
                        if (ent.actionCount < 1)
                        {
                            ScreenBrowser.Refresh();
                            Keyboard.KeyDown += new InputEventHandler(OnKeyDown_ActionMenu);
                        }
                        cursorX = o_entities[initiative[currentInitiative]].x;
                        cursorY = o_entities[initiative[currentInitiative]].y;
                        break;
                    }
                }
            }
            if (currentInitiative <= initiative.Keys.Min())
            {
                initiative.Clear();
                foreach (Cell cl in o_entities.Keys)
                {
                    o_entities[cl].actionCount = 0;
                    int curr = rnd.Next(10000);
                    while (initiative.ContainsKey(curr))
                    {
                        curr = rnd.Next(10000);
                    }
                    initiative[curr] = cl;
                }
                currentInitiative = initiative.Keys.Max();
                Update();
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
            for (int row = (cursorY < 20) ? 0 : (cursorY > mapHeight - 10) ? mapHeight - 20 : cursorY - 20; row <= mapHeight && row <= cursorY + 20; row++)
            {
                // maxVisibleY++;
                var pY = tileVIncrease * ((cursorY <= 10) ? row : (cursorY > mapHeight - 10) ? row - (mapHeight - 19) : row - (cursorY - 10)); //   //(cursorY > mapHeight - 10) ? mapHeight - (cursorY - 10) : cursorY - 10)
                var pX = tileHIncrease * (20 - 1 - ((cursorY <= 10) ? row : (cursorY > mapHeight - 10) ? row - (mapHeight - 19) : row - (cursorY - 10)));// +tileHIncrease; //row - (cursorY - 10)

                //minVisibleX = (cursorX <= 10) ? 0 : (cursorX > mapWidth - 10) ? mapWidth - 19 : cursorX - 10;
                //maxVisibleX = minVisibleX;
                for (var col = (cursorX <= 10) ? 0 : (cursorX > mapWidth - 10) ? mapWidth - 19 : cursorX - 10; col <= mapWidth && (col < cursorX + 10 || col < 20); col++)
                {
                    // maxVisibleX++;
                    if (map[row, col] == DungeonMap.gr)
                    {
                        if (highlightedTargetCells.ContainsKey(new Cell(col, row)))
                        {
                            tileset.Color = Color.FromHsv(10.0, 0.7, 0.70 + (((Timing.TotalMilliseconds % 2000) < 1000) ? (Timing.TotalMilliseconds % 2000) / 4000 : (2000 - (Timing.TotalMilliseconds % 2000)) / 4000));
                        }
                        else if (highlightedCells.ContainsKey(new Cell(col, row)))
                        {
                            tileset.Color = Color.FromHsv(190.0, 0.5, 0.75 + (((Timing.TotalMilliseconds % 2000) < 1000) ? (Timing.TotalMilliseconds % 2000) / 4000 : (2000 - (Timing.TotalMilliseconds % 2000)) / 4000));
                        }
                        var dest = new Rectangle(pX, pY, tileWidth, tileHeight);
                        var tile = map[row, col];
                        var src = new Rectangle((tile % 38) * tileWidth, (tile / 38) * tileHeight, tileWidth, tileHeight);
                        tileset.Draw(src, dest);
                        tileset.Color = Color.White;
                    }
                    pX += tileVIncrease;
                }
                pX = tileHIncrease * (20 - 1 - ((cursorY <= 10) ? row : (cursorY > mapHeight - 10) ? row - (mapHeight - 19) : row - (cursorY - 10)));
                for (var col = (cursorX <= 10) ? 0 : (cursorX > mapWidth - 10) ? mapWidth - 19 : cursorX - 10; col <= mapWidth && (col < cursorX + 10 || col < 20); col++)
                {
                    var dest = new Rectangle(pX, pY, tileWidth, tileHeight);
                    var entity = checkPos(col, row);
                    var fixture = checkFixture(col, row);
                    if (cursorX == col && cursorY == row && lockState)
                    {
                        if (lockState)
                            tileset.Color = Color.FromHsv((Timing.TotalMilliseconds % 1800) / 5.0, 1.0, 1.0);
                        int offset = 0;
                        if (entity != null && entity.x == cursorX && entity.y == cursorY)
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
                        src = new Rectangle((tile % 38) * tileWidth, (tile / 38) * tileHeight, tileWidth, tileHeight);
                        tileset.Draw(src, dest);
                    }
                    if (entity != null)
                    {
                        tile = entity.tile;
                        src = new Rectangle((tile % 38) * tileWidth, (tile / 38) * tileHeight, tileWidth, tileHeight);
                        Color tsc;// = Color.FromHsv((Timing.TotalMilliseconds % 1800) / 5.0, 0.5, 1.0);
                        if (lockState && entity.friendly && requestingMove.x == entity.o_pos.x && requestingMove.y == entity.o_pos.y)
                        {
                            //tileset.Color = Color.FromHsv((Timing.TotalMilliseconds % 1800) / 5.0, 0.5, 1.0);
                            tileset.Draw(src, dest);
                            tileset.Color = Color.White;
                        }
                        else
                            tileset.Draw(src, dest);

                        if (cursorX == col && cursorY == row && lockState)
                        {
                            if (lockState)
                                tileset.Color = Color.FromHsv((Timing.TotalMilliseconds % 1800) / 5.0, 1.0, 1.0);
                            int offset = 0;
                            if (entity.x == cursorX && entity.y == cursorY)
                                offset = (int)(((Timing.TotalMilliseconds % 2000) < 1000) ? (Timing.TotalMilliseconds % 2000) / 40 : (2000 - (Timing.TotalMilliseconds % 2000)) / 40);
                            tileset.Draw(new Rectangle((1440 % 38) * tileWidth, (1440 / 38) * tileHeight, tileWidth, tileHeight), new Rectangle(pX, pY - offset, tileWidth, tileHeight));
                            tileset.Color = Color.White;
                        }
                        if (entity.friendly)
                        {
                            src = new Rectangle((1443 % 38) * tileWidth, (1443 / 38) * tileHeight, tileWidth, tileHeight);
                            tileset.Draw(src, dest);
                        }
                        if (showHealth)
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

                        if (cursorX == col && cursorY == row && lockState)
                        {
                            if (lockState)
                                tileset.Color = Color.FromHsv((Timing.TotalMilliseconds % 1800) / 5.0, 1.0, 1.0);
                            int offset = 0;
                            tileset.Draw(new Rectangle((backTile % 38) * tileWidth, (backTile / 38) * tileHeight, tileWidth, tileHeight), new Rectangle(pX, pY - offset, tileWidth, tileHeight));
                            tileset.Color = Color.White;
                        }
                        tile = fixture.tile;
                        src = new Rectangle((tile % 38) * tileWidth, (tile / 38) * tileHeight, tileWidth, tileHeight);
                        tileset.Draw(src, dest);
                        if (cursorX == col && cursorY == row && lockState)
                        {
                            if (lockState)
                                tileset.Color = Color.FromHsv((Timing.TotalMilliseconds % 1800) / 5.0, 1.0, 1.0);
                            int offset = 0;
                            tileset.Draw(new Rectangle((frontTile % 38) * tileWidth, (frontTile / 38) * tileHeight, tileWidth, tileHeight), new Rectangle(pX, pY - offset, tileWidth, tileHeight));
                            tileset.Color = Color.White;
                        }

                    }
                    //     mandrillFont.Alpha = ((Timing.TotalMilliseconds % 2000) < 1000) ? (Timing.TotalMilliseconds % 2000) / 1000.0 : (2000 - (Timing.TotalMilliseconds % 2000)) / 1000;
                    //       mandrillFont.Alpha = ((Timing.TotalMilliseconds % 2000) < 1000) ? (Timing.TotalMilliseconds % 2000) / 1700.0 : (2000 - (Timing.TotalMilliseconds % 2000)) / 1700;
                    foreach (Cell cl in displayDamage.Keys)
                    {
                        if (cl.y == row && cl.x == col)
                        {
                            if (displayDamage[cl] < 10)
                            {
                                int offset = (int)(((Timing.TotalMilliseconds % 2000) < 1000) ? (Timing.TotalMilliseconds % 2000) / 50 : (2000 - (Timing.TotalMilliseconds % 2000)) / 50);
                                mandrillFont.Color = Color.DarkRed;
                                mandrillFont.DrawText(pX + 18, pY + 16 - offset, "" + displayDamage[cl]);
                                mandrillFont.Color = Color.White;
                            }
                            else if (displayDamage[cl] >= 10 && displayDamage[cl] < 100)
                            {
                                int offset = (int)(((Timing.TotalMilliseconds % 2000) < 1000) ? (Timing.TotalMilliseconds % 2000) / 40 : (2000 - (Timing.TotalMilliseconds % 2000)) / 40);
                                mandrillFont.Color = Color.DarkRed;
                                mandrillFont.DrawText(pX + 12, pY + 16 - offset, "" + displayDamage[cl]);
                                mandrillFont.Color = Color.White;
                            }
                        }
                    }
                    foreach (Cell cl in displayKills.Keys)
                    {
                        if (cl.y == row && cl.x == col && displayKills[cl])
                        {
                            int offset = (int)(((Timing.TotalMilliseconds % 4000) < 2000) ? (Timing.TotalMilliseconds % 4000) / 100 : (4000 - (Timing.TotalMilliseconds % 4000)) / 100);
                            mandrillFont.Color = Color.DarkRed;
                            mandrillFont.DrawText(pX, pY + 16 - offset, "DEAD");
                            mandrillFont.Color = Color.White;

                        }
                    }

                    pX += tileVIncrease;
                }
                //pY += tileHIncrease;
            }
            ScreenBrowser.Show();
            if (hoverActor != null)
                UnitInfo.ShowMobInfo(hoverActor);
            else if (currentActor != null)
                UnitInfo.ShowMobInfo(currentActor);
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
                Init();

                Keyboard.KeyDown += new InputEventHandler(OnKeyDown);

                Update();
                while (!Display.CurrentWindow.IsClosed)
                {
                    Display.BeginFrame();
                    Show();
                    Display.EndFrame();
                    Update();
                    Core.KeepAlive();
                }
            }
        }

        static void OnKeyDown(InputEventArgs e)
        {
            if (e.KeyCode == KeyCode.Q)
            {
                Display.CurrentWindow.Dispose();
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
            if (lockState && !lockForAnimation && initiative[currentInitiative] == requestingMove && o_entities.ContainsKey(requestingMove) && o_entities[requestingMove].friendly)
            {
                ScreenBrowser.OnKeyDown_Menu(e);
                //                OnKeyDown_SelectMove(e);
            }
        }
        public static void calculateAllMoves(int startX, int startY, int numMoves, bool performEntCheck)
        {
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

            }
        }
        public static void removeMovesUnderMinimum(int startX, int startY, int numMoves, bool performEntCheck)
        {
            if (numMoves == 0)
                return;
            else
            {
                Cell c = new Cell(startX, startY);

                if (map[startY, startX] != DungeonMap.gr)
                    return;
                if (highlightedCells.Count != 0 && performEntCheck && checkPos(startX, startY) != null)
                    return;
                if (highlightedCells.ContainsKey(c))
                    highlightedCells.Remove(c);
                calculateAllMoves(startX - 1, startY, numMoves - 1, performEntCheck);
                calculateAllMoves(startX + 1, startY, numMoves - 1, performEntCheck);
                calculateAllMoves(startX, startY - 1, numMoves - 1, performEntCheck);
                calculateAllMoves(startX, startY + 1, numMoves - 1, performEntCheck);

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
                        if (maxX > Demo.map.GetUpperBound(1))
                            maxX = Demo.map.GetUpperBound(1);
                        int maxY = startY + radius; // 12
                        if (maxY > Demo.map.GetUpperBound(0))
                            maxY = Demo.map.GetUpperBound(0);

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
                                if (map[j, i] == DungeonMap.gr)
                                    highlightedTargetCells.Add(new Cell(i, j), 1);
                            }
                        }
                        break;
                    }
                case SkillAreaKind.Burst:
                    {
                        Dictionary<Cell, int> damages = new Dictionary<Cell, int>();
                        Dictionary<Cell, bool> kills = new Dictionary<Cell, bool>();
                        //assuming targetSquare of 10, 11 and radius 1:
                        int minX = startX - radius; // 9
                        if (minX < 0)
                            minX = 0;
                        int minY = startY - radius; // 10
                        if (minY < 0)
                            minY = 0;
                        int maxX = startX + radius; // 11
                        if (maxX > Demo.map.GetUpperBound(1))
                            maxX = Demo.map.GetUpperBound(1);
                        int maxY = startY + radius; // 12
                        if (maxY > Demo.map.GetUpperBound(0))
                            maxY = Demo.map.GetUpperBound(0);

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
                                if (map[j, i] == DungeonMap.gr)
                                    highlightedTargetCells.Add(new Cell(i, j), 1);
                            }
                        }
                        break;
                    }
                case SkillAreaKind.Spray:
                    {

                        Dictionary<Cell, int> damages = new Dictionary<Cell, int>();
                        Dictionary<Cell, bool> kills = new Dictionary<Cell, bool>();
                        //assuming targetSquare of 10, 11 and radius 2:
                        int minX = startX - radius; // 8
                        if (minX < 0)
                            minX = 0;
                        int minY = startY - radius; // 9
                        if (minY < 0)
                            minY = 0;
                        int maxX = startX + radius; // 12
                        if (maxX > Demo.map.GetUpperBound(1))
                            maxX = Demo.map.GetUpperBound(1);
                        int maxY = startY + radius; // 13
                        if (maxY > Demo.map.GetUpperBound(0))
                            maxY = Demo.map.GetUpperBound(0);

                        if (startY == user.y && startX == user.x)
                        {
                            minY++;
                            maxY++;
                        }
                        for (int i = minX; i <= maxX; i++)
                        {
                            for (int j = minY; j <= maxY; j++)
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
                                if (map[j, i] == DungeonMap.gr)
                                    highlightedTargetCells.Add(new Cell(i, j), 1);
                            }
                        }
                    }

                    break;
                case SkillAreaKind.SingleTarget:
                    {
                        if (sk.hitsAllies == false && checkPos(startX, startY) != null && checkPos(startX, startY).friendly == user.friendly)
                            break;
                        highlightedTargetCells.Add(new Cell(startX, startY), 1);
                        break;
                    }
            }
        }
        public static void HighlightMove()
        {

            if (highlightedCells.Count == 0 && o_entities.ContainsKey(requestingMove))
            {
                int highX = o_entities[requestingMove].x;
                int highY = o_entities[requestingMove].y;
                calculateAllMoves(highX, highY, o_entities[requestingMove].maxMoveDistance, true);
            }
        }
        public static void HighlightSkill()
        {
            o_entities[requestingMove].currentSkill = o_entities[requestingMove].skillList[o_entities[requestingMove].ui.currentScreen.currentMenuItem];
            if (highlightedCells.Count == 0 && o_entities.ContainsKey(requestingMove))
            {
                int highX = o_entities[requestingMove].x;
                int highY = o_entities[requestingMove].y;
                calculateAllMoves(highX, highY, o_entities[requestingMove].currentSkill.maxSkillDistance, false);
                removeMovesUnderMinimum(highX, highY, o_entities[requestingMove].currentSkill.minSkillDistance, false);
                HighlightSkillArea();
            }
        }
        public static void HighlightSkillArea()
        {
            highlightedTargetCells.Clear();
            if (o_entities.ContainsKey(requestingMove))
            {
                o_entities[requestingMove].currentSkill = o_entities[requestingMove].skillList[o_entities[requestingMove].ui.currentScreen.currentMenuItem];
                int highX = o_entities[requestingMove].x;
                int highY = o_entities[requestingMove].y;
                calculateAllTargets(o_entities[requestingMove], cursorX, cursorY, o_entities[requestingMove].currentSkill);
            }
        }
        public static void OnKeyDown_SelectMove(InputEventArgs e)
        {
            if (lockState && !lockForAnimation && initiative[currentInitiative] == requestingMove &&
                             o_entities.ContainsKey(requestingMove) && o_entities[requestingMove].friendly)
            //                 o_entities[requestingMove].moveList.Count <= o_entities[requestingMove].maxMoveDistance)
            {
                if (e.KeyCode == KeyCode.Space)
                {
                    o_entities[requestingMove].moveList.Add(Direction.None);
                }
                else if (e.KeyCode == KeyCode.Left && cursorX > 0 && (map[cursorY, cursorX - 1] == 1194) && checkPos(cursorX - 1, cursorY) == null)
                {
                    if (highlightedCells.ContainsKey(new Cell(cursorX - 1, cursorY)))
                    {
                        cursorX--;
                        o_entities[requestingMove].moveList.Add(Direction.West);
                    }
                }
                else if (e.KeyCode == KeyCode.Right && cursorX < mapWidth && (map[cursorY, cursorX + 1] == 1194) && checkPos(cursorX + 1, cursorY) == null)
                {
                    if (highlightedCells.ContainsKey(new Cell(cursorX + 1, cursorY)))
                    {
                        cursorX++;
                        o_entities[requestingMove].moveList.Add(Direction.East);
                    }
                }
                else if (e.KeyCode == KeyCode.Up && cursorY > 0 && (map[cursorY - 1, cursorX] == 1194) && checkPos(cursorX, cursorY - 1) == null)
                {
                    if (highlightedCells.ContainsKey(new Cell(cursorX, cursorY - 1)))
                    {
                        cursorY--;
                        o_entities[requestingMove].moveList.Add(Direction.North);
                    }
                }
                else if (e.KeyCode == KeyCode.Down && cursorY < mapHeight && (map[cursorY + 1, cursorX] == 1194) && checkPos(cursorX, cursorY + 1) == null)
                {
                    if (highlightedCells.ContainsKey(new Cell(cursorX, cursorY + 1)))
                    {
                        cursorY++;
                        o_entities[requestingMove].moveList.Add(Direction.South);
                    }
                }
                else if (e.KeyCode == KeyCode.Left && cursorX > 0 && (map[cursorY, cursorX - 1] == 1187) && checkPos(cursorX - 1, cursorY) == null && checkFixture(cursorX - 1, cursorY, 1190) != null)
                {
                    map[cursorY, cursorX - 1] = 1194;
                    fixtures[new Cell() { x = cursorX - 1, y = cursorY }].tile = 1188;
                    o_entities[requestingMove].moveList.Add(Direction.None);
                }
                else if (e.KeyCode == KeyCode.Right && cursorX < mapWidth && (map[cursorY, cursorX + 1] == 1187) && checkPos(cursorX + 1, cursorY) == null && checkFixture(cursorX + 1, cursorY, 1190) != null)
                {
                    map[cursorY, cursorX + 1] = 1194;
                    fixtures[new Cell() { x = cursorX + 1, y = cursorY }].tile = 1188;
                    o_entities[requestingMove].moveList.Add(Direction.None);
                }
                else if (e.KeyCode == KeyCode.Up && cursorY > 0 && (map[cursorY - 1, cursorX] == 1187) && checkPos(cursorX, cursorY - 1) == null && checkFixture(cursorX, cursorY - 1, 1191) != null)
                {
                    map[cursorY - 1, cursorX] = 1194;
                    fixtures[new Cell() { x = cursorX, y = cursorY - 1 }].tile = 1189;
                    o_entities[requestingMove].moveList.Add(Direction.None);
                }
                else if (e.KeyCode == KeyCode.Down && cursorY < mapHeight && (map[cursorY + 1, cursorX] == 1187) && checkPos(cursorX, cursorY + 1) == null && checkFixture(cursorX, cursorY + 1, 1191) != null)
                {
                    map[cursorY + 1, cursorX] = 1194;
                    fixtures[new Cell() { x = cursorX, y = cursorY + 1 }].tile = 1189;
                    o_entities[requestingMove].moveList.Add(Direction.None);
                }
                else if (e.KeyCode == ScreenBrowser.backKey)
                {
                    cursorX = o_entities[requestingMove].x;
                    cursorY = o_entities[requestingMove].y;
                    o_entities[requestingMove].moveList.Clear();
                    highlightedCells.Clear();
                    ScreenBrowser.HandleRecall();
                    lockState = true;
                    ScreenBrowser.UnHide();
                }
                else if (e.KeyCode == ScreenBrowser.confirmKey)
                {
                    //                    o_entities[requestingMove].moveList.AddRange(Enumerable.Repeat(Direction.None, o_entities[requestingMove].maxMoveDistance - o_entities[requestingMove].moveList.Count));
                    //}
                    //if (o_entities[requestingMove].moveList.Count == o_entities[requestingMove].maxMoveDistance)
                    // {

                    ScreenBrowser.HandleFinish();
                    lockState = false;
                    highlightedCells.Clear();
                    MoveMob(o_entities[requestingMove], o_entities[requestingMove].moveList);
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
                }
            }
        }

        public static void OnKeyDown_LookAround(InputEventArgs e)
        {
            if (lockState && !lockForAnimation && initiative[currentInitiative] == requestingMove &&
                             o_entities.ContainsKey(requestingMove) && o_entities[requestingMove].friendly)
            //                             o_entities[requestingMove].moveList.Count <= o_entities[requestingMove].maxMoveDistance)
            {
                currentActor = null;
                if (e.KeyCode == KeyCode.Space)
                {
                    //o_entities[requestingMove].moveList.Add(Direction.None);
                }
                else if (e.KeyCode == KeyCode.Left && cursorX > 0 && (map[cursorY, cursorX - 1] == 1194 || map[cursorY, cursorX - 1] == 1187))   // && checkPos(cursorX - 1, cursorY) == null)
                {
                    cursorX--;
                    hoverActor = checkPos(cursorX, cursorY);
                }
                else if (e.KeyCode == KeyCode.Right && cursorX < mapWidth && (map[cursorY, cursorX + 1] == 1194 || map[cursorY, cursorX + 1] == 1187))
                {
                    cursorX++;
                    hoverActor = checkPos(cursorX, cursorY);
                }
                else if (e.KeyCode == KeyCode.Up && cursorY > 0 && (map[cursorY - 1, cursorX] == 1194 || map[cursorY - 1, cursorX] == 1187))
                {

                    cursorY--;
                    hoverActor = checkPos(cursorX, cursorY);
                }
                else if (e.KeyCode == KeyCode.Down && cursorY < mapHeight && (map[cursorY + 1, cursorX] == 1194 || map[cursorY + 1, cursorX] == 1187))
                {
                    cursorY++;
                    hoverActor = checkPos(cursorX, cursorY);
                }
                else if (e.KeyCode == ScreenBrowser.backKey || e.KeyCode == ScreenBrowser.confirmKey)
                {
                    cursorX = o_entities[requestingMove].x;
                    cursorY = o_entities[requestingMove].y;
                    currentActor = o_entities[requestingMove];
                    hoverActor = null;
                    //                    o_entities[requestingMove].moveList.Clear();
                    ScreenBrowser.HandleRecall();
                    lockState = true;
                    ScreenBrowser.UnHide();
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
            o_entities[requestingMove].currentSkill = o_entities[requestingMove].skillList[o_entities[requestingMove].ui.currentScreen.currentMenuItem];
            if (lockState && !lockForAnimation && initiative[currentInitiative] == requestingMove &&
                             o_entities.ContainsKey(requestingMove) && o_entities[requestingMove].friendly)
            // o_entities[requestingMove].moveList.Count <= o_entities[requestingMove].currentSkill.maxSkillDistance)
            {
                /*
                if (e.KeyCode == KeyCode.Space)
                {
                    o_entities[requestingMove].moveList.Add(Direction.None);
                }*/

                if (e.KeyCode == ScreenBrowser.backKey)
                {
                    cursorX = o_entities[requestingMove].x;
                    cursorY = o_entities[requestingMove].y;
                    o_entities[requestingMove].moveList.Clear();
                    highlightedCells.Clear();
                    highlightedTargetCells.Clear();
                    ScreenBrowser.HandleRecall();
                    lockState = true;
                    ScreenBrowser.UnHide();
                }
                else if (e.KeyCode == KeyCode.Left && cursorX > 0 && (map[cursorY, cursorX - 1] == 1194))
                {
                    if (highlightedCells.ContainsKey(new Cell(cursorX - 1, cursorY)))
                    {
                        cursorX--;
                        o_entities[requestingMove].moveList.Add(Direction.West);
                        HighlightSkillArea();
                    }
                    hoverActor = checkPos(cursorX, cursorY);
                }
                else if (e.KeyCode == KeyCode.Right && cursorX < mapWidth && (map[cursorY, cursorX + 1] == 1194))
                {
                    if (highlightedCells.ContainsKey(new Cell(cursorX + 1, cursorY)))
                    {
                        cursorX++;
                        o_entities[requestingMove].moveList.Add(Direction.East);
                        HighlightSkillArea();
                    }
                    hoverActor = checkPos(cursorX, cursorY);
                }
                else if (e.KeyCode == KeyCode.Up && cursorY > 0 && (map[cursorY - 1, cursorX] == 1194))
                {
                    if (highlightedCells.ContainsKey(new Cell(cursorX, cursorY - 1)))
                    {
                        cursorY--;
                        o_entities[requestingMove].moveList.Add(Direction.North);
                        HighlightSkillArea();
                    }
                    hoverActor = checkPos(cursorX, cursorY);
                }
                else if (e.KeyCode == KeyCode.Down && cursorY < mapHeight && (map[cursorY + 1, cursorX] == 1194))
                {
                    if (highlightedCells.ContainsKey(new Cell(cursorX, cursorY + 1)))
                    {
                        cursorY++;
                        o_entities[requestingMove].moveList.Add(Direction.South);
                        HighlightSkillArea();
                    }
                    hoverActor = checkPos(cursorX, cursorY);
                }



                /*

                if (e.KeyCode == KeyCode.Left && cursorX > 0 && (map[cursorY, cursorX - 1] == 1194))// || map[cursorY, cursorX - 1] == 1187))// && checkPos(cursorX - 1, cursorY) == null)
                {
                    cursorX--;
                    if (o_entities[requestingMove].moveList.Count > 0 && o_entities[requestingMove].moveList[o_entities[requestingMove].moveList.Count - 1] == Direction.East)
                        o_entities[requestingMove].moveList.RemoveAt(o_entities[requestingMove].moveList.Count - 1);
                    else
                        o_entities[requestingMove].moveList.Add(Direction.West);
                }
                else if (e.KeyCode == KeyCode.Right && cursorX < mapWidth && (map[cursorY, cursorX + 1] == 1194))// || map[cursorY, cursorX + 1] == 1187))// && checkPos(cursorX + 1, cursorY) == null)
                {
                    cursorX++;
                    if (o_entities[requestingMove].moveList.Count > 0 && o_entities[requestingMove].moveList[o_entities[requestingMove].moveList.Count - 1] == Direction.West)
                        o_entities[requestingMove].moveList.RemoveAt(o_entities[requestingMove].moveList.Count - 1);
                    else
                        o_entities[requestingMove].moveList.Add(Direction.East);
                }
                else if (e.KeyCode == KeyCode.Up && cursorY > 0 && (map[cursorY - 1, cursorX] == 1194))// || map[cursorY - 1, cursorX] == 1187))// && checkPos(cursorX, cursorY - 1) == null)
                {
                    cursorY--;
                    if (o_entities[requestingMove].moveList.Count > 0 && o_entities[requestingMove].moveList[o_entities[requestingMove].moveList.Count - 1] == Direction.South)
                        o_entities[requestingMove].moveList.RemoveAt(o_entities[requestingMove].moveList.Count - 1);
                    else
                        o_entities[requestingMove].moveList.Add(Direction.North);
                }
                else if (e.KeyCode == KeyCode.Down && cursorY < mapHeight && (map[cursorY + 1, cursorX] == 1194)) // || map[cursorY + 1, cursorX] == 1187// && checkPos(cursorX, cursorY + 1) == null)
                {
                    cursorY++;
                    if (o_entities[requestingMove].moveList.Count > 0 && o_entities[requestingMove].moveList[o_entities[requestingMove].moveList.Count - 1] == Direction.North)
                        o_entities[requestingMove].moveList.RemoveAt(o_entities[requestingMove].moveList.Count - 1);
                    else
                        o_entities[requestingMove].moveList.Add(Direction.South);
                }*/
                else if (e.KeyCode == ScreenBrowser.confirmKey && o_entities[requestingMove].moveList.Count >= o_entities[requestingMove].currentSkill.minSkillDistance)
                {

                    hoverActor = null;
                    o_entities[requestingMove].currentSkill.targetSquare = new Cell() { x = cursorX, y = cursorY };

                    //                    o_entities[requestingMove].hasActed = true;

                    SkillResult sa = o_entities[requestingMove].currentSkill.ApplySkill(o_entities[requestingMove]);
                    highlightedCells.Clear();
                    highlightedTargetCells.Clear();
                    AnimateResults(sa);
                    ScreenBrowser.HandleFinish();
                    if (o_entities.ContainsKey(requestingMove) == false)
                    {
                        Keyboard.KeyDown -= OnKeyDown_ActionMenu;
                        currentInitiative--;
                        lockState = false;
                        return;
                    }
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


                    // requestingMove.x = -1;
                    o_entities[requestingMove].moveList.Clear();
                    //Update();
                    cursorX = o_entities[requestingMove].x;
                    cursorY = o_entities[requestingMove].y;
                    currentActor = o_entities[requestingMove];
                    lockState = false;

                }
            }

        }
        public static void waitAction()
        {
            lockState = false;
            o_entities[requestingMove].actionCount = 2;
            Keyboard.KeyDown -= OnKeyDown_ActionMenu;
            currentInitiative--;
            o_entities[requestingMove].moveList.Clear();

            ScreenBrowser.HandleFinish();

            cursorX = o_entities[requestingMove].x;
            cursorY = o_entities[requestingMove].y;
        }



    }
}
