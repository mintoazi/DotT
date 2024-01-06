using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CpuController : MonoBehaviour
{
    [SerializeField] private Battler player;
    [SerializeField] private Battler cpu; // enemy
    public Battler CPU { set { cpu = value; } }

    public enum Difficulty
    {
        Easy,
        Normal,
        Hard
    }
    private Difficulty currentDifficulty = Difficulty.Easy;
    public Difficulty CurrentDifficulty { set { currentDifficulty = value; } }

    // �J�[�h�̕]���l(���㑝����\�����l�����ē_�����ɂ���)
    private const int HighPoint = 100;
    private const int LowPoint = 50;
    
    // �ړ��ł���^�C���̐�
    private const int tiles = 9;

    public Card PlayCard()
    {
        List<Card> hands = cpu.Hand.Hands;
        List<Card> canUseCards = new List<Card>();
        foreach(Card c in hands)
        {
            if (!cpu.Model.IsCostUses[c.Base.Cost].Value) canUseCards.Add(c);
        }

        int cardIndex = 0; // �J�[�h�ԍ��ۑ��p
        int cardMaxPoint = 0; // ��ԍ����̃J�[�h�̕]���l

        for (int i = 0; i < canUseCards.Count; i++)
        {
            int cardPoint = 0;
            if ((int)canUseCards[i].Base.Type == cpu.Model.CharaType.Value) cardPoint += HighPoint; // ������v�Ł{�P�O�O
            if (CheckHitCard(canUseCards[i], cpu.BattlerMove.PiecePos)) cardPoint += LowPoint; // �U�������Ă邱�Ƃ��ł���Ȃ�{�T�O
            if(cardMaxPoint <= cardPoint) // �����]���l�ȏ�ŃJ�[�h����������(�R�X�g�̍��������o�����)
            {
                cardMaxPoint = cardPoint;
                cardIndex = i;
            }
        }
        return canUseCards[cardIndex];
    }
    public Card ReDraw()
    {
        List<Card> hands = cpu.Hand.Hands;
        return hands[Random.Range(0, hands.Count)];
    }
    public Card PlaySupportCard()
    {
        List<Card> hands = cpu.Hand.Hands;
        // ������v=>�g�p�ς݃R�X�g=>�R�X�g�̒Ⴂ�J�[�h�̏��Ԃŏo���B
        for (int i = 0; i < hands.Count; i++)
        {
            if ((int)hands[i].Base.Type == cpu.Model.CharaType.Value) return hands[i];
            if (cpu.Model.IsCostUses[hands[i].Base.Cost].Value) return hands[i];
        }
        return hands[0];
    }

    public Vector2Int MovePos()
    {
        List<Vector2Int> movePos = new List<Vector2Int>();
        for(int i = 0; i < tiles; i++)
        {
            Vector2Int prePos = cpu.BattlerMove.PiecePos;
            prePos.x += i % 3 - 1;
            prePos.y += i / 3 - 1;
            if (prePos.x < 0 || prePos.x > 2) continue;
            if (prePos.y < 0 || prePos.y > 2) continue;
            if (CheckHitCard(cpu.RecentCard, prePos)) movePos.Add(prePos);
        }

        int rand = Random.Range(0, movePos.Count);
        return movePos[rand];
        
    }

    // �U���������邩�̔���
    private bool CheckHitCard(Card card, Vector2Int cpuPos)
    {
        List<Vector2Int> attackPos;
        List<Vector2Int> recievePos;

        var playerPos = player.BattlerMove.PiecePos;

        attackPos = Calculator.CalcAttackPosition(cpuPos,
                                                  card.Base.AttackPos,
                                                  card.Base.AttackAhead);
        recievePos = Calculator.CalcReflection(attackPos);

        foreach(Vector2Int v2i in recievePos)
        {
            if (v2i == playerPos) return true;
        }
        return false;
    }
}
