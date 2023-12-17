using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace RadioFurniture.ClipLoading
{
    public class UnityAudioStream : MonoBehaviour
    {
        private AudioSource _audioSource;
        private MP3Stream? _stream;
        
        private void Awake()
        {
            var audioSourceObject = new GameObject("AudioSource");
            DontDestroyOnLoad(audioSourceObject);
            audioSourceObject.hideFlags = HideFlags.HideAndDontSave;
            _audioSource = audioSourceObject.AddComponent<AudioSource>();
            // TODO: set audiosource transform/audio parameters

        }

        public void PlayAudioFromStream(string uri)
        {
            if (_stream == null) _stream = new MP3Stream();
            StartMP3Stream(uri).Forget();
        }

        private async UniTask StartMP3Stream(string uri)
        {
            await UniTask.SwitchToThreadPool();
            _stream.PlayStream(uri, _audioSource);
        }

        private void FixedUpdate()
        {
            _stream?.UpdateLoop();
        }

        private void Update()
        {
            if (_stream != null && _stream.decomp)
            {
                Debug.Log("new clip just dropped.");
                _audioSource.clip = AudioClip.Create("mp3_Stream", int.MaxValue,
                    _stream.bufferedWaveProvider.WaveFormat.Channels,
                    _stream.bufferedWaveProvider.WaveFormat.SampleRate,
                    true, new AudioClip.PCMReaderCallback(_stream.ReadData));

                _stream.decomp = false; //Do not create shitload of audioclips
            }
        }

        public void Stop()
        {
            _stream?.StopPlayback();
            _stream = null;

            if (_audioSource != null )
            {
                _audioSource.Stop();
                _audioSource.time = 0;
                _audioSource.clip = null;
            }
            // _stream?.Dispose();
        }
    }
}
