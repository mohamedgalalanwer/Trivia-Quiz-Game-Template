using UnityEngine;
using System.Linq;

namespace Crosstales.RTVoice
{
    /// <summary>Main component of RTVoice.</summary>
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    [HelpURL("https://www.crosstales.com/media/data/assets/rtvoice/api/class_crosstales_1_1_r_t_voice_1_1_speaker.html")]
    public class Speaker : MonoBehaviour
    {

        #region Variables

        /// <summary>Enables or disables MaryTTS (default: false).</summary>
        [Tooltip("Enable or disable MaryTTS (default: false).")]
        //[HideInInspector]
        public bool MaryTTSMode = false;

        /// <summary>Server URL for MaryTTS.</summary>
        [Tooltip("Server URL for MaryTTS.")]
        //[HideInInspector]
        public string MaryTTSUrl = "http://mary.dfki.de";

        /// <summary>Server port for MaryTTS (default: 59125).</summary>
        [Tooltip("Server port for MaryTTS (default: 59125).")]
        [Range(0, 65535)]
        //[HideInInspector]
        public int MaryTTSPort = 59125;

        /// <summary>User name for MaryTTS (default: empty).</summary>
        [Tooltip("User name for MaryTTS (default: empty).")]
        //[HideInInspector]
        public string MaryTTSUser = string.Empty;

        /// <summary>User password for MaryTTS (default: empty).</summary>
        [Tooltip("User password for MaryTTS (default: empty).")]
        //[HideInInspector]
        public string MaryTTSPassword = string.Empty;

        /// <summary>Input type for MaryTTS (default: MaryTTSType.RAWMARYXML).</summary>
        [Tooltip("Input type for MaryTTS (default: MaryTTSType.RAWMARYXML).")]
        public Model.Enum.MaryTTSType MaryTTSType = Model.Enum.MaryTTSType.RAWMARYXML;

        /// <summary>Automatically clear tags from speeches depending on the capabilities of the current TTS-system (default: false).</summary>
        [Tooltip("Automatically clear tags from speeches depending on the capabilities of the current TTS-system (default: false).")]
        public bool AutoClearTags = false;

        /// <summary>Silence any speeches if this component gets disabled (default: false).</summary>
        [Tooltip("Silence any speeches if this component gets disabled (default: false).")]
        public bool SilenceOnDisable = false;

        /// <summary>Silence any speeches if the application loses the focus (default: true).</summary>
        [Tooltip("Silence any speeches if the application loses the focus (default: true).")]
        public bool SilenceOnFocustLost = true;

        /// <summary>Don't destroy gameobject during scene switches (default: true).</summary>
        [Tooltip("Don't destroy gameobject during scene switches (default: true).")]
        public bool DontDestroy = true;

        private System.Collections.Generic.Dictionary<string, AudioSource> removeSources = new System.Collections.Generic.Dictionary<string, AudioSource>();

        private float cleanUpTimer = 0f;

        private const float cleanUpTime = 5f;

        private static Provider.BaseVoiceProvider voiceProvider;
        private static Speaker speaker;
        private static bool initalized = false;

        private static System.Collections.Generic.Dictionary<string, AudioSource> genericSources = new System.Collections.Generic.Dictionary<string, AudioSource>();
        private static System.Collections.Generic.Dictionary<string, AudioSource> providedSources = new System.Collections.Generic.Dictionary<string, AudioSource>();

        private static GameObject go;

        private static bool loggedVPIsNull = false;

        private static bool loggedOnlyOneInstance = false;

        private static char[] splitCharWords = new char[] { ' ' };

        private static int speakCounter = 0;

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

        public delegate void ProviderChange(string provider);

        public delegate void ErrorInfo(Model.Wrapper wrapper, string info);


        private static VoicesReady _onVoicesReady;

        private static SpeakStart _onSpeakStart;
        private static SpeakComplete _onSpeakComplete;

        private static SpeakCurrentWord _onSpeakCurrentWord;
        private static SpeakCurrentPhoneme _onSpeakCurrentPhoneme;
        private static SpeakCurrentViseme _onSpeakCurrentViseme;

        private static SpeakAudioGenerationStart _onSpeakAudioGenerationStart;
        private static SpeakAudioGenerationComplete _onSpeakAudioGenerationComplete;

        private static ProviderChange _onProviderChange;

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

        /// <summary>An event triggered whenever a new phoneme is spoken (native, Windows only).</summary>
        public static event SpeakCurrentPhoneme OnSpeakCurrentPhoneme
        {
            add { _onSpeakCurrentPhoneme += value; }
            remove { _onSpeakCurrentPhoneme -= value; }
        }

        /// <summary>An event triggered whenever a new viseme is spoken (native, Windows only).</summary>
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

        /// <summary>An event triggered whenever a provider chamges (e.g. Windows to MaryTTS).</summary>
        public static event ProviderChange OnProviderChange
        {
            add { _onProviderChange += value; }
            remove { _onProviderChange -= value; }
        }

        /// <summary>An event triggered whenever an error occurs.</summary>
        public static event ErrorInfo OnErrorInfo
        {
            add { _onErrorInfo += value; }
            remove { _onErrorInfo -= value; }
        }

        #endregion


        #region Static properties

        /// <summary>Enables or disables MaryTTS.</summary>
        public static bool isMaryMode
        {
            get
            {
                if (speaker != null)
                {
                    return speaker.MaryTTSMode;
                }

                return false;
            }

            set
            {
                if (speaker != null && speaker.MaryTTSMode != value)
                {
                    speaker.MaryTTSMode = value;

                    ReloadProvider();
                }
            }
        }

        /// <summary>Server URL for MaryTTS.</summary>
        public static string MaryUrl
        {
            get
            {
                if (speaker != null)
                {
                    return speaker.MaryTTSUrl;
                }

                return "http://mary.dfki.de";
            }

            set
            {
                if (speaker != null)
                {
                    speaker.MaryTTSUrl = value;

                    //ReloadProvider();
                }
            }
        }

