
$(document).ready(function () {
    var now = new Date();

    var day = ("0" + now.getDate()).slice(-2);
    var month = ("0" + (now.getMonth() + 1)).slice(-2);

    var today = now.getFullYear() + "-" + (month) + "-" + (day);

    console.log('today', today)
    var firstDay = now.getFullYear() + "-" + (month) + "-01";
    var lastDay = lastday();

    console.log('lastDay', lastDay)

    $('#dateFrom').val(firstDay);
    $('#dateTo').val(lastDay);

    CheckNIB(firstDay, lastDay);

    $(".flatpickr").flatpickr({

        maxDate: new Date().fp_incr(1095),
        //static: true
    });

});

function getDiscrepancyNotification() {
    $.ajax({
        url: `/DiscrepancyCenter/GetDiscrepancyCount`,
        type: 'GET',

        success: function (data) {
            $.each(data, function (count, item) {

                $('#spanCount').text(item.boxId)
            })

            console.log(data);



        }
    });
}

var lastday = function () {
    var date = new Date();
    var firstDay = new Date(date.getFullYear(), date.getMonth(), 1);
    var lastDay = new Date(date.getFullYear(), date.getMonth() + 1, 0);
    var lastDayMonth = lastDay.getMonth() + 1;
    var strLastDayMonth = lastDayMonth.toString();
    if (lastDayMonth < 10) {
        strLastDayMonth = "0" + strLastDayMonth;
    }

    var lastDayWithSlashes = lastDay.getFullYear() + '-' + strLastDayMonth + '-' + (lastDay.getDate());

    return lastDayWithSlashes;
}

function getOMSDashboard(dateFrom, dateTo) {

    getWaitingOrders(dateFrom, dateTo);
    getWaitingToCollect(dateFrom, dateTo);
    getWaitingToPick(dateFrom, dateTo);
    getPickedOrders(dateFrom, dateTo);
    getForBoxingOrders(dateFrom, dateTo);
    getToShipOrders(dateFrom, dateTo);
    getAverageOrderPerHour(dateFrom, dateTo);
    getNotOnFulfillment(dateFrom, dateTo);
    getOutOfStock(dateFrom, dateTo);
    getPickingTimePerItem(dateFrom, dateTo);
    getPickingTimePerJO(dateFrom, dateTo);
    getPackingTimePerItem(dateFrom, dateTo);
    getPackingTimePerJO(dateFrom, dateTo);
    getShippedStatus(dateFrom, dateTo);
    viewTopPickerPerItem(dateFrom, dateTo);
    viewTopPickerPerJO(dateFrom, dateTo);

}

function getDates(date) {
    var day = date.getDate();
    var month = date.getMonth() + 1;
    var year = date.getFullYear();

    return [year, month, day].join('-')
}

$("#dateFrom").change(function () {
    var dateFrom = new Date($('#dateFrom').val());
    var dateTo = new Date($('#dateTo').val());


    if (dateFrom > dateTo) {
        Swal.fire(
            'Wrong Input!',
            'Date From is greater than Date To',
            'error'
        )

        var day = ("0" + dateTo.getDate()).slice(-2);
        var month = ("0" + (dateTo.getMonth() + 1)).slice(-2);
        var today = dateTo.getFullYear() + "-" + (month) + "-" + (day);

        $('#dateFrom').val(today);

        dateFrom = new Date($('#dateFrom').val());
    }

    var strdtFrom = getDates(dateFrom);
    var strdtTo = getDates(dateTo);

    getOMSDashboard(strdtFrom, strdtTo)


});

$("#dateTo").change(function () {
    var dateFrom = new Date($('#dateFrom').val());
    var dateTo = new Date($('#dateTo').val());

    if (dateTo < dateFrom) {
        Swal.fire(
            'Wrong Input!',
            'Date To is less than Date From',
            'error'
        )

        var day = ("0" + dateFrom.getDate()).slice(-2);
        var month = ("0" + (dateFrom.getMonth() + 1)).slice(-2);
        var today = dateFrom.getFullYear() + "-" + (month) + "-" + (day);

        $('#dateTo').val(today);

        dateTo = new Date($('#dateTo').val());
    }

    var strdtFrom = getDates(dateFrom);
    var strdtTo = getDates(dateTo);

    getOMSDashboard(strdtFrom, strdtTo)



});




