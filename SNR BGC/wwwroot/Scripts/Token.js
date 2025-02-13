var isNew;

var itemsPriceTotal;

$(document).ready(function () {
    getLazadaStatus();
    getList();
    isNew = true;
});

getList = function () {
    $.ajax({
        method: "GET",
        url: '/Token/GetClearedOrders',
    }).done(function (set) {
        tableGenerator('#LazadaOrderTable', set, 'good');
        //$("#dateFromLazada").val(set.maxDate.replace("+08:00", ""));
        var dt = $('#LazadaOrderTable').DataTable();
        dt.column(3).visible(false);
        dt.column(4).visible(false);

        dt.column(6).visible(false);
        dt.column(7).visible(false);
        dt.column(8).visible(false);

        $('#withoutExcepTab').addClass('active');
        $('#withoutExcepTab').addClass('btn-primary');
        $("#liReportsLazada").hide()

        $("#withExcepTab").removeClass('active');
        $('#withExcepTab').removeClass('btn-primary');
        $("#totalOrdersCountLazada").text(set.set.length);

        $(".dt-button").addClass("btn btn-primary");
        $(".dt-button").removeClass("dt-button");
    });
}
getListExecption = function () {
    $.ajax({
        method: "GET",
        url: '/Token/GetExceptionsOrders',
    }).done(function (set) {
        tableGenerator('#LazadaOrderTable', set, 'bad');
        var dt = $('#LazadaOrderTable').DataTable();

        dt.column(6).visible(false);
        dt.column(7).visible(false);
        dt.column(8).visible(false);


        $("#totalOrdersCountLazada").text(set.set.length);
        $(".dt-button").addClass("btn btn-primary");
        $(".dt-button").removeClass("dt-button");
    });
}


getDoneOrders = function () {
    $.ajax({
        method: "GET",
        url: '/Token/GetDoneClearedOrders',
    }).done(function (set) {
        tableGenerator('#LazadaOrderTable', set, 'good');
        var dt = $('#LazadaOrderTable').DataTable();
        dt.column(3).visible(false);
        dt.column(4).visible(false);
        dt.column(7).visible(false);



        $("#totalOrdersCountLazada").text(set.set.length);
        $(".dt-button").addClass("btn btn-primary");
        $(".dt-button").removeClass("dt-button");
        console.log('GetDoneClearedOrders')
    });
}

getPOSOrders = function () {
    $.ajax({
        method: "GET",
        url: '/Token/GetDoneBoxOrders',
    }).done(function (set) {
        tableGenerator('#LazadaOrderTable', set, 'good');
        var dt = $('#LazadaOrderTable').DataTable();
        dt.column(3).visible(false);
        dt.column(4).visible(false);

       
       

        $("#totalOrdersCountLazada").text(set.set.length);
        $(".dt-button").addClass("btn btn-primary");
        $(".dt-button").removeClass("dt-button");
        console.log('GetDoneBoxOrders')
    });
}


getPickingOrders = function () {
    $.ajax({
        method: "GET",
        url: '/Token/GetCurrentlyPickingOrders',
    }).done(function (set) {
        tableGenerator('#LazadaOrderTable', set, 'bad');
        var dt = $('#LazadaOrderTable').DataTable();
        dt.column(3).visible(false);
        dt.column(4).visible(false);

        dt.column(7).visible(false);


        dt.column(8).visible(false);

        $("#totalOrdersCountLazada").text(set.set.length);
        $(".dt-button").addClass("btn btn-primary");
        $(".dt-button").removeClass("dt-button");
        console.log('GetCurrentlyPickingOrders')

    });
}

getPackingOrders = function () {
    $.ajax({
        method: "GET",
        url: '/Token/GetCurrentlyPackingOrders',
    }).done(function (set) {
        tableGenerator('#LazadaOrderTable', set, 'bad');
        var dt = $('#LazadaOrderTable').DataTable();
        dt.column(3).visible(false);
        dt.column(4).visible(false);

        


        $("#totalOrdersCountLazada").text(set.set.length);
        $(".dt-button").addClass("btn btn-primary");
        $(".dt-button").removeClass("dt-button");
        console.log('GetCurrentlyPickingOrders')

    });
}



