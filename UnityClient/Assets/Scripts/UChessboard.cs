using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 阵营
/// </summary>
public enum ECampType
{
    Red,
    Black
}

/// <summary>
/// 坐标
/// </summary>
[Serializable] 
public struct Point
{
    public int x;
    public int y;

    public Point(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    //转为对方阵营的坐标
    public Point Invert()
    {
        return new Point(10 - x, 11 - y);
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
public class UChessboard : MonoBehaviour
{
    public static UChessboard Instance;

    public float size = 0.2f;
    public Vector3 red_start_point = new Vector3(-0.77f, 0, -0.87f);

    public GameObject CameraRed;
    public GameObject CameraBlack;

    //当前棋盘的棋子
    public Chess[,] map;

    //初始棋谱
    public ChessData[] StartChessboard;

    //玩家视图类型
    UGamer RedGamer;    //红方
    UGamer BlackGamer;  //黑方
    float game_start_timer;    //游戏开始计时

    //当前正在下棋的阵营
    ECampType NowPlayer;

    void Awake()
    {
        Instance = this;
    }

    void OnDestroy()
    {
        Instance = null;
    }

    void Start()
    {
        Init();
    }
    /// <summary>
    /// 将棋子坐标转为棋盘坐标（以红方为准，从左到右 0-8，从下到上0-9）
    /// </summary>
    /// <param name="pt"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public Point ToChessboardPoint(Point pt, ECampType ct)
    {
        if (ct == ECampType.Red)
        {
            return new Point(9 - pt.x, pt.y - 1);
        }
        else
        {
            return ToChessboardPoint(pt.Invert(), ECampType.Red);
        }
    }

    /// <summary>
    /// 将棋盘坐标转化为棋子坐标
    /// </summary>
    /// <param name="chessboardPoint"></param>
    /// <returns></returns>
    public Point ToChessPoint(Point pt, ECampType ct)
    {
        if (ct == ECampType.Red)
        {
            return new Point(9 - pt.x, pt.y + 1);
        }
        else
        {
            return new Point(9 - pt.x, pt.y + 1).Invert();
        }
    }

    /// <summary>
    /// 将棋盘坐标转化为三维世界坐标
    /// </summary>
    /// <param name="pt"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public Vector3 PosToWorld(Point pt)
    {
        return red_start_point + new Vector3(pt.x * size, 0, pt.y * size);
    }

    public Point WorldToPos(Vector3 world)
    {
        Vector3 localPos = world - red_start_point;
        return new Point((int)((localPos.x+size/2)/size),(int)((localPos.z + size / 2 )/ size));
    }

    //创建棋盘
    public void Init()
    {
        //初始化棋盘
        map = new Chess[9, 10];

        for(int i=0;i<StartChessboard.Length;i++)
        {
            AddChess(new Chess_Ju(StartChessboard[i]));
        }

        //创建玩家和AI

        RedGamer = new UPlayer();
        RedGamer.name = "小明";
        RedGamer.Camp = ECampType.Red;
        RedGamer.Controller = new ULocalPlayerController();

        BlackGamer = new UBot();
        BlackGamer.name = "魔王";
        BlackGamer.Camp = ECampType.Black;
        BlackGamer.Controller = new UAIController();

        //默认为红方视角
        SetPlayerView(ECampType.Red);

        //红方起手
        SetNowGamer(ECampType.Red);
    }

    void Update()
    {
        //根据当前的下棋方，更新控制器
        if(NowPlayer == ECampType.Red)
            RedGamer.Controller.Update();
        else
            BlackGamer.Controller.Update();
    }

    /// <summary>
    /// 重载索引器
    /// </summary>
    /// <param name="pt">棋盘坐标</param>
    /// <returns>指定坐标的棋子</returns>
    public Chess this[Point pt]
    {
        get
        {
            if(pt.x>=0 && pt.x<=8 && pt.y>=0 && pt.y<=9)
            {
                return map[pt.x, pt.y];
            }
            else
            {
                Debug.LogErrorFormat("error Point {0},{1}", pt.x, pt.y);
                return null;
            }
        }
        set
        {
            map[pt.x, pt.y] = value;
        }
    }

    //添加一个棋子到棋盘上（如果指定坐标已经有棋子了，吃掉棋子）
    public void AddChess(Chess c)
    {
        if(this[c.ToChessboardPoint()] !=null)
        {
            RemoveChess(this[c.ToChessboardPoint()]);
        }
        this[c.ToChessboardPoint()] = c;
        c.onAdded();
    }

    //从棋盘移除一个棋子
    public void RemoveChess(Chess c)
    {
        this[c.ToChessboardPoint()] = null;
        c.onRemoved();
    }

    //防止棋盘坐标越界
    Point ClampChessboardPoint(Point pt)
    {
        return new Point(Mathf.Clamp(pt.x, 0, 8), Mathf.Clamp(pt.y, 0, 9));
    }

    //将棋子移动到新坐标
    public void MoveChess(Chess chess,Point newChessboardPos)
    {
        newChessboardPos = ClampChessboardPoint(newChessboardPos);
        //记录老坐标
        Point lastChessPoint = chess.point;
        //计算新坐标
        Point newChessPoint = ToChessPoint(newChessboardPos, chess.campType);

        //吃掉新坐标的棋子
        if(this[newChessboardPos]!=null && this[newChessboardPos] != chess)
        {
            RemoveChess(this[newChessboardPos]);
        }

        //设置新坐标
        chess.point = newChessPoint;
        //移动位置
        this[chess.ToChessboardPoint()] = chess;
        //事件通知
        chess.onMoved(lastChessPoint, newChessPoint);
    }

    public void SetPlayerView(ECampType ct)
    {
        CameraRed.SetActive(ct == ECampType.Red);
        CameraBlack.SetActive(ct == ECampType.Black);
    }

    //返回当前的观察相机
    public Camera GetViewCamera()
    {
        if (CameraRed.activeSelf)
            return CameraRed.GetComponent<Camera>();
        else
            return CameraBlack.GetComponent<Camera>();
    }

    public void SetNowGamer(ECampType NowCamp)
    {
        NowPlayer = NowCamp;
        if (NowPlayer == ECampType.Red)
        {
            BlackGamer.Controller.TunEnd();
            RedGamer.Controller.TunStart();
        }
        else
        {
            RedGamer.Controller.TunEnd();
            BlackGamer.Controller.TunStart();
        }
    }

    void OnGUI()
    {
        RedGamer.Controller.DebugDraw();
        BlackGamer.Controller.DebugDraw();
    }
}

[Serializable]
public struct ChessData
{
    //棋子名称
    public string name;
    //棋子所属阵营(红或蓝)
    public ECampType campType;
    //棋子坐标（以本方阵营为准，从右到左X轴1到9，从下到上Y轴1到10）
    public Point point;

    //预制体对象索引
    public GameObject prefab;
}

/// <summary>
/// 棋子基类
/// </summary>
public abstract class Chess
{
    //棋子名称
    public string name;
    //棋子所属阵营(红或蓝)
    public ECampType campType;
    //棋子坐标（以本方阵营为准，从右到左X轴1到9，从下到上Y轴1到10）
    public Point point;

    //预制体对象索引
    protected GameObject prefab;
    //实例化对象
    public GameObject gameObject;

    public Chess(ChessData data)
    {
        this.name = data.name;
        this.campType = data.campType;
        this.point = data.point;
        this.prefab = data.prefab;
    }

    /// <summary>
    /// 返回可以走动的位置
    /// </summary>
    /// <returns></returns>
    public abstract List<Point> GetAvailablePoints();

    /// <summary>
    /// 返回棋盘坐标 （以红方为准，从左到右 0-8，从下到上0-9）
    /// </summary>
    /// <returns></returns>
    public Point ToChessboardPoint()
    {
        return UChessboard.Instance.ToChessboardPoint(point, campType);
    }

    /// <summary>
    /// 返回棋子的三维世界坐标
    /// </summary>
    /// <returns></returns>
    public Vector3 GetWorldPosstion()
    {
        return UChessboard.Instance.PosToWorld(ToChessboardPoint());
    }



    public void onAdded()
    {
        //创建棋子
        gameObject = GameObject.Instantiate<GameObject>(prefab);
        //坐标
        gameObject.transform.position = GetWorldPosstion();
        if(campType == ECampType.Red)
        {
            Vector3 Eurler = gameObject.transform.localEulerAngles;
            Eurler.y = 180;
            gameObject.transform.localEulerAngles = Eurler;
        }
    }
    public void onRemoved()
    {
        GameObject.Destroy(gameObject);
    }

    public void onMoved(Point lastPos, Point newPos)
    {
        gameObject.transform.position = GetWorldPosstion();
    }
}

public class Chess_Ju : Chess
{

    public Chess_Ju(ChessData data) :base(data)
    {
            
    }

    public override List<Point> GetAvailablePoints()
    {
        return null;
    }
}
