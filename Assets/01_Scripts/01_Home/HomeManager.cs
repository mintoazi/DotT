using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Home
{
    public enum Buttons
    {
        B_HOME,
        B_CPU,
        B_ROOM
    }
    public class HomeManager : MonoBehaviour
    {
        [SerializeField] private Animator animator;

       public void ToCPU()
        {
            animator.SetInteger("State", (int)Buttons.B_CPU);
        }

        public void ToHome()
        {
            animator.SetInteger("State", (int)Buttons.B_HOME);
        }
        public void PlayCPU()
        {
            SceneLoader.Instance.Load(Scenes.Scene.CPU_ROOM).Forget();
        }
    }
}
