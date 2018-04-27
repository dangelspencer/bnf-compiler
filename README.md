# bnf-compiler

## Steps to run the program on Debian
1. Install dotnet core utilities using the following [link](https://www.microsoft.com/net/download/linux-package-manager/debian9/runtime-2.0.6)
2. Clone the repository down
2. Change directory into `bnf-compiler/BnfCompiler`
3. Run `dotnet restore`
4. Run `dotnet build`
5. Execute the program with `dotnet run --file <path to a file>`

## Command line options
* `-f <path to file>` or `--file <path to file>`: Specifies which file to parse
* `-d`: Prints debug information

## Running against all .src files in a directory
`for f in $(find ../testPgms -name '*.src'); do dotnet run --file $f; done`   
NOTE: This may require adding `sudo` before `dotnet run` 
