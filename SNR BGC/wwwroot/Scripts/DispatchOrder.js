$(document).ready(function () {
    var now = new Date();
    var day = ("0" + now.getDate()).slice(-2);
    var month = ("0" + (now.getMonth() + 1)).slice(-2);

    var today = now.getFullYear() + "-" + (month) + "-" + (day);
    document.getElementById('txtTrackingNo').focus();
    $('#dateCreated').val(today);


    $(".flatpickr").flatpickr({

        maxDate: new Date().fp_incr(1095),
        //static: true
    });

    var courierType, $courierType;

    $courierType = $("[name='DispatchOrders.CourierTypeId']").selectize({
        valueField: 'id',
        labelField: 'description',
        searchField: 'description',
        selectOnTab: true,
        preload: true,
        load: function (query, callback) {
            $.ajax({
                url: "/DispatchOrder/GetCourierTypes",
                success: function (results) {
                    try {

                        callback(results);


                    } catch (e) {
                        callback();
                    }
                },
                error: function () {
                    callback();
                }
            });
        }
    });

    courierType = $courierType[0].selectize;

    var fleetType, $fleetType;
    $fleetType = $("[name='DispatchOrders.FleetTypeId']").selectize({
        valueField: 'id',
        labelField: 'description',
        searchField: 'description',
        selectOnTab: true,
        preload: true,
        load: function (query, callback) {
            $.ajax({
                url: "/DispatchOrder/GetFleetTypes",
                success: function (results) {
                    try {

                        callback(results);
                    } catch (e) {
                        callback();
                    }
                },
                error: function () {
                    callback();
                }
            });
        }
    });

    fleetType = $fleetType[0].selectize;


    Swal.fire({
        title: 'Scan',
        text: "Start Scanning",
        icon: 'question',
        showCancelButton: false,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, start scanning!'
    }).then((result) => {
        if (result.isConfirmed) {
            document.getElementById('txtTrackingNo').focus();




        }
    })

    $("#btn_save").click(function () {
        $("#frm_dispatch").submit(); // Submit the form
    });

});
//$(function () {
  
    
//});

function addDispatchOrder(items, trackingNo) {
    let count = $(".item_row").length;
    

    let rowToAdd = "";
    let statusToAdd = "";

    var now = new Date();
    var time = now.getHours() + ":" + now.getMinutes() + ":" + now.getSeconds();
    var day = ("0" + now.getDate()).slice(-2);
    var month = ("0" + (now.getMonth() + 1)).slice(-2);

    var today = now.getFullYear() + "-" + (month) + "-" + (day) + " " + time;

    if (items.status == "Dispatched") {
        statusToAdd = ` <td style="width: 300px white-space: nowrap; text-align: center">
                            <input type="hidden" class="form-control" id="DispatchOrderDetails_Status_[${count}]" name="DispatchOrderDetails[${count}].Status" data-val="true" value="${items.status}"  />
                            <span class="badge bg-success">${items.status}</span>
                        </td>`
    }
    else if (items.status == "Double") {
        statusToAdd = ` <td style="width: 300px white-space: nowrap; text-align: center">
                            <input type="hidden" class="form-control"disabled id="DispatchOrderDetails_Status_[${count}]" name="DispatchOrderDetails[${count}].Status" data-val="true" value="${items.status}"  />
                            <span class="badge bg-warning">${items.status}</span>
                        </td>`
    }
    else if (items.status == "Cancelled") {
        statusToAdd = ` <td style="width: 300px white-space: nowrap; text-align: center">
                            <input type="hidden" class="form-control"  id="DispatchOrderDetails_Status_[${count}]" name="DispatchOrderDetails[${count}].Status" data-val="true" value="${items.status}"  />
                            <span class="badge bg-secondary">${items.status}</span>
                        </td>`
    }
    else {
        statusToAdd = ` <td style="width: 300px white-space: nowrap; text-align: center">
                            <input type="hidden" class="form-control" id="DispatchOrderDetails_Status_[${count}]" name="DispatchOrderDetails[${count}].Status" data-val="true" value="${items.status}"  />
                            <span class="badge bg-danger">${items.status}</span>
                        </td>`
    }

    rowToAdd = `<tr class="item_row" id="${count}">
                        <td style="width: 300px">

                            <input type="hidden" id="DispatchOrderDetails_Id_[${count}]" name="DispatchOrderDetails[${count}].Id" data-val="true" value="${0}" />
                            <input type="hidden" id="DispatchOrderDetails_DispatchOrderId_[${count}]" name="DispatchOrderDetails[${count}].DispatchOrderId" data-val="true" value="${0}" />

                            <input class="form-control" id="DispatchOrderDetails_TrackingNo_[${count}]" name="DispatchOrderDetails[${count}].TrackingNo" data-val="true" value="${trackingNo}" />
                        </td>
                        <td style="width: 300px white-space: nowrap; text-align: center"">
                            <input type="hidden" class="form-control" id="DispatchOrderDetails_OrderId_[${count}]" name="DispatchOrderDetails[${count}].OrderId" data-val="true" value="${items.orderId}" />
                           ${items.orderId}
                        </td>
                        <td style="width: 300px white-space: nowrap; text-align: center"">
                            <input type="hidden" class="form-control" id="DispatchOrderDetails_PlatForm_[${count}]" name="DispatchOrderDetails[${count}].PlatForm" data-val="true" value="${items.shopName}"  />
                            ${items.shopName}
                        </td>
                        <td style="width: 300px white-space: nowrap; text-align: center"">
                            <input type="hidden" class="form-control" id="DispatchOrderDetails_DateScanned_[${count}]" name="DispatchOrderDetails[${count}].DateScanned" data-val="true" value="${today}"  />
                            ${today}
                        </td>
                        `+ statusToAdd + `
                        <td style="width: 300px white-space: nowrap; text-align: center"">
                            <input  class="form-control" id="DispatchOrderDetails_Remarks_[${count}]" name="DispatchOrderDetails[${count}].Remarks" data-val="true"  />

                        </td>
                        <td style="width: 100px;white-space: nowrap; text-align: center">
                            <div>
                                <button class="btn btn-danger waves-effect deleteRowBtn" type="button" ><i class="fas fa-times-circle"></i> Remove</button>

                            </div>

                        </td>
                    </tr>`;
    $("#tbl_dispatchOrder tbody").append(rowToAdd);
}


