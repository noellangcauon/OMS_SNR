﻿
var isRunning = false;

$(document).ready(function () {
    var delay = 10 - (new Date().getTime() % 10); 

    setTimeout(function () {
        runFunctionPeriodically();
    }, delay);

   
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



// Set up a timeout to run the function every 5 minutes
function runFunctionPeriodically() {
    myFunction();
    setTimeout(runFunctionPeriodically, 10 * 60 * 1000);
}

 // Start the loop


function ShopeeGetOrders() {
    $.ajax({
        url: '/AutoReload/GetOrdersLazada',
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