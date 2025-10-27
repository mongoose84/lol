# League of Legends champion stats

The idea is to look up your champion and see some stats. More will follow

## 1st time install

#### Git
##### Mac:
use homebrew if on mac: 
```
/bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"
```
```
brew install git
```

##### Windows:
Download git installer

##### Linux Fedora:
```
sudo dnf install git-all
```

##### Set git user (from terminal)
```
git config --global user.name anon

git config --global user.email anon.anonsen@pm.me
```
#### Node.js
Download and install node js installer https://nodejs.org/en/download

#### Install Visual Studio code
https://code.visualstudio.com/download

##### VS Code Extensions
Git Graph

Markdown Preview

Vue

Github actions

C# Dev Kit

## In Visual Studio:
Git clone https://github.com/mongoose84/AgileAstronaut.com.git

The file structure contains a server and a client part.

#### Client part
##### Install development server
from root
```
cd client
```
```
npm install
```
```
npm i -D vitest @vue/test-utils axios-mock-adapter;
```
##### Run dev
```
npm run dev
```
##### Unit test
```
npm run test:unit // Run all test suites once

npm run test:unit:watch // Run all test suites but watch for changes and rerun tests when they change.

npm run test:unit:coverage // Run all tests once and show test coverage
```

#### Server part
from root
```
cd LolApi
cd lol-api
```
Install a virtual environment

create publishable build
```
dotnet publish -c Release -r win-x86 --self-contained true 
```
This will create all the files needed in the folder /bin/Release/publish