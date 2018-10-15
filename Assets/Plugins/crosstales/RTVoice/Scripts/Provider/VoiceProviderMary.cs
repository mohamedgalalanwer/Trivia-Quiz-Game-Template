using UnityEngine;
using System.Collections;

namespace Crosstales.RTVoice.Provider
{
    /// <summary>MaryTTS voice provider.</summary>
    public class VoiceProviderMary : BaseVoiceProvider
    {

        #region Variables

        private static readonly System.Collections.Generic.List<Model.Voice> cachedVoices = new System.Collections.Generic.List<Model.Voice>(50);

        private const string extension = ".wav";

        private string uri;

        private byte[] rawData;

        private System.Collections.Generic.Dictionary<string, string> headers;

        private Model.Enum.MaryTTSType type;

        #endregion


        #region Constructor

        /// <summary>
        /// Constructor for VoiceProviderMary. Needed to pass IP and Port of the MaryTTS server to the Provider.
        /// </summary>
        /// <param name="obj">Instance of the speaker</param>
        /// <param name="url">IP-Address of the MaryTTS-server</param>
        /// <param name="port">Port to connect to on the MaryTTS-server</param>
        public VoiceProviderMary(MonoBehaviour obj, string url, int port, string user, string password, Model.Enum.MaryTTSType type) : base(obj)
        {
            this.type = type;

            uri = Util.Helper.CleanUrl(url, false, false) + ":" + port;

            WWWForm form = new WWWForm();
            form.AddField("k", "v");
            headers = form.headers;
            rawData = form.data;

            if (!string.IsNullOrEmpty(user))
            {
                headers["Authorization"] = "Basic " + System.Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(user + ":" + password));
            }

            if (Util.Helper.isEditorMode)
            {
#if UNITY_EDITOR
                getVoicesInEditor();
#endif
            }
            else
            {
                speakerObj.StartCoroutine(getVoices());
            }
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
            if (wrapper == null)
            {
                Debug.LogWarning("'wrapper' is null!");
            }
            else
            {
                if (string.IsNullOrEmpty(wrapper.Text))
                {
                    Debug.LogWarning("'Text' is null or empty: " + wrapper);
                    //yield return null;
                }
                else
                {
                    if (!Util.Helper.isInternetAvailable)
                    {
                        string errorMessage = "Internet is not available - can't use MaryTTS right now!";
                        Debug.LogError(errorMessage);
                        onErrorInfo(wrapper, errorMessage);
                    }
                    else
                    {
                        yield return null; //return to the main process (uid)

                        string voiceCulture = getVoiceCulture(wrapper);
                        string voiceName = getVoiceName(wrapper);

                        silence = false;

                        onSpeakAudioGenerationStart(wrapper);

                        System.Text.StringBuilder sbXML = new System.Text.StringBuilder();
                        string request = null;

                        if (type == Model.Enum.MaryTTSType.RAWMARYXML)
                        {
                            //RAWMARYXML
                            sbXML.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" ?>");
                            sbXML.Append("<maryxml version=\"0.5\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://mary.dfki.de/2002/MaryXML\" xml:lang=\"");
                            sbXML.Append(voiceCulture);
                            sbXML.Append("\">");

                            sbXML.Append(prepareProsody(wrapper.Text, wrapper.Rate, wrapper.Pitch));

                            sbXML.Append("</maryxml>");

                            request = uri + "/process?INPUT_TEXT=" + System.Uri.EscapeDataString(sbXML.ToString()) + "&INPUT_TYPE=RAWMARYXML&OUTPUT_TYPE=AUDIO&AUDIO=WAVE_FILE&LOCALE=" + voiceCulture + "&VOICE=" + voiceName;
                        }
                        else if (type == Model.Enum.MaryTTSType.EMOTIONML)
                        {

                            //EMOTIONML
                            sbXML.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" ?>");
                            sbXML.Append("<emotionml version=\"1.0\" ");
                            sbXML.Append("xmlns=\"http://www.w3.org/2009/10/emotionml\" ");

                            //sbXML.Append ("category-set=\"http://www.w3.org/TR/emotion-voc/xml#everyday-categories\"> ");
                            sbXML.Append("category-set=\"http://www.w3.org/TR/emotion-voc/xml#big6\"> ");
                            //sbXML.Append (">");

                            sbXML.Append(wrapper.Text);
                            sbXML.Append("</emotionml>");

                            request = uri + "/process?INPUT_TEXT=" + System.Uri.EscapeDataString(sbXML.ToString()) + "&INPUT_TYPE=EMOTIONML&OUTPUT_TYPE=AUDIO&AUDIO=WAVE_FILE&LOCALE=" + voiceCulture + "&VOICE=" + voiceName;
                        }
                        else if (type == Model.Enum.MaryTTSType.SSML)
                        {
                            //SSML
                            sbXML.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" ?>");
                            sbXML.Append("<speak version=\"1.0\" xmlns=\"http://www.w3.org/2001/10/synthesis\"");
                            //sbXML.Append (" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"");
                            //sbXML.Append (" xsi:schemaLocation=\"http://www.w3.org/2001/10/synthesis http://www.w3.org/TR/speech-synthesis/synthesis.xsd\"");
                            sbXML.Append(" xml:lang=\"");
                            sbXML.Append(voiceCulture);
                            sbXML.Append("\">");

                            sbXML.Append(prepareProsody(wrapper.Text, wrapper.Rate, wrapper.Pitch));

                            sbXML.Append("</speak>");

                            request = uri + "/process?INPUT_TEXT=" + System.Uri.EscapeDataString(sbXML.ToString()) + "&INPUT_TYPE=SSML&OUTPUT_TYPE=AUDIO&AUDIO=WAVE_FILE&LOCALE=" + voiceCulture + "&VOICE=" + voiceName;
                        }
                        else
                        {
                            //TEXT
                            request = uri + "/process?INPUT_TEXT=" + System.Uri.EscapeDataString(wrapper.Text) + "&INPUT_TYPE=TEXT&OUTPUT_TYPE=AUDIO&AUDIO=WAVE_FILE&LOCALE=" + voiceCulture + "&VOICE=" + voiceName;
                        }