        /// <summary>Server port for MaryTTS.</summary>
        public static int MaryPort
        {
            get
            {
                if (speaker != null)
                {
                    return speaker.MaryTTSPort;
                }

                return 59125;
            }

            set
            {
                if (speaker != null)
                {
                    speaker.MaryTTSPort = value;

                    //ReloadProvider();
                }
            }
        }

        /// <summary>User name for MaryTTS.</summary>
        public static string MaryUser
        {
            get
            {
                if (speaker != null)
                {
                    return speaker.MaryTTSUser;
                }

                return string.Empty;
            }

            set
            {
                if (speaker != null)
                {
                    speaker.MaryTTSUser = value;

                    //ReloadProvider();
                }
            }
        }

        /// <summary>Password for MaryTTS.</summary>
        public static string MaryPassword
        {
            private get
            {
                if (speaker != null)
                {
                    return speaker.MaryTTSPassword;
                }

                return string.Empty;
            }

            set
            {
                if (speaker != null)
                {
                    speaker.MaryTTSPassword = value;

                    //ReloadProvider();
                }
            }
        }

        /// <summary>>Input type for MaryTTS.</summary>
        public static Model.Enum.MaryTTSType MaryType
        {
            private get
            {
                if (speaker != null)
                {
                    return speaker.MaryTTSType;
                }

                return Model.Enum.MaryTTSType.RAWMARYXML;
            }

            set
            {
                if (speaker != null)
                {
                    speaker.MaryTTSType = value;

                    //ReloadProvider();
                }
            }
        }

        /// <summary>Automatically clear tags from speeches depending on the capabilities of the current TTS-system.</summary>
        public static bool isAutoClearTags
        {
            get
            {
                if (speaker != null)
                {
                    return speaker.AutoClearTags;
                }

                return false;
            }

            set
            {
                if (speaker != null)
                {
                    speaker.AutoClearTags = value;
                }
            }
        }

        /// <summary>Silence any speeches if this component gets disabled.</summary>
        public static bool isSilenceOnDisable
        {
            get
            {
                if (speaker != null)
                {
                    return speaker.SilenceOnDisable;
                }

                return false;
            }

            set
            {
                if (speaker != null)
                {
                    speaker.SilenceOnDisable = value;
                }
            }
        }

        /// <summary>Silence any speeches if the application loses the focus.</summary>
        public static bool isSilenceOnFocustLost
        {
            get
            {
                if (speaker != null)
                {
                    return speaker.SilenceOnFocustLost;
                }

                return true;
            }

            set
            {
                if (speaker != null)
                {
                    speaker.SilenceOnFocustLost = value;
                }
            }
        }

        /// <summary>Returns the extension of the generated audio files.</summary>
        /// <returns>Extension of the generated audio files.</returns>
        public static string AudioFileExtension
        {
            get
            {
                if (voiceProvider != null)
                {
                    return voiceProvider.AudioFileExtension;
                }
                else
                {
                    logVPIsNull();
                }

                return string.Empty;
            }
        }

        /// <summary>Get all available voices from the current TTS-system.</summary>
        /// <returns>All available voices (alphabetically ordered by 'Name') as a list.</returns>
        public static System.Collections.Generic.List<Model.Voice> Voices
        {
            get
            {
                if (voiceProvider != null)
                {
                    return voiceProvider.Voices.OrderBy(s => s.Name).ToList();
                }
                else
                {
                    logVPIsNull();
                }

                return new System.Collections.Generic.List<Model.Voice>();
            }
        }

        /// <summary>Get all available cultures from the current TTS-system..</summary>
        /// <returns>All available cultures (alphabetically ordered by 'Culture') as a list.</returns>
        public static System.Collections.Generic.List<string> Cultures
        {
            get
            {
                System.Collections.Generic.List<string> result = new System.Collections.Generic.List<string>();

                if (voiceProvider != null)
                {
                    System.Collections.Generic.IEnumerable<Model.Voice> cultures = voiceProvider.Voices.GroupBy(cul => cul.Culture).Select(grp => grp.First()).OrderBy(s => s.Culture).ToList();

                    foreach (Model.Voice voice in cultures)
                    {
                        result.Add(voice.Culture);
                    }
                }
                else
                {
                    logVPIsNull();
                }

                return result;
            }
        }

        /// <summary>Checks if TTS is available on this system.</summary>
        /// <returns>True if TTS is available on this system.</returns>
        public static bool isTTSAvailable
        {
            get
            {
                if (voiceProvider != null)
                {
                    return voiceProvider.Voices.Count > 0;
                }
                else
                {
                    logVPIsNull();
                }

                return false;
            }
        }

        /// <summary>Checks if RT-Voice is speaking on this system.</summary>
        /// <returns>True if RT-Voice is speaking on this system.</returns>
        public static bool isSpeaking
        {
            get
            {
                return speakCounter > 0;
            }
        }

        #endregion


        #region MonoBehaviour methods

