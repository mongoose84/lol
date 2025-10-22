# tests/helpers.py
"""Utility objects shared across test modules."""

from fastapi import HTTPException   # <-- import FastAPI's exception type


class DummyResponse:
    """
    Very small stand‑in for ``httpx.Response`` that implements only the
    attributes/methods used by our service layer:

    * ``status_code``
    * ``json()``
    * ``text``
    * ``raise_for_status()`` – now raises ``fastapi.HTTPException`` so the
      FastAPI error handling pipeline returns the expected status code.
    """

    def __init__(self, status_code: int, json_data, text: str = ""):
        self.status_code = status_code
        self._json = json_data
        self.text = text
        
    async def json(self):
        return self._json

    def raise_for_status(self):
        """
        Mimic ``httpx.Response.raise_for_status`` but raise a FastAPI
        ``HTTPException`` instead of ``httpx.HTTPStatusError``.
        """
        if not (200 <= self.status_code < 300):
            # FastAPI will turn this into the proper HTTP response.
            raise HTTPException(status_code=self.status_code, detail=self.text or "error")