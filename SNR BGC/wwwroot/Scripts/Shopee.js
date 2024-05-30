/*
##############################################
#FUNCTION NAME : fetchShopeeToken
#PARAMETERS    :
#DESCRIPTION   : To get and save the lazada order items
#CREATED BY    : Michael Angelo A. De Fiesta
#DATE CREATED  : 09/18/2022
#MODIFIED BY   :
##############################################
*/
var itemsPriceTotal;

var isNew;


$(document).ready(function () {
    getList();
    isNew = true;
});

getList = function () {
    $.ajax({
        method: "GET",
        url: '/Shopee/GetClearedOrders',
    }).done(function (set) {
        tableGenerator('#ShopeeOrderTable', set, 'good');

        //$("#dateFromShopee").val(set.maxDate.replace("+08:00", ""));
        var dt = $('#ShopeeOrderTable').DataTable();
        dt.column(3).visible(false);
        dt.column(4).visible(false);

        dt.column(6).visible(false);
        dt.column(7).visible(false);
        dt.column(8).visible(false);


        $('#withoutExcepTabShopee').addClass('active');
        $('#withoutExcepTabShopee').addClass('btn-primary');
        $("#liReportShopee").hide();

        $("#withExcepTabShopee").removeClass('active');
        $('#withExcepTabShopee').removeClass('btn-primary');
        $("#totalOrdersCountShopee").text(set.set.length);

        $(".dt-button").addClass("btn btn-primary");
        $(".dt-button").removeClass("dt-button");
    });
}
getListExecption = function () {
    $.ajax({
        method: "GET",
        url: '/Shopee/GetExceptionsOrders',
    }).done(function (set) {
        tableGenerator('#ShopeeOrderTable', set, 'bad');
        var dt = $('#ShopeeOrderTable').DataTable();
        dt.column(6).visible(false);
        dt.column(7).visible(false);
        dt.column(8).visible(false);



        $("#totalOrdersCountShopee").text(set.set.length);
        $(".dt-button").addClass("btn btn-primary");
        $(".dt-button").removeClass("dt-button");
    });
}


getDoneOrders = function () {
    $.ajax({
        method: "GET",
        url: '/Shopee/GetDoneClearedOrders',
    }).done(function (set) {
        tableGenerator('#ShopeeOrderTable', set, 'good');
        var dt = $('#ShopeeOrderTable').DataTable();
        dt.column(3).visible(false);
        dt.column(4).visible(false);
        dt.column(7).visible(false);



        $("#totalOrdersCountShopee").text(set.set.length);
        $(".dt-button").addClass("btn btn-primary");
        $(".dt-button").removeClass("dt-button");
    });
}

getPOSOrders = function () {
    $.ajax({
        method: "GET",
        url: '/Shopee/GetDoneBoxOrders',
    }).done(function (set) {
        tableGenerator('#ShopeeOrderTable', set, 'good');
        var dt = $('#ShopeeOrderTable').DataTable();
        dt.column(3).visible(false);
        dt.column(4).visible(false);




        $("#totalOrdersCountShopee").text(set.set.length);
        $(".dt-button").addClass("btn btn-primary");
        $(".dt-button").removeClass("dt-button");
    });
}


getPickingOrders = function () {
    $.ajax({
        method: "GET",
        url: '/Shopee/GetCurrentlyPickingOrders',
    }).done(function (set) {
        tableGenerator('#ShopeeOrderTable', set, 'good');
        var dt = $('#ShopeeOrderTable').DataTable();
        dt.column(3).visible(false);
        dt.column(4).visible(false);

        dt.column(7).visible(false);
        
        dt.column(8).visible(false);

        $("#totalOrdersCountShopee").text(set.set.length);
        $(".dt-button").addClass("btn btn-primary");
        $(".dt-button").removeClass("dt-button");
    });
}

