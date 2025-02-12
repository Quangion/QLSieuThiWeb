$(document).ready(function() {
    $(".input_text").focus(function() {
        $(this).prev('.fa').addClass('glowIcon');
    });
    
    $(".input_text").focusout(function() {
        $(this).prev('.fa').removeClass('glowIcon');
    });
}); 