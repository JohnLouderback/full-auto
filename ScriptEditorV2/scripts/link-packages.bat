REM Link the packages together so that "script-ed-ts" can use "script-ed-ts-vfs-plugin" in
REM its node_modules folder.
cd ./script-ed-ts-vfs-plugin & yarn link & cd ../script-ed-ts-vfs & yarn link script-ed-ts-vfs-plugin & cd ../