function CheckNIB(firstDay, lastDay) {
    $.ajax({
        type: "GET",
        url: "/Home/ViewNIB"
    }).done(function (set) {
        if (set.user == 'AutoReloadShopee') {

            document.location = '/AutoReload/IndexShopee'
        }

        else if (set.user == 'AutoReloadLazada') {

            document.location = '/AutoReload/IndexLazada'
        }
        else if (set.user == 'AutoReloadReRun') {

            document.location = '/AutoReload/IndexReRun'
        }
        else if (set.user == 'AutoReloadCatchLazada') {

            document.location = '/AutoReload/IndexCatchLazada'
        }
        else if (set.user == 'AutoReloadCatchShopee') {

            document.location = '/AutoReload/IndexCatchShopee'
        }
        else if (set.user == 'OMS') {

            getOMSDashboard(firstDay, lastDay)
            getDiscrepancyNotification();
            //viewTopPickerPerItem();
            //viewTopPickerPerJO();
            viewTopPackerPerItem();
            viewTopPackerPerJO();

            Chart.defaults.global.defaultFontFamily = 'Nunito', '-apple-system,system-ui,BlinkMacSystemFont,"Segoe UI",Roboto,"Helvetica Neue",Arial,sans-serif';
            Chart.defaults.global.defaultFontColor = '#858796';

            // Pie Chart Example


            $.ajax({

                type: "POST",
                url: $("#divChartDashboard").data("request-url"),
                /*url: $("#divSaveTrans").data("request-url"),
                data: JSON.stringify(data),*/
                dataType: "json",
                contentType: "application/json;charset=utf-8",
                success: function (data) {
                    let lazadactx = document.getElementById("lazadaPieChart");
                    let lazadaPieChart = new Chart(lazadactx, {
                        type: 'doughnut',
                        data: {
                            labels: ["Cleared Order", "Out Of Stock", "Not in Basement"],
                            datasets: [{
                                data: data.lazada,
                                backgroundColor: ['#1cc88a', '#e74a3b', '#4e73df'],
                                hoverBackgroundColor: ['#17a673', '#ef1b07', '#2e59d9'],
                                hoverBorderColor: "rgba(234, 236, 244, 1)",
                            }],
                        },
                        options: {
                            maintainAspectRatio: false,
                            tooltips: {
                                backgroundColor: "rgb(255,255,255)",
                                bodyFontColor: "#858796",
                                borderColor: '#dddfeb',
                                borderWidth: 1,
                                xPadding: 15,
                                yPadding: 15,
                                displayColors: false,
                                caretPadding: 10,
                            },
                            legend: {
                                display: false
                            },
                            cutoutPercentage: 80,
                        },
                    });

                    let shopeectx = document.getElementById("shopeePieChart");
                    let shopeePieChart = new Chart(shopeectx, {
                        type: 'bar',
                        data: {
                            labels: ["Cleared Order", "Out Of Stock", "Not in Basment"],
                            datasets: [{
                                data: data.shopee,
                                backgroundColor: ['#1cc88a', '#e74a3b', '#4e73df'],
                                hoverBackgroundColor: ['#17a673', '#ef1b07', '#2e59d9'],
                                hoverBorderColor: "rgba(234, 236, 244, 1)",
                            }],
                        },
                        options: {
                            maintainAspectRatio: false,
                            tooltips: {
                                backgroundColor: "rgb(255,255,255)",
                                bodyFontColor: "#858796",
                                borderColor: '#dddfeb',
                                borderWidth: 1,
                                xPadding: 15,
                                yPadding: 15,
                                displayColors: false,
                                caretPadding: 10,
                            },
                            legend: {
                                display: false
                            },
                            cutoutPercentage: 80,
                        },
                    });
                },
                error: function (request, status, error) {

                    alert(error);
                }
            })

        } else if (set.user == 'Runner') {
            $("#toggleButton").addClass("d-none");
            if (set.set.length > 0) {
                Swal.fire({
                    title: 'Notification!',
                    text: 'You got Item(s) to Collect!',
                    icon: 'info',
                    confirmButtonText: 'Start'
                }).then((result) => {
                    /* Read more about isConfirmed, isDenied below */
                    if (result.isConfirmed) {
                        document.location = '/Runner/Index'
                    }
                })
            }
        } else if (set.user == 'Picker') {
            if (set.set.length > 0) {
                Swal.fire({
                    title: 'Notification!',
                    text: 'You got new Order(s) to Pick!',
                    icon: 'info',
                    confirmButtonText: 'Start'
                }).then((result) => {
                    /* Read more about isConfirmed, isDenied below */
                    if (result.isConfirmed) {
                        document.location = '/Picker/Index'
                    }
                })
            }
        }
        else if (set.user == 'Boxer') {
            if (set.set.length > 0) {
                //Swal.fire({
                //    title: 'Notification!',
                //    text: 'You got new Order(s) to Pick!',
                //    icon: 'info',
                //    confirmButtonText: 'Start'
                //}).then((result) => {
                //    /* Read more about isConfirmed, isDenied below */
                //    if (result.isConfirmed) {
                //        document.location = '/Picker/Index'
                //    }
                //})
            }
        }
    });

}

changeChartDesignLazada = function (type) {

    $.ajax({

        type: "POST",
        url: $("#divChartDashboard").data("request-url"),
        /*url: $("#divSaveTrans").data("request-url"),
        data: JSON.stringify(data),*/
        dataType: "json",
        contentType: "application/json;charset=utf-8",
        success: function (data) {
            let lazadactx = document.getElementById("lazadaPieChart");
            let lazadaPieChart = new Chart(lazadactx, {
                type: type,
                data: {
                    labels: ["Cleared Order", "Out Of Stock", "Not in Basement"],
                    datasets: [{
                        data: data.lazada,
                        backgroundColor: ['#1cc88a', '#e74a3b', '#4e73df'],
                        hoverBackgroundColor: ['#17a673', '#ef1b07', '#2e59d9'],
                        hoverBorderColor: "rgba(234, 236, 244, 1)",
                    }],
                },
                options: {
                    maintainAspectRatio: false,
                    tooltips: {
                        backgroundColor: "rgb(255,255,255)",
                        bodyFontColor: "#858796",
                        borderColor: '#dddfeb',
                        borderWidth: 1,
                        xPadding: 15,
                        yPadding: 15,
                        displayColors: false,
                        caretPadding: 10,
                    },
                    legend: {
                        display: false
                    },
                    cutoutPercentage: 80,
                },
            });
        },
        error: function (request, status, error) {

            alert(error);
        }
    })
}