        public void OnEnable()
        {
            if (Util.Helper.isEditorMode || !initalized)
            {

                if (Util.Constants.DEV_DEBUG)
                    Debug.Log("Creating new 'speaker'instance");

                //                if (speaker == null)
                //                {
                speaker = this;

                go = gameObject;

                go.name = Util.Constants.RTVOICE_SCENE_OBJECT_NAME;

                initProvider();

                // Subscribe event listeners
                Provider.BaseVoiceProvider.OnVoicesReady += onVoicesReady;
                Provider.BaseVoiceProvider.OnSpeakStart += onSpeakStart;
                Provider.BaseVoiceProvider.OnSpeakComplete += onSpeakComplete;
                Provider.BaseVoiceProvider.OnSpeakCurrentWord += onSpeakCurrentWord;
                Provider.BaseVoiceProvider.OnSpeakCurrentPhoneme += onSpeakCurrentPhoneme;
                Provider.BaseVoiceProvider.OnSpeakCurrentViseme += onSpeakCurrentViseme;
                Provider.BaseVoiceProvider.OnSpeakAudioGenerationStart += onSpeakAudioGenerationStart;
                Provider.BaseVoiceProvider.OnSpeakAudioGenerationComplete += onSpeakAudioGenerationComplete;
                Provider.BaseVoiceProvider.OnErrorInfo += onErrorInfo;

                //                // Subscribe event listeners
                //                Provider.BaseVoiceProvider.OnSpeakCurrentWord += onSpeakCurrentWord;
                //                Provider.BaseVoiceProvider.OnSpeakCurrentPhoneme += onSpeakCurrentPhoneme;
                //                Provider.BaseVoiceProvider.OnSpeakCurrentViseme += onSpeakCurrentViseme;
                //                Provider.BaseVoiceProvider.OnSpeakStart += onSpeakStart;
                //                Provider.BaseVoiceProvider.OnSpeakComplete += onSpeakComplete;
                //                Provider.BaseVoiceProvider.OnSpeakAudioGenerationStart += onSpeakAudioGenerationStart;
                //                Provider.BaseVoiceProvider.OnSpeakAudioGenerationComplete += onSpeakAudioGenerationComplete;
                //                Provider.BaseVoiceProvider.OnErrorInfo += onErrorInfo;
                //                }
                //                else
                //                {
                //                    Debug.LogWarning("'speaker' wasn't null!");
                //                }

                if (!Util.Helper.isEditorMode && DontDestroy)
                {
                    DontDestroyOnLoad(transform.root.gameObject);
                    initalized = true;
                }

            }
            else
            {
                if (Util.Constants.DEV_DEBUG)
                    Debug.Log("Re-using 'speaker'instance");

                if (!Util.Helper.isEditorMode && DontDestroy && speaker != this)
                {
                    if (!loggedOnlyOneInstance)
                    {
                        Debug.LogWarning("Only one active instance of 'RTVoice' allowed in all scenes!" + System.Environment.NewLine + "This object will now be destroyed.");

                        loggedOnlyOneInstance = true;
                    }

                    Destroy(gameObject, 0.2f);
                }
            }

            if (!Util.Helper.hasBuiltInTTS)
            {
                isMaryMode = true;
            }
        }

        public void Update()
        {
            cleanUpTimer += Time.deltaTime;

            if (cleanUpTimer > cleanUpTime)
            {
                cleanUpTimer = 0f;

                if (genericSources.Count > 0)
                {
                    foreach (System.Collections.Generic.KeyValuePair<string, AudioSource> source in genericSources)
                    {
                        if (source.Value != null && source.Value.clip != null && !source.Value.isPlaying)
                        {
                            removeSources.Add(source.Key, source.Value);
                        }
                    }

                    foreach (System.Collections.Generic.KeyValuePair<string, AudioSource> source in removeSources)
                    {
                        genericSources.Remove(source.Key);
                        Destroy(source.Value);
                    }

                    removeSources.Clear();
                }

                if (providedSources.Count > 0)
                {
                    foreach (System.Collections.Generic.KeyValuePair<string, AudioSource> source in providedSources)
                    {
                        if (source.Value != null && source.Value.clip != null && !source.Value.isPlaying)
                        {
                            source.Value.clip = null; //remove clip

                            removeSources.Add(source.Key, source.Value);
                        }
                    }

                    foreach (System.Collections.Generic.KeyValuePair<string, AudioSource> source in removeSources)
                    {
                        //genericSources.Remove(source.Key);
                        providedSources.Remove(source.Key);
                    }

                    removeSources.Clear();
                }
            }

            /*
            if (MaryMode != maryTTS) //update providers
            {
                Silence();

                initProvider();
            }
*/

            if (Util.Helper.isEditorMode)
            {
                if (go != null)
                {
                    go.name = Util.Constants.RTVOICE_SCENE_OBJECT_NAME; //ensure name
                }
            }
        }

        public void OnDisable()
        {
            if (SilenceOnDisable)
                Silence();
        }

        public void OnDestroy()
        {
            if (speaker == this)
            {
                Silence();

                // if (voiceProvider != null)
                // {
                // voiceProvider.Silence();
                // }

                // Unsubscribe event listeners
                Provider.BaseVoiceProvider.OnVoicesReady -= onVoicesReady;
                Provider.BaseVoiceProvider.OnSpeakStart -= onSpeakStart;
                Provider.BaseVoiceProvider.OnSpeakComplete -= onSpeakComplete;
                Provider.BaseVoiceProvider.OnSpeakCurrentWord -= onSpeakCurrentWord;
                Provider.BaseVoiceProvider.OnSpeakCurrentPhoneme -= onSpeakCurrentPhoneme;
                Provider.BaseVoiceProvider.OnSpeakCurrentViseme -= onSpeakCurrentViseme;
                Provider.BaseVoiceProvider.OnSpeakAudioGenerationStart -= onSpeakAudioGenerationStart;
                Provider.BaseVoiceProvider.OnSpeakAudioGenerationComplete -= onSpeakAudioGenerationComplete;
                Provider.BaseVoiceProvider.OnErrorInfo -= onErrorInfo;
            }
        }

        public void OnApplicationQuit()
        {
            if (voiceProvider != null)
            {
                /*
                if (speaker != null)
                {
                    speaker.StopAllCoroutines();
                }

                voiceProvider.Silence();
*/

                if (Util.Helper.isAndroidPlatform)
                {
                    ((Provider.VoiceProviderAndroid)voiceProvider).ShutdownTTS();
                }
            }
        }

        public void OnApplicationFocus(bool hasFocus)
        {
            if (!Application.runInBackground && SilenceOnFocustLost && !hasFocus)
                Silence();
        }

        //void OnApplicationPause(bool isPaused)
        //{
        //    Debug.Log("OnApplicationPause: " + isPaused);
        //}

        //public void OnDrawGizmosSelected()
        //{
        //    Gizmos.DrawIcon(transform.position, "Radio/radio_player.png");
        //}

        #endregion


        #region Static methods

