using System.Collections;
using System.Collections.Generic;
using UnrealEngine;

namespace Game
{
    /// <summary>
    /// 阵营
    /// </summary>
    public enum CampType
    {
        Red,
        Black
    }

    /// <summary>
    /// 坐标
    /// </summary>
    public struct Point
    {
        public int x;
        public int y;

        public Point(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        //转换成世界坐标
        public FVector ToWorld(CampType ct)
        {
            return UChessboard.Instance.PosToWorld(this, ct);
        }

        //转为对方阵营的坐标
        public Point Invert()
        {
            return new Point(9 - x, 10 - y);
        }

        //是否是有效的坐标
        public bool IsValid
        {
            get
            {
                return x >= 1 && x <= 9 && y >= 1 && y <= 10;
            }
        }

        //坐标相加
        public static Point operator +(Point a, Point b)
        {
            return new Point(a.x + b.x, a.y + b.y);
        }

        //坐标相减
        public static Point operator -(Point a, Point b)
        {
            return new Point(a.x - b.x, a.y - b.y);
        }
    }


    /// <summary>
    /// 棋盘
    /// </summary>
    public class UChessboard : UnrealEngine.UMonoActorComponent
    {
        public static UChessboard Instance;

        public float size = 0.2f;
        public FVector red_start_point = new FVector(0.77f, 0, -0.87f);



        //当前棋盘的棋子
        public Chess[,] map;

        void OnRegister()
        {
            Instance = this;
        }

        void OnUnRegister()
        {
            Instance = null;
        }

        public FVector PosToWorld(Point pt, CampType ct)
        {
            if (ct == CampType.Red)
            {
                return red_start_point + new FVector(-pt.x * size, 0, pt.y * size);
            }
            else
            {
                FVector redPos = red_start_point + new FVector(-pt.x * size, 0, pt.y * size);
                return new FVector(-redPos.X, redPos.Y, -redPos.Z);
            }
        }


        //创建棋盘
        public void Init()
        {
            map = new Chess[9, 10];

            Chess c = new Chess_Ju();
            c.point = new Point(1, 1);
            AddChess(c);
            c = new Chess_Ju();
            c.point = new Point(9, 1);
            AddChess(c);
        }

        public void AddChess(Chess c)
        {
            map[c.point.x, c.point.y] = c;
        }

        public void RemoveChess(Point pt)
        {
            map[pt.x, pt.y] = null;
        }
    }


    /// <summary>
    /// 棋子基类
    /// </summary>
    public abstract class Chess
    {
        public string name;
        public CampType camp;
        public Point point;

        /// <summary>
        /// 返回可以走动的位置
        /// </summary>
        /// <returns></returns>
        public abstract List<Point> GetAvailablePoints();

    }

    public class Chess_Ju : Chess
    {
        public override List<Point> GetAvailablePoints()
        {
            return null;
        }
    }
}
