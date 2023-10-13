using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class Card : MonoBehaviour
{
    //[SerializeField] private GameObject parentObject;

    [SerializeField] private Text cardName;
    [SerializeField] private Text description;
    [SerializeField] private Text cost;
    [SerializeField] private Image cardFrame;
    [SerializeField] private Sprite[] cardSprite = null;
    [SerializeField] private Vector3 selectedScale;
    [SerializeField] private Vector3 handScale;
    [SerializeField] private GameObject buttonPanel;
    [SerializeField] private GameObject hidePanel;
    public CardBase Base { get; private set; }
    // �A�N�V������o�^
    public UnityAction<Card> OnClickCard;
    
    public void Set(CardBase cardBase, bool isEnemy)
    {
        Base = cardBase;
        cardName.text = cardBase.Name;
        description.text = cardBase.Description;
        cost.text = cardBase.Cost.ToString();
        cardFrame.sprite = cardSprite[(int)cardBase.Type];

        // �����̃J�[�h�̓{�^����\���A����̃J�[�h�͗��ʕ\��
        buttonPanel.SetActive(!isEnemy);
        hidePanel.SetActive(isEnemy);
    }

    public void SetPosition(Vector3 pos)
    {
        this.transform.localPosition = pos;
    }

    public void SetLayer(int i)
    {
        transform.SetSiblingIndex(i);
        //canvas.sortingOrder = i;
    }

    public void SetScale(bool isSelect)
    {
        if (isSelect) transform.localScale = selectedScale;
        else transform.localScale = handScale;
    }

    public void OnClick()
    {
        //Debug.Log("�J�[�h���I������܂����B");
        OnClickCard?.Invoke(this);
    }

    /*
    -���܂ɕςȂƂ���ɃJ�[�h���u�����o�O�C��
    -�J�[�h�v���C����
    -�ړ�����

    -�J�[�h�ƔՖʂ̃��C���[���

    �����ύX�̎��o��
    �L�����\���̎��o��
    �R�X�g����̎��o��

    �U������
    �J�[�h���ʎ���
    
    �SPhase��Delay�ǉ�

    �L�����I������

    Photon�Ή�

    ����������
    �E
    �E�U���G�t�F�N�g
    �E�R�}
    �E��D�̖���
    �E�g�p�����J�[�h
    �E�L����UI
    �EHP
    �E����
    �E�R�X�gUI

    �������Ȃ��I�u�W�F�N�g
    �E�^�C��
    �EUI(��D�A�I������UI�A)
    �E�J����
    �E�I���\�^�C��

    �e�Q�[����ʂɓG�̓����̏�񂾂����o����RPC�ő���̃X�N���v�g���g���C���[�W

    */
}
