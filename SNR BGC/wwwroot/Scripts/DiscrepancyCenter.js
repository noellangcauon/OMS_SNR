
var isBtnSaveEnabled = false;
var idToDeact = "";
var idToActivate = "";
var idToEdit = "";
var dataUser = [];
var isPasswordChange = false;

$(document).ready(function () {
    $("#btnSave").prop("disabled", true);
    $("#passwordField").hide();
    getList();
    getDiscrepancyNotification();
});

function getList() {
    $.ajax({
        method: "GET",
        url: '/DiscrepancyCenter/GetDiscrepancyOrders',
    }).done(function (set) {
        tableGenerator('#tbl_discrepancy', set, 'bad');
    });
}

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




function tableGenerator(table, data, flag) {


    let dTable = $(table).DataTable();
    if (data.set.length > 0) {
        var counter = 0;
        dTable.destroy();

        dTable = $(table).DataTable({
            "scrollY": '50vh',
            "aaSorting": [],
            "responsive": true,
            "lengthChange": false,
            "scrollCollapse": true,
            "paging": true,
            "searching": true,
            "data": data.set,
            "columns": [

                { "data": "orderId" },
                { "data": "moduleName" },
                { "data": "dateFetch" },
                { "data": "boxQRCode" },
                { "data": "clearedTotalItems" },
                { "data": "boxTotalItems" }, {
                    "data": "orderId",
                    "render": function (data, type, row) {
                        return data = '<div ><i class="fa fa-eye" style="cursor:pointer" onclick="openModal(this)" tubNo="' + row.boxQRCode +'" boxItems="' + row.boxTotalItems + '" clearedItems="' + row.clearedTotalItems + '"  order_id="' + row.orderId + '" flag="' + flag + '" customerID="' + row.customerID + '"></i></div>'
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
            "searching": true,
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

openModal = function (ths) {
    itemsPriceTotal = 0;
    var tub_no = $(ths).attr("tubNo");
    var order_id = $(ths).attr("order_id");
    var customerID = $(ths).attr("customerID");
    let moduleModal = $("#itemShopeeModal");
    $('#txtOrderId').val(order_id)
    var box_items = $(ths).attr("boxItems");
    var cleared_items = $(ths).attr("clearedItems");
    $('#txtReferenceNo').val(tub_no)
    document.getElementById('clearedOrdersCount').innerHTML = "Number of Items : " + cleared_items;
    document.getElementById('boxOrdersCount').innerHTML = "Number of Items : " + box_items;
    /* $('#clearedOrdersCount').val = cleared_items*/

    $.ajax({
        method: "GET",
        url: '/DiscrepancyCenter/GetBoxOrderItem?order_id=' + order_id,
    }).done(function (data) {
        /*tableGenerator('#tbl_discrepancy', set, 'bad');*/
        console.log('data', data)
        let dTable = $('#listOfShopeeItems').DataTable();
        if (data.set.length > 0) {
            var counter = 0;
            dTable.destroy();

            dTable = $('#listOfShopeeItems').DataTable({
                "scrollY": '50vh',
                "responsive": true,
                "lengthChange": false,
                "scrollCollapse": true,
                "paging": true,
                "searching": false,
                "data": data.set,
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
                        "data": "item_count",
                        //"render": function (data, type) {
                        //    let d = new Date(data),
                        //        month = '' + (d.getMonth() + 1),
                        //        day = '' + d.getDate(),
                        //        year = d.getFullYear();

                        //    if (month.length < 2)
                        //        month = '0' + month;
                        //    if (day.length < 2)
                        //        day = '0' + day;
                        //    return data = [month, day, year].join('-');
                        //}
                    },

                    { "data": "upc" },
                    {
                        "data": "item_price",

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


    });



    $.ajax({
        method: "GET",
        url: '/DiscrepancyCenter/GetOrdersByOrderId?order_id=' + order_id,
    }).done(function (data) {
        /*tableGenerator('#tbl_discrepancy', set, 'bad');*/

        let dTable = $('#listOfClearedOrders').DataTable();
        if (data.set.length > 0) {
            var counter = 0;
            dTable.destroy();

            dTable = $('#listOfClearedOrders').DataTable({
                "scrollY": '50vh',
                "responsive": true,
                "lengthChange": false,
                "scrollCollapse": true,
                "paging": true,
                "searching": false,
                "data": data.set,
                "columns": [
                    { "data": "sku_id" },
                    {
                        "data": "item_image",
                        "render": function (data) {

                            return '<div><img style="width: 80px;height: 80px;cursor:pointer;" id="previewImgShopee" href="' + data + '"  src="' + data + '" ></div>';
                        }
                    },
                    { "data": "item_description" },
                    //{
                    //    "data": "dateProcess",
                    //    "render": function (data, type) {
                    //        let d = new Date(data),
                    //            month = '' + (d.getMonth() + 1),
                    //            day = '' + d.getDate(),
                    //            year = d.getFullYear();

                    //        if (month.length < 2)
                    //            month = '0' + month;
                    //        if (day.length < 2)
                    //            day = '0' + day;
                    //        return data = [month, day, year].join('-');
                    //    }
                    //},
                    { "data": "item_count" },
                    { "data": "upc" },
                    {
                        "data": "item_price",

                    },{
                        "data": "platform_status",
                        "render": function (data) {

                            return '<label class="' + (data == "Yes" ? "text-danger" : "text-success") + '">' + data + '</label>';
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


    });



    moduleModal.modal("show");
}


function removeOrderItem(orderId) {

    let referenceNo = $('#txtReferenceNo').val();
    let barcodeModal = $("#enterBarcodeModal");
    let moduleModal = $("#itemShopeeModal");
    let tbl = $('#tbl_discrepancy')

    Swal.fire({
        title: 'Are you sure?',
        text: "You won't be able to revert this!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, clear it!'
    }).then((result) => {
        if (result.isConfirmed) {

            $.ajax({
                type: "POST",
                url: '/DiscrepancyCenter/SaveDiscrepancy?order_id=' + orderId + '&referenceNo=' + referenceNo,

                dataType: "json",
                contentType: "application/json;charset=utf-8",
                success: function (data) {
                    console.log('sample data', data)
                    if (data == 'ExistingTub') {
                        Swal.fire(
                            'Invalid!',
                            'TUB No. is in use!',
                            'error'
                        )
                    }
                    else {
                        getList()
                        ajaxLoader('hide');

                        Swal.fire(
                            'Cleared!',
                            'Orders has been Cleared.',
                            'success'
                        )
                        barcodeModal.modal('hide')
                        moduleModal.modal('hide')
                    }
                    
                   




                },
                error: function (request, status, error) {

                    alert(error);
                }
            });


        }
    })

}

$('#submitClearBtn').on('click', function () {
    let orderId = $('#txtOrderId').val();
    
    removeOrderItem(orderId)

})


$('#clearItemBtn').on('click', function () {
    let moduleModal = $("#enterBarcodeModal");
    let orderId = $('#txtOrderId').val();
  
    
    moduleModal.modal("show");

})

$('#closeBarcodeModalBtn').on('click', function () {
    let moduleModal = $("#enterBarcodeModal");


    moduleModal.modal("hide");

})





$(document).on("click", "#chkAll", function () {
    var tickAll = $(this).prop("checked")
    var gridCount = $("#nofItems tr").length - 1



    if (tickAll) {
        for (i = 0; i < gridCount; i++) {
            $("#nofItems #nofItems-tbody tr:eq(" + i + ")").find("td:eq(0) input").prop("checked", true);
        }
    }
    else {
        for (i = 0; i < gridCount; i++) {
            $("#nofItems #nofItems-tbody tr:eq(" + i + ")").find("td:eq(0) input").prop("checked", false);
        }
    }
});