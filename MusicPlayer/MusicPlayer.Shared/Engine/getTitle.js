function Main() {
    var tmpTitle = document.getElementById("tmpTitle");
    var type = "$Type";

    if (type == "youtube") {
        const data = JSON.parse(atob("$Json"));
        tmpTitle.innerHTML = btoa(encodeURIComponent(data.title));
    } else if (type == "file") {
        var url = decodeURIComponent(atob("$Url"));

        var fileName = decodeURIComponent(atob("$FileName"));
        var fileExt = fileName.slice((fileName.lastIndexOf(".") - 1 >>> 0) + 2);

        if ($isLocalFile) {
            window.ipcRenderer.send("requestBlobUrl", url);
            window.ipcRenderer.on("getBlobUrl", (event, blobUrl) => getTitle(blobUrl));
        } else getTitle(url);

        async function getTitle(blobUrl) {
            let blob = await fetch(blobUrl).then(r => r.blob())
                .then(blobFile => new File([blobFile], fileName, { type: ("audio/" + fileExt) }));

            window.jsmediatags.read(blob,
                {
                    onSuccess: function(tags) {
                        if (tags.tags.title == undefined) tmpTitle.innerHTML = btoa("null");
                        else tmpTitle.innerHTML = btoa(encodeURIComponent(tags.tags.title));
                    },
                    onError: function() {
                        tmpTitle.innerHTML = btoa("null");
                    }
                });
        }
    } else {
        tmpTitle.innerHTML = btoa("null");
    }
}

Main();