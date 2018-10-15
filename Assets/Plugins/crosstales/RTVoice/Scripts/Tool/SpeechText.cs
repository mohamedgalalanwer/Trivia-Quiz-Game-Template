using UnityEngine;

namespace Crosstales.RTVoice.Tool
{
    /// <summary>Allows to speak and store generated audio.</summary>
    //[ExecuteInEditMode]
    [HelpURL("https://www.crosstales.com/media/data/assets/rtvoice/api/class_crosstales_1_1_r_t_voice_1_1_tool_1_1_speech_text.html")]
    public class SpeechText : MonoBehaviour
    {

        #region Variables

        /// <summary>Text to speak.</summary>
        [Tooltip("Text to speak.")]
        [Multiline]
        public string Text = string.Empty;

        /// <summary>Name of the RT-Voice under Windows (optional).</summary>
        [Tooltip("Name of the RT-Voice under Windows (optional).")]
        public string RTVoiceNameWindows = "Microsoft David Desktop";

        /// <summary>Name of the RT-Voice under macOS (optional).</summary>
        [Tooltip("Name of the RT-Voice under macOS (optional).")]
        public string RTVoiceNameMac = "Alex";

        /// <summary>Name of the RT-Voice under Android.</summary>
        [Tooltip("Name of the RT-Voice under Android.")]
        public string RTVoiceNameAndroid = string.Empty;

        /// <summary>Name of the RT-Voice under iOS.</summary>
        [Tooltip("Name of the RT-Voice under iOS.")]
        public string RTVoiceNameIOS = "Daniel";

        /// <summary>Name of the RT-Voice under WSA.</summary>
        [Tooltip("Name of the RT-Voice under WSA.")]
        public string RTVoiceNameWSA = "Microsoft David Mobile";

        /// <summary>Name of the RT-Voice under MaryTTS.</summary>
        [Tooltip("Name of the RT-Voice under MaryTTS.")]
        public string RTVoiceNameMaryTTS = "cmu-rms-hsmm";

        /// <summary>Speak mode (default: 'Speak').</summary>
        [Tooltip("Speak mode (default: 'Speak').")]
        public Model.Enum.SpeakMode Mode = Model.Enum.SpeakMode.Speak;

        [Header("Optional Settings")]
        /// <summary>Fallback culture for the text (e.g. 'en', optional).</summary>
        [Tooltip("Fallback culture for the text (e.g. 'en', optional).")]
        public string Culture = "en";

        /// <summary>AudioSource for the output (optional).</summary>
        [Tooltip("AudioSource for the output (optional).")]
        public AudioSource Source;

        /// <summary>Speech rate of the speaker in percent (1 = 100%, default: 1, optional).</summary>
        [Tooltip("Speech rate of the speaker in percent (1 = 100%, default: 1, optional).")]
        [Range(0f, 3f)]
        public float Rate = 1f;

        /// <summary>Speech pitch of the speaker in percent (1 = 100%, default: 1, optional, mobile only).</summary>
        [Tooltip("Speech pitch of the speaker in percent (1 = 100%, default: 1, optional, mobile only).")]
        [Range(0f, 2f)]
        public float Pitch = 1f;

        /// <summary>Volume of the speaker in percent (1 = 100%, default: 1, optional, Windows only).</summary>
        [Tooltip("Volume of the speaker in percent (1 = 100%, default: 1, optional, Windows only).")]
        [Range(0f, 1f)]
        public float Volume = 1f;

        [Header("Behaviour Settings")]
        /// <summary>Enable speaking of the text on start (default: false).</summary>
        [Tooltip("Enable speaking of the text on start (default: false).")]
        public bool PlayOnStart = false;

        /// <summary>Delay until the speech for this text starts (default: 0).</summary>
        [Tooltip("Delay until the speech for this text starts (default: 0).")]
        public float Delay = 0f;

        [Header("Output File Settings")]
        /// <summary>Generate audio file on/off (default: false).</summary>
        [Tooltip("Generate audio file on/off (default: false).")]
        public bool GenerateAudioFile = false;

        /// <summary>File path for the generated audio.</summary>
        [Tooltip("File path for the generated audio.")]
        public string FilePath = @"_generatedAudio/";

        /// <summary>File name of the generated audio.</summary>
        [Tooltip("File name of the generated audio.")]
        public string FileName = "Speech01";

        /// <summary>Is the generated file path inside the Assets-folder (current project)? If this option is enabled, it prefixes the path with 'Application.dataPath'.</summary>
        [Tooltip("Is the generated file path inside the Assets-folder (current project)? If this option is enabled, it prefixes the path with 'Application.dataPath'.")]
        public bool FileInsideAssets = true;

        private string uid;

        private bool played = false;

        #endregion


        #region Properties

        /// <summary>Text to speak (main use is for UI).</summary>
        public string CurrentText
        {
            get
            {
                return Text;
            }

            set
            {
                Text = value;
            }
        }

