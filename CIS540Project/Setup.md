P\# Setup on macOS and Linux
===

Prerequisites
---

* .NET Core

  Install .NET Core 2.1 from <https://dotnet.microsoft.com/download/archives>. Note that the current stable version is 2.2, but we will need 2.1. You may install both if you wish.

* P\# Source

  The official project repository is <https://github.com/p-org/PSharp>. However, a few changes were necessary to make P# work on macOS and Linux. My fork of the repository with the changes can be found here: <https://github.com/fniksic/PSharp>. You can clone the repository by executing

  ```bash
  git clone https://github.com/fniksic/PSharp.git
  ```

* Visual Studio Code

  The editor that should suffice for our purposes is Visual Studio Code: <https://code.visualstudio.com/>. I tried the full Visual Studio, but it does not work that well with the P# extension on macOS. Visual Studio Code is somewhat limited, but it does offer syntax coloring, ability to build and run programs, and limited ability to debug code.

  Make sure to install the C# extension.

* Node.js

  Necessary to build the P\# extension for VSCode. I use Homebrew on my system, so I installed it by running:

  ```bash
  brew install node
  ```

## Building the VSCode extension

Assuming we are located in the P\# source directory, we run the following:

```bash
cd Tools/VSCode/msr-vscode-psharp
npm install -g typescript
npm install -g vscode
npm install
```

Next, we simply copy the `msr-vscode-psharp` directory to `~/.vscode/extensions/` (make sure this directory exists, e.g. by starting VSCode at least once):

```bash
cd ..
cp -r msr-vscode-psharp ~/.vscode/extensions/
```

## Building P\#

Assuming we are located in the P\# source directory, we run the following:

```bash
dotnet restore PSharp.sln
dotnet build -c Release PSharp.sln
```

To build the sample projects:

```bash
cd Samples
dotnet restore Samples.Framework.sln
dotnet build -c Release Samples.Framework.sln
dotnet restore Samples.Language.sln
dotnet build -c Release Samples.Language.sln
```

To make sure everything works, try running the Ping Pong example. Assuming we are located in the root of the P# repository, execute the following:

```bash
dotnet Samples/Language/bin/netcoreapp2.1/PingPong.dll
dotnet bin/netcoreapp2.1/PSharpTester.dll \
  -test:Samples/Language/bin/netcoreapp2.1/PingPong.dll
```

The first command runs the example itself, and the second one runs the P\# tester on the example.