$("#selectAllMaster").click(function () {
    $('input:checkbox').not(this).prop('checked', this.checked);
});

function tableGenerator(table, trans, flag, isNew) {


    let dTable = $(table).DataTable();
    if (trans.set.length > 0) {
        var counter = 0;
        dTable.destroy();

        dTable = $(table).DataTable({
            "paging": true,
            "aaSorting": true,
            "data": trans.set,
            dom: 'Bfrtip',
            buttons: [{
                extend: 'print',
                className: '',
                text: '<i class="bi bi-printer-fill"></i> Print',
                messageTop: "Lazada Orders",
                title: 'Lazada Orders',
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
                className: '',
                text: '<i class= "fa fa-download"></i > Download',
                messageTop: "Lazada Orders",
                title: 'Lazada Orders',
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

                { "data": "exception" },
                { "data": "typeOfException" },
                { "data": "total_amount" },
                { "data": "pickerUser" },

                { "data": "username" },

                { "data": "module" },
                {
                    "data": "orderId",
                    "render": function (data, type, row) {
                        return data = '<div ><i class="fa fa-eye" style="cursor:pointer" onclick="getOrderItems(this)" order_id="' + row.orderId + '" flag="' + flag + '" customerID="' + row.customerID + '"></i></div>'
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
}

function tableGeneratorV2(table, trans, flag, isNew) {


    let dTable = $(table).DataTable();
    if (trans.set.length > 0) {
        var counter = 0;
        dTable.destroy();

        dTable = $(table).DataTable({
            "paging": true,
            "data": trans.set,
            "aaSorting": true,

            dom: 'Bfrtip',
            buttons: [{
                extend: 'print',
                className: '',
                text: '<i class="bi bi-printer-fill"></i> Print',
                messageTop: "Lazada Orders",
                title: 'Lazada Orders',
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
                className: '',
                text: '<i class= "fa fa-download"></i > Download',
                messageTop: "Lazada Orders",
                title: 'Lazada Orders',
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
                { "data": "item_count" },
                { "data": "total_amount" },
                { "data": "tub_no" },
                {
                    "data": "orderId",
                    "render": function (data, type, row) {
                        return data = '<div ><i class="fa fa-eye" style="cursor:pointer" onclick="getOrderItems(this)" order_id="' + row.orderId + '" flag="' + flag + '" customerID="' + row.customerID + '"></i></div>'
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
}

function tableGeneratorView(table, trans) {

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
                messageTop: "Lazada Orders",
                title: 'Lazada Orders Items',
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
                messageTop: "Lazada Orders",
                title: 'Lazada Orders Items',
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
                    "data": "total_item_price",

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

/*
##############################################
#FUNCTION NAME : generateCodes
#PARAMETERS    : 
#DESCRIPTION   : To open search modal
#CREATED BY    : Michael Angelo A. De Fiesta
#DATE CREATED  : 08/30/2022
#MODIFIED BY   :
##############################################
*/
generateCodes = function () {
    var url = 'https://auth.lazada.com/oauth/authorize?response_type=code&force_auth=true&redirect_uri=https://lazpee.snrshopping.com/Token/VerifyLazadaToken&client_id=107315';
    window.open(url);

}

/*
##############################################
#FUNCTION NAME : viewLazadaOrders
#PARAMETERS    :
#DESCRIPTION   : To append the div of orders
#CREATED BY    : Michael Angelo A. De Fiesta
#DATE CREATED  : 08/31/2022
#MODIFIED BY   :
##############################################
*/
viewLazadaOrders = function (flag) {
    var dateFrom = $("#dateFromLazada").val() + ':00.000'
    var dateTo = $("#dateToLazada").val() + ':00.000'

    ajaxLoader('show');
    if (isNew) {
        if (flag == 'good') {
            getList();
            ajaxLoader('hide');
        }
        else if (flag == 'bad') {
            getListExecption();
            ajaxLoader('hide');
        }
        else if (flag == 'pickingOrders') {
            getPickingOrders();
            ajaxLoader('hide');
        }
        else if (flag == 'forBoxingTab') {
            getDoneOrders();
            ajaxLoader('hide');
        }
        else if (flag == 'toPOSTab') {
            getPOSOrders();
            ajaxLoader('hide');
        }
        else if (flag == 'forCurrentlyPacking') {
            getPackingOrders();
            ajaxLoader('hide');
        }
    }
    else {
        $.ajax({
            type: "POST",
            url: $("#divViewOrders").data("request-url") + "?dateFrom=" + dateFrom + "&dateTo=" + dateTo + "&filter=" + flag,
            /*  url: $("#divFetchOrder").data("request-url"),*/
            //data: JSON.stringify(obj),
            dataType: "json",
            contentType: "application/json;charset=utf-8",
            success: function (data) {
                ajaxLoader('hide');


                $("#totalOrdersCountLazada").text(data.length);
                tableGenerator('#LazadaOrderTable', data, flag, isNew);
                //var myObj = data.set;
                //var html = "";

                //if ($.isEmptyObject(myObj)) {
                //    html = '<tr class="text-center"><td colspan="8">No available data.</td></tr>'
                //    $("#totalOrdersCountLazada").text(0)

                //} else {

                //    $("#totalOrdersCountLazada").text(myObj.length)
                //    for (var x = 0; x < myObj.length; x++) {

                //        let d = new Date(myObj[x]['dateCreatedAt']),
                //            month = '' + (d.getMonth() + 1),
                //            day = '' + d.getDate(),
                //            year = d.getFullYear();

                //        if (month.length < 2)
                //            month = '0' + month;
                //        if (day.length < 2)
                //            day = '0' + day;
                //        let formattedDate = [month, day, year].join('-');

                //        html += '<tr>'
                //        html += '<td style="cursor:pointer" >' + myObj[x]['orderId'] + '</td>';
                //        html += '<td style="cursor:pointer" >' + formattedDate + '</td>';
                //        html += '<td style="cursor:pointer" >' + myObj[x]['item_count'] + '</td>';
                //        html += '<td style="cursor:pointer" >' + myObj[x]['total_amount'] + '</td>';
                //        html += '<td style="cursor:pointer" onclick="getOrderItems(this)" order_id="' + myObj[x]['orderId'] + '" flag="' + flag + '" customerID="' + myObj[x]['customerID'] + '"><i class="fa fa-eye" style="cursor:pointer"></i></td>';
                //        html += '</tr>';
                //        /* html += '<div class="col-xl-3 col-md-6 mb-4" order_id="' + myObj[x]['orderId'] + '" customer_name = "' + myObj[x]['customerID'] + '" style="cursor:pointer;" onclick="getOrderItems(this)">'
                //             html += '<div class="card border-left-success shadow h-100 py-2">'
                //                 html += '<div class="card-body">'
                //                      html += '<div class="row no-gutters align-items-center">'
                //                         html += '<div class="col mr-2">'
                //                              html += '<div class="font-weight-bold text-success text-uppercase mb-1">'
                //                                  html += 'Order #: ' + myObj[x]['orderId'] + ''
                //                                  html += '</div>'
                //                                  html += '<div class="h6 mb-0 font-weight-bold text-gray-800">' + myObj[x]['customerID']+ '</div>'
                //                                  html += '<div class="h6 mb-0 font-weight-bold text-gray-800">' + myObj[x]['item_price'] + '</div>'
                //                                  html += '<div class="h6 mb-0 font-weight-bold text-gray-800">Number of Item : ' + myObj[x]['item_quantity'] + '</div>'
                //                             html += '</div>'
                //                             html += '<div class="col-auto">'
                //                             html += '<i class="fas fa-calendar fa-2x text-gray-300"></i>'
                //                             html += '</div>'
                //                         html += '</div>'
                //                     html += '</div>'
                //                 html += '</div>'
                //             html += '</div>'*/

                //    }
                //}
                //$("#LazadaOrderTable > tbody").html(html)

            },
            error: function (request, status, error) {

                alert(error);
            }
        })
    }
}





/*
##############################################
#FUNCTION NAME : getLazadaOrders
#PARAMETERS    :
#DESCRIPTION   : To append the div of orders
#CREATED BY    : Michael Angelo A. De Fiesta
#DATE CREATED  : 08/31/2022
#MODIFIED BY   :
##############################################
*/
function reProcess() {

    ajaxLoader('show');

    $.ajax({
        type: "POST",
        url: $("#divReProcess").data("request-url"),
        /*  url: $("#divFetchOrder").data("request-url"),*/
        //data: JSON.stringify(obj),
        dataType: "json",
        contentType: "application/json;charset=utf-8",
        success: function (data) {

            //$("#successModalBtnLazada").click();
            //$("#successModalDivLazada").html("Lazada orders Sucessfully Updated!");
            Swal.fire(
                'Success!',
                'Lazada orders Sucessfully Updated!.',
                'success'
            )


            ajaxLoader('hide');
            var si = setInterval(function () {
                clearInterval(si);
                document.location = '/Token/ViewOrdersLazada'
            }, 2000);
        },
        error: function (request, status, error) {

            alert(error);
        }
    })

}

getLazadaStatus = function () {
    $.ajax({
        type: "POST",
        url: $("#divGetStatus").data("request-url"),
        dataType: "json",
        contentType: "application/json;charset=utf-8",
        success: function (result) {
            $('#myToggle').prop('checked', result.data);
            $('#myToggle').bootstrapToggle(result.data ? 'on' : 'off');
        },
        error: function (request, status, error) {
            alert(error);
        }
    })
}

setToggleStatus = function (e) {
    $.ajax({
        type: "POST",
        url: $("#divToggleStatus").data("request-url") + "?status=" + e.checked,
        dataType: "json",
        contentType: "application/json;charset=utf-8",
        success: function (data) {
        },
        error: function (request, status, error) {
            alert(error);
        }
    })
}

getLazadaOrders = function () {
    isNew = false;
    var dateFrom = $("#dateFromLazada").val().length == 16 ? $("#dateFromLazada").val() + ':00.000' : $("#dateFromLazada").val() + '.000';
    var dateFromCancelled = $("#dateFromLazada").val().length == 16 ? $("#dateFromLazada").val() + ':00.000' : $("#dateFromLazada").val() + '.000';
    var dateTo = $("#dateToLazada").val() + ':00.000'

    ajaxLoader('show');

    $.ajax({
        type: "POST",
        url: $("#divFetchOrder").data("request-url") + "?dateFrom=" + dateFrom +  "&dateTo=" + dateTo,
        /*  url: $("#divFetchOrder").data("request-url"),*/
        //data: JSON.stringify(obj),
        dataType: "json",
        contentType: "application/json;charset=utf-8",
        success: function (data) {

            ajaxLoader('hide');
            if (data.set == "GreaterThanFrom4Hours") {
                Swal.fire({
                    title: 'Oops...',
                    text: 'The Date To should not less than 30 minutes from the date and time today!',
                    icon: 'error',
                    confirmButtonText: 'OK'
                })
            }
            else if (data.set == "DateFromIsGreaterThanDateTo") {
                Swal.fire({
                    title: 'Oops...',
                    text: 'The Date From should not greater than the Date To!',
                    icon: 'error',
                    confirmButtonText: 'OK'
                })
            }
            else {
                $("#totalOrdersCountLazada").text(data.length);
                //$("#withoutExcepTab").click();
                //$("#successModalBtnLazada").click();
                //$("#successModalDivLazada").html("Lazada orders Sucessfully Updated!");
                Swal.fire(
                    'Success!',
                    'Lazada orders Sucessfully Updated!.',
                    'success'
                )
            }
            


        },
        error: function (request, status, error) {

            alert(error);
        }
    })
}


/*
##############################################
#FUNCTION NAME : getOrderItems
#PARAMETERS    :
#DESCRIPTION   : To append the div of orders
#CREATED BY    : Michael Angelo A. De Fiesta
#DATE CREATED  : 08/31/2022
#MODIFIED BY   :
##############################################
*/
getOrderItems = function (ths) {

    itemsPriceTotal = 0;
    var order_id = $(ths).attr("order_id");
    var customerID = $(ths).attr("customerID");
    const flag = $(ths).attr("flag");
    let moduleModal = $("#itemModal");
    ajaxLoader('show');

    $.ajax({
        type: "POST",
        url: $("#divFetchItem").data("request-url") + "?order_id=" + order_id + "&flag=" + "lazada",
        //data: JSON.stringify(obj),
        dataType: "json",
        contentType: "application/json;charset=utf-8",
        success: function (data) {

            ajaxLoader('hide');
            tableGeneratorView("#listOfItems", data)
            for (i = 0; i < data.set.length; i++) {
                itemsPriceTotal = itemsPriceTotal + data.set[i].total_item_price;
            }
            $("#txtTotalPrice").text(itemsPriceTotal);

            $(".dt-button").addClass("btn btn-primary");
            $(".dt-button").removeClass("dt-button");
            //var html = "";
            //if ($.isEmptyObject(data.set)) {
            //    html = '<tr class="text-center"><td colspan="8">No available data.</td></tr>'

            //} else {
            //    var totalPrice = 0
            //    for (var x = 0; x < data.set.length; x++) {

            //        let d = new Date(data.set[x]['created_at']),
            //            month = '' + (d.getMonth() + 1),
            //            day = '' + d.getDate(),
            //            year = d.getFullYear();

            //        if (month.length < 2)
            //            month = '0' + month;
            //        if (day.length < 2)
            //            day = '0' + day;
            //        let formattedDate = [month, day, year].join('-');

            //        if (data.set[x]['exception'] == 1) {
            //            html += '<tr id="Tr' + data.set[x]['sku_id'] + '" style="color:red;background:#f1dede">';

            //        } else {
            //            html += '<tr id="Tr' + data.set[x]['sku_id'] + '" >'

            //        }
            //        html += '<td  id="item_id' + x + '" >' + data.set[x]['sku_id'] + '</td>';
            //        html += '<td style="display:none;" id="order_number' + x + '" >' + data.set[x]['orderId'] + '</td>';
            //        html += '<td> <img style="width: 80px;height: 80px;cursor:pointer;" id="previewImg" href="' + data.set[x]['item_image'] + '"  src="' + data.set[x]['item_image'] + '" ></td>';
            //        html += '<td id="item_desc' + x + '" >' + data.set[x]['item_description'] + '</td>';
            //        html += '<td id="item_date' + x + '" style="white-space: nowrap;">' + formattedDate + '</td>';
            //        html += '<td id="item_exception' + x + '">' + data.set[x]['typeOfexception'] + '</td>';
            //        html += '<td id="item_exception' + x + '"><svg class="barcode" jsbarcode-format="CODE128" jsbarcode-value="' + data.set[x]['upc'].toString() + '" jsbarcode-textmargin="0" jsbarcode-fontoptions="bold"  jsbarcode-width="1" jsbarcode-height="40" ></svg ></td>';
            //        html += '<td class="text-success text-center" style="font-weight:700;" id="item_price' + x + '">₱' + data.set[x]['item_price'] + '</td>';
            //        /*if (data.set[x]['exception'] == 1) {
            //            html += '<td style="cursor:pointer;" class="text-center selectAllCheck" ><input  type="checkbox" class="checkBoxTd" /></td>';
            //        }*/

            //        html += '</tr>';
            //        totalPrice = totalPrice + parseFloat(data.set[x]['item_price']);



            //    }
            //    /*if (flag == 'good') {
            //        $(".selectAllCheck").hide()

            //    } else {
            //        $(".selectAllCheck").show()
            //    }*/
            //    html += '<tr class="text-right" style="font-size: 22px;font-weight: 800;"><td colspan="6">Grand Total: </td> <td colspan="1" style="text-align: center;color:red;" id="trn_grand_total">₱' + totalPrice.toLocaleString() + '  </td></tr>'

            //}
            $("#itemsTxtHeader").text('ORDER # :  ' + order_id)
            $("#itemsOrderIdHeader").text(order_id);
            $("#customerTxtHeader").text('CUSTOMER ID : ' + customerID.toUpperCase());
           /* $("#itemModalBtn").click();*/
            moduleModal.modal("show");
            if ($("#withExcepTab").hasClass("active")) {
                $("#lazadaMtpBtn").show()
                $("#lazadaMtpCloseBtn").hide()
            } else {
                $("#lazadaMtpBtn").hide()
                $("#lazadaMtpCloseBtn").show()
            }
            //$("#listOfItems > tbody").html(html)


            JsBarcode(".barcode").init();

        },
        error: function (request, status, error) {

            alert(error);
        }
    })



}
/*
##############################################
#FUNCTION NAME : getItemTransacton
#PARAMETERS    :
#DESCRIPTION   : To get and save the lazada order items
#CREATED BY    : Michael Angelo A. De Fiesta
#DATE CREATED  : 09/18/2022
#MODIFIED BY   :
##############################################
*/
getItemTransacton = function () {
    var data = [];
    var count = 0;


    var sub = $('#ribbonTag').text();
    var subsplit = sub.split(" ");
    var subModule = subsplit[0];

    $('#listOfItems >tbody >tr').each(function () {

        var id = $(this).attr('id');
        if (id) {
            data.push({
                'order_number': $('#order_number' + count).text(),
                'item_id': $('#order_number' + count).text(),
                'item_description': $('#item_desc' + count).text(),
                'item_price': +(parseFloat($('#item_price' + count).text().replace(/[^a-z0-9-]/g, "")).toFixed(2)),
                'trn_grand_total': +(parseFloat($('#trn_grand_total').text().replace(/[^a-z0-9-]/g, "")).toFixed(2)),
                'trn_user': $("#userNameProfile").text(),
                'status': 'shipped',
                'submodule': subModule

            })

        }
        count++;
    })

    ajaxLoader('show');
    $.ajax({

        type: "POST",
        /*url: $("#divSaveTrans").data("request-url") + "?transObj=" + data,*/
        url: $("#divSaveTrans").data("request-url"),
        data: JSON.stringify(data),
        dataType: "json",
        contentType: "application/json;charset=utf-8",
        success: function (data) {

            ajaxLoader('hide');


        },
        error: function (request, status, error) {

            alert(error);
        }
    })


}
printReportLazada = function () {

    var categ = $('#categoryLazadaReport').val();

    if (categ == "-") {

        return;
    }

    $.ajax({

        type: "POST",
        url: $("#divPrintReports").data("request-url") + "?categ=" + categ,
        /*url: $("#divSaveTrans").data("request-url"),
        data: JSON.stringify(data),*/
        dataType: "json",
        contentType: "application/json;charset=utf-8",
        success: function (data) {

            var file = new Blob([data], { type: 'application/pdf' });
            var fileURL = URL.createObjectURL(file);
            window.open(fileURL);
            ajaxLoader('hide');


        },
        error: function (request, status, error) {

            alert(error);
        }
    })

}

/*
##############################################
#FUNCTION NAME : moveTopickerAlert
#PARAMETERS    :
#DESCRIPTION   : warning before to move in picker
#CREATED BY    : Michael Angelo A. De Fiesta
#DATE CREATED  : 10/08/2022
#MODIFIED BY   :
##############################################
*/

moveTopickerAlert = function () {

    $("#moveToPickerModalBtnLazada").click()
    $("#moveToPickerModalDivLazada").html("Are you sure this order is clear?");
    $("#exampleModalLongTitlemoveToPicker").html("Notification!")

}

/*
##############################################
#FUNCTION NAME : moveTopicker
#PARAMETERS    :
#DESCRIPTION   : warning before to move in picker
#CREATED BY    : Michael Angelo A. De Fiesta
#DATE CREATED  : 10/08/2022
#MODIFIED BY   :
##############################################
*/

moveTopicker = function () {

    let orderId = $("#itemsOrderIdHeader").text()

    $.ajax({

        type: "POST",
        url: $("#divMoveToPicker").data("request-url") + "?orderId=" + orderId,
        /*url: $("#divSaveTrans").data("request-url"),
        data: JSON.stringify(data),*/
        dataType: "json",
        contentType: "application/json;charset=utf-8",
        success: function (data) {
            $("#itemModal").modal("hide")
            $("#moveToPickerModalLazada").modal("hide")
            $("#successModalBtnLazada").click();
            $("#withExcepTab").click()
            $("#successModalDivLazada").html("Order successfully Cleared!");
        },
        error: function (request, status, error) {

            alert(error);
        }
    })


}

function clickPrintExcel(mod) {
    if (mod === 'print') {
        $('.buttons-print').click();
    }
    else if (mod === 'excel') {
        $('.buttons-excel').click();
    }
}
function Reload() {
    document.location = '/Token/ViewOrdersLazada';
}