        /// <summary>
        /// Approximates the speech length in seconds of a given text and rate. 
        /// Note: This is an experimental method and doesn't provide an exact value; +/- 15% is "normal"!
        /// </summary>
        /// <param name="text">Text for the length approximation.</param>
        /// <param name="rate">Speech rate of the speaker in percent for the length approximation (1 = 100%, default: 1, optional).</param>
        /// <param name="wordsPerMinute">Words per minute (default: 175, optional).</param>
        /// <param name="timeFactor">Time factor for the calculated value (default: 0.9, optional).</param>
        /// <returns>Approximated speech length in seconds of the given text and rate.</returns>
        public static float ApproximateSpeechLength(string text, float rate = 1f, float wordsPerMinute = 175f, float timeFactor = 0.9f)
        {
            float words = (float)text.Split(splitCharWords, System.StringSplitOptions.RemoveEmptyEntries).Length;
            float characters = (float)text.Length - words + 1;
            float ratio = characters / words;

            //Debug.Log("words: " + words);
            //Debug.Log("characters: " + characters);
            //Debug.Log("ratio: " + ratio);

            if (Util.Helper.isWindowsPlatform)
            {
                if (rate != 1f)
                { //relevant?
                    if (rate > 1f)
                    { //larger than 1
                        if (rate >= 2.75f)
                        {
                            rate = 2.78f;
                        }
                        else if (rate >= 2.6f && rate < 2.75f)
                        {
                            rate = 2.6f;
                        }
                        else if (rate >= 2.35f && rate < 2.6f)
                        {
                            rate = 2.39f;
                        }
                        else if (rate >= 2.2f && rate < 2.35f)
                        {
                            rate = 2.2f;
                        }
                        else if (rate >= 2f && rate < 2.2f)
                        {
                            rate = 2f;
                        }
                        else if (rate >= 1.8f && rate < 2f)
                        {
                            rate = 1.8f;
                        }
                        else if (rate >= 1.6f && rate < 1.8f)
                        {
                            rate = 1.6f;
                        }
                        else if (rate >= 1.4f && rate < 1.6f)
                        {
                            rate = 1.45f;
                        }
                        else if (rate >= 1.2f && rate < 1.4f)
                        {
                            rate = 1.28f;
                        }
                        else if (rate > 1f && rate < 1.2f)
                        {
                            rate = 1.14f;
                        }
                    }
                    else
                    { //smaller than 1
                        if (rate <= 0.3f)
                        {
                            rate = 0.33f;
                        }
                        else if (rate > 0.3 && rate <= 0.4f)
                        {
                            rate = 0.375f;
                        }
                        else if (rate > 0.4 && rate <= 0.45f)
                        {
                            rate = 0.42f;
                        }
                        else if (rate > 0.45 && rate <= 0.5f)
                        {
                            rate = 0.47f;
                        }
                        else if (rate > 0.5 && rate <= 0.55f)
                        {
                            rate = 0.525f;
                        }
                        else if (rate > 0.55 && rate <= 0.6f)
                        {
                            rate = 0.585f;
                        }
                        else if (rate > 0.6 && rate <= 0.7f)
                        {
                            rate = 0.655f;
                        }
                        else if (rate > 0.7 && rate <= 0.8f)
                        {
                            rate = 0.732f;
                        }
                        else if (rate > 0.8 && rate <= 0.9f)
                        {
                            rate = 0.82f;
                        }
                        else if (rate > 0.9 && rate < 1f)
                        {
                            rate = 0.92f;
                        }
                    }
                }
            }

            float speechLength = words / ((wordsPerMinute / 60) * rate);

            //Debug.Log("speechLength before: " + speechLength);

            if (ratio < 2)
            {
                speechLength *= 1f;
            }
            else if (ratio >= 2f && ratio < 3f)
            {
                speechLength *= 1.05f;
            }
            else if (ratio >= 3f && ratio < 3.5f)
            {
                speechLength *= 1.15f;
            }
            else if (ratio >= 3.5f && ratio < 4f)
            {
                speechLength *= 1.2f;
            }
            else if (ratio >= 4f && ratio < 4.5f)
            {
                speechLength *= 1.25f;
            }
            else if (ratio >= 4.5f && ratio < 5f)
            {
                speechLength *= 1.3f;
            }
            else if (ratio >= 5f && ratio < 5.5f)
            {
                speechLength *= 1.4f;
            }
            else if (ratio >= 5.5f && ratio < 6f)
            {
                speechLength *= 1.45f;
            }
            else if (ratio >= 6f && ratio < 6.5f)
            {
                speechLength *= 1.5f;
            }
            else if (ratio >= 6.5f && ratio < 7f)
            {
                speechLength *= 1.6f;
            }
            else if (ratio >= 7f && ratio < 8f)
            {
                speechLength *= 1.7f;
            }
            else if (ratio >= 8f && ratio < 9f)
            {
                speechLength *= 1.8f;
            }
            else
            {
                speechLength *= ((ratio * ((ratio / 100f) + 0.02f)) + 1f);
            }

            if (speechLength < 0.8f)
            {
                speechLength += 0.6f;
            }

            //Debug.Log("speechLength after: " + speechLength);

            return speechLength * timeFactor;
        }

        /// <summary>Is a voice available for a given culture from the current TTS-system?</summary>
        /// <param name="culture">Culture of the voice (e.g. "en")</param>
        /// <returns>True if a voice is available for a given culture.</returns>
        public static bool isVoiceForCultureAvailable(string culture)
        {
            return VoicesForCulture(culture).Count > 0;
        }

        /// <summary>Get all available voices for a given culture from the current TTS-system.</summary>
        /// <param name="culture">Culture of the voice (e.g. "en")</param>
        /// <returns>All available voices (alphabetically ordered by 'Name') for a given culture as a list.</returns>
        public static System.Collections.Generic.List<Model.Voice> VoicesForCulture(string culture)
        {
            if (voiceProvider != null)
            {
                if (string.IsNullOrEmpty(culture))
                {
                    if (Util.Config.DEBUG)
                        Debug.LogWarning("The given 'culture' is null or empty! Returning all available voices.");

                    return Voices;
                }
                else
                {
#if UNITY_WSA
                    return voiceProvider.Voices.Where(s => s.Culture.StartsWith(culture, System.StringComparison.OrdinalIgnoreCase)).OrderBy(s => s.Name).ToList();
#else
                    return voiceProvider.Voices.Where(s => s.Culture.StartsWith(culture, System.StringComparison.InvariantCultureIgnoreCase)).OrderBy(s => s.Name).ToList();
#endif
                }
            }
            else
            {
                logVPIsNull();
            }

            return new System.Collections.Generic.List<Model.Voice>();
        }

