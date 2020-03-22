function Main() {
    var canvas = document.getElementById("canvas");
    var audio = document.getElementById("audio");
    var input = document.getElementById("select");
    var title = document.getElementById("title");
    title.innerHTML = "RH Music Player";

    audio.crossOrigin = "anonymous";

    function PlayYouTube(youtube_url) {
        var getJSON = function(url, callback) {
            var xhr = new XMLHttpRequest();
            xhr.open("GET", url, true);
            xhr.responseType = "json";
            xhr.onload = function() {
                var status = xhr.status;
                if (status === 200) {
                    callback(null, xhr.response);
                } else {
                    callback(status, xhr.response);
                }
            };
            xhr.send();
        };

        getJSON("https://stream.api.rh-utensils.hampoelz.net/getLinks.php?url=" + youtube_url,
            function(err, data) {
                if (err == null) {
                    var first = data.find(function(link) {
                        return link["format"].indexOf("webm, audio") !== -1;
                    });

                    audio.src = "https://stream.api.rh-utensils.hampoelz.net/stream.php?url=" +
                        encodeURIComponent(first["url"]);
                }
            });

        getJSON("https://stream.api.rh-utensils.hampoelz.net/getInfos.php?url=" + youtube_url,
            function(err, data) {
                if (err == null) {
                    title.innerHTML = data["title"];
                }
            });
    }

    function YouTubeValidator(youtube_url) {
        var p =
            /^(?:https?:\/\/)?(?:www\.)?(?:youtu\.be\/|youtube\.com\/(?:embed\/|v\/|watch\?v=|watch\?.+&v=))((\w|-){11})(?:\S+)?$/;
        return (youtube_url.match(p));
    }

    function UrlValidator(str) {
        var p =
            /^(?:(?:https?|ftp):\/\/)?(?:(?!(?:10|127)(?:\.\d{1,3}){3})(?!(?:169\.254|192\.168)(?:\.\d{1,3}){2})(?!172\.(?:1[6-9]|2\d|3[0-1])(?:\.\d{1,3}){2})(?:[1-9]\d?|1\d\d|2[01]\d|22[0-3])(?:\.(?:1?\d{1,2}|2[0-4]\d|25[0-5])){2}(?:\.(?:[1-9]\d?|1\d\d|2[0-4]\d|25[0-4]))|(?:(?:[a-z\u00a1-\uffff0-9]-*)*[a-z\u00a1-\uffff0-9]+)(?:\.(?:[a-z\u00a1-\uffff0-9]-*)*[a-z\u00a1-\uffff0-9]+)*(?:\.(?:[a-z\u00a1-\uffff]{2,})))(?::\d{2,5})?(?:\/\S*)?$/;
        return (str.match(p));
    }

    function getYouTubeID(youtube_url) {
        var p =
            /^(?:https?:\/\/)?(?:www\.)?(?:youtu\.be\/|youtube\.com\/(?:embed\/|v\/|watch\?v=|watch\?.+&v=))((\w|-){11})(?:\S+)?$/;
        return (youtube_url.match(p)) ? RegExp.$1 : false;
    }

    if (input.files[0] != null && input.files[0].name.endsWith(".radio")) {
        var fileReader = new FileReader();

        fileReader.onload = function(e) {
            if (input.files[0] != null) title.innerHTML = input.files[0].name;

            if (YouTubeValidator(fileReader.result)) {
                PlayYouTube("https://www.youtube.com/watch?v=" + getYouTubeID(fileReader.result));
            } else if (UrlValidator(fileReader.result)) {
                audio.src = "https://stream.api.rh-utensils.hampoelz.net/stream.php?url=" +
                    encodeURIComponent(fileReader.result);
            }
        };

        fileReader.readAsText(input.files[0]);

        input.value = "";
    } else {

        var url = URL.createObjectURL(input.files[0]);

        if (input.files[0] != null) title.innerHTML = input.files[0].name;

        if (YouTubeValidator(url)) {
            PlayYouTube("https://www.youtube.com/watch?v=" + getYouTubeID(url));
        } else if (UrlValidator(url)) {
            audio.src = "https://stream.api.rh-utensils.hampoelz.net/stream.php?url=" +
                encodeURIComponent(url);
        } else {
            audio.src = url;
        }
    }

    if (!window.audioContext) {
        window.audioContext = new (window.AudioContext || window.webkitAudioContext);
    }

    if (this.source == undefined) {
        this.source = window.audioContext.createMediaElementSource(audio);
    }

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

    window.onresize = function(event) {
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

    renderFrame();
}

Main();