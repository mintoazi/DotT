using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class PieceRotation : MonoBehaviour
{
    [SerializeField] private float speed;
    private void Awake()
    {
        this.UpdateAsObservable()
        .Subscribe(
            _ => ManagedUpdate()
        );
    }

    private void ManagedUpdate()
    {
        transform.Rotate(0f, 0f, 360f * Time.deltaTime * speed);
    }
}
