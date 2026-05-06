using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using static Defines; // ItemRarity가 선언된 곳으로 가정

public class ItemRarityManager
{
    // 기획 데이터 (나중에 CSV 등에서 읽어와서 세팅하기 좋습니다)
    public int normalRate = 50;
    public int magicRate = 30;
    public int rareRate = 15;
    public int uniqueRate = 0;

    // 누적 가중치를 저장할 구조체
    private struct RarityWeight
    {
        public ItemRarity rarity;
        public int cumulativeWeight;
    }

    // 미리 계산해둔 누적 확률 테이블과 전체 가중치 합
    private List<RarityWeight> rarityTable = new List<RarityWeight>();
    private int totalWeight = 0;

    public UniTask Init()
    {
        // 게임 시작 시, 또는 데이터 로드 완료 시 단 한 번만 계산합니다.
        CalculateCumulativeWeights();
        return UniTask.CompletedTask;
    }

    // 누적 가중치 테이블을 세팅하는 함수
    private void CalculateCumulativeWeights()
    {
        rarityTable.Clear();
        totalWeight = 0;

        // 순서대로 가중치를 누적시킵니다.
        AddWeight(ItemRarity.Normal, normalRate);
        AddWeight(ItemRarity.Magic, magicRate);
        AddWeight(ItemRarity.Rare, rareRate);
        AddWeight(ItemRarity.Unique, uniqueRate);

        // 데이터가 CSV의 리스트 형태로 들어온다면 위 과정을 for문 하나로 처리할 수 있습니다.
    }

    private void AddWeight(ItemRarity rarity, int weight)
    {
        if (weight <= 0) return; // 확률이 0인 등급은 아예 테이블에서 제외하여 최적화

        totalWeight += weight;
        rarityTable.Add(new RarityWeight { rarity = rarity, cumulativeWeight = totalWeight });
    }

    public ItemRarity SetItemRarity()
    {
        if (totalWeight == 0)
        {
            Debug.LogWarning("아이템 확률 테이블이 세팅되지 않았습니다!");
            return ItemRarity.Normal; // 방어 코드
        }

        // Random.Range(int min, int max)에서 max 값은 '제외(Exclusive)' 됩니다.
        // 즉, totalWeight가 100이라면 0부터 99까지의 값이 나옵니다.
        int randomValue = UnityEngine.Random.Range(0, totalWeight);

        // 미리 계산된 누적 테이블을 순회하며 어느 구간에 속하는지 찾습니다.
        foreach (var item in rarityTable)
        {
            if (randomValue < item.cumulativeWeight)
            {
                return item.rarity;
            }
        }

        return ItemRarity.Normal; // 이론상 도달하지 않지만 컴파일을 위해 반환
    }
}