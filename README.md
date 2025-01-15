# Cursovaya

# В bash
    ```bash
    sudo apt install nuget
    nuget restore packages.config -SolutionDirectory .
    ```

## В powershell
    ```powershell
    Set-ExecutionPolicy Bypass -Scope Process -Force; [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.SecurityProtocolType]::Tls12; iex ((New-Object System.Net.WebClient).DownloadString('https://community.chocolatey.org/install.ps1'))

    choco install nuget.commandline
    ```
    ## Для удаления
        ```powershell
        choco uninstall nuget.commandline
        ```
    
    ## Для удаления
        ```powershell
        choco uninstall chocolatey
        Remove-Item -Recurse -Force "C:\ProgramData\chocolatey"
        ```
    
    ```powershell
    nuget restore packages.config -SolutionDirectory .
    ```


## Для windows если есть Microsoft Visual Studio
    ```cmd
    "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" Cursovaya.csproj -p:Configuration=Release
    ```

## В командной строке раработчика или в bash
    ```bash
    msbuild Cursovaya.csproj -p:Configuration=Release
    ```

```bash
cd bin/Release
./Cursovaya.exe 
```

## или для cmd
    ```cmd
    Cursovaya.exe
    ```