using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AgateDemo
{
    public class Skill
    {
        public int maxSkillDistance = 1, minSkillDistance = 1;
        public int damage;
        public string name;
        public Demo.Cell targetSquare;
        public Skill(string name, int damage, int dist)
        {
            this.name = name;
            this.damage = damage;
            maxSkillDistance = dist;
            targetSquare.x = -1;
            targetSquare.y = -1;
        }
        public void ApplySkill(Demo.Mob user)
        {
            Demo.Mob tgt = Demo.checkPos(targetSquare.x, targetSquare.y);
            if (tgt == null)
                return;
            tgt.health = tgt.health - this.damage;
        }
    }
}
