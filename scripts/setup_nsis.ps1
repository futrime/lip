C:\msys64\usr\bin\wget.exe -q https://sourceforge.net/projects/nsis/files/NSIS%203/3.09/nsis-3.09.zip -O nsis.zip
C:\msys64\usr\bin\wget.exe -q https://sourceforge.net/projects/nsis/files/NSIS%203/3.09/nsis-3.09-strlen_8192.zip -O nsis-strlen.zip
C:\msys64\usr\bin\wget.exe -q https://nsis.sourceforge.io/mediawiki/images/7/7f/EnVar_plugin.zip -O EnVar_plugin.zip
unzip -q -o nsis.zip
mv nsis-3.09 nsis-portable
unzip -q -o nsis-strlen.zip -d nsis-portable
unzip -q -o EnVar_plugin.zip -d nsis-portable
mkdir nsis
cp ./scripts/make_installer.nsi ./nsis/
cp ./lip.exe ./nsis/
cp ./COPYING ./nsis/
