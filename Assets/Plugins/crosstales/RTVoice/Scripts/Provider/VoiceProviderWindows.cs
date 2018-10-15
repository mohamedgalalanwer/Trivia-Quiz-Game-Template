using UnityEngine;
using System.Collections;

namespace Crosstales.RTVoice.Provider
{
    /// <summary>Windows voice provider.</summary>
    public class VoiceProviderWindows : BaseVoiceProvider
    {

        #region Variables

        private static readonly System.Collections.Generic.List<Model.Voice> cachedVoices = new System.Collections.Generic.List<Model.Voice>();

        private const string extension = ".wav";

        private string dataPath;

#if (UNITY_STANDALONE_WIN || UNITY_EDITOR) && !UNITY_WEBPLAYER
        private const string idVoice = "@VOICE:";
        private const string idSpeak = "@SPEAK";
        private const string idWord = "@WORD";
        private const string idPhoneme = "@PHONEME:";
        private const string idViseme = "@VISEME:";
        private const string idStart = "@STARTED";

        private static char[] splitChar = new char[] { ':' };
#endif

        #endregion


        #region Constructor

        /// <summary>
        /// Constructor for VoiceProviderWindows.
        /// </summary>
        /// <param name="obj">Instance of the speaker</param>
        public VoiceProviderWindows(MonoBehaviour obj) : base(obj)
        {
            dataPath = Application.dataPath;

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
#if (UNITY_STANDALONE_WIN || UNITY_EDITOR) && !UNITY_WEBPLAYER
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

                    string application = applicationName();

                    if (System.IO.File.Exists(application))
                    {
                        int calculatedRate = calculateRate(wrapper.Rate);
                        int calculatedVolume = calculateVolume(wrapper.Volume);

                        string voiceName = string.Empty;

                        if (wrapper.Voice == null || string.IsNullOrEmpty(wrapper.Voice.Name))
                        {
                            if (Util.Config.DEBUG)
                                Debug.LogWarning("'Voice' or 'Voice.Name' is null! Using the OS 'default' voice.");
                        }
                        else
                        {
                            voiceName = wrapper.Voice.Name;
                        }

                        //using (System.Diagnostics.Process speakProcess = new System.Diagnostics.Process())
                        //{

                        System.Diagnostics.Process speakProcess = new System.Diagnostics.Process();

                        string args = "--speak " + '"' + prepareText(wrapper.Text, wrapper.Pitch) + "\" " +
                                      calculatedRate.ToString() + " " +
                                      calculatedVolume.ToString() + " \"" +
                                      voiceName.Replace('"', '\'') + '"';

                        if (Util.Config.DEBUG)
                            Debug.Log("Process argruments: " + args);

                        speakProcess.StartInfo.FileName = application;
                        speakProcess.StartInfo.Arguments = args;
                        speakProcess.StartInfo.CreateNoWindow = true;
                        speakProcess.StartInfo.RedirectStandardOutput = true;
                        speakProcess.StartInfo.RedirectStandardError = true;
                        speakProcess.StartInfo.UseShellExecute = false;
                        speakProcess.StartInfo.StandardOutputEncoding = System.Text.Encoding.UTF8;
                        //* Set your output and error (asynchronous) handlers
                        //speakProcess.OutputDataReceived += new DataReceivedEventHandler(speakNativeHandler);
                        //scanProcess.ErrorDataReceived += new DataReceivedEventHandler(ErrorHandler);

                        //speakProcess.Start();
                        //speakProcess.BeginOutputReadLine();
                        //speakProcess.BeginErrorReadLine();

                        string[] speechTextArray = Util.Helper.CleanText(wrapper.Text, false).Split(splitCharWords, System.StringSplitOptions.RemoveEmptyEntries);
                        int wordIndex = 0;
                        int wordIndexCompare = 0;
                        string phoneme = string.Empty;
                        string viseme = string.Empty;
                        bool start = false;

                        System.Threading.Thread worker = new System.Threading.Thread(() => readSpeakNativeStream(ref speakProcess, ref speechTextArray, out wordIndex, out phoneme, out viseme, out start)) { Name = wrapper.Uid.ToString() };
                        worker.Start();

                        silence = false;
                        processes.Add(wrapper.Uid, speakProcess);

                        do
                        {
                            yield return null;

                            if (wordIndex != wordIndexCompare)
                            {
                                onSpeakCurrentWord(wrapper, speechTextArray, wordIndex - 1);

                                wordIndexCompare = wordIndex;
                            }

                            if (!string.IsNullOrEmpty(phoneme))
                            {
                                onSpeakCurrentPhoneme(wrapper, phoneme);

                                phoneme = string.Empty;
                            }

                            if (!string.IsNullOrEmpty(viseme))
                            {
                                onSpeakCurrentViseme(wrapper, viseme);

                                viseme = string.Empty;
                            }

                            if (start)
                            {
                                onSpeakStart(wrapper);

                                start = false;
                            }
                        } while (worker.IsAlive || !speakProcess.HasExited);

                        // clear output
                        onSpeakCurrentPhoneme(wrapper, string.Empty);
                        onSpeakCurrentViseme(wrapper, string.Empty);

                        if (speakProcess.ExitCode == 0 || speakProcess.ExitCode == -1)
                        { //0 = normal ended, -1 = killed
                            if (Util.Config.DEBUG)
                                Debug.Log("Text spoken: " + wrapper.Text);
                        }
                        else
                        {
                            using (System.IO.StreamReader sr = speakProcess.StandardError)
                            {
                                string errorMessage = "Could not speak the text: " + speakProcess.ExitCode + System.Environment.NewLine + sr.ReadToEnd();
                                Debug.LogError(errorMessage);
                                onErrorInfo(wrapper, errorMessage);
                            }
                        }

                        onSpeakComplete(wrapper);
                        processes.Remove(wrapper.Uid);

                        speakProcess.Dispose();
                        //}
                    }
                    else
                    {
                        string errorMessage = "Could not find the TTS-wrapper: '" + application + "'";
                        Debug.LogError(errorMessage);
                        onErrorInfo(wrapper, errorMessage);
                    }
                }
            }
#else
            yield return null;
#endif
        }


        public override IEnumerator Speak(Model.Wrapper wrapper)
        {
#if (UNITY_STANDALONE_WIN || UNITY_EDITOR) && !UNITY_WEBPLAYER
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
                        yield return null; //return to the main process (uid)

                        string application = applicationName();

                        if (System.IO.File.Exists(application))
                        {
                            int calculatedRate = calculateRate(wrapper.Rate);
                            int calculatedVolume = calculateVolume(wrapper.Volume);

                            string voiceName = string.Empty;

                            if (wrapper.Voice == null || string.IsNullOrEmpty(wrapper.Voice.Name))
                            {
                                if (Util.Config.DEBUG)
                                    Debug.LogWarning("'Voice' or 'Voice.Name' is null! Using the OS 'default' voice.");
                            }
                            else
                            {
                                voiceName = wrapper.Voice.Name;
                            }

                            //using (System.Diagnostics.Process speakToFileProcess = new System.Diagnostics.Process())
                            //{

                            System.Diagnostics.Process speakToFileProcess = new System.Diagnostics.Process();

                            string outputFile = Util.Config.AUDIOFILE_PATH + wrapper.Uid + extension;

                            string args = "--speakToFile" + " \"" +
                                          prepareText(wrapper.Text, wrapper.Pitch) + "\" \"" +
                                          outputFile.Replace('"', '\'') + "\" " +
                                          calculatedRate.ToString() + " " +
                                          calculatedVolume.ToString() + " \"" +
                                          voiceName.Replace('"', '\'') + '"';

                            if (Util.Config.DEBUG)
                                Debug.Log("Process argruments: " + args);

                            speakToFileProcess.StartInfo.FileName = application;
                            speakToFileProcess.StartInfo.Arguments = args;
                            speakToFileProcess.StartInfo.CreateNoWindow = true;
                            speakToFileProcess.StartInfo.RedirectStandardOutput = true;
                            speakToFileProcess.StartInfo.RedirectStandardError = true;
                            speakToFileProcess.StartInfo.UseShellExecute = false;
                            speakToFileProcess.StartInfo.StandardOutputEncoding = System.Text.Encoding.UTF8;
                            //* Set your output and error (asynchronous) handlers
                            //speakToFileProcess.OutputDataReceived += new DataReceivedEventHandler(speakNativeHandler);
                            //speakToFileProcess.ErrorDataReceived += new DataReceivedEventHandler(ErrorHandler);

                            //speakToFileProcess.Start();
                            //speakToFileProcess.BeginOutputReadLine();
                            //speakToFileProcess.BeginErrorReadLine();

                            System.Threading.Thread worker = new System.Threading.Thread(() => startProcess(ref speakToFileProcess)) { Name = wrapper.Uid.ToString() };
                            worker.Start();

                            silence = false;
                            onSpeakAudioGenerationStart(wrapper);

                            do
                            {
                                yield return null;
                            } while (worker.IsAlive || !speakToFileProcess.HasExited);

                            if (speakToFileProcess.ExitCode == 0)
                            {
                                using (WWW www = new WWW(Util.Constants.PREFIX_FILE + outputFile))
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

                                            if (wrapper.SpeakImmediately && wrapper.Source != null)
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
                            }
                            else
                            {
                                using (System.IO.StreamReader sr = speakToFileProcess.StandardError)
                                {
                                    string errorMessage = "Could not speak the text: " + speakToFileProcess.ExitCode + System.Environment.NewLine + sr.ReadToEnd();
                                    Debug.LogError(errorMessage);
                                    onErrorInfo(wrapper, errorMessage);
                                }
                            }

                            onSpeakAudioGenerationComplete(wrapper);

                            speakToFileProcess.Dispose();
                            //Debug.Log("Speak complete: " + wrapper.Text);
                            //}
                        }
                        else
                        {
                            string errorMessage = "Could not find the TTS-wrapper: '" + application + "'";
                            Debug.LogError(errorMessage);
                            onErrorInfo(wrapper, errorMessage);
                        }
                    }
                }
            }
