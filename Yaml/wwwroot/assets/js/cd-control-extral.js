
/*▲画面起動エリア▲*/
$(function () {
    
    initPage.initElementEvent();
    initPage.initValidation();
})

const initPage = (function ($){
    "use strict"
    // init イベント
    function initElementEvent(){
        $("#login-btn").click(function () {
            loginComponent.login($("#name").val(), $("#pwd").val())
        })
    }
    
    function initValidation(){
        const run = {
            pwd: {
                required: true
            },
            name: {
                required: true
            }
        }
    }

    return {
        initValidation: initValidation,
        initElementEvent: initElementEvent
    };
})(jQuery)

const loginComponent = (function ($) {
    "use strict"
    
    function login(name, pwd){
        
        if($("loginForm").valid()){
            axios.post('/api/User/login', {name: name, password: pwd}).then(function (res){
                window.location.href="../../cd-control.html";
            }).catch(function (error) {
                console.log(error.response.data.errors.Name[0]);
            })
        }
     
    }

    return {
        login: login
    };
    
})(jQuery);
