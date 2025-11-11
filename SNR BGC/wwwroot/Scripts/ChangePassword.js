





$(document).ready(function () {

    $("#btnChangePassword").prop("disabled", true);


});


$(document).on('mousedown', '#btnShowPassword', function () {
    $('#txtPassword').attr('type', 'text')
});
$(document).on('mouseup', '#btnShowPassword', function () {
    $('#txtPassword').attr('type', 'password')
});
$(document).on('touchstart', '#btnShowPassword', function () {
    $('#txtPassword').attr('type', 'text')
});
$(document).on('touchend', '#btnShowPassword', function () {
    $('#txtPassword').attr('type', 'password')
});

$(document).on('mousedown', '#btnShowPasswordNew', function () {
    $('#txtPasswordNew').attr('type', 'text')
});
$(document).on('mouseup', '#btnShowPasswordNew', function () {
    $('#txtPasswordNew').attr('type', 'password')
});
$(document).on('touchstart', '#btnShowPasswordNew', function () {
    $('#txtPasswordNew').attr('type', 'text')
});
$(document).on('touchend', '#btnShowPasswordNew', function () {
    $('#txtPasswordNew').attr('type', 'password')
});



$(document).on('click', '#btnChangePassword', function () {
    var data = {
        oldPass: $('#txtPassword').val(),
        newPass: $('#txtPasswordNew').val(),
        confirmPass: $('#txtPasswordConfirm').val()
    };

    $.ajax({
        type: "POST",
        url: "/Home/CheckPasswords",
        data: {
            oldPass: $('#txtPassword').val(),
            newPass: $('#txtPasswordNew').val(),
            confirmPass: $('#txtPasswordConfirm').val()
    },
    }).done(function (set) {
        if (set.set == "Success") {
            Swal.fire({
                title: 'Succes!',
                text: 'Password Changed!',
                icon: 'success',
                confirmButtonText: 'OK'
            }).then((result) => {
                /* Read more about isConfirmed, isDenied below */
                if (result.isConfirmed) {
                    document.location = '/'
                }
            })

            var si = setInterval(function () {
                clearInterval(si);
                document.location = '/'
            }, 2000);
        }
        else if (set.set == "ConfirmNotMatch") {
            $("#errorDiv").removeClass("d-none");
            $("#errorLabel").text("The new password and confirm password does not match!")
        }
        else if (set.set == "InvalidOldPassword") {
            $("#errorDiv").removeClass("d-none");
            $("#errorLabel").text("The old password does not match!")
        }
        else if (set.set == "DefaultPassword") {
            $("#errorDiv").removeClass("d-none");
            $("#errorLabel").text("The new password should not be the default password!")
        }
        else if (set.set == "UsedPassword") {
            $("#errorDiv").removeClass("d-none");
            $("#errorLabel").text("This password has been used recently. Choose a new one.")
        }
    });
}); 

$(document).on("keyup", "#txtPasswordNew", function () {
    var enableButton;
    var password = $("#txtPasswordNew").val();
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



$(document).on("keyup", "#txtPassword, #txtPasswordNew, #txtPasswordConfirm", function () {


    var password = $("#txtPassword").val();
    var passwordNew = $("#txtPasswordNew").val();
    var passwordConfirm = $("#txtPasswordConfirm").val();
    var role = $("#txtRole").val();

    if (password != "" && passwordNew != "" && passwordConfirm != "") {

        if (passwordNew.match(/[A-Z]/) && passwordNew.match(/[a-z]/) && passwordNew.match(/[0-9]/) && passwordNew.match(/[\\@#$-/:-?{-~!"^_`\[\]]/) && passwordNew.length >= 8) {

            $("#btnChangePassword").prop("disabled", false);
        }
        else {

            $("#btnChangePassword").prop("disabled", true);
        }
    }
    else {

        $("#btnChangePassword").prop("disabled", true);
    }
}); 
    