using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        public Cell targetSquare;
        public Skill(string name, int damage, int dist)
        {
            this.name = name;
            this.damage = damage;
            maxSkillDistance = dist;
            hitsAllies = false;
            targetSquare.x = -1;
            targetSquare.y = -1;
        }
        public Skill(string name, int damage, int minDist, int maxDist, SkillAreaKind areaKind, int area, bool hitsFriendlies)
        {
            this.name = name;
            this.damage = damage;
            minSkillDistance = minDist;
            maxSkillDistance = maxDist;
            targetSquare.x = -1;
            targetSquare.y = -1;
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
                        Dictionary<Cell, int> damages = new Dictionary<Cell, int>();
                        Dictionary<Cell, bool> kills = new Dictionary<Cell, bool>();
                        //assuming targetSquare of 10, 11 and radius 1:
                        int minX = targetSquare.x - radius; // 9
                        if (minX < 0)
                            minX = 0;
                        int minY = targetSquare.y - radius; // 10
                        if (minY < 0)
                            minY = 0;
                        int maxX = targetSquare.x + radius; // 11
                        if (maxX > Demo.map.GetUpperBound(1))
                            maxX = Demo.map.GetUpperBound(1);
                        int maxY = targetSquare.y + radius; // 12
                        if (maxY > Demo.map.GetUpperBound(0))
                            maxY = Demo.map.GetUpperBound(0);

                        for (int i = minX; i <= maxX; i++)
                        {
                            for (int j = minY; j <= maxY; j++)
                            {
                                if (j - targetSquare.y > i - targetSquare.x + radius ||        // 10 - 10 > 10 - 11 + 1
                                    j - targetSquare.y > -1 * (i - targetSquare.x) + radius || // 10 - 10 > -10 + 11 + 1
                                    j - targetSquare.y < i - targetSquare.x - radius ||        // 10 - 10 < 10 - 11 - 1
                                    j - targetSquare.y < -1 * (i - targetSquare.x) - radius || // 10 - 10 < -10 + 11 - 1
                                    (i == targetSquare.x && j == targetSquare.y))
                                    continue;

                                Demo.Mob tgt = Demo.checkPos(i, j);
                                if (tgt != null)
                                {
                                    if (hitsAllies == false && tgt.friendly == user.friendly)
                                        continue;
                                    Cell o_tgt = new Cell(tgt.o_pos.x, tgt.o_pos.y);
                                    Cell tgt_pos = new Cell(tgt.pos.x, tgt.pos.y);
                                    tgt.health = tgt.health - this.damage;
                                    bool didKill = false;
                                    if (!Demo.o_entities.ContainsKey(o_tgt))
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
                        Dictionary<Cell, int> damages = new Dictionary<Cell, int>();
                        Dictionary<Cell, bool> kills = new Dictionary<Cell, bool>();
                        //assuming targetSquare of 10, 11 and radius 1:
                        int minX = targetSquare.x - radius; // 9
                        if (minX < 0)
                            minX = 0;
                        int minY = targetSquare.y - radius; // 10
                        if (minY < 0)
                            minY = 0;
                        int maxX = targetSquare.x + radius; // 11
                        if (maxX > Demo.map.GetUpperBound(1))
                            maxX = Demo.map.GetUpperBound(1);
                        int maxY = targetSquare.y + radius; // 12
                        if (maxY > Demo.map.GetUpperBound(0))
                            maxY = Demo.map.GetUpperBound(0);

                        for (int i = minX; i <= maxX; i++)
                        {
                            for (int j = minY; j <= maxY; j++)
                            {
                                if (j - targetSquare.y > i - targetSquare.x + radius ||        // 10 - 10 > 10 - 11 + 1
                                    j - targetSquare.y > -1 * (i - targetSquare.x) + radius || // 10 - 10 > -10 + 11 + 1
                                    j - targetSquare.y < i - targetSquare.x - radius ||        // 10 - 10 < 10 - 11 - 1
                                    j - targetSquare.y < -1 * (i - targetSquare.x) - radius)   // 10 - 10 < -10 + 11 - 1
                                    continue;

                                Demo.Mob tgt = Demo.checkPos(i, j);
                                if (tgt != null)
                                {
                                    if (hitsAllies == false && tgt.friendly == user.friendly)
                                        continue;
                                    Cell o_tgt = new Cell(tgt.o_pos.x, tgt.o_pos.y);
                                    Cell tgt_pos = new Cell(tgt.pos.x, tgt.pos.y);
                                    tgt.health = tgt.health - this.damage;
                                    bool didKill = false;
                                    if (!Demo.o_entities.ContainsKey(o_tgt))
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

                        Dictionary<Cell, int> damages = new Dictionary<Cell, int>();
                        Dictionary<Cell, bool> kills = new Dictionary<Cell, bool>();
                        //assuming targetSquare of 10, 11 and radius 2:
                        int minX = targetSquare.x - radius; // 8
                        if (minX < 0)
                            minX = 0;
                        int minY = targetSquare.y - radius; // 9
                        if (minY < 0)
                            minY = 0;
                        int maxX = targetSquare.x + radius; // 12
                        if (maxX > Demo.map.GetUpperBound(1))
                            maxX = Demo.map.GetUpperBound(1);
                        int maxY = targetSquare.y + radius; // 13
                        if (maxY > Demo.map.GetUpperBound(0))
                            maxY = Demo.map.GetUpperBound(0);
                        if (targetSquare.y == user.y && targetSquare.x == user.x)
                        {
                            minY++;
                            maxY++;
                        }
                        for (int i = minX; i <= maxX; i++)
                        {
                            for (int j = minY; j <= maxY; j++)
                            {
                                if (targetSquare.y == user.y && targetSquare.x == user.x)
                                {
                                    if (j - targetSquare.y > radius + 1 ||
                                        j - targetSquare.y < i - targetSquare.x + 1 ||
                                        j - targetSquare.y < -1 * (i - targetSquare.x) + 1 ||
                                        (i == targetSquare.x && j == targetSquare.y))
                                        continue;
                                }
                                if (targetSquare.y - user.y >= user.x - targetSquare.x && targetSquare.y - user.y >= targetSquare.x - user.x)
                                {
                                    if (j - targetSquare.y > radius ||
                                        j - targetSquare.y < i - targetSquare.x ||
                                        j - targetSquare.y < -1 * (i - targetSquare.x)) //|| //+ 1 
//                                        (i == targetSquare.x && j == targetSquare.y))
                                        continue;
                                }
                                else if (user.y - targetSquare.y >= user.x - targetSquare.x && user.y - targetSquare.y >= targetSquare.x - user.x)
                                {
                                    if (j - targetSquare.y < -1 * radius ||
                                        j - targetSquare.y > i - targetSquare.x ||
                                        j - targetSquare.y > -1 * (i - targetSquare.x)) //|| //- 1 
                  //                      (i == targetSquare.x && j == targetSquare.y))
                                        continue;
                                }
                                else if (targetSquare.x - user.x > user.y - targetSquare.y && targetSquare.x - user.x > targetSquare.y - user.y)
                                {
                                    if (i - targetSquare.x > radius ||
                                        i - targetSquare.x < j - targetSquare.y ||
                                        i - targetSquare.x < -1 * (j - targetSquare.y)) //|| //+ 1 
  //                                      (i == targetSquare.x && j == targetSquare.y))
                                        continue;
                                }
                                else if (user.x - targetSquare.x > user.y - targetSquare.y && user.x - targetSquare.x > targetSquare.y - user.y)
                                {
                                    if (i - targetSquare.x < -1 * radius ||
                                        i - targetSquare.x > j - targetSquare.y ||
                                        i - targetSquare.x > -1 * (j - targetSquare.y)) //- 1 
//                                        (i == targetSquare.x && j == targetSquare.y))
                                        continue;
                                }
                                Demo.Mob tgt = Demo.checkPos(i, j);
                                if (tgt != null)
                                {
                                    if (hitsAllies == false && tgt.friendly == user.friendly)
                                        continue;
                                    Cell o_tgt = new Cell(tgt.o_pos.x, tgt.o_pos.y);
                                    Cell tgt_pos = new Cell(tgt.pos.x, tgt.pos.y);
                                    tgt.health = tgt.health - this.damage;
                                    bool didKill = false;
                                    if (!Demo.o_entities.ContainsKey(o_tgt))
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
                        Demo.Mob tgt = Demo.checkPos(targetSquare.x, targetSquare.y);
                        if (tgt == null)
                            return new SkillResult();
                        if (hitsAllies == false && tgt.friendly == user.friendly)
                            return new SkillResult();
                        Cell o_tgt = new Cell(tgt.o_pos.x, tgt.o_pos.y);
                        Cell tgt_pos = new Cell(tgt.pos.x, tgt.pos.y);
                        tgt.health = tgt.health - this.damage;
                        bool didKill = false;
                        if (!Demo.o_entities.ContainsKey(o_tgt))
                            didKill = true;
                        return new SkillResult(new Dictionary<Cell, int>() { { tgt_pos, this.damage } }, new Dictionary<Cell, bool>() { { tgt_pos, didKill } });
                    }
            }
        }
    }

    public class SkillResult
    {
        public Dictionary<Cell, int> damages = new Dictionary<Cell, int>();
        public Dictionary<Cell, bool> kills = new Dictionary<Cell, bool>();
        public SkillResult()
        {
        }
        public SkillResult(Dictionary<Cell, int> cellsToDamage, Dictionary<Cell, bool> cellsKilled)
        {
            damages = cellsToDamage;
            kills = cellsKilled;
        }
    }
}
