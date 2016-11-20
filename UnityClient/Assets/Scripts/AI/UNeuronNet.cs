using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UNeuronNet {

    public List<UNeuronLayer> AllLayers;

    public UNeuronNet()
    {
        AllLayers = new List<UNeuronLayer>();
        if (Config.NumHiddenLayer>0)
        {
            AllLayers.Add(new UNeuronLayer(Config.NumNeuronPerHiddenLayer, Config.NumInputs));

            for(int i=0;i<Config.NumHiddenLayer-1;i++)
            {
                AllLayers.Add(new UNeuronLayer(Config.NumNeuronPerHiddenLayer,Config.NumNeuronPerHiddenLayer));
            }

            AllLayers.Add(new UNeuronLayer(Config.NumOutputs, Config.NumNeuronPerHiddenLayer));
        }
        else
        {
            AllLayers.Add(new UNeuronLayer(Config.NumOutputs, Config.NumInputs));
        }
    }

    double Sigmod(double activation,double response)
    {
        return 0;
    }

    public double[] Update(double[] inputs)
    {
        List<double> outputs = new List<double>();

        for(int i=0;i< AllLayers.Count;i++)
        {
            if(i>0)
            {
                inputs = outputs.ToArray();
            }
            outputs.Clear();

            
            for(int j=0;j< AllLayers[i].Neurons.Length;j++)
            {
                double NetInputs = 0;
                int WeightIndex = 0;
                //算权重和输入的和
                for (int k=0;k< AllLayers[i].Neurons[i].InputWeights.Length-1;k++)
                {
                    NetInputs += AllLayers[i].Neurons[i].InputWeights[k] * inputs[WeightIndex++];
                }
                //加入偏移
                NetInputs += AllLayers[i].Neurons[i].InputWeights[AllLayers[i].Neurons[i].InputWeights.Length - 1] * Config.Bias;

                //输出
                outputs.Add(Sigmod(NetInputs, Config.ActivationResponse));
            }
        }
        return outputs.ToArray();
    }
}
