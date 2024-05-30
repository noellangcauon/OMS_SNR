$("#searchUser").keypress(  
    function (event) {
        if (event.which == '13') {
            event.preventDefault();
        }
    });
$("#searchUserList").keypress(
    function (event) {
        if (event.which == '13') {
            event.preventDefault();
        }
    });
/*$("#uIdDiv").click(function () {
    console.log(">>>>>>>mag focus ka>>>>>>>")
    $("#searchUserList").focus();
});*/
/*
##############################################
#FUNCTION NAME : addUser
#PARAMETERS    : -
#DESCRIPTION   : To add the user
#CREATED BY    : Michael Angelo A. De Fiesta
#DATE CREATED  : 08/16/2022
#MODIFIED BY   :
##############################################
*/
function addUser() {
    var obj = {};
    var accessList = "";
    var uID = 0;
    uID = parseInt($("#uId").val());

    $('input[type=checkbox]').each(function () {
        var sThisVal = (this.checked ? "1" : "0");
        if (sThisVal != 0) {
            accessList += (accessList == "" ? $(this).val() : "," + $(this).val());
        }
    });

    if ($("#uname").val() == '') {
        $("#successModalBtn").click();
        $("exampleModalLongTitle").html("User Management!");
        $("#successModalDiv").html("Please complete user information!");
        return;
    }
 

    if ($("#userInfoBtn").val() == 'Update') {
        obj = {
            'userId': uID,
            'username': $("#uname").val(),
            'userFullname': $("#fullname").val(),
            'userRole': $("#urole").val(),
            'userModule': $("#umodule").val(),
            'userSubModule': $("#usubmodule").val(),
            'userAccess': accessList,
            'userStatus': $("#ustatus").val(),
            'lastEditUser': $('#userNameProfile').text()
        }

    } else {
        obj = {
            'username': $("#uname").val(),
            'userFullname': $("#fullname").val(),
            'userRole': $("#urole").val(),
            'userModule': $("#umodule").val(),
            'userSubModule': $("#usubmodule").val(),
            'userAccess': accessList,
            'userStatus': $("#ustatus").val()
        }
    }
    $(".addUserD").each(function () {
           if ($(this).val() == 0) {
            return;
        }
    });
    ajaxLoader('show');
    $.ajax({
        type: "POST",
        url: $("#divAdduser").data("request-url"),
        data: JSON.stringify(obj),
        dataType: "json",
        contentType: "application/json;charset=utf-8",
        success: function (data) {
            ajaxLoader('hide');
            if (data.responseText == "Success") {
                $("#successModalBtn").click();
                fetchUser("");
                clearUserForm();
                $("#exampleModalLongTitle").text("User Management!");
                $("#successModalDiv").html("User  " + data.uname + "  " +data.set );
                $('#userInfoBtn').val('Save');
                
            } else {
                $("#successModalBtn").click();
                $("#successModalDiv").html("User NOT Sucessfully Save!");
            }
        },
        error: function (request, status, error) {

            alert(error);
        }
    })
}
/*
##############################################
#FUNCTION NAME : fetchUser
#PARAMETERS    : filter
#DESCRIPTION   : To fetch user in database(DBLazPee) table(userAccessTable)
#CREATED BY    : Michael Angelo A. De Fiesta
#DATE CREATED  : 08/16/2022
#MODIFIED BY   :
##############################################
*/
function fetchUser(filter) {
    var category = $("#userCategory").val();
    ajaxLoader('show');
    $.ajax({
        type: "POST",
        url: $("#divFetchUser").data("request-url") + "?filter=" + filter + "&category=" + category,
        //data: JSON.stringify(obj),
        dataType: "json",
        contentType: "application/json;charset=utf-8",
        success: function (data) {
            ajaxLoader('hide')
            var html = "";
            if ($.isEmptyObject(data)) {
                html = '<tr class="text-center"><td colspan="8">No available data.</td></tr>'

            } else {
                for (var x = 0; x < data.length; x++) {
                    html += '<tr>'				
                        html += '<td style="cursor:pointer" onclick="viewUserInfo(this)" userId="' + data[x]['userId']+'" username="'+data[x]['username']+'" userFullname="'+data[x]['userFullname']+'" userRole="'+data[x]['userRole']+'" userModule="'+data[x]['userModule']+'" userSubModule="'+data[x]['userSubModule']+'" userStatus="'+data[x]['userStatus']+'" userAccess="'+data[x]['userAccess']+'" >'+data[x]['username']+'</td>';
                        html += '<td style="cursor:pointer" onclick="viewUserInfo(this)" userId="' + data[x]['userId']+'" username="'+data[x]['username']+'" userFullname="'+data[x]['userFullname']+'" userRole="'+data[x]['userRole']+'" userModule="'+data[x]['userModule']+'" userSubModule="'+data[x]['userSubModule']+'" userStatus="'+data[x]['userStatus']+'" userAccess="'+data[x]['userAccess']+'" >'+data[x]['userFullname']+'</td>';
                        html += '<td style="cursor:pointer" onclick="viewUserInfo(this)" userId="' + data[x]['userId']+'" username="'+data[x]['username']+'" userFullname="'+data[x]['userFullname']+'" userRole="'+data[x]['userRole']+'" userModule="'+data[x]['userModule']+'" userSubModule="'+data[x]['userSubModule']+'" userStatus="'+data[x]['userStatus']+'" userAccess="'+data[x]['userAccess']+'" >'+data[x]['userRole']+'</td>';
                        html += '<td style="cursor:pointer" onclick="viewUserInfo(this)" userId="' + data[x]['userId']+'" username="'+data[x]['username']+'" userFullname="'+data[x]['userFullname']+'" userRole="'+data[x]['userRole']+'" userModule="'+data[x]['userModule']+'" userSubModule="'+data[x]['userSubModule']+'" userStatus="'+data[x]['userStatus']+'" userAccess="'+data[x]['userAccess']+'" >'+data[x]['userModule']+'</td>';
                        html += '<td style="cursor:pointer" onclick="viewUserInfo(this)" userId="' + data[x]['userId']+'" username="'+data[x]['username']+'" userFullname="'+data[x]['userFullname']+'" userRole="'+data[x]['userRole']+'" userModule="'+data[x]['userModule']+'" userSubModule="'+data[x]['userSubModule']+'" userStatus="'+data[x]['userStatus']+'" userAccess="'+data[x]['userAccess']+'" >'+data[x]['userSubModule']+'</td>';
                        html += '<td style="cursor:pointer" onclick="viewUserInfo(this)" userId="' + data[x]['userId']+'" username="'+data[x]['username']+'" userFullname="'+data[x]['userFullname']+'" userRole="'+data[x]['userRole']+'" userModule="'+data[x]['userModule']+'" userSubModule="'+data[x]['userSubModule']+'" userStatus="'+data[x]['userStatus']+'" userAccess="'+data[x]['userAccess']+'" >'+data[x]['userAccess']+'</td>';
                        html += '<td style="cursor:pointer" onclick="viewUserInfo(this)" userId="' + data[x]['userId']+'" username="'+data[x]['username']+'" userFullname="'+data[x]['userFullname']+'" userRole="'+data[x]['userRole']+'" userModule="'+data[x]['userModule']+'" userSubModule="'+data[x]['userSubModule']+'" userStatus="'+data[x]['userStatus']+'" userAccess="'+data[x]['userAccess']+'" >'+data[x]['userStatus']+'</td>';
                        html += '<td style="cursor:pointer" onclick="deleteOpenUserModal(this)" userId="' + data[x]['userId'] + '" userFullname="' + data[x]['userFullname'] +'"><i class="fa fa-trash" style="cursor:pointer"></i></td>';
                    html += '</tr>';
                   
                }
            }
            $('#userDataTable > tbody').html(html);
                

        },
        error: function (request, status, error) {

            alert(error);
        }
    })
}
/*
##############################################
#FUNCTION NAME : clearUserForm
#PARAMETERS    : 
#DESCRIPTION   : To clear user form
#CREATED BY    : Michael Angelo A. De Fiesta
#DATE CREATED  : 08/17/2022
#MODIFIED BY   :
##############################################
*/
function clearUserForm() {
    $(".addUser").val('');
    $(".addUserD").val('0');
    $(".addUserC").prop('checked', false)
    $("#userCancelBtn").hide()
    $('#userInfoBtn').val('Save');

}
/*
##############################################
#FUNCTION NAME : closemodal
#PARAMETERS    : ID->> to pass the modal id
#DESCRIPTION   : To dynamic close of modal
#CREATED BY    : Michael Angelo A. De Fiesta
#DATE CREATED  : 08/17/2022
#MODIFIED BY   :
##############################################
*/
function closemodal(id) {
    $("#" + id).modal("hide")

}
/*
##############################################
#FUNCTION NAME : viewUserInfo
#PARAMETERS    : ths->> to get all attr
#DESCRIPTION   : To transfer data in update form
#CREATED BY    : Michael Angelo A. De Fiesta
#DATE CREATED  : 08/17/2022
#MODIFIED BY   :
##############################################
*/
function viewUserInfo(ths) {
    var access = $(ths).attr('userAccess');
    
    $('#uId').val($(ths).attr('userId'));
    $('#uname').val($(ths).attr('username'));
    $('#fullname').val($(ths).attr('userFullname'));
    $('#urole').val($(ths).attr('userRole'));
    $('#umodule').val($(ths).attr('userModule'));
    $('#usubmodule').val($(ths).attr('userSubModule'));
    $('#ustatus').val($(ths).attr('userStatus'));

    if (access.match(/A/g)) {
        $('#AddAccess').prop('checked', true);
    } else {
        $('#AddAccess').prop('checked', false);
    }
    if (access.match(/E/g)) {
        $('#EditAccess').prop('checked', true);
    } else {
        $('#EditAccess').prop('checked', false);
    }
    if (access.match(/D/g)) {
        $('#DeleteAccess').prop('checked', true);
    } else {
        $('#DeleteAccess').prop('checked', false);
    }
    if (access.match(/R/g)) {
        $('#ReadAccess').prop('checked', true);
    } else {
        $('#ReadAccess').prop('checked', false);
    }
/*    $('.userInfoClass').each(function () {
        var attr = $(this).attr('attr')
        $('.userInfoClass [attr="'+attr+'"]').val($(ths).attr(attr));
    })*/
    $('#userInfoBtn').val('Update');
    $("#userCancelBtn").show();
}
/*
##############################################
#FUNCTION NAME : deleteOpenUserModal
#PARAMETERS    : (ths) that contain user info
#DESCRIPTION   : To open warning modal to delete
#CREATED BY    : Michael Angelo A. De Fiesta
#DATE CREATED  : 08/18/2022
#MODIFIED BY   :
##############################################
*/
function deleteOpenUserModal(ths) {
    $("#deleteUserId").val($(ths).attr("userId"))
    $("#deleteUserName").val($(ths).attr("userFullname"))
    $("#deleteModalBtn").click();
    $("#deleteModalDiv").html("Are you sure do you want to delete   " + $(ths).attr("userFullname") +"?");

}
/*
##############################################
#FUNCTION NAME : deleteUser
#PARAMETERS    : 
#DESCRIPTION   : To delete user from access table
#CREATED BY    : Michael Angelo A. De Fiesta
#DATE CREATED  : 08/18/2022
#MODIFIED BY   :
##############################################
*/
function deleteUser() {
 
    var uId = parseInt($("#deleteUserId").val())
    var uname = $("#deleteUserName").val();

    var obj = {
        'userId': uId,
        'userFullname' : uname
    }
    ajaxLoader('show');
    $.ajax({
        type: "POST",
        url: $("#divDeleteUser").data("request-url"),
        data: JSON.stringify(obj),
        dataType: "json",
        contentType: "application/json;charset=utf-8",
        success: function (data) {
            ajaxLoader('hide');
            $("#deleteUserId").val('');
            $("#deleteUserName").val('');
            $("#successModalBtn").click();
            fetchUser("");
            $("#exampleModalLongTitle").text("User Management!");
            $("#successModalDiv").html("User  " + data.uname + "  " + data.set);
           
        },
        error: function (request, status, error) {

            alert(error);
        }
    })

}
/*
##############################################
#FUNCTION NAME : fetchAD
#PARAMETERS    : 
#DESCRIPTION   : To fetch user from active directory
#CREATED BY    : Michael Angelo A. De Fiesta
#DATE CREATED  : 08/19/2022
#MODIFIED BY   :
##############################################
*/
function fetchAD() {

    var uname = $('#searchUserList').val();

    if (uname == '' || null) {
        return
    }
    ajaxLoader('show');

    $.ajax({
        type: "POST",
        url: $("#divFetchAD").data("request-url") + "?uname=" + uname,
        //data: JSON.stringify(obj),
        dataType: "json",
        contentType: "application/json;charset=utf-8",
        success: function (data) {
            ajaxLoader('hide');
           
            var html = "";
            if ($.isEmptyObject(data.set)) {
                html = '<tr class="text-center"><td colspan="8">No available data.</td></tr>'

            } else {
                for (var x = 0; x < data.set.length; x++) {
                    html += '<tr>'
                    html += '<td>' + data.set[x]['username'] + '</td>';
                    html += '<td>' + data.set[x]['userFullname'] + '</td>';
                    html += '<td style="cursor:pointer;" class="text-center" onclick="selectUserModal(this)" username="'+ data.set[x]['username'] +'" userFullname="' + data.set[x]['userFullname'] + '"><i class="fa fa-plus" style="cursor:pointer"></i></td>';
                    html += '</tr>';

                }
            }
            $("#listOfUsers > tbody").html(html)

        },
        error: function (request, status, error) {

            alert(error);
        }
    })

}
/*
##############################################
#FUNCTION NAME : openUserSearchModal
#PARAMETERS    : 
#DESCRIPTION   : To open search modal
#CREATED BY    : Michael Angelo A. De Fiesta
#DATE CREATED  : 08/19/2022
#MODIFIED BY   :
##############################################
*/
function openUserSearchModal() {
    $("#searchModalBtn").click();
    var html = '<tr class="text-center"><td colspan="3">No available data.</td></tr>';
    $("#listOfUsers > tbody").html(html)

}
/*
##############################################
#FUNCTION NAME : selectUserModal
#PARAMETERS    : 
#DESCRIPTION   : To open select user and transfer to user form
#CREATED BY    : Michael Angelo A. De Fiesta
#DATE CREATED  : 08/19/2022
#MODIFIED BY   :
##############################################
*/
function selectUserModal(ths) {
    var username = $.trim($(ths).attr('userFullname'))
    if (username == '' || username == null) {
        
        $("#successModalBtn").click();
        $("exampleModalLongTitle").html("User Management!");
        $("#successModalDiv").html("User invalid! Please check user information!");
        return;
    }
    $("#uname").val($(ths).attr("username"));
    $("#fullname").val($(ths).attr("userFullname"));
    $("#searchUserList").val('')
    closemodal("searchModal");
    $("#userCancelBtn").show();

}


