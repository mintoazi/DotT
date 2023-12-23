using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class PieceRotation : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private Direction rotDirection;
    enum Direction
    {
        X,
        Y,
        Z
    }
    private void Awake()
    {
        this.UpdateAsObservable()
        .Subscribe(
            _ => ManagedUpdate()
        );
    }

    private void ManagedUpdate()
    {
        switch (rotDirection)
        {
            case Direction.X:
                transform.Rotate(360f * Time.deltaTime * speed, 0f, 0f);
                break;
            case Direction.Y:
                transform.Rotate(0f, 360f * Time.deltaTime * speed, 0f);
                break;
            case Direction.Z:
                transform.Rotate(0f, 0f, 360f * Time.deltaTime * speed);
                break;
        }
    }
}
