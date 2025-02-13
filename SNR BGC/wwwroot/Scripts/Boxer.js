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
var orderId;
var totalOrder = 0;
var totalComplete = 0;
var index = 0;
var orderIdforDelete;
var skuIdforDelete;

var isOk

//var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();
//connection.on("ReceiveDiscrepancyNotif", function (user, message) {
//   // LoadDiscrepancyNotif();



//});
//connection.start().then(function () {

//}).catch(function (err) {
//    return console.error(err.toString());
//});




$(document).ready(function () {
    //$("#btnToPOS").prop("disabled", true);
    $("#btnToPOS").prop("disabled", true);
    getParams();
    $("#txtOrderId").text(orderId)
    getItemBox();

});
function getParams() {
    const params = new URLSearchParams(window.location.search);
    orderId = params.get('result');
}

function getItemBox() {
    $.ajax({
        type: "GET",
        url: "/Boxer/LoadScanned?orderId=" + orderId
    }).done(function (set) {
        if (set.set.length > 0) {
            if (set.isDisableToPOS == "Yes") {

                $("#btnToPOS").prop("disabled", true);
            }
            else {

                $("#btnToPOS").prop("disabled", false);
            }
        }
        else {
            $("#btnToPOS").prop("disabled", true);

        }
        tableGenerator('#itemTable', set);
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
var listOrder = [];
function tableGenerator(table, trans) {
    let dTable = $(table).DataTable();
    if (trans.set.length > 0) {
        var counter = 0;
        dTable.destroy();

        dTable = $(table).DataTable({
            "paging": true,
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
                        return data = '<div class="row touchsurface" style="border-style: solid;" > ' +
                            '<button class="text-bold px-2 btn-danger btn " style="font-size:10px;  position: absolute; margin-top:-15px; right: -10px; " onclick="deleteScan(\'' + row.orderId + '\',\'' + row.skuId + '\')"><b>X</b></button>' +
                            /*'<div class="col-10">' +*/
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
                            //'<div class="col-12" style = "font-size:10px !important; line-height:10px; margin-top:10px;" > Qty: ' + row.scannedQty + ' / ' + row.quantity + '</div > ' +
                            //   '<div style = "font-size:10px !important;" > Sub Department: ' + row.subDepartmentDesc + ' </div > ' +

                            //'<div style = "font-size:10px !important;" > Sub Class: ' + row.subClassDesc + ' </div > ' +
                            '</div>' +

                            '</div>' +


                            '<div class="col-12">' +
                            '<div style="font-size:10px !important; line-height:10px; margin:5px; " >' + 'Location: ' + (row.transferLocation == '' ? row.inventoryLocation : row.transferLocation) + (row.module == 'shopee' ? ' (Shopee)' : (row.module == 'lazada' ? ' (Lazada)' : '')) + '</div > ' +
                            '</div>' +
                            /*'</div>' +*/
                            //'<div class="col-2 btn-danger"> ' +

                            //'</div>' +
                            '</div > '
                    }
                },
            ]
        });
        totalComplete = trans.set[0].ordersQuantity;
        totalOrder = trans.set[0].totalOrdersQuantity;




    }
    else {
        dTable.clear().draw();
        dTable.destroy();
        dTable = $(table).DataTable({
            "paging": false,
            "language": {
                "emptyTable": "Click Scan to show the items"
            },
            "bFilter": false,
            "paging": false,
            "bInfo": false
        });
    }

}
$('#scanBarcode').on('hide.bs.modal', function (e) {
    // do something...
    if (!isSuccess) {
        $("#html5-qrcode-button-camera-stop").click();
    }
})