#else
         yield return null;
#endif
        }

        public override IEnumerator Generate(Model.Wrapper wrapper)
        {
#if (UNITY_STANDALONE_WIN || UNITY_EDITOR) && !UNITY_WEBPLAYER
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

                    yield return null; //return to the main process (uid)

                    string application = applicationName();

                    if (System.IO.File.Exists(application))
                    {
                        int calculatedRate = calculateRate(wrapper.Rate);
                        int calculatedVolume = calculateVolume(wrapper.Volume);

                        string voiceName = string.Empty;

                        if (wrapper.Voice == null || string.IsNullOrEmpty(wrapper.Voice.Name))
                        {
                            if (Util.Config.DEBUG)
                                Debug.LogWarning("'Voice' or 'Voice.Name' is null! Using the OS 'default' voice.");
                        }
                        else
                        {
                            voiceName = wrapper.Voice.Name;
                        }

                        //using (System.Diagnostics.Process speakToFileProcess = new System.Diagnostics.Process())
                        //{
                        System.Diagnostics.Process speakToFileProcess = new System.Diagnostics.Process();

                        string outputFile = Util.Config.AUDIOFILE_PATH + wrapper.Uid + extension;

                        string args = "--speakToFile" + " \"" +
                                      prepareText(wrapper.Text, wrapper.Pitch) + "\" \"" +
                                      outputFile.Replace('"', '\'') + "\" " +
                                      calculatedRate.ToString() + " " +
                                      calculatedVolume.ToString() + " \"" +
                                      voiceName.Replace('"', '\'') + '"';

                        if (Util.Config.DEBUG)
                            Debug.Log("Process argruments: " + args);

                        speakToFileProcess.StartInfo.FileName = application;
                        speakToFileProcess.StartInfo.Arguments = args;
                        speakToFileProcess.StartInfo.CreateNoWindow = true;
                        speakToFileProcess.StartInfo.RedirectStandardOutput = true;
                        speakToFileProcess.StartInfo.RedirectStandardError = true;
                        speakToFileProcess.StartInfo.UseShellExecute = false;
                        speakToFileProcess.StartInfo.StandardOutputEncoding = System.Text.Encoding.UTF8;
                        //* Set your output and error (asynchronous) handlers
                        //speakToFileProcess.OutputDataReceived += new DataReceivedEventHandler(speakNativeHandler);
                        //speakToFileProcess.ErrorDataReceived += new DataReceivedEventHandler(ErrorHandler);

                        //speakToFileProcess.Start();
                        //speakToFileProcess.BeginOutputReadLine();
                        //speakToFileProcess.BeginErrorReadLine();

                        System.Threading.Thread worker = new System.Threading.Thread(() => startProcess(ref speakToFileProcess)) { Name = wrapper.Uid.ToString() };
                        worker.Start();

                        silence = false;
                        onSpeakAudioGenerationStart(wrapper);

                        do
                        {
                            yield return null;
                        } while (worker.IsAlive || !speakToFileProcess.HasExited);

                        if (speakToFileProcess.ExitCode == 0)
                        {

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
                        }
                        else
                        {
                            using (System.IO.StreamReader sr = speakToFileProcess.StandardError)
                            {
                                string errorMessage = "Could not generate the text: " + speakToFileProcess.ExitCode + System.Environment.NewLine + sr.ReadToEnd();
                                Debug.LogError(errorMessage);
                                onErrorInfo(wrapper, errorMessage);
                            }
                        }

                        onSpeakAudioGenerationComplete(wrapper);

                        speakToFileProcess.Dispose();

                        //Debug.Log("Speak complete: " + wrapper.Text);

                        //}
                    }
                    else
                    {
                        string errorMessage = "Could not find the TTS-wrapper: '" + application + "'";
                        Debug.LogError(errorMessage);
                        onErrorInfo(wrapper, errorMessage);
                    }
                }
            }
