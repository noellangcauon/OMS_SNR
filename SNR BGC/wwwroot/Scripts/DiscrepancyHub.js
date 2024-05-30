var connection = new signalR.HubConnectionBuilder().withUrl('/chatHub').build();


connection.on('recieveOrders', function (name, count) {
    $('#spanCount').text(count);
});

connection.start().then(function () {

}).catch(function (err) {
    return console.error(err.toString())
});

//$('#sampleBtn').click(function (e) {
//    connection.invoke('sendOrders', 'sample', 100).catch(function (err) {
//        return console.error(err.toString())

//    });
//    e.preventDefault();
//})