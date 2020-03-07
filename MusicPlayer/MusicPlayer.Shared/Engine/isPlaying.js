function Main() {
    var audio = document.getElementById("audio");

    return (!audio.paused).toString();
}

Main();