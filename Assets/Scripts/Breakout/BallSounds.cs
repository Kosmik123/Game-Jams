using NaughtyAttributes;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Bipolar.Breakout
{
	public class BallSounds : MonoBehaviour
    {
		[System.Serializable]
        public struct TagSoundMapping
        {
            [Tag]
            public string tag;
            public AudioClip sound;
        }

		[SerializeField]
		private BallController ball;
		[SerializeField]
		private AudioSource audioSource;

        [SerializeField]
        private List<TagSoundMapping> sounds;

        private Dictionary <string, AudioClip> soundsByTag;

		private void Awake()
		{
            soundsByTag = sounds.ToDictionary(m => m.tag, m => m.sound);
		}

		private void OnEnable()
		{
			ball.OnBounced += Ball_OnBounced;
		}

		private void Ball_OnBounced(Collision2D collision)
		{
			if (soundsByTag.TryGetValue(collision.transform.tag, out var sound))
			{
				audioSource.PlayOneShot(sound);
			}
		}

		private void OnDisable()
		{
			ball.OnBounced -= Ball_OnBounced;
		}
	}
}