changeChartDesignShopee = function (type) {

    $.ajax({

        type: "POST",
        url: $("#divChartDashboard").data("request-url"),
        /*url: $("#divSaveTrans").data("request-url"),
        data: JSON.stringify(data),*/
        dataType: "json",
        contentType: "application/json;charset=utf-8",
        success: function (data) {

            let shopeectx = document.getElementById("shopeePieChart");
            let shopeePieChart = new Chart(shopeectx, {
                type: type,
                data: {
                    labels: ["Cleared Order", "Out Of Stock", "Not in Basment"],
                    datasets: [{
                        data: data.shopee,
                        backgroundColor: ['#1cc88a', '#e74a3b', '#4e73df'],
                        hoverBackgroundColor: ['#17a673', '#ef1b07', '#2e59d9'],
                        hoverBorderColor: "rgba(234, 236, 244, 1)",
                    }],
                },
                options: {
                    maintainAspectRatio: false,
                    tooltips: {
                        backgroundColor: "rgb(255,255,255)",
                        bodyFontColor: "#858796",
                        borderColor: '#dddfeb',
                        borderWidth: 1,
                        xPadding: 15,
                        yPadding: 15,
                        displayColors: false,
                        caretPadding: 10,
                    },
                    legend: {
                        display: false
                    },
                    cutoutPercentage: 80,
                },
            });
        },
        error: function (request, status, error) {

            alert(error);
        }
    })

}
var isSuccess = false;
function ScanQRForOrders() {
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

        CheckScanQR(result);

    }
    function error(err) {
        //console.error(err);
    }
}
function SubmitOrderNumber() {
    var qr = $("#txtQrCodes").val();

    CheckScanQR(qr);
}
function CheckScanQR(result) {


    if (result.toLowerCase().match(/^tub/)) {
        $.ajax({
            type: "GET",
            url: "/Home/ScannedQr?result=" + result,
        }).done(function (set) {
            if (set.set == "NotInUse") {
                Swal.fire({
                    title: 'Oops...',
                    text: 'The container is not in use',
                    icon: 'error',
                    confirmButtonText: 'OK'
                })
            }
            else if (set.set == "Discrepancy") {
                Swal.fire({
                    title: 'Oops...',
                    text: 'The orders are in Discrepancy Center',
                    icon: 'error',
                    confirmButtonText: 'OK'
                })
            }
            else {

                window.location.href = '/Boxer/index?result=' + set.set;
            }

            //if (set.set.length > 0) {
            //    if (set.box != null) {
            //        Swal.fire({
            //            title: 'Oops...',
            //            text: 'The container is not in use',
            //            icon: 'error',
            //            confirmButtonText: 'OK'
            //        })
            //    }
            //    else if (set.discrepancy != null) {
            //        Swal.fire({
            //            title: 'Oops...',
            //            text: 'The orders are in Discrepancy Center',
            //            icon: 'error',
            //            confirmButtonText: 'OK'
            //        })
            //    }
            //    else {

            //        window.location.href = '/Boxer/index?result=' + set.orderId;
            //    }
            //}
            //else {


            //    Swal.fire({
            //        title: 'Oops...',
            //        text: 'The container is not in use',
            //        icon: 'error',
            //        confirmButtonText: 'OK'
            //    })


            //}
        });
    }
    else {
        $("#closeScanQRPrinter").click();
        Swal.fire({
            title: 'Oops!',
            text: 'The QR Code must have TUB in text!',
            icon: 'error',
            confirmButtonText: 'OK'
        })
    }
}
$('#scanBarcode').on('hide.bs.modal', function (e) {
    // do something...
    if (!isSuccess) {
        $("#html5-qrcode-button-camera-stop").click();
    }
})

function getWaitingOrders(dateFrom, dateTo) {
    $.ajax({
        url: `/Home/GetOMSDashboard?condition=WAITING&dateFrom=${dateFrom}&dateTo=${dateTo}`,
        type: 'GET',
        dataType: 'json', // added data type
        success: function (data) {

            $('#waitingOrder').text(numberWithCommas(data.count_result));


        }
    });
}

function getWaitingToCollect(dateFrom, dateTo) {
    $.ajax({
        url: `/Home/GetOMSDashboard?condition=WAITINGTOCOLLECT&dateFrom=${dateFrom}&dateTo=${dateTo}`,
        type: 'GET',
        dataType: 'json', // added data type
        success: function (data) {

            $('#waitingToCollect').text(numberWithCommas(data.count_result));


        }
    });
}

//function getForTransferring(dateFrom, dateTo) {
//    $.ajax({
//        url: `/Home/GetOMSDashboard?condition=FORTRANSFERRING&dateFrom=${dateFrom}&dateTo=${dateTo}`,
//        type: 'GET',
//        dataType: 'json', // added data type
//        success: function (data) {

//            $('#forTransferring').text(data.count_result);


//        }
//    });
//}

function getWaitingToPick(dateFrom, dateTo) {
    $.ajax({
        url: `/Home/GetOMSDashboard?condition=WAITINGTOPICK&dateFrom=${dateFrom}&dateTo=${dateTo}`,
        type: 'GET',
        dataType: 'json', // added data type
        success: function (data) {

            $('#waitingToPick').text(numberWithCommas(data.count_result));


        }
    });
}

function getPickedOrders(dateFrom, dateTo) {
    $.ajax({
        url: `/Home/GetOMSDashboard?condition=FORPICKING&dateFrom=${dateFrom}&dateTo=${dateTo}`,
        type: 'GET',
        dataType: 'json', // added data type
        success: function (data) {

            $('#pickOrders').text(numberWithCommas(data.count_result));


        }
    });
}

function getForBoxingOrders(dateFrom, dateTo) {
    $.ajax({
        url: `/Home/GetOMSDashboard?condition=FORBOXING&dateFrom=${dateFrom}&dateTo=${dateTo}`,
        type: 'GET',
        dataType: 'json', // added data type
        success: function (data) {

            $('#boxOrders').text(numberWithCommas(data.count_result));


        }
    });
}

function getToShipOrders(dateFrom, dateTo) {
    $.ajax({
        url: `/Home/GetOMSDashboard?condition=TOSHIP&dateFrom=${dateFrom}&dateTo=${dateTo}`,
        type: 'GET',
        dataType: 'json', // added data type
        success: function (data) {

            $('#toShipOrders').text(numberWithCommas(data.count_result));


        }
    });
}
function getAverageOrderPerHour(dateFrom, dateTo) {
    $.ajax({
        url: `/Home/GetOMSDashboard?condition=AVERAGEORDERS&dateFrom=${dateFrom}&dateTo=${dateTo}`,
        type: 'GET',
        dataType: 'json', // added data type
        success: function (data) {

            $('#averageOrdersPerHour').text(numberWithCommas(data.count_result));


        }
    });
}
/////
function getNotOnFulfillment(dateFrom, dateTo) {
    $.ajax({
        url: `/Home/GetOMSDashboard?condition=NOF&dateFrom=${dateFrom}&dateTo=${dateTo}`,
        type: 'GET',
        dataType: 'json', // added data type
        success: function (data) {

            $('#notOnFulfillment').text(numberWithCommas(data.count_result));


        }
    });
}

