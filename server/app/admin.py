# app/admin.py
from fastapi import APIRouter, Depends, HTTPException, status

from .auth import get_current_user

router = APIRouter()


def require_admin(user: dict = Depends(get_current_user)):
    """
    Very simple RBAC check, we treat any user whose ``sub`` ends with
    ':admin' as an administrator.
    """
    if not user["sub"].endswith(":admin"):
        raise HTTPException(
            status_code=status.HTTP_403_FORBIDDEN,
            detail="Insufficient privileges",
        )
    return user


@router.get("/admin/stats")
async def admin_stats(admin: dict = Depends(require_admin)):
    """
    Dummy admin endpoint, returns a static payload.
    """
    return {"stats": "secret admin data"}