        /// <summary>Get a voice from for a given culture and otional index from the current TTS-system.</summary>
        /// <param name="culture">Culture of the voice (e.g. "en_US")</param>
        /// <param name="index">Index of the voice (default: 0, optional)</param>
        /// <param name="index">Fallback culture of the voice (e.g. "en", default "", optional)</param>
        /// <returns>Voice for the given culture and index.</returns>
        public static Model.Voice VoiceForCulture(string culture, int index = 0, string fallbackCulture = "")
        {
            Model.Voice result = null;

            if (!string.IsNullOrEmpty(culture))
            {
                System.Collections.Generic.List<Model.Voice> voices = VoicesForCulture(culture);

                if (voices.Count > 0)
                {
                    if (voices.Count - 1 >= index && index >= 0)
                    {
                        result = voices[index];
                    }
                    else
                    {
                        result = voices[0];
                        Debug.LogWarning("No voices for culture '" + culture + "' with index '" + index + "' found! Speaking with the default voice!");
                    }
                }
                else
                {
                    voices = VoicesForCulture(fallbackCulture);

                    if (voices.Count > 0)
                    {
                        result = voices[0];
                        Debug.LogWarning("No voices for culture '" + culture + "' found! Speaking with the fallback culture: '" + fallbackCulture + "'");
                    }
                    else
                    {
                        //use the default voice
                        Debug.LogWarning("No voice for culture '" + culture + "' found! Speaking with the default voice!");
                    }
                }
            }

            return result;
        }

        /// <summary>Is a voice available for a given name from the current TTS-system?</summary>
        /// <param name="name">Name of the voice (e.g. "Alex")</param>
        /// <returns>True if a voice is available for a given culture.</returns>
        public static bool isVoiceForNameAvailable(string name)
        {
            return VoiceForName(name) != null;
        }

        /// <summary>Get a voice for a given name from the current TTS-system.</summary>
        /// <param name="name">Name of the voice (e.g. "Alex")</param>
        /// <returns>Voice for the given name or null if not found.</returns>
        public static Model.Voice VoiceForName(string name)
        {
            Model.Voice result = null;

            if (string.IsNullOrEmpty(name))
            {
                Debug.LogWarning("The given 'name' is null or empty! Returning null.");
            }
            else
            {
                if (voiceProvider != null)
                {
                    foreach (Model.Voice voice in voiceProvider.Voices)
                    {
                        if (name.Equals(voice.Name))
                        {
                            result = voice;
                            break;
                        }
                    }
                }
                else
                {
                    logVPIsNull();
                }
            }

            return result;
        }

        /// <summary>Speaks a text with a given voice (native mode).</summary>
        /// <param name="text">Text to speak.</param>
        /// <param name="voice">Voice to speak (optional).</param>
        /// <param name="rate">Speech rate of the speaker in percent (1 = 100%, values: 0-3, default: 1, optional).</param>
        /// <param name="pitch">Pitch of the speech in percent (1 = 100%, values: 0-2, default: 1, optional).</param>
        /// <param name="volume">Volume of the speaker in percent (1 = 100%, values: 0-1, default: 1, optional).</param>
        /// <returns>UID of the speaker.</returns>
        public static string SpeakNative(string text, Model.Voice voice = null, float rate = 1f, float pitch = 1f, float volume = 1f)
        {
            Model.Wrapper wrapper = new Model.Wrapper(text, voice, rate, pitch, volume);

            SpeakNativeWithUID(wrapper);

            return wrapper.Uid;
        }

        /// <summary>Speaks a text with a given voice (native mode).</summary>
        /// <param name="wrapper">Speak wrapper.</param>
        public static void SpeakNativeWithUID(Model.Wrapper wrapper)
        {
            if (Util.Constants.DEV_DEBUG)
                Debug.LogWarning("SpeakNativeWithUID called: " + wrapper);

            if (wrapper != null)
            {
                if (voiceProvider != null)
                {
                    if (string.IsNullOrEmpty(wrapper.Text))
                    {
                        Debug.LogWarning("'Text' is null or empty!");
                    }
                    else
                    {
                        if (speaker != null)
                        {
                            if (Util.Helper.isWSAPlatform || isMaryMode) //add an AudioSource for providers without native support
                            {
                                if (wrapper.Source == null)
                                {
                                    wrapper.Source = go.AddComponent<AudioSource>();
                                    genericSources.Add(wrapper.Uid, wrapper.Source);
                                }
                                else
                                {
                                    if (!providedSources.ContainsKey(wrapper.Uid))
                                    {
                                        providedSources.Add(wrapper.Uid, wrapper.Source);
                                    }
                                }

                                wrapper.SpeakImmediately = true; //must always speak immediately
                            }

                            speaker.StartCoroutine(voiceProvider.SpeakNative(wrapper));
                        }
                    }
                }
                else
                {
                    logVPIsNull();
                }
            }
            else
            {
                logWrapperIsNull();
            }
        }

        /// <summary>Speaks a text with a given wrapper (native mode).</summary>
        /// <param name="wrapper">Speak wrapper.</param>
        /// <returns>UID of the speaker.</returns>
        public static string SpeakNative(Model.Wrapper wrapper)
        {
            if (wrapper != null)
            {
                SpeakNativeWithUID(wrapper);

                return wrapper.Uid;
            }
            else
            {
                logWrapperIsNull();
            }

            return string.Empty;
        }

        /// <summary>Speaks a text with a given voice.</summary>
        /// <param name="text">Text to speak.</param>
        /// <param name="source">AudioSource for the output (optional).</param>
        /// <param name="voice">Voice to speak (optional).</param>
        /// <param name="speakImmediately">Speak the text immediately (default: true). Only works if 'Source' is not null.</param>
        /// <param name="rate">Speech rate of the speaker in percent (1 = 100%, values: 0-3, default: 1, optional).</param>
        /// <param name="pitch">Pitch of the speech in percent (1 = 100%, values: 0-2, default: 1, optional).</param>
        /// <param name="volume">Volume of the speaker in percent (1 = 100%, values: 0-1, default: 1, optional).</param>
        /// <param name="outputFile">Saves the generated audio to an output file (without extension, optional).</param>
        /// <returns>UID of the speaker.</returns>
        public static string Speak(string text, AudioSource source = null, Model.Voice voice = null, bool speakImmediately = true, float rate = 1f, float pitch = 1f, float volume = 1f, string outputFile = "")
        {

            Model.Wrapper wrapper = new Model.Wrapper(text, voice, rate, pitch, volume, source, speakImmediately, outputFile);

            SpeakWithUID(wrapper);

            return wrapper.Uid;
        }

