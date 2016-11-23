using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// AI控制器（随机走法）
/// </summary>
public class UAIController : UController {

    //List<UChess> GetMyChessList()
    //{
    //    List<UChess> R = new List<UChess>();
    //    for (int i = 0; i < 9; i++)
    //    {
    //        for (int j = 0; j < 10; j++)
    //        {
    //            Point pt = new Point(i, j);
    //            if (UChessboard.Instance[pt] != null && UChessboard.Instance[pt].campType == MyCamp)
    //            {
    //                R.Add(UChessboard.Instance[pt]);
    //            }
    //        }
    //    }
    //    return R;
    //}

    //public override void Update()
    //{
    //    //随机一个棋子，随机一步
    //    List<UChess> MyChessList = GetMyChessList();
    //    int random_index = Random.Range(0, MyChessList.Count);
    //    //优先吃棋子
    //    for (int i = 0; i < MyChessList.Count; i++)
    //    {
    //        int index = (random_index + i) % MyChessList.Count;
    //        List<Point> EatedPos = MyChessList[index].GetEatPoints();
    //        if (EatedPos.Count > 0)
    //        {
    //            Command Cmd = new Command();
    //            Cmd.From = MyChessList[index].ToChessboardPoint();
    //            Cmd.To = UChessboard.Instance.ToChessboardPoint(EatedPos[Random.Range(0, EatedPos.Count)], MyCamp);
    //            Cmd.Camp = MyCamp;
    //            UChessboard.Instance.DoCommand(Cmd);
    //            return;
    //        }
    //    }
    //    //走棋子
    //    for (int i = 0; i < MyChessList.Count; i++)
    //    {
    //        int index = (random_index + i) % MyChessList.Count;
    //        List<Point> AvailablePoints = MyChessList[index].GetAvailablePoints();
    //        if (AvailablePoints.Count > 0)
    //        {
    //            Command Cmd = new Command();
    //            Cmd.From = MyChessList[index].ToChessboardPoint();
    //            Cmd.To = UChessboard.Instance.ToChessboardPoint(AvailablePoints[Random.Range(0, AvailablePoints.Count)], MyCamp);
    //            Cmd.Camp = MyCamp;
    //            UChessboard.Instance.DoCommand(Cmd);
    //            return;
    //        }
    //    }
    //}
}


/// <summary>
/// AI控制器 神经网络算法
/// </summary>
public class UBotAIController : UController
{
    public UNeuronNet_Controller Net;

    //这个AI的适应性分数(开始都是10分)
    public double Fitness = 10.0;   


    public UBotAIController()
    {
        Net = new UNeuronNet_Controller();
        UNeuronNet.ConfigData Config = new UNeuronNet.ConfigData();
        Config.NumInputs = 32 * 3;  //棋子数*3
        Config.NumHiddenLayer = 2; //1层隐藏层
        Config.NumNeuronPerHiddenLayer = 32;    //每层神经元
        Config.NumOutputs = 1;          //1个输出
        Net.Init(Config);
    }

    List<UChess> GetAllChessList()
    {
        List<UChess> R = new List<UChess>();
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                Point pt = new Point(i, j);
                if (Gamer.Chessboard[pt] != null )
                {
                    R.Add(Gamer.Chessboard[pt]);
                }
            }
        }
        return R;
    }

    public override void Update()
    {
        Command Cmd = Net.Update(GetAllChessList().ToArray(),MyCamp);
        if(Cmd != null)
        {
            Gamer.Chessboard.DoCommand(Cmd);

            //增加适应性分数
            if(Cmd.EatedHistory!=null)
            {
                double changed = 0;
                switch(Cmd.EatedHistory.chessType)
                {
                    case EChessType.Bing:
                        changed += 2;
                        break;
                    case EChessType.Ju:
                        changed += 5;
                        break;
                    case EChessType.Ma:
                        changed += 3;
                        break;
                    case EChessType.Pao:
                        changed += 3;
                        break;
                    case EChessType.Shi:
                        changed += 3;
                        break;
                    case EChessType.Xiang:
                        changed += 3;
                        break;
                    case EChessType.Shuai:
                        changed += 100;
                        break;
                    default:
                        changed -= 0.2;
                        break;
                }

                Fitness += changed;

                Fitness = Mathf.Max((float)Fitness, 0);
            }
        }
    }
}