function getOutOfStock(dateFrom, dateTo) {
    $.ajax({
        url: `/Home/GetOMSDashboard?condition=OOS&dateFrom=${dateFrom}&dateTo=${dateTo}`,
        type: 'GET',
        dataType: 'json', // added data type
        success: function (data) {

            $('#outOfStock').text(numberWithCommas(data.count_result));


        }
    });
}

function getShippedStatus(dateFrom, dateTo) {
    $.ajax({
        url: `/Home/GetOMSDashboard?condition=SHIPPEDSTATUS&dateFrom=${dateFrom}&dateTo=${dateTo}`,
        type: 'GET',
        dataType: 'json', // added data type
        success: function (data) {

            $('#courierPicked').text(numberWithCommas(data.count_result));


        }
    });
}

function getPickingTimePerItem(dateFrom, dateTo) {
    let item;
    $.ajax({
        url: `/Home/GetOMSDashboard?condition=PICKINGTIMEPERITEM&dateFrom=${dateFrom}&dateTo=${dateTo}`,
        type: 'GET',
        dataType: 'json', // added data type
        success: function (data) {

            item = data.count_result.toString();
            const result = item.split(".");
            let seconds = result[1];
            let minutes = parseInt(result[0]);
            let strSeconds = "0." + seconds;
            let subSeconds = Math.round(parseFloat(strSeconds) * 60);
            let strMinutes = minutes.toString();

            let strSecond = subSeconds.toString();
            if (subSeconds <= 9) {
                strSecond = "0" + strSecond;
            }
            if (minutes <= 9) {
                strMinutes = "0" + strMinutes;
            }

            let strTotalMinutes = strMinutes + ":" + strSecond;

            $('#freeItem').text(strTotalMinutes);
            $('#txtPickTimePerItem').val(strTotalMinutes)
        }

    });




}

function getPickingTimePerJO(dateFrom, dateTo) {
    $.ajax({
        url: `/Home/GetOMSDashboard?condition=PICKINGTIMEPERJO&dateFrom=${dateFrom}&dateTo=${dateTo}`,
        type: 'GET',
        dataType: 'json', // added data type
        success: function (data) {

            let item = data.count_result.toString();
            const result = item.split(".");
            let seconds = result[1];
            let minutes = parseInt(result[0]);
            let strSeconds = "0." + seconds;
            let subSeconds = Math.round(parseFloat(strSeconds) * 60);
            let strMinutes = minutes.toString();

            let strSecond = subSeconds.toString();
            if (subSeconds <= 9) {
                strSecond = "0" + strSecond;
            }
            if (minutes <= 9) {
                strMinutes = "0" + strMinutes;
            }

            let strTotalMinutes = strMinutes + ":" + strSecond;

            $('#timePerJO').text(strTotalMinutes);
            $('#txtPickTimePerJO').val(strTotalMinutes)


        }
    });
}

function getPackingTimePerItem(dateFrom, dateTo) {
    let item;
    $.ajax({
        url: `/Home/GetOMSDashboard?condition=PACKERTIMEPERITEM&dateFrom=${dateFrom}&dateTo=${dateTo}`,
        type: 'GET',
        dataType: 'json', // added data type
        success: function (data) {

            item = data.count_result.toString();
            const result = item.split(".");
            let seconds = result[1];
            let minutes = parseInt(result[0]);
            let strSeconds = "0." + seconds;
            let subSeconds = Math.round(parseFloat(strSeconds) * 60);
            let strMinutes = minutes.toString();

            let strSecond = subSeconds.toString();
            if (subSeconds <= 9) {
                strSecond = "0" + strSecond;
            }
            if (minutes <= 9) {
                strMinutes = "0" + strMinutes;
            }

            let strTotalMinutes = strMinutes + ":" + strSecond;

            $('#packingTimePerItem').text(strTotalMinutes);
            $('#txtPackTimePerItem').val(strTotalMinutes)
        }

    });




}

function getPackingTimePerJO(dateFrom, dateTo) {
    $.ajax({
        url: `/Home/GetOMSDashboard?condition=PACKERTIMEPERJO&dateFrom=${dateFrom}&dateTo=${dateTo}`,
        type: 'GET',
        dataType: 'json', // added data type
        success: function (data) {

            let item = data.count_result.toString();
            const result = item.split(".");
            let seconds = result[1];
            let minutes = parseInt(result[0]);
            let strSeconds = "0." + seconds;
            let subSeconds = Math.round(parseFloat(strSeconds) * 60);
            let strMinutes = minutes.toString();

            let strSecond = subSeconds.toString();
            if (subSeconds <= 9) {
                strSecond = "0" + strSecond;
            }
            if (minutes <= 9) {
                strMinutes = "0" + strMinutes;
            }

            let strTotalMinutes = strMinutes + ":" + strSecond;

            $('#packingTimePerJO').text(strTotalMinutes);
            $('#txtPackTimePerJO').val(strTotalMinutes)

        }
    });
}


$('#viewOrdersPerHour').on('click', function () {



    let moduleModal = $("#orderPerHourModal");
    moduleModal.modal("show");

    getOrdersPerHour();

})


$('#viewToShipOrders').on('click', function () {



    let moduleModal = $("#orderPerHourModal");
    moduleModal.modal("show");

    getReadyToShipOrdersPerHour();

})

$('#viewPicker').on('click', function () {

    $("h3").text("Picker Details");

    let moduleModal = $("#pickerAvgTable");
    moduleModal.modal("show");

    var count = 0;
    $.ajax({
        method: "GET",
        url: `/Home/GetPickingTimePerPicker?condition=PerItem&dateFrom=${$('#dateFrom').val()}&dateTo=${$('#dateTo').val()}`,
    }).done(function (set) {
        tableGenerator('#listOfPicker', set, 'bad', 'PerItem');
    });


})

