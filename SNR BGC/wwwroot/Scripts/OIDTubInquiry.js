

$(document).ready(function () {
    getList();
});

function getList() {
    //var dateFrom = $("#dateFrom").val()
    var search = "";
    $.ajax({
        method: "GET",
        url: '/OIDTubInquiry/GetInquiries?search=' + search,
    }).done(function (set) {
        tableGenerator('#oidItems', set);

        $(".dt-button").addClass("btn btn-primary");
        $(".dt-button").removeClass("dt-button");
    });
}



function tableGenerator(table, data) {


    let dTable = $(table).DataTable();
    if (data.set.length > 0) {
        var counter = 0;
        dTable.destroy();

        $('#loader').show();

        dTable = $(table).DataTable({
            "scrollY": '50vh',
            "responsive": true,
            "lengthChange": false,
            "scrollCollapse": true,
            "paging": true,
            "searching": true,
            "data": data.set,
            dom: 'Bfrtip',
            "pageLength": 10,
            "order": [],
            'initComplete': function () {
                $('#loader').hide(); // Hide loader once DataTable is initialized
            },
            buttons: [],
            "columns": [
                
                { "data": "orderId" },
                { "data": "module" },
                {
                    "data": "item_count", "render": function (data, type, row) {
                        return data = '<div ><a style="cursor:pointer; text-decoration: underline; color: blue;" onclick="getOrderItems(this)" order_id="' + row.orderId + '">'+data+'</a></div>'
                    }
                },
                { "data": "total_amount" },
                { "data": "oidstatus" },
                {
                    "data": "dateFetch",
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
                    "data": "tub", "render": function (data) {
                        if (data === null) {
                            return '<div>-</div>';
                        } else {
                            return '<div ><a style="cursor:pointer; text-decoration: underline; color: blue;" onclick="getTubs(this)" tub="' + data + '">'+data+'</a></div>'
                        }
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

function tableGeneratorView(table, trans) {

    let dTable = $(table).DataTable();
    if (trans.set.length > 0) {
        var counter = 0;
        dTable.destroy();


        dTable = $(table).DataTable({
            "paging": true,
            "data": trans.set,
            dom: 'Bfrtip',
            buttons: [],
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
                { "data": "platform_status" },
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

getOrderItems = function (ths) {

    var itemsPriceTotal = 0;
    var order_id = $(ths).attr("order_id");
    let moduleModal = $("#itemModal");
    ajaxLoader('show');

    $.ajax({
        type: "POST",
        url: $("#divFetchItem").data("request-url") + "?order_id=" + order_id,
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
            $("#itemsTxtHeader").text('ORDER # :  ' + order_id)
            $("#itemsOrderIdHeader").text(order_id);
            moduleModal.modal("show");


            JsBarcode(".barcode").init();

        },
        error: function (request, status, error) {

            alert(error);
        }
    })



}


function tableGeneratorTubView(table, trans) {

    let dTable = $(table).DataTable();
    if (trans.set.length > 0) {
        var counter = 0;
        dTable.destroy();


        dTable = $(table).DataTable({
            "paging": true,
            "data": trans.set,
            dom: 'Bfrtip',
            buttons: [],
            "columns": [
                { "data": "orderId" },
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
                { "data": "pickerStatus" }
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

getTubs = function (ths) {

    var tub = $(ths).attr("tub");
    let moduleModal = $("#itemModalTub");
    ajaxLoader('show');

    $.ajax({
        type: "POST",
        url: $("#divFetchTub").data("request-url") + "?tub=" + tub,
        dataType: "json",
        contentType: "application/json;charset=utf-8",
        success: function (data) {

            ajaxLoader('hide');
            tableGeneratorTubView("#listOfTubs", data)
            $(".dt-button").addClass("btn btn-primary");
            $(".dt-button").removeClass("dt-button");
            moduleModal.modal("show");


            JsBarcode(".barcode").init();

        },
        error: function (request, status, error) {

            alert(error);
        }
    })



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