        /// <summary>Speaks a text with a given voice.</summary>
        /// <param name="wrapper">Speak wrapper.</param>
        public static void SpeakWithUID(Model.Wrapper wrapper)
        {
            if (Util.Constants.DEV_DEBUG)
                Debug.LogWarning("SpeakWithUID called: " + wrapper);

            if (wrapper != null)
            {
                if (voiceProvider != null)
                {
                    if (string.IsNullOrEmpty(wrapper.Text))
                    {
                        Debug.LogWarning("'Text' is null or empty!");
                    }
                    else
                    {
                        if (speaker != null)
                        {
                            if (!Util.Helper.isIOSPlatform) //special case iOS (no audio file generation possible)
                            {
                                if (wrapper.Source == null)
                                {
                                    wrapper.Source = go.AddComponent<AudioSource>();
                                    genericSources.Add(wrapper.Uid, wrapper.Source);

                                    if (string.IsNullOrEmpty(wrapper.OutputFile))
                                    {
                                        wrapper.SpeakImmediately = true; //must always speak immediately (since there is no AudioSource given and no output file wanted)
                                    }
                                }
                                else
                                {
                                    if (!providedSources.ContainsKey(wrapper.Uid))
                                    {
                                        providedSources.Add(wrapper.Uid, wrapper.Source);
                                    }
                                }
                            }

                            speaker.StartCoroutine(voiceProvider.Speak(wrapper));
                        }
                    }
                }
                else
                {
                    logVPIsNull();
                }
            }
            else
            {
                logWrapperIsNull();
            }
        }

        /// <summary>Speaks a text with a given wrapper.</summary>
        /// <param name="wrapper">Speak wrapper.</param>
        /// <returns>UID of the speaker.</returns>
        public static string Speak(Model.Wrapper wrapper)
        {
            if (wrapper != null)
            {
                SpeakWithUID(wrapper);

                return wrapper.Uid;
            }
            else
            {
                logWrapperIsNull();
            }

            return string.Empty;
        }

        /// <summary>Speaks and marks a text with a given wrapper.</summary>
        /// <param name="wrapper">Speak wrapper.</param>
        public static void SpeakMarkedWordsWithUID(Model.Wrapper wrapper)
        {
            if (voiceProvider != null)
            {
                if (string.IsNullOrEmpty(wrapper.Text))
                {
                    Debug.LogWarning("The given 'text' is null or empty!");
                }
                else
                {
                    //AudioSource src = source;

                    if (wrapper.Source == null || wrapper.Source.clip == null)
                    {
                        Debug.LogError("'source' must be a valid AudioSource with a clip! Use 'Speak()' before!");
                    }
                    else
                    {
                        wrapper.SpeakImmediately = true;

                        if (!Util.Helper.isMacOSPlatform && !Util.Helper.isWSAPlatform && !isMaryMode) //prevent "double-speak"
                        {
                            wrapper.Volume = 0f;
                            wrapper.Source.PlayDelayed(0.1f);
                        }

                        SpeakNativeWithUID(wrapper);
                    }
                }
            }
            else
            {
                logVPIsNull();
            }
        }


        /// <summary>Speaks and marks a text with a given voice and tracks the word position.</summary>
        /// <param name="uid">UID of the speaker</param>
        /// <param name="text">Text to speak.</param>
        /// <param name="source">AudioSource for the output.</param>
        /// <param name="voice">Voice to speak (optional).</param>
        /// <param name="rate">Speech rate of the speaker in percent (1 = 100%, values: 0-3, default: 1, optional).</param>
        /// <param name="pitch">Pitch of the speech in percent (1 = 100%, values: 0-2, default: 1, optional).</param>
        public static void SpeakMarkedWordsWithUID(string uid, string text, AudioSource source, Model.Voice voice = null, float rate = 1f, float pitch = 1f)
        {
            SpeakMarkedWordsWithUID(new Model.Wrapper(uid, text, voice, rate, pitch, 0));
        }

        //      /// <summary>
        //      /// Speaks a text with a given voice and tracks the word position.
        //      /// </summary>
        //      public static Guid SpeakMarkedWords(string text, AudioSource source = null, Voice voice = null, int rate = 1, int volume = 100) {
        //         Guid result = Guid.NewGuid();
        //
        //         SpeakMarkedWordsWithUID(result, text, source, voice, rate, volume);
        //
        //         return result;
        //      }

        /// <summary>Generates an audio file from a given wrapper.</summary>
        /// <param name="wrapper">Speak wrapper.</param>
        /// <returns>UID of the generator.</returns>
        public static string Generate(Model.Wrapper wrapper)
        {
            if (wrapper != null)
            {
                if (voiceProvider != null)
                {
                    if (string.IsNullOrEmpty(wrapper.Text))
                    {
                        Debug.LogWarning("The given 'text' is null or empty!");
                    }
                    else
                    {
                        speaker.StartCoroutine(voiceProvider.Generate(wrapper));
                    }
                }
                else
                {
                    logVPIsNull();
                }

                return wrapper.Uid;
            }
            else
            {
                logWrapperIsNull();
            }

            return string.Empty;
        }


        /// <summary>Generates an audio file from a text with a given voice.</summary>
        /// <param name="text">Text to generate.</param>
        /// <param name="outputFile">Saves the generated audio to an output file (without extension).</param>
        /// <param name="voice">Voice to speak (optional).</param>
        /// <param name="rate">Speech rate of the speaker in percent (1 = 100%, values: 0-3, default: 1, optional).</param>
        /// <param name="pitch">Pitch of the speech in percent (1 = 100%, values: 0-2, default: 1, optional).</param>
        /// <param name="volume">Volume of the speaker in percent (1 = 100%, values: 0-1, default: 1, optional).</param>
        /// <returns>UID of the generator.</returns>
        public static string Generate(string text, string outputFile, Model.Voice voice = null, float rate = 1f, float pitch = 1f, float volume = 1f)
        {

            Model.Wrapper wrapper = new Model.Wrapper(text, voice, rate, pitch, volume, null, false, outputFile);

            return Generate(wrapper);
        }

