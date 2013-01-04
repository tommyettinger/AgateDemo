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
    public class Vision
    {
        public Demo.Mob viewer;
        public Boolean vision = true;

        public HashSet<Point> sight;

        public int visualRange = 10;
        public Vision(Demo.Mob viewer)
        {
            this.viewer = viewer;
        }
        /// <summary>
        /// Go through all the octants
        /// </summary>
        public void calculateSight()
        {
            sight = new HashSet<Point>() { viewer.pos };
            //start at the left
            /*foreach (Demo.Direction d in new Demo.Direction[] {Demo.Direction.East, Demo.Direction.West, Demo.Direction.North, Demo.Direction.South})
            {
                scanVisionOld(d, viewer.pos, visualRange);
            }*/
            computeFov(ref Demo.map, viewer.x, viewer.y, visualRange, true);
            //scanVision(viewer.pos, visualRange);
        }
        public Point normalizeCell(Point pt)
        {

            int betterX = pt.X, betterY = pt.Y;

            if (betterY < 0 || betterX < 0 || Demo.map.GetLength(0) < betterY || Demo.map.GetLength(1) < betterX)
            {
                if (betterX < 0) betterX = 0;
                if (betterY < 0) betterY = 0;
                if (betterX > Demo.map.GetUpperBound(1)) betterX = Demo.map.GetUpperBound(1);
                if (betterY > Demo.map.GetUpperBound(0)) betterY = Demo.map.GetUpperBound(0);
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
                        if (obstaclesInLastLine > 0 && sight.Contains(new Point(x, y)) == false)
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
                                if (visible && (sight.Contains(new Point(x, y - dy)) == false || m[y - dy, x] != DungeonMap.gr) && (x - dx >= 0 && x - dx < m.GetLength(1) && (sight.Contains(new Point(x - dx, y - dy)) == false || m[y - dy, x - dx] != DungeonMap.gr)))
                                    visible = false;
                                idx++;
                            }
                        }
                        if (visible)
                        {
                            if(!sight.Contains(new Point(x, y)) && Math.Abs(playerX - x) + Math.Abs(playerY - y) <= maxRadius)
                                sight.Add(new Point(x, y));
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
                                    sight.Remove (new Point(x, y));
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
                        if (obstaclesInLastLine > 0 && !sight.Contains(new Point(x, y)))
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
                                if (visible && (sight.Contains(new Point(x - dx, y)) == false || m[y, x - dx] != DungeonMap.gr) && (y - dy >= 0 && y - dy < m.GetLength(0) && (sight.Contains(new Point(x - dx, y - dy)) == false || m[y - dy, x - dx] != DungeonMap.gr)))
                                    visible = false;
                                idx++;
                            }
                        }
                        if (visible)
                        {
                            if (!sight.Contains(new Point(x, y)) && Math.Abs(playerX - x) + Math.Abs(playerY - y) <= maxRadius)
                                sight.Add(new Point(x, y));
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
                                    sight.Remove(new Point(x, y));
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


    }
}
