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
            sight = new HashSet<Point>() { viewer.pos};
            //start at the left
            foreach (Demo.Direction d in new Demo.Direction[] {Demo.Direction.East, Demo.Direction.West, Demo.Direction.North, Demo.Direction.South})
            {
                scanVision(d, viewer.pos, visualRange);
            }
        }
        public void scanVision(Demo.Direction dir, Point pos, int range)
        {
            if (range <= 0)
                return;
            switch (dir)
            {
                case Demo.Direction.East:
                    {
                        Point p = new Point(pos.X + 1, pos.Y);
                        if (!sight.Contains(p))
                            sight.Add(p);
                        if (Demo.map[pos.Y, pos.X + 1] == DungeonMap.gr)
                        {
                            scanVision(Demo.Direction.East, p, range - 1);
                            scanVision(Demo.Direction.North, p, range - 1);
                            scanVision(Demo.Direction.South, p, range - 1);
                        }
                        else
                        {
                            p = new Point(pos.X + 1, pos.Y + 1);
                            if (!sight.Contains(p) && Demo.map[pos.Y + 1, pos.X + 1] != DungeonMap.gr)
                                sight.Add(p);
                            p = new Point(pos.X + 1, pos.Y - 1);
                            if (!sight.Contains(p) && Demo.map[pos.Y - 1, pos.X + 1] != DungeonMap.gr)
                                sight.Add(p);
                        }
                        break;
                    }
                case Demo.Direction.West:
                    {
                        Point p = new Point(pos.X - 1, pos.Y);
                        if (!sight.Contains(p))
                            sight.Add(p);
                        if (Demo.map[pos.Y, pos.X - 1] == DungeonMap.gr)
                        {
                            scanVision(Demo.Direction.West, p, range - 1);
                            scanVision(Demo.Direction.North, p, range - 1);
                            scanVision(Demo.Direction.South, p, range - 1);
                        }
                        else
                        {
                            p = new Point(pos.X - 1, pos.Y + 1);
                            if (!sight.Contains(p) && Demo.map[pos.Y + 1, pos.X - 1] != DungeonMap.gr)
                                sight.Add(p);
                            p = new Point(pos.X - 1, pos.Y - 1);
                            if (!sight.Contains(p) && Demo.map[pos.Y - 1, pos.X - 1] != DungeonMap.gr)
                                sight.Add(p);
                        }
                        break;
                    }
                case Demo.Direction.North:
                    {
                        Point p = new Point(pos.X, pos.Y - 1);
                        if (!sight.Contains(p))
                            sight.Add(p);
                        if (Demo.map[pos.Y - 1, pos.X] == DungeonMap.gr)
                        {
                            scanVision(Demo.Direction.East, p, range - 1);
                            scanVision(Demo.Direction.North, p, range - 1);
                            scanVision(Demo.Direction.West, p, range - 1);
                        }
                        else
                        {
                            p = new Point(pos.X + 1, pos.Y - 1);
                            if (!sight.Contains(p) && Demo.map[pos.Y - 1, pos.X + 1] != DungeonMap.gr)
                                sight.Add(p);
                            p = new Point(pos.X - 1, pos.Y - 1);
                            if (!sight.Contains(p) && Demo.map[pos.Y - 1, pos.X - 1] != DungeonMap.gr)
                                sight.Add(p);
                        }
                        break;
                    }
                case Demo.Direction.South:
                    {
                        Point p = new Point(pos.X, pos.Y + 1);
                        if (!sight.Contains(p))
                            sight.Add(p);
                        if (Demo.map[pos.Y + 1, pos.X] == DungeonMap.gr)
                        {
                            scanVision(Demo.Direction.East, p, range - 1);
                            scanVision(Demo.Direction.West, p, range - 1);
                            scanVision(Demo.Direction.South, p, range - 1);
                        }
                        else
                        {
                            p = new Point(pos.X + 1, pos.Y + 1);
                            if (!sight.Contains(p) && Demo.map[pos.Y + 1, pos.X + 1] != DungeonMap.gr)
                                sight.Add(p);
                            p = new Point(pos.X - 1, pos.Y + 1);
                            if (!sight.Contains(p) && Demo.map[pos.Y + 1, pos.X - 1] != DungeonMap.gr)
                                sight.Add(p);
                        }
                        break;
                    }
            }
        }
        public void calculateSightOld()
        {
            sight = new HashSet<Point>();

            for (int octant = 1; octant < 9; octant++)
            {
                scan(1, octant, 1.0, 0.0);
            }

        }

        //is the cell within vision distance and does it match the provided state
        private bool testCellOpen(int _x, int _y, int _cellState, int _depth)
        {
            try
            {
                if (!isVisible(viewer.pos.X, viewer.pos.Y, _x, _y)) throw new Exception();
                return Demo.map[_y, _x] == DungeonMap.gr;
            }
            catch
            {
                return false;
            }
        }
        //is the cell within vision distance and does it match the provided state
        private bool testCellClosed(int _x, int _y, int _cellState, int _depth)
        {
            try
            {
                if (!isVisible(viewer.pos.X, viewer.pos.Y, _x, _y)) throw new Exception();
                return Demo.map[_y, _x] != DungeonMap.gr;
            }
            catch
            {
                return false;
            }
        }

        public int getMapPoint(int _x, int _y)
        {
            return Demo.map[_y, _x];
        }


        private double getSlope(double _x1, double _y1, double _x2, double _y2)
        {
            return (_x1 - _x2) / (_y1 - _y2);
        }

        private double getSlopeInv(double _x1, double _y1, double _x2, double _y2)
        {
            return (_y1 - _y2) / (_x1 - _x2);
        }

        protected void scan(int _depth, int _octant, double _startSlope, double _endSlope)
        {
            int x = 0;
            int y = 0;

            switch (_octant)
            {
                case 1:
                    y = viewer.pos.Y - _depth;
                    x = viewer.pos.X - Convert.ToInt32((_startSlope * Convert.ToDouble(_depth)));

                    if (x < 0) break;
                    if (x >= Demo.map.GetLength(1)) break;
                    if (y < 0) break;
                    if (y >= Demo.map.GetLength(0)) break;

                    while (getSlope(x, y, viewer.pos.X, viewer.pos.Y) >= _endSlope)
                    {
                        if (isVisible(viewer.pos.X, viewer.pos.Y, x, y))
                        {

                            if (Demo.map[y, x] != DungeonMap.gr) //cell blocked
                            {

                                //if prior open AND within range
                                if (testCellOpen(x - 1, y, 0, _depth))
                                {
                                    //recursion
                                    scan(_depth + 1, _octant, _startSlope, getSlope(x - .5, y + 0.5, viewer.pos.X, viewer.pos.Y));
                                }

                            }
                            else //not blocked
                            {
                                //if prior closed AND within range
                                if (testCellClosed(x - 1, y, 1, _depth))
                                {
                                    _startSlope = getSlope(x - .5, y - 0.5, viewer.pos.X, viewer.pos.Y);
                                }
                                sight.Add(new Point(x, y));
                            }

                        }
                        x++;

                    }
                    x--; //we step back as the last step of the while has taken us past the limit

                    break;

                case 2:

                    y = viewer.pos.Y - _depth;
                    x = viewer.pos.X + Convert.ToInt32((_startSlope * Convert.ToDouble(_depth)));

                    if (x < 0) break;
                    if (x >= Demo.map.GetLength(1)) break;
                    if (y < 0) break;
                    if (y >= Demo.map.GetLength(0)) break;

                    while (getSlope(x, y, viewer.pos.X, viewer.pos.Y) <= _endSlope)
                    {
                        if (isVisible(viewer.pos.X, viewer.pos.Y, x, y))
                        {

                            if (Demo.map[y, x] != DungeonMap.gr)
                            {
                                if (testCellOpen(x + 1, y, 0, _depth))
                                {
                                    scan(_depth + 1, _octant, _startSlope, getSlope(x + 0.5, y + 0.5, viewer.pos.X, viewer.pos.Y));
                                }
                            }
                            else
                            {
                                if (testCellClosed(x + 1, y, 1, _depth))
                                {
                                    _startSlope = -getSlope(x + 0.5, y - 0.5, viewer.pos.X, viewer.pos.Y);
                                }
                                sight.Add(new Point(x, y));
                            }

                        }
                        x--;

                    }
                    x++;

                    break;


                case 3:

                    x = viewer.pos.X + _depth;
                    y = viewer.pos.Y - Convert.ToInt32((_startSlope * Convert.ToDouble(_depth)));

                    if (x < 0) break;
                    if (x >= Demo.map.GetLength(1)) break;
                    if (y < 0) break;
                    if (y >= Demo.map.GetLength(0)) break;

                    while (getSlopeInv(x, y, viewer.pos.X, viewer.pos.Y) <= _endSlope)
                    {

                        if (isVisible(viewer.pos.X, viewer.pos.Y, x, y))
                        {

                            if (Demo.map[y, x] != DungeonMap.gr) //cell blocked
                            {
                                //if prior open AND within range
                                if (testCellOpen(x, y - 1, 0, _depth))
                                {
                                    scan(_depth + 1, _octant, _startSlope, getSlopeInv(x - 0.5, y - 0.5, viewer.pos.X, viewer.pos.Y));
                                }
                            }
                            else //not blocked
                            {
                                //if prior closed AND within range
                                if (testCellClosed(x, y - 1, 1, _depth))
                                {
                                    _startSlope = -getSlopeInv(x + 0.5, y - 0.5, viewer.pos.X, viewer.pos.Y);
                                }
                                sight.Add(new Point(x, y));
                            }

                        }
                        y++;

                    }
                    y--; //we step back as the last step of the while has taken us past the limit

                    break;

                case 4:

                    x = viewer.pos.X + _depth;
                    y = viewer.pos.Y + Convert.ToInt32((_startSlope * Convert.ToDouble(_depth)));

                    if (x < 0) break;
                    if (x >= Demo.map.GetLength(1)) break;
                    if (y < 0) break;
                    if (y >= Demo.map.GetLength(0)) break; ;

                    while (getSlopeInv(x, y, viewer.pos.X, viewer.pos.Y) >= _endSlope)
                    {

                        if (isVisible(viewer.pos.X, viewer.pos.Y, x, y))
                        {

                            if (Demo.map[y, x] != DungeonMap.gr)
                            {

                                if (testCellOpen(x, y + 1, 0, _depth))
                                {
                                    scan(_depth + 1, _octant, _startSlope, getSlopeInv(x - 0.5, y + 0.5, viewer.pos.X, viewer.pos.Y));
                                }
                            }
                            else
                            {

                                if (testCellClosed(x, y + 1, 1, _depth))
                                {
                                    _startSlope = getSlopeInv(x + 0.5, y + 0.5, viewer.pos.X, viewer.pos.Y);
                                }
                                sight.Add(new Point(x, y));
                            }

                        }
                        y--;

                    }
                    y++;

                    break;

                case 5:

                    y = viewer.pos.Y + _depth;
                    x = viewer.pos.X + Convert.ToInt32((_startSlope * Convert.ToDouble(_depth)));

                    if (x < 0) break;
                    if (x >= Demo.map.GetLength(1)) break;
                    if (y < 0) break;
                    if (y >= Demo.map.GetLength(0)) break;

                    while (getSlope(x, y, viewer.pos.X, viewer.pos.Y) >= _endSlope)
                    {
                        if (isVisible(viewer.pos.X, viewer.pos.Y, x, y))
                        {

                            if (Demo.map[y, x] != DungeonMap.gr)
                            {

                                if (testCellOpen(x + 1, y, 0, _depth))
                                {
                                    scan(_depth + 1, _octant, _startSlope, getSlope(x + 0.5, y - 0.5, viewer.pos.X, viewer.pos.Y));
                                }
                            }
                            else
                            {
                                if (testCellClosed(x + 1, y, 1, _depth))
                                {
                                    _startSlope = getSlope(x + 0.5, y + 0.5, viewer.pos.X, viewer.pos.Y);
                                }
                                sight.Add(new Point(x, y));
                            }

                        }
                        x--;

                    }
                    x++;

                    break;

                case 6:

                    y = viewer.pos.Y + _depth;
                    x = viewer.pos.X - Convert.ToInt32((_startSlope * Convert.ToDouble(_depth)));

                    if (x < 0) break;
                    if (x >= Demo.map.GetLength(1)) break;
                    if (y < 0) break;
                    if (y >= Demo.map.GetLength(0)) break;

                    while (getSlope(x, y, viewer.pos.X, viewer.pos.Y) <= _endSlope)
                    {
                        if (isVisible(viewer.pos.X, viewer.pos.Y, x, y))
                        {

                            if (Demo.map[y, x] != DungeonMap.gr)
                            {

                                if (testCellOpen(x - 1, y, 0, _depth))
                                {
                                    scan(_depth + 1, _octant, _startSlope, getSlope(x - 0.5, y - 0.5, viewer.pos.X, viewer.pos.Y));
                                }
                            }
                            else
                            {
                                if (testCellClosed(x - 1, y, 1, _depth))
                                {
                                    _startSlope = -getSlope(x - 0.5, y + 0.5, viewer.pos.X, viewer.pos.Y);
                                }
                                sight.Add(new Point(x, y));
                            }

                        }
                        x++;

                    }
                    x--;

                    break;

                case 7:

                    x = viewer.pos.X - _depth;
                    y = viewer.pos.Y + Convert.ToInt32((_startSlope * Convert.ToDouble(_depth)));

                    if (x < 0) break;
                    if (x >= Demo.map.GetLength(1)) break;
                    if (y < 0) break;
                    if (y >= Demo.map.GetLength(0)) break;

                    while (getSlopeInv(x, y, viewer.pos.X, viewer.pos.Y) <= _endSlope)
                    {
                       // Console.WriteLine(String.Format("x:{0}, y:{1}", x, y));

                        if (isVisible(viewer.pos.X, viewer.pos.Y, x, y))
                        {

                            if (Demo.map[y, x] != DungeonMap.gr)
                            {
                                if (testCellOpen(x, y + 1, 0, _depth))
                                {
                                    scan(_depth + 1, _octant, _startSlope, getSlopeInv(x + 0.5, y + 0.5, viewer.pos.X, viewer.pos.Y));
                                }
                            }
                            else
                            {
                                if (testCellClosed(x, y + 1, 1, _depth))
                                {
                                    _startSlope = getSlopeInv(x - 0.5, y + 0.5, viewer.pos.X, viewer.pos.Y);
                                }
                                sight.Add(new Point(x, y));
                            }

                        }
                        y--;

                    }
                    y++;
                    break;

                case 8:

                    x = viewer.pos.X - _depth;
                    y = viewer.pos.Y - Convert.ToInt32((_startSlope * Convert.ToDouble(_depth)));

                    if (x < 0) break;
                    if (x >= Demo.map.GetLength(1)) break;
                    if (y < 0) break;
                    if (y >= Demo.map.GetLength(0)) break;

                    while (getSlopeInv(x, y, viewer.pos.X, viewer.pos.Y) >= _endSlope)
                    {

                        if (isVisible(viewer.pos.X, viewer.pos.Y, x, y))
                        {

                            if (Demo.map[y, x] != DungeonMap.gr)
                            {
                                if (testCellOpen(x, y - 1, 0, _depth))
                                {
                                    scan(_depth + 1, _octant, _startSlope, getSlopeInv(x + 0.5, y - 0.5, viewer.pos.X, viewer.pos.Y));
                                }
                            }
                            else
                            {
                                if (testCellClosed(x, y - 1, 1, _depth))
                                {
                                    _startSlope = getSlopeInv(x - 0.5, y - 0.5, viewer.pos.X, viewer.pos.Y);
                                }
                                sight.Add(new Point(x, y));
                            }

                        }
                        y++;

                    }
                    y--;

                    break;


            }


            if (x < 0) x = 0;
            if (x >= Demo.map.GetLength(1)) x = Demo.map.GetLength(1) - 1;
            if (y < 0) y = 0;
            if (y >= Demo.map.GetLength(0)) y = Demo.map.GetLength(0) - 1;


            if (isVisible(viewer.pos.X, viewer.pos.Y, x, y) & Demo.map[y, x] == DungeonMap.gr)
            {
                scan(_depth + 1, _octant, _startSlope, _endSlope);
            }


        }

        //See if the provided points are within visual range
        protected bool isVisible(int _x1, int _y1, int _x2, int _y2)
        {
            try
            {
                int i = Demo.map[_y1, _x1];  //illegal values throw an error
                i = Demo.map[_y2, _x2];

                if (_x1 == _x2) //if they're on the same axis, we only need to test one
                //value, which is computationally cheaper than what we do below
                {
                    return Math.Abs(_y1 - _y2) <= visualRange;
                }

                if (_y1 == _y2)
                {
                    return Math.Abs(_x1 - _x2) <= visualRange;
                }

                return (Math.Pow((_x1 - _x2), 2) + Math.Pow((_y1 - _y2), 2)) <= Math.Pow(visualRange, 2);
            }
            catch
            {
                return false;
            }
        }
    }
}
