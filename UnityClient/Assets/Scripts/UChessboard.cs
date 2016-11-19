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

    public override bool Equals(object obj)
    {
        Point B = (Point)obj;
        return x == B.x && y == B.y;
    }

    //保证相等的Point返回相同的HashCode
    public override int GetHashCode()
    {
        return x.GetHashCode() ^ y.GetHashCode();
    }

    public static bool operator==(Point A,Point B)
    {
        return A.Equals(B);
    }

    public static bool operator !=(Point A, Point B)
    {
        return !(A == B);
    }

    
}


//下棋命令
public class Command
{
    public ECampType Camp;
    public Point From;
    public Point To;

    public UChess EatedHistory;
}

/// <summary>
/// 棋盘
/// </summary>
public class UChessboard : MonoBehaviour
{
    public static UChessboard Instance;

    public float size = 0.2f;
    public Vector3 red_start_point = new Vector3(-0.77f, 0, -0.87f);

    //红方相机
    public GameObject CameraRed;
    //黑方相机
    public GameObject CameraBlack;
    //可走位置标记
    public GameObject MarkPrefab;

    //当前棋盘的棋子
    public UChess[,] map;

    //初始棋谱
    public ChessData[] StartChessboard;

    //玩家视图类型
    UGamer RedGamer;    //红方
    UGamer BlackGamer;  //黑方
    float game_start_timer;    //游戏开始计时

    //当前正在下棋的阵营
    ECampType NowPlayer;

    //棋子对象工厂
    Dictionary<EChessType, Type> ChessTypeFactory;

    List<Command> History;

    bool game_over;
    ECampType Winner;


    void Awake()
    {
        Instance = this;
        History = new List<Command>();
        ChessTypeFactory = new Dictionary<EChessType, Type>();
        ChessTypeFactory[EChessType.Ju] = typeof(UChess_Ju);
        ChessTypeFactory[EChessType.Ma] = typeof(UChess_Ma);
        ChessTypeFactory[EChessType.Xiang] = typeof(UChess_Xiang);
        ChessTypeFactory[EChessType.Shi] = typeof(UChess_Shi);
        ChessTypeFactory[EChessType.Shuai] = typeof(UChess_Shuai);
        ChessTypeFactory[EChessType.Pao] = typeof(UChess_Pao);
        ChessTypeFactory[EChessType.Bing] = typeof(UChess_Bing);
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
        map = new UChess[9, 10];

        for(int i=0;i<StartChessboard.Length;i++)
        {
            UChess chess = System.Activator.CreateInstance(ChessTypeFactory[StartChessboard[i].chessType]) as UChess;
            chess.InitData(StartChessboard[i]);
            AddChess(chess);
        }

        //创建玩家和AI

        RedGamer = new UPlayer();
        RedGamer.name = "小明";
        RedGamer.Camp = ECampType.Red;
        RedGamer.Attach(new ULocalPlayerController());

        BlackGamer = new UBot();
        BlackGamer.name = "魔王";
        BlackGamer.Camp = ECampType.Black;
        BlackGamer.Attach(new UAIController());

        //默认为红方视角
        SetPlayerView(ECampType.Red);

        //红方起手
        SetNowTun(ECampType.Red);
    }

    void Update()
    {
        //根据当前的下棋方，更新控制器
        if(!game_over)
        {
            if (NowPlayer == ECampType.Red)
                RedGamer.Controller.Update();
            else
                BlackGamer.Controller.Update();

            //悔一步，倒退两步
            if(Input.GetKeyDown(KeyCode.R))
            {
                Undo();
                Undo();
            }
        }
        else
        {
            //任意按键重新开始
            if(Input.anyKeyDown)
            {
                Restart();
            }
        }
    }

    public bool IsValidChessboardPoint(Point pt)
    {
        return pt.x >= 0 && pt.x <= 8 && pt.y >= 0 && pt.y <= 9;
    }

    /// <summary>
    /// 重载索引器
    /// </summary>
    /// <param name="pt">棋盘坐标</param>
    /// <returns>指定坐标的棋子</returns>
    public UChess this[Point pt]
    {
        get
        {
            if(IsValidChessboardPoint(pt))
            {
                return map[pt.x, pt.y];
            }
            else
            {
                return null;
            }
        }
        set
        {
            if (IsValidChessboardPoint(pt))
                map[pt.x, pt.y] = value;
            else
                Debug.LogErrorFormat("error chessboard point x:{0},y:{1}", pt.x, pt.y);
        }
    }

