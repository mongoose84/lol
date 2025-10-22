from fastapi.testclient import TestClient
from app.main import app

client = TestClient(app)

# def test_riot_id_path_traversal_is_rejected():
#     # Attempt to inject a slash or ".." into the path component
#     malicious = "%2e%2e%2f%2e%2e%2fetc%2fpasswd"
#     r = client.get(f"/api/by-riot-id/{malicious}/NA1")
#     # FastAPI will reject the path param because it doesn't match the schema
#     assert r.status_code == 422   # Unprocessable Entity