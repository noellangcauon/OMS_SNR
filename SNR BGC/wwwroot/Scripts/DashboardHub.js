
$(function () {

    var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();
    connection.on("ReceiveDiscrepancyNotif", function (user, message) {
        $('#spanCount').text(user)
    });
    connection.start().then(function () {
       
    }).catch(function (err) {
        return console.error(err.toString());
    });

    $('#btnSendToHub').click(function () {
        LoadDiscrepancyNotif();
    })

    function LoadDiscrepancyNotif() {
        $.ajax({
            url: `/DiscrepancyCenter/GetDiscrepancyCount`,
            type: 'GET',

            success: function (data) {
                $.each(data, function (count, item) {
                    console.log('success discrepancy');
                   
                    connection.invoke('SendDiscrepancyNotif', item.boxId, '').catch(function (err) {
                        return console.error(err.toString());
                    });
                })

                console.log(data);
            }
        });

    }



    

})