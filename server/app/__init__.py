# app/__init__.py
"""
Package initialisation for the FastAPI application.

We expose the ``app`` object at the package level so that test code can
simply ``from app import app`` without needing to know the internal file
layout.
"""

from .main import app  # noqa: F401  (re‑export for convenient imports)