using System;
using System.Threading;
using UnityEngine;

namespace RadioFurniture.ClipLoading
{
    /// <summary>
    /// Contains audio clip from webrequest
    /// Disposed only when request cancelled or on error.
    /// </summary>
    public class DisposableAudioClip : IDisposable
    {
        private AudioSource _source;
        private AudioClip _clip;
        private CancellationToken _token;
        private bool _disposed = false;

        public AudioClip AudioClip => _clip;
        public bool Disposed => _disposed;

        public DisposableAudioClip(AudioSource source, AudioClip clip)
        {
            _source = source;
            _clip = clip;
            SetToken(new CancellationToken());
        }

        internal void SetToken(CancellationToken token)
        {
            _token = token;
            _token.Register(() => { Dispose(); });
        }

        public void Dispose()
        {
            _disposed = true;
            if (_source != null)
            {
                StopAndCleanSource();
            }

            if (_clip != null)
            {
                UnityEngine.Object.Destroy(_clip);
            }

            _source = null;
            _clip = null;
        }

        private void StopAndCleanSource()
        {
            if (_source.clip == _clip)
            {
                _source.Stop();
                _source.clip = null;
            }
        }
    }
}