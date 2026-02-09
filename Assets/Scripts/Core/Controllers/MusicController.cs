using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Core.Controllers.Audio {
	public class MusicController : MonoBehaviour {
		private AudioSource  _audioSource;
		private List<string> _allMusicFileNames = new List<string>();

		private string _musicDirPath = "Music/";
		private string _playedMusicFileName;
		private string _lastPlayedMusicFileName;

		private float _audioClipTime = 0f;
		private bool  _canPlayMusic  = true;
		private bool  _needLoop      = false;
		private float _defaultVolume = 0.3f;

		private LTDescr _fadeUpVolumeTween;
		private LTDescr _fadeDownVolumeTween;

		public void Initialize() {
			InitAudioSource();
			InitPossibleMusicTracks();

			Debug.Log("<color=green>[MusicController]</color> Initialized!");
		}

		private void InitAudioSource() {
			_audioSource = GetComponent<AudioSource>();
			SetVolume(_defaultVolume);
		}

		private void InitPossibleMusicTracks() {
			for (var i = 1; i < 2; ++i) {
				var musicPath = "track" + i;
				_allMusicFileNames.Add(musicPath);
			}
		}

		public void StartMusic(string musicName, bool needLoop) {
			if (!GameController.Instance.Player.Flags.NeedMusic)
				return;

			if (!_canPlayMusic)
				return;

			_needLoop         = needLoop;
			_audioSource.loop = needLoop;

			if (IsPlaying()) {
				FadeDownMusicVolume(
					() => { PlayMusic(musicName); });
			}
			else {
				PlayMusic(musicName);
			}
		}

		private void StartRandomTrack() {
			var trackName = GetRandomMusicName();
			StartMusic(trackName, false);
		}

		private string GetRandomMusicName() {
			var randTrackId = Random.Range(0, _allMusicFileNames.Count);
			var trackName   = _allMusicFileNames[randTrackId];
			if (trackName == _lastPlayedMusicFileName)
				return GetRandomMusicName();

			return trackName;
		}

		private void PlayMusic(string musicName) {
			var musicFileName = _musicDirPath + musicName;
			if (_playedMusicFileName == musicFileName)
				return;

			_lastPlayedMusicFileName = _playedMusicFileName;
			_playedMusicFileName     = musicFileName;

			var clip = Resources.Load<AudioClip>(_playedMusicFileName);
			if (!clip) {
				Debug.Log("<color=red>[MusicController]</color> Music file not found!");
				return;
			}

			_audioSource.clip = clip;
			_audioClipTime    = _audioSource.clip.length;

			_audioSource.volume = 0f;
			_audioSource.Play();
			FadeUpMusicVolume();
		}

		private void FadeUpMusicVolume(float time = 3f) {
			if (_fadeDownVolumeTween != null)
				LeanTween.cancel(_fadeDownVolumeTween.uniqueId);

			var volume = _audioSource.volume;
			_fadeUpVolumeTween = LeanTween.value(gameObject, volume, _defaultVolume, time).setOnUpdate(SetVolume).setOnComplete(
				() => { _fadeUpVolumeTween = null; });
		}

		private void FadeDownMusicVolume(Action onComplete = null, float time = 3f) {
			if (_fadeUpVolumeTween != null)
				LeanTween.cancel(_fadeUpVolumeTween.uniqueId);

			var volume = _audioSource.volume;
			_fadeDownVolumeTween = LeanTween.value(gameObject, volume, 0f, time).setOnUpdate(SetVolume).setOnComplete(
				() => {
					_fadeDownVolumeTween = null;
					onComplete?.Invoke();
				});
		}

		public void UpdateNeedPlay(bool isNeedPlay) {
			if (isNeedPlay) {
				StartMusic("track_1", true);
			}
			else {
				_audioClipTime       = 0f;
				_playedMusicFileName = "";
				FadeDownMusicVolume(null, 1f);
			}
		}

		private bool IsPlaying() {
			var isPlaying = _audioClipTime > 0;
			return isPlaying;
		}

		private void SetVolume(float volumeValue) {
			_audioSource.volume = volumeValue;
		}

		private void Update() {
			if (!IsPlaying() || _needLoop)
				return;

			_audioClipTime -= Time.deltaTime;
			if (_audioClipTime <= 0f)
				StartRandomTrack();
		}
	}
}
