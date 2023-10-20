$(function () {

    new Promise((resolve, reject) => {
        resolve(initI18next());
    }).then(function (result) {
        initPage.bindEvents();
        initPage.bindValidation(i18next);
    })

    // 阻止表单的默认提交行为
    $('#appForm').submit(function (event) {
        event.preventDefault();
    });
    
    outerFun();
    
    // bind switch language event
    $('#languageSwitcher').change((a, b, c) => {
        const chosenLng = $(this).find("option:selected").attr('value');
        i18next.changeLanguage(chosenLng, () => {
            render();
            initPage.renderErrorI18();
            commonFunctions.userLanguage = chosenLng;
        });
    });
})

const initPage = (($) => {
    "use strict"

    function initElementEvent() {

    }

    function initValidation(i18next) {

        const rules = {
            appName: {required: true},
            crServer: {required: true},
            token: {required: true},
            mail: {required: true},
        }
        
        const fieldName =  i18next.t('appInfoPage.appName');
        const appName = i18next.t('common.notNull', {fieldName});
        const messages = {
            appName: appName, 
            pwd: {
                required: i18next.t('login-page.pwdEmpty')
            }
        }

        const loginForm = $("#appForm");
        loginForm.validate({
            focusCleanup: true,
            onkeyup: false,
            ignore: "",
            onfocusin: false,
            rules,
            messages,
            errorPlacement: function (error, element) {
                error.insertAfter(element.parent());
            }
        })
    }

    function renderErrorI18() {
   
    }

    return {
        bindValidation: initValidation,
        renderErrorI18: renderErrorI18,
        bindEvents: initElementEvent
    };
})(jQuery)

const loginComponent = (($) =>  {
    "use strict"

    function login(name, pwd) {
        if ($("#appForm").valid()) {
            commonFunctions.axios().post('/api/User/login', {
                name: name,
                password: pwd
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

const appComponent = (($) =>  {
   function deploy (){
       
   }
    
    
    return {
       deploy : deploy
    }
    
})(jQuery)
const outerFun = ()=>{
    console.log('qwe')
    $(selectors.appName).val("123");
    $(selectors.token).val("123");
}

