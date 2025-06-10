
"use strict"

var dlabSettingsOptions = {};



(function($) {

    "use strict"

    /* var direction =  getUrlParams('dir');

    if(direction == 'rtl')
    {
        direction = 'rtl';
    }else{
        direction = 'ltr';
    } */

    dlabSettingsOptions = {
        typography: "poppins",
        version: "light",
        layout: "vertical",
        primary: "color_1",
        headerBg: "color_3",
        navheaderBg: "color_2",
        sidebarBg: "color_2",
        sidebarStyle: "full",
        sidebarPosition: "fixed",
        headerPosition: "fixed",
        containerLayout: "full",
    };


    new dlabSettings(dlabSettingsOptions);

    jQuery(window).on('resize',function(){
        /*Check container layout on resize */
        ///alert(dlabSettingsOptions.primary);
        dlabSettingsOptions.containerLayout = $('#container_layout').val();
        /*Check container layout on resize END */

        new dlabSettings(dlabSettingsOptions);
    });

})(jQuery);