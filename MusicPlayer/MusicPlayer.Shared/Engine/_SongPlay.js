function Main() {
    var canvas = document.getElementById("canvas");
    var audio = document.getElementById("audio");

    var url = decodeURIComponent(atob("$Url"));

    if ($isLocalFile) {
        window.ipcRenderer.send("requestBlobUrl", url);
        window.ipcRenderer.on("getBlobUrl", (event, blobUrl) => Play(blobUrl));
    } else Play(url);

    function Play(playBackUrl) {
        audio.pause();
        audio.currentTime = 0;
        audio.crossOrigin = "anonymous";
        audio.src = playBackUrl;

        if (!window.audioContext) window.audioContext = new (window.AudioContext || window.webkitAudioContext);

        var useRenderFrame = this.source == undefined;

        if (this.source == undefined) this.source = window.audioContext.createMediaElementSource(audio);


        if (this.source && this.analyser) {
            this.source.disconnect(this.analyser);
            this.analyser.disconnect(window.audioContext.destination);
        }

        this.analyser = window.audioContext.createAnalyser();
        this.source.connect(this.analyser);
        this.analyser.connect(window.audioContext.destination);

        var ctx = canvas.getContext("2d");

        var WIDTH;
        var HEIGHT;

        var bufferLength;
        var dataArray;

        var barWidth;

        var barHeight;
        var barDistance = 10;
        var x;

        renderSize();

        window.onresize = function() {
            renderSize();
        };

        function renderSize() {
            canvas.width = (window.innerWidth - 40);
            canvas.height = 700;

            WIDTH = canvas.width;
            HEIGHT = canvas.height;

            analyser.fftSize = 4096;

            bufferLength = analyser.frequencyBinCount;
            dataArray = new Uint8Array(bufferLength);

            barWidth = 12;

            x = 0;
        }

        function renderFrame() {
            requestAnimationFrame(renderFrame);

            x = 0;

            analyser.getByteFrequencyData(dataArray);

            ctx.fillStyle = "rgba(3, 71, 161, 1)";
            ctx.fillRect(0, 0, WIDTH, HEIGHT);

            var r, g, b;
            var bars = (WIDTH / (barWidth + barDistance)) - 0.5;

            for (let i = 0; i < bars; i++) {
                barHeight = (dataArray[i] * 2.5);

                if (dataArray[i] > 210) {
                    r = 239;
                    g = 83;
                    b = 80;
                } else if (dataArray[i] > 200) {
                    r = 236;
                    g = 64;
                    b = 122;
                } else if (dataArray[i] > 190) {
                    r = 236;
                    g = 64;
                    b = 122;
                } else if (dataArray[i] > 180) {
                    r = 186;
                    g = 104;
                    b = 200;
                } else if (dataArray[i] > 120) {
                    r = 255;
                    g = 167;
                    b = 38;
                } else {
                    r = 250;
                    g = 250;
                    b = 250;
                }
                var grad = ctx.createLinearGradient(0, 0, 0, HEIGHT);
                grad.addColorStop(0.5, `rgb(${r},${g},${b})`);
                grad.addColorStop(1, "rgba(3, 71, 161, 1)");
                ctx.fillStyle = grad;
                ctx.fillRect(x, (HEIGHT - barHeight), barWidth, barHeight);

                x += barWidth + barDistance;
            }
        }

        audio.play();

        if (useRenderFrame) renderFrame();
    }
}

Main();