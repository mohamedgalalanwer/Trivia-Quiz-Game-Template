using UnityEngine;
using System.Collections;

namespace Crosstales.RTVoice.Provider
{

    /// <summary>iOS voice provider.</summary>
    public class VoiceProviderIOS : BaseVoiceProvider
    {
        #region Variables

        private static readonly System.Collections.Generic.List<Model.Voice> cachedVoices = new System.Collections.Generic.List<Model.Voice>(60);

        private const string extension = "none";

#if (UNITY_IOS || UNITY_EDITOR) && !UNITY_WEBPLAYER

        private static string[] speechTextArray;

        private static int wordIndex = 0;

        private static bool isWorking = false;

        private static Model.Wrapper wrapperNative;
#endif

        #endregion


        #region Constructor

        /// <summary>
        /// Constructor for VoiceProviderIOS.
        /// </summary>
        /// <param name="obj">Instance of the speaker</param>
        public VoiceProviderIOS(MonoBehaviour obj) : base(obj)
        {
            /*
            cachedVoices.Clear ();

            cachedVoices.Add(new Model.Voice("Alice", "it-IT", "it-IT"));
            cachedVoices.Add(new Model.Voice("Alva", "sv-SE", "sv-SE"));
            cachedVoices.Add(new Model.Voice("Amelie", "fr-CA", "fr-CA"));
            cachedVoices.Add(new Model.Voice("Anna ", "de-DE", "de-DE"));
            cachedVoices.Add(new Model.Voice("Damayanti", "id-ID", "id-ID"));
            cachedVoices.Add(new Model.Voice("Daniel", "en-GB", "en-GB"));
            cachedVoices.Add(new Model.Voice("Ellen", "nl-BE", "nl-BE"));
            cachedVoices.Add(new Model.Voice("Ioana", "ro-RO", "ro-RO"));
            cachedVoices.Add(new Model.Voice("Joana", "pt-PT", "pt-PT"));
            cachedVoices.Add(new Model.Voice("Kanya", "th-TH", "th-TH"));
            cachedVoices.Add(new Model.Voice("Karen", "en-AU", "en-AU"));
            cachedVoices.Add(new Model.Voice("Kyoko", "ja-JP", "ja-JP"));
            cachedVoices.Add(new Model.Voice("Laura", "sk-SK", "sk-SK"));
            cachedVoices.Add(new Model.Voice("Lekha", "hi-IN", "hi-IN"));
            cachedVoices.Add(new Model.Voice("Luciana", "pt-BR", "pt-BR"));
            cachedVoices.Add(new Model.Voice("Maged", "ar-SA", "ar-SA"));
            cachedVoices.Add(new Model.Voice("Mariska", "hu-HU", "hu-HU"));
            cachedVoices.Add(new Model.Voice("Mei-Jia", "zh-TW", "zh-TW"));
            cachedVoices.Add(new Model.Voice("Melina", "el-GR", "el-GR"));
            cachedVoices.Add(new Model.Voice("Milena", "ru-RU", "ru-RU"));
            cachedVoices.Add(new Model.Voice("Moira", "en-IE", "en-IE"));
            cachedVoices.Add(new Model.Voice("Monica", "es-ES", "es-ES"));
            cachedVoices.Add(new Model.Voice("Nora", "no-NO", "no-NO"));
            cachedVoices.Add(new Model.Voice("Paulina", "es-MX", "es-MX"));
            cachedVoices.Add(new Model.Voice("Samantha", "en-US", "en-US"));
            cachedVoices.Add(new Model.Voice("Sara ", "da-DK", "da-DK"));
            cachedVoices.Add(new Model.Voice("Satu ", "fi-FI", "fi-FI"));
            cachedVoices.Add(new Model.Voice("Sin-Ji", "zh-HK", "zh-HK"));
            cachedVoices.Add(new Model.Voice("Tessa", "en-ZA", "en-ZA"));
            cachedVoices.Add(new Model.Voice("Thomas", "fr-FR", "fr-FR"));
            cachedVoices.Add(new Model.Voice("Ting-Ting", "zh-CN", "zh-CN"));
            cachedVoices.Add(new Model.Voice("Xander", "nl-NL", "nl-NL"));
            cachedVoices.Add(new Model.Voice("Yelda", "tr-TR", "tr-TR"));
            cachedVoices.Add(new Model.Voice("Yuna ", "ko-KR", "ko-KR"));
            cachedVoices.Add(new Model.Voice("Zosia", "pl-PL", "pl-PL"));
            cachedVoices.Add(new Model.Voice("Zuzana", "cs-CZ", "cs-CZ"));
*/

#if (UNITY_IOS || UNITY_EDITOR) && !UNITY_WEBPLAYER
            GetVoices();
#endif
        }

