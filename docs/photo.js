const baseUrl = "https://photo-3d.eastus.cloudapp.azure.com";
//const baseUrl = "https://localhost:44391";

const apiUrl = baseUrl + "/photorestoration";
const testUrl = baseUrl + "/test";

let dzError = false;
let dropzone;

$(document).ready(function () {
    TestConnectivity();
	setupDropFilesBox();
	$("#btn-go").click(e => {
		Go();
    });
    $("#btn-close-wait").on("click", function () {
        stopWait();
    });
});

function setupDropFilesBox() {
    $("#uploader").addClass('dropzone');
    dropzone = new Dropzone("#uploader", {
        url: apiUrl + '/p',
        paramName: "file",
        maxFilesize: 12, // MB
        maxFiles: 5,
        timeout: 3600000,
        clickable: true,
        acceptedFiles: "image/*",
        uploadMultiple: true,
        createImageThumbnails: true,
        parallelUploads: 5,
        method: "post",
        dictDefaultMessage: "Drop images here or Click to upload",
        successmultiple: onFileUploadCompleted,
		autoProcessQueue: false,
		addRemoveLinks: true,
        errormultiple: function (f, errorMessage) {
            if (!dzError) {
                dzError = true;
                stopWait();
                alert("Some files cannot be processed:\n" + errorMessage);
            }
        }
    });
}


function onFileUploadCompleted(f, response) {
    stopWait();
    dropzone.removeAllFiles();
    if (response.error) {
        dzError = true;
        alert(response.error);
    } else {
        // download file
        console.log("Successful process: " + JSON.stringify(response));
        let downloadUrl = apiUrl + "/d?t=" + response.traceId;
        $("<li>").html("<a href='" + downloadUrl + "'>" + downloadUrl + "</a>").appendTo("#download-links-list");
        $("#download-links-div").show();
        window.open(downloadUrl);
    }
}

function startWait() {
    $("#spinner").show();
    $("div.dz-preview").css("z-index", "0");
    $("#wait-dialog").modal({
        escapeClose: false,
        clickClose: false,
        showClose: false,
        fadeDuration: 100
    });

    //$("#div-main").find("*").addClass('wait');
}

function stopWait() {
    $("div.dz-preview").css("z-index", "auto");
    $("#spinner").hide();
    //$("#div-main").find("*").removeClass('wait');
    $.modal.close();
    $("#wait-dialog").hide();
}

function Go() {
	dzError = false;
    if (dropzone.getQueuedFiles().length === 0) {
		return;
	}

    // Extra parameters for process request
    $("#gpu-param").val($("#cpu-checkbox").is(":checked") ? "-1" : "0");
    $("#reformat-param").val($("#reformat-checkbox").is(":checked"));
    $("#scratched-param").val($("#scratched-checkbox").is(":checked"));

    startWait();
    dropzone.processQueue(); 
}


function TestConnectivity() {
    $("#server-down-text").html('');
    var opts = {
        method: 'GET',
        headers: {}
    };
    fetchTimeout(testUrl, opts, 4000)
        .then(function (response) {
            $("#server-down-div").toggle(!response.ok);
        })
        .catch(function (error) {
            $("#server-down-div").show();
            $("#server-down-text").html(error);
        });

}

function fetchTimeout(url, options, timeout) {
    return Promise.race([
        fetch(url, options),
        new Promise((_, reject) =>
            setTimeout(() => reject(new Error('timeout')), timeout)
        )
    ]);
}