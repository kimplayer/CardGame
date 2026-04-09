using System.Collections.Generic;
using UnityEngine;
using Unity.Sentis;

public class MLPolicyInference : MonoBehaviour
{
    [Header("모델 파일")]
    public ModelAsset modelAsset;

    private Model runtimeModel;
    private Worker worker;

    private const int INPUT_DIM = 60;
    private const int ACTION_DIM = 20;

    private void Awake()
    {
        if (modelAsset == null)
        {
            Debug.LogError("modelAsset이 연결되지 않았습니다.");
            return;
        }

        try
        {
            runtimeModel = ModelLoader.Load(modelAsset);
            worker = new Worker(runtimeModel, BackendType.CPU);
            Debug.Log("ML 모델 로드 완료");
        }
        catch (System.Exception e)
        {
            Debug.LogError("ML 모델 로드 실패 : " + e.Message);
        }
    }

    public float[] GetPolicy(float[] stateVector)
    {
        if (worker == null) return GetUniformPolicy();

        try
        {
            using var inputTensor = new Tensor<float>(
                new TensorShape(1, INPUT_DIM), stateVector);

            worker.Schedule(inputTensor);
            worker.FlushSchedule(true); // 추론 완료 대기

            var policyTensor = worker.PeekOutput("policy") as Tensor<float>;
            if (policyTensor == null) return GetUniformPolicy();

            policyTensor.CompleteAllPendingOperations();

            float[] policy = new float[ACTION_DIM];
            for (int i = 0; i < ACTION_DIM; i++)
                policy[i] = policyTensor[0, i];

            return policy;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Policy 추론 실패 : " + e.Message);
            return GetUniformPolicy();
        }
    }

    public float GetValue(float[] stateVector)
    {
        if (worker == null) return 0f;

        try
        {
            using var inputTensor = new Tensor<float>(
                new TensorShape(1, INPUT_DIM), stateVector);

            worker.Schedule(inputTensor);
            worker.FlushSchedule(true);

            var valueTensor = worker.PeekOutput("value") as Tensor<float>;
            if (valueTensor == null) return 0f;

            valueTensor.CompleteAllPendingOperations();
            return valueTensor[0, 0];
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Value 추론 실패 : " + e.Message);
            return 0f;
        }
    }

    private float[] GetUniformPolicy()
    {
        float[] policy = new float[ACTION_DIM];
        for (int i = 0; i < ACTION_DIM; i++)
            policy[i] = 1f / ACTION_DIM;
        return policy;
    }

    private void OnDestroy()
    {
        worker?.Dispose();
    }
}