                        if (Util.Constants.DEV_DEBUG)
                            Debug.Log(sbXML);

                        if (wrapper.Volume != 1f)
                        {
                            request += "&effect_Volume_selected=on&effect_Volume_parameters=amount:" + wrapper.Volume;
                        }

                        //Debug.Log (request);

                        using (WWW www = new WWW(request, rawData, headers))
                        {
                            do
                            {
                                yield return www;
                            } while (!www.isDone);

                            if (string.IsNullOrEmpty(www.error))
                            {
                                wrapper.OutputFile += AudioFileExtension;
                                System.IO.File.WriteAllBytes(wrapper.OutputFile, www.bytes);
                            }
                            else
                            {
                                string errorMessage = "Could not generate the speech: " + www.error;
                                Debug.LogError(errorMessage);
                                onErrorInfo(wrapper, errorMessage);
                            }
                        }

                        onSpeakAudioGenerationComplete(wrapper);

                    }
                }
            }
        }

        public override void Silence()
        {
            silence = true;
        }

        #endregion


        #region Private methods

        private IEnumerator speak(Model.Wrapper wrapper, bool isNative)
        {
            if (wrapper == null)
            {
                Debug.LogWarning("'wrapper' is null!");
            }
            else
            {
                if (string.IsNullOrEmpty(wrapper.Text))
                {
                    Debug.LogWarning("'Text' is null or empty: " + wrapper);
                    //yield return null;
                }
                else
                {
                    if (wrapper.Source == null)
                    {
                        Debug.LogWarning("'Source' is null: " + wrapper);
                        //yield return null;
                    }
                    else
                    {
                        if (!Util.Helper.isInternetAvailable)
                        {
                            string errorMessage = "Internet is not available - can't use MaryTTS right now!";
                            Debug.LogError(errorMessage);
                            onErrorInfo(wrapper, errorMessage);
                        }
                        else
                        {
                            yield return null; //return to the main process (uid)

                            string voiceCulture = getVoiceCulture(wrapper);
                            string voiceName = getVoiceName(wrapper);

                            silence = false;

                            if (!isNative)
                            {
                                onSpeakAudioGenerationStart(wrapper);
                            }

                            System.Text.StringBuilder sbXML = new System.Text.StringBuilder();
                            string request = null;

                            if (type == Model.Enum.MaryTTSType.RAWMARYXML)
                            {
                                //RAWMARYXML
                                sbXML.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" ?>");
                                sbXML.Append("<maryxml version=\"0.5\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://mary.dfki.de/2002/MaryXML\" xml:lang=\"");
                                sbXML.Append(voiceCulture);
                                sbXML.Append("\">");

                                sbXML.Append(prepareProsody(wrapper.Text, wrapper.Rate, wrapper.Pitch));

                                sbXML.Append("</maryxml>");

                                request = uri + "/process?INPUT_TEXT=" + System.Uri.EscapeDataString(sbXML.ToString()) + "&INPUT_TYPE=RAWMARYXML&OUTPUT_TYPE=AUDIO&AUDIO=WAVE_FILE&LOCALE=" + voiceCulture + "&VOICE=" + voiceName;
                            }
                            else if (type == Model.Enum.MaryTTSType.EMOTIONML)
                            {

                                //EMOTIONML
                                sbXML.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" ?>");
                                sbXML.Append("<emotionml version=\"1.0\" ");
                                sbXML.Append("xmlns=\"http://www.w3.org/2009/10/emotionml\" ");

                                //sbXML.Append ("category-set=\"http://www.w3.org/TR/emotion-voc/xml#everyday-categories\"> ");
                                sbXML.Append("category-set=\"http://www.w3.org/TR/emotion-voc/xml#big6\"> "); //TODO needed?
                                //sbXML.Append (">");

                                sbXML.Append(wrapper.Text);
                                sbXML.Append("</emotionml>");

                                request = uri + "/process?INPUT_TEXT=" + System.Uri.EscapeDataString(sbXML.ToString()) + "&INPUT_TYPE=EMOTIONML&OUTPUT_TYPE=AUDIO&AUDIO=WAVE_FILE&LOCALE=" + voiceCulture + "&VOICE=" + voiceName;
                            }
                            else if (type == Model.Enum.MaryTTSType.SSML)
                            {
                                //SSML
                                sbXML.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" ?>");
                                sbXML.Append("<speak version=\"1.0\" xmlns=\"http://www.w3.org/2001/10/synthesis\"");
                                //sbXML.Append (" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"");
                                //sbXML.Append (" xsi:schemaLocation=\"http://www.w3.org/2001/10/synthesis http://www.w3.org/TR/speech-synthesis/synthesis.xsd\"");
                                sbXML.Append(" xml:lang=\"");
                                sbXML.Append(voiceCulture);
                                sbXML.Append("\">");

                                sbXML.Append(prepareProsody(wrapper.Text, wrapper.Rate, wrapper.Pitch));

                                sbXML.Append("</speak>");

                                request = uri + "/process?INPUT_TEXT=" + System.Uri.EscapeDataString(sbXML.ToString()) + "&INPUT_TYPE=SSML&OUTPUT_TYPE=AUDIO&AUDIO=WAVE_FILE&LOCALE=" + voiceCulture + "&VOICE=" + voiceName;
                            }
                            else
                            {
                                //TEXT
                                request = uri + "/process?INPUT_TEXT=" + System.Uri.EscapeDataString(wrapper.Text) + "&INPUT_TYPE=TEXT&OUTPUT_TYPE=AUDIO&AUDIO=WAVE_FILE&LOCALE=" + voiceCulture + "&VOICE=" + voiceName;
                            }

                            //Debug.Log(request.Length);

                            if (Util.Constants.DEV_DEBUG)
                                Debug.Log(sbXML);

                            if (wrapper.Volume != 1f)
                            {
                                request += "&effect_Volume_selected=on&effect_Volume_parameters=amount:" + wrapper.Volume;
                            }

                            //Debug.Log(request);

                            using (WWW www = new WWW(request, rawData, headers))
                            {
                                do
                                {
                                    yield return www;
                                } while (!www.isDone);

                                if (string.IsNullOrEmpty(www.error))
                                {
                                    //just for testing!
                                    //string outputFile = Util.Config.AUDIOFILE_PATH + wrapper.Uid + extension;
                                    //System.IO.File.WriteAllBytes(outputFile, www.bytes);

                                    AudioClip ac = www.GetAudioClip(false, false, AudioType.WAV);
                                    //AudioClip ac = www.GetAudioClip(false, true, AudioType.WAV);
                                    //AudioClip ac = www.GetAudioClipCompressed(false, AudioType.WAV);

                                    do
                                    {
                                        yield return ac;
                                    } while (ac.loadState == AudioDataLoadState.Loading);

#if UNITY_WEBGL
                                    if (wrapper.Source != null) {
#else
                                    if (wrapper.Source != null && ac.loadState == AudioDataLoadState.Loaded)
                                    {
#endif
                                        wrapper.Source.clip = ac;

                                        if (Util.Config.DEBUG)
                                            Debug.Log("Text generated: " + wrapper.Text);

                                        if (!string.IsNullOrEmpty(wrapper.OutputFile))
                                        {
                                            wrapper.OutputFile += AudioFileExtension;
                                            System.IO.File.WriteAllBytes(wrapper.OutputFile, www.bytes);
                                        }

                                        if ((isNative || wrapper.SpeakImmediately) && wrapper.Source != null)
                                        {
                                            wrapper.Source.Play();
                                            onSpeakStart(wrapper);

                                            do
                                            {
                                                yield return null;
                                            } while (!silence && wrapper.Source != null && wrapper.Source.clip != null &&
                                                 ((!wrapper.Source.loop && wrapper.Source.timeSamples > 0 && wrapper.Source.timeSamples < wrapper.Source.clip.samples - 256) ||
                                                 wrapper.Source.loop ||
                                                 wrapper.Source.isPlaying));

                                            if (Util.Config.DEBUG)
                                                Debug.Log("Text spoken: " + wrapper.Text);

                                            onSpeakComplete(wrapper);
                                        }
                                    }
                                }
                                else
                                {
                                    string errorMessage = "Could not generate the speech: " + www.error;
                                    Debug.LogError(errorMessage);
                                    onErrorInfo(wrapper, errorMessage);
                                }
                            }

                            if (!isNative)
                            {
                                onSpeakAudioGenerationComplete(wrapper);
                            }
                        }
                    }
                }
            }
        }

        private IEnumerator getVoices()
        {
            System.Collections.Generic.List<string[]> serverVoicesResponse = new System.Collections.Generic.List<string[]>();

            if (!Util.Helper.isInternetAvailable)
            {
                string errorMessage = "Internet is not available - can't use MaryTTS right now!";
                Debug.LogError(errorMessage);
                onErrorInfo(null, errorMessage);
            }
            else
            {
                using (WWW www = new WWW(uri + "/voices", rawData, headers))
                {
                    do
                    {
                        yield return www;
                    } while (!www.isDone);

                    if (string.IsNullOrEmpty(www.error))
                    {
                        string[] rawVoices = www.text.Split('\n');
                        foreach (string rawVoice in rawVoices)
                        {
                            try
                            {
                                if (!string.IsNullOrEmpty(rawVoice))
                                {
                                    string[] newVoice = {
                                        rawVoice.Split (' ') [0],
                                        rawVoice.Split (' ') [1],
                                        rawVoice.Split (' ') [2]
                                    };
                                    serverVoicesResponse.Add(newVoice);
                                }
                            }
                            catch (System.Exception ex)
                            {
                                Debug.LogWarning("Problem preparing voice: " + rawVoice + " - " + ex);
                            }
                        }

                        cachedVoices.Clear();

                        foreach (string[] voice in serverVoicesResponse)
                        {
                            Model.Voice newVoice = new Model.Voice(voice[0], "MaryTTS voice: " + voice[0], voice[2], "unknown", voice[1]);
                            cachedVoices.Add(newVoice);
                        }

                        if (Util.Constants.DEV_DEBUG)
                            Debug.Log("Voices read: " + cachedVoices.CTDump());

                        //onVoicesReady();
                    }
                    else
                    {
                        string errorMessage = "Could not get the voices: " + www.error;

                        Debug.LogError(errorMessage);
                        onErrorInfo(null, errorMessage);
                    }
                }

                onVoicesReady();
            }
        }

        private static string prepareProsody(string text, float rate, float pitch)
        {
            if (rate != 1f || pitch != 1f)
            {
                System.Text.StringBuilder sbXML = new System.Text.StringBuilder();

                sbXML.Append("<prosody");

                if (rate != 1f)
                {
                    float _rate = rate > 1 ? (rate - 1f) * 0.5f : rate - 1f;

                    sbXML.Append(" rate=\"");
                    if (_rate >= 0f)
                    {
                        sbXML.Append(_rate.ToString("+#0%"));
                    }
                    else
                    {
                        sbXML.Append(_rate.ToString("#0%"));
                    }

                    sbXML.Append("\"");
                }

                if (pitch != 1f)
                {
                    float _pitch = pitch - 1f;

                    sbXML.Append(" pitch=\"");
                    if (_pitch >= 0f)
                    {
                        sbXML.Append(_pitch.ToString("+#0%"));
                    }
                    else
                    {
                        sbXML.Append(_pitch.ToString("#0%"));
                    }

                    sbXML.Append("\"");
                }

                sbXML.Append(">");

                sbXML.Append(text);

                sbXML.Append("</prosody>");

                return sbXML.ToString();
            }

            return text;
        }
        private string getVoiceName(Model.Wrapper wrapper)
        {
            if (wrapper.Voice == null || string.IsNullOrEmpty(wrapper.Voice.Name))
            {
                if (Util.Config.DEBUG)
                    Debug.LogWarning("'Voice' or 'Voice.Name' is null! Using the 'default' English voice.");

                if (Voices.Count > 0)
                {
                    //always use English as fallback
                    return Speaker.VoiceForCulture("en_US").Name;
                }

                return "cmu-bdl";
            }
            else
            {
                return wrapper.Voice.Name;
            }
        }

        private string getVoiceCulture(Model.Wrapper wrapper)
        {
            if (wrapper.Voice == null || string.IsNullOrEmpty(wrapper.Voice.Culture))
            {
                if (Util.Config.DEBUG)
                    Debug.LogWarning("'Voice' or 'Voice.Culture' is null! Using the 'default' English voice.");

                //always use English as fallback
                return "en_US";
            }
            else
            {
                return wrapper.Voice.Culture;
            }
        }

        #endregion


        #region Editor-only methods

