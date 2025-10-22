from fastapi.testclient import TestClient
from app.main import app

client = TestClient(app)

# def test_disallowed_origin_is_blocked():
#     r = client.options(
#         "/api/by-riot-id/foo/bar",
#         headers={
#             "Origin": "https://evil.example.com",
#             "Access-Control-Request-Method": "GET",
#         },
#     )
#     # FastAPI’s CORSMiddleware should NOT echo back the Origin header
#     assert "access-control-allow-origin" not in r.headers
#     assert r.status_code == 200