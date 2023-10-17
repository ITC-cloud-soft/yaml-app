$(function () {

    new Promise((resolve, reject)=>{
        resolve(initI18next());
    }).then(function (result){
        initPage.initElementEvent();
        initPage.initValidation();
    })

    // 阻止表单的默认提交行为
    $('#loginForm').submit(function (event) {
        event.preventDefault();
    });

    $('#languageSwitcher').change((a, b, c) => {
        const chosenLng = $(this).find("option:selected").attr('value');
        i18next.changeLanguage(chosenLng, () => {
            render();
            initPage.render();
            userLanguage = chosenLng;
        });
    });
})

const initPage = (function ($){
    "use strict"
    function initElementEvent(){
        $("#login-btn").click(function () {
            const name = $("#name").val();
            const pwd = $("#pwd").val();
            loginComponent.login(name, pwd)
        })
    }

    function initValidation(){
        
        const rules = {
            name: "required",
            pwd: {
                required: true,
                minlength: 6
            }
        }

        const loginForm = $("#loginForm");
        loginForm.validate({
            focusCleanup: true,
            onkeyup: false,
            ignore: "",
            onfocusin: false,
            rules: {
                name: "required",
                pwd: {
                    required: true,
                    minlength: 6
                }
            },
            messages: {
                name: i18next.t('login-page.loginId'),
                pwd: {
                    required: i18next.t('login-page.password')
                }
            },
            errorPlacement: function (error, element){
                error.insertAfter(element.parent());
            }
        })
    }
    
    function render(){
       $("#name-error").text(i18next.t('login-page.loginId')) 
       $("#pwd-error").text(i18next.t('login-page.password'))
    }

    return {
        initValidation: initValidation,
        render: render,
        initElementEvent: initElementEvent
    };
})(jQuery)

const loginComponent = (function ($) {
    "use strict"

    function login(name, pwd){
        if($("#loginForm").valid()){
            commonFunctions.axios().post('/api/User/login', {name: name, password: pwd
            }).then(function (res){
                window.location.href="../../../cd-control.html";
            }).catch(function (error) {
                console.log(error.response.data);
                // if error show modal
                commonFunctions.showModal("Login Warn", error.response.data)
            })
        }
    }
    return {
        login: login
    };
})(jQuery);

