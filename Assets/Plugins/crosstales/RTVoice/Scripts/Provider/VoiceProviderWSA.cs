using UnityEngine;
using System.Collections;

namespace Crosstales.RTVoice.Provider
{
    public class VoiceProviderWSA : BaseVoiceProvider
    {

        #region Variables

        private static readonly System.Collections.Generic.List<Model.Voice> cachedVoices = new System.Collections.Generic.List<Model.Voice>();

        private const string extension = ".wav";

#if (UNITY_WSA || UNITY_EDITOR) && !UNITY_WEBPLAYER

        private static bool isInitialized = false;
        private static RTVoiceUWPBridge ttsHandler;
        private readonly WaitForSeconds wfs = new WaitForSeconds(0.1f);

#endif

        #endregion


        #region Constructor

        /// <summary>
        /// Constructor for VoiceProviderWSA.
        /// </summary>
        /// <param name="obj">Instance of the speaker</param>
        public VoiceProviderWSA(MonoBehaviour obj) : base(obj)
        {
#if (UNITY_WSA || UNITY_EDITOR) && !UNITY_WEBPLAYER
            if (!isInitialized)
            {
                initializeTTS();
            }
#endif

            speakerObj.StartCoroutine(getVoices());
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

#if (UNITY_WSA || UNITY_EDITOR) && !UNITY_WEBPLAYER
            if (wrapper == null)
            {
                Debug.LogWarning("'wrapper' is null!");
            }
            else
            {
                if (string.IsNullOrEmpty(wrapper.Text))
                {
                    Debug.LogWarning("'Text' is null or empty: " + wrapper);
                    yield return null;
                }
                else
                {

                    yield return null; //return to the main process (uid)

                    while (!isInitialized)
                    {
                        yield return null;
                    }

                    string voiceName = getVoiceName(wrapper);

                    string outputFile = Application.persistentDataPath;
                    ttsHandler.SynthesizeToFile(wrapper.Text, outputFile.Replace('/', '\\'), wrapper.Uid + extension, voiceName);

                    silence = false;

                    onSpeakAudioGenerationStart(wrapper);

                    do
                    {
                        yield return wfs;
                    } while (!silence && ttsHandler.IsBusy());

                    //Debug.Log("FILE: " + "file://" + outputFile + "/" + wrapper.Uid + extension);


                    if (Util.Config.DEBUG)
                        Debug.Log("Text generated: " + wrapper.Text);

                    if (!string.IsNullOrEmpty(wrapper.OutputFile))
                    {
                        wrapper.OutputFile += AudioFileExtension;
                        fileCopy(outputFile, wrapper.OutputFile, Util.Config.AUDIOFILE_AUTOMATIC_DELETE);
                    }

                    if (Util.Config.AUDIOFILE_AUTOMATIC_DELETE)
                    {
                        if (System.IO.File.Exists(outputFile))
                        {
                            try
                            {
                                System.IO.File.Delete(outputFile);
                            }
                            catch (System.Exception ex)
                            {
                                string errorMessage = "Could not delete file '" + outputFile + "'!" + System.Environment.NewLine + ex;
                                Debug.LogError(errorMessage);
                                onErrorInfo(wrapper, errorMessage);
                            }
                        }
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(wrapper.OutputFile))
                        {
                            wrapper.OutputFile = outputFile;
                        }
                    }

                    onSpeakAudioGenerationComplete(wrapper);
                }
            }

#else
            yield return null;
#endif
        }

        public override void Silence()
        {
            silence = true;
        }

        #endregion


        #region Private methods

        private IEnumerator getVoices()
        {
#if (UNITY_WSA || UNITY_EDITOR) && !UNITY_WEBPLAYER

            do
            {
                yield return null;
            } while (!isInitialized);

            try
            {
                string[] myStringVoices = ttsHandler.GetVoices();
                cachedVoices.Clear();
                foreach (string voice in myStringVoices)
                {
                    string[] currentVoiceData = voice.Split(';');
                    Model.Voice newVoice = new Model.Voice(currentVoiceData[0], "UWP voice: " + voice, currentVoiceData[1]);
                    cachedVoices.Add(newVoice);
                }

                if (Util.Constants.DEV_DEBUG)
                    Debug.Log("Voices read: " + cachedVoices.CTDump());

                //onVoicesReady();
            }
            catch (System.Exception ex)
            {
                string errorMessage = "Could not get any voices!" + System.Environment.NewLine + ex;
                Debug.LogError(errorMessage);
                onErrorInfo(null, errorMessage);
            }
#else
            yield return null;
#endif

            onVoicesReady();
        }

