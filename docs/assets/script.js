/*
    LiteLoaderBDS, an epoch-making & cross-language plugin loader for Bedrock Dedicated Server for Minecraft.
    Copyright (C) 2022  LiteLDev

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Lesser General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

function changeLanguage(lang) {
    location.assign(`../${lang}/${location.hash}`);
}

function toggleDarkMode() {
    let themeCss = document.querySelector(".theme-css");

    if (themeCss !== null) {
        if (!window.isDocsifyDarkMode) {
            window.isDocsifyDarkMode = true;
            themeCss.setAttribute("href", "https://unpkg.com/docsify/lib/themes/dark.css");
            updateCustomCSS();
        }
        else {
            window.isDocsifyDarkMode = false;
            themeCss.setAttribute("href", "https://unpkg.com/docsify/lib/themes/vue.css");
            updateCustomCSS();
        }
    }
}

function updateCustomCSS() {
    let tableBars = document.querySelectorAll('.markdown-section tr:nth-child(2n)');

    // fix table bar color
    if (tableBars !== null) {
        for (var i = 0; i < tableBars.length; ++i)
            tableBars[i].style.backgroundColor = window.isDocsifyDarkMode ? "#363636" : "#f8f8f8";
    }
}
window.updateCustomCSS = updateCustomCSS;