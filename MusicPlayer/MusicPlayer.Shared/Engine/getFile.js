function Main() {
    var input = document.getElementById("select");

    if (input.files[0] == null) {
        return null;
    }

    return input.files[0].name;
}

Main();