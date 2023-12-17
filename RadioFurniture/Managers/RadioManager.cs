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
    public static class RadioManager
    {
        static List<StationInfo> _stations = new();
        public static void PreloadStations()
        {
            GetRadioStations().Forget();
        }

        private static async UniTask GetRadioStations()
        {
            Debug.Log("Searching Radio API for stations!");
            // Initialization
            var radioBrowser = new RadioBrowserClient();
            List<StationInfo> topByVotes = await radioBrowser.Stations.GetByVotesAsync(5000);
            _stations = topByVotes.Where(x => x != null && x.Codec != null && x.Codec == "MP3" && x.UrlResolved != null && x.UrlResolved.Scheme == "https").ToList();
            Debug.Log("Finished searching radio API for stations.");
            Debug.Log($"Found {_stations.Count}");
        }

        public static StationInfo? GetRandomRadioStation()
        {
            if (_stations.Count == 0) return null;
            var station = _stations[UnityEngine.Random.Range(0, _stations.Count)];
            Debug.Log(station.Name);
            Debug.Log(station.StationUuid);
            return station;
        }

        public static StationInfo? GetRadioStationByGuid(Guid guid)
        {
            return _stations.FirstOrDefault(x => x.StationUuid == guid);
        }
    }
}
