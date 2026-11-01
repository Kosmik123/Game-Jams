using UnityEngine;

namespace ZeroHour.Tower
{
    // Debug mode is switched on by "debug=1" in the page address (e.g. .../0h-game-jam-2026?debug=1).
    // It never loses the round, so the economy can be tried without the end.
    public static class DebugMode
    {
        public static bool IsEnabled => FromUrl(Application.absoluteURL);

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
