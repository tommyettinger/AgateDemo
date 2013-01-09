/* This file contains code originally from MRPAS.  That project's license is as follows:
* MRPAS.NET
* Copyright (c) 2010 Dominik Marczuk
* All rights reserved.
*
* Redistribution and use in source and binary forms, with or without
* modification, are permitted provided that the following conditions are met:
*     * Redistributions of source code must retain the above copyright
*       notice, this list of conditions and the following disclaimer.
*     * Redistributions in binary form must reproduce the above copyright
*       notice, this list of conditions and the following disclaimer in the
*       documentation and/or other materials provided with the distribution.
*     * The name of Dominik Marczuk may not be used to endorse or promote products
*       derived from this software without specific prior written permission.
*
* THIS SOFTWARE IS PROVIDED BY DOMINIK MARCZUK ``AS IS'' AND ANY
* EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
* WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
* DISCLAIMED. IN NO EVENT SHALL DOMINIK MARCZUK BE LIABLE FOR ANY
* DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
* (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
* LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
* ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
* (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
* SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AgateDemo;
using AgateLib.Geometry;
namespace AgateDemo
{
    public static class ArrayExtensions
    {
        public static void Fill<T>(this Array array, Func<int[], T> func)
        {
            var indices = new int[array.Rank];
            array.Fill(indices, 0, func);
        }

        private static void Fill<T>(this Array array, int[] indices, int rank,
            Func<int[], T> func)
        {
            for (int index = array.GetLowerBound(rank); index <= array.GetUpperBound(rank);
                index++)
            {
                indices[rank] = index;

                if (rank < array.Rank - 1)
                {
                    array.Fill(indices, rank + 1, func);
                }
                else
                {
                    array.SetValue(func(indices), indices);
                }
            }
        }
    }
    public class Vision
    {
        public Demo.Mob viewer;
        public Boolean vision = true;

        public HashSet<Point> sightSet;
        public double[,] sight;
        public int visualRange = 10;
        public Vision(Demo.Mob viewer)
        {
            this.viewer = viewer;
            //sightSet = new HashSet<Point>() { this.viewer.pos };
        }
        public void calculateSightOld()
        {
            sightSet = new HashSet<Point>() { viewer.pos };
            //start at the left
            /*foreach (Demo.Direction d in new Demo.Direction[] {Demo.Direction.East, Demo.Direction.West, Demo.Direction.North, Demo.Direction.South})
            {
                scanVisionOld(d, viewer.pos, visualRange);
            }*/
            computeFov(ref Demo.currentLevel.map, viewer.x, viewer.y, visualRange, true);
            //scanVision(viewer.pos, visualRange);
        }
        public Point normalizeCell(Point pt)
        {

            int betterX = pt.X, betterY = pt.Y;

            if (betterY < 0 || betterX < 0 || Demo.currentLevel.map.GetLength(0) < betterY || Demo.currentLevel.map.GetLength(1) < betterX)
            {
                if (betterX < 0) betterX = 0;
                if (betterY < 0) betterY = 0;
                if (betterX > Demo.currentLevel.map.GetUpperBound(1)) betterX = Demo.currentLevel.map.GetUpperBound(1);
                if (betterY > Demo.currentLevel.map.GetUpperBound(0)) betterY = Demo.currentLevel.map.GetUpperBound(0);
                return new Point(betterX, betterY);
            }
            return pt;
        }

        private void computeQuadrant(ref int[,] m, int playerX, int playerY, int maxRadius, bool lightWalls, int maxObstacles, int dx, int dy)
        {
            double[] startAngle, endAngle;
            int angleArraySize;
            angleArraySize = maxObstacles;
            startAngle = new double[maxObstacles * 2];
            endAngle = new double[maxObstacles];
            //octant: vertical edge:
            {
                int iteration = 1;
                bool done = false;
                int totalObstacles = 0;
                int obstaclesInLastLine = 0;
                double minAngle = 0.0;
                int x, y;
                //do while there are unblocked slopes left and the algo is within the map's boundaries
                //scan progressive lines/columns from the PC outwards
                y = playerY + dy;
                if (y < 0 || y >= m.GetLength(0))
                    done = true;
                while (!done)
                {
                    //process cells in the line
                    double slopesPerCell = 1.0 / (double)(iteration + 1);
                    double halfSlopes = slopesPerCell * 0.5;
                    int processedCell = (int)(minAngle / slopesPerCell);
                    int minx = Math.Max(0, playerX - iteration), maxx = Math.Min(m.GetLength(1) - 1, playerX + iteration);
                    done = true;
                    for (x = playerX + (processedCell * dx); x >= minx && x <= maxx; x += dx)
                    {
                        //int c = x + (y * m.GetLength(1));
                        //calculate slopes per cell
                        bool visible = true;
                        double startSlope = (double)processedCell * slopesPerCell;
                        double centreSlope = startSlope + halfSlopes;
                        double endSlope = startSlope + slopesPerCell;
                        if (obstaclesInLastLine > 0 && sightSet.Contains(new Point(x, y)) == false)
                        {
                            int idx = 0;
                            while (visible && idx < obstaclesInLastLine)
                            {
                                if (m[y, x] == DungeonMap.gr) //transparent is true
                                {
                                    if (centreSlope > startAngle[idx] && centreSlope < endAngle[idx])
                                        visible = false;
                                }
                                else
                                {
                                    if (startSlope >= startAngle[idx] && endSlope <= endAngle[idx])
                                        visible = false;
                                }
                                if (visible && (sightSet.Contains(new Point(x, y - dy)) == false || m[y - dy, x] != DungeonMap.gr) && (x - dx >= 0 && x - dx < m.GetLength(1) && (sightSet.Contains(new Point(x - dx, y - dy)) == false || m[y - dy, x - dx] != DungeonMap.gr)))
                                    visible = false;
                                idx++;
                            }
                        }
                        if (visible)
                        {
                            if(!sightSet.Contains(new Point(x, y)) && Math.Abs(playerX - x) + Math.Abs(playerY - y) <= maxRadius)
                                sightSet.Add(new Point(x, y));
                            done = false;
                            //if the cell is opaque, block the adjacent slopes
                            if (m[y, x] != DungeonMap.gr)
                            {
                                if (minAngle >= startSlope)
                                    minAngle = endSlope;
                                else
                                {
                                    startAngle[totalObstacles] = startSlope;
                                    endAngle[totalObstacles++] = endSlope;
                                }
                                if (!lightWalls)
                                    sightSet.Remove (new Point(x, y));
                            }
                        }
                        processedCell++;
                    }
                    if (iteration == maxRadius)
                        done = true;
                    iteration++;
                    obstaclesInLastLine = totalObstacles;
                    y += dy;
                    if (y < 0 || y >= m.GetLength(0))
                        done = true;
                    if (minAngle == 1.0)
                        done = true;
                }
            }
            //octant: horizontal edge
            {
                int iteration = 1; //iteration of the algo for this octant
                bool done = false;
                int totalObstacles = 0;
                int obstaclesInLastLine = 0;
                double minAngle = 0.0;
                int x, y;
                //do while there are unblocked slopes left and the algo is within the map's boundaries
                //scan progressive lines/columns from the PC outwards
                x = playerX + dx; //the outer slope's coordinates (first processed line)
                if (x < 0 || x >= m.GetLength(1))
                    done = true;
                while (!done)
                {
                    //process cells in the line
                    double slopesPerCell = 1.0 / (double)(iteration + 1);
                    double halfSlopes = slopesPerCell * 0.5;
                    int processedCell = (int)(minAngle / slopesPerCell);
                    int miny = Math.Max(0, playerY - iteration), maxy = Math.Min(m.GetLength(0) - 1, playerY + iteration);
                    done = true;
                    for (y = playerY + (processedCell * dy); y >= miny && y <= maxy; y += dy)
                    {
                        int c = x + (y * m.GetLength(1));
                        //calculate slopes per cell
                        bool visible = true;
                        double startSlope = (double)(processedCell * slopesPerCell);
                        double centreSlope = startSlope + halfSlopes;
                        double endSlope = startSlope + slopesPerCell;
                        if (obstaclesInLastLine > 0 && !sightSet.Contains(new Point(x, y)))
                        {
                            int idx = 0;
                            while (visible && idx < obstaclesInLastLine)
                            {
                                if (m[y, x] == DungeonMap.gr)
                                {
                                    if (centreSlope > startAngle[idx] && centreSlope < endAngle[idx])
                                        visible = false;
                                }
                                else
                                {
                                    if (startSlope >= startAngle[idx] && endSlope <= endAngle[idx])
                                        visible = false;
                                }
                                if (visible && (sightSet.Contains(new Point(x - dx, y)) == false || m[y, x - dx] != DungeonMap.gr) && (y - dy >= 0 && y - dy < m.GetLength(0) && (sightSet.Contains(new Point(x - dx, y - dy)) == false || m[y - dy, x - dx] != DungeonMap.gr)))
                                    visible = false;
                                idx++;
                            }
                        }
                        if (visible)
                        {
                            if (!sightSet.Contains(new Point(x, y)) && Math.Abs(playerX - x) + Math.Abs(playerY - y) <= maxRadius)
                                sightSet.Add(new Point(x, y));
                            done = false;
                            //if the cell is opaque, block the adjacent slopes
                            if (m[y, x] != DungeonMap.gr)
                            {
                                if (minAngle >= startSlope)
                                    minAngle = endSlope;
                                else
                                {
                                    startAngle[totalObstacles] = startSlope;
                                    endAngle[totalObstacles++] = endSlope;
                                }
                                if (!lightWalls)
                                    sightSet.Remove(new Point(x, y));
                            }
                        }
                        processedCell++;
                    }
                    if (iteration == maxRadius)
                        done = true;
                    iteration++;
                    obstaclesInLastLine = totalObstacles;
                    x += dx;
                    if (x < 0 || x >= m.GetLength(1))
                        done = true;
                    if (minAngle == 1.0)
                        done = true;
                }
            }
        }
        public void computeFov(ref int[,] m, int playerX, int playerY, int maxRadius, bool lightWalls)
        {
            //                int c;
            int maxObstacles;
            //first, zero the FOV map
            /*for (c = m.Length - 1; c >= 0; c--)
            {
                m.cells[c].fov = false;
            }*/

            //calculate an approximated (excessive, just in case) maximum number of obstacles per octant
            maxObstacles = m.Length / 7;

            //set PC's position as visible
            //sight.Add(new Point(playerX, playerY)); //unnecessary

            //compute the 4 quadrants of the map
            computeQuadrant(ref m, playerX, playerY, maxRadius, lightWalls, maxObstacles, 1, 1);
            computeQuadrant(ref m, playerX, playerY, maxRadius, lightWalls, maxObstacles, 1, -1);
            computeQuadrant(ref m, playerX, playerY, maxRadius, lightWalls, maxObstacles, -1, 1);
            computeQuadrant(ref m, playerX, playerY, maxRadius, lightWalls, maxObstacles, -1, -1);
        }

        /**
 * Performs FOV by pushing values outwards from the source location.
 *
 * This algorithm does perform bounds checking.
 *
 * @author Eben Howard - http://squidpony.com - eben@squidpony.com
 */

        public void calculateSight()
        {
        //    sight = new HashSet<Point>() { viewer.pos };
            //start at the left
            /*foreach (Demo.Direction d in new Demo.Direction[] {Demo.Direction.East, Demo.Direction.West, Demo.Direction.North, Demo.Direction.South})
            {
                scanVisionOld(d, viewer.pos, visualRange);
            }*/

            sight = calculateFOV(calculateVisionMap(viewer.dlevel.map), viewer.x, viewer.y, visualRange);
            //scanVision(viewer.pos, visualRange);
        }
        private double[,] calculateVisionMap(int[,] dungeon)
        {
            double[,] vmap = new double[dungeon.GetLength(0), dungeon.GetLength(1)];
            for (int j = 0; j < dungeon.GetLength(0); j++)
            {
                for (int i = 0; i < dungeon.GetLength(1); i++)
                {
                    switch (dungeon[j, i])
                    {
                        case 1194: { vmap[j, i] = 0.0; break; }
                        default: { vmap[j, i] = 1.0; break; }
                    }
                }
            }
            return vmap;
        }
        private double[,] lightMap;
    private double[,] map;
    private double radius, decay;
    private int startx, starty, width, height;
    private bool simplified;

    /**
     * Find the light let through by the nearest square.
     *
     * @param x
     * @param y
     * @return
     */
    private double getNearLight(int x, int y)
    {
        int x2 = x - (int)Math.Sign(x - startx);
        int y2 = y - (int)Math.Sign(y - starty);

        //clamp x2 and y2 to bound within map
        x2 = Math.Max(0, x2);
        x2 = Math.Min(width - 1, x2);
        y2 = Math.Max(0, y2);
        y2 = Math.Min(height - 1, y2);

        //find largest emmitted light in direction of source
        double light = 0 ;
        int xDominant = Math.Abs(x - startx) - Math.Abs(y - starty);

        int lit = 0;
        if (map[y2, x2] < 1 && lightMap[y2, x2] > 0)
        {
            light = Math.Max(light, lightMap[y2, x2] * (1 - map[y2, x2]));
            lit++;
            if (xDominant == 0)
            {
                lit++;
            }
        }
        if (map[y2, x] < 1 && lightMap[y2, x] > 0)
        {
            light = Math.Max(light, lightMap[y2, x] * (1 - map[y2, x]));
            lit++;
            if (xDominant < 0)
            {
                lit++;
            }
        }
        if (map[y, x2] < 1 && lightMap[y, x2] > 0)
        {
            light = Math.Max(light, lightMap[y, x2] * (1 - map[y, x2]));
            lit++;
            if (xDominant > 0)
            {
                lit++;
            }
        }
        if (lit < 2 && !(map[y2, x2] < 1 && map[y, x2] >= 1 && map[y2, x] >= 1))
        {
            light = 0;
        }

        double distance = 1;
        if (!simplified && x2 != x && y2 != y)
        {//it's a diagonal
            distance = Math.Sqrt(2);
        }

        distance = Math.Max(0, distance);
        light = light - decay * distance;
        return light;
    }
    private double getNearLightOld(int x, int y) {
        int x2 = x - (int) Math.Sign(x - startx);
        int y2 = y - (int) Math.Sign(y - starty);

        //clamp x2 and y2 to bound within map
        x2 = Math.Max(0, x2);
        x2 = Math.Min(width - 1, x2);
        y2 = Math.Max(0, y2);
        y2 = Math.Min(height - 1, y2);

        //find largest emmitted light in direction of source
        double light = 0;
        int lit = 0;
        if (map[y2, x2] < 1f && lightMap[y2, x2] > 0)
        {
            light = Math.Max(light, lightMap[y2, x2] * (1 - map[y2, x2]));
            lit++;
        }
        if (map[y2, x] < 1f && lightMap[y2, x] > 0)
        {
            light = Math.Max(light, lightMap[y2, x] * (1 - map[y2, x]));
            lit++;
        }
        if (map[y, x2] < 1f && lightMap[y, x2] > 0)
        {
            light = Math.Max(light, lightMap[y, x2] * (1 - map[y, x2]));
            lit++;
        }
        if (lit < 2)
        {
            light = 0;
        }

        double distance = 1;
        if (!simplified && x2 != x && y2 != y)
        {//it's a diagonal
            distance = Math.Sqrt(2);
        }
        /*
        light = Math.Max(Math.Max(lightMap[y, x2] * (1 - map[y, x2]),
                lightMap[y2,x] * (1 - map[y2,x])),
                lightMap[y2,x2] * (1 - map[y2,x2]));

        double distance = 1;
        if (!simplified && x2 != x && y2 != y) {//it's a diagonal
            distance = Math.Sqrt(2);
        }
        */
        distance = Math.Max(0, distance);
        light = light - decay * distance;
        return light;
    }
        
    public double[,] calculateFOV(double[,] map, int startx, int starty, double force, double decay, bool simplifiedDiagonals) {
        this.map = map;
        this.decay = decay;
        this.startx = startx;
        this.starty = starty;
        this.simplified = simplifiedDiagonals;
        radius = force / decay;//assume worst case of no resistance in tiles
        width = map.GetLength(1);
        height = map.GetLength(0);
        lightMap = new double[height,width];

        lightMap[starty,startx] = force;//make the starting space full power

        lightSurroundings(startx, starty);

        return lightMap;
    }

    private void lightSurroundings(int x, int y) {
        if (lightMap[y, x] <= 0) {
            return;//no light to spread
        }

        for (int dx = x - 1; dx <= x + 1; dx++) {
            for (int dy = y - 1; dy <= y + 1; dy++) {
                //ensure in bounds
                if (dx < 0 || dx >= width || dy < 0 || dy >= height) {
                    continue;
                }

                double r2;
                if (simplified) {
                    r2 = Math.Sqrt((dx - startx) * (dx - startx) + (dy - starty) * (dy - starty));
                } else {
                    r2 = Math.Abs(dx - startx) + Math.Abs(dy - starty);
                }
                if (r2 <= radius) {
                    double surroundingLight = getNearLight(dx, dy);
                    if (lightMap[dy,dx] < surroundingLight) {
                        lightMap[dy,dx] = surroundingLight;
                        lightSurroundings(dx, dy);//redo neighbors since this one's light changed
                    }
                }
            }
        }
    }

    
    public double[,] calculateFOV(double[,] map, int startx, int starty, double radius) {
        return calculateFOV(map, startx, starty, 1, 1 / radius, true);
    }
    }
}
