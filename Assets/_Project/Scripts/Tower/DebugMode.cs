using UnityEngine;

namespace ZeroHour.Tower
{
    // Debug mode never loses the round, so the economy can be tried without the end. It is on when the page
    // address has "debug=1" (works when the game runs on its own page) or when the switch in the version
    // menu is on (itch.io runs the game in a frame that doesn't get the address of the game page).
    public static class DebugMode
    {
        private const string StoredKey = "DebugMode";

        public static bool IsEnabled => IsStored || FromUrl(Application.absoluteURL);

        // The switch in the version menu; it is remembered between visits.
        public static bool IsStored => PlayerPrefs.GetInt(StoredKey, 0) == 1;

        public static void SetStored(bool isOn)
        {
            PlayerPrefs.SetInt(StoredKey, isOn ? 1 : 0);
            PlayerPrefs.Save(); // WebGL: written to IndexedDB right away
        }

        // True when the query string of the address has debug=1.
        public static bool FromUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
                return false;

            int queryStart = url.IndexOf('?');
            if (queryStart < 0)
                return false;

            string query = url.Substring(queryStart + 1);
            int fragmentStart = query.IndexOf('#');
            if (fragmentStart >= 0)
                query = query.Substring(0, fragmentStart);

            foreach (string pair in query.Split('&'))
            {
                if (pair == "debug=1")
                    return true;
            }
            return false;
        }
    }
}
