using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AgateDemo
{
    class DungeonMap
    {
        public static int sv = 1176, sh = 1177,
            gr = 1194, da = 1175,
            tl = 1178, tr = 1179, bl = 1180, br = 1181,
            cw = 1182,
            pu = 1183, pd = 1184, pl = 1185, pr = 1186;
        public static int[,] geomorph = 
{
{da, da, da, sv, gr, gr, sv, da, da, da, da, da, da, sv, gr, gr, sv, da, da, da}, 
{da, da, da, sv, gr, gr, sv, da, da, da, da, da, da, sv, gr, gr, sv, da, da, da}, 
{da, da, da, sv, gr, gr, sv, da, da, da, da, da, da, sv, gr, gr, sv, da, da, da}, 
{sh, sh, sh, cw, gr, gr, cw, sh, sh, sh, sh, sh, sh, cw, gr, gr, cw, sh, sh, sh}, 
{gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr}, 
{gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr}, 
{sh, sh, sh, cw, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, cw, sh, sh, sh}, 
{da, da, da, sv, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, sv, da, da, da}, 
{da, da, da, sv, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, sv, da, da, da}, 
{da, da, da, sv, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, sv, da, da, da}, 
{da, da, da, sv, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, sv, da, da, da}, 
{da, da, da, sv, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, sv, da, da, da}, 
{da, da, da, sv, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, sv, da, da, da}, 
{sh, sh, sh, cw, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, cw, sh, sh, sh}, 
{gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr}, 
{gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr}, 
{sh, sh, sh, cw, gr, gr, cw, sh, sh, sh, sh, sh, sh, cw, gr, gr, cw, sh, sh, sh}, 
{da, da, da, sv, gr, gr, sv, da, da, da, da, da, da, sv, gr, gr, sv, da, da, da}, 
{da, da, da, sv, gr, gr, sv, da, da, da, da, da, da, sv, gr, gr, sv, da, da, da}, 
{da, da, da, sv, gr, gr, sv, da, da, da, da, da, da, sv, gr, gr, sv, da, da, da}
//{da, da, da, sv, gr, gr, bl, sh, sh, sh, sh, tr, da, sv, gr, gr, sv, da, da, da}
};
        public static List<int[,]> geomorphs = new List< int[,] >() {
new int[,] {
{da, da, da, sv, gr, gr, sv, da, da, da, da, da, da, sv, gr, gr, sv, da, da, da}, 
{da, da, da, sv, gr, gr, sv, da, da, da, da, da, da, sv, gr, gr, sv, da, da, da}, 
{da, da, da, sv, gr, gr, sv, da, da, da, da, da, da, sv, gr, gr, sv, da, da, da}, 
{sh, sh, sh, cw, gr, gr, cw, sh, sh, sh, sh, sh, sh, cw, gr, gr, cw, sh, sh, sh}, 
{gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr}, 
{gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr}, 
{sh, sh, sh, cw, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, cw, sh, sh, sh}, 
{da, da, da, sv, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, sv, da, da, da}, 
{da, da, da, sv, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, sv, da, da, da}, 
{da, da, da, sv, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, sv, da, da, da}, 
{da, da, da, sv, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, sv, da, da, da}, 
{da, da, da, sv, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, sv, da, da, da}, 
{da, da, da, sv, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, sv, da, da, da}, 
{sh, sh, sh, cw, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, cw, sh, sh, sh}, 
{gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr}, 
{gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr}, 
{sh, sh, sh, cw, gr, gr, cw, sh, sh, sh, sh, sh, sh, cw, gr, gr, cw, sh, sh, sh}, 
{da, da, da, sv, gr, gr, sv, da, da, da, da, da, da, sv, gr, gr, sv, da, da, da}, 
{da, da, da, sv, gr, gr, sv, da, da, da, da, da, da, sv, gr, gr, sv, da, da, da}, 
{da, da, da, sv, gr, gr, sv, da, da, da, da, da, da, sv, gr, gr, sv, da, da, da}
},
new int[,] {
{da, da, da, cw, gr, gr, cw, da, da, da, da, da, da, cw, gr, gr, cw, da, da, da}, 
{da, da, da, cw, gr, gr, cw, da, da, da, da, da, da, cw, gr, gr, cw, da, da, da}, 
{da, da, da, cw, gr, gr, cw, da, da, da, da, da, da, cw, gr, gr, cw, da, da, da}, 
{cw, cw, cw, cw, gr, gr, cw, cw, cw, cw, cw, cw, cw, cw, gr, gr, cw, cw, cw, cw}, 
{gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr}, 
{gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr}, 
{cw, cw, cw, cw, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, cw, cw, cw, cw}, 
{da, da, da, cw, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, cw, da, da, da}, 
{da, da, da, cw, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, cw, da, da, da}, 
{da, da, da, cw, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, cw, da, da, da}, 
{da, da, da, cw, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, cw, da, da, da}, 
{da, da, da, cw, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, cw, da, da, da}, 
{da, da, da, cw, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, cw, da, da, da}, 
{cw, cw, cw, cw, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, cw, cw, cw, cw}, 
{gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr}, 
{gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr}, 
{cw, cw, cw, gr, gr, gr, cw, cw, cw, cw, cw, cw, cw, cw, gr, gr, cw, cw, cw, cw}, 
{da, da, cw, cw, gr, gr, cw, da, da, da, da, da, da, cw, gr, gr, cw, da, da, da}, 
{da, da, da, cw, gr, gr, cw, da, da, da, da, da, da, cw, gr, gr, cw, da, da, da}, 
{da, da, da, cw, gr, gr, cw, da, da, da, da, da, da, cw, gr, gr, cw, da, da, da}
},
new int[,] {
{da, da, da, cw, gr, gr, cw, da, da, da, da, da, da, cw, gr, gr, cw, da, da, da}, 
{da, da, da, cw, gr, gr, cw, da, da, da, da, da, da, cw, gr, gr, cw, da, da, da}, 
{da, da, cw, cw, gr, gr, cw, da, da, da, da, da, da, cw, gr, gr, cw, da, da, da}, 
{cw, cw, cw, gr, gr, gr, cw, cw, cw, cw, cw, cw, da, cw, gr, gr, cw, cw, cw, cw}, 
{gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, cw, da, cw, gr, gr, gr, gr, gr, gr}, 
{gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, cw, da, cw, gr, gr, gr, gr, gr, gr}, 
{cw, cw, cw, cw, gr, gr, cw, cw, cw, gr, gr, cw, da, cw, gr, gr, cw, cw, cw, cw}, 
{da, da, da, cw, gr, gr, cw, da, cw, gr, gr, cw, da, cw, gr, gr, cw, da, da, da}, 
{da, da, da, cw, gr, gr, cw, da, cw, gr, gr, cw, da, cw, gr, gr, cw, da, da, da}, 
{da, da, da, cw, gr, gr, cw, da, cw, gr, gr, cw, da, cw, gr, gr, cw, da, da, da}, 
{da, da, da, cw, gr, gr, cw, da, cw, gr, gr, cw, da, cw, gr, gr, cw, da, da, da}, 
{da, da, da, cw, gr, gr, cw, da, cw, gr, gr, cw, da, cw, gr, gr, cw, da, da, da}, 
{da, da, da, cw, gr, gr, cw, da, cw, gr, gr, cw, da, cw, gr, gr, cw, da, da, da}, 
{cw, cw, cw, cw, gr, gr, cw, da, cw, gr, gr, cw, cw, cw, gr, gr, cw, cw, cw, cw}, 
{gr, gr, gr, gr, gr, gr, cw, da, cw, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr}, 
{gr, gr, gr, gr, gr, gr, cw, da, cw, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr, gr}, 
{cw, cw, cw, gr, gr, gr, cw, da, cw, cw, cw, cw, cw, cw, gr, gr, cw, cw, cw, cw}, 
{da, da, cw, cw, gr, gr, cw, da, da, da, da, da, da, cw, gr, gr, cw, da, da, da}, 
{da, da, da, cw, gr, gr, cw, da, da, da, da, da, da, cw, gr, gr, cw, da, da, da}, 
{da, da, da, cw, gr, gr, cw, da, da, da, da, da, da, cw, gr, gr, cw, da, da, da} 
}
};
        public static int[,] cleanUp(int[,] geo)
        {
            int[,] geolocal = (int[,])geo.Clone();

            for (int i = 0; i < geolocal.GetLength(0); i++)
            {
                for (int j = 0; j < geolocal.GetLength(1); j++)
                {
                    if ((i == 0 || i == geolocal.GetUpperBound(0) ||
                        j == 0 || j == geolocal.GetUpperBound(1)) && geolocal[i, j] == gr)
                    {
                        geolocal[i, j] = cw;
                    }
                }
            }
            for (int i = 0; i < geolocal.GetLength(0); i++)
            {
                for (int j = 0; j < geolocal.GetLength(1); j++)
                {
                    if (geolocal[i, j] == cw)
                    {
                        bool left = false, right = false, top = false, bottom = false;
                        if (i > 0)
                            top = (geolocal[i - 1, j] != da) && (geolocal[i - 1, j] != gr);
                        if (j > 0)
                            left = (geolocal[i, j - 1] != da) && (geolocal[i, j - 1] != gr);
                        if (i < geolocal.GetLength(0) - 1)
                            bottom = (geolocal[i + 1, j] != da) && (geolocal[i + 1, j] != gr);
                        if (j < geolocal.GetLength(1) - 1)
                            right = (geolocal[i, j + 1] != da) && (geolocal[i, j + 1] != gr);

                        if (left)
                        {
                            if (right)
                            {
                                if (top)
                                {
                                    if (!bottom)
                                    {
                                        geolocal[i, j] = pu;
                                    }
                                }
                                else if (bottom)
                                {
                                    geolocal[i, j] = pd;
                                }
                                else
                                {
                                    geolocal[i, j] = sh;
                                }
                            }
                            else if (top)
                            {
                                if (bottom)
                                {
                                    geolocal[i, j] = pl;
                                }
                                else
                                {
                                    geolocal[i, j] = br;
                                }
                            }
                            else if (bottom)
                            {
                                geolocal[i, j] = tr;
                            }
                            else
                            {
                                geolocal[i, j] = sh;
                            }

                        }
                        else if (right)
                        {
                            if (top)
                            {
                                if (bottom)
                                {
                                    geolocal[i, j] = pr;
                                }
                                else
                                {
                                    geolocal[i, j] = bl;
                                }
                            }
                            else if (bottom)
                            {
                                geolocal[i, j] = tl;
                            }
                            else
                            {
                                geolocal[i, j] = sh;
                            }
                        }
                        else if (top || bottom)
                        {
                            geolocal[i, j] = sv;
                        }
                            
                    }
                }
            }
            return geolocal;
        }
        public static int[,] rotateCW(int[,] geo)
        {
            int[,] geolocal = (int[,])geo.Clone();

            for (int i = geo.GetUpperBound(0); i >= 0 ; --i)
            {
                for (int j = 0; j < geo.GetLength(1); ++j)
                    geolocal[j, geo.GetUpperBound(0) - i] = geo[i, j];
            }

            return geolocal;
        }
        public static int[,] merge(int[,] fst, int[,] snd, bool mergeYaxis)
        {
            if (mergeYaxis)
            {
                if (fst.GetUpperBound(1) != snd.GetUpperBound(1))
                {
                    return fst;
                }
                else
                {
                    int[,] merged = new int[fst.GetLength(0) + snd.GetLength(0), fst.GetLength(1)];

                    for (int i = 0; i < fst.GetLength(0); i++)
                    {
                        for (int j = 0; j < fst.GetLength(1); j++)
                            merged[i, j] = fst[i, j];
                    }
                    for (int i = 0; i < snd.GetLength(0); i++)
                    {
                        for (int j = 0; j < snd.GetLength(1); j++)
                            merged[fst.GetLength(0) + i, j] = snd[i, j];
                    }
                    return merged;
                }
            }
            else
            {
                if (fst.GetLength(0) != snd.GetLength(0))
                {
                    return fst;
                }
                else
                {
                    int[,] merged = new int[fst.GetLength(0), fst.GetLength(1) + snd.GetLength(1)];

                    for (int i = 0; i < fst.GetLength(0); i++)
                    {
                        for (int j = 0; j < fst.GetLength(1); j++)
                        {
                            merged[i, j] = fst[i, j];
                        }
                        for (int j = 0; j < snd.GetLength(1); j++)
                        {
                            merged[i, fst.GetLength(1) + j] = snd[i, j];
                        }
                    }
                    return merged;
                }
            }
        }
    }
}