    //添加一个棋子到棋盘上（如果指定坐标已经有棋子了，吃掉棋子）
    public void AddChess(UChess c)
    {
        if(this[c.ToChessboardPoint()] !=null)
        {
            RemoveChess(this[c.ToChessboardPoint()]);
        }
        this[c.ToChessboardPoint()] = c;
        c.onAdded();
    }

    //从棋盘移除一个棋子
    public void RemoveChess(UChess c)
    {
        this[c.ToChessboardPoint()] = null;
        c.onRemoved();
    }

    //防止棋盘坐标越界
    Point ClampChessboardPoint(Point pt)
    {
        return new Point(Mathf.Clamp(pt.x, 0, 8), Mathf.Clamp(pt.y, 0, 9));
    }

    ////将棋子移动到新坐标
    //public void MoveChess(UChess chess,Point newChessboardPos)
    //{
    //    newChessboardPos = ClampChessboardPoint(newChessboardPos);
    //    //记录老坐标
    //    Point lastChessPoint = chess.point;
    //    //计算新坐标
    //    Point newChessPoint = ToChessPoint(newChessboardPos, chess.campType);

    //    //吃掉新坐标的棋子
    //    if(this[newChessboardPos]!=null && this[newChessboardPos] != chess)
    //    {
    //        RemoveChess(this[newChessboardPos]);
    //    }

    //    //将棋子从原始位置移除
    //    this[chess.ToChessboardPoint()] = null;

    //    //设置新坐标
    //    chess.point = newChessPoint;
    //    //移动位置
    //    this[chess.ToChessboardPoint()] = chess;
    //    //事件通知
    //    chess.onMoved(lastChessPoint, newChessPoint);
    //}

    public void DoCommand(Command Cmd)
    {
        UChess chess = this[Cmd.From];

        //记录老坐标
        Point lastChessPoint = chess.point;
        //计算新坐标
        Point newChessPoint = ToChessPoint(Cmd.To, chess.campType);

        //吃掉新坐标的棋子
        if (this[Cmd.To] != null && this[Cmd.To] != chess)
        {
            Cmd.EatedHistory = this[Cmd.To];
            RemoveChess(this[Cmd.To]);

            //胜负判定
            if(!game_over)
            {
                if(Judge(Cmd.EatedHistory))
                {
                    game_over = true;
                }
            }
        }

        //将棋子从原始位置移除
        this[chess.ToChessboardPoint()] = null;

        //设置新坐标
        chess.point = newChessPoint;
        //移动位置
        this[chess.ToChessboardPoint()] = chess;
        //事件通知
        chess.ResetPosition();

        //切换玩家
        ToggleTun();

        //记录
        History.Add(Cmd);

        
    }
    public void UndoCommand(Command Cmd)
    {
        UChess chess = this[Cmd.To];

        //记录老坐标
        Point lastChessPoint = chess.point;
        //计算新坐标
        Point newChessPoint = ToChessPoint(Cmd.From, chess.campType);

        //将棋子从原始位置移除
        this[chess.ToChessboardPoint()] = null;
        //设置新坐标
        chess.point = newChessPoint;
        //移动位置
        this[chess.ToChessboardPoint()] = chess;
        //坐标还原
        chess.ResetPosition();

        //如果吃掉了棋子，还原棋子
        if (Cmd.EatedHistory!=null)
        {
            AddChess(Cmd.EatedHistory);
            Cmd.EatedHistory = null;
        }

        //切换回合
        ToggleTun();

        History.RemoveAt(History.Count - 1);

    }


    //悔一步棋
    public bool Undo()
    {
        if (History.Count > 0)
        {
            UndoCommand(History[History.Count - 1]);
            return true;
        }

        return false;
    }

    //胜负判定
    bool Judge(UChess EatedChess)
    {
        if(EatedChess.chessType == EChessType.Shuai)
        {
            Winner = EatedChess.campType == ECampType.Red? ECampType.Black:ECampType.Red;
            return true;
        }
        return false;
    }

    //重新开始
    void Restart()
    {
        game_over = false;
        History.Clear();
        for(int i=0;i<9;i++)
        {
            for(int j=0;j<10;j++)
            {
                if(map[i,j]!=null)
                {
                    RemoveChess(map[i, j]);
                }
            }
        }
        Init();
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

    //设置当前谁的回合
    void SetNowTun(ECampType NowCamp)
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

    //切换回合
    void ToggleTun()
    {
        if(NowPlayer == ECampType.Red)
        {
            SetNowTun(ECampType.Black);
        }
        else
        {
            SetNowTun(ECampType.Red);
        }
    }



    void OnGUI()
    {
        RedGamer.Controller.DebugDraw();
        BlackGamer.Controller.DebugDraw();
        GUILayout.Label("NowTun " + NowPlayer);
    }
}

