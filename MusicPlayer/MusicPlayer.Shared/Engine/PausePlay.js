function Main() {
    var audio = document.getElementById("audio");

    if (audio.paused) {
        audio.play();
    } else {
        if (audio.duration == Number.POSITIVE_INFINITY) {
            audio.src = audio.src;
        }

        audio.pause();
    }
}

Main();