        private IEnumerator speak(Model.Wrapper wrapper, bool isNative)
        {

#if (UNITY_WSA || UNITY_EDITOR) && !UNITY_WEBPLAYER
            if (wrapper == null)
            {
                Debug.LogWarning("'wrapper' is null!");
            }
            else
            {
                if (string.IsNullOrEmpty(wrapper.Text))
                {
                    Debug.LogWarning("'Text' is null or empty: " + wrapper);
                    yield return null;
                }
                else
                {
                    if (wrapper.Source == null)
                    {
                        Debug.LogWarning("'Source' is null: " + wrapper);
                        yield return null;
                    }
                    else
                    {
                        yield return null; //return to the main process (uid)

                        while (!isInitialized)
                        {
                            yield return null;
                        }

                        string voiceName = getVoiceName(wrapper);

                        string outputFile = Application.persistentDataPath;
                        ttsHandler.SynthesizeToFile(wrapper.Text, outputFile.Replace('/', '\\'), wrapper.Uid + extension, voiceName);

                        silence = false;

                        if (!isNative)
                        {
                            onSpeakAudioGenerationStart(wrapper);
                        }

                        do
                        {
                            yield return wfs;
                        } while (!silence && ttsHandler.IsBusy());

                        using (WWW www = new WWW(Util.Constants.PREFIX_FILE + "/" + outputFile + "/" + wrapper.Uid + extension))
                        {
                            do
                            {
                                yield return www;
                            } while (!www.isDone);

                            if (string.IsNullOrEmpty(www.error))
                            {
                                AudioClip ac = www.GetAudioClip(false, false, AudioType.WAV);
                                //AudioClip ac = www.GetAudioClip(false, true, AudioType.WAV);
                                //AudioClip ac = www.GetAudioClipCompressed(false, AudioType.WAV);
                                
                                do
                                {
                                    yield return ac;
                                } while (ac.loadState == AudioDataLoadState.Loading);

                                if (wrapper.Source != null && ac.loadState == AudioDataLoadState.Loaded)
                                {
                                    wrapper.Source.clip = ac;

                                    if (Util.Config.DEBUG)
                                        Debug.Log("Text generated: " + wrapper.Text);

                                    if (!string.IsNullOrEmpty(wrapper.OutputFile))
                                    {
                                        wrapper.OutputFile += AudioFileExtension;
                                        fileCopy(outputFile, wrapper.OutputFile, Util.Config.AUDIOFILE_AUTOMATIC_DELETE);
                                    }

                                    if (Util.Config.AUDIOFILE_AUTOMATIC_DELETE)
                                    {
                                        if (System.IO.File.Exists(outputFile))
                                        {
                                            try
                                            {
                                                System.IO.File.Delete(outputFile);
                                            }
                                            catch (System.Exception ex)
                                            {
                                                string errorMessage = "Could not delete file '" + outputFile + "'!" + System.Environment.NewLine + ex;
                                                Debug.LogError(errorMessage);
                                                onErrorInfo(wrapper, errorMessage);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (string.IsNullOrEmpty(wrapper.OutputFile))
                                        {
                                            wrapper.OutputFile = outputFile;
                                        }
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
                                string errorMessage = "Could not read the file: " + www.error;
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

#else
            yield return null;
#endif
        }

#if (UNITY_WSA || UNITY_EDITOR) && !UNITY_WEBPLAYER

        private void initializeTTS()
        {
            if (Util.Constants.DEV_DEBUG)
                Debug.Log("Initializing TTS...");

            ttsHandler = new RTVoiceUWPBridge();

            ttsHandler.DEBUG(Util.Config.DEBUG);

            //Debug.Log("TARGET FOLDER: " + ttsHandler.GetTargetFolder());

            isInitialized = true;
        }

#endif
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

                return "Microsoft Zira Mobile";
            }
            else
            {
                return wrapper.Voice.Name;
            }
        }

        #endregion


        #region Editor-only methods


#if UNITY_EDITOR

        public override void GenerateInEditor(Model.Wrapper wrapper)
        {
            Debug.LogError("GenerateInEditor is not supported for Unity WSA!");
        }

        public override void SpeakNativeInEditor(Model.Wrapper wrapper)
        {
            Debug.LogError("SpeakNativeInEditor is not supported for Unity WSA!");
        }

#endif

        #endregion

    }
}
// © 2016-2018 crosstales LLC (https://www.crosstales.com)