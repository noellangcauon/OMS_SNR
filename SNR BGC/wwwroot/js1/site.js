// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

/*
 #######################################################################
 *
 *  FUNCTION NAME : ajaxLoader 
 *  AUTHOR        : Michael Angelo A. de Fiesta
 *  DATE          : August 18, 2022
 *  MODIFIED BY   : 
 *  REVISION DATE : 
 *  REVISION #    : 
 *  DESCRIPTION   : 
 *  PARAMETERS    : 
 #######################################################################
 */

var retryCount = 0;

function updateRetryMessage() {
	retryCount++;
	var messageElement = document.getElementById('retry-message');
	messageElement.textContent = "Retrying... (" + retryCount + ")";
}

function retryCountReset() {
	retryCount = 0;
}

function ajaxLoader(type, label) {
	if (label == undefined) { label = 'Processing Information'; }
	if (type == 'show') {
		
		$('body').css({ 'cursor': 'progress' });
		var h = innerHeight / 2;
		var hhalf = h - 70;
		var w = innerWidth / 2;
		var whalf = w - 90;
		var msg = "<center><div class='loading'><p>Processing.....</p></div></center>";
		$("#processMsg").remove();
		$("body").append("<center><div style='width:100%; height:100%;' id='processMsg' style='' class='label'>" + msg + "<div class='ui-widget-overlay' style='z-index:145;'  ></div></div></center>");
	} else {
		
		$('body').css({ 'cursor': 'default' });

		/* Remove all instances of the ajaxLoader box */
		$('#processMsg').each(function () {
			$(this).remove();
		});

		/* Remove any stray ajax-loader image, if any */
	/*	$('img[src="images/loader.gif"]').each(function () {
			$(this).remove();
		});*/
	}
}


function ajaxLoaderRetry(type, label) {
	if (label == undefined) { label = 'Loading...'; }
	if (type == 'show') {

		$('body').css({ 'cursor': 'progress' });
		var h = innerHeight / 2;
		var hhalf = h - 70;
		var w = innerWidth / 2;
		var whalf = w - 90;
		var msg = "<center><div class='loading' style='width: 180px; padding: 20px;'><p id='retry-message' style='padding: 20px; background-color: white; z-index: 9999; position: absolute; border-radius: 50px; margin-top: 30px;'>Please wait...</p></div></center>";
		$("#processMsg").remove();
		$("body").append("<center><div style='width:100%; height:100%;' id='processMsg' style='' class='label'>" + msg + "<div class='ui-widget-overlay' style='z-index:145;'  ></div></div></center>");
	} else {

		$('body').css({ 'cursor': 'default' });

		/* Remove all instances of the ajaxLoaderRetry box */
		$('#processMsg').each(function () {
			$(this).remove();
		});

		/* Remove any stray ajax-loader image, if any */
		/*	$('img[src="images/loader.gif"]').each(function () {
				$(this).remove();
			});*/
	}
}
/*
##############################################
#FUNCTION NAME : getCurrDate
#PARAMETERS    :
#DESCRIPTION   : Dynamic get date
#CREATED BY    : Michael Angelo A. De Fiesta
#DATE CREATED  : 08/31/2022
#MODIFIED BY   :
##############################################
*/
getCurrDate = function () {
	var d = new Date();
	var mon = (d.getMonth() + 1)
	var day = (d.getDate())
	if (mon.toString().length == 1) {
		mon = "0" + mon.toString();
	}
	if (day.toString().length == 1) {
		day = "0" + day.toString();
	}
	var strDate = d.getFullYear() + "-" + mon + "-" + day;
	return strDate;
}



/*
##############################################
#FUNCTION NAME : getCurrDateTime
#PARAMETERS    :
#DESCRIPTION   : To get and save the lazada order items
#CREATED BY    : Michael Angelo A. De Fiesta
#DATE CREATED  : 09/18/2022
#MODIFIED BY   :
##############################################
*/
getCurrDateTime = function () {
	var now = new Date();
	now.setMinutes(now.getMinutes() - now.getTimezoneOffset());
	var newDateTime = new Date();
	newDateTime = now.toISOString().slice(0, 16);

	return newDateTime;

}
//const modeSwitch = document.getElementById("switch")

//modeSwitch.addEventListener("click", evt => {
//	const el = evt.target

//	el.classList.toggle("is-switched")

//	if (el.getAttribute("aria-checked") === "true") {
//		el.setAttribute("aria-checked", "false")
//	} else {
//		el.setAttribute("aria-checked", "true")
//	}
//})



function preventBack() { window.history.forward(); }
setTimeout("preventBack()", 0);
window.onunload = function () { null };  


function limitLength(element, maxLength) {
	if (element.value.length > maxLength) {
		element.value = element.value.slice(0, maxLength);
	}
}