import sys
import pytest
from pathlib import Path
from unittest.mock import AsyncMock, patch
from app import app 
from fastapi.testclient import TestClient

# Make the project root importable
PROJECT_ROOT = Path(__file__).resolve().parents[1]
sys.path.append(str(PROJECT_ROOT))

 


@pytest.fixture(scope="session")
def test_client():
    """Provides a TestClient that talks to the real FastAPI app."""
    with TestClient(app) as client:
        yield client


@pytest.fixture(autouse=True)
def fake_env(monkeypatch):
    """Guarantee that the Riot API key exists for every test."""
    monkeypatch.setenv("RIOT_API_KEY", "FAKE-KEY")

@pytest.fixture
def mock_httpx_get():
    """
    Patch ``httpx.AsyncClient.get`` for a single test.
    Yields an ``AsyncMock`` whose ``return_value`` (or ``side_effect``)
    should be a :class:`DummyResponse` instance. from tests.helpers.py
    """
    with patch("httpx.AsyncClient.get", new_callable=AsyncMock) as mock_get:
        yield mock_get