        /// <summary>Silence all active TTS-voices.</summary>
        public static void Silence()
        {
            if (Util.Constants.DEV_DEBUG)
                Debug.LogWarning("Silence called");

            if (voiceProvider != null)
            {
                if (speaker != null)
                {
                    speaker.StopAllCoroutines();
                }

                voiceProvider.Silence();

                foreach (System.Collections.Generic.KeyValuePair<string, AudioSource> source in genericSources)
                {
                    if (source.Value != null)
                    {
                        source.Value.Stop();
                        Destroy(source.Value, 0.1f);
                        //DestroyImmediate(source.Value);
                    }
                }
                genericSources.Clear();

                foreach (System.Collections.Generic.KeyValuePair<string, AudioSource> source in providedSources)
                {
                    if (source.Value != null)
                    {
                        source.Value.Stop();
                    }
                }
            }
            else
            {
                providedSources.Clear();

                if (!Util.Helper.isEditorMode)
                    logVPIsNull();
            }
        }

        /// <summary>Silence an active TTS-voice with a UID.</summary>
        /// <param name="uid">UID of the speaker</param>
        public static void Silence(string uid)
        {
            if (Util.Constants.DEV_DEBUG)
                Debug.LogWarning("Silence called: " + uid);

            if (voiceProvider != null)
            {
                if (!string.IsNullOrEmpty(uid))
                {
                    if (genericSources.ContainsKey(uid))
                    {
                        AudioSource source;

                        if (genericSources.TryGetValue(uid, out source))
                        {
                            source.Stop();
                            genericSources.Remove(uid);
                        }
                    }
                    else if (providedSources.ContainsKey(uid))
                    {
                        AudioSource source;

                        if (providedSources.TryGetValue(uid, out source))
                        {
                            source.Stop();
                            providedSources.Remove(uid);
                        }
                    }
                    else
                    {
                        voiceProvider.Silence(uid);
                    }
                }
            }
            else
            {
                logVPIsNull();
            }
        }

        public static void ReloadProvider()
        {
            Silence();
            initProvider();
        }

        #endregion


        #region Private methods

        private static void initProvider()
        {
            if (isMaryMode)
            {
                //                if (Tool.InternetCheck.isInternetAvailable)
                //                {
                if (MaryUrl.Contains("mary.dfki.de") || MaryUser.Contains("rtvdemo"))
                {
                    if (Util.Helper.isEditor)
                    {
                        Debug.LogWarning("You are using the test server of MaryTTS. Please request an account for our service at 'rtvoice@crosstales' or setup your own server from 'http://mary.dfki.de'.");

                        voiceProvider = new Provider.VoiceProviderMary(speaker, MaryUrl, MaryPort, MaryUser, MaryPassword, MaryType);
                    }
                    else
                    {
#if rtv_demo
                        voiceProvider = new Provider.VoiceProviderMary(speaker, MaryUrl, MaryPort, MaryUser, MaryPassword, MaryType);
#else
                            Debug.LogError("You are using the test server of MaryTTS - this is not allowed in builds! Please request an account for our service at 'rtvoice@crosstales' or setup your own server from 'http://mary.dfki.de'.");

                            isMaryMode = false;

                            initOSProvider();
#endif
                    }
                }
                else
                {
                    voiceProvider = new Provider.VoiceProviderMary(speaker, MaryUrl, MaryPort, MaryUser, MaryPassword, MaryType);
                }
                //                }
                //                else
                //                {
                //                    Debug.LogWarning("Internet is not available - can't use MaryTTS and enable OS provider as fallback.");
                //
                //                    //maryTTS = MaryMode = false;
                //                  MaryMode = false;
                //
                //                    initOSProvider();
                //                }
            }
            else
            {
                initOSProvider();
            }

            if (_onProviderChange != null)
            {
                _onProviderChange(voiceProvider.GetType().ToString());
            }
        }

        private static void initOSProvider()
        {
            if (Util.Helper.isWindowsPlatform)
            {
                voiceProvider = new Provider.VoiceProviderWindows(speaker);
            }
            else if (Util.Helper.isAndroidPlatform)
            {
                voiceProvider = new Provider.VoiceProviderAndroid(speaker);
            }
            else if (Util.Helper.isIOSPlatform)
            {
                voiceProvider = new Provider.VoiceProviderIOS(speaker);
            }
            else if (Util.Helper.isWSAPlatform)
            {
                voiceProvider = new Provider.VoiceProviderWSA(speaker);
            }
            else
            { // always add a default provider
                voiceProvider = new Provider.VoiceProviderMacOS(speaker);
            }
        }

        private static void logWrapperIsNull()
        {
            //if (!loggedWrapperIsNull) {
            string errorMessage = "'wrapper' is null!";

            onErrorInfo(null, errorMessage);

            //            if (OnErrorInfo != null)
            //            {
            //                OnErrorInfo(null, errorMessage);
            //            }

            Debug.LogError(errorMessage);

            //}
        }

        private static void logVPIsNull()
        {
            string errorMessage = "'voiceProvider' is null!" + System.Environment.NewLine + "Did you add the 'RTVoice'-prefab to the current scene?";

            onErrorInfo(null, errorMessage);

            //          if (_onErrorInfo != null)
            //            {
            //              _onErrorInfo(null, errorMessage);
            //            }

            if (!loggedVPIsNull)
            {
                Debug.LogWarning(errorMessage);
                loggedVPIsNull = true;
            }
        }

        #endregion


        #region Event-trigger methods

        private static void onVoicesReady()
        {
            if (_onVoicesReady != null)
            {
                _onVoicesReady();
            }
        }

        private static void onSpeakStart(Model.Wrapper wrapper)
        {
            if (_onSpeakStart != null)
            {
                _onSpeakStart(wrapper);
            }

            speakCounter++;
        }

