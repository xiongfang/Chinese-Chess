using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

public class UGameEngine : MonoBehaviour {
    public UChessboard ChessBoardPrefab;

    //是否启动的时候自动加载上次运算的AI基因组
    public bool LoadWeights;

    //自动保存的时间(秒)
    public float auto_save_time = 30;

    //遗传算法
    UGenAlg Gen;

    List<UChessboard> BoardList;

    public UILabel LabelTun;
    public UIButton BtnStop;
    public UIButton BtnStart;

    bool _start;

    double _auto_save_timer;

    public static UGameEngine Instance;
    public UGameEngine()
    {
        Instance = this;
    }

    // Use this for initialization
    void Start () {
        BoardList = new List<UChessboard>();
        //ChessBoardPrefab.gameObject.SetActive(ChessBoardPrefab.show);
        ChessBoardPrefab.learn = true;
        ChessBoardPrefab.InitStart();

        UGenAlg.ConfigData Config = new UGenAlg.ConfigData();
        Config.PopSize = 5000;
        Config.NumWeights = (ChessBoardPrefab.RedGamer.Controller as UBotAIController).Net.GetWeightCount();
        Gen = new UGenAlg();
        Gen.Init(Config);

        BoardList.Add(ChessBoardPrefab);
        //创建其他棋盘
        for(int i=1;i<Config.PopSize/2;i++)
        {
            UChessboard board = GameObject.Instantiate<GameObject>(ChessBoardPrefab.gameObject).GetComponent<UChessboard>();
            board.learn = true;
            //board.show = false;
            board.gameObject.SetActive(false);
            board.InitStart();
            BoardList.Add(board);
        }

        //更新适应性分数代理
        Gen.onUpdateFitnessScores = UpdateFitnessScores;


        //UI
        UIEventListener.Get(BtnStart.gameObject).onClick = OnClickStart;
        UIEventListener.Get(BtnStop.gameObject).onClick = OnClickStop;


        if(LoadWeights)
        {
            LoadWeightsFromFile();
        }
    }

    void LoadWeightsFromFile()
    {
        Gen.PutWeights(LoadWeightsFromFile_ForUse());
    }

    /// <summary>
    /// 加载保存的AI权重， 公有的给外部使用
    /// </summary>
    /// <returns></returns>
    public static List<double[]> LoadWeightsFromFile_ForUse()
    {
        string FileName = Path.Combine(Application.streamingAssetsPath, "Weights.bin");
        Debug.Log(FileName);

        byte[] data = File.ReadAllBytes(FileName);

        MemoryStream MS = new MemoryStream(data);
        BinaryReader BR = new BinaryReader(MS);

        int Count = BR.ReadInt32();
        int Length = BR.ReadInt32();

        List<double[]> WeightList = new List<double[]>();

        for (int i = 0; i < Count; i++)
        {
            WeightList.Add(new double[Length]);
            for (int j = 0; j < Length; j++)
            {
                double v = BR.ReadDouble();
                WeightList[i][j] = v;
            }
        }

        return WeightList;
    }

    void SaveWeightsToFile()
    {
        string FileName = Path.Combine(Application.streamingAssetsPath, "Weights.bin");
        Debug.Log(System.DateTime.Now.ToString()+FileName);

        int Count = Gen.Config.PopSize;
        List<double[]> WeightList = GetBestWeights(Count);

        MemoryStream MS = new MemoryStream();
        BinaryWriter BW = new BinaryWriter(MS);

        //基因数量
        BW.Write((int)WeightList.Count);
        //权重长度
        BW.Write(WeightList[0].Length);

        for (int i = 0; i < WeightList.Count; i++)
        {
            for (int j = 0; j < WeightList[i].Length; j++)
            {
                BW.Write(WeightList[i][j]);
            }
        }

        //保存当前优秀的AI
        System.IO.File.WriteAllBytes(FileName, MS.ToArray());
    }

    void OnClickStart(GameObject go)
    {
        _start = true;
    }

    void OnClickStop(GameObject go)
    {
        _start = false;
        //保存权重
        SaveWeightsToFile();
    }

    //赋值初始神经网络权重
    void PutWeightsToNet(List<double[]> weights)
    {
        for (int i = 0; i < weights.Count; i++)
        {
            int index = i / 2;
            int red = i % 2;
            if (red == 0)
                (BoardList[index].RedGamer.Controller as UBotAIController).Net.PutWeights(new List<double>(weights[i]));
            else
                (BoardList[index].BlackGamer.Controller as UBotAIController).Net.PutWeights(new List<double>(weights[i]));
        }
    }

    List<double[]> GetBestWeights(int count)
    {
        List<double[]> Gens = new List<double[]>();
        Gen.Genomes.Sort();
        for (int i = 0; i < count; i++)
        {
            Gens.Add(Gen.Genomes[i].Weights);
        }
        return Gens;
    }

    void UpdateFitnessScores()
    {
        double TotleFitness = 0;

        for (int i = 0; i < Gen.Genomes.Count; i++)
        {
            int index = i / 2;
            int red = i % 2;
            if (index >= BoardList.Count)
                Debug.Log(index + " "+BoardList.Count);

            if (red == 0)
                Gen.Genomes[i].Fidness = (BoardList[index].RedGamer.Controller as UBotAIController).Fitness;
            else
                Gen.Genomes[i].Fidness = (BoardList[index].BlackGamer.Controller as UBotAIController).Fitness;

            TotleFitness += Gen.Genomes[i].Fidness;
        }

        Gen.TotleFintnessScore = TotleFitness;
    }

	// Update is called once per frame
	void Update () {

        if(_start)
        {
            try
            {
                _auto_save_timer += RealTime.deltaTime;
                if(_auto_save_timer >= auto_save_time)
                {
                    _auto_save_timer = 0.0;
                    SaveWeightsToFile();
                }

                //迭代一次
                Gen.Epoch();
                PutWeightsToNet(Gen.GetWeights());

                ////下完所有的棋子
                //for (int i = 0; i < BoardList.Count; i++)
                //{
                //    while (!BoardList[i].game_over)
                //        BoardList[i].Step();
                //}

                //下完一步(每边一步)
                for (int i = 0; i < BoardList.Count; i++)
                {
                    BoardList[i].Step();
                    BoardList[i].Step();

                    if (BoardList[i].game_over)
                    {
                        BoardList[i].Restart();
                    }
                }

                //刷新界面
                LabelTun.text = Gen.Generation.ToString();
            }
            catch(Exception E)
            {
                Debug.LogError(E.StackTrace);
                OnClickStop(null);
            }
            
        }
    }
}
