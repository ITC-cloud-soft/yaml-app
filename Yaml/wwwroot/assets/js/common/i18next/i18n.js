
const lngs = {
    'ja': { nativeName: '日本語' },
    'en': { nativeName: 'English' },
    'zh': { nativeName: '中文' },
};

const render = () => {
    // start localizing, details:
    $('body').localize();
}

const initI18next = (function ($) {
    function init (callback) {
        i18next
            .use(i18nextHttpBackend)
            .init({
                debug: true,
                lng:'ja',
                fallbackLng: 'ja',
                // load: 'languageOnly', // 仅加载特定区域版本的翻译文件
                backend :{
                    loadPath: '/assets/locales/{{lng}}/{{ns}}.json'
                },
            }, (err, t) => {
                if (err) return console.error(err);

                // for options see
                jqueryI18next.init(i18next, $, {useOptionsAttr: true});

                // fill language switcher
                Object.keys(lngs).map((lng) => {
                    const opt = new Option(lngs[lng].nativeName, lng);
                    if (lng === i18next.resolvedLanguage) {
                        opt.setAttribute("selected", "selected");
                    }
                    $('#languageSwitcher').append(opt);
                });
                render();
            })
    }
    return {
        init: init
    };
})(jQuery);
