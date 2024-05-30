

$(document).ready(function () {
    getList();
});

function getList() {
    var type = $("#selectException").val()
    var dateFrom = $("#dateFrom").val()
    var dateTo = $("#dateTo").val()
    $.ajax({
        method: "GET",
        url: '/ExceptionReport/GetExceptionReportDiscrepancy?dateFrom=' + dateFrom + '&dateTo=' + dateTo,
    }).done(function (set) {
        tableGenerator('#exceptionItems', set);

        $(".dt-button").addClass("btn btn-primary");
        $(".dt-button").removeClass("dt-button");
    });
}



function tableGenerator(table, data) {


    let dTable = $(table).DataTable();
    if (data.set.length > 0) {
        var counter = 0;
        dTable.destroy();

        dTable = $(table).DataTable({
            "scrollY": '50vh',
            "responsive": true,
            "lengthChange": false,
            "scrollCollapse": true,
            "paging": true,
            "searching": true,
            "data": data.set,
            dom: 'Bfrtip',
            buttons: [{
                extend: 'print',
                text: '<i class="bi bi-printer-fill"></i> Print',
                className: '',
                messageTop: "Exception Items",
                title: 'Exception Report',
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
                    columns: [0, 1, 2, 3, 4, 5, 6, 7],
                },
                //TEMPshareholderName.length - 1 > 0 ? TEMPshareholderName.join(" OR ") : TEMPshareholderName
            }, {
                extend: 'excelHtml5',
                text: '<i class= "fa fa-download"></i > Download',
                className: '',
                messageTop: "Exception Items",
                title: 'Exception Report',
                exportOptions: {
                    orthogonal: "myExport",
                    columns: [0, 1, 2, 3, 4, 5, 6, 7],
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
                { "data": "module" },
                { "data": "tubNo" },
                { "data": "totalOrderItems" },
                { "data": "totalBoxOrderItems" },
                { "data": "boxerUser" },
                { "data": "pickerUser" },
                { "data": "status" },
                {
                    "data": "dateProcess",
                    "render": function (data) {
                        let d = new Date(data),
                            month = '' + (d.getMonth() + 1),
                            day = '' + d.getDate(),
                            year = d.getFullYear(),
                            hours = '' + d.getHours(),
                            minutes = '' + d.getMinutes();

                        if (month.length < 2)
                            month = '0' + month;
                        if (day.length < 2)
                            day = '0' + day;
                        if (hours.length < 2)
                            hours = '0' + hours;
                        if (minutes.length < 2)
                            minutes = '0' + minutes;
                        return data = [month, day, year].join('-') + ' ' + [hours, minutes].join(':');
                    }
                },
                {
                    "data": "referenceNo",
                    "render": function (data, type, row) {
                        return data = '<center ><i class="fa fa-eye" style="cursor:pointer" onclick="getDiscrepancyOrderItems(this)" order_id="' + row.orderId + '" customerID="' + row.customerID + '"></i></center>'
                    }
                }
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
function getDiscrepancyOrderItems(ths) {
    var itemsPriceTotal = 0;
    var order_id = $(ths).attr("order_id");
    var customerID = $(ths).attr("customerID");
    $('#itemModals').modal('show');
    ajaxLoader('show');

    $.ajax({
        method: "GET",
        url: '/ExceptionReport/GetExceptionReportDiscrepancyDetails?orderId=' + order_id,
    }).done(function (set) {
        tableGeneratorView('#tblViewDetails', set);
        for (i = 0; i < set.set.length; i++) {
            itemsPriceTotal = itemsPriceTotal + set.set[i].total_item_price;
        }
        //$("#txtTotalPrice").text(itemsPriceTotal);
        $(".dt-button").addClass("btn btn-primary");
        $(".dt-button").removeClass("dt-button");

        $("#itemsTxtHeader").text('ORDER # :  ' + order_id)
        $("#itemsOrderIdHeader").text(order_id);
        $("#customerTxtHeader").text('CUSTOMER ID : ' + customerID.toUpperCase());

        ajaxLoader('hide');
    });
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
                { "data": "skuId" },
                {
                    "data": "item_image",
                    "render": function (data) {

                        return '<div><img style="width: 80px;height: 80px;cursor:pointer;" id="previewImgShopee" href="' + data + '"  src="' + data + '" ></div>';
                    }
                },
                { "data": "item_description" },
                {
                    "data": "dateProcess",
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
function clickPrintExcel(mod) {
    if (mod === 'print') {
        $('.buttons-print').click();
    }
    else if (mod === 'excel') {
        $('.buttons-excel').click();
    }
}


$(document).on("change", "#dateFrom, #dateTo", function () {

    var dateFrom = new Date($("#dateFrom").val());
    var dateTo = new Date($("#dateTo").val());

    if (dateFrom > dateTo) {
        $("#dateFrom").val("");
        $("#dateTo").val("");
        Swal.fire({
            title: 'Oops!',
            text: 'Date From should not be later than Date To',
            icon: 'error',
            confirmButtonText: 'OK'
        })
    }




});