function Main() {
    const data = JSON.parse(atob("$Json"));

    var first = data.find(function(link) {
        return link["format"].indexOf("webm, audio") !== -1;
    });

    return btoa(encodeURIComponent(first["url"]));
}

Main();