using UnityEngine;

namespace Crosstales.RTVoice.Util
{
    /// <summary>Various helper functions.</summary>
    public abstract class Helper : Common.Util.BaseHelper
    {

        #region Static properties

        /// <summary>Checks if the current platform has built-in TTS.</summary>
        /// <returns>True if the current platform has built-in TTS.</returns>
        public static bool hasBuiltInTTS
        {
            get
            {
                return isWindowsPlatform || isMacOSPlatform || isAndroidPlatform || isIOSPlatform || isWSAPlatform;
            }
        }

        /// <summary>The current provider type.</summary>
        /// <returns>Current provider type.</returns>
        public static Model.Enum.ProviderType CurrentProviderType
        {
            get
            {
                if (Speaker.isMaryMode)
                    return Model.Enum.ProviderType.MaryTTS;

                if (isWindowsPlatform)
                    return Model.Enum.ProviderType.Windows;

                if (isAndroidPlatform)
                    return Model.Enum.ProviderType.Android;

                if (isIOSPlatform)
                    return Model.Enum.ProviderType.iOS;

                if (isWSAPlatform)
                    return Model.Enum.ProviderType.Windows;

                //if (isMacOSPlatform)
                return Model.Enum.ProviderType.macOS;
            }
        }

        #endregion


        #region Static methods

        /// <summary>Cleans a given text to contain only letters or digits.</summary>
        /// <param name="text">Text to clean.</param>
        /// <param name="removeTags">Removes tags from text (default: true, optional).</param>
        /// <param name="clearSpaces">Clears multiple spaces from text (default: true, optional).</param>
        /// <param name="clearLineEndings">Clears line endings from text (default: true, optional).</param>
        /// <returns>Clean text with only letters and digits.</returns>
        public static string CleanText(string text, bool removeTags = true, bool clearSpaces = true, bool clearLineEndings = true)
        {
            string result = text;

            if (removeTags)
            {
                result = ClearTags(result);
            }

            if (clearSpaces)
            {
                result = ClearSpaces(result);
            }

            if (clearLineEndings)
            {
                result = ClearLineEndings(result);
            }

            //Debug.Log (result);

            return result;
        }

        /// <summary>Marks the current word or all spoken words from a given text array.</summary>
        /// <param name="speechTextArray">Array with all text fragments</param>
        /// <param name="wordIndex">Current word index</param>
        /// <param name="markAllSpokenWords">Mark the spoken words (default: false, optional)</param>
        /// <param name="markPrefix">Prefix for every marked word (default: green, optional)</param>
        /// <param name="markPostfix">Postfix for every marked word (default: green, optional)</param>
        /// <returns>Marked current word or all spoken words.</returns>
        public static string MarkSpokenText(string[] speechTextArray, int wordIndex, bool markAllSpokenWords = false, string markPrefix = "<color=green><b>", string markPostfix = "</b></color>")
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            if (speechTextArray == null)
            {
                Debug.LogWarning("The given 'speechTextArray' is null!");
            }
            else
            {
                if (wordIndex < 0 || wordIndex > speechTextArray.Length - 1)
                {
                    Debug.LogWarning("The given 'wordIndex' is invalid: " + wordIndex);
                }
                else
                {
                    for (int ii = 0; ii < wordIndex; ii++)
                    {

                        if (markAllSpokenWords)
                            sb.Append(markPrefix);
                        sb.Append(speechTextArray[ii]);
                        if (markAllSpokenWords)
                            sb.Append(markPostfix);
                        sb.Append(" ");
                    }

                    sb.Append(markPrefix);
                    sb.Append(speechTextArray[wordIndex]);
                    sb.Append(markPostfix);
                    sb.Append(" ");

                    for (int ii = wordIndex + 1; ii < speechTextArray.Length; ii++)
                    {
                        sb.Append(speechTextArray[ii]);
                        sb.Append(" ");
                    }
                }
            }

            return sb.ToString();
        }

        #endregion

    }
}
// © 2015-2018 crosstales LLC (https://www.crosstales.com)