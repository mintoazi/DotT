using UnityEngine;
using UniRx;
using UniRx.Triggers;
public class TouchEffectController : SingletonMonoBehaviour<TouchEffectController>
{
    [SerializeField] private ParticleSystem[] touchEffects;
    [SerializeField] private ParticleSystem swipeEffect;
    [SerializeField] private Camera targetCamera;
    private int lastNum;
    private Vector2 displaySize = new Vector2(1920f, 1080f);

    private void Awake()
    {
        this.UpdateAsObservable()
                .Subscribe(
                    _ => ManagedUpdate()
                );
    }
    private void ManagedUpdate()
    {
        displaySize = new Vector2(Screen.width, Screen.height);

        if (Input.GetMouseButtonDown(0))
        {
            var particle = GetParticle();
            var mousePosition = GetMousePos();

            particle.transform.localPosition = mousePosition;
           
            particle.Play();
            swipeEffect.Play();
        }
        if (Input.GetMouseButton(0))
        {
            var mousePosition = GetMousePos();
            swipeEffect.transform.localPosition = mousePosition;
        }
        if (Input.GetMouseButtonUp(0))
        {
            swipeEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    private Vector3 GetMousePos()
    {
        var mousePos = Input.mousePosition;
        mousePos.x -= displaySize.x / 2f;
        mousePos.y -= displaySize.y / 2f;
        return mousePos;
    }
    private ParticleSystem GetParticle()
    {
        lastNum++; 
        if (lastNum >= touchEffects.Length) lastNum = 0;
        return touchEffects[lastNum];
    }
}
