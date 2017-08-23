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

	// case insensetive text search	
	$.expr[":"].contains = $.expr.createPseudo(function(arg) {
		return function( elem ) {
			return $(elem).text().toUpperCase().indexOf(arg.toUpperCase()) >= 0;
		};
	});

	// highlight the serach results
	$("td:contains('"+$('#highlight').html()+"')").each(function() {
		if ($(this).text().toUpperCase() == $('#highlight').text().toUpperCase())
		{
			$(this).html('<span class="search-found">' + $(this).html() + '</span>');
		}
		if ($(".f-info").text() != "")
		{
			$(this).html('<span class="search-found">' + $(this).html() + '</span>');
		}
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

/////////////////////
//  Search Visitors
///////////////////// 

$('.copy').on('change',function(){
	var link = $('#'+$(this).data('id')).attr('href');
	$('#'+$(this).data('id')).attr('href', link + '/' + $(this).val()); 
});

// highlight
//$("th:contains('ID')").each(function() {	
//	this.html('cool');
	//this.html() = '<span class="search-found">' + $('#highlight').text() + '</span>';
//});

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
