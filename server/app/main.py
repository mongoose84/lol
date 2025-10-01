# main.py
import os
import urllib.parse
from typing import List

import httpx
from fastapi import FastAPI, HTTPException, Path
from fastapi.middleware.cors import CORSMiddleware
from dotenv import load_dotenv

# ----------------------------------------------------------------------
# Load environment variables (mirrors `dotenv.config()` in Node)
# ----------------------------------------------------------------------
load_dotenv()                     # reads .env in the cwd
RIOT_API_KEY = os.getenv("RIOT_API_KEY")
if not RIOT_API_KEY:
    raise RuntimeError("❌ Missing RIOT_API_KEY in environment variables")

# ----------------------------------------------------------------------
# FastAPI app setup
# ----------------------------------------------------------------------
app = FastAPI()

# Allow your client  to call the API.
# Adjust `allow_origins` to a specific list for tighter security.
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],          # <‑‑ replace with ["http://localhost:8080"] etc.
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# ----------------------------------------------------------------------
# Helper functions – building Riot URLs
# ----------------------------------------------------------------------
REGION = "eun1"   # change if you need a different region

def riot_url(path: str) -> str:
    """Base URL for Riot “account” endpoints."""
    return f"https://europe.api.riotgames.com/riot{path}?api_key={RIOT_API_KEY}"

def match_url(path: str) -> str:
    """Base URL for Riot “match” endpoints."""
    # Note: the original code concatenated `api_key=` twice; we keep a single one.
    return f"https://europe.api.riotgames.com/lol{path}?api_key={RIOT_API_KEY}"

def summoner_url(path: str) -> str:
    """Base URL for Riot “summoner” endpoints."""
    return f"https://{REGION}.api.riotgames.com/lol{path}?api_key={RIOT_API_KEY}"


# ----------------------------------------------------------------------
# Async wrappers around the Riot API
# ----------------------------------------------------------------------
async def get_puuid(game_name: str, tag_line: str) -> str:
    url = riot_url(
        f"/account/v1/accounts/by-riot-id/"
        f"{urllib.parse.quote(game_name)}/{urllib.parse.quote(tag_line)}"
    )
    async with httpx.AsyncClient() as client:
        resp = await client.get(url)
        if resp.status_code != 200:
            raise HTTPException(
                status_code=resp.status_code,
                detail=f"Failed to resolve Riot ID: {resp.text}",
            )
        return resp.json()["puuid"]


async def get_match_history(puuid: str) -> List[str]:
    url = match_url(f"/match/v5/matches/by-puuid/{urllib.parse.quote(puuid)}/ids")
    params = {"start": 0, "count": 100}
    async with httpx.AsyncClient() as client:
        resp = await client.get(url, params=params)
        if resp.status_code != 200:
            raise HTTPException(
                status_code=resp.status_code,
                detail=f"Failed to fetch match history: {resp.text}",
            )
        return resp.json()  # list of match IDs


async def get_match_details(match_id: str) -> dict:
    url = match_url(f"/match/v5/matches/{urllib.parse.quote(match_id)}")
    async with httpx.AsyncClient() as client:
        resp = await client.get(url)
        if resp.status_code != 200:
            raise HTTPException(
                status_code=resp.status_code,
                detail=f"Failed to fetch match details: {resp.text}",
            )
        return resp.json()


async def get_summoner_by_puuid(puuid: str) -> dict:
    url = summoner_url(f"/summoner/v4/summoners/by-puuid/{urllib.parse.quote(puuid)}")
    async with httpx.AsyncClient() as client:
        resp = await client.get(url)
        if resp.status_code != 200:
            raise HTTPException(
                status_code=resp.status_code,
                detail=f"Failed to fetch summoner data: {resp.text}",
            )
        return resp.json()


# ----------------------------------------------------------------------
# Route – Convert Riot ID (gameName + tagLine) → PUUID → Summoner data
# ----------------------------------------------------------------------
@app.get("/api/by-riot-id/{game_name}/{tag_line}")
async def by_riot_id(
    game_name: str = Path(..., description="Summoner name (game name)"),
    tag_line: str = Path(..., description="Tag line (e.g., NA1)"),
):
    """
    Resolve a Riot ID to its PUUID, then return the summoner profile.
    Mirrors the Express endpoint `/api/by-riot-id/:gameName/:tagLine`.
    """
    try:
        puuid = await get_puuid(game_name, tag_line)
        # Uncomment the next two lines if you also want match data:
        # match_ids = await get_match_history(puuid)
        # first_match = await get_match_details(match_ids[0]) if match_ids else {}
        summoner_data = await get_summoner_by_puuid(puuid)

        # You could merge additional info here (e.g., match data) before returning.
        return summoner_data
    except HTTPException as he:
        # Propagate the exact status we got from Riot
        raise he
    except Exception as exc:
        # Catch‑all for unexpected errors
        raise HTTPException(status_code=500, detail=str(exc))


# ----------------------------------------------------------------------
# Entry point – run with `uvicorn main:app --reload`
# ----------------------------------------------------------------------
if __name__ == "__main__":
    import uvicorn

    uvicorn.run("main:app", host="0.0.0.0", reload=True)