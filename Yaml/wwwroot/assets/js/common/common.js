/**
 * JS for common functions
 */
var userLanguage = "jp";

(function (){
    function showModal(header, content){
        let modal = $("#custom-modal")[0].__component;
        $(".modal-header-a").text(header);
        $(".modal-content-a").text(content);
        modal.opened = !0;
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
            return Promise.reject(error);
        });
        return axiosInstance;
    }
    
    window.commonFunctions = {
        showModal: showModal,
        setLoading: setLoading,
        removeLoading: removeLoading,
        axios: axiosWithInterceptor
    };
    
    
})()
