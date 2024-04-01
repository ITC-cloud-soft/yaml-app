/**
 * JS for common functions
 */
var userLanguage = "jp";

(function (){
    
    function showToast(time, content, type){
         const selector = '.a-toast';
         
        $(".a-toast__message").text(content);
        $(selector).css({"background": type});

        $(selector).fadeIn();
        setTimeout(function() {
            $(selector).fadeOut();
        }, time);
    }
    function showModal(header, content, callback){
        let modal = $("#custom-modal")[0].__component;
        $(".modal-header-a").html(header);
        $(".modal-content-a").text(content);
        modal.opened = !0;
        modal.onCloseRequested = function () {
            return modal.opened = !1
        }
        
        $('#confirm-btn').off('click').on('click', function(){
            callback? callback() : ''
            closeModal(modal)
        })
        
        $('#cancel-btn').off('click').on('click', function(){
            closeModal(modal)
        })
    }
    
    function closeModal(modal) {
        modal.opened = !1;
        modal.onCloseRequested = function () {
            return modal.opened = !1
        }
    }
    
    function showCustomModal(selector){
        let modal = $(selector)[0].__component;
        modal.opened = !0;
        modal.onCloseRequested = function () {
            return modal.opened = !1
        }
    }

    function closeCustomModal(selector){
        
        let modal = $(selector)[0].__component;
        modal.opened = !1;
        modal.onCloseRequested = function () {
            return modal.opened = !1
        }
    }

    function setLoading(){
        const htmlValue = "<div class='a-loading a-loading--overlay'><div class='a-loading__dot-grid' ><span></span><span></span><span></span><span></span><span></span><span></span><span></span><span></span><span></span></div ></div >";
        $(".o-whole__body-container").append(htmlValue);
    }

    function removeLoading () {
        $(".a-loading--overlay").remove();
    }

    function axiosWithInterceptor (){

        const axiosInstance = axios.create({
            timeout: 5000,
        });

        // Add a request interceptor
        axiosInstance.interceptors.request.use(function (config) {
            setLoading()
            
            // Set the "Accept-Language" header in the request
            config.headers['Accept-Language'] = userLanguage;
            return config;
        }, function (error) {
            console.error(error)
            return Promise.reject(error);
        });

        // Add a response interceptor
        axiosInstance.interceptors.response.use(function (response) {
            removeLoading();
            return response;
        }, function (error) {
            console.error(error)
            removeLoading();

            commonFunctions.showModal('提示', 'システムエラーが発生しました。システム管理者に連絡してください')
            return Promise.reject(error);
        });
        return axiosInstance;
    }
    
    window.commonFunctions = {
        showModal: showModal,
        showToast: showToast,
        setLoading: setLoading,
        showCustomModal: showCustomModal,
        closeCustomModal: closeCustomModal,
        removeLoading: removeLoading,
        axios: axiosWithInterceptor
    };
})()
