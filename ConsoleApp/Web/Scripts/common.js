function evaluateResponse(data, showSuccess) {
    var message = null;
    if (data.Messages !== undefined && data.Messages.length > 0) {
        message = data.Messages.join("\n");
    }
    if (data.Success !== true) {
        showErrorAlert(message);
        return false;
    }
    if (showSuccess === true) {
        showSuccessAlert(message);
    }
    return true;
}

function showErrorAlert(message) {
    showAlert("errorAlert", 500, message);
}

function showSuccessAlert(message, fadeOutTime) {
    if (fadeOutTime == null) {
        fadeOutTime = 2000;
    }

    showAlert("successAlert", 1000, message, fadeOutTime);
}

function showAlert(spanName, fadeInTime, message, fadeOutTime) {
    if (message != null) {
        $("#" + spanName + " span").text(message);
    } else {
        $("#" + spanName + " span").text($("#" + spanName).data().message);
    }

    var div = $("#" + spanName);
    div.show();
    div.removeClass("hide");
    if (fadeOutTime !== undefined && fadeOutTime > 0) {
        div.delay(fadeInTime).addClass("in").fadeOut(fadeOutTime);
    } else {
        div.delay(fadeInTime).addClass("in");
    }
    CentrarPosicionElemento();
}
