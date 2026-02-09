using System.Collections.Generic;
using UnityEngine;

namespace Core.Controllers.Audio {
	public class SoundController : MonoBehaviour {
		private string                        _soundsDirPath    = "Sounds/";
		private List<AudioSource>             _audioSources     = new List<AudioSource>();
		private Dictionary<string, AudioClip> _loadedAudioClips = new Dictionary<string, AudioClip>();

		private bool  _canPlaySound  = true;
		private float _defaultVolume = 0.5f;

		public void Initialize() {
			InitializeSources();
		}

		private void InitializeSources() {
			for (var i = 0; i < 8; ++i) {
				var audioSource = gameObject.AddComponent<AudioSource>();
				_audioSources.Add(audioSource);
			}
			
			SetVolume(_defaultVolume);
			Debug.Log("<color=green>[SoundController]</color> Initialized!");
		}

		public void PlaySound(string soundName, float delay = 0f) {
			if (!_canPlaySound)
				return;

			var       filePath = _soundsDirPath + soundName;
			AudioClip clip     = null;
			if (_loadedAudioClips.ContainsKey(soundName))
				clip = _loadedAudioClips[soundName];

			if (!clip) {
				clip = Resources.Load<AudioClip>(filePath);
				if (!clip) {
					Debug.Log("<color=red>[SoundController]</color> Audio file not found!");
					return;
				}

				_loadedAudioClips[soundName] = clip;
			}

			var audioSource = GetFreeAudioSource();
			if (!audioSource)
				return;

			audioSource.clip = clip;
			audioSource.loop = false;

			if (delay > 0f)
				audioSource.PlayDelayed(delay);
			else
				audioSource.Play();
		}

		private AudioSource GetFreeAudioSource() {
			for (int i = 0; i < _audioSources.Count; ++i) {
				if (_audioSources[i].isPlaying)
					continue;

				return _audioSources[i];
			}

			return null;
		}

		private void SetVolume(float volumeValue) {
			for (var i = 0; i < _audioSources.Count; ++i)
				_audioSources[i].volume = volumeValue;
		}
	}
}
