using Bipolar.Prototyping;
using NaughtyAttributes;
using UnityEngine;

namespace Bipolar.Breakout
{
    public class GameSettings : ScriptableSingleton<GameSettings>
    {
        [SerializeField, Tag]
        private string ballTag;
        public static string BallTag => Instance.ballTag;     
       
        [SerializeField]
        private float ballSpeed;
        public static float BallSpeed => Instance.ballSpeed;
    }
}
