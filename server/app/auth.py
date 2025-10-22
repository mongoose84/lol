# app/auth.py
import time
from typing import Optional

import jwt
from fastapi import Depends, HTTPException, Request, Security, status
from fastapi.security import HTTPAuthorizationCredentials, HTTPBearer

# Secret used only for tests – in prod you’d load it from env
SECRET_KEY = "test-secret-key"
ALGORITHM = "HS256"

bearer_scheme = HTTPBearer(auto_error=False)   # returns None if no header


def create_token(sub: str, expires_in: int = 3600) -> str:
    """Utility used by the test fixture to generate a JWT."""
    payload = {
        "sub": sub,
        "exp": int(time.time()) + expires_in,
    }
    return jwt.encode(payload, SECRET_KEY, algorithm=ALGORITHM)


def decode_token(token: str) -> dict:
    try:
        return jwt.decode(token, SECRET_KEY, algorithms=[ALGORITHM])
    except jwt.ExpiredSignatureError:
        raise HTTPException(
            status_code=status.HTTP_401_UNAUTHORIZED,
            detail="Token has expired",
        )
    except jwt.InvalidTokenError:
        raise HTTPException(
            status_code=status.HTTP_401_UNAUTHORIZED,
            detail="Invalid token",
        )


def get_current_user(
    credentials: Optional[HTTPAuthorizationCredentials] = Security(bearer_scheme),
):
    """
    FastAPI dependency that returns the JWT payload (a dict) or raises 401.
    """
    if credentials is None:
        raise HTTPException(
            status_code=status.HTTP_401_UNAUTHORIZED,
            detail="Not authenticated",
        )
    return decode_token(credentials.credentials)