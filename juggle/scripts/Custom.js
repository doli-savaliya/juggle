
//$(document).ready(function($) {
//    jQuery(function($) {
//        $.mask.definitions['~'] = '[+-]';
//        $('#phoneno').mask('(999) 999-9999');

//    });
//});



function digitphone() {
    if (this.value.length > 10) {
        return false;
    }
}


function isNumber(evt) {
    evt = (evt) ? evt : window.event;
    var charCode = (evt.which) ? evt.which : evt.keyCode;
    if (charCode > 31 && (charCode < 48 || charCode > 57)) {
        return false;
    }
    return true;
}

$(function () {
    $('#firstName').keydown(function (e) {
        if (e.shiftKey || e.ctrlKey || e.altKey) {
            e.preventDefault();
        } else {
            var key = e.keyCode;
            if (!((key == 8) || (key == 32) || (key == 46) || (key >= 35 && key <= 40) || (key >= 65 && key <= 90))) {
                e.preventDefault();
            }
        }
    });
});
$(function () {
    $('#lastName').keydown(function (e) {
        if (e.shiftKey || e.ctrlKey || e.altKey) {
            e.preventDefault();
        } else {
            var key = e.keyCode;
            if (!((key == 8) || (key == 32) || (key == 46) || (key >= 35 && key <= 40) || (key >= 65 && key <= 90))) {
                e.preventDefault();
            }
        }
    });
});
$(function () {
    $('#firstName').on('keypress', function (e) {
        if (e.which === 32 && !this.value.length)
            e.preventDefault();
    });

    $('#lastName').on('keypress', function (e) {
        if (e.which === 32 && !this.value.length)
            e.preventDefault();
    });
    $('#email').on('keypress', function (e) {
        if (e.which === 32 && !this.value.length)
            e.preventDefault();
    });
    $('#username').on('keypress', function (e) {
        if (e.which === 32 && !this.value.length)
            e.preventDefault();
    });
    $('#phoneno').on('keypress', function (e) {
        if (e.which == 32)
            return false;
    });
    $('#address').on('keypress', function (e) {
        if (e.which === 32 && !this.value.length)
            e.preventDefault();
    });
    $(function () {
        $('#username').keyup(function () {
            var yourInput = $(this).val();
            re = /[`~!@#$%^&*()_|+\-=?;:'",.<>\{\}\[\]\\\/]/gi;
            var isSplChar = re.test(yourInput);
            if (isSplChar) {
                var no_spl_char = yourInput.replace(/[`~!@#$%^&*()_|+\-=?;:'",.<>\{\}\[\]\\\/]/gi, '');
                $(this).val(no_spl_char);
            }
        });

    });

    $(function () {
        $('#lastName').keyup(function () {
            var yourInput = $(this).val();
            re = /[`~!@#$%^&*()_|+\-=?;:'",.<>\{\}\[\]\\\/]/gi;
            var isSplChar = re.test(yourInput);
            if (isSplChar) {
                var no_spl_char = yourInput.replace(/[`~!@#$%^&*()_|+\-=?;:'",.<>\{\}\[\]\\\/]/gi, '');
                $(this).val(no_spl_char);
            }
        });

    });

    $(function () {
        $('#firstName').keyup(function () {
            var yourInput = $(this).val();
            re = /[`~!@#$%^&*()_|+\-=?;:'",.<>\{\}\[\]\\\/]/gi;
            var isSplChar = re.test(yourInput);
            if (isSplChar) {
                var no_spl_char = yourInput.replace(/[`~!@#$%^&*()_|+\-=?;:'",.<>\{\}\[\]\\\/]/gi, '');
                $(this).val(no_spl_char);
            }
        });

    });




});


// for create work type button validation

$(function () {
    $('#name').keydown(function (e) {
        if (e.shiftKey || e.ctrlKey || e.altKey) {
            e.preventDefault();
        } else {
            var key = e.keyCode;
            if (!((key == 8) || (key == 32) || (key == 46) || (key >= 35 && key <= 40) || (key >= 65 && key <= 90))) {
                e.preventDefault();
            }
        }
    });
});


$(function () {
    $('#name').on('keypress', function (e) {
        if (e.which === 32 && !this.value.length)
            e.preventDefault();
    });
});

$("#Create").click(function () {
    var txtfname = document.getElementById('name');
    var flag = true;
    if (txtfname.value == "") {
        txtfname.placeholder = "Name should not be blank";
        txtfname.focus();
        flag = false;
    }

    if (flag == false) {
        return false;
    }
    else {
        return true;
    }
});


// create user and supervisior

$("#button").click(function () {
    var txtfname = document.getElementById('firstName');
    var txtlname = document.getElementById('lastName');
    var email = document.getElementById('email');
    //  var dllstatus = document.getElementById('status_id');
    var username = document.getElementById('username');
    var phoneno = document.getElementById('phoneno');

    var flag = true;
    if (txtfname.value == "") {
        txtfname.placeholder = "Firstname should not be blank";
        txtfname.className += "apply";
        txtfname.focus();
        flag = false;
    }

    if (txtlname.value == "") {
        txtlname.placeholder = "Lastname should not be blank";
        txtlname.className += "apply";
        txtlname.focus();
        flag = false;
    }
    if (address.value == "") {
        address.placeholder = "Address should not be blank";
        address.className += "apply";
        address.focus();
        flag = false;
    }
    if (phoneno.value == "") {
        phoneno.placeholder = "Phone number should not be blank";
        phoneno.className += "apply";
        phoneno.focus();
        flag = false;
    }

    if (!(/^\d{10}$/.test(phoneno.value))) {
        phoneno.placeholder = "Please enter proper phone number";
        phoneno.focus();
        flag = false;

    }
    if (email.value == "") {
        email.placeholder = "Email ID should not be blank";
        email.className += "apply";
        email.focus();
        flag = false;
    }
    if (!(/^(([^<>()[\]\\.,;:\s@\"]+(\.[^<>()[\]\\.,;:\s@\"]+)*)|(\".+\"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/.test(email.value))) {
        email.placeholder = "Please enter proper Email address";
        email.focus();
        flag = false;
    }
    if (username.value == "") {
        username.placeholder = "Username should not be blank";
        username.className += "apply";
        username.focus();
        flag = false;
    }

    if (dllstatus.selectedIndex == 0) {
        dllstatus.style.color = "Red";
        dllstatus.focus();
        flag = false;

    }

    $(function () {
        $('#input1').on('keypress', function (e) {
            if (e.which == 32)
                return false;
        });
    });



    if (flag == false) {
        return false;
    }
    else {
        return true;
    }

});
function dropcolorchange() {
    dllstatus.style.color = "black";
}


// update work type button validation

$("#update").click(function () {
    var txtfname = document.getElementById('updatename');
    var flag = true;
    if (txtfname.value == "") {
        txtfname.placeholder = "Name should not be blank";
        txtfname.focus();
        flag = false;
    }

    if (flag == false) {
        return false;
    }
    else {
        return true;
    }
});

$(function () {
    $('#updatename').on('keypress', function (e) {
        if (e.which === 32 && !this.value.length)
            e.preventDefault();
    });
});


$(function () {
    $('#updatename').keydown(function (e) {
        if (e.shiftKey || e.ctrlKey || e.altKey) {
            e.preventDefault();
        } else {
            var key = e.keyCode;
            if (!((key == 8) || (key == 32) || (key == 46) || (key >= 35 && key <= 40) || (key >= 65 && key <= 90))) {
                e.preventDefault();
            }
        }
    });
});

