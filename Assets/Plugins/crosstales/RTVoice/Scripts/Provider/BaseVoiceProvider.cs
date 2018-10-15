using UnityEngine;
using System.Collections;

namespace Crosstales.RTVoice.Provider
{
    /// <summary>Base class for voice providers.</summary>
    public abstract class BaseVoiceProvider
    {

        #region Variables

        //protected static System.Collections.Generic.List<Model.Voice> cachedVoices;

#if !UNITY_WSA || UNITY_EDITOR
        protected System.Collections.Generic.Dictionary<string, System.Diagnostics.Process> processes = new System.Collections.Generic.Dictionary<string, System.Diagnostics.Process>();
#endif

        protected bool silence = false;

        protected static char[] splitCharWords = new char[] { ' ' };

        protected MonoBehaviour speakerObj;

        #endregion


        #region Constructor

        /// <summary>
        /// Constructor for a VoiceProvider.
        /// </summary>
        /// <param name="obj">Instance of the speaker</param>
        public BaseVoiceProvider(MonoBehaviour obj)
        {
            speakerObj = obj;
        }

        #endregion


        #region Properties

        /// <summary>Returns the extension of the generated audio files.</summary>
        /// <returns>Extension of the generated audio files.</returns>
        public abstract string AudioFileExtension
        {
            get;
        }

        /// <summary>Get all available voices from the current TTS-provider and fills it into a given list.</summary>
        /// <returns>All available voices from the current TTS-provider as list.</returns>
        public abstract System.Collections.Generic.List<Model.Voice> Voices
        {
            get;
        }

        #endregion


        #region Events

        public delegate void VoicesReady();

        public delegate void SpeakStart(Model.Wrapper wrapper);
        public delegate void SpeakComplete(Model.Wrapper wrapper);

        public delegate void SpeakCurrentWord(Model.Wrapper wrapper, string[] speechTextArray, int wordIndex);
        public delegate void SpeakCurrentPhoneme(Model.Wrapper wrapper, string phoneme);
        public delegate void SpeakCurrentViseme(Model.Wrapper wrapper, string viseme);

        public delegate void SpeakAudioGenerationStart(Model.Wrapper wrapper);
        public delegate void SpeakAudioGenerationComplete(Model.Wrapper wrapper);

        public delegate void ErrorInfo(Model.Wrapper wrapper, string info);

        private static VoicesReady _onVoicesReady;

        private static SpeakStart _onSpeakStart;
        private static SpeakComplete _onSpeakComplete;

        private static SpeakCurrentWord _onSpeakCurrentWord;
        private static SpeakCurrentPhoneme _onSpeakCurrentPhoneme;
        private static SpeakCurrentViseme _onSpeakCurrentViseme;

        private static SpeakAudioGenerationStart _onSpeakAudioGenerationStart;
        private static SpeakAudioGenerationComplete _onSpeakAudioGenerationComplete;

        private static ErrorInfo _onErrorInfo;

        /// <summary>An event triggered whenever the voices of a provider are ready.</summary>
        public static event VoicesReady OnVoicesReady
        {
            add { _onVoicesReady += value; }
            remove { _onVoicesReady -= value; }
        }

        /// <summary>An event triggered whenever a speak is started.</summary>
        public static event SpeakStart OnSpeakStart
        {
            add { _onSpeakStart += value; }
            remove { _onSpeakStart -= value; }
        }

        /// <summary>An event triggered whenever a speak is completed.</summary>
        public static event SpeakComplete OnSpeakComplete
        {
            add { _onSpeakComplete += value; }
            remove { _onSpeakComplete -= value; }
        }

        /// <summary>An event triggered whenever a new word is spoken (native, Windows and iOS only).</summary>
        public static event SpeakCurrentWord OnSpeakCurrentWord
        {
            add { _onSpeakCurrentWord += value; }
            remove { _onSpeakCurrentWord -= value; }
        }

