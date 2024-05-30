function generateCodes() {

    $.ajax({
        method: "GET",
        url: '/Shopee/GetAuthCode',
    }).done(function (set) {
        window.open(set.responseText);
    });
}

function saveAuth() {
    const url = new URL($("#txtAuth").val());
    var params = new URLSearchParams(url.search);
    var code = params.get('code');


    $.ajax({
        method: "POST",
        url: '/Shopee/SaveAuth',
        data: { code: code }
    }).done(function (set) {
        tableGenerator('#authTable',set.set);
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
            "searching": false,
            "data": data.set,
            "columns": [
                { "data": "ids" },
                { "data": "authCode" },
                { "data": "dateEntry" },
                { "data": "module" }          
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
