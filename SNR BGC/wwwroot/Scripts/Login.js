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


function updateSelectedOption() {
    var username = $("#txtUserName").val().trim() + "@snrshopping.com";

    $.ajax({
        url: '/Home/YourAjaxAction',
        type: 'GET',
        data: { username: username },
        dataType: 'json',
        success: function (data) {
            // Update the options in the dropdown based on the server response
            var selectOptions = document.getElementById('selectException');
            selectOptions.innerHTML = ""; // Clear existing options

            data.forEach(function (option) {
                var optionElement = document.createElement('option');
                optionElement.value = option.value;
                optionElement.text = option.text;
                if (option.value == 1) {
                    optionElement.selected = true;
                }
                selectOptions.appendChild(optionElement);
            });
        }
    });
}


$(document).on('change', '#txtUserName', function () {
    updateSelectedOption();
});