#else
            yield return null;
#endif
        }

        #endregion


        #region Private methods

        private IEnumerator getVoices()
        {

#if (UNITY_STANDALONE_WIN || UNITY_EDITOR) && !UNITY_WEBPLAYER
            string application = applicationName();

            if (System.IO.File.Exists(application))
            {
                //using (System.Diagnostics.Process voicesProcess = new System.Diagnostics.Process())
                //{
                System.Diagnostics.Process voicesProcess = new System.Diagnostics.Process();

                voicesProcess.StartInfo.FileName = application;
                voicesProcess.StartInfo.Arguments = "--voices";
                voicesProcess.StartInfo.CreateNoWindow = true;
                voicesProcess.StartInfo.RedirectStandardOutput = true;
                voicesProcess.StartInfo.RedirectStandardError = true;
                voicesProcess.StartInfo.UseShellExecute = false;
                voicesProcess.StartInfo.StandardOutputEncoding = System.Text.Encoding.UTF8;
                //* Set your output and error (asynchronous) handlers
                //voicesProcess.OutputDataReceived += new DataReceivedEventHandler(speakNativeHandler);
                //voicesProcess.ErrorDataReceived += new DataReceivedEventHandler(ErrorHandler);


                //voicesProcess.Start();
                //voicesProcess.BeginOutputReadLine();
                //voicesProcess.BeginErrorReadLine();

                System.Threading.Thread worker = new System.Threading.Thread(() => startProcess(ref voicesProcess, Util.Constants.DEFAULT_TTS_KILL_TIME));
                worker.Start();

                do
                {
                    yield return null;
                } while (worker.IsAlive || !voicesProcess.HasExited);

                if (voicesProcess.ExitCode == 0)
                {
                    cachedVoices.Clear();

                    using (System.IO.StreamReader streamReader = voicesProcess.StandardOutput)
                    {
                        string reply;
                        while (!streamReader.EndOfStream)
                        {
                            reply = streamReader.ReadLine();

                            if (!string.IsNullOrEmpty(reply))
                            {
                                if (reply.StartsWith(idVoice))
                                {
                                    string[] splittedString = reply.Split(splitChar, System.StringSplitOptions.RemoveEmptyEntries);

                                    if (splittedString.Length == 6)
                                    {
                                        cachedVoices.Add(new Model.Voice(splittedString[1], splittedString[2], splittedString[3], splittedString[4], splittedString[5]));
                                    }
                                    else
                                    {
                                        Debug.LogWarning("Voice is invalid: " + reply);
                                    }
                                    //                     } else if(reply.Equals("@DONE") || reply.Equals("@COMPLETED")) {
                                    //                        complete = true;
                                }
                            }
                        }
                    }

                    if (Util.Constants.DEV_DEBUG)
                        Debug.Log("Voices read: " + cachedVoices.CTDump());

                    //onVoicesReady();
                }
                else
                {
                    using (System.IO.StreamReader sr = voicesProcess.StandardError)
                    {
                        string errorMessage = "Could not get any voices: " + voicesProcess.ExitCode + System.Environment.NewLine + sr.ReadToEnd();
                        Debug.LogError(errorMessage);
                        onErrorInfo(null, errorMessage);
                    }
                }

                voicesProcess.Dispose();
                //}
            }
            else
            {
                string errorMessage = "Could not find the TTS-wrapper: '" + application + "'";
                Debug.LogError(errorMessage);
                onErrorInfo(null, errorMessage);
            }
#else
            yield return null;
#endif

            onVoicesReady();
        }


