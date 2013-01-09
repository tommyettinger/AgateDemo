using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AgateLib.Geometry;
namespace AgateDemo
{
    /// <summary>
    /// The BresLine class exposes a line calculated using the Bresenham Line Algorithm
    /// in an iterative approach with proper ordering of points between beginning and end
    /// </summary>
    public static class Line
    {
        //for (int row = (cursorY < 20) ? 0 : (cursorY > mapHeight - 10) ? mapHeight - 20 : cursorY - 20; row <= mapHeight && row <= cursorY + 20; row++)
        //for (int col = (cursorX <= 10) ? 0 : (cursorX > mapWidth - 10) ? mapWidth - 19 : cursorX - 10; col <= mapWidth && (col < cursorX + 10 || col < 20); col++)

        public static List<Point> LineHookDumb(Point begin, Point end)
        {
            List<Point> ret = new List<Point>();
            if (Math.Abs(end.Y - begin.Y) < Math.Abs(end.X - begin.X))
            {
                // dX > dY... not steep
                if (end.X >= begin.X)
                {
                    for (int i = begin.X; i <= end.X; i++)
                    {
                        if (i > Demo.mapWidth - 10)
                            continue;
                        ret.Add(new Point(i, begin.Y));
                    }
                    ret.Add(new Point(begin.X, end.Y));
                    if (end.Y >= begin.Y)
                    {
                        for (int i = begin.Y + 1; i <= end.Y; i++)
                        {
                            if (i > Demo.mapHeight - 10)
                                continue;
                            ret.Add(new Point(end.X, i));
                        }
                    }
                    else
                    {
                        for (int i = begin.Y - 1; i >= end.Y; i--)
                        {
                            if (i < 20)
                                continue;
                            ret.Add(new Point(end.X, i));
                        }
                    }
                }
                else
                {
                    for (int i = begin.X; i >= end.X; i--)
                    {
                        if (i <= 10)
                            continue;

                        ret.Add(new Point(i, begin.Y));
                    }
                    ret.Add(new Point(begin.X, end.Y));
                    if (end.Y >= begin.Y)
                    {
                        for (int i = begin.Y + 1; i <= end.Y; i++)
                        {
                            if (i > Demo.mapHeight - 10)
                                continue;
                            ret.Add(new Point(end.X, i));
                        }
                    }
                    else
                    {
                        for (int i = begin.Y - 1; i >= end.Y; i--)
                        {
                            if (i < 20)
                                continue;
                            ret.Add(new Point(end.X, i));
                        }
                    }
                }
            }
            else // steep (dY > dX)
            {
                if (end.Y >= begin.Y)
                {
                    for (int i = begin.Y; i <= end.Y; i++)
                    {
                        if (i > Demo.mapHeight - 10)
                            continue;
                        ret.Add(new Point(begin.X, i));
                    }
                    ret.Add(new Point(begin.Y, end.X));
                    if (end.X >= begin.X)
                    {
                        for (int i = begin.X + 1; i <= end.X; i++)
                        {
                            if (i > Demo.mapWidth - 10)
                                continue;

                            ret.Add(new Point(i, end.Y));
                        }
                    }
                    else
                    {
                        for (int i = begin.X - 1; i >= end.X; i--)
                        {
                            if (i <= 10)
                                continue;
                            ret.Add(new Point(i, end.Y));
                        }
                    }
                }
                else
                {
                    for (int i = begin.Y; i >= end.Y; i--)
                    {
                        if (i < 20)
                            continue;
                        ret.Add(new Point(begin.X, i));
                    }
                    ret.Add(new Point(begin.Y, end.X));
                    if (end.X >= begin.X)
                    {
                        for (int i = begin.X + 1; i <= end.X; i++)
                        {
                            if (i > Demo.mapWidth - 10)
                                continue;

                            ret.Add(new Point(i, end.Y));
                        }
                    }
                    else
                    {
                        for (int i = begin.X - 1; i >= end.X; i--)
                        {
                            if (i <= 10)
                                continue;
                            ret.Add(new Point(i, end.Y));
                        }
                    }
                }
            }
            return ret;
        }
        public static List<Point> LineHook(Point begin, Point end)
        {
            List<Point> ret = new List<Point>();
            if (Math.Abs(end.Y - begin.Y) < Math.Abs(end.X - begin.X))
            {
                // dX > dY... not steep
                if (end.X >= begin.X)
                {
                    for (int i = begin.X; i <= end.X; i++)
                    {
                        ret.Add(new Point(i, begin.Y));
                    }
                    if (end.Y >= begin.Y)
                    {
                        for (int i = begin.Y + 1; i <= end.Y; i++)
                        {
                            ret.Add(new Point(end.X, i));
                        }
                    }
                    else
                    {
                        for (int i = begin.Y - 1; i >= end.Y; i--)
                        {
                            ret.Add(new Point(end.X, i));
                        }
                    }
                }
                else
                {
                    for (int i = begin.X; i >= end.X; i--)
                    {
                        ret.Add(new Point(i, begin.Y));
                    }
                    if (end.Y >= begin.Y)
                    {
                        for (int i = begin.Y + 1; i <= end.Y; i++)
                        {
                            ret.Add(new Point(end.X, i));
                        }
                    }
                    else
                    {
                        for (int i = begin.Y - 1; i >= end.Y; i--)
                        {
                            ret.Add(new Point(end.X, i));
                        }
                    }
                }
            }
            else // steep (dY > dX)
            {
                if (end.Y >= begin.Y)
                {
                    for (int i = begin.Y; i <= end.Y; i++)
                    {
                        ret.Add(new Point(begin.X, i));
                    }
                    if (end.X >= begin.X)
                    {
                        for (int i = begin.X + 1; i <= end.X; i++)
                        {
                            ret.Add(new Point(i, end.Y));
                        }
                    }
                    else
                    {
                        for (int i = begin.X - 1; i >= end.X; i--)
                        {
                            ret.Add(new Point(i, end.Y));
                        }
                    }
                }
                else
                {
                    for (int i = begin.Y; i >= end.Y; i--)
                    {
                        ret.Add(new Point(begin.X, i));
                    }
                    if (end.X >= begin.X)
                    {
                        for (int i = begin.X + 1; i <= end.X; i++)
                        {
                            ret.Add(new Point(i, end.Y));
                        }
                    }
                    else
                    {
                        for (int i = begin.X - 1; i >= end.X; i--)
                        {
                            ret.Add(new Point(i, end.Y));
                        }
                    }
                }
            }
            return ret;
        }