        #endregion


        #region Bridge declaration and methods

#if (UNITY_IOS || UNITY_EDITOR) && !UNITY_WEBPLAYER

        /// <summary>Silence the current TTS-provider (native mode).</summary>
        [System.Runtime.InteropServices.DllImport("__Internal")]
        extern static public void Stop();

        /// <summary>Silence the current TTS-provider (native mode).</summary>
        [System.Runtime.InteropServices.DllImport("__Internal")]
        extern static public void GetVoices();

        /// <summary>Bridge to the native tts system</summary>
        /// <param name="name">Name of the voice to speak.</param>
        /// <param name="text">Text to speak.</param>
        /// <param name="rate">Speech rate of the speaker in percent (default: 1, optional).</param>
        /// <param name="pitch">Pitch of the speech in percent (default: 1, optional).</param>
        /// <param name="volume">Volume of the speaker in percent (default: 1, optional).</param>
        [System.Runtime.InteropServices.DllImport("__Internal")]
        extern static public void Speak(string name, string text, float rate = 1f, float pitch = 1f, float volume = 1f);

        /*
                /// <summary>Bridge to the native tts system</summary>
                /// <param name="id">Identifier of the voice to speak.</param>
                /// <param name="text">Text to speak.</param>
                /// <param name="rate">Speech rate of the speaker in percent (default: 1, optional).</param>
                /// <param name="pitch">Pitch of the speech in percent (default: 1, optional).</param>
                /// <param name="volume">Volume of the speaker in percent (default: 1, optional).</param>
                [System.Runtime.InteropServices.DllImport("__Internal")]
                extern static public void Speak(string id, string text, float rate = 1f, float pitch = 1f, float volume = 1f);
        */

#endif

        /// <summary>Receives all voices</summary>
        /// <param name="voicesText">All voices as text string.</param>
        public static void SetVoices(string voicesText)
        {
#if (UNITY_IOS || UNITY_EDITOR) && !UNITY_WEBPLAYER
            string[] voices = voicesText.Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);

            if (voices.Length % 2 == 0)
            //if (voices.Length % 3 == 0)
            {
                //string id;
                string name;
                string culture;
                Model.Voice newVoice;

                cachedVoices.Clear();

                for (int ii = 0; ii < voices.Length; ii += 2)
                //for (int ii = 0; ii < voices.Length; ii += 3)
                {
                    name = voices[ii];
                    culture = voices[ii + 1];
                    //id = voices[ii];
                    //name = voices[ii + 1];
                    //culture = voices[ii + 2];
                    name = voices[ii];
                    culture = voices[ii + 1];
                    newVoice = new Model.Voice(name, "iOS voice: " + name + " " + culture, culture);
                    //newVoice = new Model.Voice(id, name, "iOS voice: " + name + " " + culture, culture);

                    cachedVoices.Add(newVoice);
                }

                if (Util.Constants.DEV_DEBUG)
                    Debug.Log("Voices read: " + cachedVoices.CTDump());

                //onVoicesReady();
            }
            else
            {
                Debug.LogWarning("Voice-string contains wrong number of elements!");
            }
#endif

            onVoicesReady();
        }

        /// <summary>Receives the state of the speaker.</summary>
        /// <param name="state">The state of the speaker.</param>
        public static void SetState(string state)
        {
#if (UNITY_IOS || UNITY_EDITOR) && !UNITY_WEBPLAYER
            if (state.Equals("Start"))
            {
                // do nothing
            }
            else if (state.Equals("Finsish"))
            {
                isWorking = false;
            }
            else
            { //cancel
                isWorking = false;
            }
#endif
        }

        /// <summary>Called everytime a new word is spoken.</summary>
        public static void WordSpoken()
        {
#if (UNITY_IOS || UNITY_EDITOR) && !UNITY_WEBPLAYER
            if (wrapperNative != null)
            {
                onSpeakCurrentWord(wrapperNative, speechTextArray, wordIndex);
                wordIndex++;
            }
#endif
        }

        #endregion


        #region Implemented methods

        public override string AudioFileExtension
        {
            get
            {
                return extension;
            }
        }


        public override System.Collections.Generic.List<Model.Voice> Voices
        {
            get
            {
                return cachedVoices;
            }
        }