$('#viewPickerPerJO').on('click', function () {
    $("h3").text("Picker Details");
    let moduleModal = $("#pickerAvgTable");
    moduleModal.modal("show");
    var count = 0;
    $.ajax({
        method: "GET",
        url: `/Home/GetPickingTimePerPicker?condition=PerJO&dateFrom=${$('#dateFrom').val()}&dateTo=${$('#dateTo').val()}`,
    }).done(function (set) {
        tableGenerator('#listOfPicker', set, 'bad', 'PerJO');

    });


})

$('#viewPackerPerItem').on('click', function () {
    $("h3").text("Packer Details");
    let moduleModal = $("#pickerAvgTable");
    moduleModal.modal("show");
    var count = 0;
    $.ajax({
        method: "GET",
        url: '/Home/GetPackingTimePerPicker?condition=PerItem',
    }).done(function (set) {
        tableGenerator('#listOfPicker', set, 'bad', 'PerItem');
    });


})

$('#viewPackerPerJO').on('click', function () {
    $("h3").text("Packer Details");
    let moduleModal = $("#pickerAvgTable");
    moduleModal.modal("show");
    var count = 0;
    $.ajax({
        method: "GET",
        url: '/Home/GetPackingTimePerPicker?condition=PerJO',
    }).done(function (set) {
        tableGenerator('#listOfPicker', set, 'bad', 'PerJO');

    });


})

function viewTopPickerPerItem(dateFrom, dateTo) {
    $.ajax({
        method: "GET",
        url: `/Home/GetPickingTimePerPicker?condition=TopPerItem&dateFrom=${dateFrom}&dateTo=${dateTo}`,
    }).done(function (set) {
        tableGenerator('#tblTop5PickerPerItem', set, 'bad', 'PerItem');

    });
}


function viewTopPickerPerJO(dateFrom, dateTo) {
    $.ajax({
        method: "GET",
        url: `/Home/GetPickingTimePerPicker?condition=TopPerJO&dateFrom=${dateFrom}&dateTo=${dateTo}`,
    }).done(function (set) {
        tableGenerator('#tblTop5PickerPerJO', set, 'bad', 'PerJO');

    });
}

function viewTopPackerPerItem() {
    $.ajax({
        method: "GET",
        url: '/Home/GetPackingTimePerPicker?condition=TopPerItem',
    }).done(function (set) {
        tableGenerator('#tblTop5PackerPerItem', set, 'bad', 'PerItem');

    });
}


function viewTopPackerPerJO() {
    $.ajax({
        method: "GET",
        url: '/Home/GetPackingTimePerPicker?condition=TopPerJO',
    }).done(function (set) {
        tableGenerator('#tblTop5PackerPerJO', set, 'bad', 'PerJO');

    });
}


$('#viewItems').on('click', function () {
    let moduleModal = $("#listItemModal");
    moduleModal.modal("show");

    var dateFrom = getDates(new Date($('#dateFrom').val()));
    var dateTo = getDates(new Date($('#dateTo').val()));



    $.ajax({
        method: "GET",
        url: `/Home/GetInventoryItem?dateFrom=${dateFrom}&dateTo=${dateTo}`,
    }).done(function (set) {
        tableInvetoryItem('#listOfitem', set, 'bad');
    });



})

function getOrdersPerHour() {
    var dateFrom = getDates(new Date($('#dateFrom').val()));
    var dateTo = getDates(new Date($('#dateTo').val()));
    $.ajax({
        url: `/Home/GetOrdersPerHour?dateFrom=${dateFrom}&dateTo=${dateTo}`,
        type: 'GET',
        dataType: 'json', // added data type
        success: function (set) {

            tableOrdersPerHour('#tblOrderPerHour', set, 'bad');
        }
    });
}


$('#viewWaitingOrder').on('click', function () {
    let moduleModal = $("#orderItemModal");
    moduleModal.modal("show");


    GetOrderItemsDetails('WAITING', dateFrom, dateTo);


})

$('#viewWaitingToCollect').on('click', function () {
    let moduleModal = $("#orderItemModal");
    moduleModal.modal("show");


    GetOrderItemsDetails('WAITINGTOCOLLECT', dateFrom, dateTo);


})


$('#viewWaitingToPick').on('click', function () {
    let moduleModal = $("#orderItemModal");
    moduleModal.modal("show");


    GetOrderItemsDetails('WAITINGTOPICK', dateFrom, dateTo);


})

$('#viewPickOrders').on('click', function () {
    let moduleModal = $("#orderItemModal");
    moduleModal.modal("show");


    GetOrderItemsDetails('FORPICKING', dateFrom, dateTo);


})

$('#viewBoxOrders').on('click', function () {
    let moduleModal = $("#orderItemModal");
    moduleModal.modal("show");


    GetOrderItemsDetails('FORBOXING', dateFrom, dateTo);


})

$('#viewCourierPicked').on('click', function () {
    let moduleModal = $("#orderItemModal");
    moduleModal.modal("show");


    GetOrderItemsDetails('SHIPPEDSTATUS', dateFrom, dateTo);


})


$('#viewNOFOrders').on('click', function () {
    let moduleModal = $("#orderItemModal");
    moduleModal.modal("show");


    GetOrderItemsDetails('NOF', dateFrom, dateTo);


})

$('#viewOutOfStockOrders').on('click', function () {
    let moduleModal = $("#orderItemModal");
    moduleModal.modal("show");


    GetOrderItemsDetails('OOS', dateFrom, dateTo);


})

