@echo off
set sketch_folder=%1
set build_folder=%2
set hex_file=%3
set archive_folder=%4

REM See http://stackoverflow.com/q/1642677/1143274
FOR /f %%a IN ('WMIC OS GET LocalDateTime ^| FIND "."') DO SET DTS=%%a
SET DateTime=%DTS:~0,4%-%DTS:~4,2%-%DTS:~6,2%_%DTS:~8,2%-%DTS:~10,2%-%DTS:~12,2%

echo ARCHIVE_METADATA %*

set metafile=%archive_folder%\%DateTime%.txt
set copy_cmd=%build_folder%\copyhex.cmd

pushd %1

dir > %metafile%

echo.  >> %metafile%
echo git info: >> %metafile%
git config --get remote.origin.url >> %metafile%
git log -n 1 >> %metafile%

popd

echo copy %build_folder%\%hex_file% %archive_folder%\%DateTime%.hex >%copy_cmd%

