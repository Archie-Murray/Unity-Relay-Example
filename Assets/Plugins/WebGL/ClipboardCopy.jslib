mergeInto(LibraryManager.library, {
  CopyToClipboard: function(str) {
    var tempInput = document.createElement("input");
    tempInput.value = UTF8ToString(str);
    document.body.appendChild(tempInput);
    tempInput.select();
    document.execCommand("copy");
    document.body.removeChild(tempInput);
  },
});
