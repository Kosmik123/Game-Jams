// localStorage dla zapisu gry (stale klucze z prefiksem payWithPixels_v1_ po stronie C#; patrz Save.cs).
// W odroznieniu od PlayerPrefs (IndexedDB zalezne od adresu builda) przetrwa kolejne uploady na itch.io.
mergeInto(LibraryManager.library, {
  PwpHas: function (k) {
    try { return window.localStorage.getItem(UTF8ToString(k)) !== null ? 1 : 0; } catch (e) { return 0; }
  },
  PwpGet: function (k) {
    var v = null;
    try { v = window.localStorage.getItem(UTF8ToString(k)); } catch (e) { }
    if (v === null) v = "";
    var n = lengthBytesUTF8(v) + 1;
    var b = _malloc(n);
    stringToUTF8(v, b, n);
    return b;
  },
  PwpSet: function (k, v) {
    try { window.localStorage.setItem(UTF8ToString(k), UTF8ToString(v)); } catch (e) { }
  },
  PwpRemove: function (k) {
    try { window.localStorage.removeItem(UTF8ToString(k)); } catch (e) { }
  },
  PwpPrompt: function (msg, def) {
    var r = null;
    try { r = window.prompt(UTF8ToString(msg), UTF8ToString(def)); } catch (e) { }
    if (r === null) r = "";
    var n = lengthBytesUTF8(r) + 1;
    var b = _malloc(n);
    stringToUTF8(r, b, n);
    return b;
  }
});
