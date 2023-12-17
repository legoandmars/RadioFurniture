using RadioBrowser.Models;
using RadioBrowser;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Linq;
using Cysharp.Threading.Tasks;
using RadioFurniture.ClipLoading;
using System.Threading;
using UnityEngine.UIElements.UIR;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.InputSystem;

namespace RadioFurniture.Managers
{
    public class RadioManager : MonoBehaviour
    {
        // private AudioSource _audioSource;
        private string _radioUrl = null;
        private UnityAudioStream _unityAudioStream;

        List<StationInfo> _stations;
        void Start()
        {
            var audioStreamObject = new GameObject("AudioStream");
            DontDestroyOnLoad(audioStreamObject);
            audioStreamObject.hideFlags = HideFlags.HideAndDontSave;
            _unityAudioStream = audioStreamObject.AddComponent<UnityAudioStream>();

            Debug.Log("HM?");
            GetRadioStations().Forget();
        }

        void Update()
        {
            if (Keyboard.current.pKey.wasPressedThisFrame)
            {
                PlayRandomRadioStation();
            }
        }

        private async UniTask StartAsync()
        {
            await GetRadioStations();

            PlayRandomRadioStation();
        }

        private void PlayRandomRadioStation()
        {
            var randomStation = _stations[UnityEngine.Random.Range(0, _stations.Count)];
            Debug.Log("guh");
            Debug.Log(randomStation.UrlResolved);
            Debug.Log("STREAMING NOW!");
            _radioUrl = randomStation.UrlResolved.ToString();

            _unityAudioStream.Stop();
            _unityAudioStream.PlayAudioFromStream(_radioUrl);
            //await LoadAudioStream(_radioUrl);
        }

        private async UniTask GetRadioStations()
        {
            Debug.Log("SEARCHING!");
            // Initialization
            var radioBrowser = new RadioBrowserClient();
            List<StationInfo> topByVotes = await radioBrowser.Stations.GetByVotesAsync(1000);
            _stations = topByVotes.Where(x => x != null && x.Codec != null && x.Codec == "MP3" && x.UrlResolved != null && x.UrlResolved.Scheme == "https").ToList();
        }

        private async UniTask LoadAudioStream(string uri)
        {

            /*var requestTask = AudioFromWebRequest.LoadAudioFrom(audioSource, uri, null, AudioType.MPEG, true, 64, default);
            Debug.Log("new task just dropped");
            var requestWithAudio = await requestTask.Task.ConfigureAwait(true);
            // Get ready clip to be used in Unity Audio Source
            audioSource.clip = requestWithAudio.AudioClip.AudioClip;
            Debug.Log("GUHHHHHH");*/
            //StartCoroutine(LoadAudio(uri));
        }
        /*
        AudioClip clipa;
        AudioClip clipb;
        bool played;
        WWW www;
        float timer;

        public int interval = 30;

        void Update()
        {
            return;
            if (_radioUrl == null) return;
            Debug.Log(timer);

            timer = timer + 1 * Time.deltaTime; //Mathf.FloorToInt(Time.timeSinceLevelLoad*10); 
                                                //Time.frameCount; 

            if (timer >= interval)
            {           //if(timer%interval == 0){
                if (www != null)
                {
                    www.Dispose();
                    www = null;
                    played = false;
                    timer = 0;
                }
            }
            else
            {
                if (www == null)
                {
                    www = new WWW(_radioUrl);
                }
            }
            if (clipa == null)
            {
                if (www != null)
                {
                    clipa = www.GetAudioClip(false, true, AudioType.MPEG);
                }
            }

            if (clipa != null)
            {
                if (clipa.isReadyToPlay && played == false)
                {
                    _audioSource.PlayOneShot(clipa);
                    played = true;
                    clipa = null;
                }
            }
        }

        private IEnumerator LoadAudio(string uri)
        {
            using (var webRequest = UnityWebRequestMultimedia.GetAudioClip(uri, AudioType.OGGVORBIS))
            {
                ((DownloadHandlerAudioClip)webRequest.downloadHandler).streamAudio = true;
                ((DownloadHandlerAudioClip)webRequest.downloadHandler).ReceiveContentLength(1024);
                webRequest.SendWebRequest();
                while (!webRequest.isNetworkError && webRequest.downloadedBytes < 1024)
                    yield return null;

                if (webRequest.isNetworkError)
                {
                    Debug.LogError(webRequest.error);
                    yield break;
                }

                var clip = ((DownloadHandlerAudioClip)webRequest.downloadHandler).audioClip;
                _audioSource.clip = clip;
                _audioSource.Play();
            }*/
            /*
            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(uri, AudioType.OGGVORBIS))
            {
                (www.downloadHandler as DownloadHandlerAudioClip).compressed = false;
                (www.downloadHandler as DownloadHandlerAudioClip).streamAudio = true;
                var operation = www.SendWebRequest();
                while (www.downloadProgress < 0.5)
                {
                    Debug.Log("progress: " + www.downloadProgress);
                    yield return null;
                }

                if (www.result == UnityWebRequest.Result.ConnectionError)
                {
                    Debug.Log(www.error);
                }
                else
                {
                    Plugin.Log("Audio clip loaded.");
                    var clip = (www.downloadHandler as DownloadHandlerAudioClip).audioClip;
                    _audioSource.clip = clip;
                    _audioSource.Play();
                }
                yield return operation;
            }*/
        /*}
        private static async UniTask MainAsync()
        {
            Debug.Log("SEARCHING!");
            // Initialization
            var radioBrowser = new RadioBrowserClient();

            // Searching by name
            var searchByName = await radioBrowser.Search.ByNameAsync("shonan");
            Debug.Log(searchByName.FirstOrDefault()?.Name);

            // Advanced searching
            var advancedSearch = await radioBrowser.Search.AdvancedAsync(new AdvancedSearchOptions
            {
                Language = "english",
                TagList = "news",
                Limit = 5
            });

            foreach (var station in advancedSearch) Debug.Log(station.Name);
            Debug.Log("");

            // Getting top stations
            var topByVotes = await radioBrowser.Stations.GetByVotesAsync(5);

            foreach (var station in topByVotes) Debug.Log(station.Name);
            Debug.Log("");

            // Getting codecs list
            var codecs = await radioBrowser.Lists.GetCodecsAsync();
            foreach (var codec in codecs) Debug.Log($"{codec.Name} - {codec.Stationcount}");
        }*/
    }
}
