Unicode True

!define PRODUCT_NAME "Lip"
!define PRODUCT_PUBLISHER "LiteLDev"
!define PRODUCT_WEB_SITE "https://github.com/LiteLDev/Lip"
!define PRODUCT_DIR_REGKEY "Software\Microsoft\Windows\CurrentVersion\App Paths\lip.exe"
!define PRODUCT_UNINST_KEY "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_NAME}"
!define PRODUCT_UNINST_ROOT_KEY "HKLM"

!ifdef LIP_VERSION
  !define PRODUCT_VERSION "${LIP_VERSION}"
!else
  !define PRODUCT_VERSION "0.0.0"
!endif

SetCompressor /SOLID lzma
SetCompressorDictSize 32

; MUI 1.67 compatible ------
!include "MUI.nsh"

; MUI Settings
!define MUI_ABORTWARNING
!define MUI_ICON "${NSISDIR}\Contrib\Graphics\Icons\modern-install.ico"
!define MUI_UNICON "${NSISDIR}\Contrib\Graphics\Icons\modern-uninstall.ico"

; Language Selection Dialog Settings
!define MUI_LANGDLL_REGISTRY_ROOT "${PRODUCT_UNINST_ROOT_KEY}"
!define MUI_LANGDLL_REGISTRY_KEY "${PRODUCT_UNINST_KEY}"
!define MUI_LANGDLL_REGISTRY_VALUENAME "NSIS:Language"

; Welcome page
!insertmacro MUI_PAGE_WELCOME
; License page
!define MUI_LICENSEPAGE_CHECKBOX
!insertmacro MUI_PAGE_LICENSE "LICENSE"
; Components page
!insertmacro MUI_PAGE_COMPONENTS
; Directory page
!insertmacro MUI_PAGE_DIRECTORY
; Instfiles page
!insertmacro MUI_PAGE_INSTFILES
; Finish page
!insertmacro MUI_PAGE_FINISH

; Uninstaller pages
!insertmacro MUI_UNPAGE_INSTFILES

; Language files
!insertmacro MUI_LANGUAGE "English"
!insertmacro MUI_LANGUAGE "SimpChinese"

; Reserve files
!insertmacro MUI_RESERVEFILE_INSTALLOPTIONS

; MUI end ------

Name "${PRODUCT_NAME}"
OutFile "lip-windows-amd64-setup.exe"
InstallDir "$PROGRAMFILES64\Lip"
InstallDirRegKey HKLM "${PRODUCT_DIR_REGKEY}" ""
ShowInstDetails show
ShowUnInstDetails show

Function .onInit
  !insertmacro MUI_LANGDLL_DISPLAY
FunctionEnd

Section "Lip (required)" SEC01
  SectionIn RO
  SetOutPath "$INSTDIR"
  SetOverwrite ifnewer
  File "lip.exe"
SectionEnd

Section "Add to PATH" SEC02
  ; Check for write access
  EnVar::Check "NULL" "NULL"
 
  ; Set to HKLM
  EnVar::SetHKLM
 
  ; Append a value
  EnVar::AddValue "PATH" "$INSTDIR"
SectionEnd

Section -Post
  WriteUninstaller "$INSTDIR\uninst.exe"
  WriteRegStr HKLM "${PRODUCT_DIR_REGKEY}" "" "$INSTDIR\lip.exe"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "DisplayName" "$(^Name)"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "UninstallString" "$\"$INSTDIR\uninst.exe$\""
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "DisplayIcon" "$\"$INSTDIR\lip.exe$\""
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "DisplayVersion" "${PRODUCT_VERSION}"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "URLInfoAbout" "${PRODUCT_WEB_SITE}"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "Publisher" "${PRODUCT_PUBLISHER}"
SectionEnd

; Section descriptions
!insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
  !insertmacro MUI_DESCRIPTION_TEXT ${SEC01} ""
  !insertmacro MUI_DESCRIPTION_TEXT ${SEC02} ""
!insertmacro MUI_FUNCTION_DESCRIPTION_END



Function un.onInit
  ReadRegStr $0 "${PRODUCT_UNINST_ROOT_KEY}" "${PRODUCT_UNINST_KEY}" "NSIS:Language"
  ${If} $0 == "1033"
    MessageBox MB_ICONQUESTION|MB_YESNO|MB_DEFBUTTON2 "Are you sure you want to completely remove $(^Name) and all of its components?" IDYES +2
    Abort
  ${ElseIf} $0 == "2052"
    MessageBox MB_ICONQUESTION|MB_YESNO|MB_DEFBUTTON2 "你确实要完全移除 $(^Name) 及其所有组件吗？" IDYES +2
    Abort
  ${EndIf}
FunctionEnd

Function un.onUninstSuccess
  HideWindow
  ${If} $0 == "1033"
    MessageBox MB_ICONINFORMATION|MB_OK "$(^Name) has been successfully removed."
  ${ElseIf} $0 == "2052"
    MessageBox MB_ICONINFORMATION|MB_OK "$(^Name) 已成功移除。"
  ${EndIf}
FunctionEnd

Section Uninstall
  Delete "$INSTDIR\uninst.exe"
  Delete "$INSTDIR\lip.exe"

  RMDir /r "$INSTDIR\.lip"
  RMDir "$INSTDIR"

  DeleteRegKey ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}"
  DeleteRegKey HKLM "${PRODUCT_DIR_REGKEY}"
  ; Check for write access
  EnVar::Check "NULL" "NULL"
  ; Set to HKLM
  EnVar::SetHKLM
  ; Delete a value from a variable
  EnVar::DeleteValue "PATH" "D:\Program Files\Lip"
  SetAutoClose true
SectionEnd