getPackingOrders = function () {
    $.ajax({
        method: "GET",
        url: '/Shopee/GetCurrentlyPackingOrders',
    }).done(function (set) {
        tableGenerator('#ShopeeOrderTable', set, 'good');
        var dt = $('#ShopeeOrderTable').DataTable();
        dt.column(3).visible(false);
        dt.column(4).visible(false);



        $("#totalOrdersCountShopee").text(set.set.length);
        $(".dt-button").addClass("btn btn-primary");
        $(".dt-button").removeClass("dt-button");
    });
}


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
                text: '<i class="bi bi-printer-fill"></i> Print',
                className: '',
                messageTop: "Shopee Orders",
                title: 'Shopee Orders',
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
                messageTop: "Shopee Orders",
                title: 'Shopee Orders',
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
                { "data": "exceptions_count" },
                { "data": "typeOfException" },
                { "data": "total_amount" },
                { "data": "pickerUser" },
                { "data": "username" },
                { "data": "module" },
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
            /*  "paging": true,*/
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
            "aaSorting": true,
            "data": trans.set,
            dom: 'Bfrtip',
            buttons: [{
                extend: 'print',
                text: '<i class="bi bi-printer-fill"></i> Print',
                className: '',
                messageTop: "Shopee Orders",
                title: 'Shopee Orders',
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
                messageTop: "Shopee Orders",
                title: 'Shopee Orders',
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
                { "data": "exceptions_count" },
                { "data": "total_amount" },
                { "data": "tub_no" },

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
            /* "paging": true,*/
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


fetchShopeeOrder = function () {
    var dateFrom = $("#dateFromShopee").val().length == 16 ? $("#dateFromShopee").val() + ':00.000' : $("#dateFromShopee").val() + '.000';
    var dateTo = $("#dateToShopee").val() + ':00.000';

    console.log(dateFrom, "<<<<<<<<dateFromfetchShopeeOrder")
    console.log(dateTo, "<<<<<<<<dateTofetchShopeeOrder")

    //var filter = "angelo"

    ajaxLoader('show');
    $.ajax({
        type: "POST",
        url: $("#divViewShopeeOrder").data("request-url") + "?dateFrom=" + dateFrom + "&dateTo=" + dateTo,
        /*url: $("#divFetchShopeeToken").data("request-url"),*/
        /* data: JSON.stringify(data),*/
        dataType: "json",
        contentType: "application/json;charset=utf-8",
        success: function (data) {
            ajaxLoader('hide');

            if (data.set == "GreaterThanFrom4Hours") {
                Swal.fire({
                    title: 'Oops...',
                    text: 'The Date To should not less than 4 hours from the date and time today!',
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
                //$("#withoutExcepTabShopee").click();
                //$("#successModalBtnShopee").click();
                //$("#successModalDivShopee").html("Shopee orders Sucessfully Updated!");
                Swal.fire(
                    'Success!',
                    'Shopee orders Sucessfully Updated!.',
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
#FUNCTION NAME : viewShopeeOrders
#PARAMETERS    :
#DESCRIPTION   : To append shopee orders
#CREATED BY    : Michael Angelo A. De Fiesta
#DATE CREATED  : 09/29/2022
#MODIFIED BY   :
##############################################
*/
viewShopeeOrders = function (flag) {
    var dateFrom = $("#dateFromShopee").val() + '.000';
    var dateTo = $("#dateToShopee").val() + ':00.000';
    console.log(dateFrom, "<<<<<<<<dateFrom")
    console.log(dateTo, "<<<<<<<<dateTo")
    ajaxLoader('show');
    console.log(isNew)
    if (isNew) {
        if (flag == 'good') {
            getList();
            ajaxLoader('hide');
        }
        else if (flag == 'bad') {
            getListExecption();
            ajaxLoader('hide');
        }
        else if (flag == 'boxTab') {
            getDoneOrders();
            ajaxLoader('hide');
        }
        else if (flag == 'POSTab') {
            getPOSOrders();
            ajaxLoader('hide');
        }
        else if (flag == 'pickingOrders') {
            getPickingOrders();
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
            url: $("#divDisplayShopeeOrders").data("request-url") + "?dateFrom=" + dateFrom + "&dateTo=" + dateTo + "&filter=" + flag,
            /*  url: $("#divFetchOrder").data("request-url"),*/
            //data: JSON.stringify(obj),
            dataType: "json",
            contentType: "application/json;charset=utf-8",
            success: function (data) {
                ajaxLoader('hide');


                $("#totalOrdersCountShopee").text(data.length);
                tableGenerator('#LazadaOrderTable', data, flag, isNew);

                //var myObj = data.set;
                //var html = "";

                //if ($.isEmptyObject(myObj)) {
                //    html = '<tr class="text-center"><td colspan="8">No available data.</td></tr>'
                //    $("#totalOrdersCountShopee").text(0)

                //} else {

                //    $("#totalOrdersCountShopee").text(myObj.length)
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
                //        html += '<td style="cursor:pointer" onclick="getShopeeOrderItems(this)" order_id="' + myObj[x]['orderId'] + '" customerID="' + myObj[x]['customerID'] + '"><i class="fa fa-eye" style="cursor:pointer"></i></td>';
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
                //$("#ShopeeOrderTable > tbody").html(html)

            },
            error: function (request, status, error) {

                alert(error);
            }
        })
    }
}


/*
##############################################
#FUNCTION NAME : getShopeeOrderItems
#PARAMETERS    :
#DESCRIPTION   : To append the div of orders
#CREATED BY    : Michael Angelo A. De Fiesta
#DATE CREATED  : 09/29/2022
#MODIFIED BY   :
##############################################
*/
getShopeeOrderItems = function (ths) {

    itemsPriceTotal = 0;
    var order_id = $(ths).attr("order_id");
    var customerID = $(ths).attr("customerID");
    let moduleModal = $("#itemShopeeModal");

    ajaxLoader('show');

    $.ajax({
        type: "POST",
        url: $("#GetItemShopee").data("request-url") + "?order_id=" + order_id,
        //data: JSON.stringify(obj),
        dataType: "json",
        contentType: "application/json;charset=utf-8",
        success: function (data) {
            ajaxLoader('hide');
            $("#itemsShopeeTxtHeader").text('ORDER # :  ' + order_id)
            $("#itemsOrderIdHeaderShopee").text(order_id)
            $("#customerShopeeTxtHeader").text('CUSTOMER ID : ' + customerID.toUpperCase());
            moduleModal.modal("show");
            /* $("#itemShopeeModalBtn").click();*/
            if ($("#withExcepTabShopee").hasClass("active")) {
                $("#shopeeMtpBtn").show()
                $("#shopeeMtpCloseBtn").hide()
            } else {
                $("#shopeeMtpBtn").hide()
                $("#shopeeMtpCloseBtn").show()
            }
            tableGeneratorView('#listOfShopeeItems', data);
            for (i = 0; i < data.set.length; i++) {
                itemsPriceTotal = itemsPriceTotal + data.set[i].item_price;
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
            //            html += '<tr id="Tr' + data.set[x]['sku_id'] + '" style="color:red;background:#f1dede">'
            //        } else {
            //            html += '<tr id="Tr' + data.set[x]['sku_id'] + '">'
            //        }

            //        html += '<td id="item_id' + x + '" >' + data.set[x]['sku_id'] + '</td>';
            //        html += '<td style="display:none;" id="order_number' + x + '" >' + data.set[x]['orderId'] + '</td>';
            //        html += '<td> <img style="width: 80px;height: 80px;cursor:pointer;" id="previewImgShopee" href="' + data.set[x]['item_image'] + '"  src="' + data.set[x]['item_image'] + '" ></td>';
            //        html += '<td id="item_desc' + x + '">' + data.set[x]['item_description'] + '</td>';
            //        /*html += '<td id="item_quantity' + x + '">' + data.set[x]['item_quantity'] + '</td>';*/
            //        html += '<td style="white-space: nowrap;" id="item_date' + x + '">' + formattedDate + '</td>';
            //        html += '<td id="item_exception' + x + '">' + data.set[x]['typeOfexception'] + '</td>';
            //        html += '<td id="item_exception' + x + '"><svg class="barcode" jsbarcode-format="CODE128" jsbarcode-value="' + data.set[x]['upc'].toString() + '" jsbarcode-textmargin="0" jsbarcode-fontoptions="bold"  jsbarcode-width="1" jsbarcode-height="40" ></svg ></td>';
            //       /* html += '<td class="text-success text-center" style="font-weight:700;" id="item_price' + x + '">₱' + data.set[x]['item_price'] + '</td>';*/
            //        html += '<td class="text-success text-center" style="font-weight:700;" id="item_price' + x + '">₱' + data.set[x]['total_item_price'] + '</td>';
            //        //html += '<td style="cursor:pointer;" class="text-center" onclick="selectUserModal(this)" ><i class="fa fa-plus" style="cursor:pointer"></i></td>';
            //        html += '</tr>';
            //        totalPrice = totalPrice + parseFloat(data.set[x]['total_item_price']);
            //    }
            //    html += '<tr class="text-right" style="font-size: 22px;font-weight: 800;"><td colspan="6">Grand Total: </td> <td colspan="1" style="text-align: center;color:red;" id="trn_grand_total">₱' + totalPrice.toLocaleString() + '  </td></tr>'
            //}
            //$("#itemsShopeeTxtHeader").text('ORDER # :  ' + order_id)
            //$("#itemsOrderIdHeaderShopee").text(order_id)
            //$("#customerShopeeTxtHeader").text('CUSTOMER ID : ' + customerID.toUpperCase());
            //$("#itemShopeeModalBtn").click();
            //if ($("#withExcepTabShopee").hasClass("active")) {
            //    $("#shopeeMtpBtn").show()
            //    $("#shopeeMtpCloseBtn").hide()
            //} else {
            //    $("#shopeeMtpBtn").hide()
            //    $("#shopeeMtpCloseBtn").show()
            //}
            //$("#listOfShopeeItems > tbody").html(html)

            //JsBarcode(".barcode").init();
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

moveTopickerAlertShopee = function () {
    $("#moveToPickerModalBtnShopee").click()
    $("#moveToPickerModalDivShopee").html("Are you sure this order is clear?");
    $("#exampleModalLongTitlemoveToPickerShopee").html("Notification!")

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

moveTopickerShopee = function () {

    let orderId = $("#itemsOrderIdHeaderShopee").text()

    $.ajax({

        type: "POST",
        url: $("#divMoveToPickerShopee").data("request-url") + "?orderId=" + orderId,
        /*url: $("#divSaveTrans").data("request-url"),
        data: JSON.stringify(data),*/
        dataType: "json",
        contentType: "application/json;charset=utf-8",
        success: function (data) {
            $("#itemShopeeModal").modal("hide")
            $("#moveToPickerModalShopee").modal("hide")
            $("#successModalBtnShopee").click();
            $("#withExcepTabShopee").click()
            $("#successModalDivShopee").html("Order Successfully Cleared!");
        },
        error: function (request, status, error) {

            alert(error);
        }
    })


}


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

            // $("#successModalBtnShopee").click();
            // $("#successModalDivShopee").html("Shopee orders Sucessfully Updated!");
            Swal.fire(
                'Success!',
                'Shopee orders Sucessfully Updated!.',
                'success'
            )

            ajaxLoader('hide');

            var si = setInterval(function () {
                clearInterval(si);
                document.location = '/Shopee/ViewShopeeOrder'
            }, 2000);

        },
        error: function (request, status, error) {

            alert(error);
        }
    })

}


function clickPrintExcel(mod) {
    if (mod === 'print') {
        $('#ShopeeOrderTable .buttons-print').click();
    }
    else if (mod === 'excel') {
        $('#ShopeeOrderTable .buttons-excel').click();
    }
}