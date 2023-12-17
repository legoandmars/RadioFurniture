// originally from naudio, some parts taken from MSCModLoader

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using NAudio.Wave;
using UnityEngine;

namespace RadioFurniture.ClipLoading
{
    public class MP3Stream
    {
        enum StreamingPlaybackState
        {
            Stopped,
            Playing,
            Buffering,
            Paused
        }

        public bool decomp = false;
        public string buffer_info, song_info;
        public BufferedWaveProvider bufferedWaveProvider;
        private AudioSource _audioSource;
        private volatile StreamingPlaybackState playbackState;
        private volatile bool fullyDownloaded;
        private static HttpClient httpClient;
        private HttpWebRequest webRequest;

        delegate void ShowErrorDelegate(string message);

        private void ShowError(string message)
        {
            Debug.LogWarning(message);
        }

        public void PlayStream(string streamUrl, AudioSource audioSource)
        {
            _audioSource = audioSource;
            if (playbackState == StreamingPlaybackState.Stopped)
            {
                playbackState = StreamingPlaybackState.Buffering;
                bufferedWaveProvider = null;
                ThreadPool.QueueUserWorkItem(StreamMp3, streamUrl);
            }
            else if (playbackState == StreamingPlaybackState.Paused)
            {
                playbackState = StreamingPlaybackState.Buffering;
            }
        }

        private void StreamMp3(object state)
        {
            fullyDownloaded = false;
            string url = (string)state;
            webRequest = (HttpWebRequest)WebRequest.Create(url);
            int metaInt = 0; // blocksize of mp3 data

            webRequest.Headers.Clear();
            webRequest.Headers.Add("GET", "/ HTTP/1.0");
            webRequest.Headers.Add("Icy-MetaData", "1");
            webRequest.UserAgent = "LethalCompany/RadioFurniture/1.0.0";
            HttpWebResponse resp;
            try
            {
                resp = (HttpWebResponse)webRequest.GetResponse();
            }
            catch (WebException e)
            {
                if (e.Status != WebExceptionStatus.RequestCanceled)
                {
                    System.Console.WriteLine(e.Message);
                }
                return;
            }
            byte[] buffer = new byte[16384 * 4]; // needs to be big enough to hold a decompressed frame
            try
            {
                // read blocksize to find metadata block
                metaInt = Convert.ToInt32(resp.GetResponseHeader("icy-metaint"));

            }
            catch
            {
            }
            IMp3FrameDecompressor decompressor = null;
            try
            {
                using (Stream responseStream = resp.GetResponseStream())
                {
                    ReadFullyStream readFullyStream = new ReadFullyStream(responseStream);
                    do
                    {
                        if (IsBufferNearlyFull)
                        {
                            System.Console.WriteLine("Buffer getting full, taking a break");
                            Thread.Sleep(1000);
                        }
                        else
                        {
                            Mp3Frame frame;
                            try
                            {
                                frame = Mp3Frame.LoadFromStream(readFullyStream);
                            }
                            catch (EndOfStreamException)
                            {
                                fullyDownloaded = true;
                                // reached the end of the MP3 file / stream
                                break;
                            }
                            catch (WebException)
                            {
                                // probably we have aborted download from the GUI thread
                                break;
                            }
                            if (frame == null) break;
                            if (decompressor == null)
                            {
                                // don't think these details matter too much - just help ACM select the right codec
                                // however, the buffered provider doesn't know what sample rate it is working at
                                // until we have a frame
                                decompressor = CreateFrameDecompressor(frame);
                                bufferedWaveProvider = new BufferedWaveProvider(decompressor.OutputFormat)
                                {
                                    BufferDuration = TimeSpan.FromSeconds(30) // allow us to get well ahead of ourselves
                                };
                                //this.bufferedWaveProvider.BufferedDuration = 250;

                                decomp = true; //tell main Unity Thread to create AudioClip
                            }
                            int decompressed = decompressor.DecompressFrame(frame, buffer, 0);
                            bufferedWaveProvider.AddSamples(buffer, 0, decompressed);
                        }

                    } while (playbackState != StreamingPlaybackState.Stopped);
                    System.Console.WriteLine("Exiting Thread");
                    // was doing this in a finally block, but for some reason
                    // we are hanging on response stream .Dispose so never get there
                    decompressor.Dispose();
                    readFullyStream.Close();
                    readFullyStream.Dispose();
                }
            }
            finally
            {
                if (decompressor != null)
                {
                    decompressor.Dispose();
                }
            }
        }

