using UnityEngine;
using System.Collections;

public class UGenAlg
{
    public class ConfigData
    {
        //基因算法配置
        public float MutationRate = 0.1f;    //突变率
        public double MaxPerturbatlon = 0.2f; //最大突变值
    }

    static ConfigData Config;

    //突变
    public static void Mutate(double[] Weights)
    {
        for (int i = 0; i < Weights.Length; i++)
        {
            if (Random.Range(-1.0f, 1.0f) < Config.MutationRate)
            {
                Weights[i] += Random.Range(-1.0f, 1.0f) * Config.MaxPerturbatlon;
            }
        }
    }

    public static void Crossover(double[] mun,double[] dad,out double[] baby1,out double[] baby2)
    {
        baby1 = mun;
        baby2 = dad;
    }
}