function addRow() {
    let count = $(".item_row").length;


    let rowToAdd = "";
    let statusToAdd = "";

    var now = new Date();
    var time = now.getHours() + ":" + now.getMinutes() + ":" + now.getSeconds();
    var day = ("0" + now.getDate()).slice(-2);
    var month = ("0" + (now.getMonth() + 1)).slice(-2);

    var today = now.getFullYear() + "-" + (month) + "-" + (day) + " " + time;



    rowToAdd = `<tr class="item_row" id="${count}">
                        <td style="width: 300px">

                            <input type="hidden" id="DispatchOrderDetails_Id_[${count}]" name="DispatchOrderDetails[${count}].Id" data-val="true" value="${0}" />
                            <input type="hidden" id="DispatchOrderDetails_DispatchOrderId_[${count}]" name="DispatchOrderDetails[${count}].DispatchOrderId" data-val="true" value="${0}" />

                            <input class="form-control"  id="DispatchOrderDetails_TrackingNo_[${count}]" name="DispatchOrderDetails[${count}].TrackingNo" data-val="true" onchange="onScan(${count})"  />
                        </td>
                        <td style="width: 300px white-space: nowrap; text-align: center"">
                            <input type="hidden" class="form-control" id="DispatchOrderDetails_OrderId_[${count}]" name="DispatchOrderDetails[${count}].OrderId" data-val="true"  />
                           
                        </td>
                        <td style="width: 300px white-space: nowrap; text-align: center"">
                            <input type="hidden" class="form-control" id="DispatchOrderDetails_PlatForm_[${count}]" name="DispatchOrderDetails[${count}].PlatForm" data-val="true"   />
                           
                        </td>
                        <td style="width: 300px white-space: nowrap; text-align: center"">
                            <input type="hidden" class="form-control" id="DispatchOrderDetails_DateScanned_[${count}]" name="DispatchOrderDetails[${count}].DateScanned" data-val="true"   />
                            ${today}
                        </td>
                        <td style="width: 300px white-space: nowrap; text-align: center">
                            <input type="hidden" class="form-control"disabled id="DispatchOrderDetails_Status_[${count}]" name="DispatchOrderDetails[${count}].Status" data-val="true"   />
                            <span class="badge bg-warning"></span>
                        </td>
                        <td style="width: 300px white-space: nowrap; text-align: center"">
                            <input  class="form-control" id="DispatchOrderDetails_Remarks_[${count}]" name="DispatchOrderDetails[${count}].Remarks" data-val="true"  />

                        </td>
                        <td style="width: 100px;white-space: nowrap; text-align: center">
                            <div>
                                <button class="btn btn-danger waves-effect deleteRowBtn" type="button" ><i class="fas fa-times-circle"></i> Remove</button>

                            </div>

                        </td>
                    </tr>`;
    $("#tbl_dispatchOrder tbody").append(rowToAdd);

    document.getElementById("DispatchOrderDetails_TrackingNo_[" + count + "]").focus();
}

