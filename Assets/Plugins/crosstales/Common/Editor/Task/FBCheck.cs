using UnityEngine;
using UnityEditor;

namespace Crosstales.Common.EditorTask
{
    /// <summary>Checks if 'File Browser' is installed.</summary>
    [InitializeOnLoad]
    public static class FBCheck
    {
        private const string KEY_FBCHECK_DATE = "CT_CFG_FBCHECK_DATE";

        #region Constructor

        static FBCheck()
        {
            string lastDate = EditorPrefs.GetString(KEY_FBCHECK_DATE);
            string date = System.DateTime.Now.ToString("yyyyMMdd"); // every day
            //string date = System.DateTime.Now.ToString("yyyyMMddHH"); // every hour
            //string date = System.DateTime.Now.ToString("yyyyMMddHHmm"); // every minute (for tests)

            if (!date.Equals(lastDate))
            {
#if !CT_FB
                Debug.LogWarning("+++ No native file browser found. Please consider using 'File Browser': https://goo.gl/GCmzrU +++");
#endif

                EditorPrefs.SetString(KEY_FBCHECK_DATE, date);
            }
        }

        #endregion

    }
}
// © 2017-2018 crosstales LLC (https://www.crosstales.com)