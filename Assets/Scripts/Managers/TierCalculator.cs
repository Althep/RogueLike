using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TierCalculator 
{
    /// <summary>
    /// 현재 층수에 따라 아이템 티어별 출현 확률을 정규분포로 계산합니다.
    /// </summary>
    /// <param name="currentFloor">현재 던전 층수 (예: 1 ~ 25).</param>
    /// <param name="tierInterval">티어가 갱신되는 층 간격 (예: 3 또는 5), 기본 인터벌 3</param>
    /// <param name="maxTier">시스템이 가진 최대 아이템 티어.</param>
    /// <param name="isPotion">회복 아이템(포션)인지 여부. True일 경우 완만한 분포 적용.</param>
    /// <returns>키: 티어(int), 값: 확률(float)인 Dictionary.</returns>
   
    public static Dictionary<int, float> GetTierProbabilities(
        int currentFloor,
        int tierInterval,
        int maxTier,
        bool isPotion = false)
    {
        // 1. 목표 티어 (중앙값 Mu) 계산
        // 1층은 T1이 목표, X+1층부터 T2가 목표가 되도록 설정합니다.
        // 예: 간격 5일 때, 1~5층은 T1, 6~10층은 T2
        int targetTier = Mathf.FloorToInt((float)(currentFloor - 1) / tierInterval) + 1;

        // 목표 티어가 최대 티어를 초과하지 않도록 제한
        targetTier = Mathf.Min(targetTier, maxTier);

        // 2. 정규분포의 표준편차 (Sigma) 설정
        // 표준편차가 클수록 곡선이 완만해져 저티어 아이템(포션) 출현 확률이 유지됩니다.
        float stdDev;
        
        if (isPotion)
        {
            // 포션: 완만한 곡선 (더 큰 표준편차) -> 고층에서도 T1/T2 포션 출현 유도
            stdDev = 1.0f;
        }
        else
        {
            // 일반 아이템: 층수에 따른 티어 변화가 뚜렷한 곡선
            stdDev = 0.8f;
        }

        // 3. 각 티어별 확률 계산
        Dictionary<int, float> probabilities = new Dictionary<int, float>();
        float totalProbability = 0f;

        for (int tier = 1; tier <= maxTier; tier++)
        {
            // 정규분포 PDF (Probability Density Function) 계산
            // f(x) = 1 / (sigma * sqrt(2 * pi)) * exp(-0.5 * ((x - mu) / sigma)^2)

            float exponent = -0.5f * Mathf.Pow(((float)tier - targetTier) / stdDev, 2);
            float probability = (1.0f / (stdDev * Mathf.Sqrt(2.0f * Mathf.PI))) * Mathf.Exp(exponent);

            probabilities.Add(tier, probability);
            totalProbability += probability;
        }

        // 4. 확률 정규화 (모든 확률의 합을 1로 만들기)
        Dictionary<int, float> normalizedProbabilities = new Dictionary<int, float>();
        foreach (var kvp in probabilities)
        {
            normalizedProbabilities.Add(kvp.Key, kvp.Value / totalProbability);
        }

        return normalizedProbabilities;
    }

    /// <summary>
    /// 디버깅 및 테스트를 위해 확률 결과를 콘솔에 출력합니다.
    /// </summary>
    public static void PrintProbabilities(int floor, int interval, int maxTier)
    {
        Debug.Log($"--- {floor}층 일반 아이템 확률 (Target T{Mathf.FloorToInt((float)(floor - 1) / interval) + 1}, σ=0.8) ---");
        var generalProbs = GetTierProbabilities(floor, interval, maxTier, false);
        foreach (var kvp in generalProbs.OrderByDescending(x => x.Value))
        {
            Debug.Log($"  T{kvp.Key}: {kvp.Value * 100f:F2}%");
        }

        Debug.Log($"--- {floor}층 회복 포션 확률 (Target T{Mathf.FloorToInt((float)(floor - 1) / interval) + 1}, σ=1.5) ---");
        var potionProbs = GetTierProbabilities(floor, interval, maxTier, true);
        foreach (var kvp in potionProbs.OrderByDescending(x => x.Value))
        {
            Debug.Log($"  T{kvp.Key}: {kvp.Value * 100f:F2}% (완만)");
        }
    }

    public static Dictionary<int, float> GetCoreTierWeights(int currentFloor, int maxFloor, int maxTier)
    {
        Dictionary<int, float> weights = new Dictionary<int, float>();
        float totalWeight = 0f;

        float progress = Mathf.Clamp01((float)(currentFloor - 1) / (maxFloor - 1));

        // 1. 지수를 조금 더 낮게 시작 (3.2 정도로 설정하여 1~2티어 격차를 줄임)
        float exponent = Mathf.Lerp(3.2f, 0.5f, progress);

        for (int tier = 1; tier <= maxTier; tier++)
        {
            float weight = 1.0f / Mathf.Pow(tier, exponent);

            // 2. 3티어 이상부터는 '희귀도'를 위해 가중치를 추가로 감쇄 (Floor 1 기준)
            // 층이 낮을수록 3티어 이상의 아이템은 기본 지수보다 더 안 나오게 누릅니다.
            if (tier >= 3)
            {
                float rarityMultiplier = Mathf.Lerp(0.2f, 1.0f, progress); // 초반에는 원래 확률의 20%만 적용
                weight *= rarityMultiplier;
            }

            // 3. 후반부 평탄화 보정
            weight += Mathf.Pow(progress, 2) * 0.1f;

            weights.Add(tier, weight);
            totalWeight += weight;
        }

        // 정규화
        Dictionary<int, float> normalized = new Dictionary<int, float>();
        foreach (var kvp in weights)
        {
            normalized.Add(kvp.Key, kvp.Value / totalWeight);
        }

        return normalized;
    }
}
