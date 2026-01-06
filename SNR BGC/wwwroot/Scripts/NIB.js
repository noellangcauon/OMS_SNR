
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
});

function getList() {
    $.ajax({
        method: "GET",
        url: '/NIBItems/GetNIBItems',
    }).done(function (set) {
        tableGenerator('#nibItems', set);

        $(".dt-button").addClass("btn btn-primary");
        $(".dt-button").removeClass("dt-button");
    });
}




function tableGenerator(table, datas) {


    let dTable = $(table).DataTable();
    if (datas.set.length > 0) {
        var counter = 0;
        dTable.destroy();

        dTable = $(table).DataTable({
            "scrollY": '50vh',
            "responsive": true,
            "lengthChange": false,
            "scrollCollapse": true,
            "paging": true,
            "searching": true,
            "data": datas.set,
            dom: 'Bfrtip',
            buttons: [{
                extend: 'print',
                text: '<i class="bi bi-printer-fill"></i> Print',
                className: '',
                messageTop: "",
                title: 'NIB Items',
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
                    columns: [ 1, 2, 3,4,5,6],
                },
                //TEMPshareholderName.length - 1 > 0 ? TEMPshareholderName.join(" OR ") : TEMPshareholderName
            }, {
                extend: 'excelHtml5',
                text: '<i class= "fa fa-download"></i > Download',
                className: '',
                messageTop: "",
                title: 'NIB Items',
                exportOptions: {
                    
                    orthogonal: "myExport",
                    columns: [ 1, 2, 3,4,5,6],
                },
                action: function (e, indicator, dt, node, config) {
                    console.log(indicator);
                    //iqwerty.toast.toast('Downloading report file .... Please wait', toastInfo);
                    if (indicator) {
                        $.fn.DataTable.ext.buttons.excelHtml5.action.call(this, e, indicator, dt, node, config);
                        //iqwerty.toast.toast('Report downloaded successfully', toastSuccess);
                    } else {
                        Swal.fire(
                            'Error!',
                            'Download error',
                            'error'
                        ) }

                    

                }
            }],
            "columns": [
                {
                    "data": "sku_id",
                    "render": function (data) {
                        data = "<input type='checkbox' />";
                        return data;
                    },
                    "visible": false
                },
                { "data": "sku_id" },
                { "data": "item_description" },
                { "data": "quantity" },
                { "data": "onhand" }, 
                { "data": "onhand_basement" }, 
                {
                    "data": "dateProcess",
                    "render": function (data) {
                        if (!data) return "";

                        const d = new Date(data);

                        let hours = d.getHours();
                        const minutes = String(d.getMinutes()).padStart(2, "0");
                        const seconds = String(d.getSeconds()).padStart(2, "0");

                        const ampm = hours >= 12 ? "PM" : "AM";
                        hours = hours % 12;
                        hours = hours ? hours : 12; // 0 becomes 12
                        hours = String(hours).padStart(2, "0");

                        const date =
                            d.getFullYear() + "-" +
                            String(d.getMonth() + 1).padStart(2, "0") + "-" +
                            String(d.getDate()).padStart(2, "0");

                        return `${date} ${hours}:${minutes}:${seconds} ${ampm}`;
                    } },
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
function getCheckItems() {
    var items = [];
    var gridCount = $("#nibItems tr").length - 1

    for (i = 0; i < gridCount; i++) {
        if ($("#nibItems #nibItems-tbody tr:eq(" + i + ")").find("td:eq(0) input").prop("checked")) {
            var ItemsVal = {
                sku_id: $("#nibItems #nibItems-tbody tr:eq(" + i + ")").find("td:eq(1)").text(),
                Quantity: $("#nibItems #nibItems-tbody tr:eq(" + i + ")").find("td:eq(3)").text(),
                Remarks: $("#nibItems #nibItems-tbody tr:eq(" + i + ")").find("td:eq(6) select").val()

            }

            items.push(ItemsVal);
        }
    }

    clearItems(items);
}
function getCheckItemsUpdate() {
    var items = [];
    var gridCount = $("#nibItems tr").length - 1

    for (i = 0; i < gridCount; i++) {
        if ($("#nibItems #nibItems-tbody tr:eq(" + i + ")").find("td:eq(0) input").prop("checked")) {
            var ItemsVal = {
                sku_id: $("#nibItems #nibItems-tbody tr:eq(" + i + ")").find("td:eq(1)").text(),
                Quantity: $("#nibItems #nibItems-tbody tr:eq(" + i + ")").find("td:eq(3)").text(),
                Remarks: $("#nibItems #nibItems-tbody tr:eq(" + i + ")").find("td:eq(6) select").val()

            }

            items.push(ItemsVal);
        }
    }

    updateItems(items);
}

function clearItems(data) {

    $.ajax({
        method: "POST",
        url: '/NIBItems/ClearItems',
        data: { NIBClasses: data }
    }).done(function (set) {
        if (set.set.nibClasses.length > 0) {
            $("#closeUserModal").click();
            Swal.fire(
                'Succes!',
                'Successfully cleared the item(s)',
                'success'
            )

            var si = setInterval(function () {
                clearInterval(si);
                document.location = '/NIBItems/ViewNIBItems'
            }, 4000);
        }
        else {

            Swal.fire(
                'Failed!',
                set.set,
                'error'
            )

            //window.location.href = '/User/CreateUser';
        }
    });


}

function updateItems(data) {

    $.ajax({
        method: "POST",
        url: '/NIBItems/UpdateItems',
        data: { NIBClasses: data }
    }).done(function (set) {
        if (set.set.nibClasses != null) {
            $("#closeUserModal").click();
            Swal.fire(
                'Succes!',
                'Successfully updated the item(s)',
                'success'
            )

            var si = setInterval(function () {
                clearInterval(si);
                document.location = '/NIBItems/ViewNIBItems'
            }, 4000);
        }
        else {

            Swal.fire(
                'No items updated!',
                set.set,
                'error'
            )

            //window.location.href = '/User/CreateUser';
        }
    });


}
function getDataEdit(data) {

    $.ajax({
        method: "POST",
        url: '/User/EditUser',
        data: { userform: data }
    }).done(function (set) {
        if (set.set.nibClasses != null) {
            $("#closeUserModal").click();
            Swal.fire(
                'Succes!',
                'Successfully added the user',
                'success'
            )

            var si = setInterval(function () {
                clearInterval(si);
                document.location = '/User/CreateUser'
            }, 4000);
        }
        else {

            Swal.fire(
                'No items cleared!',
                set.set,
                'error'
            )

            //window.location.href = '/User/CreateUser';
        }
    });


}
$(document).on("click", "#chkAll", function () {
    var tickAll = $(this).prop("checked")
    var gridCount = $("#nibItems tr").length - 1



    if (tickAll) {
        for (i = 0; i < gridCount; i++) {
            $("#nibItems #nibItems-tbody tr:eq(" + i + ")").find("td:eq(0) input").prop("checked", true);
        }
    }
    else {
        for (i = 0; i < gridCount; i++) {
            $("#nibItems #nibItems-tbody tr:eq(" + i + ")").find("td:eq(0) input").prop("checked", false);
        }
    }
});

function clickPrintExcel(mod) {
    if (mod === 'print') {
        $('.buttons-print').click();
    }
    else if (mod === 'excel') {
        $('.buttons-excel').click();
    }
}