
$(document).ready(function () {
});



function LoginAdmin() {
    const item = {
        UserName: $("#AdminAccountName").val(),
        Password: $("#AdminAccountPassword").val()
    };

    $.ajax({
        type: "POST",
        accepts: "application/json",
        url: "api/Account/Admin/Login",
        contentType: "application/json",
        data: JSON.stringify(item),
        error: function(jqXHR, textStatus, errorThrown) {
            alert("Something went wrong in LoginAdmin!");
        },
        success: function(result) {
            $("#AdminAccountName").val("");
            $("#AdminAccountPassword").val("");
            window.location.href = "api/Account/Index";
        }
    })
}



function LogoutAdmin() {

    $.ajax({
        type: "POST",
        accepts: "application/json",
        url: "api/Account/Logout",
        contentType: "application/json",
        error: function (jqXHR, textStatus, errorThrown) {
            alert("Error in logout");
        },
        success: function (result) {
        }
    })
}


