// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.



var baseUrl = "/";
function LocationUrl() {
    baseUrl = $("#txt_baseUrl").val();
    return baseUrl;
}

function toasterBox(message, type = "success") {
    var head = "";
    var icons = "";
    var loader = "";
    var bg = "";
    switch (type) {
        case "success":
            head = "Success!";
            icons = "success";
            loader = "#5ba035"
            bg = "#8fce00";
            break;
        case 'info':
            head = "Information"
            icons = "info";
            loader = "#3b98b5";
            bg = "#3d85c6";
            break;
        case 'warning':
            head = "Warning!";
            icons = "warning";
            loader = "#da8609";
            bg = "#fc750a";
            break;
        case 'danger':
            head = "Alert!";
            icons = "danger";
            loader = "#b83206";
            bg = "#FF0000";
        default:
    }

    $.toast({
        heading: head,
        icon: icons,
        loaderBg: loader,
        position: 'top-right',
        allowToastClose: true,
        hideAfter: 2000,
        showHideTransition: 'plain',
        text: message,
        bgColor: bg
    })
}

function messageBox(message, type = "success") {
    var title = "";

    switch (type) {
        case "danger": title = 'Alert!'; break;
        case "info": title = 'Information'; break;
        case "warning": title = 'Warning!'; break;
        case "success": title = 'Success!'; break;
        default:
    }

    if (type == "danger") type = "error";

    if (typeof (message) === "object") {
        //Swal.fire(title, 'Error Occured! - Please see error logs for more information', type);

        Swal.fire({
            icon: type,
            /*title: title,*/
            text: 'Error Occured! - Please see error logs for more information',
            showConfirmButton: false,
            timer: 3000
        });

    } else {
        Swal.fire({
            icon: type,
            /*title: title,*/
            text: message,
            showConfirmButton: false,
            timer: 3000
        });
    } //Swal.fire(title, message, type);
}

function toasterBox(message, type = "success") {
    var head = "";
    var icons = "";
    var loader = "";
    var bg = "";
    switch (type) {
        case "success":
            head = "Success!";
            //icons = "success";
            loader = "#5ba035"
            bg = "#8fce00";
            break;
        case 'info':
            head = "Information"
            //icons = "info";
            loader = "#3b98b5";
            bg = "#3d85c6";
            break;
        case 'warning':
            head = "Warning!";
            //icons = "warning";
            loader = "#da8609";
            bg = "#fc750a";
            break;
        case 'danger':
            head = "Alert!";
            //icons = "danger";
            loader = "#b83206";
            bg = "#FF0000";
        default:
    }

    $.toast({
        heading: head,
        //icon: icons,
        loaderBg: loader,
        position: 'top-right',
        allowToastClose: true,
        hideAfter: 2000,
        showHideTransition: 'plain',
        text: message,
        bgColor: bg
    });
}
function fixElementSequence(element) {
    var count = 0;
    $(element).each(function (i) {
        $('input, select, textarea', $(this)).each(function () {
            var input_id = $(this).attr("id") ?? "";
            var input_name = $(this).attr("name") ?? "";
            var aria_describedby = $(this).attr("aria-describedby") ?? "";
            var start1 = input_id.indexOf("[") + 1;
            var end1 = input_id.indexOf("]");
            var start2 = input_name.indexOf("[") + 1;
            var end2 = input_name.indexOf("]");
            var start3 = aria_describedby.indexOf("[") + 1;
            var end3 = aria_describedby.indexOf("]");

            input_id = input_id.length > 0 ? input_id.replace(input_id.substring(start1, end1), count) : "";
            input_name = input_name.length > 0 ? input_name.replace(input_name.substring(start2, end2), count) : "";
            aria_describedby = aria_describedby.length > 0 ? aria_describedby.replace(aria_describedby.substring(start3, end3), count) : "";

            $(this).attr({ id: input_id, name: input_name, "aria-describedby": aria_describedby });
            $(this).trigger("change");
        });

        $('label', $(this)).each(function () {
            var label_id = $(this).attr("for") ?? "";
            var start1 = label_id.indexOf("[") + 1;
            var end1 = label_id.indexOf("]");

            label_id = label_id.length > 0 ? label_id.replace(label_id.substring(start1, end1), count) : "";
            label_id.length > 0 ? $(this).attr({ 'for': label_id }) : "";
        });

        $('span', $(this)).each(function () {
            var span_id = $(this).attr("data-valmsg-for") ?? "";
            var span_id2 = $(this).attr("id") ?? "";
            var start1 = span_id.indexOf("[") + 1;
            var end1 = span_id.indexOf("]");
            var start2 = span_id2.indexOf("[") + 1;
            var end2 = span_id2.indexOf("]");

            span_id = span_id.length > 0 ? span_id.replace(span_id.substring(start1, end1), count) : "";
            span_id2 = span_id2.length > 0 ? span_id2.replace(span_id2.substring(start2, end2), count) : "";

            span_id.length > 0 ? $(this).attr({ 'data-valmsg-for': span_id }) : "";
            span_id2.length > 0 ? $(this).attr({ 'id': span_id2 }) : "";
        });

        $('div', $(this)).each(function () {
            var div_id = $(this).attr("id") ?? "";
            var div_title = $(this).attr("title") ?? "";
            var start1 = div_id.indexOf("[") + 1;
            var end1 = div_id.indexOf("]");
            var start_title = div_title.indexOf("[") + 1;
            var end_title = div_title.indexOf("]");

            div_id = div_id.length > 0 ? div_id.replace(div_id.substring(start1, end1), count) : "";
            div_title = div_title.length > 0 ? div_title.replace(div_title.substring(start_title, end_title), count) : "";

            div_id.length > 0 ? $(this).attr({ 'id': div_id }) : "";
            div_title.length > 0 ? $(this).attr({ 'title': div_title }) : "";
        });

        count++;
    });
};
