var listItem = [];
var currentSKU;
var currentUPC;
var currentQTY;
var currentOrderId;
var currentLoc;
var completedQTY;
var scannedUPC;
var startDate;
var orderNumbers = [];
var orderForScan = '';
//var orderQR = [];

var isOk


$(document).ready(function () {

    getItemPick();
    $("#txtNum").prop("disabled", true);
    $("#btnSave").prop("disabled", true);
    $("#OKScanQR").prop("disabled", true);


});
//function checkIfCollectOrTransfer() {
//    $.ajax({
//        type: "GET",
//        url: "/Picker/CheckItem"
//    }).done(function (set) {
//        if (set.set == "Collect") {
//            getItemCollect(); 
//        }
//        else if (set.set == "Transfer") {
//            getItemTransfer();
//        }
//    });
//}
function getItemPick() {
    ajaxLoader('show');
    $.ajax({
        type: "GET",
        url: "/Picker/GetItemPick"
    }).done(function (set) {
        if (set.set.length > 0) {

            startDate = set.set[0].pickingStartTime.replaceAll("T", " ");
            $("#btnTransferred").prop("disabled", true);
            $("#btnCollected").prop("disabled", false);
            tableGenerator('#itemTable', set);
            display_c();
            ajaxLoader('hide');
        }
        else {
            ajaxLoader('hide');
            document.location = '/';
        }
    });
}
function getItemTransfer() {
    $.ajax({
        type: "GET",
        url: "/Picker/GetItemTransfer"
    }).done(function (set) {
        if (set.status == 'Complete') {
            $("#btnTransferred").prop("disabled", false);
        } else {
            $("#btnTransferred").prop("disabled", true);
        }

        $("#btnCollected").prop("disabled", true);
        tableGenerator2('#itemTable', set);
        display_c();
    });
}
var count = 0;
var listOrder = [];
function tableGenerator(table, trans) {
    let dTable = $(table).DataTable();
    if (trans.set.length > 0) {
        var counter = 0;
        dTable.destroy();

        dTable = $(table).DataTable({
            "paging": true,
            "aaSorting": [],
            "bFilter": false,
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

                        return data = '<div class="row itemBox ' + (row.pickedQty == row.quantity ? 'OnComplete' : (row.pickedQty < row.quantity && row.pickedQty > 0 ? 'OnInsufficientPicker' : (row.isFromNIB == true ? (row.hasPicked == 1 ? 'OnNIB' : '') : ''))) + '" onclick="itemBox(' + row.skuId + ',\'' + row.item_description.replaceAll("'", "|").replace(/"/g, '``') + '\',' + row.quantity + ',' + row.upc + ',\'' + row.orderId + '\')" style="border-style: solid;" > ' +
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
                            '<div class="col-12" style = "font-size:10px !important; line-height:10px; margin-top:2px;" >Order Id: ' + row.orderId + ' </div > ' +
                            '<div class="col-12" style = "font-size:10px !important; line-height:10px; margin-top:10px;" > SRP: ' + row.item_price + ' </div > ' +
                            '<div class="col-12" style = "font-size:10px !important; line-height:10px; margin-top:10px;" > SKU: ' + row.skuId + ' </div > ' +
                            '<div class="col-12" style = "font-size:10px !important; line-height:10px; margin-top:10px;" > Qty: ' + row.pickedQty + ' / ' + row.quantity + '</div > ' +
                            //   '<div style = "font-size:10px !important;" > Sub Department: ' + row.subDepartmentDesc + ' </div > ' +

                            //'<div style = "font-size:10px !important;" > Sub Class: ' + row.subClassDesc + ' </div > ' +
                            '</div>' +

                            '</div>' +
                            '<div class="col-12">' +
                            '<div style="font-size:10px !important; line-height:10px; margin:5px; " >' + 'Location: ' + (row.transferLocation == '' ? row.inventoryLocation : row.transferLocation) + (row.module == 'shopee' ? ' (Shopee)' : ' (Lazada)') + '</div > ' +
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
            "language": {
                "emptyTable": "No data available"
            }
        });
    }

    var li = '';
    orderNumbers = trans.status;
    orderForScan = '';
    for (i = 0; i < trans.status.length; i++) {
        li += '<span class="badge mb-0 ' + trans.status[i].status + '" id="' + trans.status[i].orderId + '">' + trans.status[i].orderId + ' </span> '


        if (trans.status[i].status == "badge-success") {
            $("#btnDone").prop("disabled", false);
            orderForScan += '<span class="badge mb-0" id="scan' + trans.status[i].orderId + '">' + trans.status[i].orderId + '</span> <button type="button" class="btn btn-primary" onclick="ScanQRForOrders(\'' + trans.status[i].orderId + '\')"><i class="bi bi-qr-code-scan"></i></button><input type="text" id="txt' + trans.status[i].orderId + '" class="d-none"> <br/>'
            count++;
        }
    }
    $("#orderNum").html(li);

    //if (trans.isdone) {
    //    $("#btnDone").prop("disabled", false);
    //} else {

    //    $("#btnDone").prop("disabled", true);
    //}
}

function itemBox(sku, desc, qty, upc, orderId) {
    $("#itemModalBtn").click();

    $("#txtBarcodes").val("");
    $("#txtDescription").text(desc.replaceAll("|", "'"));
    $("#txtSKU").text('SKU: ' + sku);
    $("#txtQty").text('/ ' + qty + 'pc(s)');
    currentSKU = sku;
    currentUPC = upc.toString();
    currentQTY = qty;
    currentOrderId = orderId;
    $("#txtValid").text("");
    $("#txtNum").val("");
    $("#txtNum").prop("disabled", true);
    $("#btnSave").prop("disabled", true);

    $('#txtNum').on('input', function () {

        var value = $(this).val();

        if ((value !== '') && (value.indexOf('.') === -1)) {

            $(this).val(Math.max(Math.min(value, currentQTY), 0));
        }
    });

    ScanBarcode();


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

function ScanBarcode() {
    $("#btnNib").removeClass("d-none");
    $("#scanTitle").text("Scan Barcode");
    //barcode.config.start = 0.1;
    //barcode.config.end = 0.9;
    //barcode.config.video = '#reader';
    //barcode.config.canvas = '#readercanvas';
    //barcode.config.canvasg = '#readercanvasg';
    //barcode.setHandler(function (barcode) {
    //    console.log(barcode);
    //});

    //barcode.init();

    $("#txtValid").text("");
    $("#scanBarcodeBtn").click();

    const scanner = new Html5QrcodeScanner('reader', {
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
        $("#closeScanModal").click();
        //$("#itemModalBtn").click();
        scannedUPC = result;
        barcodeInput(result);
    }
    function error(err) {
        //console.error(err);
    }
}


$(document).on('click', '#btnSend', function () {

    var result = $("#txtBarcodes").val();
    barcodeInput(result);
});

function barcodeInput(result) {
    $.ajax({
        type: "GET",
        url: "/Picker/ScannedUPC?upc=" + result + "&sku=" + currentSKU,
    }).done(function (set) {
        if (set.status == "Correct") {
            $("#txtValid").removeClass("text-danger");
            $("#txtValid").addClass("text-success");
            $("#txtValid").text("Barcode matched!");
            $("#txtNum").prop("disabled", false);
            $("#btnSave").prop("disabled", false);
            $("#txtNum").focus();
        }
        else if (set.status == "Invalid") {

            $("#txtValid").removeClass("text-success");
            $("#txtValid").addClass("text-danger");
            $("#txtValid").text(result +" Barcode doesn't match!");
            $("#txtNum").prop("disabled", true);
            $("#btnSave").prop("disabled", true);
        }
        else {

            $("#txtValid").removeClass("text-success");
            $("#txtValid").addClass("text-danger");
            $("#txtValid").text(result + " Barcode doesn't match!");
            $("#txtNum").prop("disabled", true);
            $("#btnSave").prop("disabled", true);
        }
    })
}


function SaveItem(answer) {
    completedQTY = parseInt($("#txtNum").val());
    $.ajax({
        type: "GET",
        url: "/Picker/SaveItem",
        data: {
            SKU: currentSKU,
            UPC: currentUPC,
            QTY: completedQTY,
            OrderId: currentOrderId,
            Answer: answer

        }
    }).done(function (set) {
        if (set.set.length > 0) {
            document.location = '/Picker/Index'
        }
    });
}
function SaveLocation() {
    Loc = $("#txtInputLocation").val();
    $.ajax({
        type: "GET",
        url: "/Picker/SaveItemLocation",
        data: {
            SKU: currentSKU,
            UPC: currentUPC,
            Location: $("#txtInputLocation").val()

        }
    }).done(function (set) {
        if (set.set.length > 0) {
            document.location = '/Picker/Index'
        }
        else {
            $("#txtNoLoc").removeClass("d-none");
        }
    });
}

function InputLocation() {
    if (currentLoc == $("#txtInputLocation").val()) {
        SaveLocation();
    }
    else {
        $("#txtNoLoc").removeClass("d-none");
    }
}

function OutOfStock() {
    $.ajax({
        type: "GET",
        url: "/Picker/OutOfStockItem",
        data: {
            SKU: currentSKU,
            UPC: currentUPC,
            QTY: completedQTY

        }
    }).done(function (set) {
        if (set.set.length > 0) {

            //.then((result) => {
            //    /* Read more about isConfirmed, isDenied below */
            //    if (result.isConfirmed) {
            //        document.location = '/Picker/Index'
            //    }
            //})
            var si = setInterval(function () {
                clearInterval(si);
                document.location = '/Picker/Index'
            }, 2000);
        }
    });
}
function ItemCollected() {
    $.ajax({
        type: "GET",
        url: "/Picker/ItemCollected",
        data: {
            SKU: currentSKU,
            UPC: currentUPC,
            QTY: completedQTY

        }
    }).done(function (set) {
        if (set.set.length > 0) {

            //Swal.fire({
            //    title: 'Succes!',
            //    text: 'Item(s) is for Transferring now',
            //    icon: 'success',
            //    confirmButtonText: 'OK'
            //}).then((result) => {
            /* Read more about isConfirmed, isDenied below */
            //if (result.isConfirmed) {
            //    document.location = '/Picker/Index'
            //}
            //})
            var si = setInterval(function () {
                clearInterval(si);
                document.location = '/Picker/Index'
            }, 2000);
        }
    });
}

function ItemTransferred() {
    $.ajax({
        type: "GET",
        url: "/Picker/ItemTransferred",
        data: {
            SKU: currentSKU,
            UPC: currentUPC,
            QTY: completedQTY

        }
    }).done(function (set) {
        if (set.set.length > 0) {

            //Swal.fire({
            //    title: 'Succes!',
            //    text: 'Item(s) is for Transferring now',
            //    icon: 'success',
            //    confirmButtonText: 'OK'
            //}).then((result) => {
            //    /* Read more about isConfirmed, isDenied below */
            //    if (result.isConfirmed) {
            //        document.location = '/'
            //    }
            //})
            var si = setInterval(function () {
                clearInterval(si);
                document.location = '/'
            }, 2000);
        }
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


        $("#collctingTab").removeClass("d-none");
        $("#transferringTab").addClass("d-none");

    }
    else {
        getItemTransfer();
        $("#btnCollected").addClass("d-none");
        $("#btnTransferred").removeClass("d-none");

        $("#transferringTab").removeClass("d-none");
        $("#collctingTab").addClass("d-none");
    }
});


//var  minutesCount = 0, secondsCount = 0, centisecondsCount = 0;
var hours = document.getElementById("hours")
var minutes = document.getElementById("minutes")
var seconds = document.getElementById("seconds")
var centiSecond = document.getElementById("centiSecond")

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
    setInterval(function () {
        var hoursCount = parseInt($("#hours").text());
        hoursCount += 1
        hours.innerHTML = hoursCount
    }, 3600000)
    setInterval(function () {
        var minutesCount = parseInt($("#minutes").text());
        minutesCount += 1
        minutes.innerHTML = minutesCount
    }, 60000)
    setInterval(function () {
        var secondsCount = parseInt($("#seconds").text());
        secondsCount += 1
        if (secondsCount > 59) {
            secondsCount = 1
        }
        seconds.innerHTML = secondsCount
    }, 1000)
    setInterval(function () {
        var centisecondsCount = parseInt($("#centiSecond").text());
        centisecondsCount += 1
        if (centisecondsCount > 99) {
            centisecondsCount = 1
        }
        centiSecond.innerHTML = centisecondsCount
    }, 10)

}

function onlyUnique(value, index, array) {
    return array.indexOf(value) === index;
}

function IsIncomplete() {
    var pickQTY = parseInt($("#txtNum").val());
    if (pickQTY == currentQTY) {
        SaveItem('none');
    } else {
        $("#incompleteItemModalBtn").click();
    }
}


function DoneOrders() {

    $("#scanOrdersDiv").html(orderForScan);

    $("#doneItemModalBtn").click();
}

var isSuccess = false;
function ScanQRForOrders(orderId) {
    $("#btnNib").addClass("d-none");
    $("#scanTitle").text("Scan QR");
    isSuccess = false;
    //barcode.config.start = 0.1;
    //barcode.config.end = 0.9;
    //barcode.config.video = '#reader';
    //barcode.config.canvas = '#readercanvas';
    //barcode.config.canvasg = '#readercanvasg';
    //barcode.setHandler(function (barcode) {
    //    console.log(barcode);
    //});

    //barcode.init();

    $("#txtValid").text("");
    $("#scanBarcodeBtn").click();

    const scanner = new Html5QrcodeScanner('reader', {
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
        scanner.clear();
        //$("#reader").hide();
        $("#closeScanModal").click();
        //$("#itemModalBtn").click();
        scannedUPC = result;

        if (result.toLowerCase().match(/^tub/)) {
            $.ajax({
                type: "GET",
                url: "/Picker/CheckTub?result=" + result + "&orderId=" + orderId,
            }).done(function (set) {
                if (set.set == "Good") {
                    $('#scan' + orderId).addClass("badge-success");
                    $('#txt' + orderId).val(result);

                }
                else if (set.set == "InUse") {
                    Swal.fire({
                        title: 'Oops!',
                        text: 'This TUB is currently in use',
                        icon: 'error',
                        confirmButtonText: 'OK'
                    })
                }
                else if (set.set == "Discrepancy") {
                    Swal.fire({
                        title: 'Oops!',
                        text: 'This TUB is in Discrepancy Center',
                        icon: 'error',
                        confirmButtonText: 'OK'
                    })
                }

                var num = 0;
                for (i = 0; i < orderNumbers.length; i++) {
                    var orderVal = $("#txt" + orderNumbers[i].orderId).val()

                    if (orderVal != "" && orderVal != undefined) {
                        num++;
                        //var arr = {
                        //    orderId: orderId,
                        //    qrCode: orderVal
                        //}

                        //orderQR.push(arr);
                    }

                }

                if (num == count) {
                    $("#OKScanQR").prop("disabled", false);
                }
                else {
                    $("#OKScanQR").prop("disabled", true);
                }

            });

        }
        else {
            Swal.fire({
                title: 'Oops!',
                text: 'The QR Code must have TUB in text!',
                icon: 'error',
                confirmButtonText: 'OK'
            })
        }

        




}
function error(err) {
    //console.error(err);
}
}
function DonePicker() {
    var orderQR = [];
    for (i = 0; i < orderNumbers.length; i++) {
        var orderVal = $("#txt" + orderNumbers[i].orderId).val()

        if (orderVal != "" && orderVal != undefined) {
            var arr = {
                orderId: orderNumbers[i].orderId,
                qrCode: orderVal
            }

            orderQR.push(arr);
        }

    }
    DonePickerToController(orderQR);
}
function DonePickerToController(data) {
    //var data = {
    //    List: orderQR
    //    }
    $.ajax({
        type: "POST",
        url: "/Picker/DonePicker",
        data: { qrList: data }
    }).done(function (set) {
        if (set.message == "CancelledOrders") {
            Swal.fire({
                title: 'Oops!',
                text: 'This order was cancelled while you are picking',
                icon: 'error',
                confirmButtonText: 'OK'
            }).then((result) => {
                // This code block will be executed only if the user clicks "OK"
                if (result.isConfirmed) {
                    var si = setInterval(function () {
                        clearInterval(si);
                        document.location = '/'
                    }, 1000);
                } else if (result.dismiss) {
                    var si = setInterval(function () {
                        clearInterval(si);
                        document.location = '/'
                    }, 1000);
                }
            });

        }
        else if (set.message == "SuccessComplete") {
            if (set.set.length > 0) {
                var si = setInterval(function () {
                    clearInterval(si);
                    document.location = '/'
                }, 2000);
            }

        }
        else if (set.message == "QrExist") {
            Swal.fire({
                title: 'Oops!',
                text: 'This QR is currently in use',
                icon: 'error',
                confirmButtonText: 'OK'
            })
        }
    });
}
$('#scanBarcode').on('hide.bs.modal', function (e) {
    // do something...
    if (!isSuccess) {
        $("#html5-qrcode-button-camera-stop").click();
    }
})



function NIB() {
    $.ajax({
        type: "POST",
        url: "/Picker/NIB?orderId=" + currentOrderId + "&sku=" + currentSKU,
    }).done(function (set) {
        if (set.set.length > 0) {
            if (set.set.length > 0) {
                var si = setInterval(function () {
                    clearInterval(si);
                    document.location = '/'
                }, 2000);
            }
        }
    })

}