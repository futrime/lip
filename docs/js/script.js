function rootPathJumpByLang() {
    const SUPPORTED_LANG_CODE_LIST = ['en', 'zh'];

    if (SUPPORTED_LANG_CODE_LIST.includes(location.pathname.split('/')[1])) {
        return;
    }

    for (let langCode of navigator.languages) {
        let shortLangCode = langCode.split('-')[0];

        if (!SUPPORTED_LANG_CODE_LIST.includes(shortLangCode)) {
            continue;
        }

        location.assign('/' + shortLangCode);
        return;
    }

    // Default to English
    location.assign('/en');
}

function changeLanguage(toLang) {
    location.assign('/' + toLang + location.pathname.substring(3));
}

// Run on page load
rootPathJumpByLang();
