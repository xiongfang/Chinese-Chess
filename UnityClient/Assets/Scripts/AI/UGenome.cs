using UnityEngine;
using System.Collections;

/// <summary>
/// 基因编码
/// </summary>
public class UGenome  {
    //基因编码
    public double[] Weights;
    //适应性
    public double Fidness;

    //突变
    public void Mutate()
    {
        for(int i=0;i<Weights.Length;i++)
        {
            if(Random.Range(-1.0f,1.0f)<Config.MutationRate)
            {
                Weights[i] += Random.Range(-1.0f, 1.0f) * Config.MaxPerturbatlon;
            }
        }
    }
}
