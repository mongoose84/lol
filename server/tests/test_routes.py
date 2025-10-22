# tests/test_routes.py
import pytest
from .helpers import DummyResponse

# --------------------------------------------------------------
# Payload factories – mimic the shape Riot would return
# --------------------------------------------------------------
def _make_riot_puuid_response(puuid: str = "sample-puuid"):
    return {"puuid": puuid}


def _make_summoner_response(puuid: str = "sample-puuid"):
    return {
        "id": "some-id",
        "accountId": "some-account",
        "puuid": puuid,
        "name": "SampleSummoner",
        "profileIconId": 1234,
        "revisionDate": 1700000000000,
        "summonerLevel": 30,
    }

# --------------------------------------------------------------
# Tests
# --------------------------------------------------------------
def test_successful_by_riot_id(test_client, mock_httpx_get):
    """
    Happy‑path: Riot returns a PUUID and a summoner profile.
    """
    mock_httpx_get.side_effect = [
        DummyResponse(200, _make_riot_puuid_response()),   # resolve_riot_id_to_puuid()
        DummyResponse(200, _make_summoner_response()),    # fetch_summoner_by_puuid()
    ]

    response = test_client.get("/api/by-riot-id/SomeGameName/NA1")
    print("DEBUG:", response.status_code, response.json())
    assert response.status_code == 200
    payload = response.json()
    assert payload["name"] == "SampleSummoner"
    assert payload["puuid"] == "sample-puuid"


def test_invalid_riot_id_returns_404(test_client, mock_httpx_get):
    mock_httpx_get.return_value = DummyResponse(
        404,
        {"status": {"message": "Not found"}},
        text="Not found",
    )

    response = test_client.get("/api/by-riot-id/NonExistent/XX1")
    print("DEBUG:", response.status_code, response.json())
    assert response.status_code == 404
    # The dummy raises an HTTPException with the raw text as the detail.
    assert response.json()["detail"] == "Not found"


def test_unexpected_error_is_500(test_client, mock_httpx_get):
    """
    Simulate a totally unexpected exception (e.g. network timeout).
    """
    mock_httpx_get.side_effect = Exception("boom")

    response = test_client.get("/api/by-riot-id/Anything/YY1")
    print("DEBUG:", response.status_code, response.json())
    assert response.status_code == 500
    assert "boom" in response.json()["detail"]