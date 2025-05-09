
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
    $('#loader').show();
    $('#usersTable').DataTable().clear().destroy();
    $.ajax({
        method: "GET",
        url: '/User/GetUsers',
    }).done(function (set) {
        tableGenerator('#usersTable', set);
        $('#loader').hide();
    });
}
function getListInactive() {
    $('#loader').show();
    $('#usersTable').DataTable().clear().destroy();
    $.ajax({
        method: "GET",
        url: '/User/GetUsersInactive',
    }).done(function (set) {
        tableGenerator('#usersTable', set);
        $('#loader').hide();
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
            "columns": [
                { "data": "userId" },
                { "data": "username" },
                { "data": "employeeId" },
                { "data": "userFullname" },
                { "data": "userRole" },
                {
                    "data": "accessType",
                    "render": function (data, type, row) {
                        data = data == 'AD' ? 'Active Directory' : 'Native';
                        return data;
                    }
                    },
                {
                    "data": "OmsSubModule",
                    "render": function (data, type, row) {
                        var module = "";
                        if (row.withOmsAccess) {
                            module += 'OMS'
                        }
                        if (row.withRunnerAccess) {
                            module += module == "" ? 'Runner App' : ', Runner App';
                        } 
                        if (row.withPickerAccess) {
                            module += module == "" ? 'Picker App' : ', Picker App';
                        }
                        if (row.withBoxerAccess) {
                            module += module == "" ? 'Packer App' : ', Packer App';
                        }

                        return module;
                    }
                }, 
                {
                    "data": "userStatus",
                    "render": function (data, type, row) {
                        return data = buttonGeneratorDraft(row.userId, row.employeeId)
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


function buttonGeneratorDraft(id, empId) {
    return '<div class="btn-group btn-sm" role="group">' +
        ($("#btnActive").hasClass("active") ?'<button type="button" class="btn btn-warning btn-sm" onclick="redirectToEdit(' + id + ')"><i class="bi bi-pencil"></i>&nbsp;Edit</button>':'' )+
        ($("#btnActive").hasClass("active") ?
        '<button type="button" class="btn btn-danger btn-sm" data-toggle="modal" data-target="#deactivateModal" onclick="redirectToDeactivate(' + id + ')"><i class="bi bi-slash-circle"></i>&nbsp;Deactivate</button>'
        :
        empId == null ?
            '<button type="button" class="btn btn-success btn-sm" data-toggle="modal" data-target="#ActivateModal_EditEmployeeId" onclick="redirectToActivate(' + id + ')"><i class="bi bi-check2-circle"></i>&nbsp;Activate</button>'
            :
            '<button type="button" class="btn btn-success btn-sm" data-toggle="modal" data-target="#ActivateModal" onclick="redirectToActivate(' + id + ')"><i class="bi bi-check2-circle"></i>&nbsp;Activate</button>') +
        '</div>';

}
function redirectToDeactivate(id) {
    idToDeact = id;
}
function cancel() {
    idToDeact = "";
}
function redirectToActivate(id) {
    idToActivate = id;
}
function cancelActivate() {
    idToActivate = "";
}
function EditUserToActivate() {
    redirectToEdit(idToActivate);
}
function deactivateUser() {
    if (idToDeact != "") {
        $.ajax({
            method: "POST",
            url: '/User/DeactivateUser/' + idToDeact,
        }).done(function (set) {
            Swal.fire(
                'Succes!',
                'Successfully deactivated the user',
                'success'
            )
            tableGenerator('#usersTable', set);
            $("#btnInactive").click();
        });
    }
}
function ActivateUser() {
    if (idToActivate != "") {
        $.ajax({
            method: "POST",
            url: '/User/ActivateUser/' + idToActivate,
        }).done(function (set) {
            Swal.fire(
                'Succes!',
                'Successfully activated the user',
                'success'
            )
            tableGenerator('#usersTable', set);
            $("#btnActive").click();
        });
    }
}
function redirectToEdit(id) {
    idToEdit = id;

    $("#btnUpdate").prop("disabled", false); 

    if (id != "") {
        $.ajax({
            method: "POST",
            url: '/User/EditForm/' + id,
        }).done(function (set) {
            $("#EditUserModalBtn").click();
            EditForm(set);
        });
    }
}

function EditForm(set) {

    set.set.accessType == "AD" ? $("#radioADEdit").prop("checked", true) : $("#radioNativeEdit").prop("checked", true)
    $("#txtFullnameEdit").val(set.set.userFullname);
    $("#txtEmployeeIdEdit").val(set.set.employeeId);
    $("#txtUsernameEdit").val(set.set.username.replace("@snrshopping.com", ""));
    if (set.set.password == null) {
        $("#passwordFieldEdit").hide();
    }
    else {
        $("#passwordFieldEdit").show();
        $("#txtPasswordEdit").val(set.set.password);
        $("#txtPasswordEdit").prop("disabled", true);
        $("#resetPassword").removeClass("d-none");
        $("#showPasswordEdit").addClass("d-none");
    }
    $("#txtRoleEdit").val(set.set.userRole);
    $("#chkOmsEdit").prop("checked",set.set.withOmsAccess);
    $("#chkRunnerEdit").prop("checked", set.set.withRunnerAccess);
    $("#chkPickerEdit").prop("checked", set.set.withPickerAccess);
    $("#chkBoxerEdit").prop("checked", set.set.withBoxerAccess); 



}

$(document).on("click", "#resetPassword", function () {
    $("#txtPasswordEdit").val("");
    $("#txtPasswordEdit").prop("disabled", false);
    $("#resetPassword").addClass("d-none");
    $("#showPasswordEdit").removeClass("d-none");
    isPasswordChange = true;

});

$(document).on('mousedown', '#showPassword', function () {
    $('#txtPassword').attr('type', 'text')
});
$(document).on('mouseup', '#showPassword', function () {
    $('#txtPassword').attr('type', 'password')
});
$(document).on('mousedown', '#showPasswordEdit', function () {
    $('#txtPasswordEdit').attr('type', 'text')
});
$(document).on('mouseup', '#showPasswordEdit', function () {
    $('#txtPasswordEdit').attr('type', 'password')
});


$(document).on('click', '#radioAD', function () {
    $("#passwordField").hide();
});

$(document).on('click', '#radioNative', function () {
    $("#passwordField").show();
});

//$(document).on('click', '#createNewUser', function () {
//    var alertModal = new bootstrap.Modal(document.getElementById('CreateUserModal'), {
//        backdrop: false,
//        keyboard: false,
//    });
//    alertModal.show();
//    var onClosseModal = document.getElementById('CreateUserModal')
//    onClosseModal.addEventListener('hidden.bs.modal', function (event) {
//        currentModalTable = null;
//        currentRow = null;
//        alertModal.hide();
//        $('body').removeAttr('style');
//    });

//});
$(document).on("click", "#radioAD, #radioNative", function () {
    var isNative = $("#radioNative").prop("checked");
    if (isNative) {
        $("#txtPassword").attr("required",true);
    }
    else {

        $("#txtPassword").removeAttr("required", false);
    }

    var username = $("#txtUsername").val();
    var fullname = $("#txtFullname").val();
    var employeeId = $("#txtEmployeeId").val();
    var password = $("#txtPassword").val();
    var role = $("#txtRole").val();
    if (isNative) {
        if (username != "" && fullname != "" && employeeId != "" && password != "" && role != "") {

            if (password.match(/[A-Z]/) && password.match(/[a-z]/) && password.match(/[0-9]/) && password.match(/[\\@#$-/:-?{-~!"^_`\[\]]/) && password.length >= 8) {

                $("#btnSave").prop("disabled", false);
            }
            else {

                $("#btnSave").prop("disabled", true);
            }
        }
        else {

            $("#btnSave").prop("disabled", true);
        }
    }
    else {
        if (username != "" && fullname != "" && employeeId != "" && role != "") {

            $("#btnSave").prop("disabled", false);
        }
        else {

            $("#btnSave").prop("disabled", true);
        }
    }
});

$(document).on("keyup", "#txtUsername, #txtFullname, #txtEmployeeId, #txtPassword", function () {
    var isNative = $("#radioNative").prop("checked");
    var username = $("#txtUsername").val();
    var fullname = $("#txtFullname").val();
    var employeeId = $("#txtEmployeeId").val();
    var password = $("#txtPassword").val();
    var role = $("#txtRole").val();


    if (isNative) {
        if (username != "" && fullname != "" && employeeId != "" && password != "" && role != "") {
            
            if (password.match(/[A-Z]/) && password.match(/[a-z]/) && password.match(/[0-9]/) && password.match(/[\\@#$-/:-?{-~!"^_`\[\]]/) && password.length >= 8) {

                $("#btnSave").prop("disabled", false);
            }
            else {

                $("#btnSave").prop("disabled", true);
            }
        }
        else {

            $("#btnSave").prop("disabled", true);
        }
    }
    else {
        if (username != "" && fullname != "" && employeeId != ""  && role != "") {

            $("#btnSave").prop("disabled", false);
        }
        else {

            $("#btnSave").prop("disabled", true);
        }
    }

});
$(document).on("keyup", "#txtPassword", function () {

    var password = $("#txtPassword").val();
    if (password != "") {
        if (password.match(/[A-Z]/)) {
            $("#lblUppercase").addClass("d-none");
        }
        else {

            $("#lblUppercase").removeClass("d-none");
        }
        if (password.match(/[a-z]/)) {
            $("#lblLowercase").addClass("d-none");
        }
        else {

            $("#lblLowercase").removeClass("d-none");
        }
        if (password.match(/[\\@#$-/:-?{-~!"^_`\[\]]/)) {
            $("#lblSymbols").addClass("d-none");
        }
        else {

            $("#lblSymbols").removeClass("d-none");
        }
        if (password.match(/[0-9]/)) {
            $("#lblNumbers").addClass("d-none");
        }
        else {

            $("#lblNumbers").removeClass("d-none");
        }
        if (password.length >= 8) {
            $("#lblLength").addClass("d-none");
        }
        else {

            $("#lblLength").removeClass("d-none");
        }
    }
    else {

        $("#lblUppercase").addClass("d-none");
        $("#lblLowercase").addClass("d-none");
        $("#lblSymbols").addClass("d-none");
        $("#lblNumbers").addClass("d-none");
        $("#lblLength").addClass("d-none");
    }
});
$(document).on("change", "#txtRole", function () {
    var isNative = $("#radioNative").prop("checked");
    var username = $("#txtUsername").val();
    var fullname = $("#txtFullname").val();
    var employeeId = $("#txtEmployeeId").val();
    var password = $("#txtPassword").val();
    var role = $("#txtRole").val();
    if (isNative) {
        if (username != "" && fullname != "" && employeeId != "" && password != "" && role != "") {

            if (password.match(/[A-Z]/) && password.match(/[a-z]/) && password.match(/[0-9]/) && password.match(/[\\@#$-/:-?{-~!"^_`\[\]]/) && password.length >= 8) {

                $("#btnSave").prop("disabled", false);
            }
            else {

                $("#btnSave").prop("disabled", true);
            }
        }
        else {

            $("#btnSave").prop("disabled", true);
        }
    }
    else {
        if (username != "" && fullname != "" && employeeId != "" && role != "") {

            $("#btnSave").prop("disabled", false);
        }
        else {

            $("#btnSave").prop("disabled", true);
        }
    }

});


$(document).on("click", "#btnActive", function () {
    $("#btnActive").addClass("active");
    $("#btnInactive").removeClass("active");

    getList();
});
$(document).on("click", "#btnInactive", function () {
    $("#btnInactive").addClass("active");
    $("#btnActive").removeClass("active");

    getListInactive();
    
});
function getData(data) {

    $.ajax({
        method: "POST",
        url: '/User/NewUser',
        data: { userform: data }
    }).done(function (set) {
        if (set.set.userId > 0) {
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
                'Failed!',
                set.set,
                'error'
            )

            //window.location.href = '/User/CreateUser';
        }
    });

    ;
}
function getDataEdit(data) {

    if ($("#txtEmployeeIdEdit").val() == "") {
        Swal.fire(
            'Failed!',
            "Employee ID is required.",
            'error'
        )
    }
    else {
        $.ajax({
            method: "POST",
            url: '/User/EditUser',
            data: { userform: data }
        }).done(function (set) {
            if (set.set.userId > 0) {
                $("#closeUserModal").click();
                Swal.fire(
                    'Succes!',
                    'Updated Successfully',
                    'success'
                )

                var si = setInterval(function () {
                    clearInterval(si);
                    document.location = '/User/CreateUser'
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
}

$(document).on("click", "#btnSave", function () {
    user = {

        accessType: $("#radioAD").prop("checked") ? "AD" : "Native",
        fullname: $("#txtFullname").val(),
        employeeId: $("#txtEmployeeId").val(),
        username: $("#txtUsername").val(),
        password: $("#txtPassword").val(),
        role: $("#txtRole").val(),
        oms: $("#chkOms").prop("checked"),
        runnerapp: $("#chkRunner").prop("checked"),
        pickerapp: $("#chkPicker").prop("checked"),
        boxerapp: $("#chkBoxer").prop("checked")

    }
    getData(user);
});

$(document).on("click", "#btnUpdate", function () {
    user = {

        
        userId: idToEdit,
        accessType: $("#radioADEdit").prop("checked") ? "AD" : "Native",
        fullname: $("#txtFullnameEdit").val(),
        employeeId: $("#txtEmployeeIdEdit").val(),
        username: $("#txtUsernameEdit").val(),
        password: $("#txtPasswordEdit").val(),
       /* password: isPasswordChange ? $("#txtPasswordEdit").val() : null,*/
        role: $("#txtRoleEdit").val(),
        oms: $("#chkOmsEdit").prop("checked"),
        runnerapp: $("#chkRunnerEdit").prop("checked"),
        pickerapp: $("#chkPickerEdit").prop("checked"),
        boxerapp: $("#chkBoxerEdit").prop("checked")

    }
    getDataEdit(user);
});


$(document).on("keyup", "#txtPasswordEdit", function () {
    var isNative = $("#radioNativeEdit").prop("checked");
    var username = $("#txtUsernameEdit").val();
    var fullname = $("#txtFullnameEdit").val();
    var employeeId = $("#txtEmployeeIdEdit").val();
    var password = $("#txtPasswordEdit").val();
    var role = $("#txtRoleEdit").val();


    if (isNative) {
        if (username != "" && fullname != "" && employeeId != "" && password != "" && role != "") {
            
            if (password.match(/[A-Z]/) && password.match(/[a-z]/) && password.match(/[0-9]/) && password.match(/[\\@#$-/:-?{-~!"^_`\[\]]/) && password.length >= 8) {

                $("#btnUpdate").prop("disabled", false);
            }
            else {

                $("#btnUpdate").prop("disabled", true);
            }
        }
        else {

            $("#btnUpdate").prop("disabled", true);
        }
    }
    else {
        if (username != "" && fullname != "" && employeeId != ""  && role != "") {

            $("#btnUpdate").prop("disabled", false);
        }
        else {

            $("#btnUpdate").prop("disabled", true);
        }
    }

});
$(document).on("keyup", "#txtPasswordEdit", function () {

    var password = $("#txtPasswordEdit").val();
    if (password != "") {
        if (password.match(/[A-Z]/)) {
            $("#lblUppercaseEdit").addClass("d-none");
        }
        else {

            $("#lblUppercaseEdit").removeClass("d-none");
        }
        if (password.match(/[a-z]/)) {
            $("#lblLowercaseEdit").addClass("d-none");
        }
        else {

            $("#lblLowercaseEdit").removeClass("d-none");
        }
        if (password.match(/[\\@#$-/:-?{-~!"^_`\[\]]/)) {
            $("#lblSymbolsEdit").addClass("d-none");
        }
        else {

            $("#lblSymbolsEdit").removeClass("d-none");
        }
        if (password.match(/[0-9]/)) {
            $("#lblNumbersEdit").addClass("d-none");
        }
        else {

            $("#lblNumbersEdit").removeClass("d-none");
        }
        if (password.length >= 8) {
            $("#lblLengthEdit").addClass("d-none");
        }
        else {

            $("#lblLengthEdit").removeClass("d-none");
        }
    }
    else {

        $("#lblUppercaseEdit").addClass("d-none");
        $("#lblLowercaseEdit").addClass("d-none");
        $("#lblSymbolsEdit").addClass("d-none");
        $("#lblNumbersEdit").addClass("d-none");
        $("#lblLengthEdit").addClass("d-none");
    }
});
$(document).on("change", "#txtRoleEdit", function () {
    var isNative = $("#radioNativeEdit").prop("checked");
    var username = $("#txtUsernameEdit").val();
    var fullname = $("#txtFullnameEdit").val();
    var employeeId = $("#txtEmployeeIdEdit").val();
    var password = $("#txtPasswordEdit").val();
    var role = $("#txtRoleEdit").val();
    if (isNative) {
        if (username != "" && fullname != "" && employeeId != "" && password != "" && role != "") {

            if (password.match(/[A-Z]/) && password.match(/[a-z]/) && password.match(/[0-9]/) && password.match(/[\\@#$-/:-?{-~!"^_`\[\]]/) && password.length >= 8) {

                $("#btnUpdate").prop("disabled", false);
            }
            else {

                $("#btnUpdate").prop("disabled", true);
            }
        }
        else {

            $("#btnUpdate").prop("disabled", true);
        }
    }
    else {
        if (username != "" && fullname != "" && employeeId != "" && role != "") {

            $("#btnUpdate").prop("disabled", false);
        }
        else {

            $("#btnUpdate").prop("disabled", true);
        }
    }

});
$(document).on("click", "#closeEditUserModal", function () {
    isPasswordChange = false
});



showCreateModal = function () {
    let moduleModal = $("#CreateUserModal");

    moduleModal.modal("show");
}

