using UnityEngine;

public class MatchingManager : MonoBehaviour
{
    [SerializeField] Animator anim;
    public enum State
    {
        Room,
        CharaSelect
    }
    private State state = State.Room;

    private void OnEnable() => Locator<MatchingManager>.Bind(this);
    private void OnDisable() => Locator<MatchingManager>.Unbind(this);

    public void SetState(State s)
    {
        state = s;
        anim.SetInteger("State", (int)state);
    }
}
