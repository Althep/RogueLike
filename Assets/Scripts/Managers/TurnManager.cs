using UnityEngine;
using Cysharp.Threading.Tasks;
public class TurnManager : MonoBehaviour
{
    public static TurnManager instance;
    public int GlovalTurn;
    public int LastTurn;
    public int accuredAp;
    PlayerEntity player;
    MonsterManager monsterManager;
    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        if(player == null)
        {
            
        }
        if(monsterManager == null)
        {
            monsterManager = GameManager.instance.Get_MonsterManager();
        }
    }

    public async UniTask OnTurnEnd(float actPoint)
    {
        Add_AP(Mathf.RoundToInt(actPoint));
        if(player == null)
        {
            player = PlayerController.instance.Get_PlayerEntity();
        }
        await monsterManager.OnTurnEnd(actPoint);
        player.ChangeActable(true);
    }

    public void Add_AP(int ap)
    {
        accuredAp+=ap;
        LastTurn = GlovalTurn;
        GlovalTurn = accuredAp/10;
        EventManager.instance.ProcessPassedTurn(LastTurn+1, GlovalTurn);
    }
}