        /// <summary>
        /// This function chooses an appropriate private method for rendering the line
        /// based on the begin and end characteristics. These separate methods could be
        /// combined into a single method but I believe that runtime performance while
        /// enumerating through the points would suffer. 
        /// 
        /// (given the overhead involved with the LINQ calls it may not make much difference)
        /// </summary>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static List<Point> LineList(Point begin, Point end)
        {
            if (Math.Abs(end.Y - begin.Y) < Math.Abs(end.X - begin.X))
            {
                // dX > dY... not steep
                if (end.X >= begin.X)
                {
                    return ListLineOrig(begin, end);
                }
                else
                {
                    return ListLineReverseOrig(begin, end);
                }
            }
            else // steep (dY > dX)
            {
                if (end.Y >= begin.Y)
                {
                    return ListLineSteep(begin, end);
                }
                else
                {
                    return ListLineReverseSteep(begin, end);
                }
            }
        }

        /// <summary>
        /// Creates a line from Begin to End starting at (x0,y0) and ending at (x1,y1)
        /// * where x0 less than x1 and y0 less than y1
        ///   AND line is less steep than it is wide (dx less than dy)
        /// </summary>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        private static List<Point> ListLineOrig(Point begin, Point end)
        {
            List<Point> ret = new List<Point>();
            Point nextPoint = begin;
            int deltax = end.X - begin.X;
            int deltay = end.Y - begin.Y;
            int error = deltax / 2;
            int ystep = 1;
            if (end.Y < begin.Y)
            {
                ystep = -1;
            }
            else if (end.Y == begin.Y)
            {
                ystep = 0;
            }

            while (nextPoint.X < end.X)
            {
                //if (ystep != 0 && nextPoint != begin) yield return new Point(nextPoint.X, nextPoint.Y + ystep);
                if (nextPoint != begin) ret.Add(nextPoint);
                nextPoint.X++;

                error -= deltay;
                if (error < 0)
                {
                    //if (ystep != 0 && nextPoint != begin) yield return nextPoint;
                    if (ystep != 0 && nextPoint.X < end.X) ret.Add(nextPoint);
                    nextPoint.Y += ystep;
                    error += deltax;
                }
                //else if (ystep != 0 && nextPoint != begin) yield return nextPoint;
            }
            return ret;
        }

