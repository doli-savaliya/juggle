/* on window ready */
jQuery(document).ready(function() {
	
	$('#handler').click(function()
	{
		$('#menu').slideToggle('fast');
	});

	$('.manu-handler').click(function () {
	    $('.main-menu').toggleClass('menu-open');
	});
	
	$('.tab-handler').on('click', function()
	{
		if($(".tab-row").hasClass("active"))
		{
			if($(this).parents(".tab-row").hasClass("active"))
			{
				$(this).parents(".tab-row").removeClass('active');
			}
			else
			{
				$('.tab-row').removeClass('active');
				$(this).parents(".tab-row").addClass('active');
			}
		}
		  else
		{
			$(this).parents(".tab-row").toggleClass('active');
			removeClass = true;
		}
		
		if($(".tab-row").hasClass("active"))
		{
			$(".tab-row .tab-slide").slideUp("fast");
			$(".tab-row.active .tab-slide").slideDown("fast");
		}
		else
		{
			$(".tab-row .tab-slide").slideUp("fast");
		}
		
		
	});
});

/* on window load */
jQuery(window).load(function(){

});

/* on window resize */
jQuery(window).resize(function(){

});