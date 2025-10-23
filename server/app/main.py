# app/main.py
import os
from fastapi import FastAPI, HTTPException, Path
from fastapi.middleware.cors import CORSMiddleware
from dotenv import load_dotenv
from .services import (
    fetch_match_winrate,
    resolve_riot_id_to_puuid,
    fetch_summoner_by_puuid,
)

# --------------------------------------------------------------
# Load env (same as before)
# --------------------------------------------------------------
load_dotenv()
RIOT_API_KEY = os.getenv("RIOT_API_KEY")
if not RIOT_API_KEY:
    raise RuntimeError("❌ Missing RIOT_API_KEY in environment variables")

# --------------------------------------------------------------
# FastAPI app
# --------------------------------------------------------------
app = FastAPI()
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],          # tighten in prod
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# --------------------------------------------------------------
# Route
# --------------------------------------------------------------
@app.get("/api/by-riot-id/{game_name}/{tag_line}")
async def by_riot_id(
    game_name: str = Path(..., description="Summoner name (game name)"),
    tag_line: str = Path(..., description="Tag line (e.g., NA1)"),
):
    """
    Resolve a Riot ID → PUUID → Summoner profile.
    """
    try:
        puuid = await resolve_riot_id_to_puuid(game_name, tag_line)
        summoner_data = await fetch_summoner_by_puuid(puuid)
        return summoner_data
    except HTTPException as he:
        raise he
    except Exception as exc:
        raise HTTPException(status_code=500, detail=str(exc))

@app.get("/api/get-winrate/{puuid}")
async def get_winrate(puuid: str):
    """
    Get the match win rate for a given PUUID.
    """
    try:
        winrate = await fetch_match_winrate(puuid)
        return winrate*100  # return as percentage
    except HTTPException as he:
        raise he
    except Exception as exc:
        raise HTTPException(status_code=500, detail=str(exc))

# --------------------------------------------------------------
# Run with uvicorn
# --------------------------------------------------------------
if __name__ == "__main__":
    import uvicorn

    uvicorn.run("app.main:app", host="0.0.0.0", reload=True)