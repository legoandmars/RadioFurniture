using RadioFurniture.ClipLoading;
using System;
using UnityEngine.Networking;

namespace WebRequestAudio.ClipLoading
{
    /// <summary>
    /// Helper Wrapper around webrequest and result Audio clip
    /// Automatically gets disposed.
    /// </summary>
    public class DisposableAudioWebRequest : IDisposable
    {
        // should not be disposed at the time request is disposed.
        private DisposableAudioClip _disposableAudioClip;

        private UnityWebRequest _request;
        private bool _disposed = false;
        private string _error;
        private int _code;
        private bool _isDone = false;

        /// <summary>
        /// Automatically disposed only if cancelled or failed.
        /// Be sure to dispose it when finished using it.
        /// </summary>
        public DisposableAudioClip AudioClip => _disposableAudioClip;

        public string Error => _error;
        public int Code => _code;
        public bool IsDone => _isDone;
        public bool IsDisposed => _disposed;

        public DisposableAudioWebRequest(UnityWebRequest request)
        {
            _request = request;
        }

        public string GetReadableError()
        {
            return $"Code : {_code} {_error}";
        }

        public bool HasErrors()
        {
            return !string.IsNullOrEmpty(_error);
        }

        internal void SetDisposableClip(DisposableAudioClip clip)
        {
            _disposableAudioClip = clip;
        }

        internal void SetStatus()
        {
            _code = (int)_request.responseCode;
            _error = _request.error;
            _isDone = _request.isDone;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            if (_request != null)
            {
                SetStatus();
                _request.Dispose();
            }

            _request = null;
        }
    }
}