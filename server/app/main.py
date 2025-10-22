# app/main.py
import os
import urllib.parse
from typing import List

import httpx
from fastapi import FastAPI, HTTPException, Path
from fastapi.middleware.cors import CORSMiddleware
from dotenv import load_dotenv
from fastapi import FastAPI, Request, HTTPException
from fastapi.responses import JSONResponse
from starlette.middleware.base import BaseHTTPMiddleware
from starlette.middleware.trustedhost import TrustedHostMiddleware
from starlette.middleware.httpsredirect import HTTPSRedirectMiddleware
from slowapi import Limiter, _rate_limit_exceeded_handler
from slowapi.util import get_remote_address
from fastapi import Request 
from .protected import router as protected_router
from .admin import router as admin_router

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

# Enforce HTTPS (only in production)
if os.getenv("ENV") == "production":
    app.add_middleware(HTTPSRedirectMiddleware)

# Restrict Host header to your domain(s)
app.add_middleware(
    TrustedHostMiddleware,
    allowed_hosts=["example.com", "www.example.com", "localhost", "127.0.0.1", "testserver"],
)

# 3️⃣ Secure headers (HSTS, CSP, Referrer‑Policy, etc.)
class SecurityHeadersMiddleware(BaseHTTPMiddleware):
    async def dispatch(self, request: Request, call_next):
        response = await call_next(request)
        response.headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains"
        response.headers["X-Content-Type-Options"] = "nosniff"
        response.headers["X-Frame-Options"] = "DENY"
        response.headers["Referrer-Policy"] = "no-referrer"
        response.headers["Content-Security-Policy"] = "default-src 'self'"
        return response

app.add_middleware(SecurityHeadersMiddleware)

# 4️⃣ Rate limiting (example: 60 req/min per IP)
limiter = Limiter(key_func=get_remote_address, default_limits=["60/minute"])
app.state.limiter = limiter
app.add_exception_handler(429, _rate_limit_exceeded_handler)

app.include_router(protected_router)
app.include_router(admin_router)

# --------------------------------------------------------------
# Import the pure‑logic helpers
# --------------------------------------------------------------
from .services import (
    get_puuid,
    get_summoner_by_puuid,
    # fetch_match_history, fetch_match_detail  # optional later
)

# --------------------------------------------------------------
# Route
# --------------------------------------------------------------
disallowed_characters = r"^[^\s/]+$"  # disallow slashes & whitespace
@app.get("/api/by-riot-id/{game_name}/{tag_line}")
@limiter.limit("60/minute")                     # rate‑limit stays active
async def by_riot_id(
    request: Request,
    game_name: str = Path(..., description="Summoner name (game name)", pattern=disallowed_characters),
    tag_line:  str = Path(..., description="Tag line (e.g. NA1)", pattern=disallowed_characters),
):
    """
    Resolve a Riot ID → PUUID → Summoner profile.
    """
    try:
        puuid = await get_puuid(game_name, tag_line)
        summoner_data = await get_summoner_by_puuid(puuid)
        return summoner_data
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