#if (UNITY_STANDALONE_WIN || UNITY_EDITOR) && !UNITY_WEBPLAYER
        private void readSpeakNativeStream(ref System.Diagnostics.Process process, ref string[] speechTextArray, out int wordIndex, out string phoneme, out string viseme, out bool start)
        {
            wordIndex = 0;
            phoneme = string.Empty;
            viseme = string.Empty;
            start = false;

            try
            {
                process.Start();

                string reply;

                using (System.IO.StreamReader streamReader = process.StandardOutput)
                {
                    reply = streamReader.ReadLine();
                    if (reply.Equals(idSpeak))
                    {

                        while (!process.HasExited)
                        {
                            reply = streamReader.ReadLine();

                            if (!string.IsNullOrEmpty(reply))
                            {
                                if (reply.StartsWith(idWord))
                                {
                                    if (wordIndex < speechTextArray.Length)
                                    {
                                        if (speechTextArray[wordIndex].Equals("-"))
                                        {
                                            wordIndex++;
                                        }

                                        wordIndex++;
                                    }
                                    //else
                                    //{
                                    //    Debug.LogWarning("Word index is larger than the speech text word count: " + wordIndex + "/" + speechTextArray.Length);
                                    //}
                                }
                                else if (reply.StartsWith(idPhoneme))
                                {

                                    string[] splittedString = reply.Split(splitChar, System.StringSplitOptions.RemoveEmptyEntries);

                                    if (splittedString.Length > 1)
                                    {
                                        phoneme = splittedString[1];
                                    }
                                }
                                else if (reply.StartsWith(idViseme))
                                {

                                    string[] splittedString = reply.Split(splitChar, System.StringSplitOptions.RemoveEmptyEntries);

                                    if (splittedString.Length > 1)
                                    {
                                        viseme = splittedString[1];
                                    }
                                }
                                else if (reply.Equals(idStart))
                                {
                                    start = true;
                                }
                            }
                        }
                    }
                    else
                    {
                        Debug.LogError("Unexpected process output: " + reply + System.Environment.NewLine + streamReader.ReadToEnd());
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Could not speak: " + ex);
            }
        }

#endif

#if (UNITY_STANDALONE_WIN || UNITY_EDITOR) && !UNITY_WEBPLAYER
        private void startProcess(ref System.Diagnostics.Process process, int timeout = 0)
        {
            try
            {
                process.Start();

                if (timeout > 0)
                {
                    process.WaitForExit(timeout);
                }
                else
                {
                    process.WaitForExit(); //TODO good idea?
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Could not start process: " + ex);
            }
        }
#endif

        private string applicationName()
        {
            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                if (Util.Config.ENFORCE_32BIT_WINDOWS)
                {
                    return dataPath + Util.Config.TTS_WINDOWS_EDITOR_x86;
                }
                else
                {
                    return dataPath + Util.Config.TTS_WINDOWS_EDITOR;
                }
            }
            else
            {
                return dataPath + Util.Config.TTS_WINDOWS_BUILD;
            }
        }

        private static string prepareText(string text, float pitch)
        {
            if (pitch != 1f)
            {
                System.Text.StringBuilder sbXML = new System.Text.StringBuilder();

                sbXML.Append("<prosody");

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

                sbXML.Append(">");

                sbXML.Append(text);

                sbXML.Append("</prosody>");

                return sbXML.ToString().Replace('"', '\'');
            }

            return text.Replace('"', '\'');
        }

        private static int calculateVolume(float volume)
        {
            return Mathf.Clamp((int)(100 * volume), 0, 100);
        }

        private static int calculateRate(float rate)
        { //allowed range: 0 - 3f - all other values were cropped
            int result = 0;

            if (rate != 1f)
            { //relevant?
                if (rate > 1f)
                { //larger than 1
                    if (rate >= 2.75f)
                    {
                        result = 10; //2.78
                    }
                    else if (rate >= 2.6f && rate < 2.75f)
                    {
                        result = 9; //2.6
                    }
                    else if (rate >= 2.35f && rate < 2.6f)
                    {
                        result = 8; //2.39
                    }
                    else if (rate >= 2.2f && rate < 2.35f)
                    {
                        result = 7; //2.2
                    }
                    else if (rate >= 2f && rate < 2.2f)
                    {
                        result = 6; //2
                    }
                    else if (rate >= 1.8f && rate < 2f)
                    {
                        result = 5; //1.8
                    }
                    else if (rate >= 1.6f && rate < 1.8f)
                    {
                        result = 4; //1.6
                    }
                    else if (rate >= 1.4f && rate < 1.6f)
                    {
                        result = 3; //1.45
                    }
                    else if (rate >= 1.2f && rate < 1.4f)
                    {
                        result = 2; //1.28
                    }
                    else if (rate > 1f && rate < 1.2f)
                    {
                        result = 1; //1.14
                    }
                }
                else
                { //smaller than 1
                    if (rate <= 0.3f)
                    {
                        result = -10; //0.33
                    }
                    else if (rate > 0.3 && rate <= 0.4f)
                    {
                        result = -9; //0.375
                    }
                    else if (rate > 0.4 && rate <= 0.45f)
                    {
                        result = -8; //0.42
                    }
                    else if (rate > 0.45 && rate <= 0.5f)
                    {
                        result = -7; //0.47
                    }
                    else if (rate > 0.5 && rate <= 0.55f)
                    {
                        result = -6; //0.525
                    }
                    else if (rate > 0.55 && rate <= 0.6f)
                    {
                        result = -5; //0.585
                    }
                    else if (rate > 0.6 && rate <= 0.7f)
                    {
                        result = -4; //0.655
                    }
                    else if (rate > 0.7 && rate <= 0.8f)
                    {
                        result = -3; //0.732
                    }
                    else if (rate > 0.8 && rate <= 0.9f)
                    {
                        result = -2; //0.82
                    }
                    else if (rate > 0.9 && rate < 1f)
                    {
                        result = -1; //0.92
                    }
                }
            }

            if (Util.Constants.DEV_DEBUG)
                Debug.Log("calculateRate: " + result + " - " + rate);

            return result;
        }

        #endregion


        #region Editor-only methods

#if UNITY_EDITOR

        public override void GenerateInEditor(Model.Wrapper wrapper)
        {
#if !UNITY_WEBPLAYER
            if (wrapper == null)
            {
                Debug.LogWarning("'wrapper' is null!");
            }
            else
            {
                if (string.IsNullOrEmpty(wrapper.Text))
                {
                    Debug.LogWarning("'Text' is null or empty: " + wrapper);
                }
                else
                {
                    string application = applicationName();

                    if (System.IO.File.Exists(application))
                    {
                        int calculatedRate = calculateRate(wrapper.Rate);
                        int calculatedVolume = calculateVolume(wrapper.Volume);

                        string voiceName = string.Empty;

                        if (wrapper.Voice == null || string.IsNullOrEmpty(wrapper.Voice.Name))
                        {
                            if (Util.Config.DEBUG)
                                Debug.LogWarning("'Voice' or 'Voice.Name' is null! Using the OS 'default' voice.");
                        }
                        else
                        {
                            voiceName = wrapper.Voice.Name;
                        }

                        using (System.Diagnostics.Process speakToFileProcess = new System.Diagnostics.Process())
                        {
                            string outputFile = Util.Config.AUDIOFILE_PATH + wrapper.Uid + extension;

                            string args = "--speakToFile" + " \"" +
                                          prepareText(wrapper.Text, wrapper.Pitch) + "\" \"" +
                                          outputFile.Replace('"', '\'') + "\" " +
                                          calculatedRate.ToString() + " " +
                                          calculatedVolume.ToString() + " \"" +
                                          voiceName.Replace('"', '\'') + '"';

                            if (Util.Config.DEBUG)
                                Debug.Log("Process argruments: " + args);

                            speakToFileProcess.StartInfo.FileName = application;
                            speakToFileProcess.StartInfo.Arguments = args;
                            speakToFileProcess.StartInfo.CreateNoWindow = true;
                            speakToFileProcess.StartInfo.RedirectStandardOutput = true;
                            speakToFileProcess.StartInfo.RedirectStandardError = true;
                            speakToFileProcess.StartInfo.UseShellExecute = false;
                            speakToFileProcess.StartInfo.StandardOutputEncoding = System.Text.Encoding.UTF8;

                            speakToFileProcess.Start();

                            silence = false;
                            onSpeakAudioGenerationStart(wrapper);

                            do
                            {
                                System.Threading.Thread.Sleep(50);
                            } while (!speakToFileProcess.HasExited);

                            if (speakToFileProcess.ExitCode == 0)
                            {

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

                                if (Util.Config.DEBUG)
                                    Debug.Log("Text generated: " + wrapper.Text);
                            }
                            else
                            {
                                using (System.IO.StreamReader sr = speakToFileProcess.StandardError)
                                {
                                    string errorMessage = "Could not generate the text: " + speakToFileProcess.ExitCode + System.Environment.NewLine + sr.ReadToEnd();
                                    Debug.LogError(errorMessage);
                                    onErrorInfo(wrapper, errorMessage);
                                }
                            }

                            onSpeakAudioGenerationComplete(wrapper);
                        }
                    }
                    else
                    {
                        string errorMessage = "Could not find the TTS-wrapper: '" + application + "'";
                        Debug.LogError(errorMessage);
                        onErrorInfo(wrapper, errorMessage);
                    }
                }
            }
#endif
        }

        public override void SpeakNativeInEditor(Model.Wrapper wrapper)
        {
#if !UNITY_WEBPLAYER
            if (wrapper == null)
            {
                Debug.LogWarning("'wrapper' is null!");
            }
            else
            {
                if (string.IsNullOrEmpty(wrapper.Text))
                {
                    Debug.LogWarning("'Text' is null or empty!");
                }
                else
                {
                    string application = applicationName();

                    if (System.IO.File.Exists(application))
                    {
                        int calculatedRate = calculateRate(wrapper.Rate);
                        int calculatedVolume = calculateVolume(wrapper.Volume);

                        string voiceName = string.Empty;

                        if (wrapper.Voice == null || string.IsNullOrEmpty(wrapper.Voice.Name))
                        {
                            if (Util.Config.DEBUG)
                                Debug.LogWarning("'Voice' or 'Voice.Name' is null! Using the OS 'default' voice.");
                        }
                        else
                        {
                            voiceName = wrapper.Voice.Name;
                        }

                        using (System.Diagnostics.Process speakProcess = new System.Diagnostics.Process())
                        {

                            string args = "--speak " + '"' +
                                          prepareText(wrapper.Text, wrapper.Pitch) + "\" " +
                                          calculatedRate.ToString() + " " +
                                          calculatedVolume.ToString() + " \"" +
                                          voiceName.Replace('"', '\'') + '"';

                            if (Util.Config.DEBUG)
                                Debug.Log("Process argruments: " + args);

                            speakProcess.StartInfo.FileName = application;
                            speakProcess.StartInfo.Arguments = args;
                            speakProcess.StartInfo.CreateNoWindow = true;
                            speakProcess.StartInfo.RedirectStandardOutput = true;
                            speakProcess.StartInfo.RedirectStandardError = true;
                            speakProcess.StartInfo.UseShellExecute = false;
                            speakProcess.StartInfo.StandardOutputEncoding = System.Text.Encoding.UTF8;

                            speakProcess.Start();

                            silence = false;
                            onSpeakStart(wrapper);

                            do
                            {
                                System.Threading.Thread.Sleep(50);

                                if (silence)
                                {
                                    speakProcess.Kill();
                                }
                            } while (!speakProcess.HasExited);

                            if (speakProcess.ExitCode == 0 || speakProcess.ExitCode == -1)
                            { //0 = normal ended, -1 = killed
                                if (Util.Config.DEBUG)
                                    Debug.Log("Text spoken: " + wrapper.Text);
                            }
                            else
                            {
                                using (System.IO.StreamReader sr = speakProcess.StandardError)
                                {
                                    Debug.LogError("Could not speak the text: " + speakProcess.ExitCode + System.Environment.NewLine + sr.ReadToEnd());
                                }
                            }

                            onSpeakComplete(wrapper);
                        }
                    }
                    else
                    {
                        string errorMessage = "Could not find the TTS-wrapper: '" + application + "'";
                        Debug.LogError(errorMessage);
                        onErrorInfo(wrapper, errorMessage);
                    }
                }
            }
#endif
        }

#endif

        private void getVoicesInEditor()
        {

#if (UNITY_STANDALONE_WIN || UNITY_EDITOR) && !UNITY_WEBPLAYER
            string application = applicationName();

            if (System.IO.File.Exists(application))
            {
                using (System.Diagnostics.Process voicesProcess = new System.Diagnostics.Process())
                {
                    voicesProcess.StartInfo.FileName = application;
                    voicesProcess.StartInfo.Arguments = "--voices";
                    voicesProcess.StartInfo.CreateNoWindow = true;
                    voicesProcess.StartInfo.RedirectStandardOutput = true;
                    voicesProcess.StartInfo.RedirectStandardError = true;
                    voicesProcess.StartInfo.UseShellExecute = false;
                    voicesProcess.StartInfo.StandardOutputEncoding = System.Text.Encoding.UTF8;
                    //* Set your output and error (asynchronous) handlers
                    //voicesProcess.OutputDataReceived += new DataReceivedEventHandler(speakNativeHandler);
                    //voicesProcess.ErrorDataReceived += new DataReceivedEventHandler(ErrorHandler);

                    try
                    {
                        voicesProcess.Start();
                        //voicesProcess.BeginOutputReadLine();
                        //voicesProcess.BeginErrorReadLine();

                        float time = Time.realtimeSinceStartup;

                        voicesProcess.WaitForExit(Util.Constants.DEFAULT_TTS_KILL_TIME);

                        if (Util.Constants.DEV_DEBUG)
                            Debug.Log("Finished after: " + (Time.realtimeSinceStartup - time));

                        if (voicesProcess.ExitCode == 0)
                        {
                            cachedVoices.Clear();

                            using (System.IO.StreamReader streamReader = voicesProcess.StandardOutput)
                            {
                                string reply;
                                while (!streamReader.EndOfStream)
                                {
                                    reply = streamReader.ReadLine();

                                    if (!string.IsNullOrEmpty(reply))
                                    {
                                        if (reply.StartsWith(idVoice))
                                        {

                                            string[] splittedString = reply.Split(splitChar, System.StringSplitOptions.RemoveEmptyEntries);

                                            if (splittedString.Length == 6)
                                            {
                                                cachedVoices.Add(new Model.Voice(splittedString[1], splittedString[2], splittedString[3], splittedString[4], splittedString[5]));
                                            }
                                            else
                                            {
                                                Debug.LogWarning("Voice is invalid: " + reply);
                                            }
                                            //                     } else if(reply.Equals("@DONE") || reply.Equals("@COMPLETED")) {
                                            //                        complete = true;
                                        }
                                    }
                                }
                            }

                            if (Util.Constants.DEV_DEBUG)
                                Debug.Log("Voices read: " + cachedVoices.CTDump());
                        }
                        else
                        {
                            using (System.IO.StreamReader sr = voicesProcess.StandardError)
                            {
                                string errorMessage = "Could not get any voices: " + voicesProcess.ExitCode + System.Environment.NewLine + sr.ReadToEnd();
                                Debug.LogError(errorMessage);
                            }
                        }

                    }
                    catch (System.Exception ex)
                    {
                        string errorMessage = "Could not get any voices!" + System.Environment.NewLine + ex;
                        Debug.LogError(errorMessage);
                    }
                }
            }
            else
            {
                string errorMessage = "Could not find the TTS-wrapper: '" + application + "'";
                Debug.LogError(errorMessage);
            }
#endif
        }

        #endregion
    }
}
// © 2015-2018 crosstales LLC (https://www.crosstales.com)