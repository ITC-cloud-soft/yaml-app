$(function () {

    new Promise((resolve, reject) => {
        resolve(initI18next());
    }).then(function (result) {
        initPage.initElementEvent();
        initPage.initValidation(i18next);
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

const initPage = (function ($) {
    "use strict"

    function initElementEvent() {
        $("#login-btn").click(function () {
            const name = $("#name").val();
            const pwd = $("#pwd").val();
            loginComponent.login(name, pwd)
        })
    }

    function initValidation(i18next) {

        const rules = {
            name: "required",
            pwd: {
                required: true
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
                }
            },
            messages: {
                name: i18next.t('login-page.usernameEmpty'),
                pwd: {
                    required: i18next.t('login-page.pwdEmpty')
                }
            },
            errorPlacement: function (error, element) {
                error.insertAfter(element.parent());
            }
        })
    }

    function render() {
        $("#name-error").text(i18next.t('login-page.usernameEmpty'));
        $("#pwd-error").text(i18next.t('login-page.pwdEmpty'));
    }

    return {
        initValidation: initValidation,
        render: render,
        initElementEvent: initElementEvent
    };
})(jQuery)

const loginComponent = (function ($) {
    "use strict"

    function login(name, pwd) {
        if ($("#loginForm").valid()) {
            commonFunctions.axios().post('/api/User/login', {
                name: name, password: pwd
            }).then(function (res) {
                window.location.href = "../../../cd-control.html";
            }).catch(function (error) {
                console.log(error.response.data);
                // if error show modal
                commonFunctions.showModal("", error.response.data.title)
            })
        }
    }

    return {
        login: login
    };
})(jQuery);