        /// <summary>Fallback culture for the text (main use is for UI).</summary>
        public string CurrentCulture
        {
            get
            {
                return Culture;
            }

            set
            {
                Culture = value;
            }
        }

        /// <summary>Speech rate of the speaker in percent (main use is for UI).</summary>
        public float CurrentRate
        {
            get
            {
                return Rate;
            }

            set
            {
                Rate = value;
            }
        }

        /// <summary>Speech pitch of the speaker in percent (main use is for UI).</summary>
        public float CurrentPitch
        {
            get
            {
                return Pitch;
            }

            set
            {
                Pitch = value;
            }
        }

        /// <summary>Volume of the speaker in percent (main use is for UI).</summary>
        public float CurrentVolume
        {
            get
            {
                return Volume;
            }

            set
            {
                Volume = value;
            }
        }

        /// <summary>Returns the name of the RT-Voice for the current platform.</summary>
        /// <returns>The name of the RT-Voice for the current platform.</returns>
        public string RTVoiceName
        {
            get
            {
                string result = null;

                if (Speaker.isMaryMode)
                {
                    result = RTVoiceNameMaryTTS;
                }
                else
                {
                    if (Util.Helper.isWindowsPlatform)
                    {
                        result = RTVoiceNameWindows;
                    }
                    else if (Util.Helper.isMacOSPlatform)
                    {
                        result = RTVoiceNameMac;
                    }
                    else if (Util.Helper.isAndroidPlatform)
                    {
                        result = RTVoiceNameAndroid;
                    }
                    else if (Util.Helper.isWSAPlatform)
                    {
                        result = RTVoiceNameWSA;
                    }
                    else
                    {
                        result = RTVoiceNameIOS;
                    }
                }

                return result;
            }
        }

        #endregion


        #region MonoBehaviour methods

        public void Start()
        {
            // Subscribe event listeners
            Speaker.OnVoicesReady += onVoicesReady;

            play();
        }

        public void OnDestroy()
        {
            // Unsubscribe event listeners
            Speaker.OnVoicesReady -= onVoicesReady;
        }

        public void OnValidate()
        {
            if (Rate < 0f)
            {
                Rate = 0f;
            }

            if (Rate > 3f)
            {
                Rate = 3f;
            }

            if (Pitch < 0f)
            {
                Pitch = 0f;
            }

            if (Pitch > 2f)
            {
                Pitch = 2f;
            }

            if (Volume < 0f)
            {
                Volume = 0f;
            }

            if (Volume > 1f)
            {
                Volume = 1f;
            }

            if (!string.IsNullOrEmpty(FilePath))
            {
                FilePath = Util.Helper.ValidatePath(FilePath);
            }
        }

        #endregion


        #region Public methods

        /// <summary>Speak the text.</summary>
        public void Speak()
        {
            Silence();

            Model.Voice voice = Speaker.VoiceForName(RTVoiceName);

            if (voice == null)
            {
                voice = Speaker.VoiceForCulture(Culture);
            }

            string path = null;

            if (GenerateAudioFile && !string.IsNullOrEmpty(FilePath))
            {
                if (FileInsideAssets)
                {
                    path = Util.Helper.ValidatePath(Application.dataPath + @"/" + FilePath);
                }
                else
                {
                    path = Util.Helper.ValidatePath(FilePath);
                }

                //                if (!System.IO.Directory.Exists(path))
                //                {
                //                    System.IO.Directory.CreateDirectory(path);
                //                }

                path += FileName;
            }

            if (Util.Helper.isEditorMode)
            {
#if UNITY_EDITOR
                Speaker.SpeakNativeInEditor(Text, voice, Rate, Pitch, Volume);
                if (GenerateAudioFile)
                {
                    Speaker.GenerateInEditor(Text, voice, Rate, Pitch, Volume, path);
                }
#endif
            }
            else
            {
                if (Mode == Model.Enum.SpeakMode.Speak)
                {
                    uid = Speaker.Speak(Text, Source, voice, true, Rate, Pitch, Volume, path);
                }
                else
                {
                    uid = Speaker.SpeakNative(Text, voice, Rate, Pitch, Volume);
                }
            }
        }

        /// <summary>Silence the speech.</summary>
        public void Silence()
        {
            if (Util.Helper.isEditorMode)
            {
                Speaker.Silence();
            }
            else
            {
                Speaker.Silence(uid);
            }
        }

        #endregion


        #region Private methods

        private void play()
        {
            if (PlayOnStart && !played && Speaker.Voices.Count > 0)
            {
                played = true;

                //this.CTInvoke(() => Speak(), Delay);
                Invoke("Speak", Delay);
            }
        }

        #endregion


        #region Callbacks

        private void onVoicesReady()
        {
            play();
        }

        #endregion
    }
}
// © 2016-2018 crosstales LLC (https://www.crosstales.com)