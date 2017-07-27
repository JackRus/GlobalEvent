// Write your Javascript code.

/////////////////////
//    Tickets
/////////////////////

// add/remove 1 ticket
$("input.mybtn-sm").on('click', function () {
	var type = $(this).attr("data-type");
	var current = $("#save").attr("value");
	if ($(this).attr("value") == "ADD") {
		$(this).attr("value", "REMOVE").addClass("remove").removeClass("add");

		if ($("#save").attr("value") == "" || $("#save").attr("value") == undefined) {
			$("#save").attr("value", type);
		} else {
			$("#save").attr("value", current + '|' + type);
		}
	} else {
		$(this).attr("value", "ADD").addClass("add").removeClass("remove");
		current = current.replace("|" + type, "");
		current = current.replace(type, "");
		if (current.indexOf("|") == 0) {
			current = current.slice(1);
		}
		$("#save").attr("value", current);
	}
});

// remove all tickets
$("#all").on('click', function () {
	$(".remove").each(function () {
		$(this).attr("value", "ADD").addClass("add").removeClass("remove");
	});
	$("#save").attr("value", "");
});

// add all tickets
$("#addall").on('click', function () {
	$("#save").attr("value", "");
	$("input.mybtn-sm").each(function () {
		var type = $(this).attr("data-type");
		var current = $("#save").attr("value");
		$(this).attr("value", "REMOVE").addClass("remove").removeClass("add");

		if ($("#save").attr("value") == "" || $("#save").attr("value") == undefined) {
			$("#save").attr("value", type);
		} else {
			$("#save").attr("value", current + '|' + type);
		}
	});
});


// generate QR code for name tag
$(document).ready(function() {
	var number = $("#regnum").text();
	/* Creates QR code */
	$("#qr").qrcode({
		width: 96,
		height: 96,
		//render : "table",
		text   : number
	});
});

// toggles
$('.questions').click(function(){
   $('#'+$(this).data('id')).toggle(); 
});

$("#ok").click(function(){
   $("#understand").addClass("eye"); 
   $("#understood").removeClass("eye");
});