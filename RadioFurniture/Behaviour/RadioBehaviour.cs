using Cysharp.Threading.Tasks;
using RadioFurniture.ClipLoading;
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

        private MP3Stream? _stream;
        private Guid? _lastStationId;

        public void TogglePowerLocalClient()
        {
            Debug.Log("TOGGLING POWER");
            if (_radioOn)
            {
                TurnOffRadioClientRpc();
            }
            else
            {
                TurnOnRadioClientRpc();
            }
        }

        public void ToggleStationLocalClient()
        {
            Debug.Log("TOGGLING STATION");
        }

        [ServerRpc(RequireOwnership = false)]
        public void TurnOnRadioServerRpc()
        {
            TurnOnRadioClientRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        public void TurnOffRadioServerRpc()
        {
            TurnOffRadioClientRpc();
        }

        [ClientRpc]
        public void TurnOnRadioClientRpc()
        {
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
            _radioOn = state;
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

            if (_audioSource != null)
            {
                _audioSource.Stop();
                _audioSource.time = 0;
                _audioSource.clip = null;
            }
        }
    }
}