function itemBox() {
    $("#itemModalBtn").click();


    $("#txtValid").text("");
    $("#txtNum").prop("disabled", true);
    $("#btnSave").prop("disabled", true);
    $("#txtNum").val("");
    $("#txtBarcodes").val("");
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
var isSuccess = false;
function ScanBarcode() {
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


        barcodeInput(result);
        //$.ajax({
        //    type: "GET",
        //    url: "/Boxer/ScannedUPC?upc=" + result + "&orderid=" + orderId,
        //}).done(function (set) {
        //    if (set.status == "Existing") {
        //        var si = setInterval(function () {
        //            clearInterval(si);
        //            document.location = '/Boxer/Index?result=' + set.set
        //        }, 1000);
        //    }
        //    //else if (set.status == "NotExist") {
        //    //    Swal.fire({
        //    //        title: 'Oops',
        //    //        text: 'Item not exist on the cleared Item!',
        //    //        icon: 'error',
        //    //        confirmButtonText: 'OK'
        //    //    })

        //    //}
        //    else if (set.status == "NotExistBarcode") {
        //        Swal.fire({
        //            title: 'Oops',
        //            text: 'Barcode not exist in the system!',
        //            icon: 'error',
        //            confirmButtonText: 'OK'
        //        })

        //    }
        //})
    }
    function error(err) {
        //console.error(err);
    }
}

