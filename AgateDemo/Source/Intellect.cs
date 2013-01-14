using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AgateLib.Geometry;
namespace AgateDemo
{
    public class Intellect
    {
        public Demo.Mob thinker;
        public enum MentalState { Hunting, Fleeing, Guarding, };
        public Dictionary<Point, int> paths = new Dictionary<Point, int>();
        public double[,] goals;
        public MentalState activity;
        public int maxSkillRange = 1;
        List<Point> currentBestList = new List<Point>();
        double currentBest = 0.0;
        public Intellect(Demo.Mob thinker)
        {
            this.thinker = thinker;
            this.activity = MentalState.Hunting;
            goals = new double[thinker.dlevel.map.GetLength(0), thinker.dlevel.map.GetLength(0)];
        }
        public List<Direction> Evaluate()
        {
            Dictionary<Point, int> validMoves = new Dictionary<Point, int>();
            Dictionary<Point, bool> invalidMoves = new Dictionary<Point, bool>();
            Demo.calculateAllMoves(thinker.dlevel, thinker.x, thinker.y, thinker.fov.visualRange, false, validMoves, invalidMoves, thinker.friendly);
            /*switch (activity)
            {
                case MentalState.Hunting:
                    {*/
            Point pt = new Point(0, 0);
            int pcCount = 0, npcCount = 1;
            currentBestList.Clear();
            currentBest = 0.0;
            for (int i = 0; i < goals.GetLength(0); i++, pt.Y++)
            {
                pt.X = 0;
                for (int j = 0; j < goals.GetLength(1); j++, pt.X++)
                {
                    if (validMoves.ContainsKey(pt))
                    {
                        goals[i, j] = (thinker.fov.visualRange - validMoves[pt]) / thinker.fov.visualRange; //prefer further away for movement
                        if (thinker.dlevel.entities.ContainsKey(pt))
                        {
                            if (thinker.dlevel.entities[pt].friendly)
                            {
                                goals[i, j] = 5.0 + ((validMoves[pt] + 1) / thinker.fov.visualRange); //strongly favor closer PCs
                                pcCount++;
                            }
                            else
                            {
                                npcCount++;
                            }
                        }
                        if (goals[i, j] == currentBest)
                        {
                            currentBestList.Add(pt);
                        }
                        else if (goals[i, j] > currentBest)
                        {
                            currentBestList.Clear();
                            currentBest = goals[i, j];
                            currentBestList.Add(pt);
                        }
                    }
                }
            }
            validMoves.Clear();
            invalidMoves.Clear();
            Demo.calculateAllMoves(thinker.dlevel, thinker.x, thinker.y, thinker.moveSpeed, thinker.fov.visualRange, true, validMoves, invalidMoves, currentBestList, thinker.friendly); //20
            /*foreach (Point nogo in thinker.dlevel.enemyBlockedCells.Except(invalidMoves.Keys))
            {
                invalidMoves.Add(nogo, true);
            }*/
            int randomPosition = Demo.rnd.Next(currentBestList.Count);
            Point targetSquare = currentBestList [randomPosition], currentSquare = targetSquare;
            int currentDist = thinker.moveSpeed;
            if(validMoves.ContainsKey(targetSquare))
            {
                currentDist = validMoves[targetSquare];
            }

            List<Direction> moveList = new List<Direction>();
            Direction[] randir = { Direction.East, Direction.North, Direction.West, Direction.South };
            Demo.Shuffle(randir);
            int ivCount = 0;
            List<bool> isDisplacedList = new List<bool>();
            while (currentDist < thinker.moveSpeed && ivCount < 4 * thinker.fov.visualRange)
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
                                    if (invalidMoves.ContainsKey(currentSquare))
                                    {
                                        isDisplacedList.Insert(0, true);
                                    }
                                    else
                                    {
                                        isDisplacedList.Insert(0, false);
                                    }
                                    currentDist = validMoves[currentSquare];
                                    moveList.Insert(0, Direction.West);
                                }
                                else if (invalidMoves.ContainsKey(new Point(currentSquare.X + 1, currentSquare.Y)))
                                {
                                    ivCount++;
                                }
                                break;
                            }
                        case Direction.West:
                            {
                                if (validMoves.ContainsKey(new Point(currentSquare.X - 1, currentSquare.Y)) && validMoves[new Point(currentSquare.X - 1, currentSquare.Y)] > currentDist)
                                {
                                    currentSquare = new Point(currentSquare.X - 1, currentSquare.Y);
                                    if (invalidMoves.ContainsKey(currentSquare))
                                    {
                                        isDisplacedList.Insert(0, true);
                                    }
                                    else
                                    {
                                        isDisplacedList.Insert(0, false);
                                    }
                                    currentDist = validMoves[currentSquare];
                                    moveList.Insert(0, Direction.East);
                                }
                                else if (invalidMoves.ContainsKey(new Point(currentSquare.X - 1, currentSquare.Y)))
                                {
                                    ivCount++;
                                }
                                break;
                            }
                        case Direction.North:
                            {
                                if (validMoves.ContainsKey(new Point(currentSquare.X, currentSquare.Y - 1)) && validMoves[new Point(currentSquare.X, currentSquare.Y - 1)] > currentDist)
                                {
                                    currentSquare = new Point(currentSquare.X, currentSquare.Y - 1);
                                    if (invalidMoves.ContainsKey(currentSquare))
                                    {
                                        isDisplacedList.Insert(0, true);
                                    }
                                    else
                                    {
                                        isDisplacedList.Insert(0, false);
                                    }
                                    currentDist = validMoves[currentSquare];
                                    moveList.Insert(0, Direction.South);
                                }
                                else if (invalidMoves.ContainsKey(new Point(currentSquare.X, currentSquare.Y - 1)))
                                {
                                    ivCount++;
                                }
                                break;
                            }
                        case Direction.South:
                            {
                                if (validMoves.ContainsKey(new Point(currentSquare.X, currentSquare.Y + 1)) && validMoves[new Point(currentSquare.X, currentSquare.Y + 1)] > currentDist)
                                {
                                    currentSquare = new Point(currentSquare.X, currentSquare.Y + 1);
                                    if (invalidMoves.ContainsKey(currentSquare))
                                    {
                                        isDisplacedList.Insert(0, true);
                                    }
                                    else
                                    {
                                        isDisplacedList.Insert(0, false);
                                    }
                                    currentDist = validMoves[currentSquare];
                                    moveList.Insert(0, Direction.North);
                                }
                                else if (invalidMoves.ContainsKey(new Point(currentSquare.X, currentSquare.Y + 1)))
                                {
                                    ivCount++;
                                }
                                break;
                            }
                    }
                }
            }
            paths = validMoves;
       //     thinker.moveList.Clear();
            int howMany = (moveList.Count > thinker.moveSpeed) ? thinker.moveSpeed : moveList.Count;
            if (moveList.Count > 1 && isDisplacedList[(moveList.Count > thinker.moveSpeed) ? thinker.moveSpeed : moveList.Count - 1])
                howMany = isDisplacedList.FindLastIndex((moveList.Count > thinker.moveSpeed) ? thinker.moveSpeed : moveList.Count - 1
                                                      , (moveList.Count > thinker.moveSpeed) ? thinker.moveSpeed : moveList.Count - 1, e => !e);
            thinker.moveList = moveList.Take(howMany).ToList();
            if (thinker.moveList.Count == 0)
                thinker.moveList.Add(Direction.None);
            return thinker.moveList;
            //      }
            //}
        }
    }
}