        /// <summary>
        /// Whenever dy > dx the line is considered steep and we have to change
        /// which variables we increment/decrement
        /// </summary>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        private static List<Point> ListLineSteep(Point begin, Point end)
        {
            List<Point> ret = new List<Point>();
            Point nextPoint = begin;
            int deltax = Math.Abs(end.X - begin.X);
            int deltay = end.Y - begin.Y;
            int error = Math.Abs(deltax / 2);
            int xstep = 1;

            if (end.X < begin.X)
            {
                xstep = -1;
            }
            else if (end.X == begin.X)
            {
                xstep = 0;
            }

            while (nextPoint.Y < end.Y)
            {
                //if (xstep != 0 && nextPoint != begin) yield return new Point(nextPoint.X + xstep, nextPoint.Y);
                if (nextPoint != begin) ret.Add(nextPoint);
                nextPoint.Y++;

                error -= deltax;
                if (error < 0)
                {
                    //if (xstep != 0 && nextPoint != begin) yield return nextPoint;
                    if (xstep != 0 && nextPoint.Y < end.Y) ret.Add(nextPoint);
                    nextPoint.X += xstep;
                    error += deltay;
                }
                //   else if (xstep != 0 && nextPoint != begin) yield return nextPoint;
            }
            return ret;
        }

        /// <summary>
        /// If x0 > x1 then we are going from right to left instead of left to right
        /// so we have to modify our routine slightly
        /// </summary>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        private static List<Point> ListLineReverseOrig(Point begin, Point end)
        {

            List<Point> ret = new List<Point>();
            Point nextPoint = begin;
            int deltax = end.X - begin.X;
            int deltay = end.Y - begin.Y;
            int error = deltax / 2;
            int ystep = 1;

            if (end.Y < begin.Y)
            {
                ystep = -1;
            }
            else if (end.Y == begin.Y)
            {
                ystep = 0;
            }

            while (nextPoint.X > end.X)
            {
                if (nextPoint != begin) ret.Add(nextPoint);
                nextPoint.X--;

                error += deltay;
                if (error < 0)
                {
                    //if (ystep != 0 && nextPoint != begin) yield return nextPoint;
                    if (ystep != 0 && nextPoint.X > end.X) ret.Add(nextPoint);
                    nextPoint.Y += ystep;
                    error -= deltax;
                }
                // else if (ystep != 0 && nextPoint != begin) yield return nextPoint;
            }
            return ret;
        }

        /// <summary>
        /// If x0 > x1 and dy > dx we have to go from right to left and alter the routine
        /// for a steep line
        /// </summary>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        private static List<Point> ListLineReverseSteep(Point begin, Point end)
        {
            List<Point> ret = new List<Point>();
            Point nextPoint = begin;
            int deltax = end.X - begin.X;
            int deltay = end.Y - begin.Y;
            int error = deltax / 2;
            int xstep = 1;

            if (end.X < begin.X)
            {
                xstep = -1;
            }
            else if (end.X == begin.X)
            {
                xstep = 0;
            }

            while (nextPoint.Y > end.Y)
            {
                // if (xstep != 0 && nextPoint != begin) yield return new Point(nextPoint.X + xstep, nextPoint.Y);
                if (nextPoint != begin) ret.Add(nextPoint);
                nextPoint.Y--;

                error += deltax;
                if (error < 0)
                {
                    if (xstep != 0 && nextPoint.Y > end.Y) ret.Add(nextPoint);
                    nextPoint.X += xstep;
                    error -= deltay;
                }
                //else if (xstep != 0 && nextPoint != begin) yield return nextPoint;
            }
            return ret;
        }
        public static IEnumerable<Point> RenderLine(Point begin, Point end)
        {
            if (Math.Abs(end.Y - begin.Y) < Math.Abs(end.X - begin.X))
            {
                // dX > dY... not steep
                if (end.X >= begin.X)
                {
                    return BresLineOrig(begin, end);
                }
                else
                {
                    return BresLineReverseOrig(begin, end);
                }
            }
            else // steep (dY > dX)
            {
                if (end.Y >= begin.Y)
                {
                    return BresLineSteep(begin, end);
                }
                else
                {
                    return BresLineReverseSteep(begin, end);
                }
            }
        }

        /// <summary>
        /// Creates a line from Begin to End starting at (x0,y0) and ending at (x1,y1)
        /// * where x0 less than x1 and y0 less than y1
        ///   AND line is less steep than it is wide (dx less than dy)
        /// </summary>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        private static IEnumerable<Point> BresLineOrig(Point begin, Point end)
        {
            Point nextPoint = begin;
            int deltax = end.X - begin.X;
            int deltay = end.Y - begin.Y;
            int error = deltax / 2;
            int ystep = 1;
            if (end.Y < begin.Y)
            {
                ystep = -1;
            }
            else if (end.Y == begin.Y)
            {
                ystep = 0;
            }

            while (nextPoint.X < end.X)
            {
                //if (ystep != 0 && nextPoint != begin) yield return new Point(nextPoint.X, nextPoint.Y + ystep);
                if (nextPoint != begin) yield return nextPoint;
                nextPoint.X++;

                error -= deltay;
                if (error < 0)
                {
                    //if (ystep != 0 && nextPoint != begin) yield return nextPoint;
                    if (ystep != 0) yield return nextPoint;
                    nextPoint.Y += ystep;
                    error += deltax;
                }
                //else if (ystep != 0 && nextPoint != begin) yield return nextPoint;
            }
        }