function ScanQRPrinterAlready() {
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
    $("#ScanQRPrinterBtn").click();

    const scanner = new Html5QrcodeScanner('readerPrinter', {
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

        if (result.toLowerCase().match(/^wp/)) {
            $.ajax({
                type: "POST",
                url: "/Boxer/DoneBoxerAlready?orderId=" + orderId + "&result=" + result,
            }).done(function (set) {
                if (set.set == 'Done') {
                    $("#closeScanQRPrinter").click();
                    Swal.fire({
                        title: 'Success!',
                        text: 'The order is successfully completed!',
                        icon: 'success',
                        confirmButtonText: 'OK'
                    })

                    var si = setInterval(function () {
                        clearInterval(si);
                        document.location = '/'
                    }, 1000);
                }


            });

        }
        else {
            $("#closeScanQRPrinter").click();
            Swal.fire({
                title: 'Oops!',
                text: 'The Printer QR Code must have WP in text!',
                icon: 'error',
                confirmButtonText: 'OK'
            })
        }
    }
    function error(err) {
        //console.error(err);
    }
}

function ScanQRPrinter() {
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
    $("#ScanQRPrinterBtn").click();

    const scanner = new Html5QrcodeScanner('readerPrinter', {
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

        ajaxLoaderRetry('show');
        if (result.toLowerCase().match(/^wp/)) {
            callDoneBoxer(result);
        }
        else {
            $("#closeScanQRPrinter").click();
            Swal.fire({
                title: 'Oops!',
                text: 'The Printer QR Code must have WP in text!',
                icon: 'error',
                confirmButtonText: 'OK'
            })
        }
    }

    function callDoneBoxer(result, retries = 3) {
        $.ajax({
            type: "POST",
            url: "/Boxer/DoneBoxer?orderId=" + orderId + "&result=" + result,
        }).done(function (set) {
            if (set.set == 'Done') {
                $("#closeScanQRPrinter").click();

                ajaxLoaderRetry('hide');


                Swal.fire({
                    title: 'Success!',
                    text: 'The order is successfully completed!',
                    icon: 'success',
                    confirmButtonText: 'OK'
                })

                var si = setInterval(function () {
                    clearInterval(si);
                    document.location = '/'
                }, 1000);

                retryCountReset();
            }

            else if (set.set == 'Failed') {
                $("#closeScanQRPrinter").click();

                ajaxLoaderRetry('hide');

                if (set.hasSystemError == 'SystemError') {
                    Swal.fire({
                        title: 'Opps!',
                        text: 'Unable to RTS due to system error. Please try again later.',
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
                else if (set.hasSystemError == 'SystemErrorPack') {
                    Swal.fire({
                        title: 'Opps!',
                        text: 'System error, please try again.',
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
                else {
                    Swal.fire({
                        title: 'Opps!',
                        text: 'The order is unable to process at the moment. Please try again later!',
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
                retryCountReset();
            }

            else if (set.set == "CancelledOrders") {
                ajaxLoaderRetry('hide');

                Swal.fire({
                    title: 'Oops!',
                    text: 'This order was cancelled while you are packing',
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

                retryCountReset();
            }
            else if (set.set == "Exception") {
                updateRetryMessage();

                if (retries > 0) {
                    setTimeout(function () {
                        callDoneBoxer(result, retries - 1);
                    }, 3000);
                }
                else {
                    ajaxLoaderRetry('hide');

                    Swal.fire({
                        title: 'Oops!',
                        text: 'Please retry to proceed',
                        icon: 'error',
                        confirmButtonText: 'OK'
                    })
                    retryCountReset();
                }
            }
            else if (set.set == "TrackingNumberNotFound") {
                ajaxLoaderRetry('hide');

                Swal.fire({
                    title: 'Oops!',
                    text: 'Tracking No. not found. Please retry to proceed',
                    icon: 'error',
                    confirmButtonText: 'OK'
                })
                retryCountReset();
            }
            else {
                ajaxLoaderRetry('hide');
                retryCountReset();
            }
        });
    }

    function error(err) {
        //console.error(err);
    }
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

            //Swal.fire({
            //    title: 'Succes!',
            //    text: 'Item(s) updated to OOS!',
            //    icon: 'success',
            //    confirmButtonText: 'OK'
            //}).then((result) => {
            //    /* Read more about isConfirmed, isDenied below */
            //    if (result.isConfirmed) {
            //        document.location = '/Picker/Index'
            //    }
            //})
            var si = setInterval(function () {
                clearInterval(si);
                document.location = '/Picker/Index'
            }, 1000);
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
            }, 1000);
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
            }, 1000);
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

function ToPOS() {
    ajaxLoader('show');
    $.ajax({
        type: "POST",
        url: "/Boxer/CheckDiscrepancy?orderId=" + orderId,
    }).done(function (set) {
        ajaxLoader('hide');
        if (set.set == 'Discrepancy') {
            $("#incorrectOrderModalBtn").click();

        }
        else if (set.set == 'Existinpos') {
            ScanQRPrinterAlready();
            //Swal.fire({
            //    title: 'Oops',
            //    text: 'The order are already in POS',
            //    icon: 'error',
            //    confirmButtonText: 'OK'
            //})
        }
        else {
            //var si = setInterval(function () {
            //    clearInterval(si);
            //    document.location = '/'
            //}, 1000);
            ScanQRPrinter();
        }
    });
}


function DoneOrders() {

    $("#scanOrdersDiv").html(orderForScan);

    $("#doneItemModalBtn").click();
}

function ScanQRForOrders(orderId) {
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

        $('#scan' + orderId).addClass("badge-success");
        $('#txt' + orderId).val(result);

        var num = 0;
        for (i = 0; i < orderNumbers.length; i++) {
            var orderVal = $("#txt" + orderNumbers[i].orderId).val()

            if (orderVal != "") {
                num++;
            }
        }

        if (num == orderNumbers.length) {
            $("#OKScanQR").prop("disabled", false);
        }
        else {
            $("#OKScanQR").prop("disabled", true);
        }


        //if (currentUPC == scannedUPC) {
        //    $("#txtValid").removeClass("text-danger");
        //    $("#txtValid").addClass("text-success");
        //    $("#txtValid").text("Barcode matched!");
        //    $(".numberField").prop("disabled", false);
        //    $("#btnSave").prop("disabled", false);
        //    $("#txtNum").focus();
        //}
        //else {

        //    $("#txtValid").removeClass("text-success");
        //    $("#txtValid").addClass("text-danger");
        //    $("#txtValid").text("Barcode dosent match!");
        //    $(".numberField").prop("disabled", true);
        //    $("#btnSave").prop("disabled", true);
        //    $("#txtNum").focus();
        //}
    }
    function error(err) {
        //console.error(err);
    }
}

function DonePicker() {
    $.ajax({
        type: "GET",
        url: "/Picker/DonePicker",
    }).done(function (set) {
        if (set.set.length > 0) {
            var si = setInterval(function () {
                clearInterval(si);
                document.location = '/'
            }, 1000);
        }
    });
}

function OrderIncorrect() {


    $.ajax({
        type: "GET",
        url: "/Boxer/ToPOS?orderId=" + orderId,
    }).done(function (set) {
        if (set.set == 'Success') {
            var si = setInterval(function () {
                clearInterval(si);
                document.location = '/'
            }, 1000);

            $.ajax({
                type: "GET",
                url: "/Shopee/sampleSignalR",
            }).done(function (set) {
                // do nothing send discrepancy to oms via signalR
            });

        }
        else {
            var si = setInterval(function () {
                clearInterval(si);
                document.location = '/'
            }, 1000);

            $.ajax({
                type: "GET",
                url: "/Shopee/sampleSignalR",
            }).done(function (set) {
                // do nothing send discrepancy to oms via signalR
            });
        }

    });




}


$(document).on('click', '#btnSend', function () {

    var result = $("#txtBarcodes").val();

    barcodeInput(result);
});

function barcodeInput(result) {
    currentUPC = result;

    $.ajax({
        type: "GET",
        url: "/Boxer/ScannedUPC?upc=" + result + "&orderId=" + orderId,
    }).done(function (set) {
        if (set.status == "Correct") {
            $("#txtValid").removeClass("text-danger");
            $("#txtValid").addClass("text-success");
            $("#txtValid").text("Barcode matched!");
            $("#txtNum").prop("disabled", false);
            $("#txtNum").val(1);
            $("#btnSave").prop("disabled", false);
            $("#txtNum").focus();
        }
        else {

            $("#txtValid").removeClass("text-success");
            $("#txtValid").addClass("text-danger");
            $("#txtValid").text("Barcode doesn't exist!");
            $("#txtNum").prop("disabled", true);
            $("#btnSave").prop("disabled", true);
        }



    })
}

function SaveScan() {

    ajaxLoader('show');
    //var result = $("#txtBarcodes").val(); 
    var itemQty = parseInt($("#txtNum").val());

    $.ajax({
        type: "GET",
        url: "/Boxer/SaveScan?upc=" + currentUPC + "&orderId=" + orderId + "&qty=" + itemQty,
    }).done(function (set) {
        if (set.status == "Existing") {
            ajaxLoader('hide');
            var si = setInterval(function () {
                clearInterval(si);
                document.location = '/Boxer/Index?result=' + set.set
            }, 1000);

        }

        else if (set.status == "NotExistBarcode") {
            ajaxLoader('hide');
            Swal.fire({
                title: 'Oops',
                text: 'Barcode not exist in the system!',
                icon: 'error',
                confirmButtonText: 'OK'
            })

        }
        else if (set.status == "MaxThreshold") {
            ajaxLoader('hide');
            Swal.fire({
                title: 'Oops',
                text: 'Maximum quantity threshold has been reached. Please contact IT',
                icon: 'error',
                confirmButtonText: 'OK'
            })
        }


    })
}

$(document).on("swipeleft", ".itemSwipe", function () {
    $(this).hide();
});



function deleteScan(orderId, skuId) {
    skuIdforDelete = skuId;
    orderIdforDelete = orderId;

    $("#deleteItemModalBtn").click();
}

function DeleteItem() {
    $.ajax({
        type: "POST",
        url: "/Boxer/DeleteItem?orderId=" + orderIdforDelete + "&skuId=" + skuIdforDelete,
    }).done(function (set) {
        if (set != null) {
            var si = setInterval(function () {
                clearInterval(si);
                document.location = '/Boxer/Index?result=' + set.set.orderId
            }, 1000);
        }
        else {
            Swal.fire({
                title: 'Oops',
                text: 'No item deleted!',
                icon: 'error',
                confirmButtonText: 'OK'
            })
        }
    })
}



//function sendToHub() {


//    connection.invoke('SendDiscrepancyNotif', 'sampleDiscName', 'sampleDiscMessage').catch(function (err) {
//        return console.error(err.toString());
//    });

//}