#if UNITY_EDITOR

        public override void GenerateInEditor(Model.Wrapper wrapper)
        {
            Debug.LogError("GenerateInEditor is not supported for MaryTTS!");
        }

        public override void SpeakNativeInEditor(Model.Wrapper wrapper)
        {
            Debug.LogError("SpeakNativeInEditor is not supported for MaryTTS!");

            //if (wrapper == null)
            //{
            //    Debug.LogWarning("'wrapper' is null!");
            //}
            //else
            //{
            //    if (string.IsNullOrEmpty(wrapper.Text))
            //    {
            //        Debug.LogWarning("'Text' is null or empty: " + wrapper);
            //    }
            //    else
            //    {
            //        if (wrapper.Source == null)
            //        {
            //            Debug.LogWarning("'Source' is null: " + wrapper);
            //        }
            //        else
            //        {
            //            if (!Util.Helper.isInternetAvailable)
            //            {
            //                string errorMessage = "Internet is not available - can't use MaryTTS right now!";
            //                Debug.LogError(errorMessage);
            //                onErrorInfo(wrapper, errorMessage);
            //            }
            //            else
            //            {
            //                string voiceCulture = getVoiceCulture(wrapper);
            //                string voiceName = getVoiceName(wrapper);

            //                silence = false;

            //                onSpeakAudioGenerationStart(wrapper);

            //                System.Text.StringBuilder sbXML = new System.Text.StringBuilder();
            //                string request = null;

            //                if (type == Model.Enum.MaryTTSType.RAWMARYXML)
            //                {
            //                    //RAWMARYXML
            //                    sbXML.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" ?>");
            //                    sbXML.Append("<maryxml version=\"0.5\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://mary.dfki.de/2002/MaryXML\" xml:lang=\"");
            //                    sbXML.Append(voiceCulture);
            //                    sbXML.Append("\">");

            //                    sbXML.Append(prepareProsody(wrapper.Text, wrapper.Rate, wrapper.Pitch));

            //                    sbXML.Append("</maryxml>");

            //                    request = uri + "/process?INPUT_TEXT=" + System.Uri.EscapeDataString(sbXML.ToString()) + "&INPUT_TYPE=RAWMARYXML&OUTPUT_TYPE=AUDIO&AUDIO=WAVE_FILE&LOCALE=" + voiceCulture + "&VOICE=" + voiceName;
            //                }
            //                else if (type == Model.Enum.MaryTTSType.EMOTIONML)
            //                {

            //                    //EMOTIONML
            //                    sbXML.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" ?>");
            //                    sbXML.Append("<emotionml version=\"1.0\" ");
            //                    sbXML.Append("xmlns=\"http://www.w3.org/2009/10/emotionml\" ");

            //                    //sbXML.Append ("category-set=\"http://www.w3.org/TR/emotion-voc/xml#everyday-categories\"> ");
            //                    sbXML.Append("category-set=\"http://www.w3.org/TR/emotion-voc/xml#big6\"> "); //TODO needed?
            //                    //sbXML.Append (">");

            //                    sbXML.Append(wrapper.Text);
            //                    sbXML.Append("</emotionml>");

            //                    request = uri + "/process?INPUT_TEXT=" + System.Uri.EscapeDataString(sbXML.ToString()) + "&INPUT_TYPE=EMOTIONML&OUTPUT_TYPE=AUDIO&AUDIO=WAVE_FILE&LOCALE=" + voiceCulture + "&VOICE=" + voiceName;
            //                }
            //                else if (type == Model.Enum.MaryTTSType.SSML)
            //                {
            //                    //SSML
            //                    sbXML.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" ?>");
            //                    sbXML.Append("<speak version=\"1.0\" xmlns=\"http://www.w3.org/2001/10/synthesis\"");
            //                    //sbXML.Append (" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"");
            //                    //sbXML.Append (" xsi:schemaLocation=\"http://www.w3.org/2001/10/synthesis http://www.w3.org/TR/speech-synthesis/synthesis.xsd\"");
            //                    sbXML.Append(" xml:lang=\"");
            //                    sbXML.Append(voiceCulture);
            //                    sbXML.Append("\">");

            //                    sbXML.Append(prepareProsody(wrapper.Text, wrapper.Rate, wrapper.Pitch));

            //                    sbXML.Append("</speak>");

            //                    request = uri + "/process?INPUT_TEXT=" + System.Uri.EscapeDataString(sbXML.ToString()) + "&INPUT_TYPE=SSML&OUTPUT_TYPE=AUDIO&AUDIO=WAVE_FILE&LOCALE=" + voiceCulture + "&VOICE=" + voiceName;
            //                }
            //                else
            //                {
            //                    //TEXT
            //                    request = uri + "/process?INPUT_TEXT=" + System.Uri.EscapeDataString(wrapper.Text) + "&INPUT_TYPE=TEXT&OUTPUT_TYPE=AUDIO&AUDIO=WAVE_FILE&LOCALE=" + voiceCulture + "&VOICE=" + voiceName;
            //                }

            //                //Debug.Log(request.Length);

            //                if (Util.Constants.DEV_DEBUG)
            //                    Debug.Log(sbXML);

            //                if (wrapper.Volume != 1f)
            //                {
            //                    request += "&effect_Volume_selected=on&effect_Volume_parameters=amount:" + wrapper.Volume;
            //                }

            //                //Debug.Log(request);

            //                using (WWW www = new WWW(request, rawData, headers))
            //                {
            //                    do
            //                    {
            //                        //wait
            //                        //System.Threading.Thread.Sleep(50);
            //                    } while (!www.isDone);

            //                    if (string.IsNullOrEmpty(www.error))
            //                    {
            //                        AudioClip ac = www.GetAudioClip(false, true, AudioType.WAV);

            //                        do
            //                        {
            //                            //wait
            //                            //System.Threading.Thread.Sleep(50);
            //                        } while (ac.loadState == AudioDataLoadState.Loading);

            //                        if (wrapper.Source != null && ac.loadState == AudioDataLoadState.Loaded)
            //                        {
            //                            wrapper.Source.clip = ac;

            //                            if (Util.Config.DEBUG)
            //                                Debug.Log("Text generated: " + wrapper.Text);

            //                            if (!string.IsNullOrEmpty(wrapper.OutputFile))
            //                            {
            //                                wrapper.OutputFile += AudioFileExtension;
            //                                System.IO.File.WriteAllBytes(wrapper.OutputFile, www.bytes);
            //                            }

            //                            wrapper.Source.Play();
            //                            onSpeakStart(wrapper);

            //                            do
            //                            {
            //                                //System.Threading.Thread.Sleep(50);
            //                            } while (!silence && !wrapper.Source.isPlaying);

            //                            if (Util.Config.DEBUG)
            //                                Debug.Log("Text spoken: " + wrapper.Text);

            //                            onSpeakComplete(wrapper);
            //                        }
            //                    }
            //                    else
            //                    {
            //                        string errorMessage = "Could not generate the speech: " + www.error;
            //                        Debug.LogError(errorMessage);
            //                        onErrorInfo(wrapper, errorMessage);
            //                    }
            //                }

            //                onSpeakAudioGenerationComplete(wrapper);
            //            }
            //        }
            //    }
            //}
        }


