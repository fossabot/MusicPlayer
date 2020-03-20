function Main() {
    var input = document.getElementById("select");

    if (input.value == null || input.value == "") {
        return null;
    }

    return input.value.toString();
}

Main();