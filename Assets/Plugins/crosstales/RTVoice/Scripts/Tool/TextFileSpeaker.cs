using UnityEngine;

namespace Crosstales.RTVoice.Tool
{
    /// <summary>Allows to speak text files.</summary>
    //[ExecuteInEditMode]
    [HelpURL("https://www.crosstales.com/media/data/assets/rtvoice/api/class_crosstales_1_1_r_t_voice_1_1_tool_1_1_text_file_speaker.html")]
    public class TextFileSpeaker : MonoBehaviour
    {
        #region Variables

        /// <summary>Text files to speak.</summary>
        [Tooltip("Text files to speak.")]
        public TextAsset[] TextFiles;

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

        [Header("Behaviour Settings")]
        /// <summary>Enable speaking of a random text file on start (default: false).</summary>
        [Tooltip("Enable speaking of a random text file on start (default: false).")]
        public bool PlayOnStart = false;

        /*
		/// <summary>Play the radio stations in random order(default: false).</summary>
		[Tooltip("Play the radio stations in random order (default: false).")]
		public bool PlayRandom = false;
		*/

        /// <summary>Delay until the speech for this text starts (default: 0).</summary>
        [Tooltip("Delay until the speech for this text starts (default: 0).")]
        public float Delay = 0f;

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

        //private string[] texts;

        //private Voice voice;

        private static System.Random rnd = new System.Random();

        private string uid;

        private bool played = false;

        #endregion


        #region Properties

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
        }

        #endregion


        #region Public methods

        /*
		 
		/// <summary>Plays a radio (main use is for UI).</summary>
		public void Play()
		{
			Play(PlayRandom);
		}

		/// <summary>Plays a (normal/random) radio.</summary>
		/// <param name="random">Play a random radio station (default: false, optional)</param>
		/// <param name="filter">Filter (default: null, optional)</param>
		public void Play(bool random, Model.RadioFilter filter = null)
		{
			if (Player != null && Manager != null)
			{
				if (lastPlaytime + Util.Constants.PLAY_CALL_SPEED < Time.realtimeSinceStartup)
				{
					lastPlaytime = Time.realtimeSinceStartup;

					Stop();

					//Player.Station = Manager.NextStation(random, getFilter(filter));

					if (Util.Helper.isEditorMode)
					{
						#if UNITY_EDITOR
						Player.PlayInEditor();
						#endif
					}
					else
					{
						//this.CTInvoke(() => play(), Util.Constants.INVOKE_DELAY);
						Invoke("play", Util.Constants.INVOKE_DELAY);
					}
				}
				else
				{
					Debug.LogWarning("Play called to fast - please slow down.");
				}
			}
		}

		/// <summary>Plays the next radio (main use for UI).</summary>
		public void Next()
		{
			Next(PlayRandom);
		}

		/// <summary>Plays the next (normal/random) radio.</summary>
		/// <param name="random">Play a random radio station (default: false, optional)</param>
		/// <param name="filter">Filter (default: null, optional)</param>
		public void Next(bool random, Model.RadioFilter filter = null)
		{
			Player.Station = Manager.NextStation(random, getFilter(filter));

			Play(random, getFilter(filter));
		}

		/// <summary>Plays the previous radio (main use for UI).</summary>
		public void Previous()
		{
			Previous(PlayRandom);
		}

		/// <summary>Plays the previous radio.</summary>
		/// <param name="random">Play a random radio station (default: false, optional)</param>
		/// <param name="filter">Filter (default: null, optional)</param>
		public void Previous(bool random, Model.RadioFilter filter = null)
		{
			if (Player != null && Manager != null)
			{
				//                Stop();

				Player.Station = Manager.PreviousStation(random, getFilter(filter));

				Play(random, getFilter(filter));

			}
		}

		*/

        /// <summary>Speaks a random text.</summary>
        public void Speak()
        {
            Silence();
            uid = SpeakText();
        }

        /// <summary>Speaks a text with an optional index.</summary>
        /// <param name="index">Index of the text (default: -1 (random), optional).</param>
        /// <returns>UID of the speaker.</returns>
        public string SpeakText(int index = -1)
        {
            string[] texts = new string[TextFiles.Length];
            string result = null;

            for (int ii = 0; ii < TextFiles.Length; ii++)
            {
                if (TextFiles[ii] != null)
                {
                    texts[ii] = TextFiles[ii].text;
                }
                else
                {
                    texts[ii] = string.Empty;
                }
            }

            Model.Voice voice = Speaker.VoiceForName(RTVoiceName);

            if (voice == null)
            {
                voice = Speaker.VoiceForCulture(Culture);
            }

            if (texts.Length > 0)
            {
                if (index < 0)
                {
                    if (Util.Helper.isEditorMode)
                    {
#if UNITY_EDITOR
                        Speaker.SpeakNativeInEditor(texts[rnd.Next(texts.Length)], voice, Rate, Pitch, Volume);
#endif
                    }
                    else
                    {
                        if (Mode == Model.Enum.SpeakMode.Speak)
                        {
                            result = Speaker.Speak(texts[rnd.Next(texts.Length)], Source, voice, true, Rate, Pitch, Volume);
                        }
                        else
                        {
                            result = Speaker.SpeakNative(texts[rnd.Next(texts.Length)], voice, Rate, Pitch, Volume);
                        }
                    }

                }
                else
                {
                    if (index < texts.Length)
                    {
                        if (Util.Helper.isEditorMode)
                        {
#if UNITY_EDITOR
                            Speaker.SpeakNativeInEditor(texts[index], voice, Rate, Pitch, Volume);
#endif
                        }
                        else
                        {
                            if (Mode == Model.Enum.SpeakMode.Speak)
                            {
                                result = Speaker.Speak(texts[index], Source, voice, true, Rate, Pitch, Volume);
                            }
                            else
                            {
                                result = Speaker.SpeakNative(texts[index], voice, Rate, Pitch, Volume);
                            }
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Text file index is out of bounds: " + index + " - maximal index is: " + (texts.Length - 1));
                    }
                }
            }
            else
            {
                Debug.LogError("No text files added - speak cancelled!");
            }

            return result;
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