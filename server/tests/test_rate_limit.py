# from fastapi.testclient import TestClient
# import pytest
# from app.main import app

# @pytest.fixture(autouse=True)
# def reset_limiter():
#     """Clear the in‑memory counters before every test."""
#     # `app.state.limiter` is the Limiter we attached in main.py
#     app.state.limiter.reset()
#     yield

# def test_rate_limit_returns_429():
#     client = TestClient(app)          # fresh client for this test
#     # hit the endpoint 60 times – they should all be OK (200)
#     for i in range(60):
#         resp = client.get("/api/by-riot-id/foo/bar")
#         assert resp.status_code == 200, f"unexpected failure at iteration {i+1}"
#     # 61st request must be throttled
#     resp = client.get("/api/by-riot-id/foo/bar")
#     assert resp.status_code == 429
#     assert "Too Many Requests" in resp.text