        /// <summary>An event triggered whenever a new phoneme is spoken (native mode, Windows only).</summary>
        public static event SpeakCurrentPhoneme OnSpeakCurrentPhoneme
        {
            add { _onSpeakCurrentPhoneme += value; }
            remove { _onSpeakCurrentPhoneme -= value; }
        }

        /// <summary>An event triggered whenever a new viseme is spoken (native mode, Windows only).</summary>
        public static event SpeakCurrentViseme OnSpeakCurrentViseme
        {
            add { _onSpeakCurrentViseme += value; }
            remove { _onSpeakCurrentViseme -= value; }
        }

        /// <summary>An event triggered whenever a speak audio generation is started.</summary>
        public static event SpeakAudioGenerationStart OnSpeakAudioGenerationStart
        {
            add { _onSpeakAudioGenerationStart += value; }
            remove { _onSpeakAudioGenerationStart -= value; }
        }

        /// <summary>An event triggered whenever a speak audio generation is completed.</summary>
        public static event SpeakAudioGenerationComplete OnSpeakAudioGenerationComplete
        {
            add { _onSpeakAudioGenerationComplete += value; }
            remove { _onSpeakAudioGenerationComplete -= value; }
        }

        /// <summary>An event triggered whenever an error occurs.</summary>
        public static event ErrorInfo OnErrorInfo
        {
            add { _onErrorInfo += value; }
            remove { _onErrorInfo -= value; }
        }

        #endregion


        #region Public methods

        /// <summary>Silence all active TTS-providers.</summary>
        public virtual void Silence()
        {
            silence = true;

#if (UNITY_STANDALONE || UNITY_EDITOR) && !UNITY_WEBPLAYER

            foreach (System.Collections.Generic.KeyValuePair<string, System.Diagnostics.Process> kvp in processes)
            {
                if (!kvp.Value.HasExited)
                {
                    kvp.Value.Kill();
                }
            }

            processes.Clear();
#endif

        }

        /// <summary>Silence the current TTS-provider (native mode).</summary>
        /// <param name="uid">UID of the speaker</param>
        public virtual void Silence(string uid)
        {
#if (UNITY_STANDALONE || UNITY_EDITOR) && !UNITY_WEBPLAYER
            if (uid != null)
            {
                if (processes.ContainsKey(uid) && !processes[uid].HasExited)
                {
                    processes[uid].Kill();
                }

                processes.Remove(uid);
            }
#endif

        }

        /// <summary>The current provider speaks a text with a given voice (native mode).</summary>
        /// <param name="wrapper">Wrapper containing the data.</param>
        public abstract IEnumerator SpeakNative(Model.Wrapper wrapper);

        /// <summary>The current provider speaks a text with a given voice.</summary>
        /// <param name="wrapper">Wrapper containing the data.</param>
        public abstract IEnumerator Speak(Model.Wrapper wrapper);

        /// <summary>The current provider generates an audio file from a text with a given voice.</summary>
        /// <param name="wrapper">Wrapper containing the data.</param>
        public abstract IEnumerator Generate(Model.Wrapper wrapper);

        #endregion


        #region Protected methods

