from fastapi import APIRouter, Depends

from .auth import get_current_user

router = APIRouter()


@router.get("/protected/resource")
async def protected_resource(user: dict = Depends(get_current_user)):
    """
    Simple endpoint that requires a valid JWT.
    Returns the payload so the test can inspect it if needed.
    """
    return {"msg": "ok", "user": user}