        public override IEnumerator SpeakNative(Model.Wrapper wrapper)
        {
            yield return speak(wrapper, true);
        }

        public override IEnumerator Speak(Model.Wrapper wrapper)
        {
            yield return speak(wrapper, false);
        }

        public override IEnumerator Generate(Model.Wrapper wrapper)
        {
            Debug.LogError("Generate is not supported for iOS!");
            yield return null;
        }

        public override void Silence()
        {
            silence = true;

#if (UNITY_IOS || UNITY_EDITOR) && !UNITY_WEBPLAYER
            Stop();
#endif
        }

        #endregion


        #region Private methods

        private IEnumerator speak(Model.Wrapper wrapper, bool isNative)
        {

#if (UNITY_IOS || UNITY_EDITOR) && !UNITY_WEBPLAYER
            if (wrapper == null)
            {
                Debug.LogWarning("'wrapper' is null!");
            }
            else
            {
                if (string.IsNullOrEmpty(wrapper.Text))
                {
                    Debug.LogWarning("'Text' is null or empty!");
                    //yield return null;
                }
                else
                {
                    yield return null; //return to the main process (uid)

                    string voiceName = getVoiceName(wrapper);
                    //string voiceId = getVoiceId(wrapper);

                    silence = false;

                    if (!isNative)
                    {
                        onSpeakAudioGenerationStart(wrapper); //just a fake event if some code needs the feedback...
                    }

                    onSpeakStart(wrapper);
                    isWorking = true;

                    speechTextArray = Util.Helper.CleanText(wrapper.Text, false).Split(splitCharWords, System.StringSplitOptions.RemoveEmptyEntries);
                    wordIndex = 0;
                    wrapperNative = wrapper;

                    Speak(voiceName, wrapper.Text, calculateRate(wrapper.Rate), wrapper.Pitch, wrapper.Volume);
                    //Speak(voiceId, wrapper.Text, calculateRate(wrapper.Rate), wrapper.Pitch, wrapper.Volume);

                    do
                    {
                        yield return null;
                    } while (isWorking && !silence);

                    if (Util.Config.DEBUG)
                        Debug.Log("Text spoken: " + wrapper.Text);

                    wrapperNative = null;
                    onSpeakComplete(wrapper);

                    if (!isNative)
                    {
                        onSpeakAudioGenerationComplete(wrapper); //just a fake event if some code needs the feedback...
                    }
                }
            }
#else
            yield return null;
#endif
        }

        private static float calculateRate(float rate)
        {
            float result = rate;

            if (rate > 1f)
            {
                //result = (rate + 1f) * 0.5f;
                result = 1f + (rate - 1f) * 0.25f;
            }

            if (Util.Constants.DEV_DEBUG)
                Debug.Log("calculateRate: " + result + " - " + rate);

            return result;
        }

        private string getVoiceName(Model.Wrapper wrapper)
        {
            if (wrapper.Voice == null || string.IsNullOrEmpty(wrapper.Voice.Name))
            {
                if (Util.Config.DEBUG)
                    Debug.LogWarning("'Voice' or 'Voice.Name' is null! Using the OS 'default' voice.");

                if (Voices.Count > 0)
                {
                    //always use English as fallback
                    return Speaker.VoiceForCulture("en-US").Name;
                }

                return "Samantha";
            }
            else
            {
                return wrapper.Voice.Name;
            }
        }
        /*
                private string getVoiceId(Model.Wrapper wrapper)
                {
                    if (wrapper.Voice == null || string.IsNullOrEmpty(wrapper.Voice.Identifier))
                    {
                        if (Util.Config.DEBUG)
                            Debug.LogWarning("'Voice' or 'Voice.Identifier' is null! Using the OS 'default' voice.");

                        if (Voices.Count > 0)
                        {
                            //always use English as fallback
                            return Speaker.VoiceForCulture("en-US").Identifier;
                        }

                        return "Samantha"; //TODO change!
                    }
                    else
                    {
                        return wrapper.Voice.Identifier;
                    }
                }
         */
        #endregion


        #region Editor-only methods

#if UNITY_EDITOR

        public override void GenerateInEditor(Model.Wrapper wrapper)
        {
            Debug.LogError("GenerateInEditor is not supported for Unity iOS!");
        }

        public override void SpeakNativeInEditor(Model.Wrapper wrapper)
        {
            Debug.LogError("SpeakNativeInEditor is not supported for Unity iOS!");
        }
#endif

        #endregion
    }
}
// © 2016-2018 crosstales LLC (https://www.crosstales.com)