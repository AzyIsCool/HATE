if [[ "$OSTYPE" == "linux-gnu" ]]; then
        sudo mono HATE.exe;
elif [[ "$OSTYPE" == "darwin"* ]]; then
        cd "Applications/" && sudo mono HATE.exe;
else
        sudo mono HATE.exe
fi
