using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using ICSharpCode.SharpZipLib;
using ICSharpCode.SharpZipLib.GZip;

public class UGameEngine : MonoBehaviour {
    public UChessboard ChessBoardPrefab;

    //是否启动的时候自动加载上次运算的AI基因组
    public bool LoadWeights;

    //是否显示第一对AI的训练过程
    public bool showFirstChessboard;

    //自动保存的时间(秒)
    public float auto_save_time = 30;

    //每一代走的步数
    public int step_count_per_gen = 5;

    //遗传算法
    UGenAlg Gen;

    List<UChessboard> BoardList;

    public UILabel LabelTime;
    public UILabel LabelTun;
    public UIButton BtnStop;
    public UIButton BtnStart;

    bool _start;

    double _auto_save_timer;

    double _learn_timer;

    public UGameEngine()
    {
    }

    public static byte[] Compress(byte[] inputBytes)
    {
        MemoryStream ms = new MemoryStream();
        GZipOutputStream gzip = new GZipOutputStream(ms);
        gzip.Write(inputBytes, 0, inputBytes.Length);
        gzip.Close();
        return ms.ToArray();
    }

    /// <summary>
    /// 解压缩字节数组
    /// </summary>
    /// <param name="str"></param>
    public static byte[] Decompress(byte[] inputBytes)
    {
        GZipInputStream gzi = new GZipInputStream(new MemoryStream(inputBytes));

        MemoryStream re = new MemoryStream();
        int count = 0;
        byte[] data = new byte[4096];
        while ((count = gzi.Read(data, 0, data.Length)) != 0)
        {
            re.Write(data, 0, count);
        }
        return re.ToArray();
    }

    // Use this for initialization
    void Start () {
        //UI
        UIEventListener.Get(BtnStart.gameObject).onClick = OnClickStart;
        UIEventListener.Get(BtnStop.gameObject).onClick = OnClickStop;


        if(LoadWeights)
        {
            try
            {
                LoadGenFromFile();
            }
            catch(Exception E)
            {
                Debug.Log("Load Gen Failed "+E.Message);
                LoadWeights = false;
            }
        }
        
        if(!LoadWeights)
        {
            UGenAlg.ConfigData Config = new UGenAlg.ConfigData();
            Config.PopSize = 3000;
            Config.NumWeights = new UBotAIController().Net.GetWeightCount();
            Gen = new UGenAlg();
            Gen.Init(Config);
        }

        //更新适应性分数代理
        Gen.onUpdateFitnessScores = UpdateFitnessScores;

        BoardList = new List<UChessboard>();
        ChessBoardPrefab.learn = true;
        ChessBoardPrefab.gameObject.SetActive(showFirstChessboard);
        ChessBoardPrefab.InitStart();

        BoardList.Add(ChessBoardPrefab);
        //创建其他棋盘
        for (int i = 1; i < Gen.Config.PopSize / 2; i++)
        {
            UChessboard board = GameObject.Instantiate<GameObject>(ChessBoardPrefab.gameObject).GetComponent<UChessboard>();
            board.learn = true;
            //board.show = false;
            board.gameObject.SetActive(false);
            board.InitStart();
            BoardList.Add(board);
        }

        //将加载的适应性分数保存
        if(LoadWeights)
            PutFitnessScores();
    }

    void LoadGenFromFile()
    {
        string FileName = Path.Combine(Application.streamingAssetsPath, "Gen.txt");
        Debug.Log("加载基因:"+FileName);

        string jsonData = System.Text.Encoding.UTF8.GetString(File.ReadAllBytes(FileName));
        Gen = JsonUtility.FromJson<UGenAlg>(jsonData);
    }

    /// <summary>
    /// 加载保存的AI权重， 公有的给外部使用
    /// </summary>
    /// <returns></returns>
    public static List<double[]> LoadBestWeightsFromFileForUse()
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

    void SaveToFile()
    {
        //保存基因
        {
            string FileName = Path.Combine(Application.streamingAssetsPath, "Gen.txt");
            Debug.Log(System.DateTime.Now.ToString() + FileName);

            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(UnityEngine.JsonUtility.ToJson(Gen));

            //保存当前优秀的AI
            System.IO.File.WriteAllBytes(FileName, bytes);
        }

        //保存最好的AI的权重
        {
            string FileName = Path.Combine(Application.streamingAssetsPath, "Weights.bin");
            Debug.Log(System.DateTime.Now.ToString() + FileName);

            int Count = 5; //5个AI差不多了
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
    }

    void OnClickStart(GameObject go)
    {
        _start = true;
    }

    void OnClickStop(GameObject go)
    {
        _start = false;
        //保存权重
        SaveToFile();
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
        //先更新积分，否则排序出问题
        UpdateFitnessScores();
        //克隆出新的副本列表来排序，否则出问题
        List<UGenome> CloneGenomeList = new List<UGenome>();
        CloneGenomeList.AddRange(Gen.Genomes);
        CloneGenomeList.Sort();

        List<double[]> Gens = new List<double[]>();
        for (int i = 0; i < count; i++)
        {
            Gens.Add(Gen.Genomes[i].Weights);
        }
        return Gens;
    }

    void UpdateFitnessScores()
    {
        double TotleFitness = 0;

        for (int i = 0; i < Gen.Genomes.Length; i++)
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

    void PutFitnessScores()
    {
        for (int i = 0; i < Gen.Genomes.Length; i++)
        {
            int index = i / 2;
            int red = i % 2;

            if (red == 0)
                (BoardList[index].RedGamer.Controller as UBotAIController).Fitness = Gen.Genomes[i].Fidness;
            else
                (BoardList[index].BlackGamer.Controller as UBotAIController).Fitness = Gen.Genomes[i].Fidness;
        }
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
                    SaveToFile();
                }

                _learn_timer += RealTime.deltaTime;

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
                for(int step=0; step < step_count_per_gen; step++)
                {
                    for (int i = 0; i < BoardList.Count; i++)
                    {
                        BoardList[i].Step();
                        BoardList[i].Step();

                        if (BoardList[i].game_over)
                        {
                            BoardList[i].Restart();
                        }
                    }
                }

                //刷新界面
                LabelTun.text =string.Format("产生{0}代",Gen.Generation.ToString());
                LabelTime.text = string.Format("时间{0}:{1}:{2}", (int)(_learn_timer/3600), ((int)_learn_timer%3600)/60, (int)_learn_timer%60);
                
            }
            catch(Exception E)
            {
                Debug.LogError(E.Message);
                Debug.LogError(E.StackTrace);
                OnClickStop(null);
            }
            
        }
    }
}