        public void ReadData(float[] data)
        {
            if (bufferedWaveProvider != null)
            {
                bufferedWaveProvider.ToSampleProvider().Read(data, 0, data.Length);
            }
        }

        private static IMp3FrameDecompressor CreateFrameDecompressor(Mp3Frame frame)
        {
            WaveFormat waveFormat = new Mp3WaveFormat(frame.SampleRate, frame.ChannelMode == ChannelMode.Mono ? 1 : 2,
                frame.FrameLength, frame.BitRate);
            return new AcmMp3FrameDecompressor(waveFormat);
        }

        private bool IsBufferNearlyFull
        {
            get
            {
                return bufferedWaveProvider != null &&
                       bufferedWaveProvider.BufferLength - bufferedWaveProvider.BufferedBytes
                       < bufferedWaveProvider.WaveFormat.AverageBytesPerSecond / 4;
            }
        }

        private void buttonPlay_Click(object sender, EventArgs e)
        {
            // starting logic
            /*
            if (playbackState == StreamingPlaybackState.Stopped)
            {
                playbackState = StreamingPlaybackState.Buffering;
                bufferedWaveProvider = null;
                ThreadPool.QueueUserWorkItem(StreamMp3, textBoxStreamingUrl.Text);
                timer1.Enabled = true;
            }
            else if (playbackState == StreamingPlaybackState.Paused)
            {
                playbackState = StreamingPlaybackState.Buffering;
            }*/
        }

        public void StopPlayback()
        {
            if (playbackState != StreamingPlaybackState.Stopped)
            {
                if (!fullyDownloaded)
                {
                    //webRequest.Abort();
                }

                decomp = false;
                playbackState = StreamingPlaybackState.Stopped;
                if (_audioSource != null)
                {
                    _audioSource.Stop();
                }
                // n.b. streaming thread may not yet have exited
                ShowBufferState(0, 0);
            }
        }

        private void ShowBufferState(double buffered, double total)
        {
            buffer_info = $"{buffered:0.0}s/{total:0.0}s";
        }

        public void UpdateLoop()
        {
            if (playbackState != StreamingPlaybackState.Stopped)
            {
                if (bufferedWaveProvider != null)
                {
                    double bufferedSeconds = bufferedWaveProvider.BufferedDuration.TotalSeconds;
                    ShowBufferState(bufferedSeconds, bufferedWaveProvider.BufferDuration.TotalSeconds);
                    // make it stutter less if we buffer up a decent amount before playing
                    if (bufferedSeconds < 0.5 && playbackState == StreamingPlaybackState.Playing && !fullyDownloaded)
                    {
                        Pause();
                    }
                    else if (bufferedSeconds > 3 && playbackState == StreamingPlaybackState.Buffering)
                    {
                        Play();
                    }
                    else if (fullyDownloaded && bufferedSeconds < 0.5)
                    {
                        Plugin.Log("Reached end of stream");
                        StopPlayback();
                    }
                }

            }
        }

        public void Play()
        {
            _audioSource.Play();
            // Debug.WriteLine(String.Format("Started playing, waveOut.PlaybackState={0}", waveOut.PlaybackState));
            playbackState = StreamingPlaybackState.Playing;
        }

        public void Pause()
        {
            playbackState = StreamingPlaybackState.Buffering;
            _audioSource.Pause();
            // Debug.WriteLine(String.Format("Paused to buffer, waveOut.PlaybackState={0}", waveOut.PlaybackState));
        }

        private IWavePlayer CreateWaveOut()
        {
            return new WaveOut();
        }
    }
}