        /// <summary>
        /// Whenever dy > dx the line is considered steep and we have to change
        /// which variables we increment/decrement
        /// </summary>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        private static IEnumerable<Point> BresLineSteep(Point begin, Point end)
        {
            Point nextPoint = begin;
            int deltax = Math.Abs(end.X - begin.X);
            int deltay = end.Y - begin.Y;
            int error = Math.Abs(deltax / 2);
            int xstep = 1;

            if (end.X < begin.X)
            {
                xstep = -1;
            }
            else if (end.X == begin.X)
            {
                xstep = 0;
            }

            while (nextPoint.Y < end.Y)
            {
                //if (xstep != 0 && nextPoint != begin) yield return new Point(nextPoint.X + xstep, nextPoint.Y);
                if (nextPoint != begin) yield return nextPoint;
                nextPoint.Y++;

                error -= deltax;
                if (error < 0)
                {
                    //if (xstep != 0 && nextPoint != begin) yield return nextPoint;
                    if (xstep != 0) yield return nextPoint;
                    nextPoint.X += xstep;
                    error += deltay;
                }
                //   else if (xstep != 0 && nextPoint != begin) yield return nextPoint;
            }
        }

        /// <summary>
        /// If x0 > x1 then we are going from right to left instead of left to right
        /// so we have to modify our routine slightly
        /// </summary>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        private static IEnumerable<Point> BresLineReverseOrig(Point begin, Point end)
        {
            Point nextPoint = begin;
            int deltax = end.X - begin.X;
            int deltay = end.Y - begin.Y;
            int error = deltax / 2;
            int ystep = 1;

            if (end.Y < begin.Y)
            {
                ystep = -1;
            }
            else if (end.Y == begin.Y)
            {
                ystep = 0;
            }

            while (nextPoint.X > end.X)
            {
                if (nextPoint != begin) yield return nextPoint;
                nextPoint.X--;

                error += deltay;
                if (error < 0)
                {
                    //if (ystep != 0 && nextPoint != begin) yield return nextPoint;
                    if (ystep != 0) yield return nextPoint;
                    nextPoint.Y += ystep;
                    error -= deltax;
                }
                // else if (ystep != 0 && nextPoint != begin) yield return nextPoint;
            }
        }

        /// <summary>
        /// If x0 > x1 and dy > dx we have to go from right to left and alter the routine
        /// for a steep line
        /// </summary>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        private static IEnumerable<Point> BresLineReverseSteep(Point begin, Point end)
        {
            Point nextPoint = begin;
            int deltax = end.X - begin.X;
            int deltay = end.Y - begin.Y;
            int error = deltax / 2;
            int xstep = 1;

            if (end.X < begin.X)
            {
                xstep = -1;
            }
            else if (end.X == begin.X)
            {
                xstep = 0;
            }

            while (nextPoint.Y > end.Y)
            {
                // if (xstep != 0 && nextPoint != begin) yield return new Point(nextPoint.X + xstep, nextPoint.Y);
                if (nextPoint != begin) yield return nextPoint;
                nextPoint.Y--;

                error += deltax;
                if (error < 0)
                {
                    if (xstep != 0) yield return nextPoint;
                    nextPoint.X += xstep;
                    error -= deltay;
                }
                //else if (xstep != 0 && nextPoint != begin) yield return nextPoint;
            }
        }
    }

    public static class Geometry
    {
        public static Point normalizeCell(Point pt, Level lvl)
        {

            int betterX = pt.X, betterY = pt.Y;

            if (betterY < 0 || betterX < 0 || lvl.map.GetUpperBound(0) < betterY || lvl.map.GetUpperBound(1) < betterX)
            {
                if (betterX < 0) betterX = 0;
                if (betterY < 0) betterY = 0;
                if (betterX > lvl.map.GetUpperBound(1)) betterX = lvl.map.GetUpperBound(1);
                if (betterY > lvl.map.GetUpperBound(0)) betterY = lvl.map.GetUpperBound(0);
                return new Point(betterX, betterY);
            }
            return pt;
        }
    }
}

