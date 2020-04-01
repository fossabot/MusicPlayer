function Main() {
    var input = document.getElementById("select");

    var files = [];
    var urls = [];

    Array.prototype.forEach.call(input.files,
        file => {
            files.push(btoa(encodeURIComponent(file.name)));
            urls.push(btoa(encodeURIComponent($isPwaWrapper ? file.path : URL.createObjectURL(file))));
        });

    input.value = "";

    if (files.length == 0 || urls.length == 0 || files.length != urls.length) return null;

    return files.toString() + "|" + urls.toString();
}

Main();