function tableOrdersPerHour(table, data, flag, type) {


    let dTable = $(table).DataTable();

    if (data.set.length > 0) {
        var counter = 0;
        dTable.destroy();

        dTable = $(table).DataTable({


            "responsive": true,
            "lengthChange": false,
            "scrollCollapse": true,
            "paging": false,
            "searching": false,
            "data": data.set,
            dom: 'Bfrtip',
            buttons: [{
                extend: 'print',
                text: '<i class="bi bi-printer-fill"></i> Print',
                className: '',
                messageTop: "Average Time Report",
                title: 'Average Time Report',
                orientation: 'landscape',
                customize: function (win) {
                    $(win.document.body)
                        .css('font-size', '7pt')
                    $(win.document.body).find('table')
                        .addClass('compact')
                        .css('font-size', 'inherit');
                    $(win.document.body).find('td > input').parent().text(function () {
                        return $(this).find('.handicapIndex').val();
                    });
                },
                exportOptions: {
                    //stripHtml: false,
                    orthogonal: "myExport",
                    columns: [0, 1, 2, 3],
                },
                //TEMPshareholderName.length - 1 > 0 ? TEMPshareholderName.join(" OR ") : TEMPshareholderName
            }, {
                extend: 'excelHtml5',
                text: '<i class= "fa fa-download"></i > Download',
                className: '',
                messageTop: "Average Time Report",
                title: 'Average Time Report',
                exportOptions: {
                    orthogonal: "myExport",
                    columns: [0, 1, 2, 3],
                },
                action: function (e, indicator, dt, node, config) {
                    //console.log(indicator);
                    //iqwerty.toast.toast('Downloading report file .... Please wait', toastInfo);
                    if (indicator) {
                        $.fn.DataTable.ext.buttons.excelHtml5.action.call(this, e, indicator, dt, node, config);
                        //iqwerty.toast.toast('Report downloaded successfully', toastSuccess);
                    } else { iqwerty.toast.toast('Download failed', toastFailed); }

                }
            }],
            "columns": [

                { "data": "description" },
                { "data": "lazada" },
                { "data": "shopee" },



            ]
        });



    }
    else {
        dTable.clear().draw();
        dTable.destroy();
        dTable = $(table).DataTable({

            "language": {
                "emptyTable": "No data available"
            }
        });
    }
}

function tableGenerator(table, data, flag, type) {


    let dTable = $(table).DataTable();

    if (data.set.length > 0) {
        var counter = 0;
        dTable.destroy();

        dTable = $(table).DataTable({


            "responsive": true,
            "lengthChange": false,
            "scrollCollapse": true,
            "paging": false,
            "searching": false,
            "data": data.set,
            dom: 'Bfrtip',
            buttons: [{
                extend: 'print',
                text: '<i class="bi bi-printer-fill"></i> Print',
                className: '',
                messageTop: "Average Time Report",
                title: 'Average Time Report',
                orientation: 'landscape',
                customize: function (win) {
                    $(win.document.body)
                        .css('font-size', '7pt')
                    $(win.document.body).find('table')
                        .addClass('compact')
                        .css('font-size', 'inherit');
                    $(win.document.body).find('td > input').parent().text(function () {
                        return $(this).find('.handicapIndex').val();
                    });
                },
                exportOptions: {
                    //stripHtml: false,
                    orthogonal: "myExport",
                    columns: [0, 1, 2, 3],
                },
                //TEMPshareholderName.length - 1 > 0 ? TEMPshareholderName.join(" OR ") : TEMPshareholderName
            }, {
                extend: 'excelHtml5',
                text: '<i class= "fa fa-download"></i > Download',
                className: '',
                messageTop: "Average Time Report",
                title: 'Average Time Report',
                exportOptions: {
                    orthogonal: "myExport",
                    columns: [0, 1, 2, 3],
                },
                action: function (e, indicator, dt, node, config) {
                    //console.log(indicator);
                    //iqwerty.toast.toast('Downloading report file .... Please wait', toastInfo);
                    if (indicator) {
                        $.fn.DataTable.ext.buttons.excelHtml5.action.call(this, e, indicator, dt, node, config);
                        //iqwerty.toast.toast('Report downloaded successfully', toastSuccess);
                    } else { iqwerty.toast.toast('Download failed', toastFailed); }

                }
            }],
            "columns": [

                { "data": "row_num" },
                { "data": "username" },
                { "data": "userFullname" },
                { "data": "pickingTime" },
                { "data": "itemCount" },
                { "data": "orderCount" },
                {
                    "data": "totalPickingTime",
                    render: function (data) {
                        let item = data.toString();
                        const result = item.split(".");
                        let seconds = result[1];
                        let minutes = parseInt(result[0]);
                        let strSeconds = "0." + seconds;
                        let subSeconds = Math.round(parseFloat(strSeconds) * 60);
                        let strMinutes = minutes.toString();

                        let strSecond = subSeconds.toString();
                        if (subSeconds <= 9) {
                            strSecond = "0" + strSecond;
                        }
                        if (minutes <= 9) {
                            strMinutes = "0" + strMinutes;
                        }

                        let strTotalMinutes = strMinutes + ":" + strSecond;
                        return strTotalMinutes;
                    }
                }

            ]
        });

        if (type == 'PerJO') {
            var dt = $(table).DataTable();
            dt.column(4).visible(false);
            dt.column(5).visible(true);


        }
        else {
            var dt = $(table).DataTable();
            dt.column(5).visible(false);
            dt.column(4).visible(true);


        }

    }
    else {
        dTable.clear().draw();
        dTable.destroy();
        dTable = $(table).DataTable({

            "language": {
                "emptyTable": "No data available"
            }
        });
    }
}


function GetOrderItemsDetails(filter) {
    var dateFrom = getDates(new Date($('#dateFrom').val()));
    var dateTo = getDates(new Date($('#dateTo').val()));

    $.ajax({
        method: "GET",
        url: `/Home/GetOrderDetails?condition=${filter}&dateFrom=${dateFrom}&dateTo=${dateTo}`,
    }).done(function (set) {
        tableOrderItem('#tblOrderItem', set, 'good');
    });


}

