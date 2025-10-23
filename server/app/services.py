# app/services.py
import json
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
    matches = [str]
    parsed_puuid = urllib.parse.quote(puuid)
    match_url = _match_url(f"/match/v5/matches/by-puuid/{parsed_puuid}/ids")
    
    try:
        async with httpx.AsyncClient() as client:
            resp = await client.get(match_url, params={"api_key": RIOT_API_KEY,"start": start, "count": count})
            resp.raise_for_status()
            matches = json.loads(resp.text)
            return matches
    except Exception as e:
        print("DEBUG: Error getting match history:", e)
        return matches 

async def fetch_match_winrate(puuid: str) -> float:
    """
    Fetch the match win rate for a given PUUID.
    """
    
    match_history = await fetch_match_history(puuid, 0, 10)
    print(f"DEBUG: Fetched {len(match_history)} matches")
    wins = 0
    for match_id in match_history:
        lol_match = await fetch_match(match_id)

         # participants is a list of dicts – find the one with our puuid
        for participant in lol_match["info"]["participants"]:
            if participant["puuid"] == puuid:
                if participant["win"]:
                    wins += 1

    return wins / len(match_history) if match_history else 0.0

async def fetch_match(match_id: str) -> Dict[str, Any]:
    """
    Retrieve the full payload for a single match.
    """
    match_url = _match_url(f"/match/v5/matches/{urllib.parse.quote(match_id)}")
    async with httpx.AsyncClient() as client:
        resp = await client.get(match_url)
        resp.raise_for_status()
        return resp.json()

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