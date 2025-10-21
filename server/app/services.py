# app/services.py
import os
import urllib.parse
from typing import List, Dict, Any

import httpx
from dotenv import load_dotenv

# ----------------------------------------------------------------------
# Configuration – loaded once at import time
# ----------------------------------------------------------------------
load_dotenv()                     # reads .env in the cwd
RIOT_API_KEY = os.getenv("RIOT_API_KEY")
if not RIOT_API_KEY:
    raise RuntimeError("❌ Missing RIOT_API_KEY in environment variables")

REGION = "eun1"   # tweak as needed

def _riot_url(path: str) -> str:
    """Base URL for Riot “account” endpoints."""
    return f"https://europe.api.riotgames.com/riot{path}?api_key={RIOT_API_KEY}"

def _match_url(path: str) -> str:
    """Base URL for Riot “match” endpoints."""
    return f"https://europe.api.riotgames.com/lol{path}?api_key={RIOT_API_KEY}"

def _summoner_url(path: str) -> str:
    """Base URL for Riot “summoner” endpoints."""
    return f"https://{REGION}.api.riotgames.com/lol{path}?api_key={RIOT_API_KEY}"


# ----------------------------------------------------------------------
# Public service functions
# ----------------------------------------------------------------------
async def resolve_riot_id_to_puuid(game_name: str, tag_line: str) -> str:
    """
    Convert a Riot ID (game name + tag line) into a PUUID.
    """
    url = _riot_url(
        f"/account/v1/accounts/by-riot-id/"
        f"{urllib.parse.quote(game_name)}/{urllib.parse.quote(tag_line)}"
    )
    async with httpx.AsyncClient() as client:
        resp = await client.get(url)
        resp.raise_for_status()
        return resp.json()["puuid"]


async def fetch_match_history(puuid: str, start: int = 0, count: int = 100) -> List[str]:
    """
    Return a list of match IDs for the given PUUID.
    """
    url = _match_url(f"/match/v5/matches/by-puuid/{urllib.parse.quote(puuid)}/ids")
    async with httpx.AsyncClient() as client:
        resp = await client.get(url, params={"start": start, "count": count})
        resp.raise_for_status()
        return resp.json()  # type: ignore[return-value]


async def fetch_match_detail(match_id: str) -> Dict[str, Any]:
    """
    Retrieve the full payload for a single match.
    """
    url = _match_url(f"/match/v5/matches/{urllib.parse.quote(match_id)}")
    async with httpx.AsyncClient() as client:
        resp = await client.get(url)
        resp.raise_for_status()
        return resp.json()


async def fetch_summoner_by_puuid(puuid: str) -> Dict[str, Any]:
    """
    Get the summoner profile for a PUUID.
    """
    url = _summoner_url(f"/summoner/v4/summoners/by-puuid/{urllib.parse.quote(puuid)}")
    async with httpx.AsyncClient() as client:
        resp = await client.get(url)
        resp.raise_for_status()
        return resp.json()