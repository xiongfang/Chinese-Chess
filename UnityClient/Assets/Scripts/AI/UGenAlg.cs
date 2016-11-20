using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UGenAlg
{
    List<UGenome> Genomes;

    public class ConfigData
    {
        //群体大小
        public int PopSize = 1024;
        //每个基因的权重数量
        public int NumWeights = 100;
        //突变率
        public float MutationRate = 0.1f;
        //最大突变值
        public double MaxPerturbatlon = 0.2f;
        //杂交率
        public float CrossoverRate = 0.7f;  
    }

    ConfigData Config;
    //当前代数
    int Generation;
    //总适应性分数（轮盘选择需要）
    double TotleFintnessScore;

    public UGenAlg()
    {
        Generation = 0;
        TotleFintnessScore = 0.0f;
        Config = new ConfigData();
        Genomes = new List<UGenome>();
    }

    /// <summary>
    /// 选择最好基因用来产生后代 (轮盘选择)
    /// </summary>
    /// <returns></returns>
    public UGenome Selection()
    {
        double fSlice = Random.Range(0, (float)TotleFintnessScore);
        double fTotle = 0;
        for(int i=0;i<Genomes.Count;i++)
        {
            fTotle += Genomes[i].Fidness;
            if(fTotle>=fSlice)
            {
                return Genomes[i];
            }
        }
        return null;
    }

    //突变
    public void Mutate(double[] Weights)
    {
        for (int i = 0; i < Weights.Length; i++)
        {
            if (Random.Range(-1.0f, 1.0f) < Config.MutationRate)
            {
                Weights[i] += Random.Range(-1.0f, 1.0f) * Config.MaxPerturbatlon;
            }
        }
    }

    public void Crossover(double[] mum,double[] dad,out double[] baby1,out double[] baby2)
    {
        //杂交不成功
        if(Random.value>Config.CrossoverRate || mum == dad)
        {
            baby1 = mum;
            baby2 = dad;
            return;
        }

        baby1 = new double[mum.Length];
        baby2 = new double[dad.Length];

        //随机选择一个杂交点
        int cp = Random.Range(0, mum.Length - 1);

        //前半部分直接遗传
        for(int i=0;i<cp;i++)
        {
            baby1[i] = mum[i];
            baby2[i] = dad[i];
        }
        //后半部分杂交
        for(int i=cp;i<mum.Length;i++)
        {
            baby1[i] = dad[i];
            baby2[i] = mum[i];
        }
    }


    /// <summary>
    /// 更新适应性分数
    /// </summary>
    void UpdateFitnessScores()
    {

    }

    /// <summary>
    /// 迭代
    /// </summary>
    public void Epoch()
    {
        UpdateFitnessScores();

        List<UGenome> NewBabys = new List<UGenome>();
        while(NewBabys.Count<Genomes.Count)
        {
            //选择两个父辈用来产生后代
            UGenome Dad = Selection();
            UGenome Mum = Selection();
            UGenome Baby1 = new UGenome(Config.NumWeights);
            UGenome Baby2 = new UGenome(Config.NumWeights);
            Crossover(Mum.Weights, Dad.Weights,out Baby1.Weights, out Baby2.Weights);

            //变异
            Mutate(Baby1.Weights);
            Mutate(Baby2.Weights);

            //增加新婴儿
            NewBabys.Add(Baby1);
            NewBabys.Add(Baby2);
        }

        //设置为新代
        Genomes = NewBabys;

        Generation++;
    }
}
