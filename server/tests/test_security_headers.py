from fastapi.testclient import TestClient
from app.main import app
import os

client = TestClient(app)

# def test_hsts_and_secure_headers():
#     r = client.get("/")
#     # HSTS only in production; adjust env if needed
#     if os.getenv("ENV") == "production":
#         assert r.headers["strict-transport-security"].startswith("max-age=")
#     assert r.headers["x-content-type-options"] == "nosniff"
#     assert r.headers["x-frame-options"] == "DENY"
#     assert r.headers["referrer-policy"] == "no-referrer"
#     assert "content-security-policy" in r.headers