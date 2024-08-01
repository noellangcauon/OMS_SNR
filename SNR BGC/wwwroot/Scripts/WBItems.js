

$(document).ready(function () {
    $('#loader').hide();
    getList();
});

function getList() {
    $.ajax({
        method: "GET",
        url: '/Boxer/GetWBItems',
    }).done(function (set) {
        tableGenerator('#wbList', set);

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
            "columnDefs": [
                {
                    className: 'dtr-control',
                    orderable: false,
                    targets: 0
                }
            ],
            "order": [1, 'asc'],
            "responsive": {
                details: {
                    type: 'column'
                }
            },
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
                
                { "data": "trackingNo" },
                { "data": "orderId" },
                { "data": "skuId" },
                { "data": "upc" },
                { "data": "item_description" }
            ],
            "rowCallback": function (row, data, index) {
                if (data.IsTooLong == "1") {
                    $(row).css('color', 'red');
                }
            }
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