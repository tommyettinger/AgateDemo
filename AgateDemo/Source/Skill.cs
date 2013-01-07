using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AgateLib.Geometry;

namespace AgateDemo
{
    public enum SkillAreaKind { SingleTarget, Ring, Burst, Spray };
    public class Skill
    {
        public int maxSkillDistance = 1;
        private int minRange = 1;
        public int minSkillDistance
        {
        get {return minRange;}
            set { minRange = value; if (minRange == 0) hitsAllies = true; }
    }
        public int radius = 0;
        public SkillAreaKind areaKind = SkillAreaKind.SingleTarget;
        public int damage;
        public bool hitsAllies;
        public string name;
        public Point targetSquare;
        public Skill(string name, int damage, int dist)
        {
            this.name = name;
            this.damage = damage;
            maxSkillDistance = dist;
            hitsAllies = false;
            targetSquare.X = -1;
            targetSquare.Y = -1;
        }
        public Skill(string name, int damage, int minDist, int maxDist, SkillAreaKind areaKind, int area, bool hitsFriendlies)
        {
            this.name = name;
            this.damage = damage;
            minSkillDistance = minDist;
            maxSkillDistance = maxDist;
            targetSquare.X = -1;
            targetSquare.Y = -1;
            this.areaKind = areaKind;
            radius = area;
            hitsAllies = hitsFriendlies;
        }
        public SkillResult ApplySkill(Demo.Mob user)
        {
            switch (areaKind)
            {
                case SkillAreaKind.Ring:
                    {
                        Dictionary<Point, int> damages = new Dictionary<Point, int>();
                        Dictionary<Point, bool> kills = new Dictionary<Point, bool>();
                        //assuming targetSquare of 10, 11 and radius 1:
                        int minX = targetSquare.X - radius; // 9
                        if (minX < 0)
                            minX = 0;
                        int minY = targetSquare.Y - radius; // 10
                        if (minY < 0)
                            minY = 0;
                        int maxX = targetSquare.X + radius; // 11
                        if (maxX > Demo.currentLevel.map.GetUpperBound(1))
                            maxX = Demo.currentLevel.map.GetUpperBound(1);
                        int maxY = targetSquare.Y + radius; // 12
                        if (maxY > Demo.currentLevel.map.GetUpperBound(0))
                            maxY = Demo.currentLevel.map.GetUpperBound(0);

                        for (int i = minX; i <= maxX; i++)
                        {
                            for (int j = minY; j <= maxY; j++)
                            {
                                if (j - targetSquare.Y > i - targetSquare.X + radius ||        // 10 - 10 > 10 - 11 + 1
                                    j - targetSquare.Y > -1 * (i - targetSquare.X) + radius || // 10 - 10 > -10 + 11 + 1
                                    j - targetSquare.Y < i - targetSquare.X - radius ||        // 10 - 10 < 10 - 11 - 1
                                    j - targetSquare.Y < -1 * (i - targetSquare.X) - radius || // 10 - 10 < -10 + 11 - 1
                                    (i == targetSquare.X && j == targetSquare.Y))
                                    continue;

                                Demo.Mob tgt = Demo.checkPos(i, j);
                                if (tgt != null && user.fov.sight[j,i] > 0)
                                {
                                    if (hitsAllies == false && tgt.friendly == user.friendly)
                                        continue;
                                    
                                    Point o_tgt = new Point(tgt.o_pos.X, tgt.o_pos.Y);
                                    Point tgt_pos = new Point(tgt.pos.X, tgt.pos.Y);
                                    tgt.health = tgt.health - this.damage;
                                    bool didKill = false;
                                    if (!Demo.currentLevel.o_entities.ContainsKey(o_tgt))
                                        didKill = true;
                                    damages.Add(tgt_pos, this.damage);
                                    kills.Add(tgt_pos, didKill);
                                }
                            }
                        }
                        return new SkillResult(damages, kills);
                    }
                /*{
                        Dictionary<Cell, int> damages = new Dictionary<Cell, int>();
                        Dictionary<Cell, bool> kills = new Dictionary<Cell, bool>();
                        Demo.Mob tgt = Demo.checkPos(targetSquare.x - radius, targetSquare.y);
                        if (tgt != null)
                        {
                            Cell o_tgt = new Cell(tgt.o_pos.x, tgt.o_pos.y);
                            Cell tgt_pos = new Cell(tgt.pos.x, tgt.pos.y);
                            tgt.health = tgt.health - this.damage;
                            bool didKill = false;
                            if (!Demo.o_entities.ContainsKey(o_tgt))
                                didKill = true;
                            damages.Add(tgt_pos, this.damage);
                            kills.Add(tgt_pos, didKill);
                        }
                        tgt = Demo.checkPos(targetSquare.x + radius, targetSquare.y);
                        if (tgt != null)
                        {
                            Cell o_tgt = new Cell(tgt.o_pos.x, tgt.o_pos.y);
                            Cell tgt_pos = new Cell(tgt.pos.x, tgt.pos.y);
                            tgt.health = tgt.health - this.damage;
                            bool didKill = false;
                            if (!Demo.o_entities.ContainsKey(o_tgt))
                                didKill = true;
                            damages.Add(tgt_pos, this.damage);
                            kills.Add(tgt_pos, didKill);
                        }
                        tgt = Demo.checkPos(targetSquare.x, targetSquare.y - radius);
                        if (tgt != null)
                        {
                            Cell o_tgt = new Cell(tgt.o_pos.x, tgt.o_pos.y);
                            Cell tgt_pos = new Cell(tgt.pos.x, tgt.pos.y);
                            tgt.health = tgt.health - this.damage;
                            bool didKill = false;
                            if (!Demo.o_entities.ContainsKey(o_tgt))
                                didKill = true;
                            damages.Add(tgt_pos, this.damage);
                            kills.Add(tgt_pos, didKill);
                        }
                        tgt = Demo.checkPos(targetSquare.x, targetSquare.y + radius);
                        if (tgt != null)
                        {
                            Cell o_tgt = new Cell(tgt.o_pos.x, tgt.o_pos.y);
                            Cell tgt_pos = new Cell(tgt.pos.x, tgt.pos.y);
                            tgt.health = tgt.health - this.damage;
                            bool didKill = false;
                            if (!Demo.o_entities.ContainsKey(o_tgt))
                                didKill = true;
                            damages.Add(tgt_pos, this.damage);
                            kills.Add(tgt_pos, didKill);
                        }
                        return new SkillResult(damages, kills);
                    }*/
                case SkillAreaKind.Burst:
                    {
                        Dictionary<Point, int> damages = new Dictionary<Point, int>();
                        Dictionary<Point, bool> kills = new Dictionary<Point, bool>();
                        //assuming targetSquare of 10, 11 and radius 1:
                        int minX = targetSquare.X - radius; // 9
                        if (minX < 0)
                            minX = 0;
                        int minY = targetSquare.Y - radius; // 10
                        if (minY < 0)
                            minY = 0;
                        int maxX = targetSquare.X + radius; // 11
                        if (maxX > Demo.currentLevel.map.GetUpperBound(1))
                            maxX = Demo.currentLevel.map.GetUpperBound(1);
                        int maxY = targetSquare.Y + radius; // 12
                        if (maxY > Demo.currentLevel.map.GetUpperBound(0))
                            maxY = Demo.currentLevel.map.GetUpperBound(0);

                        for (int i = minX; i <= maxX; i++)
                        {
                            for (int j = minY; j <= maxY; j++)
                            {
                                if (j - targetSquare.Y > i - targetSquare.X + radius ||        // 10 - 10 > 10 - 11 + 1
                                    j - targetSquare.Y > -1 * (i - targetSquare.X) + radius || // 10 - 10 > -10 + 11 + 1
                                    j - targetSquare.Y < i - targetSquare.X - radius ||        // 10 - 10 < 10 - 11 - 1
                                    j - targetSquare.Y < -1 * (i - targetSquare.X) - radius)   // 10 - 10 < -10 + 11 - 1
                                    continue;

                                Demo.Mob tgt = Demo.checkPos(i, j);
                                if (tgt != null && user.fov.sight[j, i] > 0)
                                {
                                    if (hitsAllies == false && tgt.friendly == user.friendly)
                                        continue;
                                    Point o_tgt = new Point(tgt.o_pos.X, tgt.o_pos.Y);
                                    Point tgt_pos = new Point(tgt.pos.X, tgt.pos.Y);
                                    tgt.health = tgt.health - this.damage;
                                    bool didKill = false;
                                    if (!Demo.currentLevel.o_entities.ContainsKey(o_tgt))
                                        didKill = true;
                                    damages.Add(tgt_pos, this.damage);
                                    kills.Add(tgt_pos, didKill);
                                }
                            }
                        }
                        return new SkillResult(damages, kills);
                    }
                case SkillAreaKind.Spray:
                    {

                        Dictionary<Point, int> damages = new Dictionary<Point, int>();
                        Dictionary<Point, bool> kills = new Dictionary<Point, bool>();
                        //assuming targetSquare of 10, 11 and radius 2:
                        int minX = targetSquare.X - (radius - 1); // 8
                        if (minX < 0)
                            minX = 0;
                        int minY = targetSquare.Y - (radius - 1); // 9
                        if (minY < 0)
                            minY = 0;
                        int maxX = targetSquare.X + (radius - 1); // 12
                        if (maxX > Demo.currentLevel.map.GetUpperBound(1))
                            maxX = Demo.currentLevel.map.GetUpperBound(1);
                        int maxY = targetSquare.Y + (radius - 1); // 13
                        if (maxY > Demo.currentLevel.map.GetUpperBound(0))
                            maxY = Demo.currentLevel.map.GetUpperBound(0);
                        if (targetSquare.Y == user.y && targetSquare.X == user.x)
                        {
                            minY++;
                            maxY++;
                        }
                        for (int i = minX; i <= maxX; i++)
                        {
                            for (int j = minY; j <= maxY; j++)
                            {
                                if (targetSquare.Y == user.y && targetSquare.X == user.x)
                                {
                                    if (j - targetSquare.Y > radius + 1 ||
                                        j - targetSquare.Y < i - targetSquare.X + 1 ||
                                        j - targetSquare.Y < -1 * (i - targetSquare.X) + 1 ||
                                        (i == targetSquare.X && j == targetSquare.Y))
                                        continue;
                                }
                                else if (targetSquare.Y - user.y >= user.x - targetSquare.X && targetSquare.Y - user.y >= targetSquare.X - user.x)
                                {
                                    if (j - targetSquare.Y > radius ||
                                        j - targetSquare.Y < i - targetSquare.X ||
                                        j - targetSquare.Y < -1 * (i - targetSquare.X)) //|| //+ 1 
//                                        (i == targetSquare.x && j == targetSquare.y))
                                        continue;
                                }
                                else if (user.y - targetSquare.Y >= user.x - targetSquare.X && user.y - targetSquare.Y >= targetSquare.X - user.x)
                                {
                                    if (j - targetSquare.Y < -1 * radius ||
                                        j - targetSquare.Y > i - targetSquare.X ||
                                        j - targetSquare.Y > -1 * (i - targetSquare.X)) //|| //- 1 
                  //                      (i == targetSquare.x && j == targetSquare.y))
                                        continue;
                                }
                                else if (targetSquare.X - user.x > user.y - targetSquare.Y && targetSquare.X - user.x > targetSquare.Y - user.y)
                                {
                                    if (i - targetSquare.X > radius ||
                                        i - targetSquare.X < j - targetSquare.Y ||
                                        i - targetSquare.X < -1 * (j - targetSquare.Y)) //|| //+ 1 
  //                                      (i == targetSquare.x && j == targetSquare.y))
                                        continue;
                                }
                                else if (user.x - targetSquare.X > user.y - targetSquare.Y && user.x - targetSquare.X > targetSquare.Y - user.y)
                                {
                                    if (i - targetSquare.X < -1 * radius ||
                                        i - targetSquare.X > j - targetSquare.Y ||
                                        i - targetSquare.X > -1 * (j - targetSquare.Y)) //- 1 
//                                        (i == targetSquare.x && j == targetSquare.y))
                                        continue;
                                }
                                Demo.Mob tgt = Demo.checkPos(i, j);
                                if (tgt != null && user.fov.sight[j, i] > 0)
                                {
                                    if (hitsAllies == false && tgt.friendly == user.friendly)
                                        continue;
                                    Point o_tgt = new Point(tgt.o_pos.X, tgt.o_pos.Y);
                                    Point tgt_pos = new Point(tgt.pos.X, tgt.pos.Y);
                                    tgt.health = tgt.health - this.damage;
                                    bool didKill = false;
                                    if (!Demo.currentLevel.o_entities.ContainsKey(o_tgt))
                                        didKill = true;
                                    damages.Add(tgt_pos, this.damage);
                                    kills.Add(tgt_pos, didKill);
                                }
                            }
                        }
                        return new SkillResult(damages, kills);
                    }

                case SkillAreaKind.SingleTarget:
                default:
                    {
                        Demo.Mob tgt = Demo.checkPos(targetSquare.X, targetSquare.Y);
                        if (tgt == null)
                            return new SkillResult();
                        if (hitsAllies == false && tgt.friendly == user.friendly)
                            return new SkillResult();
                        if (user.fov.sight[tgt.y, tgt.x] <= 0)
                            return new SkillResult();
                        Point o_tgt = new Point(tgt.o_pos.X, tgt.o_pos.Y);
                        Point tgt_pos = new Point(tgt.pos.X, tgt.pos.Y);
                        tgt.health = tgt.health - this.damage;
                        bool didKill = false;
                        if (!Demo.currentLevel.o_entities.ContainsKey(o_tgt))
                            didKill = true;
                        return new SkillResult(new Dictionary<Point, int>() { { tgt_pos, this.damage } }, new Dictionary<Point, bool>() { { tgt_pos, didKill } });
                    }
            }
        }
    }

    public class SkillResult
    {
        public Dictionary<Point, int> damages = new Dictionary<Point, int>();
        public Dictionary<Point, bool> kills = new Dictionary<Point, bool>();
        public SkillResult()
        {
        }
        public SkillResult(Dictionary<Point, int> cellsToDamage, Dictionary<Point, bool> cellsKilled)
        {
            damages = cellsToDamage;
            kills = cellsKilled;
        }
    }
}