function getBoxOrdersByTrackingNo(trackingNo) {


    $.ajax({
        url: '/DispatchOrder/GetBoxOrdersByTrackingNo?trackingNo=' + trackingNo,
        type: 'GET',
        dataType: 'json',
        success: function (data) {
            
            
            addDispatchOrder(data, trackingNo)

        }
    });
}

$('#btn_startScan').on('click', function () {
    document.getElementById('txtTrackingNo').focus();

})



function deleteRow(index) {
    var tid = "";
    $('#tbl_dispatchOrder tr').click(function (event) {
        tid = $(this).attr('id');
    });
    $("#deleteRowBtn").click(function () {
        console.log(tid);
        if ($('#' + tid).length) {
            $('#' + tid).remove();
        }
    });
   // $("#tbl_dispatchOrder tr:eq(" + (index + 1) + ")").remove();
}

$("#tbl_dispatchOrder").on('click', '.deleteRowBtn', function () {
    $(this).closest('tr').remove();
   
});

$('#frm_dispatch').submit(function (e) {
    e.preventDefault();
    let fleetType = $('#fleetTypeId').val();
    let courierType = $('#courierTypeId').val();
    let plateNo = $('#plateNo').val();
    let errors = [];

    if (!fleetType) {
        errors.push({
           
            message: 'This is required!'
        });
    }
    if (!fleetType) {
        errors.push({
            
            message: 'This is required!'
        });
    }
    if (!fleetType) {
        errors.push({
          
            message: 'This is required!'
        });
    }
    if (errors.length) {
        Swal.fire(
            'Invalid!',
            'Please fill up all fields!',
            'error'
        )
    }
    else {
        let formData = new FormData(e.target);
        const btn = $('#btn_save');

        $.ajax({
            url: $(this).attr("action"),
            method: $(this).attr("method"),
            data: formData,
            cache: false,
            contentType: false,
            processData: false,
            beforeSend: function () {
                btn.attr({ disabled: true }).html("<span class='spinner-border spinner-border-sm'></span> Saving...");
            },
            success: function (response) {
                var message_context = "";
                Swal.fire(
                    'Success!',
                    'Orders has been Dispatched.',
                    'success'
                )
                btn.attr({ disabled: false }).html("<i class='fe-save'></i> Save Changes");
                setTimeout(function () {
                    window.location = "/DispatchOrder/Index";
                }, 2500);
            },
            error: function (response) {
                messageBox(response, "error");
            }
        });
    }



});

$('#btn_scan').on('click', function () {
    let moduleModal = $("#scanBarcode");
    moduleModal.modal("show");

    const scanner = new Html5QrcodeScanner('reader', {
        qrbox: {
            width: 250,
            height: 250,
        },
        fps: 20,
    });

    scanner.render(success, error);
    function success(result) {
        console.log(result)

        scanner.clear();
        getBoxOrdersByTrackingNo(result)
        moduleModal.modal("hide");
        console.log(result)
        
    }
    function error(err) {
        //console.error(err);
    }

});

function onScan() {
    var trackingNo = $('#txtTrackingNo').val();
    getBoxOrdersByTrackingNo(trackingNo);
    $('#txtTrackingNo').val('');

}

//function onSubmit() {
//    let $form = $(frm_dispatch);
//    $form.unbind();
//    $form.data("validator", null);

//    $form.submit(function (e) {
//        e.preventDefault();
       
//        let button = $("#btn_save");

//        $.ajax({
//            url: $(this).attr("action"),
//            method: $(this).attr("method"),
//            data: formData,
//            cache: false,
//            contentType: false,
//            processData: false,
//            beforeSend: function () {
//                button.html("<span class='spinner-border spinner-border-sm'></span> Saving...");
//                button.attr({ disabled: true });
//            },
//            success: function (response) {
               
//            },
//            error: function (response) {
               
//                button.html("<span class='fa fa-save'></span> Save");
//                button.attr({ disabled: false });
//            }
//        });
//    });

//}