function tableOrderItem(table, data, flag, isNew) {


    let dTable = $(table).DataTable();
    if (data.set.length > 0) {
        var counter = 0;
        dTable.destroy();

        dTable = $(table).DataTable({

            "responsive": true,
            "lengthChange": false,
            "scrollCollapse": true,
            "aaSorting": [],
            "paging": false,
            "searching": false,
            "data": data.set,
            dom: 'Bfrtip',
            buttons: [{
                extend: 'print',
                text: '<i class="bi bi-printer-fill"></i> Print',
                className: '',
                messageTop: "Order Details",
                title: 'Order Details',
                orientation: 'landscape',
                customize: function (win) {
                    $(win.document.body)
                        .css('font-size', '7pt')
                    $(win.document.body).find('table')
                        .addClass('compact')
                        .css('font-size', 'inherit');
                    $(win.document.body).find('td > input').parent().text(function () {
                        return $(this).find('.handicapIndex').val();
                    });
                },
                exportOptions: {
                    //stripHtml: false,
                    orthogonal: "myExport",
                    columns: [0, 1, 2, 3],
                },
                //TEMPshareholderName.length - 1 > 0 ? TEMPshareholderName.join(" OR ") : TEMPshareholderName
            }, {
                extend: 'excelHtml5',
                text: '<i class= "fa fa-download"></i > Download',
                className: '',
                messageTop: "Order Details",
                title: 'Order Details',
                exportOptions: {
                    orthogonal: "myExport",
                    columns: [0, 1, 2, 3],
                },
                action: function (e, indicator, dt, node, config) {
                    //console.log(indicator);
                    //iqwerty.toast.toast('Downloading report file .... Please wait', toastInfo);
                    if (indicator) {
                        $.fn.DataTable.ext.buttons.excelHtml5.action.call(this, e, indicator, dt, node, config);
                        //iqwerty.toast.toast('Report downloaded successfully', toastSuccess);
                    } else { iqwerty.toast.toast('Download failed', toastFailed); }

                }
            }],
            "columns": [
                { "data": "row_num" },
                { "data": "orderId" },
                {
                    "data": "dateCreatedAt",
                    "render": function (data, type) {
                        let d = new Date(data),
                            month = '' + (d.getMonth() + 1),
                            day = '' + d.getDate(),
                            year = d.getFullYear();

                        if (month.length < 2)
                            month = '0' + month;
                        if (day.length < 2)
                            day = '0' + day;
                        return data = [month, day, year].join('-');
                    }
                },
                { "data": "item_count" },

                { "data": "total_amount" },


                {
                    "data": "orderId",
                    "render": function (data, type, row) {
                        return data = '<div ><i class="fa fa-eye" style="cursor:pointer" onclick="getShopeeOrderItems(this)" order_id="' + row.orderId + '" flag="' + flag + '" customerID="' + row.customerID + '"></i></div>'
                    }
                },
            ]
        });

    }
    else {
        dTable.clear().draw();
        dTable.destroy();
        dTable = $(table).DataTable({
            "scrollY": '50vh',
            "responsive": true,
            "lengthChange": false,
            "searching": false,
            "scrollX": true,
            "scrollCollapse": true,
            "paging": true,
            "paging": true,
            "language": {
                "emptyTable": "No data available"
            }
        });
    }
}
function tableInvetoryItem(table, data, flag) {


    let dTable = $(table).DataTable();
    if (data.set.length > 0) {
        var counter = 0;
        dTable.destroy();

        dTable = $(table).DataTable({

            "responsive": true,
            "lengthChange": false,
            "scrollCollapse": true,
            "aaSorting": [],
            "paging": false,
            "searching": false,
            "data": data.set,
            dom: 'Bfrtip',
            buttons: [{
                extend: 'print',
                text: '<i class="bi bi-printer-fill"></i> Print',
                className: '',
                messageTop: "Order Details",
                title: 'Order Details',
                orientation: 'landscape',
                customize: function (win) {
                    $(win.document.body)
                        .css('font-size', '7pt')
                    $(win.document.body).find('table')
                        .addClass('compact')
                        .css('font-size', 'inherit');
                    $(win.document.body).find('td > input').parent().text(function () {
                        return $(this).find('.handicapIndex').val();
                    });
                },
                exportOptions: {
                    //stripHtml: false,
                    orthogonal: "myExport",
                    columns: [0, 1, 2, 3],
                },
                //TEMPshareholderName.length - 1 > 0 ? TEMPshareholderName.join(" OR ") : TEMPshareholderName
            }, {
                extend: 'excelHtml5',
                text: '<i class= "fa fa-download"></i > Download',
                className: '',
                messageTop: "Order Details",
                title: 'Order Details',
                exportOptions: {
                    orthogonal: "myExport",
                    columns: [0, 1, 2, 3],
                },
                action: function (e, indicator, dt, node, config) {
                    //console.log(indicator);
                    //iqwerty.toast.toast('Downloading report file .... Please wait', toastInfo);
                    if (indicator) {
                        $.fn.DataTable.ext.buttons.excelHtml5.action.call(this, e, indicator, dt, node, config);
                        //iqwerty.toast.toast('Report downloaded successfully', toastSuccess);
                    } else { iqwerty.toast.toast('Download failed', toastFailed); }

                }
            }],
            "columns": [

                { "data": "skuId" },
                {
                    "data": "itemImage",
                    "render": function (data) {

                        return '<div><img style="width: 50px;height: 50px;cursor:pointer;" id="previewImgShopee" href="' + data + '"  src="' + data + '" ></div>';
                    }
                },
                { "data": "itemDescription" },
                { "data": "orderQty" },
                { "data": "itemQty" }

            ]
        });

    }
    else {
        dTable.clear().draw();
        dTable.destroy();
        dTable = $(table).DataTable({
            "scrollY": '50vh',
            "responsive": true,
            "lengthChange": false,
            "searching": false,
            "scrollX": true,
            "scrollCollapse": true,
            "paging": true,
            "paging": true,
            "language": {
                "emptyTable": "No data available"
            }
        });
    }
}
function SupervisorCreds() {
    var id = $("#txtSupervisorID").val();
    var pin = $("#txtSupervisorPIN").val();
    $("#txtSupervisorID").val("");
    $("#txtSupervisorPIN").val("");

    $.ajax({
        type: "POST",
        url: "/Boxer/SupervisorCreds?Id=" + id + "&pin=" + pin,
    }).done(function (set) {
        if (set.set != null) {
            ScanQRForPrinter(id, pin);
        }
        else {
            Swal.fire({
                title: 'Oops',
                text: 'Invalid Supervisor Credentials!',
                icon: 'error',
                confirmButtonText: 'OK'
            })
        }
    })

}

