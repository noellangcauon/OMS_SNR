var listItem = [];
var currentSKU;
var currentUPC;
var currentQTY;
var currentLoc;
var completedQTY;
var scannedUPC;
var startDate;



$(document).ready(function () {

    $("#toggleButton").removeClass("d-none");
    getItemCollect();
    $("#txtNum").prop("disabled", true);
    $("#btnSave").prop("disabled", true);
    $("#btnTransferred").prop("disabled", false);


});
//function checkIfCollectOrTransfer() {
//    $.ajax({
//        type: "GET",
//        url: "/Runner/CheckItem"
//    }).done(function (set) {
//        if (set.set == "Collect") {
//            getItemCollect(); 
//        }
//        else if (set.set == "Transfer") {
//            getItemTransfer();
//        }
//    });
//}
function getItemCollect() {
    ajaxLoader('show');
    $.ajax({
        type: "GET",
        url: "/Runner/GetItemCollect"
    }).done(function (set) {
        if (set.set.length > 0) {
            startDate = set.set[0].collectingStartTime.replaceAll("T", " ");
            //$("#btnTransferred").prop("disabled", true);
            $("#btnCollected").prop("disabled", false);
            tableGenerator('#itemTable', set);
            display_c();

            ajaxLoader('hide');
        }
        else {

            //$("#btnTransferred").prop("disabled", true);
            $("#btnCollected").prop("disabled", false);
            tableGenerator('#itemTable', set);
            clearInterval(hoursInterval);
            clearInterval(minutesInterval);
            clearInterval(secondsInterval);
            clearInterval(centiSecondInterval);
            $("#seconds").text(0);
            $("#minutes").text(0);
            $("#hours").text(0);
            $("#centiSecond").text(0);
            ajaxLoader('hide');
        }
    });
}
function getItemTransfer() {
    ajaxLoader('show');
    $.ajax({
        type: "GET",
        url: "/Runner/GetItemTransfer"
    }).done(function (set) {
        //if (set.status == 'Complete') {
        //    $("#btnTransferred").prop("disabled", false);
        //} else {
        //    $("#btnTransferred").prop("disabled", true);
        //}

        if (set.set.length > 0) {
            startDate = set.set[0].transferringStartTime.replaceAll("T", " ");
            $("#btnCollected").prop("disabled", true);
            tableGenerator2('#itemTable', set);
            display_c();
            ajaxLoader('hide');
        }
        else {

            $("#btnCollected").prop("disabled", true);
            tableGenerator2('#itemTable', set);
            clearInterval(hoursInterval);
            clearInterval(minutesInterval);
            clearInterval(secondsInterval);
            clearInterval(centiSecondInterval);
            $("#seconds").text(0);
            $("#minutes").text(0);
            $("#hours").text(0);
            $("#centiSecond").text(0);
            ajaxLoader('hide');
        }
        
    });
}