#endif

        private void getVoicesInEditor()
        {
            System.Collections.Generic.List<string[]> serverVoicesResponse = new System.Collections.Generic.List<string[]>();

            if (!Util.Helper.isInternetAvailable)
            {
                string errorMessage = "Internet is not available - can't use MaryTTS right now!";
                Debug.LogError(errorMessage);
            }
            else
            {
                using (WWW www = new WWW(uri + "/voices", rawData, headers))
                {
                    float time = Time.realtimeSinceStartup;

                    do
                    {
                        // waiting...

#if !UNITY_WSA && !UNITY_WEBGL
                        System.Threading.Thread.Sleep(50);
#endif
                    } while (Time.realtimeSinceStartup - time < 5 && !www.isDone);

                    if (string.IsNullOrEmpty(www.error) && Time.realtimeSinceStartup - time < 5)
                    {
                        string[] rawVoices = www.text.Split('\n');
                        foreach (string rawVoice in rawVoices)
                        {
                            try
                            {
                                if (!string.IsNullOrEmpty(rawVoice))
                                {
                                    string[] newVoice = {
                                        rawVoice.Split (' ') [0],
                                        rawVoice.Split (' ') [1],
                                        rawVoice.Split (' ') [2]
                                    };
                                    serverVoicesResponse.Add(newVoice);
                                }
                            }
                            catch (System.Exception ex)
                            {
                                Debug.LogWarning("Problem preparing voice: " + rawVoice + " - " + ex);
                            }
                        }

                        cachedVoices.Clear();

                        foreach (string[] voice in serverVoicesResponse)
                        {
                            Model.Voice newVoice = new Model.Voice(voice[0], "MaryTTS voice: " + voice[0], voice[2], "unknown", voice[1]);
                            cachedVoices.Add(newVoice);
                        }

                        if (Util.Constants.DEV_DEBUG)
                            Debug.Log("Voices read: " + cachedVoices.CTDump());
                    }
                    else
                    {
                        string errorMessage = null;

                        if (!string.IsNullOrEmpty(www.error))
                        {
                            errorMessage = "Could not get the voices: " + www.error;
                        }
                        else
                        {
                            errorMessage = "Could not get the voices, MaryTTS had a timeout after 5 seconds. This indicates a very slow network connection or a bigger problem.";
                        }

                        Debug.LogError(errorMessage);
                    }
                }
            }
        }

        #endregion

    }
}
// © 2016-2018 crosstales LLC (https://www.crosstales.com)