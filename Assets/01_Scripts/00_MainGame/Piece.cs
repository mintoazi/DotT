using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class Piece : MonoBehaviour
{
    [SerializeField] private float speed = 5.0f;
    [SerializeField] private Vector3 targetPos = Vector3.zero;
    [Header("UP,DOWN,LEFT,RIGHT")]
    [SerializeField] private Vector3[] moveDirection = new Vector3[4];
    private enum Direction{ Up, Down, Left, Right }

    private void Awake()
    {
        //���P�[�g
        Locator<Piece>.Bind(this);

        //Update�����̒ǉ�
        this.UpdateAsObservable()
        .Subscribe(
            _ => ManagedUpdate(), 
            () => Locator<Piece>.Unbind(this)
        );

        Init();
    }
    private void Init()
    {
        targetPos = transform.localPosition;
    }
    private void ManagedUpdate()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow)) Move((int)Direction.Up);
        if (Input.GetKeyDown(KeyCode.DownArrow)) Move((int)Direction.Down);
        if (Input.GetKeyDown(KeyCode.LeftArrow)) Move((int)Direction.Left);
        if (Input.GetKeyDown(KeyCode.RightArrow)) Move((int)Direction.Right);

        //
        if ((transform.localPosition - targetPos).magnitude == 0f) return;
        transform.localPosition = Vector3.MoveTowards(transform.localPosition, targetPos, speed * Time.deltaTime);

        
    }

    /// <summary>
    /// �ړ����\�b�h
    /// </summary>
    /// <param name="direction">����</param>
    private void Move(int direction)
    {
        //SoundManager.instance.PlaySE(SoundManager.SE_Type.Move);
        targetPos = targetPos + moveDirection[direction];
    }
}
