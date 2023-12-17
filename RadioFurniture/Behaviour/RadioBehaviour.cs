using Cysharp.Threading.Tasks;
using RadioFurniture.ClipLoading;
using RadioFurniture.Managers;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace RadioFurniture.Behaviour
{
    public class RadioBehaviour : NetworkBehaviour
    {
        private bool _radioOn = false;

        [SerializeField]
        private AudioSource _audioSource = default;

        [SerializeField]
        private List<AudioClip> _channelSeekClips = new();

        [SerializeField]
        private AudioClip _static;

        private MP3Stream? _stream;
        private Guid? _lastStationId;
        private bool _playingStatic = false;

        private void Awake()
        {
            _audioSource.volume = 0.5f;
        }

        public void TogglePowerLocalClient()
        {
            Debug.Log("TOGGLING POWER");
            if (_radioOn)
            {
                TurnOffRadioServerRpc();
            }
            else
            {
                TurnOnRadioServerRpc();
            }
        }

        private Guid GetRandomRadioGuid()
        {
            var randomStation = RadioManager.GetRandomRadioStation();
            if (randomStation == null) return Guid.Empty;

            return randomStation.StationUuid;
        }

        public void ToggleStationLocalClient()
        {
            Debug.Log("TOGGLING STATION");
            if (_radioOn)
            {
                ChangeStationServerRpc();
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void TurnOnRadioServerRpc()
        {
            if (_lastStationId == null)
            {
                _lastStationId = GetRandomRadioGuid();
                TurnOnAndSyncRadioClientRpc(_lastStationId!.Value.ToString());
            }
            TurnOnRadioClientRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        public void TurnOffRadioServerRpc()
        {
            TurnOffRadioClientRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        public void ChangeStationServerRpc()
        {
            if (!_radioOn) return;
            _lastStationId = GetRandomRadioGuid();
            TurnOnAndSyncRadioClientRpc(_lastStationId!.Value.ToString());
        }

        [ClientRpc]
        public void TurnOnRadioClientRpc()
        {
            TurnRadioOnOff(true);
        }

        [ClientRpc]
        public void TurnOnAndSyncRadioClientRpc(string guidString)
        {
            // SYNC 
            _lastStationId = Guid.Parse(guidString);
            TurnRadioOnOff(true);
        }

        [ClientRpc]
        public void TurnOffRadioClientRpc()
        {
            TurnRadioOnOff(false);
        }

        private void TurnRadioOnOff(bool state)
        {
            Debug.Log("Changing radio state!");
            Debug.Log(state);

            StopStaticIfPlaying();

            if (state && _lastStationId != null)
            {
                Debug.Log("Changing radio station...");
                PlayTransitionSound();
                PlayStatic();

                var station = RadioManager.GetRadioStationByGuid(_lastStationId.Value);
                if (station != null)
                {
                    if (_stream != null)
                    {
                        Stop();
                    }
                    PlayAudioFromStream(station.UrlResolved.ToString());
                }
            }
            else if (!state && _stream != null)
            {
                Stop();
                PlayTransitionSound();
            }
            _radioOn = state;
        }

        private void PlayTransitionSound()
        {
            var seekClip = _channelSeekClips[UnityEngine.Random.Range(0, _channelSeekClips.Count)];
            _audioSource.PlayOneShot(seekClip);
        }

        private void PlayStatic()
        {
            _playingStatic = true;
            _audioSource.clip = _static;
            _audioSource.Play();
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
                StopStaticIfPlaying();

                Debug.Log("new clip just dropped.");
                _audioSource.clip = AudioClip.Create("mp3_Stream", int.MaxValue,
                    _stream.bufferedWaveProvider.WaveFormat.Channels,
                    _stream.bufferedWaveProvider.WaveFormat.SampleRate,
                    true, new AudioClip.PCMReaderCallback(_stream.ReadData));

                _stream.decomp = false; //Do not create shitload of audioclips
            }
        }

        private void StopStaticIfPlaying()
        {
            if (_playingStatic)
            {
                _audioSource.Stop();
                _audioSource.time = 0;
                _audioSource.clip = null;
                _playingStatic = false;
            }
        }

        public void Stop()
        {
            _stream?.StopPlayback();
            _stream = null;

            if (_audioSource != null)
            {
                _audioSource.Stop();
                _audioSource.time = 0;
                _audioSource.clip = null;
            }
        }
    }
}
