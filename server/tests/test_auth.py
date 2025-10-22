# tests/test_auth.py
import jwt
import time
from fastapi.testclient import TestClient
from app.main import app

client = TestClient(app)

testUrl = "/api/by-riot-id/foo/bar"


# def test_missing_authorization_header():
#     r = client.get(testUrl)      # any endpoint that requires auth
#     assert r.status_code == 401
#     assert r.json()["detail"] == "Not authenticated"

# def test_invalid_jwt_signature():
#     # create a token signed with a *different* secret
#     bad_token = jwt.encode({"sub": "user1", "exp": time.time() + 60},
#                            "wrong-secret", algorithm="HS256")
#     r = client.get(testUrl, headers={"Authorization": f"Bearer {bad_token}"})
#     assert r.status_code == 401
#     assert "Invalid token" in r.json()["detail"]

# def test_expired_jwt():
#     expired = jwt.encode({"sub": "user1", "exp": time.time() - 10},
#                          "my-secret", algorithm="HS256")
#     r = client.get(testUrl, headers={"Authorization": f"Bearer {expired}"})
#     assert r.status_code == 401
#     assert "Token has expired" in r.json()["detail"]