function ScanQRForPrinter(id, pin) {
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

    $("#scanBarcodePrinterBtn").click();

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
        $("#closeScanPrinterModal").click();
        //$("#itemModalBtn").click();

        RePrintWaybill(id, pin, result);

    }
    function error(err) {
        //console.error(err);
    }
}

function RePrintWaybill(id, pin, result) {
    if (result.toLowerCase().match(/^wp/)) {
        $.ajax({
            type: "POST",
            url: "/Boxer/RePrintWaybill?Id=" + id + "&pin=" + pin + "&result=" + result,
        }).done(function (set) {
            //if (set != null) {
            //    var si = setInterval(function () {
            //        clearInterval(si);
            //        document.location = '/Boxer/Index?result=' + set.set.orderId
            //    }, 1000);
            //}
            //else {
            //    Swal.fire({
            //        title: 'Oops',
            //        text: 'No item deleted!',
            //        icon: 'error',
            //        confirmButtonText: 'OK'
            //    })
            //}
        })
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


function getOrderId() {
    $.ajax({
        type: "GET",
        url: "/Boxer/GetOrderId",
    }).done(function (set) {
        if (set.orderId != "" && set.module != "") {
            if (set.message == "Exist") {
                Swal.fire({
                    title: 'Success!',
                    text: 'The order has been successfully reprinted!',
                    icon: 'success',
                    confirmButtonText: 'OK'
                })
            }
            else {
                $("#lblModule").text(set.module == "shopee" ? "SHOPEE" : "LAZADA");
                $("#lblOrderId").text("#" + set.orderId);
                $("#reprintModalBtn").click();
            }
        }
        else {
            Swal.fire({
                title: 'Oops',
                text: 'No Orders available!',
                icon: 'error',
                confirmButtonText: 'OK'
            })
        }
    })
}

function numberWithCommas(number) {
    var parts = number.toString().split(".");
    parts[0] = parts[0].replace(/\B(?=(\d{3})+(?!\d))/g, ",");
    return parts.join(".");
}

getShopeeOrderItems = function (ths) {

    itemsPriceTotal = 0;
    var order_id = $(ths).attr("order_id");

    let moduleModal = $("#itemModal");



    $.ajax({
        type: "POST",
        url: "/Home/GetItemOrder?order_id=" + order_id,
        //data: JSON.stringify(obj),
        dataType: "json",
        contentType: "application/json;charset=utf-8",
        success: function (data) {
            moduleModal.modal("show");
            tableItemView('#listOfItems', data);
            for (i = 0; i < data.set.length; i++) {
                itemsPriceTotal = itemsPriceTotal + data.set[i].item_price;
            }
            $("#txtTotalPrice").text(itemsPriceTotal);

        },
        error: function (request, status, error) {

            alert(error);
        }
    })

}

function tableItemView(table, trans) {

    let dTable = $(table).DataTable();
    if (trans.set.length > 0) {
        var counter = 0;
        dTable.destroy();


        dTable = $(table).DataTable({
            "paging": true,
            "data": trans.set,
            dom: 'Bfrtip',
            buttons: [{
                extend: 'print',
                text: '<i class="bi bi-printer-fill"></i> Print',
                className: '',
                messageTop: "Shopee Orders",
                title: 'Shopee Orders Items',
                orientation: 'landscape',
                customize: function (win) {
                    $(win.document.body)
                        .css('font-size', '7pt')
                    $(win.document.body).find('table')
                        .addClass('compact')
                        .css('font-size', 'inherit');
                    $(win.document.body).find('td > input').parent().text(function () {
                        return $(this).find('.handicapIndex').val();
                    });
                },
                exportOptions: {
                    //stripHtml: false,
                    orthogonal: "myExport",
                    columns: [0, 1, 2, 3],
                },
                //TEMPshareholderName.length - 1 > 0 ? TEMPshareholderName.join(" OR ") : TEMPshareholderName
            }, {
                extend: 'excelHtml5',
                text: '<i class= "fa fa-download"></i > Download',
                className: '',
                messageTop: "Shopee Cleared Orders",
                title: 'Shopee Orders Items',
                exportOptions: {
                    orthogonal: "myExport",
                    columns: [0, 1, 2, 3],
                },
                action: function (e, indicator, dt, node, config) {
                    //console.log(indicator);
                    //iqwerty.toast.toast('Downloading report file .... Please wait', toastInfo);
                    if (indicator) {
                        $.fn.DataTable.ext.buttons.excelHtml5.action.call(this, e, indicator, dt, node, config);
                        //iqwerty.toast.toast('Report downloaded successfully', toastSuccess);
                    } else { iqwerty.toast.toast('Download failed', toastFailed); }

                }
            }],
            "columns": [
                { "data": "sku_id" },
                {
                    "data": "item_image",
                    "render": function (data) {

                        return '<div><img style="width: 80px;height: 80px;cursor:pointer;" id="previewImgShopee" href="' + data + '"  src="' + data + '" ></div>';
                    }
                },
                { "data": "item_description" },
                {
                    "data": "created_at",
                    "render": function (data, type) {
                        let d = new Date(data),
                            month = '' + (d.getMonth() + 1),
                            day = '' + d.getDate(),
                            year = d.getFullYear();

                        if (month.length < 2)
                            month = '0' + month;
                        if (day.length < 2)
                            day = '0' + day;
                        return data = [month, day, year].join('-');
                    }
                },
                { "data": "typeOfexception" },
                { "data": "upc" },
                {
                    "data": "item_price",

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
}

function getReadyToShipOrdersPerHour() {
    var dateFrom = getDates(new Date($('#dateFrom').val()));
    var dateTo = getDates(new Date($('#dateTo').val()));
    $.ajax({
        url: `/Home/GetReadyToShipOrdersPerHour?dateFrom=${dateFrom}&dateTo=${dateTo}`,
        type: 'GET',
        dataType: 'json', // added data type
        success: function (set) {

            tableOrdersPerHour('#tblOrderPerHour', set, 'bad');
        }
    });
}