
var isRunning = false;

$(document).ready(function () {
    runFunctionPeriodically();
   
});
function myFunction() {
    if (isRunning) {
        console.log("Previous execution still in progress. Skipping this interval.");
        return;
    }

    isRunning = true;

    // Your potentially long-running code here


    ajaxLoader('show');
    ShopeeGetOrders();
    ajaxLoader('hide');

    console.log("Function executed!");

    isRunning = false;
}



// Set up a timeout to run the function every 2 hours
function runFunctionPeriodically() {
    myFunction();
    setTimeout(runFunctionPeriodically, 2 * 60 * 60 * 1000);
}

 // Start the loop


function ShopeeGetOrders() {
    $.ajax({
        url: '/AutoReload/GetOrdersCatchLazada',
        method: 'GET',
        success: function () {
            // Handle successful response
            ajaxLoader('hide'); // Hide loading indicator
        },
        error: function () {
            // Handle AJAX error
            ajaxLoader('hide'); // Hide loading indicator
        }
    });
}