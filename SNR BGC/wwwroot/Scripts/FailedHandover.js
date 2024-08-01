

$(document).ready(function () {
    $('#loader').hide();
    getList();
});

function getList() {
    var dateFrom = $("#dateFrom").val()
    var dateTo = $("#dateTo").val()
    $.ajax({
        method: "GET",
        url: '/FailedHandover/GetFailedHandover?dateFrom=' + dateFrom + '&dateTo=' + dateTo,
    }).done(function (set) {
        tableGenerator('#failedHandoverList', set);

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
            buttons: [{
                extend: 'excelHtml5',
                text: '<i class= "fa fa-download"></i > Download',
                className: '',
                messageTop: "",
                title: 'Failed Handover List',
                exportOptions: {
                    orthogonal: "myExport",
                    columns: [0, 1],
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
                    "data": "boxerEndTime",
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
                }
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