from fastapi.testclient import TestClient
from app.main import app

client = TestClient(app)

# def test_user_cannot_access_admin_endpoint(user_token):
#     # `user_token` fixture yields a valid JWT for a regular user
#     r = client.get("/admin/stats", headers={"Authorization": f"Bearer {user_token}"})
#     assert r.status_code == 403
#     assert "Insufficient privileges" in r.json()["detail"]