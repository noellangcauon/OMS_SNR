
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
        url: '/NOFItems/GetNOFItems',
    }).done(function (set) {
        tableGenerator('#nofItems', set);

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
                messageTop: "Not On Fulfillment",
                title: 'NOF Items',
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
                messageTop: "Not On Fulfillment",
                title: 'NOF Items',
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
                    }
                },
                { "data": "sku_id" },
                { "data": "item_description" },
                { "data": "quantity" },
                { "data": "onhand" }, 
                { "data": "dateProcess" },
                {
                    "data": "remarksDesc",
                    "render": function (data, type, row, meta) {
                        // Check if it's the display type, and return the formatted select box
                        if (type === 'display') {
                            var selectOptions = '<select class="form-select">';
                            for (var i = 0; i < datas.remarksData.length; i++) {

                                var selected = (row.remarks === datas.remarksData[i].code.toString()) ? 'selected' : '';
                                if (i == 0) {

                                    selectOptions += '<option value="0" ' + selected + '>Choose remarks</option>';
                                }
                                selectOptions += '<option value="' + datas.remarksData[i].code + '" ' + selected + '>' + datas.remarksData[i].description + '</option>';
                            }
                            selectOptions += '</select>';
                            return selectOptions;
                        }
                        // If it's not the display type, return the original data
                        return data;
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
function getCheckItems() {
    var items = [];
    var gridCount = $("#nofItems tr").length - 1

    for (i = 0; i < gridCount; i++) {
        if ($("#nofItems #nofItems-tbody tr:eq(" + i + ")").find("td:eq(0) input").prop("checked")) {
            var ItemsVal = {
                sku_id: $("#nofItems #nofItems-tbody tr:eq(" + i + ")").find("td:eq(1)").text(),
                Quantity: $("#nofItems #nofItems-tbody tr:eq(" + i + ")").find("td:eq(3)").text(),
                Remarks: $("#nofItems #nofItems-tbody tr:eq(" + i + ")").find("td:eq(6) select").val()

            }

            items.push(ItemsVal);
        }
    }

    clearItems(items);
}
function getCheckItemsUpdate() {
    var items = [];
    var gridCount = $("#nofItems tr").length - 1

    for (i = 0; i < gridCount; i++) {
        if ($("#nofItems #nofItems-tbody tr:eq(" + i + ")").find("td:eq(0) input").prop("checked")) {
            var ItemsVal = {
                sku_id: $("#nofItems #nofItems-tbody tr:eq(" + i + ")").find("td:eq(1)").text(),
                Quantity: $("#nofItems #nofItems-tbody tr:eq(" + i + ")").find("td:eq(3)").text(),
                Remarks: $("#nofItems #nofItems-tbody tr:eq(" + i + ")").find("td:eq(6) select").val()

            }

            items.push(ItemsVal);
        }
    }

    updateItems(items);
}

function clearItems(data) {

    $.ajax({
        method: "POST",
        url: '/NOFItems/ClearItems',
        data: { NOFClasses: data }
    }).done(function (set) {
        if (set.set.nofClasses.length > 0) {
            $("#closeUserModal").click();
            Swal.fire(
                'Succes!',
                'Successfully cleared the item(s)',
                'success'
            )

            var si = setInterval(function () {
                clearInterval(si);
                document.location = '/NOFItems/ViewNOFItems'
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
        url: '/NOFItems/UpdateItems',
        data: { NOFClasses: data }
    }).done(function (set) {
        if (set.set.nofClasses != null) {
            $("#closeUserModal").click();
            Swal.fire(
                'Succes!',
                'Successfully updated the item(s)',
                'success'
            )

            var si = setInterval(function () {
                clearInterval(si);
                document.location = '/NOFItems/ViewNOFItems'
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
        if (set.set.nofClasses != null) {
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

function clickPrintExcel(mod) {
    if (mod === 'print') {
        $('.buttons-print').click();
    }
    else if (mod === 'excel') {
        $('.buttons-excel').click();
    }
}