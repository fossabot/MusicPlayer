function Main() {
    var audio = document.getElementById("audio");

    if (audio.paused) {
        audio.play();
    } else {
        audio.pause();
    }
}

Main();