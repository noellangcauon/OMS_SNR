

$(document).ready(function () {
    getList();
});

function getList() {
    $.ajax({
        method: "GET",
        url: '/Runner/GetRunnerStatus',
    }).done(function (set) {
        tableGenerator('#runnerList', set);

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
                { "data": "sku_id" },
                { "data": "item_description" },
                {
                    "data": "collectingStartTime",
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
                { "data": "runnerUser" },
                { "data": "runnerStatus" },
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