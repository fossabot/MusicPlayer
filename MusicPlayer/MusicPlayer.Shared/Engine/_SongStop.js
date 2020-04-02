function Main() {
    var audio = document.getElementById("audio");

    audio.src = "data:audio/wav;base64,UklGRiQAAABXQVZFZm10IBAAAAABAAEAVFYAAFRWAAABAAgAZGF0YQAAAAA=";
    if (audio.play() !== undefined) { audio.play().then(() => audio.pause()) }
}

Main();