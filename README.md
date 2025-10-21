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

##### Set git user
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

Python

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
cd server
cd app
```
Install a virtual environment
```
python3 -m venv venv      # creates a folder named “venv”
source venv/bin/activate  # macOS / Linux / zsh
# Windows PowerShell:
# .\venv\Scripts\Activate.ps1
```

```
pip install fastapi uvicorn python-dotenv httpx pytest pytest-asyncio
```

##### Riot API
create a .env file and add the Riot API key like this
```
RIOT_API_KEY=your_key_here
```

##### Run server
To run the server write this:
```
uvicorn main:app --reload; 
```

##### Run server tests
```
cd server/tests
pytest -vv
```