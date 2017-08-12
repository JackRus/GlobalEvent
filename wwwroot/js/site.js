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

/////////////////////
//    QR Code
/////////////////////

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

	$(":checkbox:checked").each(function() {
		$("[data-id=" + $(this).attr('id') + "]").html('X').removeClass('true-false').addClass('true-false-checked');
	}); 
});

/////////////////////
//    Q&A
/////////////////////

$('.questions').click(function(){
   $('#'+$(this).data('id')).toggle(); 
});

/////////////////////
//    Toggles
/////////////////////

$('.tog').click(function(){
   $('#'+$(this).data('id')).toggle(); 
});

$("#ok").click(function(){
   $("#understand").addClass("eye"); 
   $("#understood").removeClass("eye");
});

$('.copy').on('change',function(){
   $('#'+$(this).data('id')).attr('asp-route-ID', $(this).val()); 
   $('#'+$(this).data('id')).attr('asp-route-Name', $(this).data('name')); 
});

/////////////////////
//    Check-Box
/////////////////////

$('.true-false').click(function(){
	if ($('#'+$(this).data('id')).is(':checked')) 
	{
		$(this).html('').removeClass('true-false-checked').addClass('true-false');
		$('#'+$(this).data('id')).prop('checked', false);
	}
	else
	{
		$(this).html('X').removeClass('true-false').addClass('true-false-checked');
		$('#'+$(this).data('id')).prop('checked', true)
	}
});
