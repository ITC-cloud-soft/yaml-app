var myModule = (function ($) {
    "use strict"
    // 内部函数
    function privateFunction() {
        console.log("这是一个内部函数");
    }

    // 公共函数，可以在外部访问
    function publicFunction() {
        console.log("这是一个公共函数");
    }

    // 返回一个对象，包含可以在外部访问的函数
    return {
        publicFunction: publicFunction
    };
})(jQuery);