        protected static void fileCopy(string inputFile, string outputFile, bool move = false)
        {
            if (!string.IsNullOrEmpty(outputFile))
            {
                try
                {
                    if (!System.IO.File.Exists(inputFile))
                    {
                        Debug.LogError("Input file does not exists: " + inputFile);
                    }
                    else
                    {
                        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(outputFile));

                        if (System.IO.File.Exists(outputFile))
                        {
                            if (Util.Constants.DEV_DEBUG)
                                Debug.LogWarning("Overwrite output file: " + outputFile);

                            System.IO.File.Delete(outputFile);
                        }

                        if (move)
                        {
#if (UNITY_STANDALONE || UNITY_EDITOR) && !UNITY_WEBPLAYER

                            System.IO.File.Move(inputFile, outputFile);
#else
                            System.IO.File.Copy(inputFile, outputFile);
                            System.IO.File.Delete(inputFile);
#endif
                        }
                        else
                        {
                            System.IO.File.Copy(inputFile, outputFile);
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError("Could not copy file!" + System.Environment.NewLine + ex);
                }
            }
        }

        #endregion


        #region Event-trigger methods

        protected static void onVoicesReady()
        {
            if (Util.Config.DEBUG)
                Debug.Log("onVoicesReady");

            if (_onVoicesReady != null)
            {
                _onVoicesReady();
            }
        }

        protected static void onSpeakStart(Model.Wrapper wrapper)
        {
            if (Util.Config.DEBUG)
                Debug.Log("onSpeakStart: " + wrapper);

            if (_onSpeakStart != null)
            {
                _onSpeakStart(wrapper);
            }
        }

        protected static void onSpeakComplete(Model.Wrapper wrapper)
        {
            if (Util.Config.DEBUG)
                Debug.Log("onSpeakComplete: " + wrapper);

            if (_onSpeakComplete != null)
            {
                _onSpeakComplete(wrapper);
            }
        }

        protected static void onSpeakCurrentWord(Model.Wrapper wrapper, string[] speechTextArray, int wordIndex)
        {
            if (wordIndex < speechTextArray.Length)
            {
                if (Util.Config.DEBUG)
                    Debug.Log("onSpeakCurrentWord: " + speechTextArray[wordIndex] + System.Environment.NewLine + wrapper);

                if (_onSpeakCurrentWord != null)
                {
                    _onSpeakCurrentWord(wrapper, speechTextArray, wordIndex);
                }
            }
            else
            {
                Debug.LogWarning("Word index is larger than the speech text word count: " + wordIndex + "/" + speechTextArray.Length);
            }
        }

        protected static void onSpeakCurrentPhoneme(Model.Wrapper wrapper, string phoneme)
        {
            if (Util.Config.DEBUG)
                Debug.Log("onSpeakCurrentPhoneme: " + phoneme + System.Environment.NewLine + wrapper);

            if (_onSpeakCurrentPhoneme != null)
            {
                _onSpeakCurrentPhoneme(wrapper, phoneme);
            }
        }

        protected static void onSpeakCurrentViseme(Model.Wrapper wrapper, string viseme)
        {
            if (Util.Config.DEBUG)
                Debug.Log("onSpeakCurrentViseme: " + viseme + System.Environment.NewLine + wrapper);

            if (_onSpeakCurrentViseme != null)
            {
                _onSpeakCurrentViseme(wrapper, viseme);
            }
        }

        protected static void onSpeakAudioGenerationStart(Model.Wrapper wrapper)
        {
            if (Util.Config.DEBUG)
                Debug.Log("onSpeakAudioGenerationStart: " + wrapper);

            if (_onSpeakAudioGenerationStart != null)
            {
                _onSpeakAudioGenerationStart(wrapper);
            }
        }

        protected static void onSpeakAudioGenerationComplete(Model.Wrapper wrapper)
        {
            if (Util.Config.DEBUG)
                Debug.Log("onSpeakAudioGenerationComplete: " + wrapper);

            if (_onSpeakAudioGenerationComplete != null)
            {
                _onSpeakAudioGenerationComplete(wrapper);
            }
        }

        protected static void onErrorInfo(Model.Wrapper wrapper, string info)
        {
            if (Util.Config.DEBUG)
                Debug.Log("onErrorInfo: " + info);

            if (_onErrorInfo != null)
            {
                _onErrorInfo(wrapper, info);
            }
        }

        #endregion


        #region Editor-only methods

#if UNITY_EDITOR

        /// <summary>The current provider speaks a text with a given voice (native mode & Editor only).</summary>
        /// <param name="wrapper">Wrapper containing the data.</param>
        public abstract void SpeakNativeInEditor(Model.Wrapper wrapper);

        /// <summary>Generates an audio file with the current provider (Editor only).</summary>
        /// <param name="wrapper">Wrapper containing the data.</param>
        public abstract void GenerateInEditor(Model.Wrapper wrapper);

#endif

        #endregion
    }
}
// © 2015-2018 crosstales LLC (https://www.crosstales.com)