        private static void onSpeakComplete(Model.Wrapper wrapper)
        {
            if (_onSpeakComplete != null)
            {
                _onSpeakComplete(wrapper);
            }

            speakCounter--;
        }

        private static void onSpeakCurrentWord(Model.Wrapper wrapper, string[] speechTextArray, int wordIndex)
        {
            if (_onSpeakCurrentWord != null)
            {
                _onSpeakCurrentWord(wrapper, speechTextArray, wordIndex);
            }
        }

        private static void onSpeakCurrentPhoneme(Model.Wrapper wrapper, string phoneme)
        {
            if (_onSpeakCurrentPhoneme != null)
            {
                _onSpeakCurrentPhoneme(wrapper, phoneme);
            }
        }

        private static void onSpeakCurrentViseme(Model.Wrapper wrapper, string viseme)
        {
            if (_onSpeakCurrentViseme != null)
            {
                _onSpeakCurrentViseme(wrapper, viseme);
            }
        }

        private static void onSpeakAudioGenerationStart(Model.Wrapper wrapper)
        {
            if (_onSpeakAudioGenerationStart != null)
            {
                _onSpeakAudioGenerationStart(wrapper);
            }
        }

        private static void onSpeakAudioGenerationComplete(Model.Wrapper wrapper)
        {
            if (_onSpeakAudioGenerationComplete != null)
            {
                _onSpeakAudioGenerationComplete(wrapper);
            }
        }

        private static void onErrorInfo(Model.Wrapper wrapper, string errorInfo)
        {
            if (_onErrorInfo != null)
            {
                _onErrorInfo(wrapper, errorInfo);
            }
        }

        #endregion


        #region Editor-only methods

#if UNITY_EDITOR

        /// <summary>Speaks a text with a given voice (native mode & Editor only).</summary>
        /// <param name="text">Text to speak.</param>
        /// <param name="voice">Voice to speak (optional).</param>
        /// <param name="rate">Speech rate of the speaker in percent (1 = 100%, default: 1, optional).</param>
        /// <param name="pitch">Pitch of the speech (1 = 100%, default: 1, optional).</param>
        /// <param name="volume">Volume of the speaker in percent (1 = 100%, default: 1, optional).</param>
        /// <returns>UID of the generator.</returns>
        public static string SpeakNativeInEditor(string text, Model.Voice voice = null, float rate = 1f, float pitch = 1f, float volume = 1f)
        {
            if (Util.Helper.isEditorMode)
            {
                if (voiceProvider != null)
                {
                    Model.Wrapper wrapper = new Model.Wrapper(text, voice, rate, pitch, volume);

                    SpeakNativeInEditor(wrapper);

                    return wrapper.Uid;
                }
                else
                {
                    logVPIsNull();
                }
            }
            else
            {
                Debug.LogWarning("'SpeakNativeInEditor()' works only inside the Unity Editor!");
            }

            return string.Empty;
        }

        /// <summary>Speaks a text with a given voice (native mode & Editor only).</summary>
        /// <param name="wrapper">Speak wrapper.</param>
        /// <returns>UID of the generator.</returns>
        public static string SpeakNativeInEditor(Model.Wrapper wrapper)
        {
            if (Util.Helper.isEditorMode)
            {
                if (voiceProvider != null)
                {
                    if (string.IsNullOrEmpty(wrapper.Text))
                    {
                        Debug.LogWarning("'Text' is null or empty!");
                    }
                    else
                    {
                        System.Threading.Thread worker = new System.Threading.Thread(() => voiceProvider.SpeakNativeInEditor(wrapper));
                        worker.Start();
                    }

                    return wrapper.Uid;
                }
                else
                {
                    logVPIsNull();
                }
            }
            else
            {
                Debug.LogWarning("'SpeakNativeInEditor()' works only inside the Unity Editor!");
            }

            return string.Empty;
        }

        /// <summary>Generates audio for a text with a given voice (Editor only).</summary>
        /// <param name="text">Text to speak.</param>
        /// <param name="voice">Voice to speak (optional).</param>
        /// <param name="rate">Speech rate of the speaker in percent (1 = 100%, default: 1, optional).</param>
        /// <param name="pitch">Pitch of the speech (1 = 100%, default: 1, optional).</param>
        /// <param name="volume">Volume of the speaker in percent (1 = 100%, default: 1, optional).</param>
        /// <param name="outputFile">Saves the generated audio to an output file (without extension, optional).</param>
        /// <returns>UID of the generator.</returns>
        public static string GenerateInEditor(string text, Model.Voice voice = null, float rate = 1f, float pitch = 1f, float volume = 1f, string outputFile = "")
        {
            if (Util.Helper.isEditorMode)
            {
                if (voiceProvider != null)
                {
                    Model.Wrapper wrapper = new Model.Wrapper(text, voice, rate, pitch, volume, null, true, outputFile);

                    GenerateInEditor(wrapper);

                    return wrapper.Uid;
                }
                else
                {
                    logVPIsNull();
                }
            }
            else
            {
                Debug.LogWarning("'GenerateInEditor()' works only inside the Unity Editor!");
            }

            return string.Empty;
        }

        /// <summary>Generates audio for a text with a given voice (Editor only).</summary>
        /// <param name="wrapper">Speak wrapper.</param>
        /// <returns>UID of the generator.</returns>
        public static string GenerateInEditor(Model.Wrapper wrapper)
        {
            if (Util.Helper.isEditorMode)
            {
                if (voiceProvider != null)
                {
                    if (string.IsNullOrEmpty(wrapper.Text))
                    {
                        Debug.LogWarning("'Text' is null or empty!");
                    }
                    else
                    {
                        System.Threading.Thread worker = new System.Threading.Thread(() => voiceProvider.GenerateInEditor(wrapper));
                        worker.Start();
                    }

                    return wrapper.Uid;
                }
                else
                {
                    logVPIsNull();
                }
            }
            else
            {
                Debug.LogWarning("'GenerateInEditor()' works only inside the Unity Editor!");
            }

            return string.Empty;
        }

#endif

        #endregion
    }
}
// © 2015-2018 crosstales LLC (https://www.crosstales.com)