function tableGenerator(table, trans) {
    let dTable = $(table).DataTable();
    if (trans.set.length > 0) {
        var counter = 0;
        dTable.destroy();

        dTable = $(table).DataTable({
            "paging": true,
            "bFilter": false,
            "paging": false,
            "aaSorting": [],
            'columnDefs': [
                {
                    "searchable": false,
                    'targets': [0]
                }],
            "data": trans.set,
            "columns": [
                {
                    "data": "item_image",
                    "render": function (data, type, row) {
                        return data = '<div class="row itemBox ' + (row.collectedQty == row.quantity ? 'OnComplete' : (row.collectedQty < row.quantity && row.collectedQty > 0 ? 'OnInsufficient' : (row.typeOfexception == 'NOF' ? 'OnNOF' : ''))) + '" onclick="itemBox(' + row.sku_id + ',\'' + row.item_description.replaceAll("'", "|").replace(/"/g, '``') + '\',' + row.quantity + ',' + row.upc + ')" style="border-style: solid;" > ' +
                            '<div class="col-4">' +
                            '<img style="width: 80px;height: 80px;cursor:pointer;" id="previewImg" href="' + data + '"  src="' + data + '" >' +
                            '</div>' +
                            '<div class="col-4"> ' +
                            '<div class="row">' +
                            '<div class="col-12" style = "font-size:10px !important; line-height:10px; margin-top:2px;" > ' + row.item_description.replaceAll("'", "|").replace(/"/g, '``') + ' </div > ' +
                            '</div>' +

                            '</div>' +

                            '<div class="col-4"> ' +
                            '<div class="row">' +
                            '<div class="col-12" style = "font-size:10px !important; line-height:10px; margin-top:2px;" >UPC: ' + row.upc + ' </div > ' +
                            '<div class="col-12" style = "font-size:10px !important; line-height:10px; margin-top:6px;" > SRP: ' + row.item_price + ' </div > ' +
                            '<div class="col-12" style = "font-size:10px !important; line-height:10px; margin-top:6px;" > SKU: ' + row.sku_id + ' </div > ' +
                            '<div class="col-12" style = "font-size:10px !important; line-height:10px; margin-top:6px;" > Qty: ' + row.collectedQty + ' / ' + row.quantity + '</div > ' + 
                            '<div class="col-12" style = "font-size:10px !important; line-height:10px; margin-top:6px;" > 217 Inv: ' + row.onhand + ' </div > ' +
                            //   '<div style = "font-size:10px !important;" > Sub Department: ' + row.subDepartmentDesc + ' </div > ' +

                            //'<div style = "font-size:10px !important;" > Sub Class: ' + row.subClassDesc + ' </div > ' +
                            '</div>' +

                            '</div>' +
                            '<div class="col-12">' +
                            '<div style="font-size:10px !important; line-height:10px; margin:5px; " >' + row.departmentDesc + '> ' + row.subDepartmentDesc + '> ' + row.classDesc + '> ' + row.subClassDesc + '</div > ' +
                            '</div>' +
                            '</div > '
                    }
                },
            ]
        });

    }
    else {
        dTable.clear().draw();
        dTable.destroy();
        dTable = $(table).DataTable({
            "paging": true,
            "bFilter": false,
            "paging": false,
            "language": {
                "emptyTable": "No data available"
            }
        });
    }
}
function tableGenerator2(table, trans) {
    let dTable = $(table).DataTable();
    if (trans.set.length > 0) {
        var counter = 0;
        dTable.destroy();

        dTable = $(table).DataTable({
            "paging": true,
            "bFilter": false,
            "aaSorting": [],
            "paging": false,
            'columnDefs': [
                {
                    "searchable": false,
                    'targets': [0]
                }],
            "data": trans.set,
            "columns": [
                {
                    "data": "item_image",
                    "render": function (data, type, row) {
                        return data = '<div class="row itemBox ' + (row.transferLocation != "" ? 'OnComplete' : '') + '" onclick="itemBox2(' + row.sku_id + ',\'' + row.inventoryLocation + '\',' + row.quantity + ',' + row.upc + ')" style="border-style: solid;" > ' +
                            '<div class="col-4">' +
                            '<img style="width: 80px;height: 80px;cursor:pointer;" id="previewImg" href="' + data + '"  src="' + data + '" >' +
                            '</div>' +
                            '<div class="col-4"> ' +
                            '<div class="row">' +
                            '<div class="col-12" style = "font-size:10px !important; line-height:10px; margin-top:2px;" > ' + row.item_description.replaceAll("'", "|").replace(/"/g, '``') + ' </div > ' +
                            '</div>' +

                            '</div>' +

                            '<div class="col-4"> ' +
                            '<div class="row">' +
                            '<div class="col-12" style = "font-size:10px !important; line-height:10px; margin-top:2px;" >UPC: ' + row.upc + ' </div > ' +
                            '<div class="col-12" style = "font-size:10px !important; line-height:10px; margin-top:10px;" > SKU: ' + row.sku_id + ' </div > ' +
                            '<div class="col-12" style = "font-size:10px !important; line-height:10px; margin-top:10px;" > Loc: ' + row.inventoryLocation + '</div > ' +
                            '<div class="col-12" style = "font-size:10px !important; line-height:10px; margin-top:10px;" > Transferred Loc: ' + row.transferLocation + '</div > ' +
                            //   '<div style = "font-size:10px !important;" > Sub Department: ' + row.subDepartmentDesc + ' </div > ' +

                            //'<div style = "font-size:10px !important;" > Sub Class: ' + row.subClassDesc + ' </div > ' +
                            '</div>' +

                            '</div>' +
                            '<div class="col-12">' +
                            '<div style="font-size:10px !important; line-height:10px; margin:5px; " >' + row.departmentDesc + '> ' + row.subDepartmentDesc + '> ' + row.classDesc + '> ' + row.subClassDesc + '</div > ' +
                            '</div>' +
                            '</div > '
                    }
                },
            ]
        });

    }
    else {
        dTable.clear().draw();
        dTable.destroy();
        dTable = $(table).DataTable({
            "paging": true,
            "bFilter": false,
            "paging": false,
            "language": {
                "emptyTable": "No data available"
            }
        });
    }
}

function itemBox(sku, desc, qty, upc) {
    $("#itemModalBtn").click();
    $("#txtBarcodes").val("");
    $("#txtDescription").text(desc.replaceAll("|", "'"));
    $("#txtSKU").text('SKU: ' + sku);
    $("#txtQty").text('/ ' + qty + 'pc(s)');

    currentSKU = sku;
    currentUPC = upc.toString();
    currentQTY = qty;

    $("#txtValid").text("");
    $(".numberField").prop("disabled", true);
    $("#btnSave").prop("disabled", true);

    ScanBarcode('collect');
    //$("#txtDescription").text(desc);
    $("#txtNum").val("");
    $('#txtNum').on('input', function () {

        var value = $(this).val();

        if ((value !== '') && (value.indexOf('.') === -1)) {

            $(this).val(Math.max(Math.min(value, currentQTY), 0));
        }
    });
}
function itemBox2(sku, desc, qty, upc) {
    $("#itemModalTransferBtn").click();
    $("#txtInputLocation").val("");
    $("#txtNoLoc").addClass("d-none");
    $("#txtLocation").text(desc);
    //$("#txtSKU").text('SKU: ' + sku);
    //$("#txtQty").text('/ ' + qty + 'pc(s)');
    currentSKU = sku;
    currentUPC = upc.toString();
    currentQTY = qty;
    currentLoc = desc;

    $("#txtValid").text("");
    $(".numberField").prop("disabled", true);
    $("#btnSave").prop("disabled", true);
    ScanBarcode('transfer');
    //$("#txtDescription").text(desc);
}

$(document).on("click", ".itemBox", function () {

    $(this).addClass("OnUse");
});


const input = $('input[type=number]')

const increment = () => {
    input.val(Number(input.val()) + 1)
}
const decrement = () => {
    input.val(Number(input.val()) - 1)
}

$('.spinner.increment').click(increment)
$('.spinner.decrement').click(decrement)


$('#itemModal').on('hidden.bs.modal', function (e) {
    // do something...
    $(".itemBox").removeClass("OnUse");
});
$('#itemModalTransfer').on('hidden.bs.modal', function (e) {
    // do something...
    $(".itemBox").removeClass("OnUse");
});
var scanner;
var isSuccess = false;
function ScanBarcode(mode) {
    isSuccess = false;
    if (mode == 'collect') {
        $("#txtValid").text("");
        $("#scanBarcodeBtn").click();

        scanner = new Html5QrcodeScanner('reader', {
            qrbox: {
                width: 250,
                height: 250,
            },
            fps: 20,
        });


        scanner.render(success, error);

        function success(result) {
            console.log(result)

            scanner.clear();
            //$("#reader").hide();
            isSuccess = true;
            $("#closeScanModal").click();
            //$("#itemModalBtn").click();
            scannedUPC = result;

            barcodeInput(result);
            //if (currentUPC == scannedUPC) {
                
            //}
            //else {

                
            //}
        }
        function error(err) {
            //console.error(err);
        }
    }
    else if (mode == 'transfer') {
        $("#scanBarcodeBtn").click();

        scanner = new Html5QrcodeScanner('reader', {
            qrbox: {
                width: 250,
                height: 250,
            },
            fps: 20,
        });


        scanner.render(success, error);

        function success(result) {
            console.log(result)

            isSuccess = true;
            $("#closeScanModal").click();
            scanner.clear();
            $("#txtInputLocation").val(result);
        }
        function error(err) {
            //console.error(err);
        }
    }
}

$('#scanBarcode').on('hide.bs.modal', function (e) {
    // do something...
    if (!isSuccess) {
        $("#html5-qrcode-button-camera-stop").click();
    }
})

$(document).on('click', '#btnSend', function () {

    var result = $("#txtBarcodes").val();
    barcodeInput(result);
});

function barcodeInput(result) {
    $.ajax({
        type: "GET",
        url: "/Runner/ScannedUPC?upc=" + result + "&sku=" + currentSKU,
    }).done(function (set) {
        if (set.status == "Correct") {
            $("#txtValid").removeClass("text-danger");
            $("#txtValid").addClass("text-success");
            $("#txtValid").text("Barcode matched!");
            $("#txtNum").prop("disabled", false);
            $("#btnSave").prop("disabled", false);
            $("#txtNum").focus();
        }
        else if (set.status == "Invalid"){
            $("#txtValid").removeClass("text-success");
            $("#txtValid").addClass("text-danger");
            $("#txtValid").text(result +" Barcode doesn't match!");
            $("#txtNum").prop("disabled", true);
            $("#btnSave").prop("disabled", true);
        }
        else {
            $("#txtValid").removeClass("text-success");
            $("#txtValid").addClass("text-danger");
            $("#txtValid").text(result+" Barcode doesn't match!");
            $("#txtNum").prop("disabled", true);
            $("#btnSave").prop("disabled", true);
        }
    });
}
function SaveItem() {
    completedQTY = parseInt($("#txtNum").val());
    $.ajax({
        type: "GET",
        url: "/Runner/SaveItem",
        data: {
            SKU: currentSKU,
            UPC: currentUPC,
            QTY: completedQTY

        }
    }).done(function (set) {
        if (set.set.length > 0) {
            document.location = '/Runner/Index'
        }
    });
}
function SaveLocation() {
    Loc = $("#txtInputLocation").val();
    $.ajax({
        type: "GET",
        url: "/Runner/SaveItemLocation",
        data: {
            SKU: currentSKU,
            UPC: currentUPC,
            Location: $("#txtInputLocation").val()

        }
    }).done(function (set) {
        if (set.set.length > 0) {
            document.location = '/Runner/Index'
        }
        else {
            $("#txtNoLoc").removeClass("d-none");
        }
    });
}

function InputLocation() {
    var loc = $("#txtInputLocation").val();
    if (loc != "") {
        SaveLocation();
    }
    else {
        $("#txtNoLoc").removeClass("d-none");
    }
}

function NOF() {
    $.ajax({
        type: "GET",
        url: "/Runner/NOF",
        data: {
            SKU: currentSKU,
            UPC: currentUPC,
            QTY: completedQTY

        }
    }).done(function (set) {
        if (set.set.length > 0) {

            //Swal.fire({
            //    title: 'Succes!',
            //    text: 'Item(s) updated to OOS!',
            //    icon: 'success',
            //    confirmButtonText: 'OK'
            //}).then((result) => {
            //    /* Read more about isConfirmed, isDenied below */
            //    if (result.isConfirmed) {
            //        document.location = '/Runner/Index'
            //    }
            //})
            var si = setInterval(function () {
                clearInterval(si);
                document.location = '/Runner/Index'
            }, 2000);
        }
    });
}
function ItemCollected() {
    $.ajax({
        type: "GET",
        url: "/Runner/ItemCollected",
        data: {
            SKU: currentSKU,
            UPC: currentUPC,
            QTY: completedQTY

        }
    }).done(function (set) {
        if (set.hasNoOrders == 'No') {
            var si = setInterval(function () {
                clearInterval(si);
                document.location = '/Runner/Index'
            }, 2000);
        }
        else {
            var si = setInterval(function () {
                clearInterval(si);
                document.location = '/'
            }, 2000);
        }


        //if (set.set.length > 0 || set.nof.length > 0 ) {

        //    //Swal.fire({
        //    //    title: 'Succes!',
        //    //    text: 'Item(s) is for Transferring now',
        //    //    icon: 'success',
        //    //    confirmButtonText: 'OK'
        //    //}).then((result) => {
        //    /* Read more about isConfirmed, isDenied below */
        //    //if (result.isConfirmed) {
        //    //    document.location = '/Runner/Index'
        //    //}
        //    //})
        //    var si = setInterval(function () {
        //        clearInterval(si);
        //        document.location = '/Runner/Index'
        //    }, 2000);

        //}
    });
}

function ItemTransferred() {
    $.ajax({
        type: "POST",
        url: "/Runner/ItemTransferred",
        data: {
            SKU: currentSKU,
            UPC: currentUPC,
            QTY: completedQTY

        }
    }).done(function (set) {
        if (set.hasNoOrders == 'No') {
            var si = setInterval(function () {
                clearInterval(si);
                document.location = '/Runner/Index'
            }, 2000);
        }
        else {
            var si = setInterval(function () {
                clearInterval(si);
                document.location = '/'
            }, 2000);
        }
        //if (set.set.length > 0) {

        //    //Swal.fire({
        //    //    title: 'Succes!',
        //    //    text: 'Item(s) is for Transferring now',
        //    //    icon: 'success',
        //    //    confirmButtonText: 'OK'
        //    //}).then((result) => {
        //    //    /* Read more about isConfirmed, isDenied below */
        //    //    if (result.isConfirmed) {
        //    //        document.location = '/'
        //    //    }
        //    //})
        //    var si = setInterval(function () {
        //        clearInterval(si);
        //        document.location = '/Runner/Index'
        //    }, 2000);
        //}
    });
}
var my_date_format = function (input) {
    var d = new Date(Date.parse(input.replace(/-/g, "/")));
    var month = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
    var date = d.getDate() + " " + month[d.getMonth()] + ", " + d.getFullYear();
    var time = d.toLocaleTimeString().toLowerCase().replace(/([\d]+:[\d]+):[\d]+(\s\w+)/g, "$1$2");
    return (date + " " + time);
};




$(document).on("click", "#switchMode", function () {
    var isCollecting = $("#switchMode").prop("checked")

    if (isCollecting) {
        
        getItemCollect();
        $("#btnCollected").removeClass("d-none");
        $("#btnTransferred").addClass("d-none");
        $("#btnOos").removeClass("d-none"); 


        $("#collctingTab").removeClass("d-none");
        $("#transferringTab").addClass("d-none");

    }
    else {
        getItemTransfer();
        $("#btnCollected").addClass("d-none");
        $("#btnTransferred").removeClass("d-none");
        $("#btnOos").addClass("d-none"); 

        $("#transferringTab").removeClass("d-none");
        $("#collctingTab").addClass("d-none");
    }
});


//var  minutesCount = 0, secondsCount = 0, centisecondsCount = 0;
var hours = document.getElementById("hours")
var minutes = document.getElementById("minutes")
var seconds = document.getElementById("seconds")
var centiSecond = document.getElementById("centiSecond")
var hoursInterval;
var minutesInterval;
var secondsInterval;
var centiSecondInterval;

function display_c() {

    var diff = Math.abs(new Date() - new Date(startDate));
    var diffseconds = Math.floor(diff / 1000); //ignore any left over units smaller than a second
    var diffminutes = Math.floor(diffseconds / 60);
    diffseconds = diffseconds % 60;
    var diffhours = Math.floor(diffminutes / 60);
    diffminutes = diffminutes % 60;

    /*alert("Diff = " + hours + ":" + minutes + ":" + seconds);*/
    $("#seconds").text(diffseconds);
    $("#minutes").text(diffminutes);
    $("#hours").text(diffhours);
    hoursInterval = setInterval(function () {
        var hoursCount = parseInt($("#hours").text());
        hoursCount += 1
        hours.innerHTML = hoursCount
    }, 3600000)
    minutesInterval = setInterval(function () {
        var minutesCount = parseInt($("#minutes").text());
        minutesCount += 1
        minutes.innerHTML = minutesCount
    }, 60000)
    secondsInterval = setInterval(function () {
        var secondsCount = parseInt($("#seconds").text());
        secondsCount += 1
        if (secondsCount > 59) {
            secondsCount = 1
        }
        seconds.innerHTML = secondsCount
    }, 1000)
    centiSecondInterval = setInterval(function () {
        var centisecondsCount = parseInt($("#centiSecond").text());
        centisecondsCount += 1
        if (centisecondsCount > 99) {
            centisecondsCount = 1
        }
        centiSecond.innerHTML = centisecondsCount
    }, 10)
}
function stopVideoOnly(stream) {
    stream.getTracks().forEach((track) => {
        if (track.readyState == 'live' && track.kind === 'video') {
            track.stop();
        }
    });
}